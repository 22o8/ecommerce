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

  /**
   * ✅ ملاحظة مهمة:
   * الـ BFF يضبط cookie اسمها "token" كـ HttpOnly.
   * يعني المتصفح/JS ما يقدر يقراها (على client useCookie يرجع null).
   * لذلك نعتمد على كوكيز غير HttpOnly يضبطها الـ BFF: auth=1 و role و user.
   */
  const token = useCookie<string | null>('token', cookieOptions) // مفيد للـ SSR فقط
  const auth = useCookie<string | null>('auth', cookieOptions)
  const role = useCookie<string | null>('role', cookieOptions)
  const user = useCookie<User | null>('user', cookieOptions)

  const isAuthed = computed(() => auth.value === '1' || !!role.value)
  const userData = computed(() => user.value)

  /**
   * ✅ مهم للـ Plugin auth-init.ts
   * يمنع 500: auth.initFromCookies is not a function
   * ويضمن تثبيت الحالة من الكوكيز بعد أي Refresh.
   */
  function initFromCookies() {
    // ما نحتاج نسوي شي كبير لأن state أصلاً مبني على useCookie
    // بس نخليها حتى أي مكان يناديها (plugin) ما يطيح.
    // (اختياري) إذا role موجود و auth فاضي نخليه 1
    if (!auth.value && role.value) auth.value = '1'
  }

  function authHeaders(): Record<string, string> {
    // على الـ client ما نقدر نقرأ token (HttpOnly)
    // على الـ server (SSR) ممكن يكون متوفر.
    const t = process.server ? token.value : null
    if (t && typeof t === 'string' && t.trim().length > 0) {
      return { Authorization: `Bearer ${t}` }
    }
    return {}
  }

  function applyAuthFromResponse(res: any) {
    // الـ token يضبطه الـ BFF كـ HttpOnly (Set-Cookie) — لا نحاول نخزنه من هنا
    auth.value = '1'
    role.value = (res?.user?.role ?? role.value ?? null) as any
    user.value = (res?.user ?? user.value ?? null) as any
  }

  async function login(payload: LoginRequest) {
    const api = useApi()
    const res: any = await api.post('/Auth/login', payload)
    applyAuthFromResponse(res)
    return res
  }

  async function register(payload: RegisterRequest) {
    const api = useApi()
    const res: any = await api.post('/Auth/register', payload)
    applyAuthFromResponse(res)
    return res
  }

  async function logout() {
    // ✅ امسح كوكيز الـ BFF (خصوصاً token HttpOnly)
    try {
      await $fetch('/api/bff/Auth/logout', {
        method: 'POST',
        credentials: 'include',
      })
    } catch {
      // ignore
    }

    // ✅ نظف محلياً (غير HttpOnly)
    auth.value = null
    role.value = null
    user.value = null

    // ملاحظة: token HttpOnly ما نكدر نمسحه من JS فعلياً
    // لكن خليها null حتى SSR إذا قرأه من نفس السياق ما يبقى متذبذب
    token.value = null
  }

  return {
    token,
    auth,
    role,
    user,
    userData,
    isAuthed,
    initFromCookies,
    authHeaders,
    login,
    register,
    logout,
  }
})
