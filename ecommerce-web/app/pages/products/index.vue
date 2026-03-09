<template>
  <div class="catalog-page mx-auto max-w-7xl px-4 py-8 sm:py-10">
    <section class="catalog-hero rounded-[32px] p-6 sm:p-8 lg:p-10">
      <div class="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
        <div class="max-w-2xl">
          <div class="catalog-kicker rtl-text">{{ t('productsPage.kicker') }}</div>
          <h1 class="mt-3 text-3xl font-black tracking-tight rtl-text sm:text-5xl">{{ t('productsPage.title') }}</h1>
          <p class="mt-3 text-sm text-muted rtl-text sm:text-base">{{ t('productsPage.subtitle') }}</p>
        </div>

        <div class="grid w-full gap-3 sm:grid-cols-3 lg:w-auto lg:min-w-[430px]">
          <div class="catalog-stat">
            <div class="catalog-stat__label rtl-text">{{ t('productsPage.resultsCount') }}</div>
            <div class="catalog-stat__value keep-ltr">{{ products.totalCount || products.items.length }}</div>
          </div>
          <div class="catalog-stat">
            <div class="catalog-stat__label rtl-text">{{ t('productsPage.activeSort') }}</div>
            <div class="catalog-stat__value rtl-text text-base">{{ sortLabel }}</div>
          </div>
          <div class="catalog-stat">
            <div class="catalog-stat__label rtl-text">{{ t('productsPage.selectedBrand') }}</div>
            <div class="catalog-stat__value rtl-text text-base">{{ selectedBrandLabel }}</div>
          </div>
        </div>
      </div>
    </section>

    <section class="mt-6 grid gap-6 xl:grid-cols-[280px_minmax(0,1fr)]">
      <aside class="catalog-sidebar rounded-[28px] p-4 sm:p-5 xl:sticky xl:top-24 h-fit">
        <div class="flex items-center justify-between gap-3">
          <div>
            <div class="text-lg font-black rtl-text">{{ t('productsPage.filtersTitle') }}</div>
            <div class="mt-1 text-xs text-muted rtl-text">{{ t('productsPage.filtersHint') }}</div>
          </div>
          <button type="button" class="catalog-reset rtl-text" @click="resetFilters">{{ t('common.reset') }}</button>
        </div>

        <div class="mt-5 grid gap-4">
          <label class="grid gap-2">
            <span class="text-xs font-bold text-muted rtl-text">{{ t('productsPage.sortTitle') }}</span>
            <select v-model="sort" class="catalog-field" @change="applyFilters" aria-label="Sort">
              <option value="new">{{ t('productsPage.sortNewest') }}</option>
              <option value="priceAsc">{{ t('productsPage.sortPriceAsc') }}</option>
              <option value="priceDesc">{{ t('productsPage.sortPriceDesc') }}</option>
            </select>
          </label>

          <label class="grid gap-2">
            <span class="text-xs font-bold text-muted rtl-text">{{ t('productsPage.brandTitle') }}</span>
            <select v-model="brand" class="catalog-field" @change="applyFilters" aria-label="Brand">
              <option value="">{{ t('productsPage.allBrands') }}</option>
              <option v-for="b in brandOptions" :key="b.slug" :value="b.slug">{{ b.name }}</option>
            </select>
          </label>

          <div class="rounded-2xl border border-app bg-surface-2/60 p-3">
            <div class="text-xs font-bold text-muted rtl-text">{{ t('productsPage.quickTipTitle') }}</div>
            <div class="mt-2 text-sm text-muted rtl-text">{{ t('productsPage.quickTip') }}</div>
          </div>
        </div>
      </aside>

      <div class="min-w-0">
        <div class="catalog-toolbar rounded-[28px] p-4 sm:p-5">
          <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div class="text-lg font-black rtl-text">{{ t('productsPage.collectionTitle') }}</div>
              <div class="mt-1 text-sm text-muted rtl-text">{{ toolbarHint }}</div>
            </div>
            <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-2 text-sm">
              <span class="h-2.5 w-2.5 rounded-full bg-[rgb(var(--primary))]" />
              <span class="rtl-text">{{ t('productsPage.readyToShop') }}</span>
            </div>
          </div>
        </div>

        <div v-if="products.loading && products.items.length === 0" class="mt-6">
          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
            <div v-for="n in 6" :key="n" class="catalog-skeleton" />
          </div>
        </div>

        <div v-else-if="!products.loading && products.items.length === 0" class="catalog-empty mt-6 rounded-[28px] p-10 text-center">
          <div class="text-xl font-black rtl-text">{{ t('productsPage.emptyTitle') }}</div>
          <div class="mt-2 text-sm text-muted rtl-text">{{ t('productsPage.emptyDesc') }}</div>
          <button type="button" class="mt-5 catalog-empty__btn rtl-text" @click="resetFilters">{{ t('productsPage.clearFilters') }}</button>
        </div>

        <div v-else class="mt-6 grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-3">
          <template v-for="(p, idx) in products.items" :key="p.id">
            <RevealOnScroll v-if="!liteMode" :parity="(idx % 2) as 0 | 1" :delay="35 * (idx % 6)">
              <ProductCard :p="p" />
            </RevealOnScroll>
            <div v-else>
              <ProductCard :p="p" />
            </div>
          </template>
        </div>

        <div class="mt-8 flex flex-col gap-3 rounded-[24px] border border-app bg-surface px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
          <div class="text-sm text-muted rtl-text">{{ t('productsPage.page') }} <span class="keep-ltr font-black">{{ page }}</span> / <span class="keep-ltr font-black">{{ totalPages }}</span></div>
          <div class="flex items-center gap-2">
            <button class="catalog-page-btn" :disabled="page <= 1 || products.loading" @click="goPage(page - 1)">{{ t('productsPage.prev') }}</button>
            <button class="catalog-page-btn catalog-page-btn--primary" :disabled="!hasNext || products.loading" @click="goPage(page + 1)">{{ t('productsPage.next') }}</button>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { liteMode } = useMobilePerf()

