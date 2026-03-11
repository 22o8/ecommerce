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
          <div class="flex items-center gap-2">
            <input
              type="checkbox"
              class="h-4 w-4 accent-[rgb(var(--primary))]"
              :checked="allSelected"
              @change="toggleAll"
            />
            <span class="hidden md:inline">ID</span>
          </div>
          <div class="rtl-text">{{ t('admin.status') }}</div>
          <div class="rtl-text">المنتج</div>
          <div class="rtl-text hidden md:block">{{ t('admin.user') }}</div>
          <div class="rtl-text">التاريخ</div>
          <div class="text-right rtl-text">{{ t('common.actions') }}</div>
        </div>

        <div v-if="selectedIds.length" class="px-4 py-3 flex items-center justify-between border-b border-[rgba(var(--border),0.9)]">
          <div class="text-sm admin-muted rtl-text">
            {{ (t('common.selected') || 'المحدد') }}: <span class="font-semibold">{{ selectedIds.length }}</span>
          </div>
          <button class="admin-danger" type="button" @click="removeSelected" :disabled="loading">
            {{ t('common.delete') }}
          </button>
        </div>

        <div v-for="o in orders" :key="o.id" class="admin-tr">
          <div class="flex items-start gap-2">
            <input
              type="checkbox"
              class="mt-1 h-4 w-4 accent-[rgb(var(--primary))]"
              :checked="selectedIds.includes(o.id)"
              @change="toggleOne(o.id)"
            />
            <div class="font-mono text-xs break-all hidden md:block">{{ o.id }}</div>
          </div>

          <div>
            <span :class="statusClass(o.status)">{{ o.status }}</span>
          </div>

          <div class="truncate rtl-text">{{ o.primaryItemTitle || '-' }}</div>

          <div class="truncate hidden md:block">{{ o.userName || o.userEmail || '-' }}</div>

          <div class="keep-ltr text-xs text-muted">{{ formatDate(o.createdAt) }}</div>

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

import { ref, computed } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'
import { formatIqd } from '~/composables/useMoney'

type OrderRow = {
  id: string
  status: string
  primaryItemTitle?: string
  userName?: string
  userEmail?: string
  createdAt?: string
  totalIqd?: number
}

const { t } = useI18n()
const api = useApi()

const loading = ref(false)
const error = ref('')
const orders = ref<OrderRow[]>([])

// تحديد متعدد (تحديد الكل + حذف جماعي)
const selectedIds = ref<string[]>([])

const allSelected = computed(() => {
  const total = orders.value.length
  return total > 0 && selectedIds.value.length === total
})

function toggleAll() {
  selectedIds.value = allSelected.value ? [] : orders.value.map(o => o.id)
}

function toggleOne(id: string) {
  const set = new Set(selectedIds.value)
  if (set.has(id)) set.delete(id)
  else set.add(id)
  selectedIds.value = Array.from(set)
}

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

async function removeSelected() {
  if (!selectedIds.value.length) return
  const ok = confirm((t('admin.confirmDeleteOrder') || t('common.confirmDelete') || 'Delete?') + ` (${selectedIds.value.length})`)
  if (!ok) return

  loading.value = true
  error.value = ''
  try {
    for (const id of selectedIds.value) {
      await api.del(`/admin/orders/${id}`)
    }
    orders.value = orders.value.filter(o => !selectedIds.value.includes(o.id))
    selectedIds.value = []
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

function extractErr(e: any) {
  return e?.data?.message || e?.message || t('common.requestFailed')
}

function formatDate(iso?: string){
  if (!iso) return '—'
  try { return new Date(iso).toLocaleString('en-US') } catch { return iso }
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
      primaryItemTitle: x.primaryItemTitle ? String(x.primaryItemTitle) : '',
      userName: x.userFullName ? String(x.userFullName) : (x.user?.fullName ? String(x.user.fullName) : ''),
      userEmail: x.userEmail ? String(x.userEmail) : (x.user?.email ? String(x.user.email) : ''),
      createdAt: x.createdAt ? String(x.createdAt) : '',
      totalIqd: Number(x.totalIqd ?? 0),
    }))
    selectedIds.value = []
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
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface));
  padding: 16px;
}
.admin-muted{ color: rgb(var(--muted)); }

.admin-ghost{
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  color: rgb(var(--fg));
  font-weight: 800;
}

.admin-pill{
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  color: rgb(var(--fg));
  font-weight: 800;
}

.admin-danger{
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.12);
  color: rgb(var(--fg));
  font-weight: 800;
}

.admin-danger:disabled{
  opacity: .55;
  cursor: not-allowed;
}

.admin-table{ display: grid; }
.admin-tr{
  display: grid;
  /* موبايل: اخفِ أعمدة غير مهمة */
  grid-template-columns: 1.2fr 1fr 1.6fr 1fr 1fr;
  gap: 12px;
  padding: 12px 16px;
  border-top: 1px solid rgb(var(--border));
  align-items: center;
}

@media (min-width: 768px){
  .admin-tr{
    grid-template-columns: 2fr 1fr 1.2fr 1.2fr 1fr 1fr;
  }
}
.admin-th{
  border-top: none;
  background: rgb(var(--surface-2));
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: .08em;
  color: rgb(var(--muted));
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
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  color: rgb(var(--muted));
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
