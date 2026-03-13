<template>
  <div class="products-page container mx-auto px-4 py-8 sm:py-10">
    <section class="products-hero card-soft overflow-hidden p-6 sm:p-8">
      <div class="grid gap-6 lg:grid-cols-[1.25fr_.95fr] lg:items-end">
        <div>
          <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-1 text-xs font-bold text-[rgb(var(--muted))]">
            <span class="h-2 w-2 rounded-full bg-[rgb(var(--primary))]" />
            {{ t('productsPage.title') }}
          </div>
          <h1 class="mt-4 text-3xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-5xl rtl-text">
            {{ t('productsPage.title') }}
          </h1>
          <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">
            {{ t('productsPage.subtitle') }}
          </p>
        </div>

        <div class="products-summary grid gap-3 sm:grid-cols-3">
          <div class="summary-chip">
            <div class="summary-chip__label rtl-text">{{ t('productsPage.resultsLabel') }}</div>
            <div class="summary-chip__value keep-ltr">{{ products.totalCount || products.items.length || 0 }}</div>
          </div>
          <div class="summary-chip">
            <div class="summary-chip__label rtl-text">{{ t('productsPage.activeSortLabel') }}</div>
            <div class="summary-chip__value rtl-text">{{ activeSortLabel }}</div>
          </div>
          <div class="summary-chip">
            <div class="summary-chip__label rtl-text">{{ t('productsPage.brandFilterLabel') }}</div>
            <div class="summary-chip__value rtl-text truncate">{{ activeBrandLabel }}</div>
          </div>
        </div>
      </div>
    </section>

    <section class="mt-6 grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
      <aside class="products-filters card-soft p-4 sm:p-5 xl:sticky xl:top-24 h-fit">
        <div class="flex items-center justify-between gap-3">
          <div>
            <div class="text-lg font-extrabold text-[rgb(var(--text))] rtl-text">{{ t('productsPage.filtersTitle') }}</div>
            <div class="mt-1 text-xs text-[rgb(var(--muted))] rtl-text">{{ t('productsPage.filtersHint') }}</div>
          </div>
          <button
            type="button"
            class="btn-secondary px-3 py-2 text-sm"
            @click="clearFilters"
            :disabled="products.loading"
          >
            {{ t('common.clear') }}
          </button>
        </div>

        <div class="mt-4 grid gap-3">
          <div class="filter-field">
            <label class="filter-label rtl-text">{{ t('productsPage.sort') }}</label>
            <select v-model="sort" class="input products-select" @change="applyFilters" :aria-label="t('productsPage.sort')">
              <option value="new">{{ t('productsPage.sortNewest') }}</option>
              <option value="priceAsc">{{ t('productsPage.sortPriceAsc') }}</option>
              <option value="priceDesc">{{ t('productsPage.sortPriceDesc') }}</option>
            </select>
          </div>

          <div class="filter-field">
            <label class="filter-label rtl-text">{{ t('productsPage.chooseBrand') }}</label>
            <select v-model="brand" class="input products-select" @change="applyFilters" :aria-label="t('productsPage.chooseBrand')">
              <option value="">{{ t('productsPage.allBrands') }}</option>
              <option v-for="b in brandOptions" :key="b.slug" :value="b.slug">
                {{ b.name }}
              </option>
            </select>
          </div>

          <div class="filter-field">
            <label class="filter-label rtl-text">{{ t('productsPage.chooseCategory') }}</label>
            <select v-model="category" class="input products-select" @change="subCategory=''; applyFilters()" :aria-label="t('productsPage.chooseCategory')">
              <option value="">{{ t('productsPage.allCategories') }}</option>
              <option v-for="c in categoryOptions" :key="c.key" :value="c.key">
                {{ c.name }}
              </option>
            </select>
          </div>

          <div class="filter-field">
            <label class="filter-label rtl-text">{{ t('productsPage.chooseSubCategory') }}</label>
            <select v-model="subCategory" class="input products-select" @change="applyFilters" :aria-label="t('productsPage.chooseSubCategory')">
              <option value="">{{ t('productsPage.allSubCategories') }}</option>
              <option v-for="s in subCategoryOptions(category)" :key="s.key" :value="s.key">
                {{ s.name }}
              </option>
            </select>
          </div>

          <div class="results-pill rtl-text">
            {{ t('productsPage.resultsCount', { count: products.totalCount || products.items.length || 0 }) }}
          </div>
        </div>
      </aside>

      <div>
        <div class="products-toolbar card-soft p-4 sm:p-5">
          <div class="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div class="text-lg font-extrabold text-[rgb(var(--text))] rtl-text">{{ t('productsPage.resultsTitle') }}</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">{{ toolbarText }}</div>
            </div>

            <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-2 text-sm text-[rgb(var(--muted))]">
              <span class="h-2.5 w-2.5 rounded-full bg-[rgb(var(--primary))]" />
              <span class="rtl-text">{{ activeSortLabel }}</span>
            </div>
          </div>
        </div>

        <div v-if="products.loading && products.items.length === 0" class="mt-6">
          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
            <div v-for="n in 6" :key="n" class="skeleton-card products-skeleton" />
          </div>
          <div class="mt-6 flex items-center justify-center text-sm text-[rgb(var(--muted))] rtl-text">
            <span class="spinner" aria-hidden="true" />
            <span class="ms-2">{{ t('common.loading') }}</span>
          </div>
        </div>

        <div v-else class="mt-6 grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-3">
          <RevealOnScroll
            v-for="(p, idx) in products.items"
            :key="p.id"
            :parity="(idx % 2) as 0 | 1"
            :delay="35 * (idx % 6)"
          >
            <ProductCard :p="p" />
          </RevealOnScroll>
        </div>

        <div v-if="!products.loading && products.items.length === 0" class="mt-6 card-soft p-10 text-center text-[rgb(var(--muted))] rtl-text">
          <div class="text-lg font-extrabold text-[rgb(var(--text))]">{{ t('productsPage.emptyTitle') }}</div>
          <div class="mt-2 text-sm">{{ t('productsPage.emptyDesc') }}</div>
        </div>

        <div class="mt-8 card-soft p-4 sm:p-5">
          <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <button class="btn-secondary min-w-[112px] px-4 py-3" :disabled="page <= 1 || products.loading" @click="goPage(page - 1)">
              {{ t('productsPage.prev') }}
            </button>

            <div class="pagination-center text-center">
              <div class="text-xs uppercase tracking-[0.24em] text-[rgb(var(--muted))]">{{ t('productsPage.page') }}</div>
              <div class="mt-1 text-lg font-black text-[rgb(var(--text))] keep-ltr">{{ page }}</div>
            </div>

            <button class="btn-secondary min-w-[112px] px-4 py-3" :disabled="!hasNext || products.loading" @click="goPage(page + 1)">
              {{ t('productsPage.next') }}
            </button>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import ProductCard from '~/components/ProductCard.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

