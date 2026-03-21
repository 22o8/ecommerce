<template>
  <article
    role="button"
    tabindex="0"
    class="group relative product-card-shell overflow-hidden rounded-[1.6rem] border border-app transition duration-300 select-none"
    :class="props.compact ? 'product-card-shell--compact' : ''"
    @click="goProduct"
    @keydown.enter.prevent="goProduct"
    @keydown.space.prevent="goProduct"
  >
    <div class="relative product-card-media">
      <SmartImage
        :src="mainImage || ''"
        :alt="displayName"
        fit="contain"
        wrapper-class="w-full h-full"
        img-class="w-full h-full object-contain p-3 sm:p-4 transition duration-300 group-hover:scale-[1.03]"
      />

      <div class="absolute inset-x-0 bottom-0 h-14 bg-gradient-to-t from-black/10 via-black/0 to-transparent pointer-events-none"></div>

      <div class="absolute top-3 left-3 flex items-center gap-2">
        <div
          v-if="isOutOfStock"
          class="px-3 py-1 rounded-full bg-[rgb(var(--danger))]/90 text-white text-[11px] sm:text-xs font-black shadow-lg rtl-text"
        >
          {{ t('common.unavailable') }}
        </div>
      </div>

      <div v-if="discountPercent > 0" class="absolute top-3 right-3">
        <div class="px-3 py-1 rounded-full bg-red-500/95 text-white text-[11px] sm:text-xs font-black keep-ltr shadow-lg">
          -{{ discountPercent }}%
        </div>
      </div>
    </div>

    <div class="product-card-content p-3 sm:p-4">
      <div class="min-w-0">
        <h3 class="product-card-title rtl-text line-clamp-2 min-w-0">
          {{ displayName }}
        </h3>
        <p v-if="displayDescription && !props.compact" class="hidden sm:block text-sm text-muted rtl-text line-clamp-2 mt-1.5">
          {{ displayDescription }}
        </p>
      </div>

      <div class="product-card-top-actions relative z-20">
        <button
          type="button"
          class="product-card-icon-btn"
          @click.stop="toggleFav"
          :aria-label="t('wishlist.toggle')"
        >
          <Icon :name="fav ? 'mdi:heart' : 'mdi:heart-outline'" class="text-[1.05rem] sm:text-lg" />
        </button>

        <button
          type="button"
          class="product-card-icon-btn"
          @click.stop="openPreview"
          :aria-label="t('products.quickPreview')"
        >
          <Icon name="mdi:eye-outline" class="text-[1.05rem] sm:text-lg" />
        </button>
      </div>

      <div class="product-card-bottom">
        <div class="product-card-price">
          <div class="text-lg sm:text-xl font-black keep-ltr">
            {{ formatPrice(displayFinalPrice) }}
          </div>
          <div v-if="discountPercent > 0" class="text-xs text-muted keep-ltr mt-1">
            <span class="line-through opacity-70">{{ formatPrice(priceValue) }}</span>
          </div>
        </div>

        <div class="product-card-actions relative z-20">
          <button
            type="button"
            class="product-card-btn product-card-btn--main"
            :class="props.compact ? 'product-card-btn--compact' : ''"
            @click.stop="addToCart"
            :disabled="isOutOfStock || adding"
          >
            <Icon name="mdi:cart-plus" class="text-base" />
            <span class="rtl-text">{{ t('common.addToCart') }}</span>
          </button>

          <button
            type="button"
            class="product-card-btn product-card-btn--main"
            :class="props.compact ? 'product-card-btn--compact' : ''"
            @click.stop="buyNow"
            :disabled="isOutOfStock || buying"
          >
            <Icon name="mdi:flash" class="text-base" />
            <span class="rtl-text">{{ t('common.buy') }}</span>
          </button>
        </div>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'
import { formatIqd } from '~/composables/useMoney'

const props = defineProps<{ product?: any; p?: any; compact?: boolean }>()
const { t } = useI18n()
const cart = useCartStore()
const auth = useAuthStore()
const toast = useToast()
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


const wishlistKey = computed(() => String((p.value as any)?.id ?? (p.value as any)?.productId ?? p.value?.slug ?? ''))
const fav = computed(() => isInWishlist(wishlistKey.value))
const adding = ref(false)
const buying = ref(false)

function formatPrice(v: any) {
  return formatIqd(v)
}

function addToCart() {
  if (isOutOfStock.value || adding.value) return
  adding.value = true
  try {
    cart.add(p.value)
  } finally {
    setTimeout(() => { adding.value = false }, 220)
  }
}

const { checkoutSingleProduct } = useWhatsappCheckout()

async function buyNow() {
  if (isOutOfStock.value || buying.value) return
  buying.value = true
  try {
    await checkoutSingleProduct(p.value, 1)
  } catch {
    cart.add(p.value)
    navigateTo('/cart')
  } finally {
    setTimeout(() => { buying.value = false }, 250)
  }
}

async function toggleFav() {
  if (!auth.isAuthed) {
    toast.error('يجب تسجيل الدخول أولاً')
    return
  }
  try {
    await toggle(wishlistKey.value)
    toast.success(fav.value ? 'تمت إزالة المنتج من المفضلة' : 'تمت إضافة المنتج إلى المفضلة')
  } catch (e:any) {
    toast.error(e?.data?.message || e?.message || 'تعذر تحديث المفضلة')
  }
}

