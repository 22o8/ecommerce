<template>
  <div class="container mx-auto px-4 py-8">
    <div class="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-4">
      <div>
        <h1 class="text-3xl sm:text-4xl font-extrabold rtl-text">{{ t('productsPage.title') }}</h1>
        <p class="mt-2 text-muted rtl-text">{{ t('productsPage.subtitle') }}</p>
      </div>

      <div class="control-box w-full lg:w-[560px] grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="relative">
          <input
            v-model="q"
            :placeholder="t('productsPage.searchPlaceholder')"
            class="input"
            @keydown.enter="applyFilters"
          />
          <button
            v-if="q"
            type="button"
            class="absolute left-3 top-1/2 -translate-y-1/2 icon-btn"
            @click="q = ''"
            aria-label="clear"
          >
            ✕
          </button>
        </div>

        <select v-model="sort" class="input py-3">
          <option value="new">{{ t('productsPage.sort.new') }}</option>
          <option value="priceAsc">{{ t('productsPage.sort.priceAsc') }}</option>
          <option value="priceDesc">{{ t('productsPage.sort.priceDesc') }}</option>
        </select>

        <select v-model="brand" class="input py-3 sm:col-span-2">
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

    <div v-else class="mt-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 page-stagger">
      <ProductCard v-for="p in products.items" :key="p.id" :p="p" />
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