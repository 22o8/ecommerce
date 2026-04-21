<script setup lang="ts">
import ProductResultsSection from '~/components/ProductResultsSection.vue'
import { useProductsStore } from '~/stores/products'

const { t } = useI18n()
const productsStore = useProductsStore()
const sort = ref('new')
await useAsyncData('discounts-page', async () => { await productsStore.fetchDiscounts(36); return true }, { server: false, default: () => true })
const items = computed(() => sortProducts(productsStore.discountItems || [], sort.value))
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

<template>
  <div class="mx-auto max-w-6xl px-4 py-10">
    <div class="flex items-end justify-between gap-4">
      <div>
        <h1 class="text-2xl sm:text-4xl font-extrabold rtl-text">{{ t('discounts.title') || 'التخفيضات' }}</h1>
        <p class="mt-2 text-sm text-muted rtl-text">{{ t('discounts.subtitle') || 'منتجات عليها خصم حقيقي' }}</p>
      </div>
      <NuxtLink to="/products" class="btn inline-flex items-center rounded-full px-4 py-2 text-sm font-semibold">{{ t('home.viewAll') || 'عرض الكل' }}</NuxtLink>
    </div>

    <ProductResultsSection
      :items="items"
      :loading="false"
      :sort="sort"
      :count="items.length"
      title="فلترة التخفيضات"
      hint="رتّب منتجات التخفيضات حسب الوقت أو الاسم أو السعر."
      sort-label="الترتيب"
      clear-label="إعادة"
      results-title="منتجات التخفيضات"
      count-label="عدد المنتجات"
      :empty-text="t('discounts.empty') || 'حالياً ماكو منتجات عليها تخفيض.'"
      @update:sort="(v) => sort = v"
      @reset="sort = 'new'"
    />
  </div>
</template>