const brandsStore = useBrandsStore()
const products = useProductsStore()
const { categoryOptions, subCategoryOptions } = useProductTaxonomy()

const q = ref(String(route.query.q || ''))
const sort = ref(String(route.query.sort || 'new'))
const brand = ref(String(route.query.brand || ''))
const category = ref(String(route.query.category || ''))
const subCategory = ref(String(route.query.subCategory || ''))
const page = ref(Number(route.query.page || 1) || 1)

await useAsyncData('products_page_boot', async () => {
  await brandsStore.fetchPublic()
  await fetchProducts()
  return true
})

const brandOptions = computed(() => (brandsStore.publicItems || []).map((b: any) => ({ name: b.name, slug: b.slug })))

const hasNext = computed(() => {
  const total = Number(products.totalCount || 0)
  const pageSize = 12
  return page.value * pageSize < total
})

const activeSortLabel = computed(() => {
  if (sort.value === 'priceAsc') return t('productsPage.sortPriceAsc')
  if (sort.value === 'priceDesc') return t('productsPage.sortPriceDesc')
  return t('productsPage.sortNewest')
})

const activeBrandLabel = computed(() => {
  if (!brand.value) return t('productsPage.allBrands')
  const match = brandOptions.value.find((b: any) => b.slug === brand.value)
  return match?.name || t('productsPage.allBrands')
})

const activeCategoryLabel = computed(() => {
  if (!category.value) return t('productsPage.allCategories')
  const match = categoryOptions.value.find((c: any) => c.key === category.value)
  return match?.name || t('productsPage.allCategories')
})

const activeSubCategoryLabel = computed(() => {
  if (!subCategory.value) return t('productsPage.allSubCategories')
  const match = subCategoryOptions(category.value).find((s: any) => s.key === subCategory.value)
  return match?.name || t('productsPage.allSubCategories')
})

