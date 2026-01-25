<script setup lang="ts">
import UiCard from '~/components/ui/UiCard.vue'
import UiButton from '~/components/ui/UiButton.vue'
import { useI18n } from '~/app/composables/useI18n'
import { useCartStore } from '~/app/stores/cart'

const { t } = useI18n()
const cart = useCartStore()

const props = defineProps<{
  product: {
    id: string
    title: string
    slug: string
    priceUsd: number
    ratingAvg?: number | null
    ratingCount?: number | null
    images?: Array<{ id: string; url: string; alt?: string | null }>
  }
}>()

const firstImage = computed(() => props.product.images?.[0]?.url || '')

function addToCart(e: Event) {
  e.preventDefault()
  e.stopPropagation()
  cart.add({
    id: props.product.id,
    slug: props.product.slug,
    title: props.product.title,
    priceUsd: props.product.priceUsd,
    image: firstImage.value,
  }, 1)
}
</script>

<template>
  <UiCard class="group overflow-hidden">
    <NuxtLink :to="`/products/${product.slug}`" class="block">
      <div class="aspect-[4/3] bg-black/5 dark:bg-white/5 overflow-hidden">
        <img v-if="firstImage" :src="firstImage" :alt="product.title" class="h-full w-full object-cover group-hover:scale-[1.03] transition" />
        <div v-else class="h-full w-full grid place-items-center text-muted">
          <Icon name="mdi:image-outline" class="text-3xl" />
        </div>
      </div>

      <div class="p-4">
        <div class="font-semibold rtl-text line-clamp-2">{{ product.title }}</div>
        <div class="mt-2 flex items-center justify-between">
          <div class="text-lg font-extrabold keep-ltr">${{ Number(product.priceUsd).toFixed(2) }}</div>
          <div v-if="product.ratingCount" class="text-sm text-muted keep-ltr">
            ★ {{ Number(product.ratingAvg || 0).toFixed(1) }} ({{ product.ratingCount }})
          </div>
        </div>
      </div>
    </NuxtLink>

    <div class="px-4 pb-4">
      <UiButton class="w-full" @click="addToCart">
        <Icon name="mdi:cart-plus" class="text-lg" />
        <span class="rtl-text">{{ t('cart.add') }}</span>
      </UiButton>
    </div>
  </UiCard>
</template>
