<template>
  <article
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
          :alt="`${displayName} - ${brandName || 'DR SEOUL BEAUTY'}`"
          :title="displayName"
          width="480"
          height="480"
          fit="contain"
          wrapper-class="w-full h-full"
          img-class="w-full h-full object-contain p-5 sm:p-6 transition duration-300 group-hover:scale-[1.04]"
        />

        <div class="product-card-badges">
          <span v-if="discountPercent > 0" class="product-card-badge product-card-badge--discount keep-ltr">
            -{{ discountPercent }}%
          </span>
          <span class="product-card-badge" :class="isOutOfStock ? 'product-card-badge--danger' : 'product-card-badge--ok'">
            {{ stockLabel }}
          </span>
        </div>

        <div class="product-card-view-hint">
          <Icon name="mdi:eye-outline" class="text-base" />
          <span>{{ locale === 'en' ? 'View details' : 'عرض التفاصيل' }}</span>
        </div>
      </div>
    </div>

    <div class="product-card-content">
      <div class="product-card-meta-row">
        <span v-if="brandName" class="product-card-brand rtl-text line-clamp-1">{{ brandName }}</span>
        <span v-if="categoryName" class="product-card-category rtl-text line-clamp-1">{{ categoryName }}</span>
      </div>

      <div class="product-card-heading">
        <h3 class="product-card-title rtl-text line-clamp-2 min-w-0">{{ displayName }}</h3>
        <div class="product-card-price-block">
          <span v-if="discountPercent > 0" class="product-card-old-price keep-ltr">{{ formatPrice(priceValue) }}</span>
          <span class="product-card-price-line keep-ltr">{{ formatPrice(displayFinalPrice) }}</span>
        </div>
      </div>

      <p v-if="displayDescription && !props.compact" class="product-card-desc rtl-text line-clamp-2">
        {{ displayDescription }}
      </p>

      <div class="product-card-actions" @click.stop>
        <button
          type="button"
          :aria-label="`إضافة ${displayName} إلى السلة`"
          class="product-card-btn product-card-btn--cart"
          :class="props.compact ? 'product-card-btn--compact' : ''"
          @click.stop="addToCart"
          :disabled="isOutOfStock || adding"
        >
          <Icon name="mdi:cart-plus" class="text-lg" />
          <span class="rtl-text">{{ t('common.addToCart') }}</span>
        </button>

        <button
          type="button"
          :aria-label="`شراء ${displayName} الآن`"
          class="product-card-btn product-card-btn--buy"
          :class="props.compact ? 'product-card-btn--compact' : ''"
          @click.stop="openBuyConfirm"
          :disabled="isOutOfStock || buying"
        >
          <Icon name="mdi:flash" class="text-lg" />
          <span class="rtl-text">{{ t('common.buy') }}</span>
        </button>
      </div>
    </div>
  </article>

  <Teleport to="body">
    <Transition name="buy-confirm-fade">
      <div
        v-if="showBuyConfirm"
        class="buy-confirm-overlay"
        role="dialog"
        aria-modal="true"
        @click.self="closeBuyConfirm"
      >
        <div class="buy-confirm-card">
          <button type="button" class="buy-confirm-close" aria-label="إغلاق" @click="closeBuyConfirm">
            <Icon name="mdi:close" />
          </button>

          <div class="buy-confirm-icon">
            <Icon name="mdi:cart-check" />
          </div>

          <div class="buy-confirm-title rtl-text">تأكيد الشراء</div>
          <p class="buy-confirm-text rtl-text">
            هل تريد إكمال شراء هذا المنتج الآن؟
          </p>

          <div class="buy-confirm-product">
            <SmartImage :src="mainImage" :alt="displayName" :title="displayName" width="96" height="96" fit="contain" wrapper-class="w-16 h-16 shrink-0" img-class="w-full h-full" />
            <div>
              <b class="rtl-text">{{ displayName }}</b>
              <span class="keep-ltr">{{ formatPrice(displayFinalPrice) }}</span>
            </div>
          </div>

          <div class="buy-confirm-actions">
            <button type="button" class="buy-confirm-btn buy-confirm-btn--ghost" @click="closeBuyConfirm">
              إلغاء
            </button>
            <button type="button" class="buy-confirm-btn buy-confirm-btn--main" :disabled="buying" @click="confirmBuyNow">
              <Icon name="mdi:flash" />
              تأكيد الشراء
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'
import { formatIqd } from '~/composables/useMoney'

