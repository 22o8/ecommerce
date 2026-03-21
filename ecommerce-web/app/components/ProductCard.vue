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
        img-class="w-full h-full object-contain p-4 sm:p-5 transition duration-300 group-hover:scale-[1.02]"
      />

      <div class="absolute inset-x-0 bottom-0 h-16 bg-gradient-to-t from-black/10 via-black/0 to-transparent pointer-events-none"></div>

      <div v-if="isOutOfStock" class="absolute top-3 left-3 flex items-center gap-2">
        <div class="px-3 py-1 rounded-full bg-[rgb(var(--danger))]/90 text-white text-[11px] sm:text-xs font-black shadow-lg rtl-text">
          {{ t('common.unavailable') }}
        </div>
      </div>

      <div v-if="discountPercent > 0" class="absolute top-3 right-3">
        <div class="px-3 py-1 rounded-full bg-red-500/95 text-white text-[11px] sm:text-xs font-black keep-ltr shadow-lg">
          -{{ discountPercent }}%
        </div>
      </div>
    </div>

    <div class="product-card-content p-4 sm:p-5">
      <div class="min-w-0">
        <h3 class="product-card-title rtl-text line-clamp-2 min-w-0">
          {{ displayName }}
        </h3>
        <p v-if="displayDescription && !props.compact" class="text-sm text-muted rtl-text line-clamp-2 mt-2">
          {{ displayDescription }}
        </p>
      </div>

      <div class="product-card-bottom">
        <div class="product-card-price">
          <div class="text-xl sm:text-2xl font-black keep-ltr">
            {{ formatPrice(displayFinalPrice) }}
          </div>
          <div v-if="discountPercent > 0" class="text-xs text-muted keep-ltr mt-1">
            <span class="line-through opacity-70">{{ formatPrice(priceValue) }}</span>
          </div>
        </div>

        <div class="product-card-actions relative z-20">
          <button
            type="button"
            class="product-card-btn product-card-btn--secondary"
            :class="props.compact ? 'product-card-btn--compact' : ''"
            @click.stop="addToCart"
            :disabled="isOutOfStock || adding"
          >
            <Icon name="mdi:cart-plus" class="text-base" />
            <span class="rtl-text">{{ t('common.addToCart') }}</span>
          </button>

          <button
            type="button"
            class="product-card-btn product-card-btn--primary"
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
  background: rgb(var(--surface));
  box-shadow: 0 18px 42px rgba(0,0,0,.14);
  display:flex;
  flex-direction:column;
  min-height:100%;
}
.product-card-shell:hover{ transform: translateY(-2px); }
.product-card-media{
  aspect-ratio: 1 / 1;
  background: rgba(0,0,0,.04);
}
.product-card-content{
  display:grid;
  gap:1rem;
  flex:1;
}
.product-card-title{
  font-weight:900;
  font-size: 1rem;
  line-height:1.6;
}
.product-card-bottom{
  display:grid;
  gap:1rem;
  margin-top:auto;
}
.product-card-price{
  display:grid;
  gap:.15rem;
}
.product-card-actions{
  display:grid;
  grid-template-columns: 1fr 1fr;
  gap:.75rem;
}
.product-card-btn{
  min-height:46px;
  border-radius:14px;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.5rem;
  font-weight:800;
  border:1px solid transparent;
  transition:transform .18s ease, opacity .18s ease, box-shadow .18s ease, background .18s ease, color .18s ease;
  padding:.8rem 1rem;
}
.product-card-btn:hover{ transform:translateY(-1px); }
.product-card-btn:disabled{ opacity:.55; cursor:not-allowed; }
.product-card-btn--primary{
  background:#fff;
  color:#111;
  box-shadow:0 12px 22px rgba(0,0,0,.12);
}
.product-card-btn--secondary{
  background:transparent;
  color:#fff;
  border-color:rgba(255,255,255,.22);
}
:global(html.theme-light) .product-card-btn--primary{
  background:#111;
  color:#fff;
}
:global(html.theme-light) .product-card-btn--secondary{
  background:transparent;
  color:#111;
  border-color:rgba(17,17,17,.18);
}
.product-card-shell--compact .product-card-content{ padding:.9rem; gap:.8rem; }
.product-card-shell--compact .product-card-actions{ gap:.55rem; }
.product-card-shell--compact .product-card-btn{
  min-height:42px;
  padding:.72rem .8rem;
  font-size:.92rem;
}
@media (max-width: 640px){
  .product-card-title{ font-size:.96rem; }
  .product-card-content{ gap:.85rem; }
  .product-card-actions{ gap:.55rem; }
  .product-card-btn{ min-height:42px; padding:.72rem .85rem; font-size:.92rem; }
}
</style>
