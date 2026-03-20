<template>
  <!-- الضغط على الكارد يفتح صفحة المنتج (والـ Quick Preview عبر زر العين) -->
  <div
    role="button"
    tabindex="0"
    class="group relative product-card-shell overflow-hidden rounded-2xl transition duration-300 will-change-transform hover:-translate-y-1 touch-manipulation select-none" :class="props.compact ? 'product-card-shell--compact' : ''"
    @click="goProduct"
    @keydown.enter.prevent="goProduct"
    @keydown.space.prevent="goProduct"
  >
    <div class="relative">
      <div class="relative aspect-[4/3] product-card-media">
        <SmartImage
          :src="mainImage || ''"
          :alt="displayName"
          fit="contain"
          wrapper-class="w-full h-full"
          img-class="w-full h-full object-contain p-3 transition duration-300 group-hover:scale-105"
        />

        <div class="absolute inset-0 pointer-events-none opacity-0 group-hover:opacity-100 transition">
          <div class="absolute inset-0 bg-gradient-to-t from-black/40 via-black/0 to-black/0" />
        </div>
      </div>

      <div class="absolute top-3 left-3 flex items-center gap-2">
        <div
          v-if="isOutOfStock"
          class="px-3 py-1 rounded-full bg-[rgb(var(--danger))]/90 text-white text-xs font-black shadow-lg rtl-text"
        >
          {{ t('common.unavailable') }}
        </div>
        <div
          v-if="isNew"
          class="px-3 py-1 rounded-full bg-surface-2 backdrop-blur border border-app text-xs"
        >
          <span class="rtl-text">{{ t('common.new') }}</span>
        </div>
      </div>

      <!-- Discount badge -->
      <div v-if="discountPercent > 0" class="absolute top-3 right-3">
        <div class="px-3 py-1 rounded-full bg-red-500/90 text-white text-xs font-black keep-ltr shadow-lg">
          -{{ discountPercent }}%
        </div>
      </div>
    </div>

    <div class="p-2.5 sm:p-4 grid gap-2 sm:gap-3" :class="props.compact ? 'product-card-content--compact' : ''">
      <div class="min-w-0">
        <div class="flex items-start justify-between gap-3">
          <div class="font-extrabold line-clamp-2 rtl-text min-w-0 text-sm sm:text-base leading-6" :class="props.compact ? 'text-[13px] sm:text-sm leading-5' : ''">{{ displayName }}</div>
        </div>
        <div v-if="displayDescription && !props.compact" class="hidden sm:block text-sm text-muted line-clamp-2 rtl-text">
          {{ displayDescription }}
        </div>

        <!-- أزرار (مفضلة/معاينة) -->
        <div class="relative z-20 flex items-center justify-end gap-2 mt-2 touch-manipulation">
          <button
            type="button"
            class="rounded-full border border-app bg-[rgba(var(--surface),.72)] hover:bg-[rgba(var(--surface),.95)] transition p-1.5 sm:p-2"
            @pointerdown.stop
            @touchstart.stop
            @touchend.stop.prevent
            @click.stop.prevent="toggleFav"
            :aria-label="t('wishlist.toggle')"
          >
            <Icon :name="fav ? 'mdi:heart' : 'mdi:heart-outline'" class="text-base sm:text-lg" />
          </button>

          <button
            type="button"
            class="rounded-full border border-app bg-[rgba(var(--surface),.72)] hover:bg-[rgba(var(--surface),.95)] transition p-1.5 sm:p-2"
            @pointerdown.stop
            @touchstart.stop
            @touchend.stop.prevent
            @click.stop.prevent="openPreview"
            :aria-label="t('products.quickPreview')"
          >
            <Icon name="mdi:eye-outline" class="text-base sm:text-lg" />
          </button>
        </div>
      </div>

      <div class="flex items-center justify-between gap-3">
        <div class="min-w-0">
          <div class="text-base sm:text-lg font-black keep-ltr" :class="props.compact ? 'text-sm sm:text-base' : ''">
            {{ formatPrice(displayFinalPrice) }}
          </div>
          <div v-if="discountPercent > 0" class="text-xs text-muted keep-ltr">
            <span class="line-through opacity-70">{{ formatPrice(priceValue) }}</span>
          </div>
        </div>

        <div class="relative z-20 flex flex-row items-center gap-1.5 sm:gap-2 touch-manipulation">
          <button
            type="button"
            class="product-card-btn inline-flex items-center justify-center gap-1 px-2 py-1.5 rounded-xl border border-app transition text-[11px] sm:text-xs min-w-0 flex-1" :class="props.compact ? 'product-card-btn--compact' : ''"
            @pointerdown.stop
            @touchstart.stop
            @touchend.stop.prevent
            @click.stop.prevent="addToCart"
            :disabled="isOutOfStock"
          >
            <Icon name="mdi:cart-plus" class="text-base" />
            <span class="rtl-text">{{ t('common.addToCart') }}</span>
          </button>

          <button
            type="button"
            class="product-card-btn inline-flex items-center justify-center px-2 py-1.5 rounded-xl border border-app transition text-[11px] sm:text-xs min-w-0" :class="props.compact ? 'product-card-btn--compact' : ''"
            @pointerdown.stop
            @touchstart.stop
            @touchend.stop.prevent
            @click.stop.prevent="buyNow"
            :disabled="isOutOfStock"
          >
            <span class="rtl-text">{{ t('common.buy') }}</span>
          </button>
        </div>
      </div>
    </div>
    <div class="pointer-events-none absolute inset-x-6 -bottom-px h-px bg-gradient-to-r from-transparent via-[rgb(var(--primary))]/55 to-transparent opacity-0 transition group-hover:opacity-100"></div>
