// عدّاد الزيارات العام
// يرسل POST /api/metrics/visit عند التنقل بين الصفحات

export default defineNuxtPlugin(() => {
  const api = useApi()
  const route = useRoute()

  const KEY = 'metrics:lastVisit'

  async function send(path: string) {
    try {
      await api.post('/metrics/visit', { path })
    } catch {
      // ignore
    }
  }

  function shouldSend(path: string) {
    try {
      const raw = sessionStorage.getItem(KEY)
      if (!raw) return true
      const prev = JSON.parse(raw)
      const lastPath = String(prev?.path || '')
      const lastAt = Number(prev?.at || 0)

      // نفس الصفحة + قريب جداً => لا
      if (lastPath === path && Date.now() - lastAt < 8000) return false
      return true
    } catch {
      return true
    }
  }

  function save(path: string) {
    try {
      sessionStorage.setItem(KEY, JSON.stringify({ path, at: Date.now() }))
    } catch {
      // ignore
    }
  }

  watch(
    () => route.fullPath,
    (p) => {
      const path = String(p || '/')
      if (!import.meta.client) return
      if (!shouldSend(path)) return
      save(path)
      send(path)
    },
    { immediate: true }
  )
})
