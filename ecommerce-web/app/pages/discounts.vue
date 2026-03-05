<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'
import RevealOnScroll from '~/components/RevealOnScroll.vue'
import { useProductsStore } from '~/stores/products'

const { t } = useI18n()
const productsStore = useProductsStore()

await useAsyncData(
  'discounts-page',
  async () => {
    await productsStore.fetchDiscounts(36)
    return true
  },
  { server: false, default: () => true }
)

const items = computed(() => productsStore.discountItems)
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-10">
    <div class="flex items-end justify-between gap-4">
      <div>
        <h1 class="text-2xl sm:text-4xl font-extrabold rtl-text">{{ t('discounts.title') || 'التخفيضات' }}</h1>
        <p class="mt-2 text-sm text-muted rtl-text">{{ t('discounts.subtitle') || 'منتجات عليها خصم حقيقي' }}</p>
      </div>
      <NuxtLink to="/products" class="btn inline-flex items-center rounded-full px-4 py-2 text-sm font-semibold">
        {{ t('home.viewAll') || 'عرض الكل' }}
      </NuxtLink>
    </div>

    <div v-if="!items?.length" class="mt-8 rounded-3xl border border-app bg-surface p-8 text-center">
      <div class="text-muted">{{ t('discounts.empty') || 'حالياً ماكو منتجات عليها تخفيض.' }}</div>
    </div>

    <div v-else class="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
      <RevealOnScroll v-for="(p, idx) in items" :key="p.id" :parity="idx % 2">
        <ProductCard :p="p" />
      </RevealOnScroll>
    </div>
  </div>
</template>
