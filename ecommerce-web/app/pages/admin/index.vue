<template>
  <div class="space-y-6">
    <div class="admin-box">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="text-xl font-extrabold rtl-text">{{ t('admin.dashboard') }}</div>
          <div class="text-sm admin-muted rtl-text">{{ t('admin.dashboardHint') }}</div>
        </div>

        <div class="flex gap-2">
          <button class="admin-ghost" type="button" @click="fetchStats" :disabled="loading">
            {{ t('common.refresh') }}
          </button>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="admin-card">
        <div class="admin-muted text-sm rtl-text">{{ t('admin.totalOrders') }}</div>
        <div class="text-3xl font-extrabold mt-1">{{ stats.totalOrders }}</div>
      </div>

      <div class="admin-card">
        <div class="admin-muted text-sm rtl-text">{{ t('admin.totalUsers') }}</div>
        <div class="text-3xl font-extrabold mt-1">{{ stats.totalUsers }}</div>
      </div>

      <div class="admin-card">
        <div class="admin-muted text-sm rtl-text">{{ t('admin.totalRevenue') }}</div>
        <div class="text-3xl font-extrabold mt-1">{{ formatMoney(stats.totalRevenueUsd) }}</div>
        <div class="admin-muted text-xs mt-1 rtl-text">{{ t('admin.revenueHint') }}</div>
      </div>
    </div>

    <div class="admin-box">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
        <NuxtLink class="admin-action" to="/admin/products">
          <div class="font-extrabold rtl-text">{{ t('admin.manageProducts') }}</div>
          <div class="admin-muted text-sm rtl-text">{{ t('admin.manageProductsHint') }}</div>
        </NuxtLink>

        <NuxtLink class="admin-action" to="/admin/orders">
          <div class="font-extrabold rtl-text">{{ t('admin.manageOrders') }}</div>
          <div class="admin-muted text-sm rtl-text">{{ t('admin.manageOrdersHint') }}</div>
        </NuxtLink>

        <NuxtLink class="admin-action" to="/admin/services">
          <div class="font-extrabold rtl-text">{{ t('admin.manageServices') }}</div>
          <div class="admin-muted text-sm rtl-text">{{ t('admin.manageServicesHint') }}</div>
        </NuxtLink>
      </div>
    </div>

    <div v-if="error" class="admin-error rtl-text">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { ref } from 'vue'
import { useI18n } from '~/composables/useI18n'
import { useAdminApi } from '~/composables/useAdminApi'

const { t } = useI18n()
const adminApi = useAdminApi()

const loading = ref(false)
const error = ref('')

const stats = ref({
  totalOrders: 0,
  totalUsers: 0,
  totalRevenueUsd: 0,
})

function extractErr(e: any) {
  return e?.data?.message || e?.message || t('common.requestFailed')
}

function formatMoney(v: number) {
  const n = Number(v || 0)
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}

async function fetchStats() {
  loading.value = true
  error.value = ''
  try {
    const res = await adminApi.getDashboardStats<any>()
    stats.value.totalOrders = Number(res?.totalOrders ?? 0)
    stats.value.totalUsers = Number(res?.totalUsers ?? 0)
    stats.value.totalRevenueUsd = Number(res?.totalRevenueUsd ?? 0)
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

fetchStats()
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

.admin-card{
  border-radius: 20px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  padding: 16px;
}

.admin-action{
  border-radius: 18px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  padding: 14px;
  display: block;
}
.admin-action:hover{
  background: rgba(255,255,255,.10);
}

.admin-error{
  border-radius: 16px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.10);
  padding: 12px 14px;
}
</style>
