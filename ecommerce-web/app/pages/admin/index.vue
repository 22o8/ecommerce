<template>
  <div class="space-y-6">
    <div class="admin-box dashboard-hero">
      <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <div class="text-xl font-extrabold rtl-text">{{ t('admin.dashboard') }}</div>
          <div class="text-sm admin-muted rtl-text">{{ t('admin.dashboardHint') }}</div>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <div class="inline-flex rounded-full border border-app bg-surface-2 p-1">
            <button class="admin-chip" :class="range==='daily' ? 'is-active' : ''" type="button" @click="range='daily'">
              {{ t('admin.rangeDaily') }}
            </button>
            <button class="admin-chip" :class="range==='monthly' ? 'is-active' : ''" type="button" @click="range='monthly'">
              {{ t('admin.rangeMonthly') }}
            </button>
          </div>

          <button class="admin-ghost" type="button" @click="loadAll" :disabled="loading">
            {{ loading ? t('common.loading') : t('common.refresh') }}
          </button>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-2">
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

    <div class="grid grid-cols-1 gap-4 xl:grid-cols-3">
      <div class="admin-box xl:col-span-2">
        <div class="mb-4 flex items-center justify-between gap-3">
          <div class="font-extrabold rtl-text">{{ range==='daily' ? t('admin.activityDaily') : t('admin.activityMonthly') }}</div>
          <div class="admin-muted text-sm rtl-text">
            {{ range==='daily' ? $t('admin.last30Days') : $t('admin.last12Months') }}
          </div>
        </div>

        <div v-if="loading && activityBars.length===0" class="admin-muted rtl-text">{{ t('common.loading') }}...</div>
        <div v-else-if="activityBars.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="bars">
          <div v-for="b in activityBars" :key="b.key" class="bar">
            <div class="bar-fill" :style="{ height: b.h + '%' }"></div>
            <div class="bar-label keep-ltr">{{ b.label }}</div>
          </div>
        </div>

        <div class="mt-4 grid grid-cols-1 gap-3 md:grid-cols-3">
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">{{ t('admin.ordersLabel') }}</div>
            <div class="font-black keep-ltr">{{ activityTotals.orders }}</div>
          </div>
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">{{ t('admin.newUsersLabel') }}</div>
            <div class="font-black keep-ltr">{{ activityTotals.users }}</div>
          </div>
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">{{ t('admin.visitsLabel') }}</div>
            <div class="font-black keep-ltr">{{ activityTotals.visits }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="mb-3 font-extrabold rtl-text">{{ $t('admin.summary.title') }}</div>

        <div class="space-y-3">
          <div class="topbox">
            <div class="topbox-title rtl-text">🔥 {{ $t('admin.summary.mostPurchased') }}</div>
            <div v-if="overview.topPurchased.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="grid gap-2">
              <div v-for="x in overview.topPurchased.slice(0,5)" :key="x.productId" class="toprow">
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black">{{ x.purchases }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">👁️ {{ $t('admin.summary.mostViewed') }}</div>
            <div v-if="overview.topViews.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="grid gap-2">
              <div v-for="x in overview.topViews.slice(0,5)" :key="x.productId" class="toprow">
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black">{{ x.views }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">❤️ {{ $t('admin.summary.mostFavorited') }}</div>
            <div v-if="overview.topFavorites.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="grid gap-2">
              <div v-for="x in overview.topFavorites.slice(0,5)" :key="x.productId" class="toprow">
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black">{{ x.favorites }}</div>
              </div>
            </div>
          </div>

          <NuxtLink class="admin-link rtl-text" to="/admin/insights">
            {{ t('admin.viewInsightsDetails') }}
          </NuxtLink>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-4 xl:grid-cols-3">
      <div class="admin-box xl:col-span-2">
        <div class="mb-4 flex items-center justify-between gap-3">
          <div class="font-extrabold rtl-text">{{ $t('admin.latestOrders') }}</div>
          <NuxtLink class="admin-link rtl-text" to="/admin/orders">{{ $t('admin.viewAll') }}</NuxtLink>
        </div>

        <div v-if="latestOrders.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="orders">
          <div v-for="o in latestOrders" :key="o.id" class="order-row">
            <div class="min-w-0">
              <div class="flex items-center gap-2">
                <div class="order-badge" :class="badgeClass(o.status)">{{ o.status }}</div>
                <div class="rtl-text truncate font-extrabold">{{ o.primaryItemTitle || '—' }}</div>
              </div>
              <div class="admin-muted mt-1 truncate text-xs rtl-text">
                {{ o.userFullName || o.userEmail || '—' }} · {{ formatDate(o.createdAt) }}
              </div>
            </div>

            <div class="keep-ltr font-black">{{ formatMoney(o.totalIqd) }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="mb-3 font-extrabold rtl-text">{{ $t('admin.adminShortcuts') }}</div>
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
            <div class="font-extrabold rtl-text">{{ t('admin.insights') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ t('admin.insightsHint') }}</div>
          </NuxtLink>
        </div>
      </div>
    </div>

    <div v-if="error" class="admin-error rtl-text">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { computed, ref, onMounted } from 'vue'
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

async function withTimeout<T>(promise: Promise<T>, ms = 6000): Promise<T> {
  return await Promise.race([
    promise,
    new Promise<T>((_, reject) => setTimeout(() => reject(new Error('timeout')), ms)),
  ])
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
    const [dash, ov, act, vis, orders] = await Promise.allSettled([
      withTimeout(adminApi.getDashboardStats<any>()),
      withTimeout(adminApi.get<any>('/admin/analytics/overview')),
      withTimeout(adminApi.get<any>('/admin/analytics/activity')),
      withTimeout(api.get<any>('/metrics/visits/summary')),
      withTimeout(adminApi.get<any>('/admin/orders')),
    ])

    if (dash.status === 'fulfilled') {
      stats.value.totalOrders = Number(dash.value?.totalOrders ?? 0)
      stats.value.totalUsers = Number(dash.value?.totalUsers ?? 0)
      stats.value.totalRevenueIqd = Number(dash.value?.totalRevenueIqd ?? 0)
    }
    if (ov.status === 'fulfilled') {
      overview.value.topPurchased = Array.isArray(ov.value?.topPurchased) ? ov.value.topPurchased : []
      overview.value.topFavorites = Array.isArray(ov.value?.topFavorites) ? ov.value.topFavorites : []
      overview.value.topViews = Array.isArray(ov.value?.topViews) ? ov.value.topViews : []
      overview.value.neglected = Array.isArray(ov.value?.neglected) ? ov.value.neglected : []
    }
    if (act.status === 'fulfilled') {
      activity.value.daily = Array.isArray(act.value?.daily) ? act.value.daily : []
      activity.value.monthly = Array.isArray(act.value?.monthly) ? act.value.monthly : []
    }
    if (vis.status === 'fulfilled') {
      visits.value.total = Number(vis.value?.total ?? 0)
      visits.value.today = Number(vis.value?.today ?? 0)
      visits.value.month = Number(vis.value?.month ?? 0)
    }
    if (orders.status === 'fulfilled') {
      const list = Array.isArray(orders.value) ? orders.value : (Array.isArray((orders.value as any)?.items) ? (orders.value as any).items : [])
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
    }

    lastUpdatedAt.value = new Date()
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadAll()
})
</script>

<style scoped>
.admin-box{
  border-radius: 24px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface));
  padding: 16px;
}
.dashboard-hero{
  background: linear-gradient(135deg, rgba(124,58,237,.08), rgba(255,255,255,.02));
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
  border: 1px solid transparent;
  padding: 8px 14px;
  font-weight: 900;
  font-size: 12px;
  color: rgb(var(--muted));
}
.admin-chip.is-active{
  background: rgba(124,58,237,.18);
  border-color: rgba(124,58,237,.45);
  color: rgb(var(--fg));
}
.kpi-card{
  border-radius: 22px;
  border: 1px solid rgb(var(--border));
  background: linear-gradient(180deg, rgba(255,255,255,.03), rgba(255,255,255,.01));
  padding: 14px;
  display: flex;
  gap: 12px;
  align-items: center;
  box-shadow: 0 10px 30px rgba(0,0,0,.10);
}
.kpi-icon{
  width: 48px; height: 48px;
  border-radius: 16px;
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
  grid-auto-columns: minmax(20px, 1fr);
  gap: 8px;
  align-items: end;
  overflow-x: auto;
  padding-bottom: 4px;
}
.bar{
  position: relative;
  height: 100%;
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
  overflow: hidden;
  min-width: 22px;
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
.mini-stat,.topbox,.order-row,.admin-action{
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.02);
}
.mini-stat{ padding: 10px 12px; }
.topbox{ padding: 10px 12px; }
.topbox-title{ font-weight: 900; margin-bottom: 8px; }
.toprow{ display:flex; align-items:center; justify-content: space-between; gap: 10px; }
.orders{ display:grid; gap:10px; }
.order-row{ padding: 10px 12px; display:flex; align-items:center; justify-content: space-between; gap: 12px; }
.order-badge{
  font-size: 10px; font-weight: 900; padding: 4px 8px; border-radius: 999px;
  border: 1px solid rgb(var(--border)); background: rgba(255,255,255,.02); white-space: nowrap;
}
.order-badge.ok{ background: rgba(34,197,94,.14); border-color: rgba(34,197,94,.35); }
.order-badge.warn{ background: rgba(234,179,8,.14); border-color: rgba(234,179,8,.35); }
.order-badge.bad{ background: rgba(239,68,68,.14); border-color: rgba(239,68,68,.35); }
.order-badge.neutral{ background: rgba(148,163,184,.12); border-color: rgba(148,163,184,.25); }
.admin-link{ display:inline-flex; align-items:center; gap: 6px; font-weight: 900; color: rgba(124,58,237,.95); }
.admin-ghost{
  padding: 10px 12px; border-radius: 14px; border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2)); color: rgb(var(--fg)); font-weight: 900;
}
.admin-action{ display:block; padding: 14px; transition: transform .15s ease, border-color .15s ease, background .15s ease; }
.admin-action:hover{ transform: translateY(-2px); border-color: rgba(124,58,237,.45); background: rgba(124,58,237,.10); }
@media (max-width: 768px){
  .admin-box{ padding: 14px; border-radius: 20px; }
  .kpi-value{ font-size: 22px; }
}
</style>