const toolbarText = computed(() => {
  const count = products.totalCount || products.items.length || 0
  if (subCategory.value) return t('productsPage.toolbarSubCategory', { count, subCategory: activeSubCategoryLabel.value })
  if (category.value) return t('productsPage.toolbarCategory', { count, category: activeCategoryLabel.value })
  if (brand.value) return t('productsPage.toolbarBrand', { count, brand: activeBrandLabel.value })
  return t('productsPage.toolbarAll', { count })
})

async function fetchProducts() {
  await products.fetch({
    page: page.value,
    pageSize: 12,
    q: q.value || undefined,
    sort: sort.value as any,
    brand: brand.value || undefined,
    category: category.value || undefined,
    subCategory: subCategory.value || undefined,
  })
}

function applyFilters() {
  page.value = 1
  router.push({
    path: '/products',
    query: {
      ...(q.value ? { q: q.value } : {}),
      ...(sort.value && sort.value !== 'new' ? { sort: sort.value } : {}),
      ...(brand.value ? { brand: brand.value } : {}),
      ...(category.value ? { category: category.value } : {}),
      ...(subCategory.value ? { subCategory: subCategory.value } : {}),
      page: '1',
    },
  })
}

function clearFilters() {
  sort.value = 'new'
  brand.value = ''
  category.value = ''
  subCategory.value = ''
  q.value = ''
  page.value = 1
  router.push({ path: '/products', query: { page: '1' } })
}

function goPage(p: number) {
  page.value = Math.max(1, p)
  router.push({
    path: '/products',
    query: {
      ...route.query,
      page: String(page.value),
    },
  })
}

watch(
  () => route.query,
  async (qv) => {
    q.value = String(qv.q || '')
    sort.value = String(qv.sort || 'new')
    brand.value = String(qv.brand || '')
    category.value = String(qv.category || '')
    subCategory.value = String(qv.subCategory || '')
    page.value = Number(qv.page || 1) || 1
    await fetchProducts()
  },
  { deep: true }
)
</script>

<style scoped>
.products-hero{
  position: relative;
  border: 1px solid rgba(var(--border), .92);
  background:
    radial-gradient(circle at top right, rgba(var(--primary), .12), transparent 24%),
    linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .94));
  box-shadow: 0 24px 56px rgba(16,24,40,.08);
}
.products-summary{
  align-self: stretch;
}
.summary-chip{
  min-height: 94px;
  padding: 16px;
  border-radius: 22px;
  border: 1px solid rgba(var(--border), .92);
  background: rgba(var(--surface-rgb), .82);
  box-shadow: inset 0 1px 0 rgba(255,255,255,.08);
}
.summary-chip__label{
  font-size: 12px;
  color: rgb(var(--muted));
}
.summary-chip__value{
  margin-top: 12px;
  font-size: 1.2rem;
  font-weight: 900;
  color: rgb(var(--text));
}
.products-filters,
.products-toolbar{
  border: 1px solid rgba(var(--border), .92);
}
.filter-label{
  display:block;
  margin-bottom: 8px;
  font-size: .85rem;
  font-weight: 800;
  color: rgb(var(--text));
}
.products-select{
  min-height: 52px;
}
.results-pill{
  display:flex;
  align-items:center;
  justify-content:center;
  min-height: 46px;
  padding: 0 14px;
  border-radius: 999px;
  border: 1px solid rgba(var(--primary), .18);
  background: rgba(var(--primary), .08);
  color: rgb(var(--text));
  font-size: .9rem;
  font-weight: 800;
}
.products-skeleton{
  min-height: 360px;
  border-radius: 28px;
}
.pagination-center{
  min-width: 84px;
}
:global(html.theme-light) .products-hero{
  background:
    radial-gradient(circle at top right, rgba(236,72,153,.14), transparent 24%),
    linear-gradient(180deg, rgba(255,255,255,.98), rgba(251,245,249,.95));
  border-color: rgba(228, 208, 221, .95);
  box-shadow: 0 24px 56px rgba(236,72,153,.08), 0 8px 22px rgba(17,24,39,.04);
}
:global(html.theme-light) .summary-chip,
:global(html.theme-light) .products-filters,
:global(html.theme-light) .products-toolbar{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(252,247,250,.95));
  border-color: rgba(228, 208, 221, .95);
  box-shadow: 0 16px 32px rgba(17,24,39,.04), 0 8px 22px rgba(236,72,153,.06);
}
:global(html.theme-dark) .products-hero{
  box-shadow: 0 24px 56px rgba(0,0,0,.32);
}
@media (max-width: 1279px){
  .products-filters{ position: static; }
}
</style>
