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
  // دعم أكبر عدد ممكن من الأسماء/الأشكال اللي ممكن يرجّعها الـ API
  const raw =
    p?.images ??
    p?.gallery ??
    p?.productImages ??
    p?.imageUrls ??
    p?.imagePaths ??
    p?.coverImageUrl ??
    p?.thumbnailUrl ??
    p?.mainImageUrl ??
    p?.imageUrl ??
    p?.image ??
    []

  // لو يرجع سترنغ مفصولة بفواصل
  const toArray = (v: any) => {
    if (!v) return []
    if (Array.isArray(v)) return v
    if (typeof v === 'string') {
      const s = v.trim()
      if (!s) return []
      return s.includes(',') ? s.split(',').map(x => x.trim()).filter(Boolean) : [s]
    }
    return [v]
  }

  const arr = toArray(raw)

  return arr
    .flatMap((x: any) => {
      if (!x) return []
      // بعض الـ APIs ترجع مصفوفة سترنغ، وبعضها ترجع مصفوفة Objects
      if (typeof x === 'string') {
        const s = x.trim()
        if (!s) return []
        // لو داخل عنصر واحد بيه فواصل
        const parts = s.includes(',') ? s.split(',').map(p => p.trim()).filter(Boolean) : [s]
        return parts.map((url) => ({ url, id: url }))
      }

      const url =
        x.url ||
        x.path ||
        x.fileUrl ||
        x.imageUrl ||
        x.src ||
        x.href ||
        x.location ||
        ''

      if (!url) return []
      return [{ url, id: x.id ?? url }]
    })
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