const brandsStore = useBrandsStore()
const products = useProductsStore()

const q = ref(String(route.query.q || ''))
const sort = ref(String(route.query.sort || 'new'))
const brand = ref(String(route.query.brand || ''))
const page = ref(Number(route.query.page || 1) || 1)

await useAsyncData('products_page_boot', async () => {
  await brandsStore.fetchPublic()
  await fetchProducts()
  return true
})

const brandOptions = computed(() => (brandsStore.publicItems || []).map((b: any) => ({ name: b.name, slug: b.slug })))
const selectedBrandLabel = computed(() => {
  if (!brand.value) return t('productsPage.allBrands')
  return brandOptions.value.find((b: any) => b.slug === brand.value)?.name || t('productsPage.allBrands')
})
const sortLabel = computed(() => {
  if (sort.value === 'priceAsc') return t('productsPage.sortPriceAsc')
  if (sort.value === 'priceDesc') return t('productsPage.sortPriceDesc')
  return t('productsPage.sortNewest')
})
const toolbarHint = computed(() => {
  if (brand.value) return `${t('productsPage.filteredByBrand')}: ${selectedBrandLabel.value}`
  return t('productsPage.collectionHint')
})

const hasNext = computed(() => {
  const total = Number(products.totalCount || 0)
  const currentPageSize = liteMode.value ? 9 : 12
  return page.value * currentPageSize < total
})
const totalPages = computed(() => {
  const total = Number(products.totalCount || products.items.length || 0)
  const currentPageSize = liteMode.value ? 9 : 12
  return Math.max(1, Math.ceil(total / currentPageSize))
})

async function fetchProducts() {
  await products.fetch({
    page: page.value,
    pageSize: liteMode.value ? 9 : 12,
    q: q.value || undefined,
    sort: sort.value as any,
    brand: brand.value || undefined,
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
      page: '1',
    },
  })
}

