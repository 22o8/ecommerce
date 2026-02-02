<template>
  <UiModal v-model="open" :title="p?.name || p?.title || ''" maxWidth="900px">
    <div class="grid gap-4 md:grid-cols-[1.1fr_.9fr]">
      <div class="rounded-2xl overflow-hidden bg-surface border border-app">
        <ProductGallery
          :images="galleryImages"
          :alt="p?.name || p?.title || ''"
          :height="420"
        />
      </div>

      <div class="grid gap-3">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0">
            <div class="font-black text-lg rtl-text truncate">
              {{ p?.name || p?.title }}
            </div>

            <div class="mt-1 text-sm text-muted rtl-text">
              {{ p?.description || '' }}
            </div>
          </div>

          <div class="shrink-0 text-right keep-ltr">
            <div class="text-xl font-black">
              {{ formatPrice(p?.priceUsd ?? p?.PriceUsd ?? p?.price ?? p?.Price ?? 0) }}
            </div>
          </div>
        </div>

        <div class="grid gap-2">
          <UiButton class="w-full" @click="addToCart(p)">
            <Icon name="mdi:cart-outline" class="text-lg" />
            <span class="rtl-text">{{ t('productsPage.addToCart') }}</span>
          </UiButton>

          <UiButton variant="secondary" class="w-full" @click="goDetails(p)">
            <Icon name="mdi:open-in-new" class="text-lg" />
            <span class="rtl-text">{{ t('productsPage.viewDetails') }}</span>
          </UiButton>
        </div>
      </div>
    </div>
  </UiModal>
</template>

<script setup lang="ts">
import UiModal from '~/components/ui/UiModal.vue'
import UiButton from '~/components/ui/UiButton.vue'
import ProductGallery from '~/components/ProductGallery.vue'
import { useCartStore } from '~/stores/cart'
import { useApi } from '~/composables/useApi'

const { t } = useI18n()
const router = useRouter()
const cart = useCartStore()
const api = useApi()
const qp = useQuickPreview()

const open = computed({
  get: () => qp.open,
  set: (v) => (qp.open = v),
})

const p = computed(() => qp.product)

const galleryImages = computed(() => {
  const prod: any = p.value
  const imgs: string[] = Array.isArray(prod?.images) ? prod.images : []
  const cover = prod?.imageUrl || prod?.coverImage || prod?.ImageUrl || prod?.CoverImage
  const coverUrl = cover ? api.buildAssetUrl(String(cover)) : ''
  const all = [...(coverUrl ? [coverUrl] : []), ...imgs].filter(Boolean)
  // إزالة التكرار
  return Array.from(new Set(all))
})

function formatPrice(v: number) {
  const n = Number(v || 0)
  return `$${n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function addToCart(prod: any) {
  if (!prod) return
  cart.add(prod)
}

function goDetails(prod: any) {
  if (!prod) return
  // دعم id أو slug
  const id = prod?.id ?? prod?.Id
  const slug = prod?.slug ?? prod?.Slug
  if (slug) router.push(`/products/${slug}`)
  else if (id) router.push(`/products/${id}`)
  else router.push('/products')
  open.value = false
}
</script>
