import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { useAuthStore } from './auth'

export const useFavoritesStore = defineStore('favorites', () => {
  const auth = useAuthStore()
  const api = useApi()

  const items = ref<any[]>([])
  const loading = ref(false)

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
      items.value = Array.isArray(list) ? list : (list?.items ?? [])
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
        // Optionally push lightweight record
        if (!ids.value.has(productId)) items.value = [{ id: productId }, ...items.value]
      } else {
        items.value = items.value.filter((p: any) => String(p?.id ?? p?.Id ?? '') !== productId)
      }
    } else if (Array.isArray(res)) {
      items.value = res
    } else {
      // safest: reload
      await load()
    }

    return res
  }

  function isFavorite(productId: string) {
    return ids.value.has(productId)
  }

  return { items, loading, count, load, toggle, isFavorite }
})
