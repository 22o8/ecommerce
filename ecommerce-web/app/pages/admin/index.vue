
<template>
  <div class="space-y-6 admin-overview-page">
    <!-- Header -->
    <div class="admin-box admin-box--hero">
      <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <div>
          <div class="text-xl font-extrabold rtl-text">{{ t('admin.dashboard') }}</div>
          <div class="text-sm admin-muted rtl-text">{{ t('admin.dashboardHint') }}</div>
        </div>

        <div class="flex flex-col sm:flex-row gap-2 sm:items-center">
          <div class="flex gap-2">
            <button class="admin-chip" :class="range==='daily' ? 'is-active' : ''" type="button" @click="range='daily'">
              يومي
            </button>
            <button class="admin-chip" :class="range==='monthly' ? 'is-active' : ''" type="button" @click="range='monthly'">
              شهري
            </button>
          </div>

          <button class="admin-ghost" type="button" @click="loadAll" :disabled="loading">
            {{ loading ? t('common.loading') : t('common.refresh') }}
          </button>
        </div>
      </div>
    </div>

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div class="kpi-card">
        <div class="kpi-icon">
          <svg viewBox="0 0 24 24" class="kpi-ic" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 7h18M3 12h18M3 17h18" />
          </svg>
        </div>
        <div class="min-w-0">
          <div class="kpi-label rtl-text">{{ $t('admin.cards.orders') }}</div>
          <div class="kpi-value keep-ltr">{{ stats.totalOrders }}</div>
          <div class="kpi-sub rtl-text">{{ $t('admin.lastUpdated') }}: {{ lastUpdatedLabel }}</div>
        </div>
      </div>

      <!-- تم إخفاء (المستخدمين) و(الإيرادات) حسب الطلب -->

      <div class="kpi-card">
        <div class="kpi-icon">
          <svg viewBox="0 0 24 24" class="kpi-ic" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z"/>
            <path d="M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z"/>
          </svg>
        </div>
        <div class="min-w-0">
          <div class="kpi-label rtl-text">{{ $t('admin.cards.visitsToday') }}</div>
          <div class="kpi-value keep-ltr">{{ visits.today }}</div>
          <div class="kpi-sub rtl-text">{{ $t('admin.totalVisits') }}: {{ visits.total }}</div>
        </div>
      </div>

      <div class="kpi-card">
        <div class="min-w-0">
          <div class="kpi-label rtl-text">نفاد الكمية</div>
          <div class="kpi-value keep-ltr">{{ stats.outOfStockCount }}</div>
          <div class="kpi-sub rtl-text">منخفض المخزون: {{ stats.lowStockCount }}</div>
        </div>
      </div>
    </div>

    <!-- Activity + Top lists -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      <div class="admin-box xl:col-span-2">
        <div class="flex items-center justify-between gap-3 mb-4">
          <div class="font-extrabold rtl-text">النشاط {{ range==='daily' ? 'اليومي' : 'الشهري' }}</div>
          <div class="admin-muted text-sm rtl-text">
            {{ range==='daily' ? $t('admin.last30Days') : $t('admin.last12Months') }}
          </div>
        </div>

        <div v-if="activityBars.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="bars-shell"><div class="bars">
          <div v-for="b in activityBars" :key="b.key" class="bar">
            <div class="bar-fill" :style="{ height: b.h + '%' }"></div>
            <div class="bar-label keep-ltr">{{ b.label }}</div>
          </div>
        </div></div>

        <div class="mt-4 grid grid-cols-1 md:grid-cols-3 gap-3">
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">طلبات</div>
            <div class="font-black keep-ltr">{{ activityTotals.orders }}</div>
          </div>
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">مستخدمين جدد</div>
            <div class="font-black keep-ltr">{{ activityTotals.users }}</div>
          </div>
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">زيارات</div>
            <div class="font-black keep-ltr">{{ activityTotals.visits }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="flex items-center justify-between gap-3 mb-3"><div class="font-extrabold rtl-text">{{ $t('admin.summary.title') }}</div><div class="text-xs admin-muted rtl-text">{{ $t('admin.summaryTables') }}</div></div>

        <div class="space-y-3">
          <div class="topbox">
            <div class="topbox-title rtl-text">🔥 {{ $t('admin.summary.mostPurchased') }}</div>
            <div v-if="overview.topPurchased.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="metric-table">
              <div class="metric-table__head">
                <div>#</div>
                <div class="rtl-text">{{ $t('admin.tableProduct') }}</div>
                <div class="text-right rtl-text">{{ $t('admin.tableMetric') }}</div>
              </div>
              <div v-for="(x, idx) in overview.topPurchased.slice(0,5)" :key="x.productId" class="metric-table__row">
                <div class="metric-rank">{{ idx + 1 }}</div>
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black text-right">{{ x.purchases }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">👁️ {{ $t('admin.summary.mostViewed') }}</div>
            <div v-if="overview.topViews.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="metric-table">
              <div class="metric-table__head">
                <div>#</div>
                <div class="rtl-text">{{ $t('admin.tableProduct') }}</div>
                <div class="text-right rtl-text">{{ $t('admin.tableMetric') }}</div>
              </div>
              <div v-for="(x, idx) in overview.topViews.slice(0,5)" :key="x.productId" class="metric-table__row">
                <div class="metric-rank">{{ idx + 1 }}</div>
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black text-right">{{ x.views }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">❤️ {{ $t('admin.summary.mostFavorited') }}</div>
            <div v-if="overview.topFavorites.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="metric-table">
              <div class="metric-table__head">
                <div>#</div>
                <div class="rtl-text">{{ $t('admin.tableProduct') }}</div>
                <div class="text-right rtl-text">{{ $t('admin.tableMetric') }}</div>
              </div>
              <div v-for="(x, idx) in overview.topFavorites.slice(0,5)" :key="x.productId" class="metric-table__row">
                <div class="metric-rank">{{ idx + 1 }}</div>
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black text-right">{{ x.favorites }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">📦 منخفض المخزون</div>
            <div v-if="overview.lowStock.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="metric-table">
              <div v-for="x in overview.lowStock.slice(0,5)" :key="x.productId" class="metric-table__row">
                <div></div><div class="rtl-text truncate font-bold">{{ x.title }}</div><div class="keep-ltr font-black text-right">{{ x.stockQuantity }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">🚫 نافد المخزون</div>
            <div v-if="overview.outOfStock.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="metric-table">
              <div v-for="x in overview.outOfStock.slice(0,5)" :key="x.productId" class="metric-table__row">
                <div></div><div class="rtl-text truncate font-bold">{{ x.title }}</div><div class="keep-ltr font-black text-right">{{ x.stockQuantity }}</div>
              </div>
            </div>
          </div>

          <NuxtLink class="admin-link rtl-text" to="/admin/insights">
            عرض التفاصيل في صفحة Insights →
          </NuxtLink>
        </div>
      </div>
    </div>

    <!-- Latest Orders + Quick actions -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      <div class="admin-box xl:col-span-2">
        <div class="flex items-center justify-between gap-3 mb-4">
          <div class="font-extrabold rtl-text">{{ $t('admin.latestOrders') }}</div>
          <NuxtLink class="admin-link rtl-text" to="/admin/orders">{{ $t('admin.viewAll') }}</NuxtLink>
        </div>

        <div v-if="latestOrders.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="orders">
          <div v-for="o in latestOrders" :key="o.id" class="order-row">
            <div class="min-w-0">
              <div class="flex items-center gap-2">
                <div class="order-badge" :class="badgeClass(o.status)">{{ o.status }}</div>
                <div class="rtl-text font-extrabold truncate">{{ o.primaryItemTitle || '—' }}</div>
              </div>
              <div class="admin-muted text-xs rtl-text mt-1 truncate">
                {{ o.userFullName || o.userEmail || '—' }} · {{ formatDate(o.createdAt) }}
              </div>
            </div>

            <div class="keep-ltr font-black">{{ formatMoney(o.totalIqd) }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="font-extrabold rtl-text mb-3">{{ $t('admin.adminShortcuts') }}</div>
        <div class="grid gap-3">
          <NuxtLink class="admin-action" to="/admin/products">
            <div class="font-extrabold rtl-text">{{ t('admin.manageProducts') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ t('admin.manageProductsHint') }}</div>
          </NuxtLink>

          <NuxtLink class="admin-action" to="/admin/orders">
            <div class="font-extrabold rtl-text">{{ t('admin.manageOrders') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ t('admin.manageOrdersHint') }}</div>
          </NuxtLink>

          <NuxtLink class="admin-action" to="/admin/brands">
            <div class="font-extrabold rtl-text">{{ $t('admin.manageBrands') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ $t('admin.manageBrandsHint') }}</div>
          </NuxtLink>

          <NuxtLink class="admin-action" to="/admin/insights">
            <div class="font-extrabold rtl-text">Insights</div>
            <div class="admin-muted text-sm rtl-text">تحليلات المفضلة/الزيارة/الشراء</div>
          </NuxtLink>
        </div>
      </div>
    </div>

    <div v-if="error" class="admin-error rtl-text">{{ error }}</div>
  </div>
</template>


<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { computed, ref, watch } from 'vue'
import { useI18n } from '~/composables/useI18n'
import { useAdminApi } from '~/composables/useAdminApi'
import { useApi } from '~/composables/useApi'
import { formatIqd } from '~/composables/useMoney'

const { t } = useI18n()
const adminApi = useAdminApi()
const api = useApi()

const loading = ref(false)
const error = ref('')

const range = ref<'daily'|'monthly'>('daily')
const lastUpdatedAt = ref<Date | null>(null)

const stats = ref({
  totalOrders: 0,
  totalUsers: 0,
  totalRevenueIqd: 0,
  totalProducts: 0,
  outOfStockCount: 0,
  lowStockCount: 0,
})

const visits = ref({
  total: 0,
  today: 0,
  month: 0,
})

const overview = ref({
  topPurchased: [] as any[],
  topFavorites: [] as any[],
  topViews: [] as any[],
  neglected: [] as any[],
  lowStock: [] as any[],
  outOfStock: [] as any[],
})

const activity = ref({
  daily: [] as any[],
  monthly: [] as any[],
})

const latestOrders = ref<any[]>([])

function extractErr(e: any) {
  return e?.data?.message || e?.message || t('common.requestFailed')
}

function formatMoney(v: any) { return formatIqd(v) }

function formatDate(v: any) {
  if (!v) return '—'
  try {
    const d = new Date(v)
    return d.toLocaleString('en-GB', { year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit' })
  } catch { return String(v) }
}

const lastUpdatedLabel = computed(() => {
  if (!lastUpdatedAt.value) return '—'
  return lastUpdatedAt.value.toLocaleTimeString('en-GB', { hour:'2-digit', minute:'2-digit' })
})

const activitySeries = computed(() => range.value === 'daily' ? activity.value.daily : activity.value.monthly)

const activityTotals = computed(() => {
  const s = activitySeries.value || []
  const sum = (k: string) => s.reduce((a: number, x: any) => a + Number(x?.[k] ?? 0), 0)
  return {
    orders: sum('orders'),
    users: sum('users'),
    visits: sum('visits'),
  }
})

const activityBars = computed(() => {
  const s = activitySeries.value || []
  if (s.length === 0) return []
  const maxV = Math.max(1, ...s.map((x: any) => Number(x?.visits ?? 0)))
  return s.map((x: any, idx: number) => {
    const v = Number(x?.visits ?? 0)
    const h = Math.round((v / maxV) * 100)
    const label = String(x?.label ?? x?.key ?? idx+1)
    return { key: String(x?.key ?? idx), label, h }
  })
})

function badgeClass(status: string) {
  const s = String(status || '').toLowerCase()
  if (s.includes('paid') || s.includes('complete') || s.includes('done')) return 'ok'
  if (s.includes('cancel')) return 'bad'
  if (s.includes('pending') || s.includes('new')) return 'warn'
  return 'neutral'
}

async function loadAll() {
  loading.value = true
  error.value = ''
  try {
    const [dash, ov, act, vis, orders] = await Promise.all([
      adminApi.getDashboardStats<any>(),
      adminApi.get<any>('/admin/analytics/overview', { days: 3650 }),
      adminApi.get<any>('/admin/analytics/activity'),
      api.get<any>('/metrics/visits/summary'),
      adminApi.get<any>('/admin/orders'),
    ])

    stats.value.totalOrders = Number(dash?.totalOrders ?? 0)
    stats.value.totalUsers = Number(dash?.totalUsers ?? 0)
    stats.value.totalRevenueIqd = Number(dash?.totalRevenueIqd ?? 0)
    stats.value.totalProducts = Number(dash?.totalProducts ?? 0)
    stats.value.outOfStockCount = Number(dash?.outOfStockCount ?? 0)
    stats.value.lowStockCount = Number(dash?.lowStockCount ?? 0)

    const normalizeMetricRows = (rows: any[] = [], metricKeys: string[] = []) =>
      rows
        .map((x: any, idx: number) => ({
          ...x,
          productId: x?.productId ?? x?.id ?? idx,
          title: String(x?.title ?? x?.name ?? x?.productTitle ?? '').trim(),
          metric: metricKeys.reduce((acc, k) => acc || Number(x?.[k] ?? 0), 0),
        }))
        .filter((x: any) => x.title || x.metric > 0)

    overview.value.topPurchased = normalizeMetricRows(Array.isArray(ov?.topPurchased) ? ov.topPurchased : [], ['purchases','count','quantity','total'])
    overview.value.topFavorites = normalizeMetricRows(Array.isArray(ov?.topFavorites) ? ov.topFavorites : [], ['favorites','count','total'])
    overview.value.topViews = normalizeMetricRows(Array.isArray(ov?.topViews) ? ov.topViews : [], ['views','count','total'])
    overview.value.neglected = normalizeMetricRows(Array.isArray(ov?.neglected) ? ov.neglected : [], ['views','favorites','purchases'])
    overview.value.lowStock = Array.isArray(ov?.lowStock) ? ov.lowStock : []
    overview.value.outOfStock = Array.isArray(ov?.outOfStock) ? ov.outOfStock : []

    activity.value.daily = Array.isArray(act?.daily) ? act.daily : []
    activity.value.monthly = Array.isArray(act?.monthly) ? act.monthly : []

    visits.value.total = Number(vis?.total ?? 0)
    visits.value.today = Number(vis?.today ?? 0)
    visits.value.month = Number(vis?.month ?? 0)

    const list = Array.isArray(orders) ? orders : (Array.isArray(orders?.items) ? orders.items : [])
    latestOrders.value = list
      .slice()
      .sort((a: any, b: any) => new Date(b?.createdAt || 0).getTime() - new Date(a?.createdAt || 0).getTime())
      .slice(0, 6)
      .map((o: any) => ({
        id: o?.id || o?.Id,
        status: o?.status || o?.Status || '—',
        totalIqd: o?.totalIqd ?? o?.TotalIqd ?? 0,
        createdAt: o?.createdAt || o?.CreatedAt,
        userEmail: o?.userEmail || o?.User?.Email,
        userFullName: o?.userFullName || o?.User?.FullName,
        primaryItemTitle: o?.primaryItemTitle || '—',
      }))

    lastUpdatedAt.value = new Date()
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

watch(range, () => {
  // مجرد تحديث UI (البيانات نفسها محمّلة)
})

loadAll()
</script>


<style scoped>
.admin-overview-page{
  --admin-shadow: 0 24px 70px rgba(12, 16, 32, .16);
}
.admin-box{
  border-radius: 26px;
  border: 1px solid rgba(var(--border), .95);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .97), rgba(var(--surface-rgb), .89));
  padding: 18px;
  box-shadow: var(--admin-shadow);
}
.admin-box--hero{
  position: relative;
  overflow: hidden;
}
.admin-box--hero::after{
  content: '';
  position: absolute;
  inset: auto auto -90px -30px;
  width: 250px;
  height: 250px;
  border-radius: 999px;
  background: radial-gradient(circle, rgba(var(--primary), .16), transparent 67%);
  pointer-events: none;
}
.admin-muted{ color: rgb(var(--muted)); }
.admin-error{
  border: 1px solid rgba(255,0,0,.25);
  background: rgba(255,0,0,.08);
  color: #ffb4b4;
  padding: 12px 14px;
  border-radius: 16px;
}
.admin-chip{
  border-radius: 999px;
  border: 1px solid rgb(var(--border));
  padding: 9px 14px;
  font-weight: 900;
  font-size: 12px;
  background: rgba(255,255,255,.03);
}
.admin-chip.is-active{
  background: rgba(124,58,237,.18);
  border-color: rgba(124,58,237,.45);
}
.kpi-card{
  border-radius: 24px;
  border: 1px solid rgba(var(--border), .95);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .92), rgba(var(--surface-2-rgb), .86));
  padding: 16px;
  display: flex;
  gap: 12px;
  align-items: center;
  box-shadow: 0 14px 34px rgba(0,0,0,.10);
}
.kpi-icon{
  width: 46px; height: 46px;
  border-radius: 16px;
  display:flex; align-items:center; justify-content:center;
  background: rgba(124,58,237,.18);
  border: 1px solid rgba(124,58,237,.35);
}
.kpi-ic{ width: 22px; height: 22px; }
.kpi-label{ font-size: 12px; font-weight: 800; color: rgb(var(--muted)); }
.kpi-value{ font-size: 30px; font-weight: 1000; margin-top: 2px; }
.kpi-sub{ font-size: 12px; color: rgb(var(--muted)); margin-top: 4px; }
.bars-shell{ overflow-x:auto; padding-bottom:6px; }
.bars{
  height: 190px;
  min-width: 520px;
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: 1fr;
  gap: 8px;
  align-items: end;
}
.bar{
  position: relative;
  height: 100%;
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  overflow: hidden;
}
.bar-fill{
  position: absolute;
  bottom: 0; left: 0; right: 0;
  background: linear-gradient(180deg, rgba(124,58,237,.32), rgba(124,58,237,.52));
  border-top: 1px solid rgba(124,58,237,.45);
}
.bar-label{
  position: absolute;
  bottom: 6px;
  left: 0; right: 0;
  text-align: center;
  font-size: 10px;
  color: rgba(255,255,255,.7);
}
.mini-stat{
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  padding: 12px 14px;
}
.topbox{
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  padding: 12px 14px;
}
.topbox-title{ font-weight: 900; margin-bottom: 8px; }
.metric-table{display:grid;gap:8px;}
.metric-table__head,.metric-table__row{display:grid;grid-template-columns:36px minmax(0,1fr) 66px;gap:10px;align-items:center;}
.metric-table__head{padding-bottom:8px;border-bottom:1px dashed rgb(var(--border));font-size:11px;color:rgb(var(--muted));text-transform:uppercase;letter-spacing:.06em;}
.metric-table__row{padding-top:10px;border-top:1px solid rgba(var(--border), .75);}
.metric-rank,.rank-badge{width:28px;height:28px;border-radius:999px;display:grid;place-items:center;background:rgba(124,58,237,.16);border:1px solid rgba(124,58,237,.34);font-weight:900;}
.orders{display:grid;gap:10px;}
.order-row{display:flex;align-items:center;justify-content:space-between;gap:12px;padding:14px;border-radius:18px;border:1px solid rgba(var(--border), .88);background:rgba(var(--surface-2-rgb), .74);}
.order-badge{padding:6px 10px;border-radius:999px;font-size:11px;font-weight:900;border:1px solid rgba(var(--border), .9);}
.order-badge.ok{background:rgba(16,185,129,.12);border-color:rgba(16,185,129,.4);}
.order-badge.warn{background:rgba(245,158,11,.14);border-color:rgba(245,158,11,.38);}
.order-badge.bad{background:rgba(239,68,68,.14);border-color:rgba(239,68,68,.38);}
.order-badge.neutral{background:rgba(255,255,255,.05);}
.admin-action,.admin-link,.admin-ghost{display:inline-flex;align-items:center;justify-content:center;min-height:44px;padding:10px 14px;border-radius:16px;font-weight:800;border:1px solid rgb(var(--border));background:rgb(var(--surface-2));}
.admin-link{background:rgba(var(--primary), .1);border-color:rgba(var(--primary), .3);}
@media (max-width: 768px){
  .admin-box{ border-radius:22px; padding:14px; }
  .kpi-card{ border-radius:20px; }
  .orders{ gap:8px; }
  .order-row{ align-items:flex-start; flex-direction:column; }
}
</style>

