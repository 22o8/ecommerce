<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'
import ProductGallery from '~/components/ProductGallery.vue'
import ProductReviews from '~/components/ProductReviews.vue'
import { formatIqd } from '~/composables/useMoney'

const route = useRoute()
const { t } = useI18n()
const api = useApi()
const cart = useCartStore()

const productId = computed(() => String(route.params.productId || ''))

const { data: product, pending, error, refresh } = await useAsyncData(
  () => `product-${productId.value}`,
  async () => {
    if (!productId.value) return null
    const p = await api.get<any>(`/Products/${productId.value}`)
    api.post(`/Products/${productId.value}/view`, {}).catch(() => {})
    return p
  },
  { watch: [productId] }
)

const galleryImages = computed(() => {
  const arr = product.value?.images || []
  return Array.isArray(arr) && arr.length ? arr : [product.value?.coverImage].filter(Boolean)
})

const priceIqd = computed(() => Number(product.value?.priceIqd ?? 0))
const discountPercent = computed(() => Number(product.value?.discountPercent ?? 0))
const finalPriceIqd = computed(() => Number(product.value?.finalPriceIqd ?? (discountPercent.value > 0 ? (priceIqd.value * (100 - discountPercent.value) / 100) : priceIqd.value)))
const savedAmount = computed(() => Math.max(0, priceIqd.value - finalPriceIqd.value))

const brand = computed(() => String(product.value?.brand ?? ''))
const ratingAvg = computed(() => Number(product.value?.ratingAvg ?? 0))
const ratingCount = computed(() => Number(product.value?.ratingCount ?? 0))
const viewCount = computed(() => Number(product.value?.viewCount ?? 0))
const favoriteCount = computed(() => Number(product.value?.favoriteCount ?? 0))

const detailBullets = computed(() => {
  const items:string[] = []
  if (brand.value) items.push(`${t('productPage.brandLabel')}: ${brand.value}`)
  if (discountPercent.value > 0) items.push(`${t('productPage.discountLabel')}: ${discountPercent.value}%`)
  if (ratingCount.value > 0) items.push(`${t('productPage.ratedBy')}: ${ratingCount.value}`)
  return items
})

const { data: similar } = await useAsyncData(
  () => `product-similar-${productId.value}-${brand.value}`,
  async () => {
    if (!brand.value) return []
    const res = await api.get<any>('/Products', { page: 1, pageSize: 8, brand: brand.value, sort: 'new' })
    const items = Array.isArray(res?.items) ? res.items : []
    return items.filter((x: any) => String(x?.id) !== String(productId.value)).slice(0, 4)
  },
  { watch: [productId, brand] }
)

function addToCart() {
  if (!product.value) return
  cart.add(product.value)
}

function buyNow() {
  addToCart()
  navigateTo('/cart')
}

