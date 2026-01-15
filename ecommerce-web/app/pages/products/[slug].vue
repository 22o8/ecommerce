<template>
  <div class="space-y-6">
    <div v-if="pending" class="rounded-3xl border border-white/10 bg-white/5 p-6">
      {{ t('loading') }}
    </div>

    <div v-else-if="errorMsg" class="rounded-3xl border border-red-500/30 bg-red-500/10 p-6 text-red-200">
      {{ errorMsg }}
    </div>
    <div v-else class="grid gap-6 md:grid-cols-3">
      <div class="md:col-span-2 rounded-3xl border border-white/10 bg-white/5 p-6">
        <!-- Gallery -->
        <div class="grid gap-4 md:grid-cols-5">
          <div class="md:col-span-3 rounded-3xl overflow-hidden border border-white/10 bg-black/20">
            <img
              v-if="activeImage"
              :src="activeImage"
              :alt="product.title"
              class="w-full h-full object-cover aspect-square"
            />
            <div v-else class="aspect-square grid place-items-center text-white/60">
              {{ t('noImage') }}
            </div>
          </div>

          <div class="md:col-span-2 grid gap-3">
            <h1 class="text-3xl font-black leading-tight">{{ product.title }}</h1>
            <p class="text-sm text-white/70">{{ product.description }}</p>

            <div v-if="thumbs.length" class="grid grid-cols-4 gap-2">
              <button
                v-for="(im, idx) in thumbs"
                :key="idx"
                class="rounded-2xl overflow-hidden border"
                :style="{ borderColor: selectedIndex === idx ? 'rgba(99,102,241,.55)' : 'rgba(255,255,255,.10)' }"
                type="button"
                @click="selectedIndex = idx"
              >
                <img :src="im" class="w-full h-full object-cover aspect-square" />
              </button>
            </div>
          </div>
        </div>

        <div class="mt-6 rounded-2xl border border-white/10 bg-black/20 p-4">
          <div class="text-sm text-white/60">{{ t('productDetails') }}</div>
          <div class="mt-2 text-sm">
            <div><span class="text-white/60">Slug:</span> <span class="font-bold">{{ product.slug }}</span></div>
            <div class="mt-1"><span class="text-white/60">{{ t('price') }}:</span> <span class="font-black">${{ product.priceUsd }}</span></div>
          </div>
        </div>
      </div>

      <div class="rounded-3xl border border-white/10 bg-white/5 p-6">
        <div class="text-sm text-white/60">{{ t('price') }}</div>
        <div class="mt-2 text-2xl font-black">${{ product.priceUsd }}</div>

        <div class="mt-5 grid gap-3">
          <AppButton v-if="auth.isAuthed" type="button" @click="buyNow">
            {{ t('buy') }}
          </AppButton>

          <AppButton type="button" variant="soft" @click="orderViaWhatsApp">
            {{ t('whatsappOrder') }}
          </AppButton>
          <AppButton v-if="!auth.isAuthed" type="button" variant="soft" @click="goLoginForCheckout">
            {{ t('loginToBuy') }}
          </AppButton>

          <NuxtLink to="/products" class="text-sm font-extrabold text-blue-300 hover:underline">
            ← {{ t('backToProducts') }}
          </NuxtLink>
        </div>

        <div v-if="actionError" class="mt-4 rounded-2xl border border-red-500/30 bg-red-500/10 p-3 text-sm text-red-200">
          {{ actionError }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRoute, navigateTo } from '#app'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'
import { useSiteMeta } from '~/composables/useSiteMeta'
import { useAuthStore } from '~/stores/auth'

const route = useRoute()
const api = useApi()
const auth = useAuthStore()
const { t } = useI18n()

const runtimeConfig = useRuntimeConfig()
const whatsappNumber = computed(() => String((runtimeConfig.public as any)?.whatsappNumber || '').replace(/^\+/, ''))

const slug = computed(() => String(route.params.slug || ''))

type Product = {
  id: string
  title: string
  slug: string
  description: string
  priceUsd: number
  isPublished?: boolean
  coverImage?: string | null
  images?: Array<{ url: string; alt?: string | null; sortOrder?: number | null }>
}

const errorMsg = ref<string | null>(null)
const actionError = ref<string>('')

function extractErrorMessage(e: any) {
  return e?.data?.message || e?.data || e?.message || 'صار خطأ'
}

const { data, pending } = await useAsyncData(`product_${slug.value}`, async () => {
  return await api.get<Product>(`/Products/slug/${slug.value}`)
})

const product = computed(() => data.value as Product)

// صور المنتج تأتي جاهزة من API: GET /api/Products/slug/{slug}
const selectedIndex = ref(0)

const thumbs = computed(() => {
  const p = product.value
  const list = (p?.images || [])
    .slice()
    .sort((a, b) => Number(a.sortOrder ?? 0) - Number(b.sortOrder ?? 0))
    .map((x) => api.buildAssetUrl(String(x?.url || '')))
    .filter(Boolean)

  // إذا ماكو صور، جرّب coverImage
  if (!list.length && p?.coverImage) {
    const c = api.buildAssetUrl(String(p.coverImage))
    if (c) list.push(c)
  }
  return list
})

const activeImage = computed(() => thumbs.value[selectedIndex.value] || '')

watchEffect(() => {
  if (selectedIndex.value >= thumbs.value.length) selectedIndex.value = 0
})

watchEffect(() => {
  if (!pending.value && !data.value) errorMsg.value = t('requestFailed')
})

useSiteMeta({
  title: product.value?.title ? `${product.value.title} | Ecommerce` : 'Product | Ecommerce',
  description: product.value?.description || 'Product details',
  path: `/products/${slug.value}`,
})

// JSON-LD (Product)
useHead(() => {
  if (!product.value) return {}
  return {
    script: [
      {
        type: 'application/ld+json',
        children: JSON.stringify({
          '@context': 'https://schema.org',
          '@type': 'Product',
          name: product.value.title,
          description: product.value.description,
          sku: product.value.id,
          offers: {
            '@type': 'Offer',
            priceCurrency: 'USD',
            price: product.value.priceUsd,
            availability: 'https://schema.org/InStock',
          },
        }),
      },
    ],
  }
})

async function goLoginForCheckout() {
  await navigateTo('/login')
}

// مبدئياً: ننشئ Order بالباك ثم نودّي المستخدم لصفحة order للتحميل/الدفع حسب نظامك
async function buyNow() {
  actionError.value = ''
  try {
    // ✅ Swagger: POST /api/Checkout/products
    // ينشئ Order + Payment mock ثم تقدر تفتح صفحة الطلب
    const res: any = await api.post('/Checkout/products', {
      productId: product.value.id,
      quantity: 1,
    })

    const orderId = res?.orderId || res?.id
    if (!orderId) throw new Error('Order id missing')
    await navigateTo(`/orders/${orderId}`)
  } catch (e: any) {
    actionError.value = extractErrorMessage(e)
  }
}

function orderViaWhatsApp() {
  const number = whatsappNumber.value
  if (!number) {
    actionError.value = 'WHATSAPP_NUMBER غير مضبوط'
    return
  }
  const title = product.value?.title || 'Product'
  const price = product.value?.priceUsd != null ? `$${product.value.priceUsd}` : ''
  const url = typeof window !== 'undefined' ? window.location.href : ''
  const msg = encodeURIComponent(`طلب منتج: ${title}\nالسعر: ${price}\nالرابط: ${url}`)
  if (typeof window !== 'undefined') {
    window.open(`https://wa.me/${number}?text=${msg}`, '_blank')
  }
}

</script>
