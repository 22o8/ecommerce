<template>
  <div class="mx-auto max-w-6xl px-3 sm:px-4 lg:px-0">
    <div class="grid gap-4 lg:gap-8 lg:grid-cols-[minmax(0,1fr)_360px]">
      <div class="card-soft p-4 sm:p-5 md:p-8 min-w-0 overflow-hidden">
        <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div class="min-w-0">
            <h1 class="text-2xl sm:text-3xl font-black rtl-text">{{ t('cartTitle') }}</h1>
            <p class="text-sm text-muted rtl-text mt-1">{{ t('cartSubtitle') }}</p>
          </div>

          <UiButton v-if="cart.count" variant="ghost" class="w-full sm:w-auto justify-center" @click="cart.clear()">
            <Icon name="mdi:trash-can-outline" class="text-lg" />
            <span class="rtl-text">{{ t('clearCart') }}</span>
          </UiButton>
        </div>

        <div v-if="!cart.items.length" class="mt-6 sm:mt-8 rounded-3xl border border-app bg-surface p-6 sm:p-10 text-center">
          <Icon name="mdi:cart-off" class="text-5xl opacity-70" />
          <div class="mt-3 text-lg font-bold rtl-text">{{ t('cartEmpty') }}</div>
          <NuxtLink to="/products" class="mt-6 inline-flex">
            <UiButton>
              <Icon name="mdi:shopping-outline" class="text-lg" />
              <span class="rtl-text">{{ t('browseProducts') }}</span>
            </UiButton>
          </NuxtLink>
        </div>

        <div v-else class="mt-6 sm:mt-8 grid gap-4">
          <div
            v-for="it in cart.items"
            :key="it.id"
            class="rounded-3xl border border-app bg-surface p-3 sm:p-4 hover:bg-surface-2 transition overflow-hidden"
          >
            <div class="flex flex-col gap-4 sm:flex-row">
              <div class="mx-auto sm:mx-0 h-24 w-24 sm:h-20 sm:w-20 rounded-2xl overflow-hidden bg-black/20 shrink-0">
                <img v-if="it.imageUrl" :src="buildAssetUrl(it.imageUrl)" :alt="it.title" class="h-full w-full object-cover" />
              </div>

              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between gap-3">
                  <div class="min-w-0 flex-1">
                    <div class="font-bold rtl-text text-sm sm:text-base break-words leading-7">{{ it.title }}</div>
                    <div class="text-sm text-muted rtl-text mt-1">{{ fmtMoney(it.price) }}</div>
                    <div v-if="Number(it.stockQuantity || 0) <= 0" class="mt-2 inline-flex rounded-full bg-[rgb(var(--danger))]/15 px-3 py-1 text-xs font-bold text-[rgb(var(--danger))] rtl-text">{{ t('common.unavailable') }}</div>
                  </div>

                  <button class="icon-btn bg-surface-2 border border-app text-muted hover:text-[rgb(var(--danger))] shrink-0" @click="cart.remove(it.id)">
                    <Icon name="mdi:close" class="text-xl" />
                  </button>
                </div>

                <div class="mt-4 flex flex-wrap items-center gap-3 sm:gap-4">
                  <div class="flex items-center gap-2 rounded-2xl border border-app bg-surface-2 px-2 py-1.5">
                    <UiButton size="sm" variant="ghost" @click="cart.decrease(it.id)" :disabled="Number(it.stockQuantity || 0) <= 0">
                      <Icon name="mdi:minus" />
                    </UiButton>

                    <div class="min-w-[36px] text-center font-bold">{{ it.quantity }}</div>

                    <UiButton size="sm" variant="ghost" @click="cart.increase(it.id)" :disabled="it.quantity >= Number(it.stockQuantity || 99)">
                      <Icon name="mdi:plus" />
                    </UiButton>
                  </div>

                  <div class="w-full sm:w-auto sm:ml-auto text-start sm:text-end font-black text-base break-words">
                    {{ fmtMoney(it.price * it.quantity) }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="card-soft p-4 sm:p-5 md:p-8 h-fit lg:sticky lg:top-24 min-w-0 overflow-hidden">
        <h2 class="text-xl font-extrabold rtl-text">{{ t('checkout') }}</h2>

        <div class="mt-6 grid gap-3 text-sm">
          <div class="flex items-center justify-between gap-3">
            <span class="rtl-text">{{ t('itemsCount') }}</span>
            <span class="font-bold shrink-0">{{ cart.count }}</span>
          </div>

          <div class="flex items-center justify-between gap-3">
            <span class="rtl-text">{{ t('total') }}</span>
            <span class="font-black text-lg shrink-0 text-end">{{ fmtMoney(cart.total) }}</span>
          </div>

          <div v-if="appliedCoupon" class="flex items-center justify-between gap-3">
            <span class="rtl-text">{{ t('cart.couponDiscount') }}</span>
            <span class="font-bold text-[rgb(var(--success))] shrink-0 text-end">- {{ fmtMoney(appliedCoupon.discountAmountIqd || 0) }}</span>
          </div>

          <div v-if="appliedCoupon" class="flex items-center justify-between gap-3">
            <span class="rtl-text">{{ t('cart.finalTotal') }}</span>
            <span class="font-black text-lg shrink-0 text-end">{{ fmtMoney(finalTotal) }}</span>
          </div>
        </div>

        <div class="mt-6 grid gap-2 min-w-0">
          <label class="text-sm rtl-text font-bold">{{ t('cart.coupon') }}</label>
          <div class="flex flex-col gap-2 sm:flex-row min-w-0">
            <input
              v-model="couponCode"
              class="w-full min-w-0 rounded-2xl border border-app bg-surface px-4 py-3 outline-none"
              :placeholder="t('cart.couponPlaceholder')"
              @keydown.enter.prevent="applyCoupon"
            />
            <UiButton variant="secondary" class="w-full sm:w-auto justify-center" :disabled="couponLoading || !couponCode.trim()" @click="applyCoupon">
              {{ couponLoading ? t('common.loading') : t('cart.applyCoupon') }}
            </UiButton>
          </div>
          <div v-if="couponError" class="rounded-2xl border border-[rgb(var(--danger))]/30 bg-[rgb(var(--danger))]/10 px-3 py-2 text-sm rtl-text text-[rgb(var(--danger))] break-words">{{ couponError }}</div>
          <div v-if="appliedCoupon" class="rounded-2xl border border-app bg-surface p-3 text-sm min-w-0">
            <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div class="rtl-text break-words"><strong>{{ appliedCoupon.code }}</strong> - {{ appliedCoupon.title }}</div>
              <button class="text-[rgb(var(--danger))] rtl-text text-start sm:text-end" @click="removeCoupon">{{ t('common.remove') }}</button>
            </div>
            <div class="mt-1 rtl-text text-muted break-words">{{ t('cart.couponDiscount') }}: {{ fmtMoney(appliedCoupon.discountAmountIqd || 0) }}</div>
          </div>
        </div>

        <div class="mt-8 grid gap-3">
          <UiButton class="w-full justify-center" :disabled="!cart.items.length || hasUnavailableItems" @click="openWhatsApp">
            <Icon name="mdi:whatsapp" class="text-lg" />
            <span class="rtl-text">{{ t('buyNow') }}</span>
          </UiButton>

          <p v-if="hasUnavailableItems" class="text-sm rtl-text text-[rgb(var(--danger))] break-words">{{ t('common.unavailable') }}: بعض المنتجات نفدت كميتها.</p>
          <p v-if="error" class="rounded-2xl border border-[rgb(var(--danger))]/30 bg-[rgb(var(--danger))]/10 px-3 py-2 text-sm rtl-text text-[rgb(var(--danger))] break-words">{{ error }}</p>
        </div>
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

function normalizeApiMessage(input: any, fallback: string) {
  const raw = String(input || '').trim()
  if (!raw) return fallback
  if (/^\[(GET|POST|PUT|PATCH|DELETE|HEAD)\]\s/i.test(raw)) return fallback
  if (/fetch failed|Failed to fetch|NetworkError|Load failed/i.test(raw)) return 'تعذر الاتصال بالخادم حالياً. حاول مرة أخرى.'
  return raw
}

function mapErrorMessage(e: any, context: 'coupon' | 'checkout' = 'coupon') {
  const status = Number(e?.statusCode || e?.status || e?.response?.status || 0)
  const data = e?.data ?? e?.response?._data
  const raw =
    data?.message ||
    data?.error ||
    (typeof data === 'string' ? data : '') ||
    e?.friendlyMessage ||
    e?.message ||
    ''

  const text = String(raw || '').trim().toLowerCase()

  if (status === 401 || /unauthor|invalid token|login|sign in|authentication/i.test(text)) {
    return context === 'checkout'
      ? 'يرجى تسجيل الدخول أولاً لإكمال الطلب.'
      : 'يرجى تسجيل الدخول أولاً لاستخدام الكوبون.'
  }

  if (/coupon already used on this device|used on this device|same device/i.test(text)) {
    return 'هذا الكوبون مستخدم مسبقاً على هذا الجهاز.'
  }

  if (/coupon already used by this account|used by this account|this account/i.test(text)) {
    return 'هذا الكوبون مستخدم مسبقاً على هذا الحساب.'
  }

  if (/coupon not found|coupon is invalid|not found/.test(text)) {
    return 'الكوبون غير موجود أو غير صالح.'
  }

  if (/coupon is not active yet|not active yet/.test(text)) {
    return 'هذا الكوبون غير مفعّل حالياً.'
  }

  if (/coupon expired|expired/.test(text)) {
    return 'انتهت صلاحية هذا الكوبون.'
  }

  if (/coupon usage limit reached|usage limit reached|max uses/.test(text)) {
    return 'تم الوصول إلى الحد الأقصى لاستخدام هذا الكوبون.'
  }

  if (/minimum order not reached/.test(text)) {
    const min = Number(data?.minimumOrderIqd || 0)
    return min > 0
      ? `الحد الأدنى لتفعيل هذا الكوبون هو ${fmtMoney(min)}.`
      : 'قيمة الطلب أقل من الحد الأدنى المطلوب لتفعيل الكوبون.'
  }

  if (/coupon cannot be applied to one or more products|cannot be applied/.test(text)) {
    return 'لا يمكن تطبيق هذا الكوبون على منتج أو أكثر داخل السلة.'
  }

  if (/insufficient stock/.test(text)) {
    return 'بعض المنتجات لم تعد متوفرة بالكمية المطلوبة.'
  }

  if (context === 'coupon' && status >= 500) {
    return 'تعذر التحقق من الكوبون حالياً. حاول مرة أخرى بعد قليل.'
  }

  if (context === 'checkout' && status >= 500) {
    return 'تعذر إكمال الطلب حالياً. حاول مرة أخرى بعد قليل.'
  }

  return normalizeApiMessage(raw, context === 'coupon' ? 'تعذر التحقق من الكوبون حالياً.' : 'تعذر إكمال الطلب حالياً.')
}

async function applyCoupon() {
  couponError.value = ''

  if (!cart.items.length) {
    couponError.value = 'السلة فارغة. أضف منتجاً أولاً ثم جرّب الكوبون.'
    return
  }

  couponLoading.value = true
  try {
    const res: any = await api.get('/Coupons/validate', {
      code: couponCode.value.trim(),
      subtotalIqd: Number(cart.total || 0),
      deviceKey: process.client ? getDeviceKey() : '',
      productIds: cart.items.map((x:any) => x.id).join(',')
    })
    appliedCoupon.value = res
    couponError.value = ''
  } catch (e: any) {
    appliedCoupon.value = null
    couponError.value = mapErrorMessage(e, 'coupon')
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

  if (!auth.token) {
    error.value = 'يرجى تسجيل الدخول أولاً لإكمال الطلب.'
    return
  }

  try {
    await openWhatsappForCart()
    cart.clear()
    removeCoupon()
  } catch (e: any) {
    error.value = mapErrorMessage(e, 'checkout')
  }
}

watch(() => cart.total, async (v) => {
  if (!appliedCoupon.value?.code) return
  if (!v) {
    appliedCoupon.value = null
    return
  }
  try {
    const res: any = await api.get('/Coupons/validate', {
      code: appliedCoupon.value.code,
      subtotalIqd: Number(v || 0),
      deviceKey: process.client ? getDeviceKey() : '',
      productIds: cart.items.map((x:any) => x.id).join(',')
    })
    appliedCoupon.value = res
  } catch {
    appliedCoupon.value = null
    couponError.value = 'تمت إزالة الكوبون لأن شروطه لم تعد متوافقة مع محتوى السلة.'
  }
})

onMounted(() => profile.hydrateFromAuth(auth.token || ''))
</script>
