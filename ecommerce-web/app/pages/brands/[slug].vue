<template>
  <div class="container mx-auto px-4 py-8">
    <!-- Brand hero -->
    <div class="rounded-3xl border border-[rgba(var(--border),1)] bg-[rgba(var(--surface),0.7)] overflow-hidden">
      <div class="p-6 sm:p-8 flex flex-col sm:flex-row gap-6 sm:items-center">
        <div class="w-20 h-20 sm:w-24 sm:h-24 rounded-2xl bg-black/20 border border-[rgba(var(--border),1)] overflow-hidden flex items-center justify-center">
          <SmartImage v-if="brandLogo" :src="brandLogo" :alt="brand?.name" class="w-full h-full object-cover" />
          <div v-else class="text-xs text-[rgba(var(--muted),0.85)]">Logo</div>
        </div>

        <div class="min-w-0">
          <h1 class="text-3xl sm:text-4xl font-extrabold text-[rgb(var(--text))]">
            {{ brand?.name || slug }}
          </h1>
          <p class="text-[rgba(var(--muted),0.9)] mt-2 max-w-2xl">
            {{ brand?.description || t('brandPage.defaultDesc') }}
          </p>
        </div>

        <div class="ms-auto flex gap-2 w-full sm:w-auto">
          <div class="relative flex-1 sm:w-[320px]">
            <input
              v-model="q"
              :placeholder="t('brandPage.search')"
              class="w-full rounded-xl bg-white/[0.04] border border-[rgba(var(--border),1)] px-4 py-3 outline-none focus:ring-2 focus:ring-white/10"
            />
            <button
              v-if="q"
              class="absolute right-2 top-1/2 -translate-y-1/2 text-[rgba(var(--muted),0.85)] hover:text-[rgb(var(--text))]"
              @click="q = ''"
              aria-label="clear"
            >
              ✕
            </button>
          </div>

          <select v-model="sort" class="rounded-xl bg-white/[0.04] border border-[rgba(var(--border),1)] px-4 py-3 outline-none">
            <option value="new">{{ t('products.sortNew') }}</option>
            <option value="priceAsc">{{ t('products.sortPriceAsc') }}</option>
            <option value="priceDesc">{{ t('products.sortPriceDesc') }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Products grid -->
    <div class="mt-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <ProductCard v-for="p in products.items" :key="p.id" :p="p" />
    </div>

    <div v-if="!products.loading && products.items.length === 0" class="mt-10 rounded-2xl border border-[rgba(var(--border),1)] bg-[rgba(var(--surface),0.7)] p-10 text-center text-[rgba(var(--muted),0.95)]">
      {{ t('brandPage.empty') }}
    </div>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'
import ProductCard from '~/components/ProductCard.vue'

const { t } = useI18n()
const route = useRoute()

const slug = computed(() => String(route.params.slug || '').toLowerCase())
const brands = useBrandsStore()
const products = useProductsStore()
const { buildAssetUrl } = useApi()

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

watch([q, sort], async () => {
  await load()
})

const brandLogo = computed(() => buildAssetUrl(brand.value?.logoUrl || ''))
</script>
