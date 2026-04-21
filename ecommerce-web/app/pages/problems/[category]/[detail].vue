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

    <ProductResultsSection
      :items="sortedItems"
      :loading="products.loading"
      :sort="sort"
      :count="sortedItems.length"
      title="الفلاتر"
      hint="رتّب النتائج حسب الوقت أو الاسم أو السعر."
      sort-label="الترتيب"
      clear-label="إعادة"
      results-title="المنتجات المناسبة"
      count-label="عدد المنتجات"
      :empty-text="t('productsPage.emptyDesc')"
      @update:sort="onSortChange"
      @reset="resetSort"
    />
  </div>
</template>

<script setup lang="ts">
import ProductResultsSection from '~/components/ProductResultsSection.vue'

const { t } = useI18n()
const route = useRoute()
const { problemCategories, fetchCategories, fetchProblemChildren } = useCategories()
const products = useProductsStore()

const categoryKey = computed(() => String(route.params.category || '').toLowerCase())
const detailKey = computed(() => String(route.params.detail || '').toLowerCase())
const childSections = ref<any[]>([])
const parentRoute = computed(() => `/problems/${encodeURIComponent(categoryKey.value)}`)
const sort = ref(String(route.query.sort || 'new'))

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
const sortedItems = computed(() => sortProducts(products.items || [], sort.value))

function onSortChange(value: string) { sort.value = value }
function resetSort() { sort.value = 'new' }
function sortProducts(items: any[], mode: string) {
  const list = [...items]
  switch (mode) {
    case 'oldest': return list.sort((a, b) => new Date(a.createdAt || a.created_at || 0).getTime() - new Date(b.createdAt || b.created_at || 0).getTime())
    case 'alphabetical': return list.sort((a, b) => String(a.name || a.title || '').localeCompare(String(b.name || b.title || ''), 'ar'))
    case 'priceAsc': return list.sort((a, b) => Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0) - Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0))
    case 'priceDesc': return list.sort((a, b) => Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0) - Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0))
    default: return list.sort((a, b) => new Date(b.createdAt || b.created_at || 0).getTime() - new Date(a.createdAt || a.created_at || 0).getTime())
  }
}
</script>
