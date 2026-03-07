<template>
  <div class="products-page container mx-auto px-4 py-8">
    <!-- Header + Filters (no page search; only Navbar search remains) -->
    <div class="products-hero flex flex-col lg:flex-row lg:items-end lg:justify-between gap-4">
      <div>
        <h1 class="text-3xl sm:text-4xl font-extrabold rtl-text">{{ t('productsPage.title') }}</h1>
        <p class="mt-2 text-muted rtl-text">{{ t('productsPage.subtitle') }}</p>
      </div>

      <div class="products-filters control-box glass-panel glow-border rounded-3xl p-3 w-full lg:w-[560px] grid grid-cols-1 sm:grid-cols-2 gap-3 lg:sticky lg:top-24">
        <select v-model="sort" class="input" @change="applyFilters" aria-label="Sort">
          <option value="priceAsc">{{ t('productsPage.sortPriceAsc') }}</option>
          <option value="priceDesc">{{ t('productsPage.sortPriceDesc') }}</option>
          <option value="new">{{ t('productsPage.sortNewest') }}</option>
        </select>

        <select v-model="brand" class="input" @change="applyFilters" aria-label="Brand">
          <option value="">{{ t('productsPage.allBrands') }}</option>
          <option v-for="b in brandOptions" :key="b.slug" :value="b.slug">
            {{ b.name }}
          </option>
        </select>
      </div>
    </div>

    <!-- Loading state (API delay) -->
    <div v-if="products.loading && products.items.length === 0" class="mt-6">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div v-for="n in 8" :key="n" class="skeleton-card" />
      </div>
      <div class="mt-6 flex items-center justify-center text-sm text-muted rtl-text">
        <span class="spinner" aria-hidden="true" />
        <span class="ms-2">{{ t('common.loading') }}</span>
      </div>
    </div>

    <div v-else class="mt-8 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-5">
      <RevealOnScroll
        v-for="(p, idx) in products.items"
        :key="p.id"
        :parity="(idx % 2) as 0 | 1"
        :delay="35 * (idx % 8)"
      >
        <ProductCard :p="p" />
      </RevealOnScroll>
    </div>

    <div v-if="!products.loading && products.items.length === 0" class="mt-10 card-soft p-10 text-center text-muted rtl-text">
      <div class="font-extrabold">{{ t('productsPage.emptyTitle') }}</div>
      <div class="mt-2 text-sm">{{ t('productsPage.emptyDesc') }}</div>
    </div>

    <!-- Pagination -->
    <div class="mt-8 flex items-center justify-between gap-3">
      <button class="btn-secondary" :disabled="page <= 1 || products.loading" @click="goPage(page - 1)">
        {{ t('productsPage.prev') }}
      </button>
      <div class="text-sm text-muted keep-ltr">
        {{ t('productsPage.page') }}: {{ page }}
      </div>
      <button class="btn-secondary" :disabled="!hasNext || products.loading" @click="goPage(page + 1)">
        {{ t('productsPage.next') }}
      </button>
    </div>
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
  // نجيب البراندات مرة حتى يشتغل فلتر البراند
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

watch([q, sort, brand], () => {
  // لا نسوي fetch مباشر هنا حتى ما يصير spam...
  // المستخدم يقدر يضغط Enter أو يغير sort/brand فتتحدث URL وتعمل watcher أدناه
})

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
.products-page{ min-height: calc(100vh - 120px); }
.products-hero{
  border-radius: 32px;
  padding: 28px;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .85), rgba(var(--surface-2-rgb), .78));
  border: 1px solid rgba(var(--border), .95);
  box-shadow: 0 20px 50px rgba(0,0,0,.10);
}
.products-filters{
  border-radius: 28px;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .95), rgba(var(--surface-2-rgb), .92));
  border: 1px solid rgba(var(--border), .95);
  box-shadow: 0 20px 44px rgba(15,23,42,.08);
}
.products-filters .input{
  min-height: 52px;
  border-radius: 18px;
  border: 1px solid rgba(var(--border), .95);
  background: rgba(var(--surface-rgb), .92);
  padding: 0 16px;
}
.skeleton-card{
  height: 360px;
  border-radius: 28px;
  border: 1px solid rgba(var(--border), .95);
  background: linear-gradient(90deg, rgba(var(--surface-2-rgb), .9), rgba(var(--surface-rgb), 1), rgba(var(--surface-2-rgb), .9));
  background-size: 220% 100%;
  animation: shimmer 1.4s linear infinite;
}
@keyframes shimmer { 0%{background-position:200% 0}100%{background-position:-20% 0} }
:global(html.theme-light) .products-hero{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(251,244,248,.95));
  border-color: rgba(230, 214, 224, .95);
  box-shadow: 0 24px 56px rgba(17,24,39,.05), 0 12px 30px rgba(236,72,153,.08);
}
:global(html.theme-light) .products-filters{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(250,244,248,.95));
  box-shadow: 0 18px 40px rgba(236,72,153,.08);
}
:global(html.theme-dark) .products-hero, :global(html.theme-dark) .products-filters{
  box-shadow: 0 22px 54px rgba(0,0,0,.28);
}
</style>