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

  const { get, post, put, del, postForm } = useApi()

  const fetchPublic = async () => {
    loading.value = true
    try {
      const res = await get<{ items: BrandDto[] }>('/Brands')
      items.value = (res?.items || []).filter(b => b && b.slug)
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
