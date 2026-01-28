<template>
  <teleport to="body">
    <div v-if="open" class="fixed inset-0 z-[90]">
      <div class="absolute inset-0 bg-black/60" @click="close" />
      <div class="absolute inset-0 flex items-end sm:items-center justify-center p-0 sm:p-4">
	        <div class="card-soft w-full sm:max-w-5xl overflow-hidden max-h-[92vh] flex flex-col rounded-t-3xl sm:rounded-3xl">
          <div class="flex items-center justify-between gap-3 border-b border-app px-4 py-3">
            <div class="font-extrabold rtl-text truncate">{{ displayName }}</div>

            <button class="btn-soft" type="button" @click="close" aria-label="Close">
              <Icon name="mdi:close" class="text-xl" />
            </button>
          </div>

			  <div class="flex-1 overflow-y-auto pb-[env(safe-area-inset-bottom)]" style="-webkit-overflow-scrolling: touch;">
	            <div class="grid gap-4 p-4 md:p-6 md:grid-cols-2">
            <ProductGallery :images="images" :title="displayName" />

            <div class="grid gap-4">
              <div class="flex items-center justify-between gap-3">
	                <div class="text-xl md:text-2xl font-black keep-ltr">{{ priceText }}</div>
                <UiBadge v-if="p?.isFeatured" class="keep-ltr">Featured</UiBadge>
              </div>

              <div class="text-sm text-muted rtl-text" v-if="p?.description">{{ p.description }}</div>

	              <div class="flex flex-wrap gap-2">
                <UiButton variant="secondary" @click="addToCart">
                  <Icon name="mdi:cart-plus" class="text-lg" />
                  <span class="rtl-text">{{ t('productsPage.addToCart') }}</span>
                </UiButton>

                <UiButton variant="ghost" @click="toggleFav">
                  <Icon :name="isFav ? 'mdi:heart' : 'mdi:heart-outline'" class="text-lg" />
                  <span class="rtl-text">{{ isFav ? t('wishlist.remove') : t('wishlist.add') }}</span>
                </UiButton>
              </div>

              <div class="card-soft p-4">
                <ProductReviews :product-id="p?.id" />
              </div>

              <div class="card-soft p-4">
                <div class="font-bold rtl-text mb-2">{{ t('whatsapp.title') }}</div>
                <a
                  class="btn-soft inline-flex items-center gap-2"
                  :href="waLink"
                  target="_blank"
                  rel="noopener"
                >
                  <Icon name="mdi:whatsapp" class="text-xl" />
                  <span class="rtl-text">{{ t('whatsapp.cta') }}</span>
                </a>
	            </div>
	            </div>
	          </div>
          </div>
        </div>
      </div>
    </div>
  </teleport>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiBadge from '~/components/ui/UiBadge.vue'
import ProductReviews from '~/components/ProductReviews.vue'
import ProductGallery from '~/components/ProductGallery.vue'
import { useCartStore } from '~/stores/cart'
import { useWishlist } from '~/composables/useWishlist'
import { useQuickPreview } from '~/composables/useQuickPreview'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const cart = useCartStore()
const wl = useWishlist()
const qp = useQuickPreview()
const products = useProductsStore()

const open = computed(() => qp.open.value)
const p = computed<any>(() => qp.product.value)

// منع سكرول الخلفية فقط أثناء فتح المودال (وخاصة على iOS)
let prevHtmlOverflow = ''
let prevBodyOverflow = ''
watch(open, (v) => {
  if (!import.meta.client) return
  const html = document.documentElement
  const body = document.body

  if (v) {
    prevHtmlOverflow = html.style.overflow
    prevBodyOverflow = body.style.overflow
    html.style.overflow = 'hidden'
    body.style.overflow = 'hidden'
  } else {
    html.style.overflow = prevHtmlOverflow || ''
    body.style.overflow = prevBodyOverflow || ''
  }
})

const displayName = computed(() => p.value?.name || p.value?.title || p.value?.Title || '')

const images = computed(() => {
  const arr: string[] = []
  if (p.value?.images?.length) arr.push(...p.value.images.map((x: any) => x.url || x))
  if (p.value?.imageUrl) arr.unshift(p.value.imageUrl)
  if (p.value?.coverImage) arr.unshift(p.value.coverImage)
  return [...new Set(arr.filter(Boolean))]
})

const priceText = computed(() => {
  const v = p.value?.priceUsd ?? p.value?.PriceUsd ?? p.value?.price ?? p.value?.Price ?? 0
  const n = Number(v)
  return isFinite(n) ? `$${n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` : String(v)
})

const isFav = computed(() => (p.value?.id ? wl.has(String(p.value.id)) : false))

const waLink = computed(() => {
  const name = displayName.value || ''
  const text = encodeURIComponent(`${t('whatsapp.messagePrefix')} ${name}`)
  // رقم واتساب يُقرأ من env: NUXT_PUBLIC_WHATSAPP_PHONE (بدون +)
  const phone = (useRuntimeConfig().public.whatsappPhone || '').toString().replace(/\D/g, '')
  const target = phone ? `https://wa.me/${phone}` : 'https://wa.me/'
  return `${target}?text=${text}`
})

// دعم فتح الـ popup من أي صفحة عبر ?p=PRODUCT_ID
watch(
  () => route.query.p,
  (val) => {
    if (typeof val !== 'string' || !val) return
    // إذا كانت نفس السلعة مفتوحة أصلاً، لا نسوي شي
    const currentId = String(p.value?.id ?? p.value?.Id ?? '')
    if (open.value && currentId === val) return

    // حاول نلقاها من المنتجات المحمّلة حالياً
    const found = products.items.find((x: any) => String(x?.id ?? x?.Id ?? '') === val)
    if (found) qp.show(found)
  },
  { immediate: true }
)

// قفل سكرول الخلفية عند فتح المودال + السماح بالسكرول داخل المودال
watch(
  open,
  (v) => {
    if (!import.meta.client) return
    const el = document.documentElement
    const body = document.body
    if (v) {
      el.style.overflow = 'hidden'
      body.style.overflow = 'hidden'
    } else {
      el.style.overflow = ''
      body.style.overflow = ''
    }
  },
  { immediate: true }
)

function close() {
  qp.close()

  // إزالة ?p من الرابط بعد إغلاق النافذة
  if (route.query.p) {
    const q = { ...route.query } as Record<string, any>
    delete q.p
    router.replace({ query: q })
  }
}

function addToCart() {
  if (!p.value) return
  cart.add(p.value)
}

function toggleFav() {
  if (!p.value?.id) return
  wl.toggle(String(p.value.id))
}
</script>

<style scoped>
.btn-soft{
  border-radius: 16px;
  border: 1px solid rgba(255,255,255,.15);
  background: rgba(255,255,255,.06);
  padding: 10px 12px;
  transition: background .15s ease, transform .15s ease, border-color .15s ease;
}
.btn-soft:hover{ background: rgba(255,255,255,.10); }
.btn-soft:active{ transform: scale(.98); }
</style>
