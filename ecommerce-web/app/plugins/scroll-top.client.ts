export default defineNuxtPlugin((nuxtApp) => {
  if (process.client && 'scrollRestoration' in history) {
    history.scrollRestoration = 'manual'
  }

  nuxtApp.hook('page:finish', () => {
    window.scrollTo({ top: 0, left: 0, behavior: 'auto' })
  })

  const router = useRouter()
  router.afterEach((_to, _from) => {
    requestAnimationFrame(() => {
      window.scrollTo({ top: 0, left: 0, behavior: 'auto' })
    })
  })
})
