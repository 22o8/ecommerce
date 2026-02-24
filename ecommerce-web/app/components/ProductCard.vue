<template>
  <!-- الضغط على الكارد يفتح Quick Preview فقط -->
  <div
    role="button"
    tabindex="0"
    class="group card-soft overflow-hidden transition duration-300 hover:-translate-y-0.5 hover:shadow-lg"
    @click="openPreview"
    @keydown.enter.prevent="openPreview"
    @keydown.space.prevent="openPreview"
  >
    <div class="relative">
      <div class="relative aspect-[4/3] bg-black/20">
        <SmartImage
          :src="mainImage || ''"
          :alt="displayName"
          fit="cover"
          wrapper-class="w-full h-full"
          img-class="w-full h-full object-cover"
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
    </div>

    <div class="p-4 grid gap-3">
      <div class="min-w-0">
        <div class="flex items-start justify-between gap-3">
          <div class="font-extrabold line-clamp-1 rtl-text min-w-0">{{ displayName }}</div>
        </div>
        <div v-if="displayDescription" class="text-sm text-muted line-clamp-2 rtl-text">
          {{ displayDescription }}
        </div>

        <!-- أزرار (مفضلة/معاينة) -->
        <div class="flex items-center justify-end gap-2 mt-2">
          <button
            type="button"
            class="rounded-full border border-app bg-[rgba(var(--surface),.72)] hover:bg-[rgba(var(--surface),.95)] transition p-2"
            @click.stop.prevent="toggleFav"
            :aria-label="t('wishlist.toggle')"
          >
            <Icon :name="fav ? 'mdi:heart' : 'mdi:heart-outline'" class="text-lg" />
          </button>

          <button
            type="button"
            class="rounded-full border border-app bg-[rgba(var(--surface),.72)] hover:bg-[rgba(var(--surface),.95)] transition p-2"
            @click.stop.prevent="openPreview"
            :aria-label="t('products.quickPreview')"
          >
            <Icon name="mdi:eye-outline" class="text-lg" />
          </button>
        </div>
      </div>

      <div class="flex items-center justify-between gap-3">
        <div class="text-lg font-black keep-ltr">
          {{ formatPrice(priceValue) }}
        </div>

        <div class="flex flex-row items-center gap-2">
          <button
            type="button"
            class="inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg border border-app bg-surface hover:bg-white/5 transition text-xs"
            @click.stop.prevent="addToCart"
          >
            <Icon name="mdi:cart-plus" class="text-base" />
            <span class="rtl-text">{{ t('common.addToCart') }}</span>
          </button>

          <button
            type="button"
            class="inline-flex items-center px-2.5 py-1.5 rounded-lg border border-app bg-surface hover:bg-white/5 transition text-xs"
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
import { formatIqd } from '~/composables/useMoney'

const props = defineProps<{ product?: any; p?: any }>()
const { t } = useI18n()
const cart = useCartStore()
const { isInWishlist, toggle } = useWishlist()
const qp = useQuickPreview()
const router = useRouter()
const route = useRoute()
const { buildAssetUrl } = useApi()

const p = computed(() => props.product ?? props.p ?? {})

const displayName = computed(() => String(p.value?.title ?? p.value?.name ?? ''))
const displayDescription = computed(() => String(p.value?.description ?? '') || '')

const priceValue = computed(() => p.value?.priceIqd ?? p.value?.price ?? p.value?.priceUsd)

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

const wishlistKey = computed(() => String((p.value as any)?.id ?? (p.value as any)?.productId ?? p.value?.slug ?? ''))
const fav = computed(() => isInWishlist(wishlistKey.value))

function formatPrice(v: any) {
  return formatIqd(v)
}

function addToCart() {
  cart.add(p.value)
}

function buyNow() {
  cart.add(p.value)
  navigateTo('/cart')
}

function toggleFav() {
  toggle(wishlistKey.value)
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
