import { defineStore } from 'pinia'

function decodeJwt(token: string): any {
  try {
    const payload = token.split('.')[1]
    const json = decodeURIComponent(
      atob(payload.replace(/-/g, '+').replace(/_/g, '/'))
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    )
    return JSON.parse(json)
  } catch {
    return null
  }
}

export const useProfileStore = defineStore('profile', () => {
  const authToken = useCookie<string | null>('token')
  const profile = useState<{ name: string; email: string; phone: string }>('profile', () => ({
    name: '',
    email: '',
    phone: '',
  }))

  watch(
    () => authToken.value,
    (tok) => {
      if (!tok) {
        profile.value = { name: '', email: '', phone: '' }
        return
      }
      const p = decodeJwt(tok)
      if (!p) return
      profile.value = {
        name: (p.name || p.unique_name || '').toString(),
        email: (p.email || '').toString(),
        phone: (p.phone || '').toString(),
      }
    },
    { immediate: true },
  )

  function setManual(data: Partial<{ name: string; email: string; phone: string }>) {
    profile.value = { ...profile.value, ...data }
  }

  return { profile, setManual }
})

function hydrateFromAuth() {
  // لا نحتاج شي خاص: الـ watcher على التوكن ينفذ refresh تلقائياً
  // لكن نتركها حتى ما يصير كراش بالصفحات القديمة
  return refresh()
}

