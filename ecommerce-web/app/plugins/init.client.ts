export default defineNuxtPlugin(() => {
  const ui = useUiStore()
  ui.initClient()
})
