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
  isFeatured?: boolean
  brand?: string
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

  const imageUrl = cover ? api.buildAssetUrl(String(cover)) : ''
  const normImages = Array.isArray(images)
    ? images
        .map((im: any) => {
          const u = typeof im === 'string' ? im : (im?.url || im?.path || im?.src || im?.imageUrl)
          return u ? api.buildAssetUrl(String(u)) : ''
        })
        .filter(Boolean)
    : []

  return { ...p, name, priceUsd, imageUrl, slug, images: normImages, description }
}


  const items = ref<any[]>([])
  // ✅ قائمة المنتجات المميّزة (تُعرض بالصفحة الرئيسية)
  // مفصوله عن items حتى ما تتداخل مع صفحات المنتجات/الفلاتر.
  const featuredItems = ref<any[]>([])
  const totalCount = ref(0)
  const loading = ref(false)

  // Featured is simply first few items from latest fetch
  // Featured products shown on home page.
  // Prefer server-filtered featured list when available.
  const featured = computed(() => {
    const list = featuredItems.value
    if (Array.isArray(list) && list.length) return list
    return items.value.slice(0, 8)
  })
  const hasFeatured = computed(() => featured.value.length > 0)

  async function fetch(params: FetchParams = {}) {
    loading.value = true
    try {
			// NOTE: useApi.get expects query object directly (not { query: {...} }).
			// Featured is served from a dedicated endpoint.
			const path = params.isFeatured ? '/Products/featured' : '/Products'
			const res = await api.get<Paged<any>>(path, {
				page: params.page || 1,
				pageSize: params.pageSize || (params.isFeatured ? 8 : 12),
				q: params.q || undefined,
				sort: params.sort || 'new',
				brand: params.brand || undefined,
			})

      // Support different API shapes
      const raw = (res as any)?.items ?? (res as any)?.data?.items ?? (res as any)?.data
      const arr = Array.isArray(raw)
        ? raw
        : (Array.isArray(res as any) ? (res as any) : [])
      const normalized = arr.map(normalizeProduct)

      // إذا كانت جلبة المميّزات، خزّنها بقائمة منفصلة.
      if (params.isFeatured) {
        featuredItems.value = normalized
      } else {
        items.value = normalized
        totalCount.value = Number(
          (res as any)?.totalCount ?? (res as any)?.data?.totalCount ?? (res as any)?.total ?? items.value.length ?? 0
        )
      }

      return res
    } finally {
      loading.value = false
    }
  }

  async function fetchFeatured() {
    return await fetch({ page: 1, pageSize: 8, sort: 'new', isFeatured: true })
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