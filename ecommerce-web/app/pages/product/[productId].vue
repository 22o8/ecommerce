<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'
import ProductCard from '~/components/ProductCard.vue'
import { formatIqd } from '~/composables/useMoney'

const route = useRoute()
const { t } = useI18n()
const api = useApi()
const cart = useCartStore()
const auth = useAuthStore()
const toast = useToast()

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

const images = computed(() => Array.isArray(product.value?.images) ? product.value.images : [])
const reviews = computed(() => Array.isArray(product.value?.reviews) ? product.value.reviews : [])
const myReview = computed(() => product.value?.myReview || null)

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
const avgRating = computed(() => Number(product.value?.ratingAvg ?? 0))
const ratingCount = computed(() => Number(product.value?.ratingCount ?? 0))
const isOutOfStock = computed(() => Number(product.value?.stockQuantity ?? 0) <= 0)

const reviewForm = reactive({ rating: 5, comment: '' })
const reviewSubmitting = ref(false)
watch(myReview, (v) => {
  reviewForm.rating = Number(v?.rating ?? 5)
  reviewForm.comment = String(v?.comment ?? '')
}, { immediate: true })

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
  if (!product.value || isOutOfStock.value) return
  cart.add(product.value)
  toast.success('تمت الإضافة إلى السلة')
}

const { checkoutSingleProduct } = useWhatsappCheckout()

async function buyNow() {
  if (!product.value || isOutOfStock.value) return
  try {
    await checkoutSingleProduct(product.value, 1)
  } catch {
    addToCart()
    navigateTo('/cart')
  }
}

async function submitReview() {
  if (!auth.isAuthed) {
    toast.error('يجب تسجيل الدخول أولاً')
    return
  }
  reviewSubmitting.value = true
  try {
    const res: any = await api.post(`/Products/${productId.value}/rate`, {
      rating: Number(reviewForm.rating || 5),
      comment: reviewForm.comment || null,
    })
    toast.success(res?.message || 'تم حفظ التقييم')
    await refresh()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حفظ التقييم')
  } finally {
    reviewSubmitting.value = false
  }
}

function fmt(v: any) {
  return formatIqd(v)
}

