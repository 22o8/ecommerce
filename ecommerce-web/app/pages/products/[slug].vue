<template>
  <div class="grid gap-6">
    <div class="flex items-center justify-between gap-3">
      <NuxtLink to="/products" class="inline-flex items-center gap-2 text-sm opacity-80 hover:opacity-100">
        <Icon name="mdi:arrow-left" class="text-xl keep-ltr" />
        <span class="rtl-text">{{ t('product.back') }}</span>
      </NuxtLink>

      <div class="flex items-center gap-2">
        <UiButton variant="secondary" @click="toggleFav()">
          <Icon :name="isFav ? 'mdi:heart' : 'mdi:heart-outline'" class="text-lg" />
          <span class="rtl-text">{{ isFav ? t('wishlist.saved') : t('wishlist.save') }}</span>
        </UiButton>
      </div>
    </div>

    <div v-if="pending" class="grid gap-4 lg:grid-cols-2">
      <div class="card-soft p-4"><div class="skeleton h-80" /></div>
      <div class="card-soft p-4 grid gap-3">
        <div class="skeleton h-8 w-3/4" />
        <div class="skeleton h-5 w-1/2" />
        <div class="skeleton h-24" />
        <div class="skeleton h-12 w-1/2" />
      </div>
    </div>

    <div v-else-if="!product" class="card-soft p-10 text-center">
      <Icon name="mdi:alert-circle-outline" class="text-4xl opacity-70 mx-auto" />
      <div class="mt-3 font-bold rtl-text">{{ t('product.notFoundTitle') }}</div>
      <div class="mt-1 text-sm text-muted rtl-text">{{ t('product.notFoundDesc') }}</div>
      <div class="mt-4">
        <UiButton variant="secondary" to="/products">
          <span class="rtl-text">{{ t('product.backToProducts') }}</span>
        </UiButton>
      </div>
    </div>

    <div v-else class="grid gap-6 lg:grid-cols-2">
      <!-- Gallery -->
      <div class="card-soft overflow-hidden">
        <div
          class="relative aspect-[4/3] bg-black/20"
          @mousemove="onZoomMove"
          @mouseenter="zoomOn = true"
          @mouseleave="zoomOn = false"
        >
          <SmartImage
            v-if="activeImage"
            :src="activeImage"
            :alt="product.name"
            class="absolute inset-0"
            img-class="w-full h-full object-contain bg-black/10"
          />

          <!-- Mouse zoom lens -->
          <div
            v-if="zoomOn && activeImage"
            class="absolute inset-0"
            :style="zoomStyle"
          />

          <!-- Controls -->
          <button
            v-if="images.length > 1"
            class="absolute left-3 top-1/2 -translate-y-1/2 h-10 w-10 rounded-full bg-black/40 hover:bg-black/60 grid place-items-center"
            @click.stop.prevent="prevImage"
            aria-label="Prev"
          >
            <Icon name="mdi:chevron-left" class="text-2xl text-white keep-ltr" />
          </button>
          <button
            v-if="images.length > 1"
            class="absolute right-3 top-1/2 -translate-y-1/2 h-10 w-10 rounded-full bg-black/40 hover:bg-black/60 grid place-items-center"
            @click.stop.prevent="nextImage"
            aria-label="Next"
          >
            <Icon name="mdi:chevron-right" class="text-2xl text-white keep-ltr" />
          </button>

          <button
            class="absolute bottom-3 right-3 rounded-xl border border-white/15 bg-black/40 hover:bg-black/60 px-3 py-2 text-xs text-white"
            @click.stop.prevent="openFullscreen()"
          >
            <Icon name="mdi:fullscreen" class="text-base" />
            <span class="ms-1 rtl-text">{{ t('product.fullscreen') }}</span>
          </button>
        </div>

        <div v-if="images.length > 1" class="p-3 flex gap-2 overflow-x-auto">
          <button
            v-for="(img, idx) in images"
            :key="img + idx"
            class="relative h-16 w-24 shrink-0 rounded-xl overflow-hidden border"
            :class="idx === activeIndex ? 'border-primary' : 'border-white/10'"
            @click.stop.prevent="activeIndex = idx"
          >
            <SmartImage :src="img" :alt="product.name" class="absolute inset-0" img-class="w-full h-full object-cover" />
          </button>
        </div>
      </div>

      <!-- Details -->
      <div class="grid gap-4">
        <div class="card-soft p-5 grid gap-3">
          <div class="flex items-start justify-between gap-3">
            <div>
              <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ product.name }}</h1>
              <div class="mt-1 text-sm text-muted keep-ltr">/{{ product.slug }}</div>
            </div>
            <div class="text-2xl font-black keep-ltr">{{ formatMoney(product.price) }}</div>
          </div>

          <div v-if="product.description" class="text-sm leading-7 rtl-text text-white/90">
            {{ product.description }}
          </div>

          <div class="flex flex-wrap gap-2 pt-2">
            <UiButton variant="primary" @click="addToCart(product)">
              <Icon name="mdi:cart-plus" class="text-lg" />
              <span class="rtl-text">{{ t('product.addToCart') }}</span>
            </UiButton>
            <UiButton variant="secondary" @click="buyNow(product)">
              <Icon name="mdi:flash" class="text-lg" />
              <span class="rtl-text">{{ t('product.buyNow') }}</span>
            </UiButton>

            <UiButton variant="ghost" :href="waLink" target="_blank">
              <Icon name="mdi:whatsapp" class="text-lg" />
              <span class="rtl-text">{{ t('product.whatsapp') }}</span>
            </UiButton>
          </div>
        </div>

        <div class="card-soft p-5">
          <ProductReviews :product-id="String(product.id)" />
        </div>
      </div>
    </div>

    <!-- Related products -->
    <div v-if="related.length" class="grid gap-3">
      <div class="flex items-center justify-between">
        <h2 class="text-lg font-black rtl-text">{{ t('product.related') }}</h2>
        <NuxtLink to="/products" class="text-sm opacity-80 hover:opacity-100 rtl-text">
          {{ t('product.viewAll') }}
        </NuxtLink>
      </div>
      <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <ProductCard v-for="rp in related" :key="rp.id" :p="rp" />
      </div>
    </div>

    <!-- Fullscreen slider -->
    <teleport to="body">
      <div v-if="fullscreen" class="fixed inset-0 z-[95]">
        <div class="absolute inset-0 bg-black/80" @click="fullscreen = false" />
        <div class="absolute inset-0 p-4 flex items-center justify-center">
          <div
            class="relative w-full max-w-6xl"
            @touchstart.passive="onTouchStart"
            @touchmove.passive="onTouchMove"
            @touchend="onTouchEnd"
          >
            <div class="card-soft overflow-hidden">
              <div class="relative aspect-[16/9] bg-black/30">
                <SmartImage
                  v-if="activeImage"
                  :src="activeImage"
                  :alt="product?.name || ''"
                  class="absolute inset-0"
                  img-class="w-full h-full object-contain"
                />
              </div>
            </div>

            <button
              class="absolute -top-2 -right-2 h-11 w-11 rounded-full bg-black/60 hover:bg-black/80 grid place-items-center"
              @click="fullscreen = false"
              aria-label="Close"
            >
              <Icon name="mdi:close" class="text-2xl text-white" />
            </button>

            <button
              v-if="images.length > 1"
              class="absolute left-0 top-1/2 -translate-y-1/2 h-12 w-12 rounded-full bg-black/50 hover:bg-black/70 grid place-items-center"
              @click="prevImage"
              aria-label="Prev"
            >
              <Icon name="mdi:chevron-left" class="text-3xl text-white keep-ltr" />
            </button>
            <button
              v-if="images.length > 1"
              class="absolute right-0 top-1/2 -translate-y-1/2 h-12 w-12 rounded-full bg-black/50 hover:bg-black/70 grid place-items-center"
              @click="nextImage"
              aria-label="Next"
            >
              <Icon name="mdi:chevron-right" class="text-3xl text-white keep-ltr" />
            </button>

            <div class="mt-3 flex items-center justify-center gap-2">
              <button
                v-for="(img, idx) in images"
                :key="img + idx"
                class="h-2.5 w-2.5 rounded-full"
                :class="idx === activeIndex ? 'bg-primary' : 'bg-white/30'"
                @click="activeIndex = idx"
                aria-label="Dot"
              />
            </div>
            <div class="mt-2 text-center text-xs text-white/70 keep-ltr">
              {{ activeIndex + 1 }} / {{ images.length }}
            </div>
          </div>
        </div>
      </div>
    </teleport>

    <ProductQuickPreviewModal />
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import SmartImage from '~/components/SmartImage.vue'
import ProductCard from '~/components/ProductCard.vue'
import ProductReviews from '~/components/ProductReviews.vue'
import ProductQuickPreviewModal from '~/components/ProductQuickPreviewModal.vue'

