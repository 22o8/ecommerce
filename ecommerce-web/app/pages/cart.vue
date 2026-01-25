<script setup lang="ts">
import { useCartStore } from '~/app/stores/cart'
import { useAuthStore } from '~/app/stores/auth'
import { useProfileStore } from '~/app/stores/profile'
import UiButton from '~/app/components/ui/UiButton.vue'
import UiCard from '~/app/components/ui/UiCard.vue'
import UiInput from '~/app/components/ui/UiInput.vue'
import { useI18n } from '~/app/composables/useI18n'

const { t } = useI18n()
const cart = useCartStore()
const auth = useAuthStore()
const profile = useProfileStore()
const config = useRuntimeConfig()

const whatsappNumber = computed(() => (config.public as any).whatsappNumber || '')

const localPhone = ref(profile.phone)
watch(localPhone, (v) => profile.setPhone(v))

const messageText = computed(() => {
  const lines: string[] = []
  lines.push('طلب جديد من المتجر')
  lines.push('-------------------------')
  if (auth.user?.fullName) lines.push(`الاسم: ${auth.user.fullName}`)
  if (auth.user?.email) lines.push(`الايميل: ${auth.user.email}`)
  if (profile.phone) lines.push(`رقم الهاتف: ${profile.phone}`)
  lines.push('')
  lines.push('المنتجات:')
  for (const it of cart.items) {
    lines.push(`- ${it.title} × ${it.qty} (USD ${it.priceUsd})`)
  }
  lines.push('')
  lines.push(`المجموع: USD ${cart.totalUsd.toFixed(2)}`)
  return lines.join('\n')
})

const whatsappLink = computed(() => {
  const number = String(whatsappNumber.value).replace(/[^0-9]/g, '')
  const txt = encodeURIComponent(messageText.value)
  if (!number) return `https://wa.me/?text=${txt}`
  return `https://wa.me/${number}?text=${txt}`
})
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-10">
    <div class="flex items-center justify-between gap-4">
      <h1 class="text-2xl font-semibold rtl-text">{{ t('cart.title') }}</h1>
      <NuxtLink to="/products" class="text-sm opacity-80 hover:opacity-100 rtl-text">
        {{ t('cart.continue') }}
      </NuxtLink>
    </div>

    <div v-if="cart.items.length === 0" class="mt-8">
      <UiCard class="p-6">
        <p class="rtl-text opacity-80">{{ t('cart.empty') }}</p>
      </UiCard>
    </div>

    <div v-else class="mt-8 grid gap-6 lg:grid-cols-3">
      <div class="lg:col-span-2 space-y-4">
        <UiCard v-for="it in cart.items" :key="it.id" class="p-4">
          <div class="flex gap-4">
            <div class="h-20 w-20 overflow-hidden rounded-xl bg-white/5">
              <img v-if="it.image" :src="it.image" :alt="it.title" class="h-full w-full object-cover" />
            </div>

            <div class="flex-1">
              <div class="flex items-start justify-between gap-3">
                <div>
                  <NuxtLink v-if="it.slug" :to="`/products/${it.slug}`" class="font-semibold rtl-text hover:underline">
                    {{ it.title }}
                  </NuxtLink>
                  <p v-else class="font-semibold rtl-text">{{ it.title }}</p>
                  <p class="mt-1 text-sm opacity-80">USD {{ it.priceUsd }}</p>
                </div>

                <button class="text-sm opacity-70 hover:opacity-100" @click="cart.remove(it.id)">
                  {{ t('remove') }}
                </button>
              </div>

              <div class="mt-3 flex items-center justify-between gap-3">
                <div class="flex items-center gap-2">
                  <button class="h-9 w-9 rounded-xl bg-white/5 hover:bg-white/10" @click="cart.setQty(it.id, it.qty - 1)">
                    -
                  </button>
                  <div class="min-w-12 text-center">{{ it.qty }}</div>
                  <button class="h-9 w-9 rounded-xl bg-white/5 hover:bg-white/10" @click="cart.setQty(it.id, it.qty + 1)">
                    +
                  </button>
                </div>

                <div class="text-sm opacity-80">USD {{ (it.priceUsd * it.qty).toFixed(2) }}</div>
              </div>
            </div>
          </div>
        </UiCard>
      </div>

      <div>
        <UiCard class="p-5">
          <h2 class="text-lg font-semibold rtl-text">{{ t('cart.summary') }}</h2>

          <div class="mt-4 flex items-center justify-between">
            <span class="opacity-80 rtl-text">{{ t('cart.total') }}</span>
            <span class="font-semibold">USD {{ cart.totalUsd.toFixed(2) }}</span>
          </div>

          <div class="mt-5 space-y-2">
            <label class="text-sm opacity-80 rtl-text">{{ t('phone') }}</label>
            <UiInput v-model="localPhone" placeholder="07xxxxxxxxx" />
            <p class="text-xs opacity-60 rtl-text">{{ t('register.warning') }}</p>
          </div>

          <div class="mt-6 space-y-3">
            <a :href="whatsappLink" target="_blank" rel="noopener" class="block">
              <UiButton class="w-full">{{ t('cart.whatsapp') }}</UiButton>
            </a>
            <UiButton class="w-full" variant="ghost" @click="cart.clear()">
              {{ t('cart.clear') }}
            </UiButton>
          </div>
        </UiCard>
      </div>
    </div>
  </div>
</template>
