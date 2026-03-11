<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'
import ProductCard from '~/components/ProductCard.vue'
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
    // track view (لا يفشل الصفحة إذا تعذّر)
    api.post(`/Products/${productId.value}/view`, {}).catch(() => {})
    return p
  },
  { watch: [productId] }
)

const images = computed(() => {
  const arr = product.value?.images || []
  return Array.isArray(arr) ? arr : []
})

const activeIndex = ref(0)
watch(images, () => { activeIndex.value = 0 })

const activeImage = computed(() => {
  const im = images.value?.[activeIndex.value]
  const raw = im?.url || im || product.value?.coverImage || ''
  return raw ? api.buildAssetUrl(String(raw)) : '/hero-placeholder.svg'
})

const priceIqd = computed(() => Number(product.value?.priceIqd ?? 0))
const discountPercent = computed(() => Number(product.value?.discountPercent ?? 0))
const finalPriceIqd = computed(() => Number(product.value?.finalPriceIqd ?? (discountPercent.value > 0 ? (priceIqd.value * (100 - discountPercent.value) / 100) : priceIqd.value)))

const brand = computed(() => String(product.value?.brand ?? ''))

// Similar / You may also like: fallback بسيط عبر نفس البراند
const { data: similar } = await useAsyncData(
  () => `product-similar-${productId.value}-${brand.value}`,
  async () => {
    if (!brand.value) return []
    const res = await api.get<any>('/Products', { page: 1, pageSize: 8, brand: brand.value, sort: 'new' })
    const items = Array.isArray(res?.items) ? res.items : []
    return items.filter((x: any) => String(x?.id) !== String(productId.value))
  },
  { watch: [productId, brand] }
)

function addToCart() {
  if (!product.value) return
  cart.add(product.value)
}

const { checkoutSingleProduct } = useWhatsappCheckout()

async function buyNow() {
  if (!product.value) return
  try {
    await checkoutSingleProduct(product.value, 1)
  } catch (e) {
    addToCart()
    navigateTo('/cart')
  }
}

function fmt(v: any) {
  return formatIqd(v)
}
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-10 product-page">
    <div v-if="pending" class="text-muted">{{ t('common.loading') }}</div>
    <div v-else-if="error || !product" class="rounded-2xl border border-app bg-surface p-6">
      <div class="font-extrabold">{{ t('common.notFound') }}</div>
      <button class="mt-3 btn" @click="refresh">{{ t('common.retry') }}</button>
    </div>

    <div v-else class="grid gap-8 lg:grid-cols-2">
      <!-- Gallery -->
      <div class="grid gap-3">
        <div class="relative overflow-hidden product-gallery-shell rounded-3xl">
          <div class="group relative aspect-square">
            <SmartImage
              :src="activeImage"
              :alt="product.title"
              fit="cover"
              wrapper-class="w-full h-full"
              img-class="w-full h-full object-cover transition duration-300 group-hover:scale-125"
            />

            <div v-if="discountPercent > 0" class="absolute top-4 right-4">
              <div class="px-4 py-2 rounded-full bg-red-500/90 text-white text-sm font-black keep-ltr shadow-xl">
                -{{ discountPercent }}%
              </div>
            </div>
          </div>
        </div>

        <div v-if="images.length" class="flex gap-2 overflow-x-auto pb-1">
          <button
            v-for="(im, idx) in images"
            :key="im.id || idx"
            class="shrink-0 h-20 w-20 rounded-2xl overflow-hidden border transition"
            :class="idx === activeIndex ? 'border-[rgb(var(--primary))] ring-2 ring-[rgb(var(--primary))]/30' : 'border-app opacity-80 hover:opacity-100'"
            @click="activeIndex = idx"
            type="button"
          >
            <img
              class="h-full w-full object-cover"
              :src="api.buildAssetUrl(String(im.url || im))"
              :alt="product.title"
            />
          </button>
        </div>
      </div>

      <!-- Details -->
      <div class="grid gap-5">
        <div>
          <div class="text-sm text-muted">{{ brand }}</div>
          <h1 class="mt-1 text-2xl sm:text-3xl font-extrabold rtl-text">{{ product.title }}</h1>
        </div>

        <div class="product-sheet rounded-3xl p-5">
          <div class="flex items-end justify-between gap-3">
            <div>
              <div class="text-2xl font-black keep-ltr">{{ fmt(finalPriceIqd) }}</div>
              <div v-if="discountPercent > 0" class="text-sm text-muted keep-ltr">
                <span class="line-through opacity-70">{{ fmt(priceIqd) }}</span>
              </div>
            </div>

            <div class="flex items-center gap-2">
              <button class="btn-cta-animated inline-flex items-center justify-center rounded-full px-5 py-2.5 text-sm font-semibold" @click="addToCart">
                <Icon name="mdi:cart-plus" class="text-lg" />
                <span class="rtl-text">{{ t('common.addToCart') }}</span>
              </button>
              <button class="btn-cta-animated btn-cta-secondary inline-flex items-center justify-center rounded-full px-5 py-2.5 text-sm font-semibold" @click="buyNow">
                <span class="rtl-text">{{ t('common.buy') }}</span>
              </button>
            </div>
          </div>
        </div>

        <div class="product-sheet rounded-3xl p-5">
          <div class="font-extrabold mb-2 rtl-text">{{ t('products.description') }}</div>
          <div class="text-sm text-muted whitespace-pre-line rtl-text">{{ product.description }}</div>
        </div>

        <!-- You may also like -->
        <div v-if="(similar?.length || 0) > 0" class="grid gap-3">
          <div class="flex items-center justify-between">
            <div class="text-lg font-extrabold rtl-text">{{ t('products.youMayAlsoLike') }}</div>
            <NuxtLink :to="`/products?brand=${encodeURIComponent(brand)}`" class="text-sm text-[rgb(var(--primary))]">{{ t('home.viewAll') }}</NuxtLink>
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <ProductCard v-for="p in similar" :key="p.id" :p="p" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>


<style scoped>
.product-gallery-shell, .product-sheet{
  border: 1px solid rgba(var(--border), .95);
  background: rgb(var(--surface));
}
:global(html.theme-light) .product-gallery-shell,
:global(html.theme-light) .product-sheet{
  background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(255,247,252,.95));
  box-shadow: 0 28px 64px rgba(232, 91, 154, .10), 0 12px 32px rgba(24,24,24,.05);
}
:global(html.theme-dark) .product-gallery-shell,
:global(html.theme-dark) .product-sheet{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90));
  box-shadow: 0 20px 52px rgba(0,0,0,.28);
}
</style>
