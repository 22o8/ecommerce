<template>
  <div class="mx-auto max-w-6xl grid gap-8 lg:grid-cols-[1fr_360px]">
    <div class="card-soft p-6 md:p-8">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-black rtl-text">{{ t('cartTitle') }}</h1>
          <p class="text-sm text-muted rtl-text mt-1">{{ t('cartSubtitle') }}</p>
        </div>

        <UiButton v-if="cart.count" variant="ghost" @click="cart.clear()">
          <Icon name="mdi:trash-can-outline" class="text-lg" />
          <span class="rtl-text">{{ t('clearCart') }}</span>
        </UiButton>
      </div>

      <div v-if="!cart.items.length" class="mt-8 rounded-3xl border border-app bg-surface p-10 text-center">
        <Icon name="mdi:cart-off" class="text-5xl opacity-70" />
        <div class="mt-3 text-lg font-bold rtl-text">{{ t('cartEmpty') }}</div>
        <NuxtLink to="/products" class="mt-6 inline-flex">
          <UiButton>
            <Icon name="mdi:shopping-outline" class="text-lg" />
            <span class="rtl-text">{{ t('browseProducts') }}</span>
          </UiButton>
        </NuxtLink>
      </div>

      <div v-else class="mt-8 grid gap-4">
        <div
          v-for="it in cart.items"
          :key="it.id"
          class="rounded-3xl border border-app bg-surface p-4 hover:bg-surface-2 transition"
        >
          <div class="flex gap-4">
            <div class="h-20 w-20 rounded-2xl overflow-hidden bg-black/20 shrink-0">
              <img v-if="it.imageUrl" :src="buildAssetUrl(it.imageUrl)" :alt="it.title" class="h-full w-full object-cover" />
            </div>

            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between gap-3">
                <div class="min-w-0">
                  <div class="font-bold rtl-text truncate text-base">{{ it.title }}</div>
                  <div class="text-sm text-muted rtl-text">{{ fmtMoney(it.price) }}</div>
                  <div v-if="Number(it.stockQuantity || 0) <= 0" class="mt-1 inline-flex rounded-full bg-[rgb(var(--danger))]/15 px-3 py-1 text-xs font-bold text-[rgb(var(--danger))] rtl-text">{{ t('common.unavailable') }}</div>
                </div>

                <button class="icon-btn bg-surface-2 border border-app text-muted hover:text-[rgb(var(--danger))]" @click="cart.remove(it.id)">
                  <Icon name="mdi:close" class="text-xl" />
                </button>
              </div>

              <div class="mt-4 flex items-center gap-3">
                <UiButton size="sm" variant="ghost" @click="cart.decrease(it.id)" :disabled="Number(it.stockQuantity || 0) <= 0">
                  <Icon name="mdi:minus" />
                </UiButton>

                <div class="min-w-[40px] text-center font-bold">{{ it.quantity }}</div>

                <UiButton size="sm" variant="ghost" @click="cart.increase(it.id)" :disabled="it.quantity >= Number(it.stockQuantity || 99)">
                  <Icon name="mdi:plus" />
                </UiButton>

                <div class="ml-auto font-black">{{ fmtMoney(it.price * it.quantity) }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="card-soft p-6 md:p-8 h-fit sticky top-24">
      <h2 class="text-xl font-extrabold rtl-text">{{ t('checkout') }}</h2>

      <div class="mt-6 grid gap-3 text-sm">
        <div class="flex justify-between">
          <span class="rtl-text">{{ t('itemsCount') }}</span>
          <span class="font-bold">{{ cart.count }}</span>
        </div>

        <div class="flex justify-between">
          <span class="rtl-text">{{ t('total') }}</span>
          <span class="font-black text-lg">{{ fmtMoney(cart.total) }}</span>
        </div>

        <div v-if="appliedCoupon" class="flex justify-between">
          <span class="rtl-text">{{ t('cart.couponDiscount') }}</span>
          <span class="font-bold text-[rgb(var(--success))]">- {{ fmtMoney(appliedCoupon.discountAmountIqd || 0) }}</span>
        </div>

        <div v-if="appliedCoupon" class="flex justify-between">
          <span class="rtl-text">{{ t('cart.finalTotal') }}</span>
          <span class="font-black text-lg">{{ fmtMoney(finalTotal) }}</span>
        </div>
      </div>

      <div class="mt-6 grid gap-2">
        <label class="text-sm rtl-text font-bold">{{ t('cart.coupon') }}</label>
        <div class="flex gap-2">
          <input v-model="couponCode" class="flex-1 rounded-2xl border border-app bg-surface px-4 py-3 outline-none" :placeholder="t('cart.couponPlaceholder')" />
          <UiButton variant="secondary" :disabled="couponLoading || !couponCode.trim()" @click="applyCoupon">{{ couponLoading ? t('common.loading') : t('cart.applyCoupon') }}</UiButton>
        </div>
        <div v-if="couponError" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ couponError }}</div>
        <div v-if="appliedCoupon" class="rounded-2xl border border-app bg-surface p-3 text-sm">
          <div class="flex items-center justify-between gap-3">
            <div class="rtl-text"><strong>{{ appliedCoupon.code }}</strong> - {{ appliedCoupon.title }}</div>
            <button class="text-[rgb(var(--danger))] rtl-text" @click="removeCoupon">{{ t('common.remove') }}</button>
          </div>
          <div class="mt-1 rtl-text text-muted">{{ t('cart.couponDiscount') }}: {{ fmtMoney(appliedCoupon.discountAmountIqd || 0) }}</div>
        </div>
      </div>

      <div class="mt-8 grid gap-3">
        <UiButton :disabled="!cart.items.length || hasUnavailableItems" @click="openWhatsApp">
          <Icon name="mdi:whatsapp" class="text-lg" />
          <span class="rtl-text">{{ t('buyNow') }}</span>
        </UiButton>

        <p v-if="hasUnavailableItems" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ t('common.unavailable') }}: بعض المنتجات نفدت كميتها.</p>
        <p v-if="error" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ error }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import { buildAssetUrl, useApi } from '~/composables/useApi'
