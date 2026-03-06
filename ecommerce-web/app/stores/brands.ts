// ecommerce-web/app/stores/brands.ts
import { defineStore } from 'pinia'

export type BrandDto = {
  id: string
  slug: string
  name: string
  description?: string | null
  logoUrl?: string | null
  isActive?: boolean
  createdAt?: string
}

export const useBrandsStore = defineStore('brands', () => {
  const items = ref<BrandDto[]>([])
  const loading = ref(false)

  // ✅ واجهة المتجر تعتمد على publicItems
  // حتى ما تنكسر صفحات الهوم/البراندات إذا تغيّر شكل البيانات.
  const publicItems = computed(() =>
    (items.value || []).filter((b: any) => b && b.slug && (b.isActive ?? true))
  )

  const { get, post, put, del, postForm, buildAssetUrl } = useApi()

  const normalizeBrand = (b: any): BrandDto => {
    const name = (b?.name ?? b?.title ?? b?.brandName ?? '').toString()
    const slug = (b?.slug ?? (name ? name.toLowerCase().replace(/\s+/g, '-') : '')).toString()

    let logoUrl: string | null | undefined =
      b?.logoUrl ?? b?.logo ?? b?.imageUrl ?? b?.image ?? b?.coverImage ?? null

    // إذا كان رابط نسبي، حوله لرابط كامل لتجنب 404
    if (logoUrl && typeof logoUrl === 'string' && logoUrl.startsWith('/')) {
      logoUrl = buildAssetUrl(logoUrl)
    }

    return {
      id: (b?.id ?? '').toString(),
      slug,
      name,
      description: b?.description ?? null,
      logoUrl: logoUrl ?? null,
      isActive: b?.isActive ?? true,
      createdAt: b?.createdAt,
    }
  }

  const fetchPublic = async (take: number = 10) => {
    loading.value = true
    try {
      // بعض السيرفرات تدعم take/pageSize وبعضها لا؛ جرّب take أولاً.
      const res: any = await get<any>(`/Brands?take=${encodeURIComponent(String(take))}`)
      // بعض الـ endpoints ترجع {items} وبعضها {page,totalCount,items}
      const list = Array.isArray(res) ? res : (res?.items || [])
      items.value = (list || []).map(normalizeBrand).filter(b => b && b.slug)
    } catch (e) {
      // لا تكسر SSR/الهوم إذا صار خطأ بالـ API
      console.warn('[brands] fetchPublic failed', e)
      items.value = []
    } finally {
      loading.value = false
    }
  }

  const fetchAdmin = async () => {
    loading.value = true
    try {
      // AdminBrandsController يرجّع Array مباشرة
      const res = await get<any>('/admin/brands')
      items.value = Array.isArray(res) ? res : (res?.items || [])
    } finally {
      loading.value = false
    }
  }

  const getBySlug = async (slug: string) => {
    return await get<BrandDto>(`/Brands/slug/${encodeURIComponent(slug)}`)
  }

  const createBrand = async (payload: { name: string; slug?: string; description?: string; isActive?: boolean }) => {
    return await post<{ id: string }>('/admin/brands', payload)
  }

  const updateBrand = async (id: string, payload: { name: string; slug?: string; description?: string; isActive?: boolean }) => {
    return await put<{ message: string }>(`/admin/brands/${id}`, payload)
  }

  const deleteBrand = async (id: string) => {
    return await del<{ message: string }>(`/admin/brands/${id}`)
  }

  const uploadLogo = async (id: string, file: File) => {
    const fd = new FormData()
    fd.append('file', file)
    return await postForm<{ logoUrl: string }>(`/admin/brands/${id}/logo`, fd)
  }

  return {
    items,
    loading,
    publicItems,
    fetchPublic,
    fetchAdmin,
    getBySlug,
    createBrand,
    updateBrand,
    deleteBrand,
    uploadLogo,
  }
})
