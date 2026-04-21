<template>
  <div class="container mx-auto px-4 py-8 sm:py-10">
    <section class="card-soft overflow-hidden p-6 sm:p-8">
      <div class="flex flex-wrap items-center gap-2 text-sm text-[rgb(var(--muted))]">
        <NuxtLink :to="parentRoute" class="hover:text-[rgb(var(--text))]">{{ categoryLabel }}</NuxtLink>
        <span>/</span>
        <span class="text-[rgb(var(--text))]">{{ detailLabel }}</span>
      </div>
      <h1 class="mt-4 text-3xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-5xl rtl-text">{{ detailLabel }}</h1>
      <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">{{ detailDescription }}</p>
    </section>

    <ProductListingWithSidebar
      :items="displayItems"
      :loading="products.loading"
      :count="displayItems.length"
      :sort="sort"
      :sort-label="activeSortLabel"
      heading="المنتجات المناسبة"
      :subheading="t('productsPage.resultsCount', { count: displayItems.length })"
      @update:sort="onSortChange"
      @reset="resetFilters"
    />
  </div>
</template>

<script setup lang="ts">
import ProductListingWithSidebar from '~/components/ProductListingWithSidebar.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { problemCategories, fetchCategories, fetchProblemChildren } = useCategories()
const products = useProductsStore()

const categoryKey = computed(() => String(route.params.category || '').toLowerCase())
const detailKey = computed(() => String(route.params.detail || '').toLowerCase())
const childSections = ref<any[]>([])
const parentRoute = computed(() => `/problems/${encodeURIComponent(categoryKey.value)}`)

await useAsyncData(`problem-detail:${categoryKey.value}:${detailKey.value}`, async () => {
  await fetchCategories(false, 'problem')
  const parent = (problemCategories.value || []).find((c: any) => String(c.key || '').toLowerCase() === categoryKey.value)
  if (parent?.id) childSections.value = await fetchProblemChildren(String(parent.id))
  await products.fetch({ page: 1, pageSize: 24, sort: 'new', problemCategory: categoryKey.value, problemSubCategory: detailKey.value })
  return true
}, { watch: [categoryKey, detailKey] })

const categoryLabel = computed(() => (problemCategories.value || []).find((c: any) => String(c.key || '').toLowerCase() === categoryKey.value)?.nameAr || categoryKey.value)
const detailItem = computed(() => (childSections.value || []).find((c: any) => String(c.key || '').toLowerCase() === detailKey.value) || null)
const detailLabel = computed(() => detailItem.value?.nameAr || detailKey.value)
const detailDescription = computed(() => detailItem.value?.descriptionAr || 'هذه المنتجات مرتبطة بهذا القسم الدقيق ضمن حلول المشكلة.')
const sort = ref(String(route.query.sort || 'new'))
const displayItems = computed(() => {
  const list = [...(products.items || [])]
  const key = detailKey.value
  const filtered = list.filter((item: any) => {
    const values = new Set([String(item.subCategory || '').toLowerCase(), String(item.problemSubCategory || '').toLowerCase()].filter(Boolean))
    return values.size ? values.has(key) : true
  })
  if (sort.value === 'priceAsc') return filtered.sort((a,b) => Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0) - Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0))
  if (sort.value === 'priceDesc') return filtered.sort((a,b) => Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0) - Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0))
  if (sort.value === 'alpha') return filtered.sort((a,b) => String(a.name || a.title || '').localeCompare(String(b.name || b.title || ''), 'ar'))
  if (sort.value === 'oldest') return [...filtered].reverse()
  return filtered
})
const activeSortLabel = computed(() => sort.value === 'priceAsc' ? t('productsPage.sortPriceAsc') : sort.value === 'priceDesc' ? t('productsPage.sortPriceDesc') : sort.value === 'alpha' ? 'حسب الأبجدية' : sort.value === 'oldest' ? 'الأقدم' : t('productsPage.sortNewest'))
function onSortChange(value: string) {
  sort.value = value
  router.replace({ query: { ...route.query, ...(value && value !== 'new' ? { sort: value } : {}) } })
}
function resetFilters() { onSortChange('new') }
watch(() => route.query.sort, (v) => { sort.value = String(v || 'new') })

</script>
