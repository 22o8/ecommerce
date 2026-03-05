export default defineNuxtPlugin(async () => {
  const store = useAppearanceStore()
  const route = useRoute()
  if (!store.loaded) await store.refresh()

  const apply = () => {
    if (!process.client) return
    const el = document.documentElement

    // clear old classes
    ;[...el.classList]
      .filter((c) => c.startsWith('theme-'))
      .forEach((c) => el.classList.remove(c))

    store.data.themes.forEach((t) => el.classList.add(`theme-${t}`))
  }

  // لا نطبّق ثيمات/تأثيرات المتجر داخل صفحات الأدمن حتى ما يصير تغيير تلقائي أو تلخبط
  if (!route.path?.startsWith('/admin')) apply()
  watch(
    () => store.data.themes,
    () => {
      if (route.path?.startsWith('/admin')) return
      apply()
    },
    { deep: true }
  )
})
