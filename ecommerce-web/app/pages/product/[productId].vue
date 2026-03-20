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
const categoryKey = computed(() => String(product.value?.category ?? ''))
const subCategoryKey = computed(() => String(product.value?.subCategory ?? ''))
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
  () => `product-similar-${productId.value}-${categoryKey.value}-${subCategoryKey.value}`,
  async () => {
    const primaryCategory = subCategoryKey.value || categoryKey.value
    if (!primaryCategory) return []
    const res = await api.get<any>('/Products', { page: 1, pageSize: 8, category: primaryCategory, sort: 'new' })
    const items = Array.isArray(res?.items) ? res.items : []
    return items.filter((x: any) => String(x?.id) !== String(productId.value)).slice(0, 4)
  },
  { watch: [productId, categoryKey, subCategoryKey] }
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

    <div v-else class="product-layout-grid gap-6 lg:gap-8">
      <div class="grid gap-3 product-media-col">
        <div class="relative overflow-hidden product-gallery-shell rounded-[2rem]">
          <div class="group relative aspect-[1/1] md:aspect-[1.02/1]">
            <SmartImage :src="activeImage" :alt="product.title" fit="cover" wrapper-class="w-full h-full" img-class="w-full h-full object-cover transition duration-500 group-hover:scale-110" />
            <div class="absolute inset-x-0 bottom-0 h-32 bg-gradient-to-t from-black/30 via-black/5 to-transparent pointer-events-none"></div>
            <div v-if="discountPercent > 0" class="absolute top-4 right-4">
              <div class="product-badge-hero keep-ltr">-{{ discountPercent }}%</div>
            </div>
          </div>
        </div>

        <div v-if="images.length" class="product-thumbs-row">
          <button v-for="(im, idx) in images" :key="im.id || idx" class="product-thumb-btn" :class="idx === activeIndex ? 'is-active' : ''" @click="activeIndex = idx" type="button">
            <img class="h-full w-full object-cover" :src="api.buildAssetUrl(String(im.url || im))" :alt="product.title" />
          </button>
        </div>
      </div>

      <div class="grid gap-5 product-info-col lg:sticky lg:top-24 self-start">
        <div class="product-sheet rounded-[2rem] p-5 sm:p-6">
          <div class="flex flex-wrap items-center gap-2 mb-3">
            <span v-if="brand" class="product-meta-pill">{{ brand }}</span>
            <span v-if="categoryKey" class="product-meta-pill product-meta-pill--ghost">{{ categoryKey }}</span>
            <span v-if="subCategoryKey" class="product-meta-pill product-meta-pill--ghost">{{ subCategoryKey }}</span>
          </div>

          <h1 class="text-2xl sm:text-3xl lg:text-[2.1rem] font-extrabold leading-tight rtl-text text-balance">{{ product.title }}</h1>

          <div class="mt-4 flex flex-wrap items-center gap-3">
            <div class="flex items-center gap-1 text-amber-400">
              <Icon v-for="n in 5" :key="n" :name="starFill(n) ? 'mdi:star' : 'mdi:star-outline'" class="text-lg" />
            </div>
            <div class="text-sm text-white/75">{{ avgRating.toFixed(1) }} / 5</div>
            <div class="text-sm text-white/55">({{ ratingCount }} تقييم)</div>
          </div>

          <div class="product-price-card mt-5">
            <div>
              <div class="text-3xl sm:text-[2.2rem] font-black keep-ltr">{{ fmt(finalPriceIqd) }}</div>
              <div v-if="discountPercent > 0" class="mt-1 text-sm text-muted keep-ltr"><span class="line-through opacity-70">{{ fmt(priceIqd) }}</span></div>
            </div>
            <div v-if="isOutOfStock" class="inline-flex rounded-full bg-[rgb(var(--danger))]/15 px-4 py-2 text-sm font-bold text-[rgb(var(--danger))] rtl-text">{{ t('common.unavailable') }}</div>
          </div>

          <div class="mt-5 grid grid-cols-1 sm:grid-cols-2 gap-3">
            <button class="btn-cta-animated product-primary-action inline-flex items-center justify-center rounded-full px-5 py-3 text-sm font-semibold" @click="addToCart" :disabled="isOutOfStock">
              <Icon name="mdi:cart-plus" class="text-lg" />
              <span class="rtl-text">{{ t('common.addToCart') }}</span>
            </button>
            <button class="btn-cta-animated btn-cta-secondary product-secondary-action inline-flex items-center justify-center rounded-full px-5 py-3 text-sm font-semibold" @click="buyNow" :disabled="isOutOfStock">
              <span class="rtl-text">{{ t('common.buy') }}</span>
            </button>
          </div>
        </div>

        <div class="product-sheet rounded-[2rem] p-5 sm:p-6">
          <div class="font-extrabold mb-3 rtl-text text-lg">{{ t('products.description') }}</div>
          <div class="text-sm sm:text-[15px] text-muted whitespace-pre-line rtl-text leading-8">{{ product.description }}</div>
        </div>

        <div class="product-sheet rounded-[2rem] p-5 sm:p-6 grid gap-4">
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

        <div v-if="(similar?.length || 0) > 0" class="grid gap-3 pt-1">
          <div class="flex items-center justify-between">
            <div class="text-lg font-extrabold rtl-text">{{ t('products.youMayAlsoLike') }}</div>
            <NuxtLink :to="`/products?category=${encodeURIComponent(subCategoryKey || categoryKey)}`" class="text-sm text-[rgb(var(--primary))]">{{ t('home.viewAll') }}</NuxtLink>
          </div>
          <div class="grid grid-cols-2 gap-3 sm:gap-4 xl:grid-cols-4">
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

