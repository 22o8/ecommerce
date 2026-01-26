<template>
  <div class="grid gap-6">
    <UiButton variant="ghost" class="w-fit" @click="back">
      <Icon name="mdi:arrow-left" class="keep-ltr text-xl" />
      <span class="rtl-text">{{ t('backToProducts') }}</span>
    </UiButton>

    <div v-if="loading" class="grid gap-6 lg:grid-cols-2">
      <div class="card-soft p-4">
        <div class="skeleton h-[420px]" />
      </div>
      <div class="card-soft p-6 grid gap-3">
        <div class="skeleton h-7 w-3/4" />
        <div class="skeleton h-5 w-1/2" />
        <div class="skeleton h-24" />
        <div class="skeleton h-12" />
      </div>
    </div>

    <div v-else-if="!p" class="card-soft p-10 text-center">
      <Icon name="mdi:alert-circle-outline" class="text-4xl opacity-70 mx-auto" />
      <div class="mt-3 font-bold rtl-text">{{ t('notFound') }}</div>
    </div>

    <div v-else class="grid gap-6 lg:grid-cols-2">
      <div class="card-soft overflow-hidden">
        <div class="p-4 md:p-5 grid gap-4">
          <!-- Main image (بدون قص) + تكبير عند الضغط -->
          <button
            type="button"
            class="relative rounded-2xl overflow-hidden bg-surface-2 grid place-items-center focus:outline-none"
            :class="gallery.length ? 'cursor-zoom-in' : ''"
            @click="gallery.length && (lightbox = true)"
          >
            <div class="h-[420px] w-full">
              <img
                v-if="currentImg"
                :src="currentImg"
                :alt="displayName"
                class="h-full w-full object-contain"
                loading="eager"
                decoding="async"
              />
              <div v-else class="text-center grid gap-2 px-6 py-16">
                <Icon name="mdi:image-outline" class="text-4xl opacity-70 mx-auto" />
                <div class="text-sm text-muted rtl-text">{{ t('noImage') }}</div>
              </div>
            </div>

            <div v-if="gallery.length" class="absolute top-3 right-3 rounded-full px-3 py-1 text-xs bg-black/40 text-white keep-ltr">
              {{ currentIndex + 1 }} / {{ gallery.length }}
            </div>
          </button>

          <!-- Thumbnails -->
          <div v-if="gallery.length > 1" class="flex gap-2 overflow-x-auto pb-1">
            <button
              v-for="(u, i) in gallery"
              :key="u + i"
              type="button"
              class="relative shrink-0 h-16 w-24 rounded-xl overflow-hidden bg-surface-2 border transition"
              :class="i === currentIndex ? 'border-white/60' : 'border-app hover:border-white/30'"
              @click="currentIndex = i"
            >
              <img :src="u" :alt="displayName" class="h-full w-full object-contain" loading="lazy" decoding="async" />
            </button>
          </div>
        </div>
      </div>

      <!-- Lightbox -->
      <Teleport to="body">
        <div v-if="lightbox" class="fixed inset-0 z-[60] bg-black/80 backdrop-blur-sm" @click.self="lightbox = false">
          <div class="h-full w-full grid place-items-center p-4">
            <div class="w-full max-w-6xl">
              <div class="flex items-center justify-between gap-2 mb-3">
                <div class="text-white/90 font-semibold rtl-text">{{ displayName }}</div>
                <div class="flex items-center gap-2">
                  <button class="rounded-full px-3 py-2 bg-white/10 hover:bg-white/20 text-white" @click="prevImg">
                    <Icon name="mdi:chevron-left" class="text-xl keep-ltr" />
                  </button>
                  <button class="rounded-full px-3 py-2 bg-white/10 hover:bg-white/20 text-white" @click="nextImg">
                    <Icon name="mdi:chevron-right" class="text-xl keep-ltr" />
                  </button>
                  <button class="rounded-full px-3 py-2 bg-white/10 hover:bg-white/20 text-white" @click="lightbox = false">
                    <Icon name="mdi:close" class="text-xl" />
                  </button>
                </div>
              </div>

              <div class="rounded-3xl overflow-hidden bg-black/20 border border-white/10">
                <div class="h-[75vh] w-full grid place-items-center">
                  <img :src="currentImg" :alt="displayName" class="max-h-[75vh] w-full object-contain" />
                </div>
              </div>

              <div class="mt-3 flex items-center justify-center gap-2 text-white/70 text-xs keep-ltr">
                <span>{{ currentIndex + 1 }} / {{ gallery.length }}</span>
                <span>•</span>
                <span>Esc</span>
              </div>
            </div>
          </div>
        </div>
      </Teleport>

      <div class="card-soft p-6 md:p-8 grid gap-4">
        <div>
          <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ displayName }}</h1>
          <div class="mt-2 text-muted rtl-text">{{ p.description || '' }}</div>
        </div>

        <div class="flex items-center justify-between">
          <div class="text-sm text-muted rtl-text">{{ t('price') }}</div>
          <div class="text-2xl font-black keep-ltr">{{ fmt(displayPrice) }}</div>
        </div>

        <div class="grad-line" />

        <div class="grid gap-3">
          <UiButton v-if="auth.isAuthed" @click="buy" :loading="buying">
            <Icon name="mdi:cart-outline" class="text-lg" />
            <span class="rtl-text">{{ t('buyNow') }}</span>
          </UiButton>

          <NuxtLink v-else to="/login">
            <UiButton>
              <Icon name="mdi:login-variant" class="text-lg" />
              <span class="rtl-text">{{ t('loginToBuy') }}</span>
            </UiButton>
          </NuxtLink>

          <a class="rounded-2xl border border-app bg-surface px-4 py-3 text-sm hover:bg-surface-2 transition keep-ltr" :href="waOrderLink" target="_blank" rel="noreferrer">
            <Icon name="mdi:whatsapp" class="inline-block text-lg align-middle" />
            <span class="ml-2 rtl-text">{{ t('whatsappOrder') }}</span>
          </a>

          <p v-if="msg" class="text-sm rtl-text" :class="ok ? 'text-[rgb(var(--success))]' : 'text-[rgb(var(--danger))]'">{{ msg }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import { useApi } from '~/composables/useApi'

const { t } = useI18n()
const auth = useAuthStore()
const api = useApi()
const route = useRoute()
const router = useRouter()
const config = useRuntimeConfig()

const loading = ref(true)
const buying = ref(false)
const msg = ref('')
const ok = ref(false)

const p = ref<any>(null)

const displayName = computed(() => String(p.value?.title ?? p.value?.name ?? ''))
const displayPrice = computed(() => Number(p.value?.priceUsd ?? p.value?.price ?? 0))

// صور المنتج (تعمل مع: images: [{url}] أو images: [string] أو coverUrl)
const images = computed<string[]>(() => {
  const list = (p.value?.images ?? []) as any[]
  const urls = list
    .map((x) => (typeof x === 'string' ? x : x?.url))
    .filter(Boolean)
    .map((u) => api.buildAssetUrl(String(u)))

  const fallback = p.value?.coverUrl || p.value?.imageUrl || p.value?.coverImage
  if (urls.length === 0 && fallback) urls.push(api.buildAssetUrl(String(fallback)))
  return urls
})

const selectedIndex = ref(0)
watch(images, () => {
  selectedIndex.value = 0
})

const activeImage = computed(() => images.value[selectedIndex.value])

const lightboxOpen = ref(false)
const openLightbox = () => {
  if (!activeImage.value) return
  lightboxOpen.value = true
}
const closeLightbox = () => (lightboxOpen.value = false)
const nextImage = () => {
  if (!images.value.length) return
  selectedIndex.value = (selectedIndex.value + 1) % images.value.length
}
const prevImage = () => {
  if (!images.value.length) return
  selectedIndex.value = (selectedIndex.value - 1 + images.value.length) % images.value.length
}

const waOrderLink = computed(() => {
  const n = String((config.public as any).whatsappNumber || '').replace(/[^0-9]/g, '')
  const text = encodeURIComponent(`Order: ${displayName.value} | Price: ${displayPrice.value}`)
  return n ? `https://wa.me/${n}?text=${text}` : '#'
})

function fmt(v: any) {
  const n = Number(v || 0)
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}

function back() { router.push('/products') }

async function fetchProduct() {
  loading.value = true
  msg.value = ''
  try {
    const slug = String(route.params.slug || '')
    p.value = await api.get(`/Products/slug/${encodeURIComponent(slug)}`).catch(async () => {
      return await api.get(`/Products/${slug}`)
    })
  } catch {
    p.value = null
  } finally {
    loading.value = false
  }
}

async function buy() {
  if (!p.value) return
  buying.value = true
  msg.value = ''
  ok.value = false
  try {
    // Swagger: POST /api/Checkout/products { productId, quantity }
    const res: any = await api.post('/Checkout/products', { productId: p.value.id, quantity: 1 })
    ok.value = true
    msg.value = `تم إنشاء الطلب (#${res?.orderId || ''}) وحالته ${res?.status || ''}. بانتظار تأكيد الدفع.`
    // اعرض صفحة طلباتي لو تحب
    // router.push('/orders')
  } catch (e: any) {
    ok.value = false
    msg.value = e?.data?.message || e?.message || t('buyFailed')
  } finally {
    buying.value = false
  }
}

onMounted(fetchProduct)
</script>

