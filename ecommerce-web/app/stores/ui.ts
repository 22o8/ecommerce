// app/stores/ui.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useCookie } from '#app'

type Theme = 'dark' | 'light'
type Locale = 'en' | 'ar'

type ApiErrorInfo = {
  at: string
  url: string
  method?: string
  status?: number
  message: string
  data?: any
}


export const useUiStore = defineStore('ui', () => {
  // ✅ الافتراضي Light
  const theme = ref<Theme>('light')
  // ✅ نفس مصدر اللغة مع useI18n (كوكي واحدة) حتى ما يصير اختلاف بين الـ Navbar وباقي المحتوى
  const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
  const locale = ref<Locale>(localeCookie.value)

  // ✅ وضع التشخيص: يعرض آخر خطأ API بشكل واضح (للأجهزة التي تفشل بدون رسالة)
  const apiDebug = ref<boolean>(false)
  const lastApiError = ref<ApiErrorInfo | null>(null)

  function applyThemeToHtml(t: Theme) {
    if (import.meta.server) return
    const root = document.documentElement
    root.classList.toggle('theme-dark', t === 'dark')
    root.classList.toggle('theme-light', t === 'light')
    root.classList.toggle('dark', t === 'dark')
  }

  function applyLocaleToHtml(l: Locale) {
    if (import.meta.server) return
    const root = document.documentElement
    // Keep layout stable: always LTR and do not toggle structural CSS classes when changing locale.
    root.setAttribute('lang', l)
    root.setAttribute('dir', l === 'ar' ? 'rtl' : 'ltr')
    root.classList.toggle('lang-ar', l === 'ar')
    root.classList.toggle('lang-en', l === 'en')
  }

  function initClient() {
    if (import.meta.server) return

    const savedTheme = (localStorage.getItem('theme') as Theme | null)
    theme.value = savedTheme === 'dark' ? 'dark' : 'light'
    // مصدر اللغة الأساسي هو الكوكي (يتم تحديثها من زر تغيير اللغة)
    locale.value = localeCookie.value === 'ar' ? 'ar' : 'en'

    apiDebug.value = localStorage.getItem('apiDebug') === '1'

    applyThemeToHtml(theme.value)
    applyLocaleToHtml(locale.value)
  }

  function toggleTheme() {
    theme.value = theme.value === 'dark' ? 'light' : 'dark'
    if (!import.meta.server) localStorage.setItem('theme', theme.value)
    applyThemeToHtml(theme.value)
  }

  function setLocale(l: Locale) {
    locale.value = l
    // ✅ حدث الكوكي حتى i18n وباقي التطبيق يتزامن
    localeCookie.value = l
    applyLocaleToHtml(locale.value)
  }


  function setApiDebugEnabled(v: boolean) {
    apiDebug.value = !!v
    if (!import.meta.server) localStorage.setItem('apiDebug', apiDebug.value ? '1' : '0')
  }

  function setLastApiError(err: ApiErrorInfo) {
    lastApiError.value = err
  }

  function clearLastApiError() {
    lastApiError.value = null
  }

  return { theme, locale, apiDebug, lastApiError, initClient, toggleTheme, setLocale, setApiDebugEnabled, setLastApiError, clearLastApiError, applyLocaleToHtml, applyThemeToHtml }
})
