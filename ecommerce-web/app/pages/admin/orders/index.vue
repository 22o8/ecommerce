<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'

const { t } = useI18n()
const api = useApi()

type Order = {
  id: string
  status: string
  totalUsd: number
  createdAt: string
  user: { userId: string; fullName: string; email: string; phone: string }
  items: any[]
}

const loading = ref(false)
const error = ref<string | null>(null)
const orders = ref<Order[]>([])

async function load() {
  loading.value = true
  error.value = null
  try {
    orders.value = await api.get('/api/bff/admin/orders')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed'
  } finally {
    loading.value = false
  }
}

async function remove(id: string) {
  if (!confirm(t('admin.confirmDelete'))) return
  loading.value = true
  error.value = null
  try {
    await api.del(`/api/bff/admin/orders/${id}`)
    await load()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed'
  } finally {
    loading.value = false
  }
}

async function removeAll() {
  if (!confirm(t('admin.confirmDelete'))) return
  loading.value = true
  error.value = null
  try {
    await api.del('/api/bff/admin/orders/all')
    await load()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed'
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3">
      <h1 class="text-xl font-bold">{{ t('admin.orders') }}</h1>

      <div class="flex items-center gap-2">
        <button class="btn-secondary" :disabled="loading" @click="load">{{ t('admin.refresh') }}</button>
        <button class="btn-danger" :disabled="loading" @click="removeAll">{{ t('admin.deleteAllOrders') }}</button>
      </div>
    </div>

    <div v-if="error" class="rounded-xl border border-red-500/30 bg-red-500/10 p-3 text-sm text-red-200">
      {{ error }}
    </div>

    <div class="grid gap-3">
      <div
        v-for="o in orders"
        :key="o.id"
        class="rounded-2xl border border-white/10 bg-white/5 p-4 space-y-2"
      >
        <div class="flex items-center justify-between gap-3">
          <div class="font-semibold truncate">
            {{ o.user?.fullName || '—' }} • {{ o.totalUsd }} US$
          </div>
          <button class="btn-danger" :disabled="loading" @click="remove(o.id)">{{ t('admin.delete') }}</button>
        </div>

        <div class="text-xs opacity-70 flex flex-wrap gap-3">
          <span>id: {{ o.id }}</span>
          <span>status: {{ o.status }}</span>
          <span>{{ new Date(o.createdAt).toLocaleString() }}</span>
          <span>{{ o.user?.email }}</span>
          <span>{{ o.user?.phone }}</span>
        </div>

        <div class="text-sm opacity-90">
          <div class="font-semibold mb-1">العناصر</div>
          <ul class="list-disc ps-6 space-y-1">
            <li v-for="(it, idx) in o.items" :key="idx">
              {{ it.itemType }} • qty: {{ it.quantity }} • {{ it.unitPriceUsd }} US$
            </li>
          </ul>
        </div>
      </div>

      <div v-if="!loading && orders.length === 0" class="opacity-70 text-sm">
        لا توجد طلبات.
      </div>
    </div>
  </div>
</template>