function openPreview() {
  qp.show(p.value)
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
  background: rgb(var(--surface));
  box-shadow: 0 20px 46px rgba(0,0,0,.18);
  display:flex;
  flex-direction:column;
  min-height:100%;
}
.product-card-shell:hover{ transform: translateY(-2px); }
.product-card-media{
  aspect-ratio: 1 / 1;
  background: rgba(0,0,0,.06);
}
.product-card-content{
  display:grid;
  gap:.8rem;
  flex:1;
}
.product-card-title{
  font-weight:900;
  font-size: .98rem;
  line-height:1.55;
}
.product-card-top-actions{
  display:flex;
  justify-content:flex-end;
  gap:.6rem;
}
.product-card-icon-btn{
  width:46px;
  height:46px;
  border-radius:999px;
  border:1px solid rgba(var(--border), .95);
  display:inline-flex;
  align-items:center;
  justify-content:center;
  background:rgba(255,255,255,.06);
  color:#fff;
  box-shadow:0 12px 24px rgba(0,0,0,.14);
  transition:transform .18s ease, box-shadow .18s ease, background .18s ease, color .18s ease;
}
.product-card-icon-btn:hover{ transform:translateY(-1px); }
.product-card-bottom{
  display:grid;
  gap:.8rem;
  margin-top:auto;
}
.product-card-actions{
  display:grid;
  grid-template-columns:repeat(2, minmax(0, 1fr));
  gap:.6rem;
}
.product-card-btn{
  min-height:48px;
  border-radius:999px;
  border:1px solid rgba(var(--border), .95);
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.45rem;
  padding:.78rem .85rem;
  font-weight:900;
  font-size:.84rem;
  transition:transform .18s ease, box-shadow .18s ease, opacity .18s ease, background .18s ease;
  white-space:nowrap;
}
.product-card-btn--main{
  background:#fff;
  color:#0f1117;
  border-color:rgba(255,255,255,.24);
  box-shadow:0 14px 30px rgba(0,0,0,.16);
}
.product-card-btn--main:hover{ transform:translateY(-1px); box-shadow:0 18px 34px rgba(0,0,0,.20); }
.product-card-btn:disabled{ opacity:.5; cursor:not-allowed; }
.product-card-shell--compact{ border-radius:1.25rem; }
.product-card-shell--compact .product-card-title{ font-size:.88rem; line-height:1.45; }
.product-card-shell--compact .product-card-content{ padding:.75rem; gap:.65rem; }
.product-card-shell--compact .product-card-btn{ min-height:44px; padding:.68rem .72rem; font-size:.78rem; }
.product-card-shell--compact .product-card-icon-btn{ width:42px; height:42px; }

:global(html.theme-light) .product-card-shell{
  background:linear-gradient(180deg, rgba(255,255,255,.99), rgba(255,247,252,.96));
  box-shadow:0 20px 48px rgba(24,24,24,.06), 0 10px 24px rgba(232,91,154,.08);
}
:global(html.theme-light) .product-card-media{
  background:linear-gradient(180deg, rgba(252,248,251,.95), rgba(245,239,245,.9));
}
:global(html.theme-light) .product-card-icon-btn{
  background:#111319;
  color:#fff;
  border-color:rgba(17,19,25,.15);
  box-shadow:0 14px 30px rgba(17,19,25,.12);
}
:global(html.theme-light) .product-card-btn--main{
  background:#111319;
  color:#fff;
  border-color:rgba(17,19,25,.14);
  box-shadow:0 14px 30px rgba(17,19,25,.14);
}
:global(html.theme-light) .product-card-btn--main:hover,
:global(html.theme-light) .product-card-icon-btn:hover{
  background:#0b0d12;
}
:global(html.theme-dark) .product-card-icon-btn{
  background:#fff;
  color:#0f1117;
  border-color:rgba(255,255,255,.14);
}
:global(html.theme-dark) .product-card-btn--main{
  background:#fff;
  color:#0f1117;
  border-color:rgba(255,255,255,.18);
}

@media (max-width: 640px){
  .product-card-shell{ border-radius:1.35rem; }
  .product-card-content{ gap:.7rem; }
  .product-card-title{ font-size:.9rem; line-height:1.45; }
  .product-card-top-actions{ justify-content:flex-end; }
  .product-card-icon-btn{ width:42px; height:42px; }
  .product-card-actions{ grid-template-columns:repeat(2, minmax(0,1fr)); gap:.5rem; }
  .product-card-btn{ min-height:44px; font-size:.8rem; padding:.7rem .5rem; }
  .product-card-content{ padding:.85rem; gap:.7rem; }
  .product-card-title{ font-size:.9rem; line-height:1.5; }
  .product-card-top-actions{ gap:.5rem; }
  .product-card-icon-btn{ width:44px; height:44px; }
  .product-card-price{ display:flex; align-items:flex-end; justify-content:space-between; gap:.75rem; }
  .product-card-btn{ min-height:46px; font-size:.82rem; padding:.72rem .55rem; }
}
</style>
