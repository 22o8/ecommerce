<template>
  <div class="space-y-6 admin-dashboard-page">
    <section class="admin-hero">
      <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <div class="admin-kicker rtl-text">{{ t('admin.overview') }}</div>
          <div class="mt-2 text-2xl font-black rtl-text sm:text-3xl">{{ t('admin.dashboard') }}</div>
          <div class="mt-2 text-sm admin-muted rtl-text">{{ t('admin.dashboardHint') }}</div>
        </div>

        <div class="flex flex-col gap-3 sm:flex-row sm:items-center">
          <div class="inline-flex items-center rounded-full border border-app bg-surface p-1">
            <button class="admin-chip" :class="range==='daily' ? 'is-active' : ''" type="button" @click="range='daily'">{{ t('admin.daily') }}</button>
            <button class="admin-chip" :class="range==='monthly' ? 'is-active' : ''" type="button" @click="range='monthly'">{{ t('admin.monthly') }}</button>
          </div>

          <button class="admin-ghost" type="button" @click="loadAll" :disabled="loading">
            {{ loading ? t('common.loading') : t('common.refresh') }}
          </button>
        </div>
      </div>
    </section>

    <section class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
      <div class="admin-stat-card">
        <div class="admin-stat-card__icon">
          <Icon name="mdi:receipt-text-outline" class="text-xl" />
        </div>
        <div>
          <div class="admin-stat-card__label rtl-text">{{ t('admin.cards.orders') }}</div>
          <div class="admin-stat-card__value keep-ltr">{{ stats.totalOrders }}</div>
          <div class="admin-stat-card__hint rtl-text">{{ t('admin.lastUpdated') }}: {{ lastUpdatedLabel }}</div>
        </div>
      </div>

      <div class="admin-stat-card">
        <div class="admin-stat-card__icon admin-stat-card__icon--violet">
          <Icon name="mdi:eye-outline" class="text-xl" />
        </div>
        <div>
          <div class="admin-stat-card__label rtl-text">{{ t('admin.cards.visitsToday') }}</div>
          <div class="admin-stat-card__value keep-ltr">{{ visits.today }}</div>
          <div class="admin-stat-card__hint rtl-text">{{ t('admin.totalVisits') }}: {{ visits.total }}</div>
        </div>
      </div>

      <div class="admin-stat-card">
        <div class="admin-stat-card__icon admin-stat-card__icon--mint">
          <Icon name="mdi:star-four-points-outline" class="text-xl" />
        </div>
        <div>
          <div class="admin-stat-card__label rtl-text">{{ t('admin.cards.featuredProducts') }}</div>
          <div class="admin-stat-card__value keep-ltr">{{ overview.topPurchased.length }}</div>
          <div class="admin-stat-card__hint rtl-text">{{ t('admin.cards.quickSummary') }}</div>
        </div>
      </div>

      <div class="admin-stat-card">
        <div class="admin-stat-card__icon admin-stat-card__icon--amber">
          <Icon name="mdi:clock-outline" class="text-xl" />
        </div>
        <div>
          <div class="admin-stat-card__label rtl-text">{{ t('admin.cards.activityWindow') }}</div>
          <div class="admin-stat-card__value rtl-text text-xl">{{ range==='daily' ? t('admin.last30Days') : t('admin.last12Months') }}</div>
          <div class="admin-stat-card__hint rtl-text">{{ t('admin.cards.liveSnapshot') }}</div>
        </div>
      </div>
    </section>

    <section class="grid grid-cols-1 gap-4 xl:grid-cols-[1.25fr_.75fr]">
      <div class="admin-panel">
        <div class="flex items-center justify-between gap-3 mb-5">
          <div>
            <div class="text-lg font-black rtl-text">{{ t('admin.activityTitle') }}</div>
            <div class="mt-1 text-sm admin-muted rtl-text">{{ range==='daily' ? t('admin.last30Days') : t('admin.last12Months') }}</div>
          </div>
          <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface-2 px-3 py-2 text-xs font-bold rtl-text">
            <span class="h-2.5 w-2.5 rounded-full bg-[rgb(var(--primary))]" />
            {{ t('admin.cards.liveSnapshot') }}
          </div>
        </div>

        <div v-if="activityBars.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="bars">
          <div v-for="b in activityBars" :key="b.key" class="bar">
            <div class="bar-fill" :style="{ height: b.h + '%' }"></div>
            <div class="bar-label keep-ltr">{{ b.label }}</div>
          </div>
        </div>

        <div class="mt-4 grid grid-cols-1 gap-3 md:grid-cols-3">
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">{{ t('admin.colOrders') }}</div>
            <div class="font-black keep-ltr">{{ activityTotals.orders }}</div>
          </div>
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">{{ t('admin.newUsers') }}</div>
            <div class="font-black keep-ltr">{{ activityTotals.users }}</div>
          </div>
          <div class="mini-stat">
            <div class="admin-muted text-xs rtl-text">{{ t('admin.colVisits') }}</div>
            <div class="font-black keep-ltr">{{ activityTotals.visits }}</div>
          </div>
        </div>
      </div>

      <div class="admin-panel">
        <div class="font-black rtl-text mb-4">{{ t('admin.summary.title') }}</div>

        <div class="space-y-3">
          <div class="topbox">
            <div class="topbox-title rtl-text">🔥 {{ t('admin.summary.mostPurchased') }}</div>
            <div v-if="overview.topPurchased.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="grid gap-2">
              <div v-for="x in overview.topPurchased.slice(0,5)" :key="x.productId" class="toprow">
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black">{{ x.purchases }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">👁️ {{ t('admin.summary.mostViewed') }}</div>
            <div v-if="overview.topViews.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="grid gap-2">
              <div v-for="x in overview.topViews.slice(0,5)" :key="x.productId" class="toprow">
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black">{{ x.views }}</div>
              </div>
            </div>
          </div>

          <div class="topbox">
            <div class="topbox-title rtl-text">❤️ {{ t('admin.summary.mostFavorited') }}</div>
            <div v-if="overview.topFavorites.length===0" class="admin-muted rtl-text">—</div>
            <div v-else class="grid gap-2">
              <div v-for="x in overview.topFavorites.slice(0,5)" :key="x.productId" class="toprow">
                <div class="rtl-text truncate font-bold">{{ x.title }}</div>
                <div class="keep-ltr font-black">{{ x.favorites }}</div>
              </div>
            </div>
          </div>

          <NuxtLink class="admin-link rtl-text" to="/admin/insights">{{ t('admin.viewInsightsDetails') }} →</NuxtLink>
        </div>
      </div>
    </section>

    <section class="grid grid-cols-1 gap-4 xl:grid-cols-[1.25fr_.75fr]">
      <div class="admin-panel">
        <div class="flex items-center justify-between gap-3 mb-4">
          <div class="font-black rtl-text">{{ t('admin.latestOrders') }}</div>
          <NuxtLink class="admin-link rtl-text" to="/admin/orders">{{ t('admin.viewAll') }}</NuxtLink>
        </div>

        <div v-if="latestOrders.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="orders">
          <div v-for="o in latestOrders" :key="o.id" class="order-row">
            <div class="min-w-0">
              <div class="flex items-center gap-2">
                <div class="order-badge" :class="badgeClass(o.status)">{{ o.status }}</div>
                <div class="rtl-text font-extrabold truncate">{{ o.primaryItemTitle || '—' }}</div>
              </div>
              <div class="admin-muted text-xs rtl-text mt-1 truncate">{{ o.userFullName || o.userEmail || '—' }} · {{ formatDate(o.createdAt) }}</div>
            </div>
            <div class="keep-ltr font-black">{{ formatMoney(o.totalIqd) }}</div>
          </div>
        </div>
      </div>

      <div class="admin-panel">
        <div class="font-black rtl-text mb-4">{{ t('admin.adminShortcuts') }}</div>
        <div class="grid gap-3">
          <NuxtLink class="admin-action" to="/admin/products">
            <div class="font-black rtl-text">{{ t('admin.manageProducts') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ t('admin.manageProductsHint') }}</div>
          </NuxtLink>
          <NuxtLink class="admin-action" to="/admin/orders">
            <div class="font-black rtl-text">{{ t('admin.manageOrders') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ t('admin.manageOrdersHint') }}</div>
          </NuxtLink>
          <NuxtLink class="admin-action" to="/admin/brands">
            <div class="font-black rtl-text">{{ t('admin.manageBrands') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ t('admin.manageBrandsHint') }}</div>
          </NuxtLink>
          <NuxtLink class="admin-action" to="/admin/insights">
            <div class="font-black rtl-text">{{ t('admin.insightsTitle') }}</div>
            <div class="admin-muted text-sm rtl-text">{{ t('admin.insightsSubtitle') }}</div>
          </NuxtLink>
        </div>
      </div>
    </section>

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
const stats = ref({ totalOrders: 0, totalUsers: 0, totalRevenueIqd: 0 })
const visits = ref({ total: 0, today: 0, month: 0 })
const overview = ref({ topPurchased: [] as any[], topFavorites: [] as any[], topViews: [] as any[], neglected: [] as any[] })
const activity = ref({ daily: [] as any[], monthly: [] as any[] })
const latestOrders = ref<any[]>([])

