import { defineStore } from 'pinia'
import { computed } from 'vue'
import { useApi } from '~/composables/useApi'

type LoginRequest = { email: string; password: string }
type RegisterRequest = { fullName: string; phone: string; email: string; password: string }

export type User = {
  id: string
  fullName: string
  email: string
  phone?: string
  role: string
}

export const useAuthStore = defineStore('auth', () => {
  const cookieOptions = {
    default: () => null,
    path: '/',
    sameSite: 'lax' as const,
    secure: process.env.NODE_ENV === 'production',
    maxAge: 60 * 60 * 24 * 30, // 30 days
  }

  // ✅ ملاحظة مهمة (حل جذري لمشكلة الداشبورد يرجع للّوغن):
  // الـ BFF يضبط cookie اسمها "token" كـ HttpOnly.
  // هذا يعني المتصفح/JS ما يقدر يقراها (useCookie على client راح يرجع null)
  // فلو نعتمد على token.value حتى نحدد isAuthed راح يصير دائماً false بالفرونت.
  // لذلك نعتمد على كوكيز غير HttpOnly يضبطها الـ BFF: auth=1 و role.
  const token = useCookie<string | null>('token', cookieOptions) // مفيد للـ SSR فقط
  const auth = useCookie<string | null>('auth', cookieOptions)
  const role = useCookie<string | null>('role', cookieOptions)
  const user = useCookie<User | null>('user', cookieOptions)

  const isAuthed = computed(() => auth.value === '1' || !!role.value)
  const userData = computed(() => user.value) // ✅ هذا اللي نستخدمه بكل مكان

  function authHeaders(): Record<string, string> {
    // على الـ client ما نقدر نقرأ token (HttpOnly) لذلك نرجع {}
    // وعلى الـ server (SSR) ممكن يكون متوفر.
    const t = process.server ? token.value : null
    if (t && typeof t === 'string' && t.trim().length > 0) {
      return { Authorization: `Bearer ${t}` }
    }
    return {}
  }

  async function login(payload: LoginRequest) {
    const api = useApi()
    const res: any = await api.post('/Auth/login', payload)
    // token cookie ينضبط من الـ BFF كـ HttpOnly — لا نحاول نخزنه من هنا
    auth.value = '1'
    role.value = res?.user?.role ?? role.value ?? null
    user.value = res?.user ?? null
    return res
  }

  async function register(payload: RegisterRequest) {
    const api = useApi()
    const res: any = await api.post('/Auth/register', payload)
    auth.value = '1'
    role.value = res?.user?.role ?? role.value ?? null
    user.value = res?.user ?? null
    return res
  }

  async function logout() {
    // ✅ امسح كوكيز الـ BFF (خصوصاً token اللي هو HttpOnly)
    try {
      await $fetch('/api/bff/Auth/logout', {
        method: 'POST',
        credentials: 'include',
      })
    } catch {
      // ignore
    }

    // نظف محلياً
    auth.value = null
    role.value = null
    token.value = null
    user.value = null
  }

  return {
    token,
    auth,
    role,
    user,
    userData,
    isAuthed,
    authHeaders,
    login,
    register,
    logout,
  }
})
