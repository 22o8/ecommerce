<template>
  <teleport to="body">
    <div v-if="open" class="fixed inset-0 z-[90]">
      <div class="absolute inset-0 bg-black/60" @click="close" />
      <div class="absolute inset-0 flex items-center justify-center p-4">
        <div class="card-soft w-full max-w-5xl overflow-hidden">
          <div class="flex items-center justify-between gap-3 border-b border-app px-4 py-3">
            <div class="font-extrabold rtl-text truncate">{{ p?.name || '' }}</div>
            <button class="btn-soft px-3 py-2" @click="close">
              <Icon name="mdi:close" class="text-xl" />
            </button>
          </div>

          <div class="grid gap-4 p-4 md:grid-cols-2">
            <ProductGallery :images="images" :title="p?.name" />

            <div class="grid gap-4">
              <div class="flex items-center justify-between gap-3">
                <div class="text-2xl font-black keep-ltr">{{ priceText }}</div>
                <UiBadge v-if="p?.isFeatured" class="keep-ltr">Featured</UiBadge>
              </div>

              <div class="text-sm text-muted rtl-text" v-if="p?.description">{{ p.description }}</div>

              <div class="flex flex-wrap gap-2">
                <UiButton @click="goTo">
                  <Icon name="mdi:open-in-new" class="text-lg" />
                  <span class="rtl-text">{{ t('productsPage.viewDetails') }}</span>
                </UiButton>
                <UiButton variant="secondary" @click="addToCart">
                  <Icon name="mdi:cart-plus" class="text-lg" />
                  <span class="rtl-text">{{ t('productsPage.addToCart') }}</span>
                </UiButton>
                <UiButton variant="ghost" @click="toggleFav">
                  <Icon :name="isFav ? 'mdi:heart' : 'mdi:heart-outline'" class="text-lg" />
                  <span class="rtl-text">{{ isFav ? t('wishlist.remove') : t('wishlist.add') }}</span>
                </UiButton>
              </div>

              <div class="card-soft p-4">
                <ProductReviews :product-id="p?.id" />
              </div>

              <div class="card-soft p-4">
                <div class="font-bold rtl-text mb-2">{{ t('whatsapp.title') }}</div>
                <a
                  class="btn-soft inline-flex items-center gap-2 px-4 py-2"
                  :href="waLink"
                  target="_blank"
                  rel="noopener"
                >
                  <Icon name="mdi:whatsapp" class="text-xl" />
                  <span class="rtl-text">{{ t('whatsapp.cta') }}</span>
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </teleport>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiBadge from '~/components/ui/UiBadge.vue'
import ProductReviews from '~/components/ProductReviews.vue'
import ProductGallery from '~/components/ProductGallery.vue'
import { useCartStore } from '~/stores/cart'
import { useWishlist } from '~/composables/useWishlist'
import { useQuickPreview } from '~/composables/useQuickPreview'

const { t } = useI18n()
const router = useRouter()

const cart = useCartStore()
const wl = useWishlist()
const qp = useQuickPreview()

const open = computed(() => qp.open.value)
const p = computed<any>(() => qp.product.value)

const images = computed(() => {
  const arr: string[] = []
  if (p.value?.images?.length) arr.push(...p.value.images.map((x: any) => x.url || x))
  if (p.value?.imageUrl) arr.unshift(p.value.imageUrl)
  // de-dupe
  return [...new Set(arr.filter(Boolean))]
})

const priceText = computed(() => {
  const v = p.value?.priceUsd ?? p.value?.price ?? 0
  const n = Number(v)
  return isFinite(n) ? `$${n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` : String(v)
})

const isFav = computed(() => (p.value?.id ? wl.has(String(p.value.id)) : false))

const waLink = computed(() => {
  const name = p.value?.name || p.value?.title || ''
  const text = encodeURIComponent(`${t('whatsapp.messagePrefix')} ${name}`)
  // رقم افتراضي — المستخدم يبدله من الإعدادات/الـ env لاحقاً
  const phone = (p.value?.whatsappPhone || '').replace(/\D/g, '')
  const target = phone ? `https://wa.me/${phone}` : 'https://wa.me/'
  return `${target}?text=${text}`
})

function close() {
  qp.close()
}

function goTo() {
  if (!p.value) return
  close()
  const slug = p.value.slug || p.value.id
  router.push(`/products/${slug}`)
}

function addToCart() {
  if (!p.value) return
  cart.add(p.value)
}

function toggleFav() {
  if (!p.value?.id) return
  wl.toggle(String(p.value.id))
}
</script>

<style scoped>
.btn-soft {
  @apply rounded-2xl border border-white/15 px-3 py-2 transition hover:bg-white/5;
  background-color: rgba(255, 255, 255, 0.04);
}

</style>