function extractErr(e: any) { return e?.data?.message || e?.message || t('common.requestFailed') }
function formatMoney(v: any) { return formatIqd(v) }
function formatDate(v: any) {
  if (!v) return '—'
  try { const d = new Date(v); return d.toLocaleString('en-GB', { year:'numeric', month:'2-digit', day:'2-digit', hour:'2-digit', minute:'2-digit' }) } catch { return String(v) }
}
const lastUpdatedLabel = computed(() => !lastUpdatedAt.value ? '—' : lastUpdatedAt.value.toLocaleTimeString('en-GB', { hour:'2-digit', minute:'2-digit' }))
const activitySeries = computed(() => range.value === 'daily' ? activity.value.daily : activity.value.monthly)
const activityTotals = computed(() => {
  const s = activitySeries.value || []
  const sum = (k: string) => s.reduce((a: number, x: any) => a + Number(x?.[k] ?? 0), 0)
  return { orders: sum('orders'), users: sum('users'), visits: sum('visits') }
})
const activityBars = computed(() => {
  const s = activitySeries.value || []
  if (s.length === 0) return []
  const maxV = Math.max(1, ...s.map((x: any) => Number(x?.visits ?? 0)))
  return s.map((x: any, idx: number) => ({ key: String(x?.key ?? idx), label: String(x?.label ?? x?.key ?? idx+1), h: Math.round((Number(x?.visits ?? 0) / maxV) * 100) }))
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
    stats.value.totalOrders = Number(dash?.totalOrders ?? 0)
    stats.value.totalUsers = Number(dash?.totalUsers ?? 0)
    stats.value.totalRevenueIqd = Number(dash?.totalRevenueIqd ?? 0)
    overview.value.topPurchased = Array.isArray(ov?.topPurchased) ? ov.topPurchased : []
    overview.value.topFavorites = Array.isArray(ov?.topFavorites) ? ov.topFavorites : []
    overview.value.topViews = Array.isArray(ov?.topViews) ? ov.topViews : []
    overview.value.neglected = Array.isArray(ov?.neglected) ? ov.neglected : []
    activity.value.daily = Array.isArray(act?.daily) ? act.daily : []
    activity.value.monthly = Array.isArray(act?.monthly) ? act.monthly : []
    visits.value.total = Number(vis?.total ?? 0)
    visits.value.today = Number(vis?.today ?? 0)
    visits.value.month = Number(vis?.month ?? 0)
    const list = Array.isArray(orders) ? orders : (Array.isArray(orders?.items) ? orders.items : [])
    latestOrders.value = list.slice().sort((a:any,b:any) => new Date(b?.createdAt || 0).getTime() - new Date(a?.createdAt || 0).getTime()).slice(0, 6).map((o:any) => ({
      id: o?.id || o?.Id,
      status: o?.status || o?.Status || '—',
      totalIqd: o?.totalIqd ?? o?.TotalIqd ?? 0,
      createdAt: o?.createdAt || o?.CreatedAt,
      userEmail: o?.userEmail || o?.User?.Email,
      userFullName: o?.userFullName || o?.User?.FullName,
      primaryItemTitle: o?.primaryItemTitle || '—',
    }))
    lastUpdatedAt.value = new Date()
  } catch (e:any) { error.value = extractErr(e) } finally { loading.value = false }
}
watch(range, () => {})
loadAll()
</script>

