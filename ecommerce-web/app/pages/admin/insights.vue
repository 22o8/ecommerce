<template>
  <div class="space-y-5 insights-admin-page">
    <div class="admin-box admin-box--hero flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
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
      <div class="admin-box">
        <div class="table-card__title rtl-text">{{ $t('admin.topPurchased') }}</div>
        <div v-if="topPurchased.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="table-card">
          <div class="table-card__head">
            <div>#</div><div class="rtl-text">{{ $t('admin.tableProduct') }}</div><div class="text-right rtl-text">{{ $t('admin.tableMetric') }}</div>
          </div>
          <div v-for="(x, idx) in topPurchased" :key="x.productId || x.id || idx" class="table-card__row">
            <div class="rank-badge">{{ idx + 1 }}</div>
            <div class="rtl-text font-bold truncate">{{ x.title || x.name || x.productTitle || '—' }}</div>
            <div class="keep-ltr font-black text-right">{{ x.purchases ?? x.count ?? x.quantity ?? x.total ?? 0 }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="table-card__title rtl-text">❤️ {{ $t('admin.topFavorited') || 'الأكثر مفضلة' }}</div>
        <div v-if="topFavorites.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="table-card">
          <div class="table-card__head">
            <div>#</div><div class="rtl-text">{{ $t('admin.tableProduct') }}</div><div class="text-right rtl-text">{{ $t('admin.tableMetric') }}</div>
          </div>
          <div v-for="(x, idx) in topFavorites" :key="x.productId" class="table-card__row">
            <div class="rank-badge">{{ idx + 1 }}</div>
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black text-right">{{ x.favorites }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="table-card__title rtl-text">{{ $t('admin.topViewed') }}</div>
        <div v-if="topViews.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="table-card">
          <div class="table-card__head">
            <div>#</div><div class="rtl-text">{{ $t('admin.tableProduct') }}</div><div class="text-right rtl-text">{{ $t('admin.tableMetric') }}</div>
          </div>
          <div v-for="(x, idx) in topViews" :key="x.productId" class="table-card__row">
            <div class="rank-badge">{{ idx + 1 }}</div>
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black text-right">{{ x.views }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="table-card__title rtl-text">📦 منخفض المخزون</div>
        <div v-if="lowStock.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="table-card">
          <div v-for="(x, idx) in lowStock" :key="x.productId || idx" class="table-card__row">
            <div class="rank-badge">{{ idx + 1 }}</div>
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black text-right">{{ x.stockQuantity }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="table-card__title rtl-text">🚫 نافد المخزون</div>
        <div v-if="outOfStock.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="table-card">
          <div v-for="(x, idx) in outOfStock" :key="x.productId || idx" class="table-card__row">
            <div class="rank-badge">{{ idx + 1 }}</div>
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black text-right">{{ x.stockQuantity }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="table-card__title rtl-text">💤 {{ $t('admin.neglectedProducts') || 'المنتجات المهملة' }}</div>
        <div v-if="neglected.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="table-card table-card--wide">
          <div class="table-card__head table-card__head--wide">
            <div>#</div><div class="rtl-text">{{ $t('admin.tableProduct') }}</div><div class="text-right rtl-text">{{ $t('admin.colViews') }}</div><div class="text-right rtl-text">{{ $t('admin.colFavorites') }}</div><div class="text-right rtl-text">{{ $t('admin.colOrders') }}</div>
          </div>
          <div v-for="(x, idx) in neglected" :key="x.productId" class="table-card__row table-card__row--wide">
            <div class="rank-badge">{{ idx + 1 }}</div>
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black text-right">{{ x.views }}</div>
            <div class="keep-ltr font-black text-right">{{ x.favorites }}</div>
            <div class="keep-ltr font-black text-right">{{ x.purchases }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="admin-box">
      <div class="font-extrabold rtl-text mb-3">{{ $t('admin.activityTitle') }}</div>

      <div class="grid gap-4 md:grid-cols-2">
        <div class="sub-box">
          <div class="label rtl-text mb-2">{{ $t('admin.dailyTableTitle') }}</div>
          <div class="table-scroll">
            <div class="grid gap-2 min-w-[520px]">
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
        </div>

        <div class="sub-box">
          <div class="label rtl-text mb-2">{{ $t('admin.monthlyTableTitle') }}</div>
          <div class="table-scroll">
            <div class="grid gap-2 min-w-[520px]">
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
const lowStock = ref<any[]>([])
const outOfStock = ref<any[]>([])

const daily = ref<any[]>([])
const monthly = ref<any[]>([])

function extractErr(e: any) {
  return e?.data?.message || e?.message || t('common.requestFailed')
}

function normalizeRows(list: any) {
  const rows = Array.isArray(list) ? list : []
  return rows
    .map((x: any, idx: number) => ({
      productId: x?.productId ?? x?.id ?? idx,
      title: String(x?.title ?? x?.name ?? x?.productTitle ?? x?.productName ?? '').trim(),
      purchases: Number(x?.purchases ?? x?.count ?? x?.quantity ?? x?.total ?? 0),
      favorites: Number(x?.favorites ?? x?.count ?? x?.total ?? 0),
      views: Number(x?.views ?? x?.count ?? x?.total ?? 0),
    }))
    .filter((x: any) => x.title || x.purchases > 0 || x.favorites > 0 || x.views > 0)
}

async function loadAll() {
  loading.value = true
  error.value = ''
  try {
    const ov: any = await adminApi.get('/admin/analytics/overview', { days: 3650 })
    topPurchased.value = normalizeRows(ov?.topPurchased ?? ov?.mostPurchased ?? ov?.topPurchasedProducts)
    topFavorites.value = normalizeRows(ov?.topFavorites ?? ov?.mostFavorited ?? ov?.topFavorited)
    topViews.value = normalizeRows(ov?.topViews ?? ov?.mostViewed ?? ov?.topViewedProducts)
    neglected.value = normalizeRows(ov?.neglected ?? ov?.neglectedProducts)
    lowStock.value = Array.isArray(ov?.lowStock) ? ov.lowStock : []
    outOfStock.value = Array.isArray(ov?.outOfStock) ? ov.outOfStock : []

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
.insights-admin-page{
  --insight-shadow: 0 24px 70px rgba(12, 16, 32, .16);
}
.admin-box{
  border-radius: 26px;
  border: 1px solid rgba(var(--border), .95);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .97), rgba(var(--surface-rgb), .89));
  padding: 18px;
  box-shadow: var(--insight-shadow);
}
.admin-box--hero{ position:relative; overflow:hidden; }
.admin-box--hero::after{ content:''; position:absolute; inset:auto auto -90px -30px; width:240px; height:240px; border-radius:999px; background:radial-gradient(circle, rgba(var(--primary), .16), transparent 68%); pointer-events:none; }
.sub-box{
  border-radius: 20px;
  border: 1px solid rgba(var(--border), .95);
  background: rgba(var(--surface-2-rgb), .9);
  padding: 14px;
}
.label{ font-size: 12px; letter-spacing: .08em; text-transform: uppercase; color: rgb(var(--muted)); }
.admin-muted{ color: rgb(var(--muted)); }
.admin-ghost{
  padding: 10px 14px;
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  color: rgb(var(--fg));
  font-weight: 800;
}
.admin-error{
  border-radius: 16px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.10);
  padding: 12px 14px;
}
.table-card{display:grid;gap:8px;}
.table-card__title{font-weight:900;margin-bottom:12px;}
.table-card__head,.table-card__row{display:grid;grid-template-columns:40px minmax(0,1fr) 76px;gap:10px;align-items:center;}
.table-card__head{padding-bottom:8px;border-bottom:1px dashed rgb(var(--border));font-size:11px;color:rgb(var(--muted));text-transform:uppercase;letter-spacing:.06em;}
.table-card__row{padding:10px 0;border-top:1px solid rgba(var(--border),.7);}
.table-card__row:first-of-type{border-top:none;}
.table-card--wide .table-card__head--wide,.table-card--wide .table-card__row--wide{grid-template-columns:40px minmax(0,1.5fr) 64px 64px 64px;}
.rank-badge{width:30px;height:30px;border-radius:999px;display:grid;place-items:center;background:rgba(124,58,237,.16);border:1px solid rgba(124,58,237,.34);font-weight:900;}
.table-scroll{overflow-x:auto;}
.table-scroll > .grid{min-width:520px;}
@media (max-width: 768px){
  .admin-box{ border-radius:22px; padding:14px; }
  .table-card__head,.table-card__row{grid-template-columns:32px minmax(0,1fr) 52px;gap:8px;}
  .table-card--wide .table-card__head--wide,.table-card--wide .table-card__row--wide{grid-template-columns:28px minmax(0,1fr) 48px 48px 48px;}
}
</style>
