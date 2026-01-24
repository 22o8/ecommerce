import { defineStore } from 'pinia'
import { computed } from 'vue'
import { useApi } from '~/composables/useApi'

type LoginRequest = { email: string; password: string }
type RegisterRequest = { fullName: string; email: string; password: string }

export type User = {
  id: string
  fullName: string
  email: string
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

  const token = useCookie<string | null>('token', cookieOptions)
  const user = useCookie<User | null>('user', cookieOptions)

  const isAuthed = computed(() => !!token.value)
  const userData = computed(() => user.value) // ✅ هذا اللي نستخدمه بكل مكان

  function authHeaders(): Record<string, string> {
    const t = token.value
    if (t && typeof t === 'string' && t.trim().length > 0) {
      return { Authorization: `Bearer ${t}` }
    }
    return {}
  }

  async function login(payload: LoginRequest) {
    const api = useApi()
    const res: any = await api.post('/Auth/login', payload)
    token.value = res?.token ?? null
    user.value = res?.user ?? null
    return res
  }

  async function register(payload: RegisterRequest) {
    const api = useApi()
    return await api.post('/Auth/register', payload)
  }

  function logout() {
    token.value = null
    user.value = null
  }

  return {
    token,
    user,
    userData,
    isAuthed,
    authHeaders,
    login,
    register,
    logout,
  }
})
