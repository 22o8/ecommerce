// app/composables/useI18n.ts
import { computed } from 'vue'
import ar from '~/i18n/locales/ar.json'
import en from '~/i18n/locales/en.json'

type Locale = 'ar' | 'en'
type Dict = Record<string, any>

const DICTS: Record<Locale, Dict> = { ar: ar as Dict, en: en as Dict }

function getByPath(obj: any, path: string): any {
  if (!path) return undefined
  const parts = path.split('.')
  let cur: any = obj
  for (const p of parts) {
    if (cur && typeof cur === 'object' && p in cur) cur = cur[p]
    else return undefined
  }
  return cur
}

function interpolate(text: string, params?: Record<string, any>) {
  if (!params) return text
  return text.replace(/\{(\w+)\}/g, (_, k) => String(params[k] ?? `{${k}}`))
}

export function useI18n() {
  const ui = useUiStore()

  const locale = computed<Locale>(() => (ui?.locale === 'en' ? 'en' : 'ar'))

  function t(key: string, params?: Record<string, any>) {
    const dict = DICTS[locale.value]
    const raw = getByPath(dict, key)
    if (typeof raw === 'string') return interpolate(raw, params)

    // Fallback: إذا كان المفتاح موجود كـ string مسطح داخل root
    const flat = (dict as any)?.[key]
    if (typeof flat === 'string') return interpolate(flat, params)

    // fallback إلى اللغة الثانية إذا المفتاح مفقود (حتى لا تظهر keys للمستخدم)
    const other: Locale = locale.value === 'ar' ? 'en' : 'ar'
    const raw2 = getByPath(DICTS[other], key) ?? (DICTS[other] as any)?.[key]
    if (typeof raw2 === 'string') return interpolate(raw2, params)

    return key
  }

  return { t, locale }
}
