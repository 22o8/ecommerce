<template>
  <div class="space-y-4">
    <div class="admin-box flex items-center justify-between gap-3">
      <div>
        <div class="text-xl font-extrabold rtl-text">{{ t('admin.orderDetails') }}</div>
        <div class="text-sm admin-muted keep-ltr">{{ id }}</div>
      </div>
      <NuxtLink to="/admin/orders" class="admin-ghost rtl-text">{{ t('common.back') }}</NuxtLink>
    </div>

    <div v-if="loading" class="admin-box admin-muted rtl-text">{{ t('common.loading') }}</div>

    <div v-else-if="!order" class="admin-box admin-muted rtl-text">{{ t('admin.orderNotFound') }}</div>
    
    <div v-else class="admin-box space-y-4">
      <div class="grid gap-3 md:grid-cols-4">
        <div class="sub-box">
          <div class="label rtl-text">{{ t('admin.orderDetailsPage.status') }}</div>
          <div class="font-extrabold">{{ order.status || '—' }}</div>
        </div>
        <div class="sub-box">
          <div class="label rtl-text">{{ t('admin.orderDetailsPage.accountInfo') }}</div>
          <div class="font-extrabold">{{ accountDisplay.fullName }}</div>
          <div class="text-sm text-muted keep-ltr">{{ accountDisplay.email }}</div>
          <div class="text-sm text-muted keep-ltr">{{ accountDisplay.phone }}</div>
          <div class="mt-2 text-xs admin-muted rtl-text">{{ t('admin.orderDetailsPage.accountId') }}</div>
          <div class="text-sm keep-ltr">{{ accountDisplay.id }}</div>
        </div>
        <div class="sub-box">
          <div class="label rtl-text">{{ t('admin.orderDetailsPage.date') }}</div>
          <div class="font-extrabold keep-ltr">{{ formatDate(order.createdAt) }}</div>
        </div>
        <div class="sub-box">
          <div class="label rtl-text">{{ t('admin.orderDetailsPage.total') }}</div>
          <div class="font-extrabold keep-ltr">{{ formatIqd(order.totalIqd) }}</div>
        </div>
      </div>

      <div class="sub-box overflow-hidden">
        <div class="label rtl-text mb-2">{{ t('admin.orderDetailsPage.products') }}</div>

        <div class="grid gap-2">
          <div class="grid grid-cols-[1fr_80px_120px_120px] gap-2 text-xs admin-muted">
            <div class="rtl-text">{{ t('admin.orderDetailsPage.item') }}</div>
            <div class="text-center rtl-text">{{ t('admin.orderDetailsPage.quantity') }}</div>
            <div class="text-right rtl-text">{{ t('admin.orderDetailsPage.price') }}</div>
            <div class="text-right rtl-text">{{ t('admin.orderDetailsPage.lineTotal') }}</div>
          </div>

          <div v-for="(it, idx) in order.items || []" :key="idx" class="grid grid-cols-[1fr_80px_120px_120px] gap-2 items-center py-2 border-t border-app">
            <div class="min-w-0">
              <div class="font-bold rtl-text truncate">
                {{ it.productTitle || it.serviceTitle || it.packageTitle || it.itemType || '—' }}
              </div>
              <div class="text-xs text-muted keep-ltr">{{ it.productId || it.serviceId || it.packageId || '' }}</div>
            </div>
            <div class="text-center font-bold keep-ltr">{{ it.quantity }}</div>
            <div class="text-right font-bold keep-ltr">{{ formatIqd(it.unitPriceIqd) }}</div>
            <div class="text-right font-black keep-ltr">{{ formatIqd(it.lineTotalIqd) }}</div>
          </div>
        </div>
      </div>

      <div v-if="(order.payments || []).length" class="sub-box">
        <div class="label rtl-text">{{ t('admin.orderDetailsPage.payments') }}</div>
        <div class="grid gap-2 mt-2">
          <div v-for="p in order.payments" :key="p.id" class="flex items-center justify-between gap-3">
            <div class="min-w-0">
              <div class="font-bold keep-ltr">{{ p.provider }} · {{ p.status }}</div>
              <div class="text-xs text-muted keep-ltr">{{ formatDate(p.createdAt) }}</div>
            </div>
            <div class="font-black keep-ltr">{{ formatIqd(p.amountIqd) }}</div>
          </div>
        </div>
      </div>
    </div>


    <div v-if="error" class="admin-error">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { computed, ref, onMounted } from 'vue'
import { useRoute } from '#imports'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'
import { formatIqd } from '~/composables/useMoney'

const { t } = useI18n()
const api = useApi()
const route = useRoute()
const id = computed(() => String(route.params.id))

const loading = ref(true)
const error = ref('')
const order = ref<any | null>(null)

const account = ref<any | null>(null)
const accountLoading = ref(false)

const accountDisplay = computed(() => {
  const o: any = order.value
  const a: any = account.value || o?.customer || o?.user || o?.account || o?.userInfo || o?.createdBy || null
  const fullName = a?.fullName || a?.name || a?.displayName || a?.username || '—'
  const email = a?.email || '—'
  const phone = a?.phone || a?.phoneNumber || a?.mobile || a?.mobileNumber || '—'
  const id = a?.id || a?.userId || o?.userId || o?.customerId || '—'
  return { fullName, email, phone, id }
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    const res: any = await api.get(`/admin/orders/${id.value}`)
    order.value = res || null

    // try to resolve account info (some APIs only return userId)
    const o: any = order.value
    account.value = o?.customer || o?.user || o?.account || o?.userInfo || o?.createdBy || null

    const needsUser = !account.value && (o?.userId || o?.customerId)
    if (needsUser) {
      const uid = String(o.userId || o.customerId)
      try {
        accountLoading.value = true
        const u: any = await api.get(`/admin/users/${uid}`)
        account.value = u || null
      } catch {
        // ignore (order details should still load)
      } finally {
        accountLoading.value = false
      }
    }
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('failedLoad')
  } finally {
    loading.value = false
  }
}

function formatDate(iso?: string){
  if (!iso) return '—'
  try { return new Date(iso).toLocaleString('en-US') } catch { return String(iso) }
}

onMounted(load)
</script>

<style scoped>
.admin-box{ border-radius: 20px; border: 1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); padding: 16px; }
.sub-box{ border-radius: 18px; border: 1px solid rgba(255,255,255,.10); background: rgba(0,0,0,.14); padding: 14px; }
.label{ font-size: 12px; letter-spacing: .08em; text-transform: uppercase; color: rgba(255,255,255,.65); margin-bottom: 6px; }
.admin-muted{ color: rgba(255,255,255,.65); }
.admin-ghost{ padding:10px 12px; border-radius:14px; border:1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); color: rgba(255,255,255,.85); font-weight:700; }
.admin-error{ border-radius:16px; border:1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); padding:12px 14px; }
</style>
