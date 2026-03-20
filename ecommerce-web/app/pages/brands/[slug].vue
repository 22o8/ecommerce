<template>
  <div class="container mx-auto px-4 py-8">
    <!-- Brand hero -->
    <div class="brand-hero overflow-hidden">
      <div class="p-6 sm:p-8 lg:p-10 flex flex-col sm:flex-row gap-6 sm:items-center">
        <div class="brand-logo-shell">
          <SmartImage v-if="brandLogo" :src="brandLogo" :alt="brand?.name" class="w-full h-full object-contain p-2" />
          <div v-else class="text-xs text-[rgba(var(--muted),0.85)]">Logo</div>
        </div>

        <div class="min-w-0">
          <h1 class="text-3xl sm:text-4xl lg:text-5xl font-extrabold text-[rgb(var(--text))]">
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
              class="input"
            />
            <button
              v-if="q"
              class="absolute right-2 top-1/2 -translate-y-1/2 icon-btn"
              @click="q = ''"
              aria-label="clear"
            >
              ✕
            </button>
          </div>

          <select v-model="sort" class="select">
            <option value="new">{{ t('products.sortNew') }}</option>
            <option value="priceAsc">{{ t('products.sortPriceAsc') }}</option>
            <option value="priceDesc">{{ t('products.sortPriceDesc') }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Products grid -->
    <div class="mt-6 grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-4">
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


<style scoped>
.brand-hero{
  border-radius: 32px;
  border: 1px solid rgba(var(--border), .96);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .9));
  box-shadow: 0 24px 64px rgba(0,0,0,.18);
}
.brand-logo-shell{
  width: 104px;
  height: 104px;
  border-radius: 28px;
  border: 1px solid rgba(var(--border), .96);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .94), rgba(var(--surface-2-rgb), .86));
  overflow: hidden;
  display:flex;
  align-items:center;
  justify-content:center;
  box-shadow: inset 0 1px 0 rgba(255,255,255,.1), 0 18px 44px rgba(0,0,0,.16);
  flex: 0 0 auto;
}
:global(html.theme-light) .brand-hero{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,246,250,.94));
  box-shadow: 0 22px 56px rgba(232,91,154,.10), 0 10px 24px rgba(17,24,39,.05);
}
:global(html.theme-light) .brand-logo-shell{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(250,244,248,.94));
}
@media (max-width: 640px){
  .brand-logo-shell{ width: 88px; height: 88px; border-radius: 24px; }
}
</style>