function resetFilters() {
  q.value = ''
  sort.value = 'new'
  brand.value = ''
  page.value = 1
  router.push({ path: '/products', query: { page: '1' } })
}

function goPage(p: number) {
  page.value = Math.max(1, p)
  router.push({ path: '/products', query: { ...route.query, page: String(page.value) } })
}

watch(() => route.query, async (qv) => {
  q.value = String(qv.q || '')
  sort.value = String(qv.sort || 'new')
  brand.value = String(qv.brand || '')
  page.value = Number(qv.page || 1) || 1
  await fetchProducts()
}, { deep: true })
</script>

<style scoped>
.catalog-hero,
.catalog-sidebar,
.catalog-toolbar,
.catalog-empty,
.catalog-skeleton{
  border: 1px solid rgba(var(--border), .96);
}
.catalog-hero{
  background: linear-gradient(135deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .92));
  box-shadow: 0 24px 64px rgba(0,0,0,.16);
}
.catalog-kicker{
  font-size: 12px;
  font-weight: 900;
  letter-spacing: .18em;
  text-transform: uppercase;
  color: rgb(var(--primary));
}
.catalog-stat{
  border-radius: 24px;
  padding: 16px;
  background: rgba(var(--surface-rgb), .78);
  border: 1px solid rgba(var(--border), .92);
}
.catalog-stat__label{ font-size: 12px; color: rgb(var(--muted)); }
.catalog-stat__value{ margin-top: 6px; font-size: 24px; font-weight: 900; color: rgb(var(--text)); }
.catalog-sidebar,
.catalog-toolbar,
.catalog-empty{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90));
  box-shadow: 0 16px 40px rgba(0,0,0,.12);
}
.catalog-field{
  width: 100%; border-radius: 18px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); padding: 12px 14px; color: rgb(var(--text)); outline: none;
}
.catalog-field:focus{ border-color: rgba(var(--primary), .45); box-shadow: 0 0 0 4px rgba(var(--primary), .12); }
.catalog-reset{
  border-radius: 999px; padding: 8px 12px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .82); font-weight: 800;
}
.catalog-skeleton{ height: 360px; border-radius: 28px; background: linear-gradient(90deg, rgba(var(--surface-rgb), .92) 25%, rgba(var(--surface-2-rgb), .98) 50%, rgba(var(--surface-rgb), .92) 75%); background-size: 200% 100%; animation: pulse-shift 1.5s infinite; }
.catalog-empty__btn,
.catalog-page-btn{ border-radius: 999px; padding: 11px 16px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); font-weight: 800; }
.catalog-page-btn--primary,
.catalog-empty__btn{ background: rgb(var(--primary)); color: #111; border-color: rgba(var(--primary), .3); }
@keyframes pulse-shift { 0% { background-position: 200% 0 } 100% { background-position: -200% 0 } }
:global(html.theme-light) .catalog-hero{
  background: linear-gradient(135deg, rgba(255,255,255,.98), rgba(252,244,249,.95));
  box-shadow: 0 24px 64px rgba(236,72,153,.08), 0 12px 28px rgba(24,24,24,.04);
}
:global(html.theme-light) .catalog-sidebar,
:global(html.theme-light) .catalog-toolbar,
:global(html.theme-light) .catalog-empty{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(253,247,250,.96));
  box-shadow: 0 18px 44px rgba(236,72,153,.07), 0 10px 24px rgba(24,24,24,.03);
}
:global(html.theme-light) .catalog-stat{
  background: rgba(255,255,255,.92);
  box-shadow: inset 0 1px 0 rgba(255,255,255,.8);
}
:global(html.theme-light) .catalog-field,
:global(html.theme-light) .catalog-reset,
:global(html.theme-light) .catalog-page-btn{
  background: rgba(255,255,255,.96);
}
</style>
