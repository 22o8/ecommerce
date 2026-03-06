// عدّاد الزيارات العام
// يرسل POST /api/metrics/visit عند التنقل بين الصفحات

export default defineNuxtPlugin(() => {
  const api = useApi()
  const route = useRoute()

  const KEY = 'metrics:lastVisit'
  const conn: any = import.meta.client ? (navigator as any)?.connection : null
  const isConstrained = !!conn?.saveData || /2g/.test(String(conn?.effectiveType || ''))

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
      if (lastPath === path && Date.now() - lastAt < 15000) return false
      // على الهاتف أو الشبكة الضعيفة نخفف أكثر
      if (Date.now() - lastAt < 30000 && /\/(products|product|brands|cart|favorites|admin)/.test(path)) return false
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
      if (path.startsWith('/admin')) return
      if (isConstrained) return
      if (!shouldSend(path)) return
      save(path)
      send(path)
    },
    { immediate: true }
  )
})
