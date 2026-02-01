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
   * ✅ ملاحظة مهمة:
   * الـ BFF يضبط cookie اسمها "token" كـ HttpOnly.
   * يعني المتصفح/JS ما يقدر يقراها على الـ client.
   * لذلك نعتمد على كوكيز غير HttpOnly يضبطها الـ BFF: auth=1 و role و user.
   */
  const token = useCookie<string | null>('token', cookieOptions) // SSR فقط
  const auth = useCookie<string | null>('auth', cookieOptions)
  const role = useCookie<string | null>('role', cookieOptions)

  // user قد تكون JSON string لأن BFF يخزنها JSON.stringify
  const userRaw = useCookie<any>('user', cookieOptions)

  const user = computed<User | null>(() => {
    const v = userRaw.value
    if (!v) return null
    if (typeof v === 'object') return v as User
    if (typeof v === 'string') {
      try {
        return JSON.parse(v) as User
      } catch {
        return null
      }
    }
    return null
  })

  const isAuthed = computed(() => auth.value === '1' || !!(role.value && String(role.value).trim()))
  const userData = computed(() => user.value)

  /**
   * ✅ مهم للـ Plugin auth-init.ts
   * يمنع: auth.initFromCookies is not a function
   */
  function initFromCookies() {
    if (!auth.value && role.value) auth.value = '1'
    if (role.value && typeof role.value === 'string') role.value = role.value.trim()
  }

  function applyAuthFromResponse(res: any) {
    auth.value = '1'
    const r = (res?.user?.role ?? res?.role ?? role.value ?? null)
    role.value = typeof r === 'string' ? r.trim() : (r as any)

    // نخزن user كسترنغ JSON حتى تبقى ثابتة
    if (res?.user) userRaw.value = JSON.stringify(res.user)
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
    userRaw.value = null
    token.value = null
  }

  return {
    // state
    token,
    auth,
    role,
    user: userRaw,

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
