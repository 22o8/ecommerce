<template>
  <div class="grid gap-6 lg:grid-cols-[1fr_380px]">
    <div class="card-soft p-6 md:p-8">
      <div class="flex items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black rtl-text">{{ t('cartTitle') }}</h1>
          <p class="text-sm text-muted rtl-text">{{ t('cartSubtitle') }}</p>
        </div>
        <UiButton v-if="cart.count" variant="ghost" @click="cart.clear()">
          <Icon name="mdi:trash-can-outline" class="text-lg" />
          <span class="rtl-text">{{ t('clearCart') }}</span>
        </UiButton>
      </div>

      <div v-if="!cart.items.length" class="mt-6 rounded-3xl border border-app bg-surface p-6 text-center">
        <Icon name="mdi:cart-remove" class="text-4xl opacity-70" />
        <div class="mt-2 font-bold rtl-text">{{ t('cartEmpty') }}</div>
        <NuxtLink to="/products" class="mt-4 inline-flex">
          <UiButton>
            <Icon name="mdi:shopping-outline" class="text-lg" />
            <span class="rtl-text">{{ t('browseProducts') }}</span>
          </UiButton>
        </NuxtLink>
      </div>

      <div v-else class="mt-6 grid gap-3">
        <div v-for="it in cart.items" :key="it.id" class="rounded-3xl border border-app bg-surface p-4">
          <div class="flex gap-4">
            <div class="h-16 w-16 rounded-2xl overflow-hidden bg-black/20 shrink-0">
              <img v-if="it.imageUrl" :src="buildAssetUrl(it.imageUrl)" :alt="it.title" class="h-full w-full object-cover" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between gap-3">
                <div class="min-w-0">
                  <div class="font-bold rtl-text truncate">{{ it.title }}</div>
                  <div class="text-sm text-muted rtl-text">{{ fmtMoney(it.price) }} × {{ it.quantity }}</div>
                </div>
                <button
                  class="icon-btn bg-surface-2 border border-app text-muted hover:text-[rgb(var(--danger))] hover:border-[rgba(var(--danger),0.35)] hover:bg-[rgba(var(--danger),0.12)]"
                  @click="cart.remove(it.id)"
                  aria-label="remove"
                >
                  <Icon name="mdi:close" class="text-xl" />
                </button>
              </div>

              <div class="mt-3 flex items-center gap-2">
                <UiButton size="sm" variant="ghost" @click="cart.decrease(it.id)">
                  <Icon name="mdi:minus" class="text-lg" />
                </UiButton>
                <div class="min-w-[40px] text-center font-bold">{{ it.quantity }}</div>
                <UiButton size="sm" variant="ghost" @click="cart.increase(it.id)">
                  <Icon name="mdi:plus" class="text-lg" />
                </UiButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="card-soft p-6 md:p-8 h-fit">
      <h2 class="text-xl font-extrabold rtl-text">{{ t('checkout') }}</h2>

      <div class="mt-4 grid gap-2 text-sm">
        <div class="flex items-center justify-between">
          <span class="rtl-text">{{ t('itemsCount') }}</span>
          <span class="font-bold">{{ cart.count }}</span>
        </div>
        <div class="flex items-center justify-between">
          <span class="rtl-text">{{ t('total') }}</span>
          <span class="font-black">{{ fmtMoney(cart.total) }}</span>
        </div>
      </div>

      <div class="mt-6 grid gap-3">
        <!-- ✅ Buy Now = WhatsApp checkout -->
        <UiButton :disabled="!cart.items.length" @click="openWhatsApp">
          <Icon name="mdi:whatsapp" class="text-lg" />
          <span class="rtl-text">{{ t('buyNow') }}</span>
        </UiButton>

        <p v-if="error" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ error }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import { buildAssetUrl } from '~/composables/useApi'
import { formatIqd } from '~/composables/useMoney'

const { t } = useI18n()
const cart = useCartStore()
const auth = useAuthStore()
const profile = useProfileStore()
const config = useRuntimeConfig()

const error = ref('')

function fmtMoney(v: any){ return formatIqd(v) }

function whatsappText() {
  const when = new Date().toLocaleString('ar-IQ')
  const lines = [
    `طلب جديد من المتجر`,
    `الوقت: ${when}`,
    `الاسم: ${profile.fullName || auth.user?.fullName || '-'}`,
    `الهاتف: ${profile.phone || (auth.user as any)?.phone || '-'}`,
    `الإيميل: ${profile.email || auth.user?.email || '-'}`,
    `صفحة المتجر: ${window.location.origin}`,
    '',
    'المنتجات:',
    ...cart.items.map(i => `- ${i.title} × ${i.quantity} = ${fmtMoney(i.price * i.quantity)}`),
    '',
    `الإجمالي: ${fmtMoney(cart.total)}`,
  ]
  return lines.join('\n')
}

function openWhatsApp() {
  error.value = ''
  const number = (config.public as any).whatsappNumber || (config.public as any).whatsappPhone || ''
  const text = encodeURIComponent(whatsappText())
  const url = number
    ? `https://wa.me/${String(number).replace(/\D/g, '')}?text=${text}`
    : `https://wa.me/?text=${text}`
  window.open(url, '_blank')
}

onMounted(() => profile.hydrateFromAuth(auth.token || ''))
</script>
