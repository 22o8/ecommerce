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
  const publicLoaded = ref(false)
  const publicItems = computed(() => (items.value || []).filter((b: any) => b && b.slug && (b.isActive ?? true)))

  const { get, post, put, del, postForm, buildAssetUrl } = useApi()

  const normalizeBrand = (b: any): BrandDto => {
    const name = (b?.name ?? b?.title ?? b?.brandName ?? '').toString()
    const slug = (b?.slug ?? (name ? name.toLowerCase().replace(/\s+/g, '-') : '')).toString()
    let logoUrl: string | null | undefined = b?.logoUrl ?? b?.logo ?? b?.imageUrl ?? b?.image ?? b?.coverImage ?? null
    if (logoUrl && typeof logoUrl === 'string' && logoUrl.startsWith('/')) logoUrl = buildAssetUrl(logoUrl)
    return { id: (b?.id ?? '').toString(), slug, name, description: b?.description ?? null, logoUrl: logoUrl ?? null, isActive: b?.isActive ?? true, createdAt: b?.createdAt }
  }

  const fetchPublic = async (take: number = 10, force = false) => {
    if (publicLoaded.value && !force && items.value.length) return
    loading.value = true
    try {
      const res: any = await get<any>(`/Brands?take=${encodeURIComponent(String(take))}`)
      const list = Array.isArray(res) ? res : (res?.items || [])
      items.value = (list || []).map(normalizeBrand).filter(b => b && b.slug)
      publicLoaded.value = true
    } catch (e) {
      console.warn('[brands] fetchPublic failed', e)
      items.value = []
    } finally {
      loading.value = false
    }
  }

  const fetchAdmin = async () => {
    loading.value = true
    try {
      const res = await get<any>('/admin/brands')
      items.value = Array.isArray(res) ? res : (res?.items || [])
    } finally {
      loading.value = false
    }
  }

  const getBySlug = async (slug: string) => await get<BrandDto>(`/Brands/slug/${encodeURIComponent(slug)}`)
  const createBrand = async (payload: { name: string; slug?: string; description?: string; isActive?: boolean }) => await post<{ id: string }>('/admin/brands', payload)
  const updateBrand = async (id: string, payload: { name: string; slug?: string; description?: string; isActive?: boolean }) => await put<{ message: string }>(`/admin/brands/${id}`, payload)
  const deleteBrand = async (id: string) => await del<{ message: string }>(`/admin/brands/${id}`)
  const uploadLogo = async (id: string, file: File) => { const fd = new FormData(); fd.append('file', file); return await postForm<{ logoUrl: string }>(`/admin/brands/${id}/logo`, fd) }

  return { items, loading, publicItems, fetchPublic, fetchAdmin, getBySlug, createBrand, updateBrand, deleteBrand, uploadLogo }
})