function fmt(v: any) {
  return formatIqd(v)
}
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-8 sm:py-10 product-page">
    <div v-if="pending" class="product-status-card">{{ t('common.loading') }}</div>

    <div v-else-if="error || !product" class="product-status-card grid gap-3">
      <div class="font-extrabold">{{ t('common.notFound') }}</div>
      <button class="btn w-fit" @click="refresh">{{ t('common.retry') }}</button>
    </div>

    <div v-else class="grid gap-8">
      <div class="flex flex-wrap items-center gap-2 text-sm text-[rgb(var(--muted))] rtl-text">
        <NuxtLink to="/" class="hover:text-[rgb(var(--text))] transition">{{ t('nav.home') }}</NuxtLink>
        <span>/</span>
        <NuxtLink to="/products" class="hover:text-[rgb(var(--text))] transition">{{ t('nav.products') }}</NuxtLink>
        <template v-if="brand">
          <span>/</span>
          <NuxtLink :to="`/products?brand=${encodeURIComponent(brand)}`" class="hover:text-[rgb(var(--text))] transition">{{ brand }}</NuxtLink>
        </template>
      </div>

      <div class="grid gap-8 xl:grid-cols-[1.08fr_.92fr] xl:items-start">
        <div class="product-gallery-shell rounded-[30px] p-3 sm:p-4">
          <ProductGallery :images="galleryImages" :title="product.title" />
        </div>

        <div class="grid gap-5 xl:sticky xl:top-24">
          <section class="product-hero-card rounded-[30px] p-5 sm:p-6">
            <div class="flex flex-wrap items-center gap-2">
              <span v-if="brand" class="product-soft-badge">{{ brand }}</span>
              <span v-if="discountPercent > 0" class="product-discount-badge keep-ltr">-{{ discountPercent }}%</span>
              <span class="product-soft-badge">{{ t('productPage.readyToShip') }}</span>
            </div>

            <h1 class="mt-4 text-2xl font-black leading-tight text-[rgb(var(--text))] sm:text-[2rem] rtl-text">
              {{ product.title }}
            </h1>

            <div class="mt-4 flex flex-wrap items-center gap-4 text-sm text-[rgb(var(--muted))]">
              <div class="inline-flex items-center gap-1.5 keep-ltr">
                <Icon name="mdi:star" class="text-lg text-amber-400" />
                <span class="font-extrabold text-[rgb(var(--text))]">{{ ratingAvg > 0 ? ratingAvg.toFixed(1) : '—' }}</span>
                <span>{{ ratingCount > 0 ? `(${ratingCount})` : t('productPage.noReviewsShort') }}</span>
              </div>
              <div class="inline-flex items-center gap-1.5 keep-ltr">
                <Icon name="mdi:eye-outline" class="text-lg" />
                <span>{{ viewCount }}</span>
              </div>
              <div class="inline-flex items-center gap-1.5 keep-ltr">
                <Icon name="mdi:heart-outline" class="text-lg" />
                <span>{{ favoriteCount }}</span>
              </div>
            </div>

            <div class="mt-5 rounded-[26px] product-price-panel p-4 sm:p-5">
              <div class="flex flex-wrap items-end justify-between gap-4">
                <div>
                  <div class="text-sm font-semibold text-[rgb(var(--muted))] rtl-text">{{ t('productPage.finalPrice') }}</div>
                  <div class="mt-1 text-3xl font-black keep-ltr text-[rgb(var(--text))]">{{ fmt(finalPriceIqd) }}</div>
                  <div v-if="discountPercent > 0" class="mt-2 flex flex-wrap items-center gap-3 text-sm">
                    <span class="line-through text-[rgb(var(--muted))] keep-ltr">{{ fmt(priceIqd) }}</span>
                    <span class="product-save-pill keep-ltr">{{ t('productPage.youSave') }} {{ fmt(savedAmount) }}</span>
                  </div>
                </div>

                <div class="grid gap-2 min-w-[160px]">
                  <button class="btn-cta-animated inline-flex items-center justify-center gap-2 rounded-full px-5 py-3 text-sm font-semibold" @click="addToCart">
                    <Icon name="mdi:cart-plus" class="text-lg" />
                    <span class="rtl-text">{{ t('common.addToCart') }}</span>
                  </button>
                  <button class="btn-cta-animated btn-cta-secondary inline-flex items-center justify-center rounded-full px-5 py-3 text-sm font-semibold" @click="buyNow">
                    <span class="rtl-text">{{ t('common.buyNow') }}</span>
                  </button>
                </div>
              </div>
            </div>

            <div class="mt-5 grid gap-3 sm:grid-cols-3">
              <div class="product-mini-stat">
                <div class="product-mini-stat__label">{{ t('productPage.deliveryLabel') }}</div>
                <div class="product-mini-stat__value rtl-text">{{ t('productPage.deliveryValue') }}</div>
              </div>
              <div class="product-mini-stat">
                <div class="product-mini-stat__label">{{ t('productPage.paymentLabel') }}</div>
                <div class="product-mini-stat__value rtl-text">{{ t('productPage.paymentValue') }}</div>
              </div>
              <div class="product-mini-stat">
                <div class="product-mini-stat__label">{{ t('productPage.authenticityLabel') }}</div>
                <div class="product-mini-stat__value rtl-text">{{ t('productPage.authenticityValue') }}</div>
              </div>
            </div>
          </section>

          <section class="product-sheet rounded-[30px] p-5 sm:p-6 grid gap-4">
            <div class="flex items-center justify-between gap-3">
              <h2 class="text-lg font-black text-[rgb(var(--text))] rtl-text">{{ t('products.description') }}</h2>
              <span class="product-soft-badge">{{ t('productPage.detailsBadge') }}</span>
            </div>
            <p class="text-sm leading-7 text-[rgb(var(--muted))] whitespace-pre-line rtl-text">
              {{ product.description || t('productPage.noDescription') }}
            </p>

            <div v-if="detailBullets.length" class="grid gap-2 sm:grid-cols-2">
              <div v-for="item in detailBullets" :key="item" class="product-detail-item rtl-text">
                <Icon name="mdi:check-decagram" class="text-lg text-[rgb(var(--primary))]" />
                <span>{{ item }}</span>
              </div>
            </div>
          </section>
        </div>
      </div>

      <section class="product-sheet rounded-[30px] p-5 sm:p-6 grid gap-5">
        <div class="flex items-center justify-between gap-3">
          <div>
            <h2 class="text-xl font-black text-[rgb(var(--text))] rtl-text">{{ t('reviews.title') }}</h2>
            <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">{{ t('productPage.reviewsSubtitle') }}</p>
          </div>
          <span class="product-soft-badge keep-ltr">{{ ratingCount }} {{ t('productPage.reviewsCountSuffix') }}</span>
        </div>
        <ProductReviews :product-id="product.id" />
      </section>

      <section v-if="(similar?.length || 0) > 0" class="grid gap-5">
        <div class="flex flex-wrap items-end justify-between gap-3">
          <div>
            <h2 class="text-xl font-black text-[rgb(var(--text))] rtl-text">{{ t('products.youMayAlsoLike') }}</h2>
            <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">{{ t('productPage.moreFromBrand') }}</p>
          </div>
          <NuxtLink :to="`/products?brand=${encodeURIComponent(brand)}`" class="btn inline-flex items-center rounded-full px-4 py-2 text-sm font-semibold">
            {{ t('home.viewAll') }}
          </NuxtLink>
        </div>

        <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <ProductCard v-for="p in similar" :key="p.id" :p="p" />
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.product-status-card,
.product-gallery-shell,
.product-sheet,
.product-hero-card{
  border: 1px solid rgba(var(--border), .95);
  background: rgb(var(--surface));
}

