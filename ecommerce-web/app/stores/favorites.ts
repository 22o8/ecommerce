import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { useAuthStore } from './auth'

export const useFavoritesStore = defineStore('favorites', () => {
  const auth = useAuthStore()
  const api = useApi()

  const items = ref<any[]>([])
  const loading = ref(false)

  function normalizeFavoriteItem(raw: any) {
    const p = raw?.product ?? raw
    const id = String(p?.id ?? p?.Id ?? raw?.productId ?? raw?.ProductId ?? '')
    const title = p?.title ?? p?.Title ?? p?.name ?? ''
    const priceIqd = Number(p?.priceIqd ?? p?.PriceIqd ?? p?.price ?? 0)
    const discountPercent = Number(p?.discountPercent ?? p?.DiscountPercent ?? 0)
    const finalPriceIqd = Number(p?.finalPriceIqd ?? p?.FinalPriceIqd ?? (discountPercent > 0 ? (priceIqd * (100 - discountPercent) / 100) : priceIqd))
    const rawImage = p?.images?.[0]?.url || p?.images?.[0] || p?.coverImage || p?.imageUrl || p?.ImageUrl || p?.image || ''
    const imageUrl = rawImage ? api.buildAssetUrl(String(rawImage)) : ''
    const images = Array.isArray(p?.images)
      ? p.images.map((im: any) => {
          const u = typeof im === 'string' ? im : (im?.url || im?.path || im?.src || im?.imageUrl)
          return u ? api.buildAssetUrl(String(u)) : ''
        }).filter(Boolean)
      : (imageUrl ? [imageUrl] : [])

    return {
      ...p,
      id,
      title,
      name: title,
      slug: p?.slug ?? p?.Slug ?? '',
      description: p?.description ?? p?.Description ?? '',
      brand: p?.brand ?? p?.Brand ?? '',
      priceIqd,
      price: priceIqd,
      discountPercent,
      finalPriceIqd,
      imageUrl,
      coverImage: imageUrl || rawImage || '',
      images,
    }
  }

  const ids = computed(() => new Set(items.value.map((p: any) => String(p?.id ?? p?.Id ?? '')).filter(Boolean)))
  const count = computed(() => items.value.length)

  async function load() {
    if (!auth.token) {
      items.value = []
      return
    }

    loading.value = true
    try {
      // Backend: GET /api/Favorites/my
      const list: any = await api.get('/Favorites/my')
      items.value = (Array.isArray(list) ? list : (list?.items ?? [])).map(normalizeFavoriteItem)
    } finally {
      loading.value = false
    }
  }

  async function toggle(productId: string) {
    if (!auth.token) throw new Error('Unauthorized')

    // Backend: POST /api/Favorites/toggle/{productId}
    const res: any = await api.post(`/Favorites/toggle/${productId}`)

    // We accept either { isFavorite: true/false } OR the full product list OR nothing.
    if (typeof res?.isFavorited === 'boolean') {
      if (res.isFavorited) {
        await load()
      } else {
        items.value = items.value.filter((p: any) => String(p?.id ?? p?.Id ?? '') !== productId)
      }
    } else if (Array.isArray(res)) {
      items.value = res.map(normalizeFavoriteItem)
    } else {
      await load()
    }

    return res
  }

  function isFavorite(productId: string) {
    return ids.value.has(productId)
  }

  return { items, loading, count, load, toggle, isFavorite }
})
