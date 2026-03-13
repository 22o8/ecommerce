import { computed } from 'vue'
import { useI18n } from '~/composables/useI18n'

export type ProductLocale = 'ar' | 'en'

export type TaxonomySubCategory = {
  key: string
  label: Record<ProductLocale, string>
  category: string
}

export type TaxonomyCategory = {
  key: string
  icon: string
  label: Record<ProductLocale, string>
}

const categories: TaxonomyCategory[] = [
  { key: 'serum', icon: '💧', label: { ar: 'سيروم', en: 'Serum' } },
  { key: 'moisturizer', icon: '🧴', label: { ar: 'مرطب', en: 'Moisturizer' } },
  { key: 'sunscreen', icon: '☀️', label: { ar: 'واقي شمس', en: 'Sunscreen' } },
  { key: 'cleanser', icon: '🫧', label: { ar: 'غسول', en: 'Cleanser' } },
  { key: 'perfume', icon: '🌸', label: { ar: 'عطر', en: 'Perfume' } },
  { key: 'eye-care', icon: '👁️', label: { ar: 'العناية بالعين', en: 'Eye Care' } },
  { key: 'toner', icon: '🧪', label: { ar: 'تونر', en: 'Toner' } },
  { key: 'mask', icon: '✨', label: { ar: 'ماسك', en: 'Mask' } },
  { key: 'lip-care', icon: '💋', label: { ar: 'العناية بالشفاه', en: 'Lip Care' } },
]

const subCategories: TaxonomySubCategory[] = [
  { key: 'face-serum', category: 'serum', label: { ar: 'سيروم الوجه', en: 'Face Serum' } },
  { key: 'face-cream', category: 'moisturizer', label: { ar: 'كريم الوجه', en: 'Face Cream' } },
  { key: 'day-cream', category: 'moisturizer', label: { ar: 'كريم نهاري', en: 'Day Cream' } },
  { key: 'night-cream', category: 'moisturizer', label: { ar: 'كريم ليلي', en: 'Night Cream' } },
  { key: 'sun-cream', category: 'sunscreen', label: { ar: 'واقي شمس كريم', en: 'Sun Cream' } },
  { key: 'sun-fluid', category: 'sunscreen', label: { ar: 'واقي شمس فلويد', en: 'Sun Fluid' } },
  { key: 'gel-cleanser', category: 'cleanser', label: { ar: 'غسول جل', en: 'Gel Cleanser' } },
  { key: 'foam-cleanser', category: 'cleanser', label: { ar: 'غسول رغوي', en: 'Foam Cleanser' } },
  { key: 'eye-serum', category: 'eye-care', label: { ar: 'سيروم العين', en: 'Eye Serum' } },
  { key: 'eye-cream', category: 'eye-care', label: { ar: 'كريم العين', en: 'Eye Cream' } },
  { key: 'eye-gel', category: 'eye-care', label: { ar: 'جل العين', en: 'Eye Gel' } },
  { key: 'lip-balm', category: 'lip-care', label: { ar: 'بلسم شفاه', en: 'Lip Balm' } },
]

export function useProductTaxonomy() {
  const { locale } = useI18n()

  const categoryOptions = computed(() => categories.map((c) => ({
    ...c,
    name: c.label[(locale.value as ProductLocale) || 'ar'] ?? c.label.ar,
  })))

  const subCategoryOptions = (categoryKey?: string) => subCategories
    .filter((s) => !categoryKey || s.category === categoryKey)
    .map((s) => ({
      ...s,
      name: s.label[(locale.value as ProductLocale) || 'ar'] ?? s.label.ar,
    }))

  return {
    categories,
    subCategories,
    categoryOptions,
    subCategoryOptions,
  }
}