const { t } = useI18n()
const route = useRoute()
const api = useApi()
const cart = useCartStore()
const { has, toggle, load } = useWishlist()

const slug = computed(() => String(route.params.slug || ''))

const { data: product, pending } = await useAsyncData(
  () => `product:${slug.value}`,
  async () => {
    if (!slug.value) return null
    try {
      // IMPORTANT: useApi.get returns the data directly (not {data})
      return await api.get<any>(`/Products/slug/${encodeURIComponent(slug.value)}`)
    } catch {
      // fallback: sometimes route uses id instead of slug
      try {
        return await api.get<any>(`/Products/${encodeURIComponent(slug.value)}`)
      } catch {
        return null
      }
    }
  }
)

watch(
  () => product.value?.id,
  () => {
    load()
  },
  { immediate: true }
)

const isFav = computed(() => (product.value ? has(String(product.value.id)) : false))
function toggleFav() {
  if (!product.value) return
  toggle(String(product.value.id))
}

const images = computed<string[]>(() => {
  const p: any = product.value
  const arr: string[] = []

  const pushResolved = (v: any) => {
    if (!v) return
    const s = String(v)
    arr.push(api.buildAssetUrl(s))
  }

  // API shapes supported:
  // - images: string[]
  // - images: [{ id, url/path }]
  const list = p?.images || p?.Images || []
  if (Array.isArray(list)) {
    for (const im of list) {
      if (typeof im === 'string') pushResolved(im)
      else if (im?.id) arr.push(api.buildProductImageUrl(im.id))
      else pushResolved(im?.url || im?.path || im?.src || im?.imageUrl)
    }
  }

  pushResolved(p?.imageUrl)
  pushResolved(p?.thumbnailUrl)

  // unique
  return Array.from(new Set(arr.filter(Boolean)))
})