const props = defineProps<{ product?: any; p?: any; compact?: boolean }>()
const { t, locale } = useI18n()
const cart = useCartStore()
const { buildAssetUrl } = useApi()

const p = computed(() => props.product ?? props.p ?? {})

const displayName = computed(() => String(p.value?.title ?? p.value?.name ?? ''))
const displayDescription = computed(() => String(p.value?.description ?? '') || '')
const brandName = computed(() => String(p.value?.brandName ?? p.value?.brand?.name ?? p.value?.brand ?? '').trim())
const categoryName = computed(() => String(p.value?.categoryName ?? p.value?.category ?? '').trim())

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
const stockLabel = computed(() => isOutOfStock.value ? (locale.value === 'en' ? 'Out' : 'نافد') : (locale.value === 'en' ? 'In stock' : 'متوفر'))

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
const showBuyConfirm = ref(false)

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

function openBuyConfirm() {
  if (isOutOfStock.value || buying.value) return
  showBuyConfirm.value = true
}

function closeBuyConfirm() {
  if (buying.value) return
  showBuyConfirm.value = false
}

async function confirmBuyNow() {
  if (isOutOfStock.value || buying.value) return
  buying.value = true
  try {
    await checkoutSingleProduct(p.value, 1)
    showBuyConfirm.value = false
  } catch {
    cart.add(p.value)
    showBuyConfirm.value = false
    navigateTo('/cart')
  } finally {
    setTimeout(() => { buying.value = false }, 250)
  }
}

function goProduct() {
  const slug = String(p.value?.slug || p.value?.Slug || p.value?.id || '')
  if (!slug) return
  navigateTo(`/products/${encodeURIComponent(slug)}`)
}
</script>

<style scoped>
.product-card-shell{
  position:relative;
  display:flex;
  flex-direction:column;
  min-height:100%;
  border-radius: 1.65rem;
  border: 1px solid rgba(var(--border), .88);
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .82));
  box-shadow:0 18px 42px rgba(0,0,0,.18), inset 0 1px 0 rgba(255,255,255,.07);
  padding:.72rem;
  overflow:hidden;
  transition: transform .22s ease, opacity .18s ease, border-color .22s ease, box-shadow .22s ease, background .22s ease;
  isolation:isolate;
}
.product-card-shell::before{
  content:'';
  position:absolute;
  inset:auto -22% -30% -22%;
  height:46%;
  background:radial-gradient(circle at 50% 0, rgba(var(--primary), .18), transparent 64%);
  opacity:.8;
  pointer-events:none;
  z-index:-1;
}
.product-card-shell:hover{
  transform: translateY(-5px);
  border-color:rgba(var(--primary), .36);
  box-shadow:0 26px 60px rgba(0,0,0,.24), 0 0 0 1px rgba(var(--primary), .08) inset;
}

.product-card-media-wrap{
  position:relative;
  border-radius:1.25rem;
  background:
    linear-gradient(135deg, rgba(255,255,255,.08), rgba(255,255,255,.02)),
    rgba(var(--surface-2-rgb), .86);
  overflow:hidden;
  border:1px solid rgba(var(--border), .72);
}
.product-card-media{
  position:relative;
  aspect-ratio: 1 / 1.08;
  overflow:hidden;
  background:
    radial-gradient(circle at 22% 16%, rgba(var(--primary), .12), transparent 42%),
    rgba(255,255,255,.03);
}
.product-card-badges{
  position:absolute;
  inset:.7rem .7rem auto .7rem;
  display:flex;
  align-items:center;
  justify-content:space-between;
  gap:.45rem;
  pointer-events:none;
}
.product-card-badge{
  display:inline-flex;
  align-items:center;
  min-height:1.8rem;
  padding:0 .62rem;
  border-radius:999px;
  font-size:.72rem;
  font-weight:1000;
  border:1px solid transparent;
  backdrop-filter:blur(12px);
}
.product-card-badge--discount{
  color:#fff;
  background:rgba(225,29,72,.72);
  border-color:rgba(255,255,255,.18);
}
.product-card-badge--ok{
  color:rgb(34 197 94);
  background:rgba(34,197,94,.14);
  border-color:rgba(34,197,94,.25);
}
.product-card-badge--danger{
  color:rgb(248 113 113);
  background:rgba(248,113,113,.13);
  border-color:rgba(248,113,113,.25);
}
.product-card-view-hint{
  position:absolute;
  inset:auto .8rem .8rem .8rem;
  display:flex;
  align-items:center;
  justify-content:center;
  gap:.4rem;
  min-height:2.35rem;
  border-radius:999px;
  background:rgba(var(--surface-rgb), .74);
  border:1px solid rgba(var(--border), .78);
  color:rgb(var(--text));
  font-size:.78rem;
  font-weight:900;
  opacity:0;
  transform:translateY(8px);
  transition:opacity .2s ease, transform .2s ease;
  backdrop-filter:blur(14px);
}
.product-card-shell:hover .product-card-view-hint{ opacity:1; transform:translateY(0); }

