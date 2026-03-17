<template>
  <!-- الضغط على الكارد يفتح صفحة المنتج (والـ Quick Preview عبر زر العين) -->
  <div
    role="button"
    tabindex="0"
    class="group relative product-card-shell overflow-hidden transition duration-300 will-change-transform hover:-translate-y-1 touch-manipulation select-none"
    @click="goProduct"
    @keydown.enter.prevent="goProduct"
    @keydown.space.prevent="goProduct"
  >
    <div class="relative">
      <div class="relative aspect-[4/3] product-card-media">
        <SmartImage
          :src="mainImage || ''"
          :alt="displayName"
          fit="cover"
          wrapper-class="w-full h-full"
          img-class="w-full h-full object-cover transition duration-500 group-hover:scale-110"
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

    <div class="p-4 grid gap-3">
      <div class="min-w-0">
        <div class="flex items-start justify-between gap-3">
          <div class="font-extrabold line-clamp-1 rtl-text min-w-0">{{ displayName }}</div>
        </div>
        <div v-if="displayDescription" class="text-sm text-muted line-clamp-2 rtl-text">
          {{ displayDescription }}
        </div>

        <!-- أزرار (مفضلة/معاينة) -->
        <div class="relative z-20 flex items-center justify-end gap-2 mt-2 touch-manipulation">
          <button
            type="button"
            class="rounded-full border border-app bg-[rgba(var(--surface),.72)] hover:bg-[rgba(var(--surface),.95)] transition p-2"
            @click.stop.prevent="toggleFav"
            :aria-label="t('wishlist.toggle')"
          >
            <Icon :name="fav ? 'mdi:heart' : 'mdi:heart-outline'" class="text-lg" />
          </button>

          <button
            type="button"
            class="rounded-full border border-app bg-[rgba(var(--surface),.72)] hover:bg-[rgba(var(--surface),.95)] transition p-2"
            @click.stop.prevent="openPreview"
            :aria-label="t('products.quickPreview')"
          >
            <Icon name="mdi:eye-outline" class="text-lg" />
          </button>
        </div>
      </div>

      <div class="flex items-center justify-between gap-3">
        <div class="min-w-0">
          <div class="text-lg font-black keep-ltr">
            {{ formatPrice(displayFinalPrice) }}
          </div>
          <div v-if="discountPercent > 0" class="text-xs text-muted keep-ltr">
            <span class="line-through opacity-70">{{ formatPrice(priceValue) }}</span>
          </div>
        </div>

        <div class="relative z-20 flex flex-row items-center gap-2 touch-manipulation">
          <button
            type="button"
            class="product-card-btn inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-xl border border-app transition text-xs"
            @click.stop.prevent="addToCart"
            :disabled="isOutOfStock"
          >
            <Icon name="mdi:cart-plus" class="text-base" />
            <span class="rtl-text">{{ t('common.addToCart') }}</span>
          </button>

          <button
            type="button"
            class="product-card-btn inline-flex items-center px-2.5 py-1.5 rounded-xl border border-app transition text-xs"
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

const props = defineProps<{ product?: any; p?: any }>()
const { t } = useI18n()
const cart = useCartStore()
const { isInWishlist, toggle } = useWishlist()
const qp = useQuickPreview()
const router = useRouter()
const route = useRoute()
const toast = useToast()
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
  toast.success('تمت إضافة المنتج إلى السلة')
}

const { checkoutSingleProduct } = useWhatsappCheckout()

async function buyNow() {
  if (isOutOfStock.value) return
  try {
    await checkoutSingleProduct(p.value, 1)
  } catch (e) {
    cart.add(p.value)
    await router.push('/cart')
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

async function goProduct() {
  const id = String(p.value?.id ?? '')
  if (!id) return
  await router.push(`/product/${id}`)
  if (import.meta.client) window.scrollTo({ top: 0, left: 0, behavior: 'auto' })
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
</style>
