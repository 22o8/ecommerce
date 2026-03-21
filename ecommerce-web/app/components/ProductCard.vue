<template>
  <article
    role="button"
    tabindex="0"
    class="group product-card-shell"
    :class="props.compact ? 'product-card-shell--compact' : ''"
    @click="goProduct"
    @keydown.enter.prevent="goProduct"
    @keydown.space.prevent="goProduct"
  >
    <div class="product-card-media-wrap">
      <div class="product-card-media">
        <SmartImage
          :src="mainImage || ''"
          :alt="displayName"
          fit="contain"
          wrapper-class="w-full h-full"
          img-class="w-full h-full object-contain p-5 sm:p-6 transition duration-300 group-hover:scale-[1.02]"
        />
      </div>
    </div>

    <div class="product-card-content">
      <div class="product-card-heading">
        <h3 class="product-card-title rtl-text line-clamp-1 min-w-0">{{ displayName }}</h3>
        <div class="product-card-price-line keep-ltr">{{ formatPrice(displayFinalPrice) }}</div>
      </div>

      <p v-if="displayDescription" class="product-card-desc rtl-text line-clamp-2">
        {{ displayDescription }}
      </p>

      <div class="product-card-actions" @click.stop>
        <button
          type="button"
          class="product-card-btn product-card-btn--cart"
          :class="props.compact ? 'product-card-btn--compact' : ''"
          @click.stop="addToCart"
          :disabled="isOutOfStock || adding"
        >
          <span class="rtl-text">{{ t('common.addToCart') }}</span>
        </button>

        <button
          type="button"
          class="product-card-btn product-card-btn--buy"
          :class="props.compact ? 'product-card-btn--compact' : ''"
          @click.stop="buyNow"
          :disabled="isOutOfStock || buying"
        >
          <span class="rtl-text">{{ t('common.buy') }}</span>
        </button>
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

function goProduct() {
  const id = String(p.value?.id ?? '')
  if (!id) return
  navigateTo(`/product/${id}`)
}
</script>

<style scoped>
.product-card-shell{
  display:flex;
  flex-direction:column;
  min-height:100%;
  border-radius: 1.5rem;
  border: 1px solid rgba(var(--border), .72);
  background: rgb(var(--surface));
  box-shadow: 0 18px 42px rgba(0,0,0,.10);
  padding: 1rem;
  overflow: hidden;
  transition: transform .18s ease, box-shadow .18s ease, border-color .18s ease;
}
.product-card-shell:hover{
  transform: translateY(-3px);
  box-shadow: 0 22px 52px rgba(0,0,0,.14);
  border-color: rgba(var(--border), .96);
}
.product-card-media-wrap{ width:100%; }
.product-card-media{
  aspect-ratio: 1 / 1.18;
  border-radius: 1.15rem;
  overflow: hidden;
  background: rgba(127,127,127,.08);
}
.product-card-content{
  display:grid;
  gap:.8rem;
  padding-top: 1rem;
}
.product-card-heading{
  display:grid;
  grid-template-columns: minmax(0,1fr) auto;
  gap: .8rem;
  align-items:start;
}
.product-card-title{
  font-weight: 600;
  font-size: 1.02rem;
  line-height: 1.45;
  color: rgb(var(--text-strong));
}
.product-card-price-line{
  font-weight: 600;
  font-size: 1rem;
  white-space: nowrap;
  color: rgb(var(--text-strong));
}
.product-card-desc{
  font-size: .92rem;
  line-height: 1.55;
  color: rgb(var(--text-soft));
}
.product-card-actions{
  display:grid;
  grid-template-columns: 1fr 1fr;
  gap: .75rem;
  margin-top: .1rem;
}
.product-card-btn{
  min-height: 46px;
  border-radius: .95rem;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  font-weight: 700;
  border: 1px solid transparent;
  transition: transform .18s ease, opacity .18s ease, box-shadow .18s ease, background .18s ease, color .18s ease, border-color .18s ease;
  padding: .72rem 1rem;
}
.product-card-btn:hover{ transform: translateY(-1px); }
.product-card-btn:disabled{ opacity:.55; cursor:not-allowed; }
.product-card-btn--cart,
.product-card-btn--buy{
  background: #ffffff;
  color: #111111;
  border-color: rgba(255,255,255,.9);
  box-shadow: 0 10px 22px rgba(255,255,255,.10);
}
:global(html.theme-light) .product-card-btn--cart,
:global(html.theme-light) .product-card-btn--buy{
  background: #111111;
  color: #ffffff;
  border-color: rgba(17,17,17,.9);
  box-shadow: 0 12px 26px rgba(17,17,17,.10);
}
.product-card-shell--compact{ padding:.92rem; border-radius:1.35rem; }
.product-card-shell--compact .product-card-media{ aspect-ratio: 1 / 1.12; }
.product-card-shell--compact .product-card-content{ gap:.66rem; padding-top:.82rem; }
.product-card-shell--compact .product-card-title{ font-size:.98rem; }
.product-card-shell--compact .product-card-price-line{ font-size:.96rem; }
.product-card-shell--compact .product-card-btn{ min-height:42px; padding:.62rem .8rem; font-size:.92rem; }
@media (max-width: 640px){
  .product-card-shell{ padding:.85rem; border-radius:1.3rem; }
  .product-card-media{ aspect-ratio: 1 / 1.08; border-radius:1rem; }
  .product-card-title{ font-size:.94rem; }
  .product-card-price-line{ font-size:.94rem; }
  .product-card-desc{ font-size:.83rem; }
  .product-card-actions{ gap:.55rem; }
  .product-card-btn{ min-height:40px; padding:.62rem .7rem; font-size:.88rem; }
}
</style>
