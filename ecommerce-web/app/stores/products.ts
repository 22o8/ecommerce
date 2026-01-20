// app/stores/products.ts
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'

// ستور خفيفة فقط للصفحة الرئيسية (Featured)
// الهدف: تجنّب تكرار المنطق داخل index.vue مع الحفاظ على بنية المشروع.

type Paged<T> = {
  items: T[]
  totalCount: number
}

export const useProductsStore = defineStore('products', () => {
  const api = useApi()

  const featured = ref<any[]>([])
  const loadingFeatured = ref(false)
  const featuredError = ref<string | null>(null)
  const hasFeatured = computed(() => featured.value.length > 0)

  async function fetchFeatured() {
    loadingFeatured.value = true
    featuredError.value = null
    try {
      // نفس شكل صفحة المنتجات
      const res = await api.get<Paged<any>>('/Products', {
        page: 1,
        pageSize: 8,
        sort: 'new',
      })
      featured.value = Array.isArray(res?.items) ? res.items : []
    } catch (e) {
      // لا نخلي المنتجات "تظهر ثواني وتختفي" إذا صار خطأ لحظي
      // نخلي آخر بيانات موجودة ونعرض رسالة (إذا تحتاجها بالواجهة)
      featuredError.value = (e as any)?.message || 'Failed to load products'
    } finally {
      loadingFeatured.value = false
    }
  }

  return {
    featured,
    loadingFeatured,
    featuredError,
    hasFeatured,
    fetchFeatured,
  }
})
