<template>
  <div class="space-y-5 admin-insights-page">
    <div class="admin-box flex items-center justify-between gap-3">
      <div>
        <div class="text-xl font-extrabold rtl-text">{{ $t('admin.insightsTitle') }}</div>
        <div class="text-sm admin-muted rtl-text">{{ $t('admin.insightsSubtitle') }}</div>
      </div>
      <button class="admin-ghost" type="button" @click="loadAll" :disabled="loading">
        {{ t('common.refresh') }}
      </button>
    </div>

    <div v-if="loading" class="admin-box admin-muted rtl-text">{{ t('common.loading') }}</div>
    <div v-else class="grid gap-4 lg:grid-cols-2">
      <div class="admin-box insights-card">
        <div class="font-extrabold rtl-text mb-3">{{ $t('admin.topPurchased') }}</div>
        <div v-if="topPurchased.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in topPurchased" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black">{{ x.purchases }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box insights-card">
        <div class="font-extrabold rtl-text mb-3">❤️ {{ $t('admin.topFavorited') }}</div>
        <div v-if="topFavorites.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in topFavorites" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black">{{ x.favorites }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box insights-card">
        <div class="font-extrabold rtl-text mb-3">{{ $t('admin.topViewed') }}</div>
        <div v-if="topViews.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in topViews" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black">{{ x.views }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box insights-card">
        <div class="font-extrabold rtl-text mb-3">💤 {{ $t('admin.neglectedProducts') }}</div>
        <div v-if="neglected.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in neglected" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr text-xs text-muted">views: {{ x.views }} · fav: {{ x.favorites }} · buy: {{ x.purchases }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="admin-box insights-activity-box">
      <div class="font-extrabold rtl-text mb-3">{{ $t('admin.activityTitle') }}</div>

      <div class="grid gap-4 md:grid-cols-2">
        <div class="sub-box">
          <div class="label rtl-text mb-2">{{ $t('admin.dailyTableTitle') }}</div>
          <div class="grid gap-2">
            <div class="grid grid-cols-[120px_1fr_1fr_1fr_1fr] gap-2 text-xs admin-muted">
              <div class="keep-ltr">{{ $t('admin.colDate') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colOrders') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colViews') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colFavorites') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colVisits') }}</div>
            </div>
            <div v-for="r in daily" :key="r.period" class="grid grid-cols-[120px_1fr_1fr_1fr_1fr] gap-2 items-center py-2 border-t border-app">
              <div class="keep-ltr text-xs">{{ r.period }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.orders }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.views }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.favorites }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.visits }}</div>
            </div>
          </div>
        </div>

        <div class="sub-box">
          <div class="label rtl-text mb-2">{{ $t('admin.monthlyTableTitle') }}</div>
          <div class="grid gap-2">
            <div class="grid grid-cols-[120px_1fr_1fr_1fr_1fr] gap-2 text-xs admin-muted">
              <div class="keep-ltr">{{ $t('admin.colMonth') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colOrders') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colViews') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colFavorites') }}</div>
              <div class="text-center rtl-text">{{ $t('admin.colVisits') }}</div>
            </div>
            <div v-for="r in monthly" :key="r.period" class="grid grid-cols-[120px_1fr_1fr_1fr_1fr] gap-2 items-center py-2 border-t border-app">
              <div class="keep-ltr text-xs">{{ r.period }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.orders }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.views }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.favorites }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.visits }}</div>
            </div>
          </div>
        </div>
      </div>

      <div v-if="error" class="admin-error rtl-text mt-4">{{ error }}</div>
    </div>
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

const topPurchased = ref<any[]>([])
const topFavorites = ref<any[]>([])
const topViews = ref<any[]>([])
const neglected = ref<any[]>([])

const daily = ref<any[]>([])
const monthly = ref<any[]>([])

function extractErr(e: any) {
  return e?.data?.message || e?.message || t('common.requestFailed')
}

async function loadAll() {
  loading.value = true
  error.value = ''
  try {
    const ov: any = await adminApi.get('/admin/analytics/overview')
    topPurchased.value = ov?.topPurchased || []
    topFavorites.value = ov?.topFavorites || []
    topViews.value = ov?.topViews || []
    neglected.value = ov?.neglected || []

    const act: any = await adminApi.get('/admin/analytics/activity')
    daily.value = act?.daily || []
    monthly.value = act?.monthly || []
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

loadAll()
</script>

<style scoped>
.admin-insights-page :deep(.admin-box){
  border-radius: 28px;
  border: 1px solid rgba(var(--border), .95);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .94));
  padding: 18px;
  box-shadow: 0 22px 56px rgba(17,24,39,.08);
}
.admin-insights-page :deep(.sub-box){
  border-radius: 22px;
  background: rgba(var(--surface-rgb), .76);
}
.admin-insights-page :deep(.admin-ghost){
  min-height: 46px;
  border-radius: 16px;
}
.insights-card{ min-height: 240px; }
:global(html.theme-light) .admin-insights-page :deep(.admin-box){
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(251,245,249,.95));
  border-color: rgba(229,213,223,.95);
  box-shadow: 0 24px 56px rgba(17,24,39,.04), 0 12px 28px rgba(236,72,153,.06);
}
:global(html.theme-light) .admin-insights-page :deep(.sub-box){
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(249,242,247,.94));
  box-shadow: inset 0 1px 0 rgba(255,255,255,.75);
}
.admin-muted{ color: rgb(var(--muted)); }
.admin-error{
  border-radius: 18px; border:1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); padding: 12px 14px;
}
</style>
