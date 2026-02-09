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
  // ✅ API client (يشتغل على السيرفر والكلينت)
  // كان يسبب خطأ: api is not defined
  const api = useApi()

  const cookieOptions = {
    default: () => null,
    path: '/',
    sameSite: 'lax' as const,
    secure: process.env.NODE_ENV === 'production',
    maxAge: 60 * 60 * 24 * 30,
  }

  /**
   * token = HttpOnly (من BFF) - غالباً JS ما يقره، وأحياناً in-app ما يثبته
   * access = غير HttpOnly - fallback حتى نرسل Authorization للـ BFF إذا احتجنا
   */
  const token = useCookie<string | null>('token', cookieOptions)      // SSR غالباً فقط
  const access = useCookie<string | null>('access', cookieOptions)    // Client fallback
  const auth = useCookie<string | null>('auth', cookieOptions)
  const role = useCookie<string | null>('role', cookieOptions)
  const user = useCookie<any>('user', cookieOptions)

  const userData = computed<User | null>(() => {
    const v = user.value
    if (!v) return null
    if (typeof v === 'string') {
      try { return JSON.parse(v) } catch { return null }
    }
    return v as User
  })

  const normalizedRole = computed(() => {
    const r1 = userData.value?.role
    const r2 = role.value
    return String(r1 || r2 || '').trim().toLowerCase()
  })

  const isAuthed = computed(() => auth.value === '1' || !!normalizedRole.value)
  const isAdmin = computed(() => normalizedRole.value === 'admin')

  /**
   * ✅ لا تحذفها أبدًا
   * تمنع 500: auth.initFromCookies is not a function
   */
  function initFromCookies() {
    // إذا عندنا role أو user وماكو auth، ثبّت auth=1
    if (!auth.value && (role.value || user.value)) auth.value = '1'

    // نظّف مسافات
    if (typeof role.value === 'string') role.value = role.value.trim() as any
    if (typeof access.value === 'string') access.value = access.value.trim()
  }

  function applyAuthFromResponse(res: any) {
    auth.value = '1'

    // role
    const r = res?.user?.role ?? res?.role ?? role.value ?? null
    role.value = typeof r === 'string' ? r.trim() : (r as any)

    // user
    const u = res?.user ?? null
    if (u) user.value = u

    // ✅ access fallback من token اللي يرجع بالـ JSON
    const t = (res?.token ?? res?.accessToken ?? null)
    if (t && typeof t === 'string') access.value = t.trim()
  }

  async function login(payload: LoginRequest) {
    const res: any = await api.post('/Auth/login', payload)
    applyAuthFromResponse(res)
    return res
  }

  async function register(payload: RegisterRequest) {
    const res: any = await api.post('/Auth/register', payload)
    applyAuthFromResponse(res)
    return res
  }

  async function logout() {
    try {
      await api.post('/Auth/logout', {})
    } catch {
      // ignore
    }

    // نظف محلياً
    auth.value = null
    role.value = null
    user.value = null
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
    isAdmin,

    // helpers
    initFromCookies,

    // actions
    login,
    register,
    logout,
  }
})
