export default defineNuxtPlugin(async () => {
  const store = useAppearanceStore()
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

  apply()
  watch(
    () => store.data.themes,
    () => apply(),
    { deep: true }
  )
})
