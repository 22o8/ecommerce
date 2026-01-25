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
        <div class="h-[420px] bg-surface-2 grid place-items-center">
          <img v-if="img" :src="img" class="h-full w-full object-cover" :alt="displayName" />
          <div v-else class="text-center grid gap-2 px-6">
            <Icon name="mdi:image-outline" class="text-4xl opacity-70 mx-auto" />
            <div class="text-sm text-muted rtl-text">{{ t('noImage') }}</div>
          </div>
        </div>
      </div>

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
          <UiButton variant="secondary" @click="addToCart">
            <Icon name="mdi:cart-plus" class="text-lg" />
            <span class="rtl-text">{{ t('addToCart') }}</span>
          </UiButton>
          <NuxtLink to="/cart">
            <UiButton variant="ghost">
              <Icon name="mdi:cart-outline" class="text-lg" />
              <span class="rtl-text">{{ t('goToCart') }}</span>
            </UiButton>
          </NuxtLink>
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
import { useI18n } from '~/app/composables/useI18n'
import { useAuthStore } from '~/app/stores/auth'
import { useCartStore } from '~/app/stores/cart'
import { useApi } from '~/composables/useApi'

const { t } = useI18n()
const auth = useAuthStore()
const cart = useCartStore()
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

const img = computed(() => {
  const first =
    p.value?.images?.[0]?.url ||
    p.value?.images?.[0] ||
    p.value?.coverImage ||
    p.value?.imageUrl ||
    p.value?.image ||
    ''
  return api.buildAssetUrl(first)
})

const waOrderLink = computed(() => {
  const n = String((config.public as any).whatsappNumber || '').replace(/[^0-9]/g, '')
  const text = encodeURIComponent(`Order: ${displayName.value} | Price: ${displayPrice.value}`)
  return n ? `https://wa.me/${n}?text=${text}` : '#'
})

function fmt(v: any) {
  const n = Number(v || 0)
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}

function addToCart(){
  if (!p.value) return
  cart.add(p.value, 1)
  msg.value = t('addedToCart')
  ok.value = true
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

