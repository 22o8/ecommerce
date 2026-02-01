import { defineStore } from 'pinia'
import { computed } from 'vue'

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
    // على Vercel دائما https، فخليها true بالإنتاج
    secure: process.env.NODE_ENV === 'production',
    maxAge: 60 * 60 * 24 * 30, // 30 days
  }

  // token يكون HttpOnly من الـ BFF (مفيد للـ SSR فقط)
  const token = useCookie<string | null>('token', cookieOptions)

  // كوكيز غير HttpOnly نستخدمها بالفرونت حتى ما نحتاج نقرا token
  const auth = useCookie<string | null>('auth', cookieOptions)
  const role = useCookie<string | null>('role', cookieOptions)
  const user = useCookie<User | null>('user', cookieOptions)

  const isAuthed = computed(() => auth.value === '1' || !!(role.value && role.value.trim()))
  const userData = computed(() => user.value)

  // لا تعتمد Authorization header بالـ client (لأن token HttpOnly)
  function authHeaders(): Record<string, string> {
    const t = process.server ? token.value : null
    if (t && typeof t === 'string' && t.trim().length > 0) return { Authorization: `Bearer ${t}` }
    return {}
  }

  async function login(payload: LoginRequest) {
    // ✅ مهم جداً: خلي اللوجن يروح للـ BFF حتى يكتب الكوكيز على نفس دومين Vercel
    const res: any = await $fetch('/api/bff/Auth/login', {
      method: 'POST',
      body: payload,
      credentials: 'include',
    })

    // ✅ ثبّت كوكيز الفرونت حتى الأدمن يشتغل حتى لو token ما ينقري
    auth.value = '1'
    role.value = (res?.user?.role || res?.role || role.value || '').toString()
    user.value = res?.user ?? null

    return res
  }

  async function register(payload: RegisterRequest) {
    const res: any = await $fetch('/api/bff/Auth/register', {
      method: 'POST',
      body: payload,
      credentials: 'include',
    })

    auth.value = '1'
    role.value = (res?.user?.role || res?.role || role.value || '').toString()
    user.value = res?.user ?? null

    return res
  }

  async function logout() {
    try {
      await $fetch('/api/bff/Auth/logout', {
        method: 'POST',
        credentials: 'include',
      })
    } catch {
      // ignore
    }

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
