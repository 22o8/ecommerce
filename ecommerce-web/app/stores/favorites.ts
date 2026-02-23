import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import { useApi } from '~/composables/useApi'
import { useAuthStore } from '~/stores/auth'

export const useFavoritesStore = defineStore('favorites', () => {
  const api = useApi()
  const auth = useAuthStore()

  const ids = ref<Set<string>>(new Set())
  const items = ref<any[]>([])
  const loading = ref(false)
  const ready = ref(false)

  const count = computed(() => ids.value.size)

  function has(productId: string) {
    return ids.value.has(String(productId))
  }

  async function refresh() {
    if (!auth.isAuthed) {
      // للمستخدم غير المسجل: نخليها فاضية (حتى ما يصير تضارب مع شرط "تنحفظ بالسيرفر")
      ids.value = new Set()
      items.value = []
      ready.value = true
      return
    }

    loading.value = true
    try {
      const list: any[] = await api.get('/favorites')
      items.value = Array.isArray(list) ? list : []
      ids.value = new Set(items.value.map(x => String(x.id)))
      ready.value = true
    } finally {
      loading.value = false
    }
  }

  async function toggle(productId: string) {
    const id = String(productId || '')
    if (!id) return

    if (!auth.isAuthed) {
      // شرط المشروع: المفضلة مرتبطة بالحساب
      // نخلي UX واضح: ودّه لتسجيل الدخول
      navigateTo({ path: '/login', query: { r: '/favorites' } })
      return
    }

    const res: any = await api.post(`/favorites/toggle/${id}`)
    const isFav = Boolean(res?.isFavorite)

    if (isFav) ids.value.add(id)
    else ids.value.delete(id)

    // حتى تبقى الصفحة والعداد متزامنة
    await refresh()
  }

  // إذا المستخدم سوّى login/logout
  watch(
    () => auth.isAuthed,
    () => refresh(),
    { immediate: true }
  )

  return { ids, items, count, loading, ready, has, refresh, toggle }
})
