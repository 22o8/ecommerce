// app/stores/products.ts
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'

type Paged<T> = {
  items: T[]
  totalCount: number
}

type FetchParams = {
  page?: number
  pageSize?: number
  q?: string
  sort?: 'new' | 'priceAsc' | 'priceDesc' | string
}

export const useProductsStore = defineStore('products', () => {
  const api = useApi()

function normalizeProduct(p: any){
  if(!p) return p
  // API returns Title/PriceUsd/coverImage; UI expects name/priceUsd/imageUrl
  const name = p.name ?? p.title ?? p.Title ?? p.productName ?? ''
  const priceUsd = p.priceUsd ?? p.PriceUsd ?? p.price ?? p.Price ?? 0
  const cover = p.coverImage ?? p.imageUrl ?? p.ImageUrl ?? null
  const slug = p.slug ?? p.Slug ?? null
  const images = p.images ?? p.Images ?? null
  const description = p.description ?? p.Description ?? null
  return { ...p, name, priceUsd, imageUrl: cover, slug, images, description }
}


  const items = ref<any[]>([])
  const totalCount = ref(0)
  const loading = ref(false)

  // Featured is simply first few items from latest fetch
  const featured = computed(() => items.value.slice(0, 8))
  const hasFeatured = computed(() => featured.value.length > 0)

  async function fetch(params: FetchParams = {}) {
    loading.value = true
    try {
      const res = await api.get<Paged<any>>('/Products', {
        page: params.page || 1,
        pageSize: params.pageSize || 12,
        q: params.q || undefined,
        sort: params.sort || 'new',
      })

      const raw = (res as any)?.items
      const arr = Array.isArray(raw) ? raw : (Array.isArray(res as any) ? (res as any) : [])
      items.value = arr.map(normalizeProduct)
      totalCount.value = Number((res as any)?.totalCount || (res as any)?.total || items.value.length || 0)
      return res
    } finally {
      loading.value = false
    }
  }

  async function fetchFeatured() {
    return await fetch({ page: 1, pageSize: 8, sort: 'new' })
  }

  return {
    items,
    totalCount,
    loading,
    featured,
    hasFeatured,
    fetch,
    fetchFeatured,
  }
})