.product-hero-card{
  position: relative;
  overflow: hidden;
}

.product-hero-card::before{
  content: '';
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at top right, rgba(var(--primary), .14), transparent 34%),
    linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .92));
  pointer-events: none;
}

.product-hero-card > *{ position: relative; z-index: 1; }

.product-price-panel{
  border: 1px solid rgba(var(--border), .9);
  background: rgba(var(--surface-2-rgb), .72);
}

.product-soft-badge,
.product-discount-badge,
.product-save-pill{
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 34px;
  padding: 0 14px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 800;
}

.product-soft-badge{
  border: 1px solid rgba(var(--border), .95);
  background: rgba(var(--surface-rgb), .78);
  color: rgb(var(--text));
}

.product-discount-badge{
  background: linear-gradient(135deg, #ef4444, #f97316);
  color: white;
  box-shadow: 0 16px 34px rgba(239, 68, 68, .24);
}

.product-save-pill{
  background: rgba(16, 185, 129, .12);
  color: rgb(5, 150, 105);
  border: 1px solid rgba(16, 185, 129, .18);
}

.product-mini-stat{
  border-radius: 24px;
  border: 1px solid rgba(var(--border), .92);
  background: rgba(var(--surface-rgb), .68);
  padding: 14px;
}
.product-mini-stat__label{
  font-size: 12px;
  font-weight: 700;
  color: rgb(var(--muted));
}
.product-mini-stat__value{
  margin-top: 6px;
  font-size: 14px;
  font-weight: 900;
  color: rgb(var(--text));
}

.product-detail-item{
  display: flex;
  align-items: center;
  gap: 10px;
  border-radius: 18px;
  border: 1px solid rgba(var(--border), .85);
  background: rgba(var(--surface-rgb), .62);
  padding: 12px 14px;
  font-size: 14px;
  color: rgb(var(--text));
}

:global(html.theme-light) .product-status-card,
:global(html.theme-light) .product-gallery-shell,
:global(html.theme-light) .product-sheet,
:global(html.theme-light) .product-hero-card{
  background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(255,247,252,.95));
  box-shadow: 0 28px 64px rgba(232, 91, 154, .10), 0 12px 32px rgba(24,24,24,.05);
}

:global(html.theme-light) .product-price-panel{
  background: linear-gradient(180deg, rgba(255,255,255,.96), rgba(252,243,249,.96));
  box-shadow: inset 0 1px 0 rgba(255,255,255,.75);
}

:global(html.theme-light) .product-mini-stat,
:global(html.theme-light) .product-detail-item,
:global(html.theme-light) .product-soft-badge{
  background: rgba(255,255,255,.78);
}

:global(html.theme-dark) .product-status-card,
:global(html.theme-dark) .product-gallery-shell,
:global(html.theme-dark) .product-sheet,
:global(html.theme-dark) .product-hero-card{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90));
  box-shadow: 0 20px 52px rgba(0,0,0,.28);
}

:global(html.theme-dark) .product-price-panel,
:global(html.theme-dark) .product-mini-stat,
:global(html.theme-dark) .product-detail-item,
:global(html.theme-dark) .product-soft-badge{
  background: rgba(var(--surface-rgb), .72);
}
</style>
