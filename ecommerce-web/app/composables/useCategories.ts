export type AppCategory = {
  id?: string
  key: string
  nameAr: string
  nameEn?: string | null
  descriptionAr?: string | null
  descriptionEn?: string | null
  imageUrl?: string | null
  sortOrder?: number
  isActive?: boolean
}

const fallbackCategories: AppCategory[] = [
  { key: 'serum', nameAr: 'السيروم', nameEn: 'Serum', descriptionAr: 'للاستكشاف والعناية اليومية.', sortOrder: 10 },
  { key: 'moisturizer', nameAr: 'مرطب', nameEn: 'Moisturizer', descriptionAr: 'ترطيب يومي وملمس ناعم.', sortOrder: 20 },
  { key: 'sunscreen', nameAr: 'واقي الشمس', nameEn: 'Sunscreen', descriptionAr: 'حماية يومية للبشرة.', sortOrder: 30 },
  { key: 'cleanser', nameAr: 'غسول', nameEn: 'Cleanser', descriptionAr: 'تنظيف لطيف وفعّال.', sortOrder: 40 },
  { key: 'toner', nameAr: 'تونر', nameEn: 'Toner', descriptionAr: 'انتعاش وتوازن بعد التنظيف.', sortOrder: 50 },
  { key: 'mask', nameAr: 'ماسك', nameEn: 'Mask', descriptionAr: 'عناية مركزة ولمسات إضافية.', sortOrder: 60 },
  { key: 'eye-care', nameAr: 'العناية بالعين', nameEn: 'Eye Care', descriptionAr: 'حلول خاصة لمنطقة العين.', sortOrder: 70 },
]

export function useCategories() {
  const api = useApi()
  const categories = useState<AppCategory[]>('app-categories', () => [])
  const loading = useState<boolean>('app-categories-loading', () => false)

  async function fetchCategories(force = false) {
    if (loading.value) return categories.value
    if (!force && categories.value.length) return categories.value
    loading.value = true
    try {
      const res: any = await api.get<any[]>('/categories/active', { _ts: Date.now() })
      const arr = Array.isArray(res) ? res : []
      categories.value = arr.length
        ? arr.map((x: any) => ({
            id: x.id,
            key: String(x.key || x.Key || '').toLowerCase(),
            nameAr: String(x.nameAr || x.NameAr || x.name || ''),
            nameEn: x.nameEn || x.NameEn || null,
            descriptionAr: x.descriptionAr || x.DescriptionAr || null,
            descriptionEn: x.descriptionEn || x.DescriptionEn || null,
            imageUrl: x.imageUrl || x.ImageUrl || null,
            sortOrder: Number(x.sortOrder || x.SortOrder || 0),
            isActive: Boolean(x.isActive ?? x.IsActive ?? true),
          })).filter((x: AppCategory) => x.key && x.nameAr)
        : [...fallbackCategories]
    } catch {
      categories.value = [...fallbackCategories]
    } finally {
      loading.value = false
    }
    return categories.value
  }

  return { categories, loading, fetchCategories, fallbackCategories }
}