const activeIndex = ref(0)
const activeImage = computed(() => images.value[activeIndex.value] || images.value[0] || '')
watch(images, () => { activeIndex.value = 0 })

function prevImage() {
  if (!images.value.length) return
  activeIndex.value = (activeIndex.value - 1 + images.value.length) % images.value.length
}
function nextImage() {
  if (!images.value.length) return
  activeIndex.value = (activeIndex.value + 1) % images.value.length
}

function formatMoney(v: any) {
  const n = Number(v || 0)
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}

function addToCart(p: any) {
  cart.add(p)
}
function buyNow(p: any) {
  cart.add(p)
  navigateTo('/cart')
}

const waLink = computed(() => {
  const p: any = product.value
  const name = p?.name || ''
  const url = process.client ? window.location.href : ''
  const msg = `${t('product.waMessage')}\n${name}\n${url}`
  return `https://wa.me/?text=${encodeURIComponent(msg)}`
})

// Related products
const related = ref<any[]>([])
watch(product, async (p) => {
  if (!p) return
  try {
    const res = await api.get<any>(`/Products?page=1&pageSize=24`)
    const list = Array.isArray(res?.items) ? res.items : Array.isArray(res) ? res : []
    related.value = list.filter((x: any) => String(x.id) !== String(p.id)).slice(0, 4)
  } catch {
    related.value = []
  }
}, { immediate: true })

// Zoom on mouse
const zoomOn = ref(false)
const zoomPos = reactive({ x: 50, y: 50 })

function onZoomMove(e: MouseEvent) {
  const el = e.currentTarget as HTMLElement
  if (!el) return
  const rect = el.getBoundingClientRect()
  const x = ((e.clientX - rect.left) / rect.width) * 100
  const y = ((e.clientY - rect.top) / rect.height) * 100
  zoomPos.x = Math.max(0, Math.min(100, x))
  zoomPos.y = Math.max(0, Math.min(100, y))
}

const zoomStyle = computed(() => {
  if (!activeImage.value) return {}
  return {
    backgroundImage: `url('${activeImage.value}')`,
    backgroundRepeat: 'no-repeat',
    backgroundSize: '200% 200%',
    backgroundPosition: `${zoomPos.x}% ${zoomPos.y}%`,
    filter: 'drop-shadow(0 20px 60px rgba(0,0,0,.25))',
  }
})

// Fullscreen slider (with swipe)
const fullscreen = ref(false)
function openFullscreen() {
  if (!images.value.length) return
  fullscreen.value = true
}

const touch = reactive({ startX: 0, lastX: 0, dragging: false })
function onTouchStart(e: TouchEvent) {
  if (e.touches.length !== 1) return
  touch.dragging = true
  touch.startX = e.touches[0].clientX
  touch.lastX = touch.startX
}
function onTouchMove(e: TouchEvent) {
  if (!touch.dragging) return
  touch.lastX = e.touches[0].clientX
}
function onTouchEnd() {
  if (!touch.dragging) return
  const dx = touch.lastX - touch.startX
  touch.dragging = false
  if (Math.abs(dx) < 40) return
  if (dx > 0) prevImage()
  else nextImage()
}

useHead(() => ({
  title: product.value?.name ? `${product.value.name} | Ecommerce` : 'Product'
}))
</script>

<style scoped>
/* Lens layer: a subtle zoom overlay that follows the cursor */
div[style*="background-size"] {
  opacity: 0.9;
  mix-blend-mode: lighten;
}
</style>
