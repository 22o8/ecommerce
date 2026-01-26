<template>
  <div class="space-y-4">
    <div class="admin-box">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="text-xl font-extrabold rtl-text">{{ t('admin.orders') }}</div>
          <div class="text-sm admin-muted rtl-text">{{ t('admin.ordersHint') }}</div>
        </div>

        <div class="flex gap-2">
          <button class="admin-ghost" type="button" @click="fetchOrders()" :disabled="loading">
            {{ t('common.refresh') }}
          </button>
        </div>
      </div>
    </div>

    <div class="admin-box overflow-hidden">
      <div v-if="loading" class="p-4 admin-muted rtl-text">{{ t('common.loading') }}</div>

      <div v-else-if="orders.length === 0" class="p-6 text-center">
        <div class="text-lg font-extrabold rtl-text">{{ t('admin.noOrders') }}</div>
        <div class="admin-muted mt-1 rtl-text">{{ t('admin.noOrdersHint') }}</div>
      </div>

      <div v-else class="admin-table">
        <div class="admin-tr admin-th">
          <div>ID</div>
          <div class="rtl-text">{{ t('admin.status') }}</div>
          <div class="rtl-text">{{ t('admin.user') }}</div>
          <div class="text-right rtl-text">{{ t('common.actions') }}</div>
        </div>

        <div v-for="o in orders" :key="o.id" class="admin-tr">
          <div class="font-mono text-xs break-all">{{ o.id }}</div>

          <div>
            <span :class="statusClass(o.status)">{{ o.status }}</span>
          </div>

          <div class="truncate">{{ o.userEmail || '-' }}</div>

          <div class="flex justify-end gap-2">
            <NuxtLink class="admin-pill" :to="`/admin/orders/${o.id}`">{{ t('common.details') }}</NuxtLink>
            <button class="admin-danger" type="button" @click="removeOrder(o.id)" :disabled="loading">
              {{ t('common.delete') }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="error" class="admin-error rtl-text">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'

type OrderRow = {
  id: string
  status: string
  userEmail?: string
}

const { t } = useI18n()
const api = useApi()

const loading = ref(false)
const error = ref('')
const orders = ref<OrderRow[]>([])

async function removeOrder(id: string) {
  const ok = confirm(t('admin.confirmDeleteOrder'))
  if (!ok) return

  loading.value = true
  error.value = ''
  try {
    await api.del(`/admin/orders/${id}`)
    orders.value = orders.value.filter(o => o.id !== id)
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

function extractErr(e: any) {
  return e?.data?.message || e?.message || t('common.requestFailed')
}

function statusClass(status: string) {
  const s = (status || '').toLowerCase()
  if (s.includes('paid') || s.includes('completed') || s.includes('success')) return 'badge-on'
  if (s.includes('cancel')) return 'badge-bad'
  return 'badge-off'
}

async function fetchOrders() {
  loading.value = true
  error.value = ''
  try {
    const res = await api.get<any[]>('/admin/orders')
    const list = Array.isArray(res) ? res : []
    orders.value = list.map(x => ({
      id: String(x.id),
      status: String(x.status || 'Unknown'),
      userEmail: x.userEmail ? String(x.userEmail) : (x.user?.email ? String(x.user.email) : ''),
    }))
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

fetchOrders()
</script>

<style scoped>
.admin-box{
  border-radius: 20px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  padding: 16px;
}
.admin-muted{ color: rgba(255,255,255,.65); }

.admin-ghost{
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.85);
  font-weight: 800;
}

.admin-pill{
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.9);
  font-weight: 800;
}

.admin-danger{
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.12);
  color: rgba(255,255,255,.95);
  font-weight: 800;
}

.admin-danger:disabled{
  opacity: .55;
  cursor: not-allowed;
}

.admin-table{ display: grid; }
.admin-tr{
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  gap: 12px;
  padding: 12px 16px;
  border-top: 1px solid rgba(255,255,255,.08);
  align-items: center;
}
.admin-th{
  border-top: none;
  background: rgba(0,0,0,.18);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: .08em;
  color: rgba(255,255,255,.65);
}

.badge-on{
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(16,185,129,.35);
  background: rgba(16,185,129,.14);
  font-weight: 800;
  display: inline-flex;
}
.badge-off{
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(255,255,255,.12);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.75);
  font-weight: 800;
  display: inline-flex;
}
.badge-bad{
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.14);
  font-weight: 800;
  display: inline-flex;
}

.admin-error{
  border-radius: 16px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.10);
  padding: 12px 14px;
}
</style>
