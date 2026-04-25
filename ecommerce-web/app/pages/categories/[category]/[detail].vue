<template>
  <div class="container mx-auto px-4 py-8 sm:py-10">
    <div class="card-soft overflow-hidden p-6 sm:p-8">
      <div class="flex flex-wrap items-center gap-2 text-sm text-[rgb(var(--muted))]">
        <NuxtLink :to="parentRoute" class="hover:text-[rgb(var(--text))]">{{ categoryLabel }}</NuxtLink>
        <span>/</span>
        <span class="text-[rgb(var(--text))]">{{ detailLabel }}</span>
      </div>
      <h1 class="mt-4 text-3xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-5xl rtl-text">
        {{ detailLabel }}
      </h1>
      <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">
        منتجات هذا التصنيف الدقيق فقط.
      </p>
    </div>

    <ProductResultsSection
      :items="sortedItems"
      :loading="products.loading"
      :sort="sort"
      :count="sortedItems.length"
      title="الفلاتر"
      hint="رتّب المنتجات حسب الوقت أو الاسم أو السعر."
      sort-label="الترتيب"
      clear-label="إعادة"
      results-title="المنتجات المناسبة"
      count-label="عدد المنتجات"
      empty-text="لا توجد منتجات داخل هذا التصنيف الدقيق حالياً. تأكد من ربط المنتج بنفس التصنيف الدقيق من لوحة التحكم."
      @update:sort="onSortChange"
      @reset="resetSort"
    />
  </div>
</template>

<script setup lang="ts">
import ProductResultsSection from '~/components/ProductResultsSection.vue'

const { categories, fetchCategories, fetchProblemChildren } = useCategories()
const route = useRoute()
const products = useProductsStore()

const categoryKey = computed(() => String(route.params.category || '').trim().toLowerCase())
const detailKey = computed(() => String(route.params.detail || '').trim().toLowerCase())
const parentRoute = computed(() => `/categories/${encodeURIComponent(categoryKey.value)}`)
const sort = ref(String(route.query.sort || 'new'))
const detailLabel = ref(detailKey.value)
const detailAliases = ref<string[]>([])

function norm(v: unknown) {
  return String(v ?? '').trim().toLowerCase()
}

function uniqueAliases(values: unknown[]) {
  return Array.from(new Set(values.map(norm).filter(Boolean)))
}

await useAsyncData(
  () => `category-detail-exact:${categoryKey.value}:${detailKey.value}`,
  async () => {
    await fetchCategories(false, 'regular')

    const parent = (categories.value || []).find((c: any) => norm(c.key) === categoryKey.value)
    const parentAliases = uniqueAliases([parent?.key, parent?.nameAr, parent?.nameEn, categoryKey.value])

    if (parent?.id) {
      const children = await fetchProblemChildren(String(parent.id), 'regular')
      const child = (children || []).find((x: any) => {
        const aliases = uniqueAliases([x?.key, x?.nameAr, x?.nameEn])
        return aliases.includes(detailKey.value)
      })

      detailLabel.value = child?.nameAr || child?.nameEn || child?.key || detailKey.value
      detailAliases.value = uniqueAliases([child?.key, child?.nameAr, child?.nameEn, detailKey.value])
    } else {
      detailAliases.value = uniqueAliases([detailKey.value])
    }

    // نجلب منتجات التصنيف الرئيسي فقط، ثم نفلتر محلياً بشكل صارم على التصنيف الدقيق.
    await products.fetch({ page: 1, pageSize: 60, sort: 'new', category: categoryKey.value })

    if (parentAliases.length) {
      products.items = (products.items || []).filter((p: any) => parentAliases.includes(norm(p?.category ?? p?.Category)))
      products.totalCount = products.items.length
    }

    return true
  },
  { watch: [categoryKey, detailKey] }
)

const categoryLabel = computed(() => {
  const c = (categories.value || []).find((x: any) => norm(x.key) === categoryKey.value)
  return c?.nameAr || c?.nameEn || categoryKey.value
})

const exactItems = computed(() => {
  const aliases = new Set(detailAliases.value.map(norm).filter(Boolean))
  if (!aliases.size) return []

  return (products.items || []).filter((p: any) => {
    const productSub = norm(p?.subCategory ?? p?.SubCategory)
    return productSub.length > 0 && aliases.has(productSub)
  })
})

const sortedItems = computed(() => sortProducts(exactItems.value, sort.value))

function onSortChange(value: string) {
  sort.value = value
}

function resetSort() {
  sort.value = 'new'
}

function sortProducts(items: any[], mode: string) {
  const list = [...items]
  switch (mode) {
    case 'oldest':
      return list.sort((a, b) => new Date(a.createdAt || a.created_at || 0).getTime() - new Date(b.createdAt || b.created_at || 0).getTime())
    case 'alphabetical':
      return list.sort((a, b) => String(a.name || a.title || '').localeCompare(String(b.name || b.title || ''), 'ar'))
    case 'priceAsc':
      return list.sort((a, b) => Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0) - Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0))
    case 'priceDesc':
      return list.sort((a, b) => Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0) - Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0))
    default:
      return list.sort((a, b) => new Date(b.createdAt || b.created_at || 0).getTime() - new Date(a.createdAt || a.created_at || 0).getTime())
  }
}
</script>
