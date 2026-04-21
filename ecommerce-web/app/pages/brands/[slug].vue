<template>
  <div class="container mx-auto px-4 py-8">
    <div class="brand-hero overflow-hidden">
      <div class="p-6 sm:p-8 lg:p-10 flex flex-col sm:flex-row gap-6 sm:items-center">
        <div class="brand-logo-shell">
          <SmartImage v-if="brandLogo" :src="brandLogo" :alt="brand?.name" class="w-full h-full object-contain p-2" />
          <div v-else class="text-xs text-[rgba(var(--muted),0.85)]">Logo</div>
        </div>
        <div class="min-w-0">
          <h1 class="text-3xl sm:text-4xl lg:text-5xl font-extrabold text-[rgb(var(--text))]">{{ brand?.name || slug }}</h1>
          <p class="text-[rgba(var(--muted),0.9)] mt-2 max-w-2xl">{{ brand?.description || t('brandPage.defaultDesc') }}</p>
        </div>
      </div>
    </div>

    <ProductResultsSection
      :items="filteredItems"
      :loading="products.loading"
      :sort="sort"
      :count="filteredItems.length"
      title="فلترة البراند"
      hint="رتّب منتجات البراند حسب الوقت أو الاسم أو السعر."
      sort-label="الترتيب"
      clear-label="إعادة"
      results-title="منتجات البراند"
      count-label="عدد المنتجات"
      :empty-text="t('brandPage.empty')"
      @update:sort="(v) => sort = v"
      @reset="resetAll"
    />
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'
import ProductResultsSection from '~/components/ProductResultsSection.vue'

const { t } = useI18n()
const route = useRoute()
const slug = computed(() => String(route.params.slug || '').toLowerCase())
const brands = useBrandsStore()
const products = useProductsStore()
const { buildAssetUrl } = useApi()

const q = ref('')
const sort = ref('new')
const brand = ref<any>(null)

const load = async () => {
  try { brand.value = await brands.getBySlug(slug.value) } catch { brand.value = null }
  await products.fetch({ page: 1, pageSize: 40, q: q.value || undefined, sort: 'new', brand: slug.value })
}
await useAsyncData(`brand-${slug.value}`, async () => { await load(); return true })
watch([q], async () => { await load() })
const brandLogo = computed(() => buildAssetUrl(brand.value?.logoUrl || ''))
const filteredItems = computed(() => sortProducts(products.items || [], sort.value))
function resetAll(){ q.value=''; sort.value='new' }
function sortProducts(items: any[], mode: string) {
  const list=[...items]
  switch(mode){
    case 'oldest': return list.sort((a,b)=>new Date(a.createdAt||0).getTime()-new Date(b.createdAt||0).getTime())
    case 'alphabetical': return list.sort((a,b)=>String(a.name||a.title||'').localeCompare(String(b.name||b.title||''),'ar'))
    case 'priceAsc': return list.sort((a,b)=>Number(a.finalPriceIqd??a.priceIqd??a.price??0)-Number(b.finalPriceIqd??b.priceIqd??b.price??0))
    case 'priceDesc': return list.sort((a,b)=>Number(b.finalPriceIqd??b.priceIqd??b.price??0)-Number(a.finalPriceIqd??a.priceIqd??a.price??0))
    default: return list.sort((a,b)=>new Date(b.createdAt||0).getTime()-new Date(a.createdAt||0).getTime())
  }
}
</script>

<style scoped>
.brand-hero{border-radius:32px;border:1px solid rgba(var(--border), .96);background:linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .9));box-shadow:0 24px 64px rgba(0,0,0,.18)}
.brand-logo-shell{width:104px;height:104px;border-radius:28px;border:1px solid rgba(var(--border), .96);background:linear-gradient(180deg, rgba(var(--surface-rgb), .94), rgba(var(--surface-2-rgb), .86));overflow:hidden;display:flex;align-items:center;justify-content:center;box-shadow:inset 0 1px 0 rgba(255,255,255,.1), 0 18px 44px rgba(0,0,0,.16);flex:0 0 auto}
:global(html.theme-light) .brand-hero{background:linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,246,250,.94));box-shadow:0 22px 56px rgba(232,91,154,.10), 0 10px 24px rgba(17,24,39,.05)}
:global(html.theme-light) .brand-logo-shell{background:linear-gradient(180deg, rgba(255,255,255,.98), rgba(250,244,248,.94))}
@media (max-width:640px){.brand-logo-shell{width:88px;height:88px;border-radius:24px}}
</style>