.product-card-content{
  display:grid;
  gap:.72rem;
  padding:.95rem .15rem .1rem;
}
.product-card-meta-row{
  display:flex;
  align-items:center;
  gap:.45rem;
  min-height:1.55rem;
}
.product-card-brand,.product-card-category{
  display:inline-flex;
  max-width:100%;
  padding:.28rem .58rem;
  border-radius:999px;
  font-size:.7rem;
  line-height:1.1;
  font-weight:900;
  border:1px solid rgba(var(--border), .78);
  color:rgb(var(--muted));
  background:rgba(var(--surface-rgb), .72);
}
.product-card-brand{ color:rgb(var(--text)); border-color:rgba(var(--primary), .22); background:rgba(var(--primary), .08); }
.product-card-heading{
  display:grid;
  gap:.6rem;
}
.product-card-title{
  min-height:2.85em;
  font-weight:1000;
  font-size:1.02rem;
  line-height:1.42;
  color: rgb(var(--text-strong));
}
.product-card-price-block{
  display:flex;
  flex-wrap:wrap;
  align-items:baseline;
  gap:.45rem;
}
.product-card-price-line{
  font-weight:1000;
  font-size:1.05rem;
  white-space: nowrap;
  color: rgb(var(--text-strong));
}
.product-card-old-price{
  color:rgb(var(--muted));
  font-size:.82rem;
  font-weight:800;
  text-decoration:line-through;
  opacity:.78;
}
.product-card-desc{
  min-height:3em;
  font-size:.9rem;
  line-height:1.55;
  color: rgb(var(--text-soft));
}
.product-card-actions{
  display:grid;
  grid-template-columns: 1fr 1fr;
  gap: .6rem;
  margin-top: .1rem;
}
.product-card-btn{
  min-height: 44px;
  border-radius: 1rem;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.35rem;
  font-weight:1000;
  border: 1px solid transparent;
  transition: transform .18s ease, opacity .18s ease, box-shadow .18s ease, background .18s ease, color .18s ease, border-color .18s ease;
  padding: .65rem .75rem;
  font-size:.88rem;
}
.product-card-btn:hover{ transform: translateY(-1px); }
.product-card-btn:disabled{ opacity:.55; cursor:not-allowed; }
.product-card-btn--cart{
  background:linear-gradient(135deg, rgb(var(--primary)), rgba(var(--cta-glow-2), .92));
  box-shadow:0 14px 28px rgba(var(--primary), .20);
}
.product-card-btn--buy{
  color:rgb(var(--text));
  background:rgba(var(--surface-rgb), .76);
  border-color:rgba(var(--border), .82);
}
.product-card-btn--buy:hover{
  border-color:rgba(var(--primary), .32);
  background:rgba(var(--primary), .08);
}
.product-card-shell--compact .product-card-media{ aspect-ratio: 1 / 1.05; }
.product-card-shell--compact .product-card-content{ gap:.58rem; padding-top:.75rem; }
.product-card-shell--compact .product-card-title{ font-size:.96rem; }
.product-card-shell--compact .product-card-price-line{ font-size:.96rem; }
.product-card-shell--compact .product-card-btn{ min-height:40px; padding:.58rem .68rem; font-size:.82rem; }

