// عدّاد الزيارات العام
// يرسل POST /api/metrics/visit بشكل محدود حتى ما يسبب 429

export default defineNuxtPlugin(() => {
  const api = useApi()
  const route = useRoute()

  const KEY = 'metrics:visits:v2'
  const TTL_MS = 60 * 1000

  async function send(path: string) {
    try {
      await api.post('/metrics/visit', { path })
    } catch {
      // ignore
    }
  }

  function readMap(): Record<string, number> {
    try {
      const raw = sessionStorage.getItem(KEY)
      return raw ? JSON.parse(raw) : {}
    } catch {
      return {}
    }
  }

  function shouldSend(path: string) {
    try {
      const m = readMap()
      const lastAt = Number(m[path] || 0)
      return !lastAt || (Date.now() - lastAt > TTL_MS)
    } catch {
      return true
    }
  }

  function save(path: string) {
    try {
      const m = readMap()
      m[path] = Date.now()
      sessionStorage.setItem(KEY, JSON.stringify(m))
    } catch {
      // ignore
    }
  }

  let inFlight = false

  watch(
    () => route.fullPath,
    async (p) => {
      const path = String(p || '/')
      if (!import.meta.client) return
      if (path.startsWith('/admin')) return
      if (!shouldSend(path)) return
      if (inFlight) return
      inFlight = true
      save(path)
      try {
        await send(path)
      } finally {
        inFlight = false
      }
    },
    { immediate: true }
  )
})
