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
            <h1 class="mt-4 text-3xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-5xl rtl-text">
              {{ categoryLabel }}
            </h1>
            <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">
              {{ categoryDescription }}
            </p>
          </div>
        </div>
      </section>

      <section v-if="loadingChildren" class="mt-6 card-soft p-8 text-center text-sm text-[rgb(var(--muted))] rtl-text">
        {{ t('common.loading') }}
      </section>

      <section v-else-if="childSections.length" class="mt-6">
        <div class="card-soft p-5 sm:p-6">
          <div class="flex items-center justify-between gap-3">
            <div>
              <div class="text-xl font-extrabold text-[rgb(var(--text))] rtl-text">الأقسام الدقيقة</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">
                اختر القسم الأنسب للمشكلة حتى تظهر لك المنتجات المناسبة مباشرة.
              </div>
            </div>
          </div>

          <div class="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
            <NuxtLink
              v-for="child in childSections"
              :key="child.id || child.key"
              :to="buildDetailRoute(child)"
              class="group overflow-hidden rounded-[1.75rem] border border-app bg-surface transition hover:-translate-y-1 hover:shadow-soft"
            >
              <div class="aspect-square overflow-hidden bg-surface-2">
                <img
                  v-if="child.imageUrl"
                  :src="buildAssetUrl(child.imageUrl)"
                  :alt="child.nameAr"
                  class="h-full w-full object-cover transition duration-500 group-hover:scale-105"
                />
                <div
                  v-else
                  class="flex h-full w-full items-center justify-center text-4xl font-black text-[rgb(var(--text))]"
                >
                  {{ child.nameAr?.slice(0, 1) }}
                </div>
              </div>

              <div class="p-4 text-center">
                <div class="text-base font-extrabold text-[rgb(var(--text))] rtl-text">
                  {{ child.nameAr }}
                </div>
                <div class="mt-1 text-xs text-[rgb(var(--muted))] rtl-text">
                  {{ child.descriptionAr || 'عرض المنتجات المناسبة لهذا القسم' }}
                </div>
              </div>
            </NuxtLink>
          </div>
        </div>
      </section>

      <ProductListingWithSidebar
        v-else
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
    </template>

    <NuxtPage v-else :page-key="route.fullPath" />
  </div>
</template>

<script setup lang="ts">
import ProductListingWithSidebar from '~/components/ProductListingWithSidebar.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { problemCategories, fetchCategories, fetchProblemChildren } = useCategories()
const products = useProductsStore()
const { buildAssetUrl } = useApi()

const categoryKey = computed(() => String(route.params.category || '').toLowerCase())
const hasActiveDetailRoute = computed(() => typeof route.params.detail === 'string' && String(route.params.detail).length > 0)
const loadingChildren = ref(true)
const childSections = ref<any[]>([])

function buildDetailRoute(child: any) {
  const childKey = String(child?.key || '').toLowerCase()
  return `/problems/${encodeURIComponent(categoryKey.value)}/${encodeURIComponent(childKey)}`
}

await useAsyncData(
  () => `problem-root:${categoryKey.value}`,
  async () => {
    await fetchCategories(false, 'problem')
    return true
  },
  { watch: [categoryKey] }
)

const categoryItem = computed(
  () => (problemCategories.value || []).find((c: any) => String(c.key || '').toLowerCase() === categoryKey.value) || null
)

const categoryLabel = computed(() => categoryItem.value?.nameAr || categoryKey.value)
const categoryDescription = computed(
  () => categoryItem.value?.descriptionAr || 'اختر القسم المناسب لتظهر لك الحلول الدقيقة الخاصة بهذه المشكلة.'
)

const sort = ref(String(route.query.sort || 'new'))
const displayItems = computed(() => {
  const list = [...(products.items || [])]
  if (sort.value === 'priceAsc') return list.sort((a,b) => Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0) - Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0))
  if (sort.value === 'priceDesc') return list.sort((a,b) => Number(b.finalPriceIqd ?? b.priceIqd ?? b.price ?? 0) - Number(a.finalPriceIqd ?? a.priceIqd ?? a.price ?? 0))
  if (sort.value === 'alpha') return list.sort((a,b) => String(a.name || a.title || '').localeCompare(String(b.name || b.title || ''), 'ar'))
  if (sort.value === 'oldest') return list.reverse()
  return list
})
const activeSortLabel = computed(() => sort.value === 'priceAsc' ? t('productsPage.sortPriceAsc') : sort.value === 'priceDesc' ? t('productsPage.sortPriceDesc') : sort.value === 'alpha' ? 'حسب الأبجدية' : sort.value === 'oldest' ? 'الأقدم' : t('productsPage.sortNewest'))
function onSortChange(value: string) {
  sort.value = value
  router.replace({ query: { ...route.query, ...(value && value !== 'new' ? { sort: value } : {}) } })
}
function resetFilters() { onSortChange('new') }
watch(() => route.query.sort, (v) => { sort.value = String(v || 'new') })

async function loadChildren() {
  if (hasActiveDetailRoute.value) return

  loadingChildren.value = true

  try {
    const parentId = String(categoryItem.value?.id || '')
    childSections.value = parentId ? await fetchProblemChildren(parentId) : []

    if (!childSections.value.length) {
      await products.fetch({
        page: 1,
        pageSize: 24,
        sort: 'new',
        problemCategory: categoryKey.value
      })
    }
  } finally {
    loadingChildren.value = false
  }
}

watch(categoryItem, () => {
  loadChildren()
}, { immediate: true })
</script>