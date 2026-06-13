export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()
  const route = useRoute()
  const gaId = String(config.public.gaId || '').trim()

  if (!gaId || !gaId.startsWith('G-')) return

  const w = window as any
  if (!w.dataLayer) w.dataLayer = []
  if (!w.gtag) {
    w.gtag = function gtag() { w.dataLayer.push(arguments) }
  }

  const scriptId = 'drseoul-google-analytics'
  if (!document.getElementById(scriptId)) {
    const s = document.createElement('script')
    s.id = scriptId
    s.async = true
    s.src = `https://www.googletagmanager.com/gtag/js?id=${encodeURIComponent(gaId)}`
    document.head.appendChild(s)
  }

  w.gtag('js', new Date())
  w.gtag('config', gaId, {
    send_page_view: false,
    anonymize_ip: true,
    currency: 'IQD',
    country: 'IQ',
    linker: { domains: ['drseoulbeauty.store'] },
  })

  function sendPageView(path: string) {
    const pagePath = path || '/'
    w.gtag('event', 'page_view', {
      page_title: document.title || 'DR SEOUL BEAUTY',
      page_location: window.location.href,
      page_path: pagePath,
      currency: 'IQD',
      market: 'Iraq',
    })
  }

  watch(
    () => route.fullPath,
    (path) => nextTick(() => sendPageView(String(path || '/'))),
    { immediate: true }
  )
})