.product-layout-grid{
  display:grid;
  grid-template-columns:minmax(0,1.02fr) minmax(0,.92fr);
}
.product-thumbs-row{
  display:flex;
  gap:.8rem;
  overflow-x:auto;
  padding:.15rem .05rem .25rem;
}
.product-thumbs-row::-webkit-scrollbar{ display:none; }
.product-thumb-btn{
  flex:0 0 auto;
  width:84px;
  height:84px;
  overflow:hidden;
  border-radius:1.15rem;
  border:1px solid rgba(var(--border), .9);
  opacity:.78;
  transition:transform .2s ease, opacity .2s ease, border-color .2s ease, box-shadow .2s ease;
  box-shadow:0 10px 24px rgba(0,0,0,.12);
}
.product-thumb-btn:hover{ opacity:1; transform:translateY(-2px); }
.product-thumb-btn.is-active{
  opacity:1;
  border-color:rgba(var(--primary), .72);
  box-shadow:0 0 0 3px rgba(var(--primary), .18), 0 12px 28px rgba(0,0,0,.16);
}
.product-badge-hero{
  padding:.7rem 1rem;
  border-radius:999px;
  background:linear-gradient(135deg, rgba(239,68,68,.98), rgba(244,63,94,.92));
  color:#fff;
  font-size:.85rem;
  font-weight:900;
  box-shadow:0 18px 40px rgba(239,68,68,.28);
}
.product-meta-pill{
  display:inline-flex;
  align-items:center;
  padding:.45rem .85rem;
  border-radius:999px;
  background:rgba(var(--primary), .14);
  border:1px solid rgba(var(--primary), .2);
  color:rgb(var(--text));
  font-size:.78rem;
  font-weight:800;
}
.product-meta-pill--ghost{
  background:rgba(255,255,255,.06);
  border-color:rgba(var(--border), .9);
}
.product-price-card{
  display:flex;
  justify-content:space-between;
  align-items:flex-end;
  gap:1rem;
  flex-wrap:wrap;
  border-radius:1.5rem;
  padding:1rem 1.15rem;
  background:linear-gradient(180deg, rgba(var(--surface-2-rgb), .92), rgba(var(--surface-rgb), .84));
  border:1px solid rgba(var(--border), .85);
}
.product-primary-action, .product-secondary-action{ min-height:52px; }
.text-balance{ text-wrap:balance; }
@media (max-width: 1024px){
  .product-layout-grid{ grid-template-columns:1fr; }
}
@media (max-width: 640px){
  .product-page{ padding-top:1.25rem; padding-bottom:2rem; }
  .product-gallery-shell{ border-radius:1.6rem; }
  .product-sheet{ border-radius:1.4rem; }
  .product-thumb-btn{ width:72px; height:72px; border-radius:1rem; }
  .product-price-card{ padding:.95rem 1rem; }
  .product-primary-action, .product-secondary-action{ width:100%; }
}

</style>
