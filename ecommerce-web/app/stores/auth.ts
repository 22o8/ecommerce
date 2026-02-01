// ecommerce-web/app/stores/auth.ts
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
    secure: process.env.NODE_ENV === 'production',
    maxAge: 60 * 60 * 24 * 30, // 30 days
  }

  /**
   * token = HttpOnly (من BFF) - قد لا ينرسل بثبات على iOS in-app
   * access = غير HttpOnly - نستخدمه لإرسال Authorization من الفرونت للـ BFF
   */
  const token = useCookie<string | null>('token', cookieOptions)  // SSR فقط
  const access = useCookie<string | null>('access', cookieOptions) // Client fallback (حل iOS)
  const auth = useCookie<string | null>('auth', cookieOptions)
  const role = useCookie<string | null>('role', cookieOptions)
  const user = useCookie<any>('user', cookieOptions)

  const isAuthed = computed(() => auth.value === '1' || !!(role.value && String(role.value).trim()))
  const userData = computed<User | null>(() => {
    const v = user.value
    if (!v) return null
    if (typeof v === 'string') {
      try { return JSON.parse(v) } catch { return null }
    }
    return v as User
  })

  /**
   * ✅ لا تحذفها أبدًا
   * تمنع 500: auth.initFromCookies is not a function
   */
  function initFromCookies() {
    if (!auth.value && role.value) auth.value = '1'
    if (typeof role.value === 'string') role.value = role.value.trim() as any
    if (typeof access.value === 'string') access.value = access.value.trim()
  }

  function applyAuthFromResponse(res: any) {
    // الـ BFF راح يضبط cookies، بس إحنا هم نخزن fallback
    auth.value = '1'

    const r = res?.user?.role ?? res?.role ?? role.value ?? null
    role.value = typeof r === 'string' ? r.trim() : (r as any)

    // user ممكن يجي object
    const u = res?.user ?? null
    if (u) user.value = u

    // ✅ access fallback من token اللي يرجع بالـ JSON
    const t = (res?.token ?? res?.accessToken ?? null)
    if (t && typeof t === 'string') access.value = t.trim()
  }

  async function login(payload: LoginRequest) {
    const res: any = await $fetch('/api/bff/Auth/login', {
      method: 'POST',
      body: payload,
      credentials: 'include',
    })
    applyAuthFromResponse(res)
    return res
  }

  async function register(payload: RegisterRequest) {
    const res: any = await $fetch('/api/bff/Auth/register', {
      method: 'POST',
      body: payload,
      credentials: 'include',
    })
    applyAuthFromResponse(res)
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
    user.value = null

    // token httpOnly ما ينمسح من JS — الـ BFF يمسحه
    token.value = null
    access.value = null
  }

  return {
    // state
    token,
    access,
    auth,
    role,
    user,

    // getters
    userData,
    isAuthed,

    // helpers
    initFromCookies,

    // actions
    login,
    register,
    logout,
  }
})
