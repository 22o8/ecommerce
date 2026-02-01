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
  const token = useCookie<string | null>('token', cookieOptions) // للـ SSR فقط (إذا انقرت)
  const auth = useCookie<string | null>('auth', cookieOptions)
  const role = useCookie<string | null>('role', cookieOptions)
  const user = useCookie<User | null>('user', cookieOptions)

  const isAuthed = computed(() => auth.value === '1' || !!(role.value && role.value.trim()))
  const userData = computed(() => user.value)

  /**
   * ✅ مهم للـ Plugin auth-init.ts
   * يمنع 500: auth.initFromCookies is not a function
   * ويضمن تثبيت الحالة بعد أي Refresh
   */
  function initFromCookies() {
    // بما أن useCookie مرتبط بالـ state مباشرة، ما نحتاج تحميل إضافي.
    // بس نخليها حتى أي مكان يناديها ما يطيح.
    if (!auth.value && role.value) auth.value = '1'
    if (role.value && typeof role.value === 'string') role.value = role.value.trim()
  }

  function authHeaders(): Record<string, string> {
    // على الـ client ما نقدر نقرأ token لأنه HttpOnly
    // على السيرفر (SSR) ممكن يكون موجود
    const t = process.server ? token.value : null
    if (t && typeof t === 'string' && t.trim().length > 0) {
      return { Authorization: `Bearer ${t}` }
    }
    return {}
  }

  function applyAuthFromResponse(res: any) {
    // token يجي Set-Cookie من الـ BFF كـ HttpOnly (لا نخزنه هنا)
    auth.value = '1'
    const r = (res?.user?.role ?? res?.role ?? role.value ?? null)
    role.value = typeof r === 'string' ? r.trim() : (r as any)
    user.value = (res?.user ?? user.value ?? null) as any
  }

  async function login(payload: LoginRequest) {
    // ✅ تسجيل الدخول عبر BFF (مهم جداً للموبايل حتى تنحفظ الكوكيز)
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
    // ✅ امسح كوكيز الـ BFF (خصوصاً token HttpOnly) من السيرفر
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
    // بس نخليه null حتى SSR إذا يعتمد عليه ما يتذبذب
    token.value = null
  }

  return {
    // state
    token,
    auth,
    role,
    user,

    // getters
    userData,
    isAuthed,

    // helpers
    initFromCookies,
    authHeaders,

    // actions
    login,
    register,
    logout,
  }
})
