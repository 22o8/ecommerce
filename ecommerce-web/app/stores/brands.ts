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
      const res = await get<{ items: BrandDto[] }>('/admin/brands')
      items.value = res?.items || []
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
    fetchPublic,
    fetchAdmin,
    getBySlug,
    createBrand,
    updateBrand,
    deleteBrand,
    uploadLogo,
  }
})