import { formatIqd } from '~/composables/useMoney'

const { t } = useI18n()
const cart = useCartStore()
const auth = useAuthStore()
const profile = useProfileStore()
const api = useApi()
const error = ref('')
const couponCode = ref('')
const couponLoading = ref(false)
const couponError = ref('')
const appliedCoupon = useState<any | null>('cart_coupon_applied', () => null)
const finalTotal = computed(() => Math.max(0, Number(cart.total || 0) - Number(appliedCoupon.value?.discountAmountIqd || 0)))
const hasUnavailableItems = computed(() => cart.items.some((x: any) => Number(x.stockQuantity || 0) <= 0))

const { openWhatsappForCart } = useWhatsappCheckout()

function fmtMoney(v: any) {
  return formatIqd(v)
}

function getDeviceKey() {
  const key = 'coupon_device_key'
  const existing = localStorage.getItem(key)
  if (existing) return existing
  const value = `${Date.now()}-${Math.random().toString(36).slice(2)}-${navigator.userAgent}`
  localStorage.setItem(key, value)
  return value
}

async function applyCoupon() {
  couponError.value = ''
  couponLoading.value = true
  try {
    const res: any = await api.get('/Coupons/validate', { code: couponCode.value.trim(), subtotalIqd: Number(cart.total || 0), deviceKey: process.client ? getDeviceKey() : '', productIds: cart.items.map((x:any) => x.id).join(',') })
    appliedCoupon.value = res
  } catch (e: any) {
    appliedCoupon.value = null
    couponError.value = String(e?.data?.message || e?.message || 'تعذر التحقق من الكوبون')
  } finally {
    couponLoading.value = false
  }
}

function removeCoupon() {
  appliedCoupon.value = null
  couponCode.value = ''
  couponError.value = ''
}

async function openWhatsApp() {
  error.value = ''
  try {
    await openWhatsappForCart()
    cart.clear()
    removeCoupon()
  } catch (e: any) {
    const msg = String(e?.data?.message || e?.message || '')
    if (/unauthor/i.test(msg)) error.value = 'يرجى تسجيل الدخول أولاً لإكمال الطلب.'
    else error.value = msg || 'تعذر إنشاء الطلب حالياً.'
  }
}

watch(() => cart.total, async (v) => {
  if (!appliedCoupon.value?.code) return
  if (!v) {
    appliedCoupon.value = null
    return
  }
  try {
    const res: any = await api.get('/Coupons/validate', { code: appliedCoupon.value.code, subtotalIqd: Number(v || 0), deviceKey: process.client ? getDeviceKey() : '', productIds: cart.items.map((x:any) => x.id).join(',') })
    appliedCoupon.value = res
  } catch {
    appliedCoupon.value = null
  }
})

onMounted(() => profile.hydrateFromAuth(auth.token || ''))
</script>
