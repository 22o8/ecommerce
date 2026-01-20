<!-- app/pages/orders/[id].vue -->
<template>
  <main class="mx-auto max-w-3xl space-y-6">
    <div class="rounded-3xl border border-white/10 bg-white/5 p-6">
      <h1 class="text-2xl font-black">{{ t('orderPage') }}</h1>
      <p class="mt-2 text-sm text-white/70">ID: {{ orderId }}</p>
    </div>

    <div v-if="pending" class="rounded-3xl border border-white/10 bg-white/5 p-6">
      {{ t('loading') }}
    </div>

    <div v-else-if="errorMsg" class="rounded-3xl border border-red-500/30 bg-red-500/10 p-6 text-red-200">
      {{ errorMsg }}
    </div>
    <template v-else>
      <div class="rounded-3xl border border-white/10 bg-white/5 p-6">
        <h3 class="text-lg font-black">تفاصيل الطلب</h3>
        <pre class="mt-3 whitespace-pre-wrap text-xs text-white/70">{{ order }}</pre>
      </div>

      <div class="rounded-3xl border border-white/10 bg-white/5 p-6">
        <h3 class="text-lg font-black">{{ t('download') }}</h3>

        <div v-if="downloadUrl" class="mt-4">
          <a
            :href="downloadUrl"
            target="_blank"
            rel="noopener"
            class="inline-flex items-center gap-2 rounded-2xl border border-white/10 bg-black/20 px-4 py-3 text-sm font-extrabold hover:bg-black/30"
          >
            ⬇️ {{ t('download') }}
          </a>
        </div>
        <div v-else class="mt-3 text-sm text-white/70">
          {{ t('noDownloadToken') }}
        </div>
      </div>
    </template>
  </main>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, navigateTo } from '#app'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'
import { useAuthStore } from '~/stores/auth'
import { useSiteMeta } from '~/composables/useSiteMeta'

const { t } = useI18n()
const route = useRoute()
const api = useApi()
const auth = useAuthStore()

const orderId = String(route.params.id ?? '')

useSiteMeta({
  title: `Order ${orderId} | Ecommerce`,
  description: 'Order details and download.',
  path: `/orders/${orderId}`,
})

const errorMsg = ref<string | null>(null)
function extractErrorMessage(e: any) {
  return e?.data?.message || e?.data || e?.message || 'صار خطأ بجلب بيانات الطلب'
}

// حماية (زيادة على middleware)
if (!auth.isAuthed) {
  await navigateTo('/login')
}

const { data, pending } = await useAsyncData(`order_${orderId}`, async () => {
  // عدل endpoints حسب باك اندك الحقيقي
  const order = await api.get<any>(`/Orders/${orderId}`)
const purchase = await api.get<any>(`/Purchases/product/${orderId}`).catch(() => null)
  return { order, purchase }
})

const order = computed(() => data.value?.order || null)
const purchase = computed(() => data.value?.purchase || null)

watchEffect(() => {
  if (!pending.value && !data.value) errorMsg.value = t('requestFailed')
})

const downloadUrl = computed(() => {
  const token = purchase.value?.download?.token || purchase.value?.downloadToken
  if (!token) return null
  return `/api/bff/Download/${token}`
})
</script>