:global(html.theme-light) .product-card-shell{
  background:linear-gradient(180deg, rgba(255,255,255,.995), rgba(255,255,255,.94));
  box-shadow:0 20px 46px rgba(232,91,154,.08), 0 10px 28px rgba(20,20,20,.055);
}
:global(html.theme-light) .product-card-media-wrap{
  background:linear-gradient(135deg, rgba(250,232,255,.88), rgba(255,255,255,.78));
}
:global(html.theme-light) .product-card-btn--cart{ color:#fff; }
:global(html.theme-light) .product-card-btn--buy{ background:#fff; }

@media (max-width: 640px){
  .product-card-shell{ border-radius:1.25rem; padding:.5rem; }
  .product-card-media-wrap{ border-radius:1rem; }
  .product-card-media{ aspect-ratio: 1 / 1.03; }
  .product-card-badges{ inset:.48rem .48rem auto .48rem; }
  .product-card-badge{ min-height:1.55rem; padding:0 .45rem; font-size:.64rem; }
  .product-card-view-hint{ display:none; }
  .product-card-content{ gap:.55rem; padding:.7rem .05rem .05rem; }
  .product-card-meta-row{ min-height:1.25rem; }
  .product-card-brand,.product-card-category{ font-size:.62rem; padding:.22rem .42rem; }
  .product-card-title{ font-size:.88rem; min-height:2.7em; }
  .product-card-price-line{ font-size:.9rem; }
  .product-card-old-price{ font-size:.72rem; }
  .product-card-desc{ font-size:.78rem; min-height:2.6em; }
  .product-card-actions{ gap:.42rem; }
  .product-card-btn{ min-height:38px; padding:.52rem .5rem; font-size:.76rem; border-radius:.82rem; }
  .product-card-btn :deep(svg){ display:none; }
}


.buy-confirm-overlay{
  position:fixed;
  inset:0;
  z-index:900;
  display:grid;
  place-items:center;
  padding:1rem;
  background:rgba(2,6,23,.62);
  backdrop-filter:blur(18px) saturate(125%);
}
.buy-confirm-card{
  position:relative;
  width:min(94vw, 470px);
  border-radius:2rem;
  border:1px solid rgba(var(--border), .9);
  background:
    radial-gradient(circle at top right, rgba(var(--primary),.22), transparent 36%),
    linear-gradient(180deg, rgba(15, 23, 42, .98), rgba(8, 13, 25, .96));
  color:#f8fafc;
  padding:1.45rem;
  box-shadow:0 34px 100px rgba(0,0,0,.35), inset 0 1px 0 rgba(255,255,255,.08);
}
.buy-confirm-close{
  position:absolute;
  inset-inline-end:.95rem;
  top:.95rem;
  width:2.35rem;
  height:2.35rem;
  border-radius:999px;
  display:grid;
  place-items:center;
  border:1px solid rgba(var(--border), .88);
  background:rgba(var(--surface-rgb), .72);
  color:#f8fafc;
}
.buy-confirm-icon{
  width:3.35rem;
  height:3.35rem;
  border-radius:1.15rem;
  display:grid;
  place-items:center;
  color:white;
  background:linear-gradient(135deg, rgb(var(--primary)), #ec4899);
  font-size:1.65rem;
  box-shadow:0 16px 36px rgba(var(--primary),.28);
}
.buy-confirm-title{ margin-top:1rem; font-size:1.45rem; font-weight:1000; color:#f8fafc; }
.buy-confirm-text{ margin-top:.45rem; color:#dbeafe; line-height:1.8; }
.buy-confirm-product{
  margin-top:1rem;
  padding:.8rem;
  border-radius:1.25rem;
  border:1px solid rgba(var(--border), .78);
  background:rgba(255,255,255,.08);
  display:flex;
  align-items:center;
  gap:.85rem;
}
.buy-confirm-product img{
  width:4.7rem;
  height:4.7rem;
  object-fit:cover;
  border-radius:1rem;
  border:1px solid rgba(var(--border), .72);
  background:rgba(var(--surface-2-rgb),.7);
}
.buy-confirm-product div{ min-width:0; display:grid; gap:.25rem; }
.buy-confirm-product b{ display:block; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; color:#f8fafc; }
.buy-confirm-product span{ color:#f472b6; font-weight:1000; }
.buy-confirm-actions{
  margin-top:1.15rem;
  display:grid;
  grid-template-columns:1fr 1.25fr;
  gap:.7rem;
}
.buy-confirm-btn{
  min-height:3rem;
  border-radius:1rem;
  font-weight:1000;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.4rem;
  border:1px solid rgba(var(--border), .82);
  transition:.18s ease;
}
.buy-confirm-btn:hover{ transform:translateY(-1px); }
.buy-confirm-btn--ghost{ background:rgba(255,255,255,.09); color:#f8fafc; }
.buy-confirm-btn--main{
  border-color:transparent;
  color:white;
  background:linear-gradient(135deg, rgb(var(--primary)), #ec4899);
  box-shadow:0 14px 32px rgba(var(--primary),.24);
}
.buy-confirm-fade-enter-active,.buy-confirm-fade-leave-active{ transition:opacity .18s ease; }
.buy-confirm-fade-enter-from,.buy-confirm-fade-leave-to{ opacity:0; }


</style>
