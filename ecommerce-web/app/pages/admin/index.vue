
<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="admin-box">
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
    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-2 gap-4">
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
        <div v-else class="bars">
          <div v-for="b in activityBars" :key="b.key" class="bar">
            <div class="bar-fill" :style="{ height: b.h + '%' }"></div>
            <div class="bar-label keep-ltr">{{ b.label }}</div>
          </div>
        </div>

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
      adminApi.get<any>('/admin/analytics/overview'),
      adminApi.get<any>('/admin/analytics/activity'),
      api.get<any>('/metrics/visits/summary'),
      adminApi.get<any>('/admin/orders'),
    ])

    const list = Array.isArray(orders) ? orders : (Array.isArray(orders?.items) ? orders.items : [])

    stats.value.totalOrders = Number(dash?.totalOrders ?? 0) || list.length
    stats.value.totalUsers = Number(dash?.totalUsers ?? 0)
    stats.value.totalRevenueIqd = Number(dash?.totalRevenueIqd ?? dash?.totalRevenueUsd ?? 0)

    const normalizedPurchased = Array.isArray(ov?.topPurchased) ? ov.topPurchased : []
    const purchasedFallback = list
      .flatMap((o: any) => Array.isArray(o?.Items) ? o.Items : Array.isArray(o?.items) ? o.items : [])
      .filter((i: any) => !!(i?.productTitle || i?.ProductTitle))
      .reduce((acc: any[], i: any) => {
        const productId = i?.ProductId || i?.productId || i?.productTitle || i?.ProductTitle
        const title = i?.productTitle || i?.ProductTitle || '—'
        const qty = Number(i?.Quantity ?? i?.quantity ?? 1)
        const ex = acc.find((x: any) => x.productId === productId)
        if (ex) ex.purchases += qty
        else acc.push({ productId, title, purchases: qty })
        return acc
      }, [])
      .sort((a: any, b: any) => b.purchases - a.purchases)
      .slice(0, 10)

    overview.value.topPurchased = normalizedPurchased.length ? normalizedPurchased : purchasedFallback
    overview.value.topFavorites = Array.isArray(ov?.topFavorites) ? ov.topFavorites : []
    overview.value.topViews = Array.isArray(ov?.topViews) ? ov.topViews : []
    overview.value.neglected = Array.isArray(ov?.neglected) ? ov.neglected : []

    activity.value.daily = Array.isArray(act?.daily) ? act.daily : []
    activity.value.monthly = Array.isArray(act?.monthly) ? act.monthly : []

    visits.value.total = Number(vis?.total ?? 0)
    visits.value.today = Number(vis?.today ?? 0)
    visits.value.month = Number(vis?.month ?? 0)

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
.admin-box{
  border-radius: 20px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface));
  padding: 16px;
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
  padding: 8px 12px;
  font-weight: 900;
  font-size: 12px;
  background: rgba(255,255,255,.02);
}
.admin-chip.is-active{
  background: rgba(124,58,237,.18);
  border-color: rgba(124,58,237,.45);
}

.kpi-card{
  border-radius: 20px;
  border: 1px solid rgb(var(--border));
  background: linear-gradient(180deg, rgba(255,255,255,.03), rgba(255,255,255,.01));
  padding: 14px;
  display: flex;
  gap: 12px;
  align-items: center;
  box-shadow: 0 10px 30px rgba(0,0,0,.10);
}
.kpi-icon{
  width: 44px; height: 44px;
  border-radius: 14px;
  display:flex; align-items:center; justify-content:center;
  background: rgba(124,58,237,.18);
  border: 1px solid rgba(124,58,237,.35);
}
.kpi-ic{ width: 22px; height: 22px; }
.kpi-label{ font-size: 12px; font-weight: 800; color: rgb(var(--muted)); }
.kpi-value{ font-size: 26px; font-weight: 1000; margin-top: 2px; }
.kpi-sub{ font-size: 12px; color: rgb(var(--muted)); margin-top: 4px; }

.bars{
  height: 170px;
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: 1fr;
  gap: 8px;
  align-items: end;
}
.bar{
  position: relative;
  height: 100%;
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  overflow: hidden;
}
.bar-fill{
  position: absolute;
  bottom: 0; left: 0; right: 0;
  background: rgba(124,58,237,.38);
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
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  padding: 10px 12px;
}

.topbox{
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  padding: 10px 12px;
}
.topbox-title{
  font-weight: 900;
  margin-bottom: 8px;
}
.toprow{
  display:flex;
  align-items:center;
  justify-content: space-between;
  gap: 10px;
}

.orders{ display:grid; gap:10px; }
.order-row{
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  padding: 10px 12px;
  display:flex;
  align-items:center;
  justify-content: space-between;
  gap: 12px;
}
.order-badge{
  font-size: 10px;
  font-weight: 900;
  padding: 4px 8px;
  border-radius: 999px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  white-space: nowrap;
}
.order-badge.ok{ background: rgba(34,197,94,.14); border-color: rgba(34,197,94,.35); }
.order-badge.warn{ background: rgba(234,179,8,.14); border-color: rgba(234,179,8,.35); }
.order-badge.bad{ background: rgba(239,68,68,.14); border-color: rgba(239,68,68,.35); }
.order-badge.neutral{ background: rgba(148,163,184,.12); border-color: rgba(148,163,184,.25); }

.admin-link{
  display:inline-flex;
  align-items:center;
  gap: 6px;
  font-weight: 900;
  color: rgba(124,58,237,.95);
}

.admin-ghost{
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  color: rgb(var(--fg));
  font-weight: 900;
}

.admin-action{
  display:block;
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  padding: 14px;
  transition: transform .15s ease, border-color .15s ease, background .15s ease;
}
.admin-action:hover{
  transform: translateY(-2px);
  border-color: rgba(124,58,237,.45);
  background: rgba(124,58,237,.10);
}


.metric-table{display:grid; gap:8px;}
.metric-table__head,.metric-table__row{display:grid; grid-template-columns:40px minmax(0,1fr) 72px; gap:10px; align-items:center;}
.metric-table__head{font-size:11px; color:rgb(var(--muted)); text-transform:uppercase; letter-spacing:.06em; padding-bottom:6px; border-bottom:1px dashed rgb(var(--border));}
.metric-table__row{padding:10px 0; border-top:1px solid rgba(var(--border),.7);}
.metric-table__row:first-of-type{border-top:none;}
.metric-rank{width:28px;height:28px;border-radius:999px;display:grid;place-items:center;background:rgba(124,58,237,.16);border:1px solid rgba(124,58,237,.34);font-weight:900;}
@media (max-width: 768px){.metric-table__head,.metric-table__row{grid-template-columns:32px minmax(0,1fr) 54px;gap:8px;}}

</style>