<style scoped>
.admin-hero,
.admin-panel,
.admin-stat-card,
.topbox,
.mini-stat,
.order-row,
.admin-action{
  border: 1px solid rgba(var(--border), .96);
}
.admin-hero,
.admin-panel{
  border-radius: 28px;
  padding: 20px;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90));
  box-shadow: 0 18px 50px rgba(0,0,0,.12);
}
.admin-kicker{ font-size: 12px; font-weight: 900; letter-spacing: .16em; text-transform: uppercase; color: rgb(var(--primary)); }
.admin-muted{ color: rgb(var(--muted)); }
.admin-chip{ border-radius: 999px; border: 1px solid rgba(var(--border), .96); padding: 10px 14px; font-weight: 900; font-size: 12px; background: transparent; }
.admin-chip.is-active{ background: rgb(var(--primary)); color: #111; border-color: rgba(var(--primary), .3); }
.admin-ghost{ padding: 11px 14px; border-radius: 16px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); color: rgb(var(--text)); font-weight: 800; }
.admin-stat-card{ border-radius: 24px; padding: 18px; display:flex; gap:14px; align-items:center; background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90)); box-shadow: 0 18px 44px rgba(0,0,0,.10); }
.admin-stat-card__icon{ width: 52px; height: 52px; border-radius: 18px; display:grid; place-items:center; background: rgba(var(--primary), .14); border: 1px solid rgba(var(--primary), .24); }
.admin-stat-card__icon--violet{ background: rgba(124,58,237,.14); border-color: rgba(124,58,237,.24); }
.admin-stat-card__icon--mint{ background: rgba(16,185,129,.14); border-color: rgba(16,185,129,.24); }
.admin-stat-card__icon--amber{ background: rgba(245,158,11,.14); border-color: rgba(245,158,11,.24); }
.admin-stat-card__label{ font-size: 12px; color: rgb(var(--muted)); }
.admin-stat-card__value{ margin-top: 6px; font-size: 28px; font-weight: 900; }
.admin-stat-card__hint{ margin-top: 6px; font-size: 12px; color: rgb(var(--muted)); }
.bars{ height: 220px; display: grid; grid-auto-flow: column; grid-auto-columns: 1fr; gap: 10px; align-items: end; }
.bar{ position: relative; height: 100%; border-radius: 20px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .74); overflow: hidden; }
.bar-fill{ position: absolute; inset-inline: 0; bottom: 0; border-radius: 18px 18px 0 0; background: linear-gradient(180deg, rgba(var(--primary), .92), rgba(var(--primary), .48)); }
.bar-label{ position: absolute; inset-inline: 0; bottom: 10px; text-align: center; font-size: 11px; font-weight: 800; }
.mini-stat{ border-radius: 20px; padding: 16px; background: rgba(var(--surface-rgb), .72); }
.topbox{ border-radius: 22px; padding: 16px; background: rgba(var(--surface-rgb), .76); }
.topbox-title{ margin-bottom: 12px; font-weight: 900; }
.toprow{ display:flex; align-items:center; justify-content:space-between; gap: 12px; }
.admin-link{ color: rgb(var(--primary)); font-weight: 800; }
.orders{ display:grid; gap:12px; }
.order-row{ display:flex; align-items:center; justify-content:space-between; gap:14px; border-radius: 20px; padding: 14px 16px; background: rgba(var(--surface-rgb), .72); }
.order-badge{ padding:6px 10px; border-radius:999px; font-size:12px; font-weight:900; border:1px solid rgba(var(--border), .96); }
.order-badge.ok{ background: rgba(16,185,129,.14); border-color: rgba(16,185,129,.30); }
.order-badge.warn{ background: rgba(245,158,11,.14); border-color: rgba(245,158,11,.30); }
.order-badge.bad{ background: rgba(239,68,68,.14); border-color: rgba(239,68,68,.30); }
.admin-action{ display:block; border-radius: 22px; padding: 16px; background: rgba(var(--surface-rgb), .74); transition: transform .2s ease, box-shadow .2s ease, border-color .2s ease; }
.admin-action:hover{ transform: translateY(-2px); border-color: rgba(var(--primary), .28); box-shadow: 0 20px 44px rgba(0,0,0,.12); }
.admin-error{ border-radius: 18px; border: 1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); padding: 12px 14px; }
:global(html.theme-light) .admin-hero,
:global(html.theme-light) .admin-panel,
:global(html.theme-light) .admin-stat-card{ background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(252,245,249,.96)); box-shadow: 0 18px 44px rgba(236,72,153,.08), 0 8px 24px rgba(24,24,24,.04); }
:global(html.theme-light) .topbox,
:global(html.theme-light) .mini-stat,
:global(html.theme-light) .order-row,
:global(html.theme-light) .admin-action{ background: rgba(255,255,255,.92); }
</style>
