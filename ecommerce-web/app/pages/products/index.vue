<template>
  <div class="products-shell container mx-auto px-4 py-8 sm:py-10">
    <section class="products-hero card-soft overflow-hidden rounded-[32px] p-5 sm:p-7 lg:p-8">
      <div class="products-hero__glow products-hero__glow--one" />
      <div class="products-hero__glow products-hero__glow--two" />

      <div class="relative z-[1] flex flex-col gap-5 xl:flex-row xl:items-end xl:justify-between">
        <div class="max-w-2xl">
          <div class="products-kicker mb-3 inline-flex items-center rounded-full px-3 py-1 text-xs font-bold">
            {{ t('nav.products') }}
          </div>
          <h1 class="text-3xl sm:text-4xl lg:text-5xl font-extrabold rtl-text tracking-tight text-[rgb(var(--text))]">
            {{ t('productsPage.title') }}
          </h1>
          <p class="mt-3 max-w-2xl text-sm sm:text-base text-muted rtl-text leading-7">
            {{ t('productsPage.subtitle') }}
          </p>
        </div>

        <div class="products-controls w-full xl:max-w-[760px]">
          <div class="products-controls__panel rounded-[28px] p-3 sm:p-4">
            <div class="grid grid-cols-1 gap-3 md:grid-cols-[1fr_1fr_auto]">
              <label class="filter-field">
                <span class="filter-label">{{ t('productsPage.sortNewest') }}</span>
                <select v-model="sort" class="input products-input" @change="applyFilters" aria-label="Sort">
                  <option value="priceAsc">{{ t('productsPage.sortPriceAsc') }}</option>
                  <option value="priceDesc">{{ t('productsPage.sortPriceDesc') }}</option>
                  <option value="new">{{ t('productsPage.sortNewest') }}</option>
                </select>
              </label>

              <label class="filter-field">
                <span class="filter-label">{{ t('productsPage.allBrands') }}</span>
                <select v-model="brand" class="input products-input" @change="applyFilters" aria-label="Brand">
                  <option value="">{{ t('productsPage.allBrands') }}</option>
                  <option v-for="b in brandOptions" :key="b.slug" :value="b.slug">
                    {{ b.name }}
                  </option>
                </select>
              </label>

              <div class="products-stats rounded-2xl px-4 py-3 md:min-w-[130px]">
                <div class="text-[11px] font-bold uppercase tracking-[0.18em] text-muted/80">{{ t('common.total') }}</div>
                <div class="mt-1 text-2xl font-extrabold text-[rgb(var(--text))] keep-ltr">{{ products.totalCount || products.items.length }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="mt-6 sm:mt-8">
      <div v-if="products.loading && products.items.length === 0" class="products-loading card-soft rounded-[28px] p-5 sm:p-6">
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div v-for="n in 8" :key="n" class="skeleton-card" />
        </div>
        <div class="mt-6 flex items-center justify-center text-sm text-muted rtl-text">
          <span class="spinner" aria-hidden="true" />
          <span class="ms-2">{{ t('common.loading') }}</span>
        </div>
      </div>

      <template v-else>
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
          <RevealOnScroll
            v-for="(p, idx) in products.items"
            :key="p.id"
            :parity="(idx % 2) as 0 | 1"
            :delay="35 * (idx % 8)"
          >
            <ProductCard :p="p" />
          </RevealOnScroll>
        </div>

        <div v-if="!products.loading && products.items.length === 0" class="mt-10 card-soft rounded-[28px] p-10 text-center text-muted rtl-text">
          <div class="font-extrabold text-[rgb(var(--text))]">{{ t('productsPage.emptyTitle') }}</div>
          <div class="mt-2 text-sm">{{ t('productsPage.emptyDesc') }}</div>
        </div>
      </template>
    </section>

    <section class="mt-8 sm:mt-10">
      <div class="products-pagination rounded-[24px] p-3 sm:p-4">
        <button class="btn-secondary products-page-btn" :disabled="page <= 1 || products.loading" @click="goPage(page - 1)">
          {{ t('productsPage.prev') }}
        </button>
        <div class="text-sm text-muted keep-ltr font-semibold">
          {{ t('productsPage.page') }}: {{ page }}
        </div>
        <button class="btn-secondary products-page-btn" :disabled="!hasNext || products.loading" @click="goPage(page + 1)">
          {{ t('productsPage.next') }}
        </button>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

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

const hasNext = computed(() => {
  const total = Number(products.totalCount || 0)
  const pageSize = 12
  return page.value * pageSize < total
})

async function fetchProducts() {
  await products.fetch({
    page: page.value,
    pageSize: 12,
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

watch([q, sort, brand], () => {})

watch(
  () => route.query,
  async (qv) => {
    q.value = String(qv.q || '')
    sort.value = String(qv.sort || 'new')
    brand.value = String(qv.brand || '')
    page.value = Number(qv.page || 1) || 1
    await fetchProducts()
  },
  { deep: true }
)
</script>

<style scoped>
.products-shell{
  padding-bottom: 3rem;
}
.products-hero{
  position: relative;
  border: 1px solid rgba(var(--border), .9);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .94));
  box-shadow: 0 28px 60px rgba(15, 23, 42, .08);
}
.products-hero__glow{
  position:absolute;
  inset:auto;
  border-radius:999px;
  filter: blur(50px);
  pointer-events:none;
  opacity:.55;
}
.products-hero__glow--one{
  width:220px; height:220px; top:-70px; inset-inline-start:-40px;
  background: rgba(var(--primary), .16);
}
.products-hero__glow--two{
  width:260px; height:260px; bottom:-100px; inset-inline-end:-20px;
  background: rgba(var(--primary), .10);
}
.products-kicker{
  border:1px solid rgba(var(--primary), .16);
  background: rgba(var(--primary), .1);
  color: rgb(var(--text));
}
.products-controls__panel,
.products-pagination{
  border: 1px solid rgba(var(--border), .88);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .95), rgba(var(--surface-2-rgb), .92));
  box-shadow: 0 18px 40px rgba(15,23,42,.06);
}
.filter-field{
  display:flex;
  flex-direction:column;
  gap:.45rem;
}
.filter-label{
  font-size:.72rem;
  font-weight:800;
  color: rgb(var(--muted));
  padding-inline: .25rem;
}
.products-input{
  min-height: 52px;
  border-radius: 18px;
  background: rgba(var(--surface-rgb), .92);
  border-color: rgba(var(--border), .92);
  box-shadow: inset 0 1px 0 rgba(255,255,255,.16);
}
.products-stats{
  border: 1px solid rgba(var(--border), .88);
  background: rgba(var(--surface-rgb), .84);
  display:flex;
  flex-direction:column;
  justify-content:center;
}
.products-page-btn{
  min-width: 110px;
  justify-content:center;
}
.products-pagination{
  display:flex;
  align-items:center;
  justify-content:space-between;
  gap: 1rem;
}
:global(html.theme-light) .products-hero{
  background:
    radial-gradient(circle at top right, rgba(236,72,153,.10), transparent 30%),
    linear-gradient(180deg, rgba(255,255,255,.98), rgba(250,244,248,.96));
  border-color: rgba(225, 208, 221, .95);
  box-shadow: 0 24px 56px rgba(17,24,39,.05), 0 14px 34px rgba(236,72,153,.08);
}
:global(html.theme-light) .products-controls__panel,
:global(html.theme-light) .products-pagination{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(250,244,248,.96));
  border-color: rgba(225, 208, 221, .95);
  box-shadow: 0 16px 38px rgba(17,24,39,.05), 0 8px 22px rgba(236,72,153,.06);
}
:global(html.theme-light) .products-input{
  background: rgba(255,255,255,.96);
  border-color: rgba(224, 205, 220, .92);
  box-shadow: 0 10px 24px rgba(236,72,153,.05);
}
:global(html.theme-light) .products-stats{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(251,246,249,.94));
  border-color: rgba(224, 205, 220, .92);
}
@media (max-width: 640px){
  .products-pagination{
    flex-wrap:wrap;
  }
  .products-page-btn{
    flex:1 1 0%;
  }
}
</style>
