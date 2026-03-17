<template>
  <div class="container mx-auto px-4 py-8">
    <div class="grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
      <aside class="products-filters card-soft p-4 sm:p-5 xl:sticky xl:top-24 h-fit">
        <div class="flex items-center justify-between gap-3">
          <div>
            <div class="text-lg font-extrabold text-[rgb(var(--text))] rtl-text">{{ t('productsPage.filtersTitle') }}</div>
            <div class="mt-1 text-xs text-[rgb(var(--muted))] rtl-text">{{ brand?.name || slug }}</div>
          </div>
          <button type="button" class="btn-secondary px-3 py-2 text-sm" @click="clearFilters" :disabled="products.loading">{{ t('common.clear') }}</button>
        </div>

        <div class="mt-4 grid gap-3">
          <div class="filter-field">
            <label class="filter-label rtl-text">{{ t('brandPage.search') }}</label>
            <input v-model="q" :placeholder="t('brandPage.search')" class="input" @input="debouncedLoad" />
          </div>

          <div class="filter-field">
            <label class="filter-label rtl-text">{{ t('productsPage.sort') }}</label>
            <select v-model="sort" class="input products-select" @change="load">
              <option value="new">{{ t('products.sortNew') }}</option>
              <option value="priceAsc">{{ t('products.sortPriceAsc') }}</option>
              <option value="priceDesc">{{ t('products.sortPriceDesc') }}</option>
            </select>
          </div>

          <div class="results-pill rtl-text">{{ t('productsPage.resultsCount', { count: products.items.length || 0 }) }}</div>
        </div>
      </aside>

      <div>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          <ProductCard v-for="p in products.items" :key="p.id" :p="p" />
        </div>

        <div v-if="!products.loading && products.items.length === 0" class="mt-10 rounded-2xl border border-[rgba(var(--border),1)] bg-[rgba(var(--surface),0.7)] p-10 text-center text-[rgba(var(--muted),0.95)]">
          {{ t('brandPage.empty') }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'

const { t } = useI18n()
const route = useRoute()

const slug = computed(() => String(route.params.slug || '').toLowerCase())
const brands = useBrandsStore()
const products = useProductsStore()

const q = ref('')
const sort = ref<'new'|'priceAsc'|'priceDesc'>('new')

const brand = ref<any>(null)

const load = async () => {
  // load brand info (not required to load products, but good for hero)
  try {
    brand.value = await brands.getBySlug(slug.value)
  } catch {
    brand.value = null
  }

  await products.fetch({
    page: 1,
    pageSize: 40,
    q: q.value || undefined,
    sort: sort.value,
    brand: slug.value,
  })
}

await useAsyncData(`brand-${slug.value}`, async () => {
  await load()
  return true
})

let searchTimer: any = null
function debouncedLoad() {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => { load() }, 180)
}

watch(sort, async () => {
  await load()
})

function clearFilters() {
  q.value = ''
  sort.value = 'new'
  load()
}
</script>


<style scoped>
.products-filters{ border: 1px solid rgba(var(--border), .92); }
.filter-label{ display:block; margin-bottom: 8px; font-size: .85rem; font-weight: 800; color: rgb(var(--text)); }
.products-select{ min-height: 52px; }
.results-pill{ display:flex; align-items:center; justify-content:center; min-height: 46px; padding: 0 14px; border-radius: 999px; border: 1px solid rgba(var(--primary), .18); background: rgba(var(--primary), .08); color: rgb(var(--text)); font-size: .9rem; font-weight: 800; }
:global(html.theme-light) .products-filters{ background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(252,247,250,.95)); border-color: rgba(228, 208, 221, .95); box-shadow: 0 16px 32px rgba(17,24,39,.04), 0 8px 22px rgba(236,72,153,.06); }
@media (max-width: 1279px){ .products-filters{ position: static; } }
</style>
