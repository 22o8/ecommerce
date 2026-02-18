<template>
  <!-- تم إلغاء صفحة تفاصيل المنتج: الضغط على الكارد يفتح Quick Preview فقط -->
  <div
    role="button"
    tabindex="0"
    class="group card-soft overflow-hidden transition duration-300 hover:-translate-y-0.5 hover:shadow-lg"
    @click="openPreview"
    @keydown.enter.prevent="openPreview"
    @keydown.space.prevent="openPreview"
  >
    <div class="relative">
      <div class="relative aspect-square bg-black/20">
        <SmartImage
          :src="mainImage || ''"
          :alt="p.name"
          fit="cover"
          wrapper-class="w-full h-full"
        />
<div class="absolute inset-0 pointer-events-none opacity-0 group-hover:opacity-100 transition">
          <div class="absolute inset-0 bg-gradient-to-t from-black/40 via-black/0 to-black/0" />
        </div>
      </div>

      <div class="absolute top-3 left-3 flex items-center gap-2">
        <div
          v-if="isNew"
          class="px-3 py-1 rounded-full bg-white/10 backdrop-blur border border-white/10 text-xs"
        >
          <span class="rtl-text">{{ t('common.new') }}</span>
        </div>
      </div>

	      <div class="absolute top-3 right-3 flex items-center gap-2">
        <button
	          class="rounded-full bg-white/10 backdrop-blur border border-white/10 p-3 sm:p-2 hover:bg-white/15 transition"
          @click.stop.prevent="toggleFav"
          :aria-label="t('wishlist.toggle')"
        >
	          <Icon
            :name="fav ? 'mdi:heart' : 'mdi:heart-outline'"
	            class="text-xl sm:text-lg"
          />
        </button>

        <button
	          class="rounded-full bg-white/10 backdrop-blur border border-white/10 p-3 sm:p-2 hover:bg-white/15 transition"
          @click.stop.prevent="openPreview"
          :aria-label="t('products.quickPreview')"
        >
	          <Icon name="mdi:eye-outline" class="text-xl sm:text-lg" />
        </button>
      </div>
    </div>

    <div class="p-4 grid gap-3">
      <div class="min-w-0">
        <div class="font-extrabold line-clamp-1 rtl-text">{{ p.name }}</div>
        <div v-if="p.description" class="text-sm text-muted line-clamp-2 rtl-text">
          {{ p.description }}
        </div>
      </div>

      <div class="flex items-center justify-between gap-3">
        <div class="text-lg font-black keep-ltr">
          {{ formatPrice(p.priceUsd) }}
        </div>

        <!-- أزرار مريحة على الهاتف: تتحول إلى عمود وتاخذ عرض كامل -->
        <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-2">
          <button
            class="px-3 py-2 rounded-xl border border-app bg-surface hover:bg-white/5 transition text-sm w-full sm:w-auto"
            @click.stop.prevent="addToCart"
          >
            <Icon name="mdi:cart-plus" class="text-lg" />
            <span class="rtl-text ml-1">{{ t('common.addToCart') }}</span>
          </button>

          <button
            class="px-3 py-2 rounded-xl border border-app bg-surface hover:bg-white/5 transition text-sm w-full sm:w-auto"
            @click.stop.prevent="buyNow"
          >
            <span class="rtl-text">{{ t('common.buy') }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const props = defineProps<{ p: any }>()
const { t } = useI18n()
const cart = useCartStore()
const { isInWishlist, toggle } = useWishlist()
const qp = useQuickPreview()
const router = useRouter()
const route = useRoute()
const { buildAssetUrl } = useApi()

const p = computed(() => props.p)

const mainImage = computed(() => {
  const raw = p.value?.images?.[0]?.url || p.value?.images?.[0] || p.value?.imageUrl || p.value?.image || ''
  const resolved = raw ? buildAssetUrl(String(raw)) : ''
  return resolved || '/hero-placeholder.svg'
})

const isNew = computed(() => {
  const created = p.value?.createdAt ? new Date(p.value.createdAt).getTime() : 0
  if (!created) return false
  const days = (Date.now() - created) / (1000 * 60 * 60 * 24)
  return days <= 14
})

const fav = computed(() => isInWishlist(String(p.value?.id ?? '')))

function formatPrice(v: any) {
  const n = Number(v ?? 0)
  try {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)
  } catch {
    return `$${n.toFixed(2)}`
  }
}

function addToCart() {
  cart.add(p.value)
}

function buyNow() {
  cart.add(p.value)
  navigateTo('/cart')
}

function toggleFav() {
  toggle(String(p.value?.id ?? ''))
}

function openPreview() {
  qp.show(p.value)

  // رابط قابل للمشاركة: نضيف ?p=<id> لنفس الصفحة
  const id = String(p.value?.id ?? p.value?.slug ?? '')
  if (id) {
    const q: Record<string, any> = { ...route.query, p: id }
    router.replace({ query: q })
  }
}
</script>