</div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'
import { formatIqd } from '~/composables/useMoney'

const props = defineProps<{ product?: any; p?: any; compact?: boolean }>()
const { t } = useI18n()
const cart = useCartStore()
const { isInWishlist, toggle } = useWishlist()
const qp = useQuickPreview()
const router = useRouter()
const route = useRoute()
const { buildAssetUrl } = useApi()

const p = computed(() => props.product ?? props.p ?? {})

const displayName = computed(() => String(p.value?.title ?? p.value?.name ?? ''))
const displayDescription = computed(() => String(p.value?.description ?? '') || '')

const priceValue = computed(() => p.value?.priceIqd ?? p.value?.price ?? p.value?.priceUsd)
const discountPercent = computed(() => Number(p.value?.discountPercent ?? 0))
const displayFinalPrice = computed(() => {
  const v = p.value?.finalPriceIqd
  if (v != null) return v
  const price = Number(priceValue.value ?? 0)
  const d = Number(discountPercent.value ?? 0)
  return d > 0 ? (price * (100 - d) / 100) : price
})

const isOutOfStock = computed(() => Number(p.value?.stockQuantity ?? p.value?.StockQuantity ?? 0) <= 0)

const mainImage = computed(() => {
  const raw =
    p.value?.images?.[0]?.url ||
    p.value?.images?.[0] ||
    p.value?.coverImage ||
    p.value?.imageUrl ||
    p.value?.image ||
    ''
  const resolved = raw ? buildAssetUrl(String(raw)) : ''
  return resolved || '/hero-placeholder.svg'
})

const isNew = computed(() => {
  const raw = p.value?.createdAt ?? p.value?.CreatedAt ?? p.value?.created_on ?? p.value?.createdOn
  const created = raw ? new Date(raw).getTime() : 0
  if (!created || Number.isNaN(created)) return false
  const days = (Date.now() - created) / (1000 * 60 * 60 * 24)
  return days >= 0 && days < 30
})

const wishlistKey = computed(() => String((p.value as any)?.id ?? (p.value as any)?.productId ?? p.value?.slug ?? ''))
const fav = computed(() => isInWishlist(wishlistKey.value))

function formatPrice(v: any) {
  return formatIqd(v)
}

function addToCart() {
  if (isOutOfStock.value) return
  cart.add(p.value)
}

const { checkoutSingleProduct } = useWhatsappCheckout()

async function buyNow() {
  if (isOutOfStock.value) return
  try {
    await checkoutSingleProduct(p.value, 1)
  } catch (e) {
    cart.add(p.value)
    navigateTo('/cart')
  }
}

function toggleFav() {
  toggle(wishlistKey.value)
}

function openPreview() {
  qp.show(p.value)

  // رابط قابل للمشاركة: نضيف ?p=<id> لنفس الصفحة
  const id = String(p.value?.id ?? p.value?.slug ?? '')
  if (id) {
    const q: Record<string, any> = { ...route.query, p: id }
    router.replace({ query: q })
  }
}

function goProduct() {
  const id = String(p.value?.id ?? '')
  if (!id) return
  navigateTo(`/product/${id}`)
}
</script>


<style scoped>
.product-card-shell{
  border: 1px solid rgba(var(--border), .95);
  background: rgb(var(--surface));
}
.product-card-media{ background: rgba(0,0,0,.08); }
.product-card-btn{
  background: rgb(var(--surface));
}
.product-card-btn:disabled{ opacity:.5; cursor:not-allowed; }
.product-card-shell--compact{ border-radius:18px; }
.product-card-shell--compact .product-card-media{ aspect-ratio: 1 / 1; }
.product-card-content--compact{ gap:.55rem; padding:.75rem; }
.product-card-btn--compact{ padding:.42rem .55rem; font-size:11px; }

:global(html.theme-light) .product-card-shell{
  background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(255,247,252,.95));
  box-shadow: 0 22px 54px rgba(232, 91, 154, .08), 0 10px 24px rgba(24,24,24,.05);
}
:global(html.theme-light) .product-card-shell:hover{
  box-shadow: 0 28px 64px rgba(232, 91, 154, .12), 0 12px 28px rgba(24,24,24,.06);
}
:global(html.theme-light) .product-card-media{
  background: linear-gradient(180deg, rgba(252,248,251,.95), rgba(245,239,245,.9));
}
:global(html.theme-light) .product-card-btn{
  background: rgba(255,255,255,.92);
  box-shadow: inset 0 1px 0 rgba(255,255,255,.7);
}
:global(html.theme-dark) .product-card-shell{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .88));
  box-shadow: 0 18px 50px rgba(0,0,0,.28);
}

@media (max-width: 640px){
  .product-card-shell{ border-radius:20px; }
  .product-card-media{ aspect-ratio: 1 / 1; }
  .product-card-shell :deep(img){ padding:.55rem !important; }
}

</style>