function starFill(n: number) {
  return avgRating.value >= n
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
      <div class="grid gap-3">
        <div class="relative overflow-hidden product-gallery-shell rounded-3xl">
          <div class="group relative aspect-square">
            <SmartImage :src="activeImage" :alt="product.title" fit="cover" wrapper-class="w-full h-full" img-class="w-full h-full object-cover transition duration-300 group-hover:scale-125" />
            <div v-if="discountPercent > 0" class="absolute top-4 right-4">
              <div class="px-4 py-2 rounded-full bg-red-500/90 text-white text-sm font-black keep-ltr shadow-xl">-{{ discountPercent }}%</div>
            </div>
          </div>
        </div>

        <div v-if="images.length" class="flex gap-2 overflow-x-auto pb-1">
          <button v-for="(im, idx) in images" :key="im.id || idx" class="shrink-0 h-20 w-20 rounded-2xl overflow-hidden border transition" :class="idx === activeIndex ? 'border-[rgb(var(--primary))] ring-2 ring-[rgb(var(--primary))]/30' : 'border-app opacity-80 hover:opacity-100'" @click="activeIndex = idx" type="button">
            <img class="h-full w-full object-cover" :src="api.buildAssetUrl(String(im.url || im))" :alt="product.title" />
          </button>
        </div>
      </div>

      <div class="grid gap-5">
        <div>
          <div class="text-sm text-muted">{{ brand }}</div>
          <h1 class="mt-1 text-2xl sm:text-3xl font-extrabold rtl-text">{{ product.title }}</h1>
          <div class="mt-3 flex flex-wrap items-center gap-3">
            <div class="flex items-center gap-1 text-amber-400">
              <Icon v-for="n in 5" :key="n" :name="starFill(n) ? 'mdi:star' : 'mdi:star-outline'" class="text-lg" />
            </div>
            <div class="text-sm text-white/70">{{ avgRating.toFixed(1) }} / 5</div>
            <div class="text-sm text-white/60">({{ ratingCount }} تقييم)</div>
          </div>
        </div>

        <div class="product-sheet rounded-3xl p-5">
          <div v-if="isOutOfStock" class="mb-4 inline-flex rounded-full bg-[rgb(var(--danger))]/15 px-4 py-2 text-sm font-bold text-[rgb(var(--danger))] rtl-text">{{ t('common.unavailable') }}</div>
          <div class="flex items-end justify-between gap-3 flex-wrap">
            <div>
              <div class="text-2xl font-black keep-ltr">{{ fmt(finalPriceIqd) }}</div>
              <div v-if="discountPercent > 0" class="text-sm text-muted keep-ltr"><span class="line-through opacity-70">{{ fmt(priceIqd) }}</span></div>
            </div>
            <div class="flex flex-wrap items-center gap-2">
              <button class="btn-cta-animated inline-flex items-center justify-center rounded-full px-5 py-2.5 text-sm font-semibold" @click="addToCart" :disabled="isOutOfStock">
                <Icon name="mdi:cart-plus" class="text-lg" />
                <span class="rtl-text">{{ t('common.addToCart') }}</span>
              </button>
              <button class="btn-cta-animated btn-cta-secondary inline-flex items-center justify-center rounded-full px-5 py-2.5 text-sm font-semibold" @click="buyNow" :disabled="isOutOfStock">
                <span class="rtl-text">{{ t('common.buy') }}</span>
              </button>
            </div>
          </div>
        </div>

        <div class="product-sheet rounded-3xl p-5">
          <div class="font-extrabold mb-2 rtl-text">{{ t('products.description') }}</div>
          <div class="text-sm text-muted whitespace-pre-line rtl-text">{{ product.description }}</div>
        </div>

        <div class="product-sheet rounded-3xl p-5 grid gap-4">
          <div class="flex items-center justify-between gap-3 flex-wrap">
            <div class="font-extrabold rtl-text">إضافة تقييم</div>
            <div class="text-sm text-white/60">يمكنك تعديل تقييمك لاحقًا</div>
          </div>

          <div class="flex items-center gap-2 text-amber-400">
            <button v-for="n in 5" :key="n" type="button" class="transition hover:scale-110" @click="reviewForm.rating = n">
              <Icon :name="reviewForm.rating >= n ? 'mdi:star' : 'mdi:star-outline'" class="text-2xl" />
            </button>
          </div>

          <textarea v-model="reviewForm.comment" rows="4" class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none" placeholder="اكتب رأيك عن المنتج"></textarea>

          <div class="flex flex-wrap items-center gap-3">
            <button class="btn-cta-animated inline-flex items-center justify-center rounded-full px-5 py-2.5 text-sm font-semibold" :disabled="reviewSubmitting" @click="submitReview">
              {{ myReview ? 'تحديث التقييم' : 'إرسال التقييم' }}
            </button>
            <div v-if="!auth.isAuthed" class="text-sm text-amber-300">يجب تسجيل الدخول أولاً</div>
          </div>
        </div>

        <div class="product-sheet rounded-3xl p-5 grid gap-4">
          <div class="font-extrabold rtl-text">التقييمات</div>
          <div v-if="!reviews.length" class="text-sm text-white/60">لا توجد تقييمات بعد</div>
          <div v-else class="grid gap-3">
            <div v-for="r in reviews" :key="r.id" class="rounded-2xl border border-white/10 bg-white/5 p-4">
              <div class="flex items-center justify-between gap-3 flex-wrap">
                <div class="font-bold">{{ r.userName || 'مستخدم' }}</div>
                <div class="flex items-center gap-1 text-amber-400">
                  <Icon v-for="n in 5" :key="n" :name="Number(r.rating) >= n ? 'mdi:star' : 'mdi:star-outline'" class="text-base" />
                </div>
              </div>
              <div v-if="r.comment" class="mt-2 text-sm text-white/80 whitespace-pre-line rtl-text">{{ r.comment }}</div>
            </div>
          </div>
        </div>

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
.product-gallery-shell, .product-sheet{ border: 1px solid rgba(var(--border), .95); background: rgb(var(--surface)); }
:global(html.theme-light) .product-gallery-shell,
:global(html.theme-light) .product-sheet{ background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(255,247,252,.95)); box-shadow: 0 28px 64px rgba(232, 91, 154, .10), 0 12px 32px rgba(24,24,24,.05); }
:global(html.theme-dark) .product-gallery-shell,
:global(html.theme-dark) .product-sheet{ background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90)); box-shadow: 0 20px 52px rgba(0,0,0,.28); }
</style>
