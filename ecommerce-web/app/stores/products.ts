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

  const items = ref<any[]>([])
  const totalCount = ref(0)
  const loading = ref(false)
function normalizeProduct(p: any): Product {
  const price = typeof p?.price === 'number' ? p.price : (typeof p?.priceUsd === 'number' ? p.priceUsd : Number(p?.price ?? p?.priceUsd ?? 0))
  const imagesRaw = p?.images ?? p?.gallery ?? p?.imageUrls ?? p?.imageUrl ?? []
  let images: any[] = []
  if (Array.isArray(imagesRaw)) {
    images = imagesRaw.map((x: any) => (typeof x === 'string' ? { url: x } : (x?.url ? x : { url: x?.path || x?.src || '' }))).filter((x: any)=>x?.url)
  } else if (typeof imagesRaw === 'string') {
    images = [{ url: imagesRaw }]
  }
  return {
    id: p.id,
    title: p.title ?? p.name ?? '',
    slug: p.slug ?? '',
    description: p.description ?? '',
    price,
    isPublished: Boolean(p.isPublished ?? p.published ?? true),
    createdAt: p.createdAt ?? p.created_at ?? '',
    images,
  } as any
}


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

      items.value = Array.isArray((res as any)?.items) ? (res as any).items : (Array.isArray(res as any) ? (res as any) : [])
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
