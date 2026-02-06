// app/stores/ui.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useCookie } from '#app'

type Theme = 'dark' | 'light'
type Locale = 'en' | 'ar'

export const useUiStore = defineStore('ui', () => {
  // ✅ الافتراضي Light
  const theme = ref<Theme>('light')
  // ✅ نفس مصدر اللغة مع useI18n (كوكي واحدة) حتى ما يصير اختلاف بين الـ Navbar وباقي المحتوى
  const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
  const locale = ref<Locale>(localeCookie.value)

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
    root.setAttribute('dir', 'ltr')
  }

  function initClient() {
    if (import.meta.server) return

    const savedTheme = (localStorage.getItem('theme') as Theme | null)
    theme.value = savedTheme === 'dark' ? 'dark' : 'light'
    // مصدر اللغة الأساسي هو الكوكي (يتم تحديثها من زر تغيير اللغة)
    locale.value = localeCookie.value === 'ar' ? 'ar' : 'en'

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

  return { theme, locale, initClient, toggleTheme, setLocale, applyLocaleToHtml, applyThemeToHtml }
})
