export default defineNuxtPlugin(() => {
  const { t } = useI18n()
  return {
    provide: { t },
    vueApp: {
      config: {
        globalProperties: { $t: t }
      }
    }
  }
})