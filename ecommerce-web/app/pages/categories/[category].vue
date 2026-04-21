<template>
  <div class="container mx-auto px-4 py-8 sm:py-10">
    <template v-if="!hasActiveDetailRoute">
      <section class="card-soft overflow-hidden p-6 sm:p-8">
        <div class="grid gap-6 lg:grid-cols-[1.2fr_.8fr] lg:items-end">
          <div>
            <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-1 text-xs font-bold text-[rgb(var(--muted))]">
              <span class="h-2 w-2 rounded-full bg-[rgb(var(--primary))]" />
              {{ categoryLabel }}
            </div>
            <h1 class="mt-4 text-3xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-5xl rtl-text">{{ categoryLabel }}</h1>
            <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">{{ categoryDescription }}</p>
          </div>
        </div>
      </section>

      <section v-if="loadingChildren" class="mt-6 card-soft p-8 text-center text-sm text-[rgb(var(--muted))] rtl-text">{{ t('common.loading') }}</section>

      <section v-else-if="childSections.length" class="mt-6">
        <div class="card-soft p-5 sm:p-6">
          <div class="flex items-center justify-between gap-3">
            <div>
              <div class="text-xl font-extrabold text-[rgb(var(--text))] rtl-text">التصنيفات الدقيقة</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">اختر التصنيف المناسب ليتم عرض المنتجات الخاصة به مباشرة.</div>
            </div>
          </div>
          <div class="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
            <NuxtLink v-for="child in childSections" :key="child.id || child.key" :to="buildDetailRoute(child)" class="group overflow-hidden rounded-[1.75rem] border border-app bg-surface transition hover:-translate-y-1 hover:shadow-soft">
              <div class="aspect-square overflow-hidden bg-surface-2">
                <img v-if="child.imageUrl" :src="buildAssetUrl(child.imageUrl)" :alt="child.nameAr" class="h-full w-full object-cover transition duration-500 group-hover:scale-105" />
                <div v-else class="flex h-full w-full items-center justify-center text-4xl font-black text-[rgb(var(--text))]">{{ child.nameAr?.slice(0, 1) }}</div>
              </div>
              <div class="p-4 text-center">
                <div class="text-base font-extrabold text-[rgb(var(--text))] rtl-text">{{ child.nameAr }}</div>
                <div class="mt-1 text-xs text-[rgb(var(--muted))] rtl-text">{{ child.descriptionAr || 'عرض المنتجات المناسبة لهذا التصنيف' }}</div>
              </div>
            </NuxtLink>
          </div>
        </div>
      </section>

      <ProductResultsSection v-else :items="sortedItems" :loading="products.loading" :sort="sort" :count="sortedItems.length" title="الفلاتر" hint="رتّب المنتجات حسب الوقت أو الاسم أو السعر." sort-label="الترتيب" clear-label="إعادة" results-title="المنتجات المناسبة" count-label="عدد المنتجات" :empty-text="t('productsPage.emptyDesc')" @update:sort="onSortChange" @reset="resetSort" />
    </template>
    <NuxtPage v-else :page-key="route.fullPath" />
  </div>
</template>

<script setup lang="ts">
import ProductResultsSection from '~/components/ProductResultsSection.vue'
const { t } = useI18n()
const route = useRoute()
const { categories, fetchCategories, fetchProblemChildren } = useCategories()
const products = useProductsStore()
const { buildAssetUrl } = useApi()
const categoryKey = computed(() => String(route.params.category || '').toLowerCase())
const hasActiveDetailRoute = computed(() => typeof route.params.detail === 'string' && String(route.params.detail).length > 0)
const loadingChildren = ref(true)
const childSections = ref<any[]>([])
const sort = ref(String(route.query.sort || 'new'))
function buildDetailRoute(child: any) {
  const childKey = String(child?.key || '').toLowerCase()
  return `/categories/${encodeURIComponent(categoryKey.value)}/${encodeURIComponent(childKey)}`
}
await useAsyncData(() => `category-root:${categoryKey.value}`, async () => { await fetchCategories(false, 'regular'); return true }, { watch: [categoryKey] })
const categoryItem = computed(() => (categories.value || []).find((c: any) => String(c.key || '').toLowerCase() === categoryKey.value) || null)
const categoryLabel = computed(() => categoryItem.value?.nameAr || categoryKey.value)
const categoryDescription = computed(() => categoryItem.value?.descriptionAr || 'اختر القسم المناسب لتظهر لك المنتجات الخاصة بهذا التصنيف.')
const sortedItems = computed(() => sortProducts(products.items || [], sort.value))
async function loadChildren() {
  if (hasActiveDetailRoute.value) return
  loadingChildren.value = true
  try {
    const parentId = String(categoryItem.value?.id || '')
    childSections.value = categoryItem.value?.hasDetailSections && parentId ? await fetchProblemChildren(parentId, 'regular') : []
    if (!childSections.value.length) {
      await products.fetch({ page: 1, pageSize: 24, sort: 'new', category: categoryKey.value })
    }
  } finally { loadingChildren.value = false }
}
watch(categoryItem, () => { loadChildren() }, { immediate: true })
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