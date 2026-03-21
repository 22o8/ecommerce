export type AppCategory = {
  id?: string
  key: string
  nameAr: string
  nameEn?: string | null
  descriptionAr?: string | null
  descriptionEn?: string | null
  imageUrl?: string | null
  section?: string | null
  sortOrder?: number
  isActive?: boolean
}

const fallbackCategories: AppCategory[] = []

export function useCategories() {
  const api = useApi()
  const categories = useState<AppCategory[]>('app-categories', () => [])
  const problemCategories = useState<AppCategory[]>('app-problem-categories', () => [])
  const loading = useState<boolean>('app-categories-loading', () => false)

  function normalize(arr: any[]): AppCategory[] {
    return arr.map((x: any) => ({
      id: x.id,
      key: String(x.key || x.Key || '').toLowerCase(),
      nameAr: String(x.nameAr || x.NameAr || x.name || ''),
      nameEn: x.nameEn || x.NameEn || null,
      descriptionAr: x.descriptionAr || x.DescriptionAr || null,
      descriptionEn: x.descriptionEn || x.DescriptionEn || null,
      imageUrl: x.imageUrl || x.ImageUrl || null,
      section: x.section || x.Section || 'regular',
      sortOrder: Number(x.sortOrder || x.SortOrder || 0),
      isActive: Boolean(x.isActive ?? x.IsActive ?? true),
    })).filter((x: AppCategory) => x.key && x.nameAr)
  }

  async function fetchCategories(force = false, section: 'regular' | 'problem' = 'regular') {
    const target = section === 'problem' ? problemCategories : categories
    if (loading.value) return target.value
    if (!force && target.value.length) return target.value
    loading.value = true
    try {
      const res: any = await api.get<any[]>('/categories/active', { _ts: Date.now(), section })
      const arr = Array.isArray(res) ? res : []
      target.value = arr.length ? normalize(arr) : []
    } catch {
      target.value = []
    } finally {
      loading.value = false
    }
    return target.value
  }

  return { categories, problemCategories, loading, fetchCategories, fallbackCategories }
}
