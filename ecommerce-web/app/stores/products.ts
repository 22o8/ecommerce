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

  function normalizeProduct(p: any) {
    if (!p) return p
    const name = p.name ?? p.title ?? p.Title ?? p.productName ?? ''
    const priceIqd = p.priceIqd ?? p.PriceIqd ?? p.price ?? p.Price ?? 0
    const discountPercent = Number(p.discountPercent ?? p.DiscountPercent ?? 0)
    const finalPriceIqd = Number(p.finalPriceIqd ?? p.FinalPriceIqd ?? (discountPercent > 0 ? (priceIqd * (100 - discountPercent) / 100) : priceIqd))
    const priceUsd = p.priceUsd ?? p.PriceUsd ?? 0
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

    return { ...p, name, priceIqd, priceUsd, price: priceIqd, discountPercent, finalPriceIqd, imageUrl, slug, images: normImages, description }
  }

  const items = ref<any[]>([])
  const featuredItems = ref<any[]>([])
  const discountItems = ref<any[]>([])
  const totalCount = ref(0)
  const loading = ref(false)

  const featuredCache = ref<Record<string, any[]>>({})
  const pageCache = ref<Record<string, { items: any[]; totalCount: number }>>({})
  const discountsCache = ref<Record<number, any[]>>({})
  const liveCache = ref<Record<string, any[]>>({})

  const featured = computed(() => featuredItems.value.length ? featuredItems.value : items.value.slice(0, 8))
  const hasFeatured = computed(() => featured.value.length > 0)

  function pageKey(params: FetchParams = {}) {
    return JSON.stringify({
      page: params.page || 1,
      pageSize: params.pageSize || 12,
      q: params.q || '',
      sort: params.sort || 'new',
      brand: params.brand || '',
      isFeatured: !!params.isFeatured,
    })
  }

  async function fetch(params: FetchParams = {}) {
    const isFeat = Boolean(params.isFeatured)
    const key = pageKey(params)

    if (!isFeat && pageCache.value[key]) {
      items.value = pageCache.value[key].items
      totalCount.value = pageCache.value[key].totalCount
      return { items: items.value, totalCount: totalCount.value }
    }

    loading.value = true
    try {
      const path = isFeat ? '/Products/featured' : '/Products'
      const res = await api.get<Paged<any>>(
        path,
        isFeat
          ? { take: params.pageSize || 8 }
          : {
              page: params.page || 1,
              pageSize: params.pageSize || 12,
              q: params.q || undefined,
              sort: params.sort || 'new',
              brand: params.brand || undefined,
            }
      )

      const raw = (res as any)?.items ?? (res as any)?.data?.items ?? (res as any)?.data
      const arr = Array.isArray(raw) ? raw : (Array.isArray(res as any) ? (res as any) : [])
      const normalized = arr.map(normalizeProduct)

      if (isFeat) {
        featuredItems.value = normalized
        featuredCache.value[String(params.pageSize || 8)] = normalized
      } else {
        items.value = normalized
        totalCount.value = Number((res as any)?.totalCount ?? (res as any)?.data?.totalCount ?? (res as any)?.total ?? items.value.length ?? 0)
        pageCache.value[key] = { items: normalized, totalCount: totalCount.value }
      }

      return res
    } finally {
      loading.value = false
    }
  }

  async function fetchFeatured(take = 8) {
    if (featuredCache.value[String(take)]?.length) {
      featuredItems.value = featuredCache.value[String(take)]
      return
    }
    try {
      const res = await api.get<{ totalCount?: number; items?: any[] }>('/Products/featured', { take })
      const list = (res?.items ?? []).map(normalizeProduct)
      if (list.length) {
        featuredItems.value = list
        featuredCache.value[String(take)] = list
        return
      }
    } catch {}

    await fetch({ page: 1, pageSize: take, sort: 'new' })
    featuredItems.value = items.value.slice(0, take)
    featuredCache.value[String(take)] = featuredItems.value
  }

  async function fetchDiscounts(take = 24) {
    if (discountsCache.value[take]?.length) {
      discountItems.value = discountsCache.value[take]
      return { items: discountItems.value }
    }
    const res = await api.get<{ totalCount?: number; items?: any[] }>('/Products/discounts', { take })
    discountItems.value = (res?.items ?? []).map(normalizeProduct)
    discountsCache.value[take] = discountItems.value
    return res
  }

  async function liveSearch(q: string, limit = 8) {
    const key = `${q}::${limit}`.trim().toLowerCase()
    if (liveCache.value[key]) return liveCache.value[key]
    const res = await api.get<any[]>('/Products/search', { q, limit })
    const list = (res ?? []).map(normalizeProduct)
    liveCache.value[key] = list
    return list
  }

  return {
    items, totalCount, loading, featured, hasFeatured, fetch, fetchFeatured, fetchDiscounts, liveSearch, discountItems
  }
})
