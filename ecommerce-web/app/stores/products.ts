// app/stores/products.ts
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'

type Paged<T> = {
  items?: T[]
  totalCount?: number
  total?: number
}

type FetchParams = {
  page?: number
  pageSize?: number
  q?: string
  sort?: 'new' | 'priceAsc' | 'priceDesc' | string
}

function normalizeImages(p: any): Array<{ url: string; id?: string }> {
  const raw = p?.images ?? p?.gallery ?? p?.imageUrls ?? p?.imageUrl ?? p?.image ?? []
  const arr = Array.isArray(raw) ? raw : (raw ? [raw] : [])
  return arr
    .map((x: any) => {
      if (!x) return null
      if (typeof x === 'string') return { url: x }
      const url = x.url || x.path || x.fileUrl || x.imageUrl || x.src || ''
      const id = x.id ?? (url || undefined)
      return url ? { url, id } : null
    })
    .filter(Boolean) as any
}

function normalizeProduct(p: any) {
  const price = Number(
    typeof p?.price === 'number'
      ? p.price
      : (p?.priceUsd ?? p?.priceUSD ?? p?.price_usd ?? p?.price_iqd ?? p?.priceIQD ?? p?.price ?? 0)
  )

  // نحافظ على name لأن أغلب الكومبوننتات تعتمد عليه
  const name = p?.name ?? p?.title ?? ''
  const slug = p?.slug ?? p?.id ?? ''
  const description = p?.description ?? ''

  return {
    ...p,
    id: p?.id,
    name,
    title: p?.title ?? name,
    slug,
    description,
    price,
    images: normalizeImages(p),
  }
}

export const useProductsStore = defineStore('products', () => {
  const api = useApi()

  const items = ref<any[]>([])
  const totalCount = ref(0)
  const loading = ref(false)

  const featured = computed(() => items.value.slice(0, 8))
  const hasFeatured = computed(() => featured.value.length > 0)

  async function fetch(params: FetchParams = {}) {
    loading.value = true
    try {
      const res = await api.get<Paged<any> | any[]>('/Products', {
        page: params.page || 1,
        pageSize: params.pageSize || 12,
        q: params.q || undefined,
        sort: params.sort || 'new',
      })

      const rawItems =
        Array.isArray((res as any)?.items) ? (res as any).items :
        Array.isArray(res) ? res :
        []

      items.value = rawItems.map(normalizeProduct)
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
