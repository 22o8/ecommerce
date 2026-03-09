<template>
  <div class="space-y-5 insights-page">
    <section class="insights-hero">
      <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <div class="insights-kicker rtl-text">{{ t('admin.insightsTitle') }}</div>
          <div class="mt-2 text-2xl font-black rtl-text sm:text-3xl">{{ t('admin.insightsHeadline') }}</div>
          <div class="mt-2 text-sm insights-muted rtl-text">{{ t('admin.insightsSubtitle') }}</div>
        </div>
        <button class="insights-refresh" type="button" @click="loadAll" :disabled="loading">{{ loading ? t('common.loading') : t('common.refresh') }}</button>
      </div>
    </section>

    <div v-if="loading" class="insights-panel insights-muted rtl-text">{{ t('common.loading') }}</div>

    <template v-else>
      <section class="grid gap-4 lg:grid-cols-2 xl:grid-cols-4">
        <div class="insights-stat">
          <div class="insights-stat__label rtl-text">{{ t('admin.topPurchased') }}</div>
          <div class="insights-stat__value keep-ltr">{{ topPurchased.length }}</div>
        </div>
        <div class="insights-stat">
          <div class="insights-stat__label rtl-text">{{ t('admin.topFavorited') }}</div>
          <div class="insights-stat__value keep-ltr">{{ topFavorites.length }}</div>
        </div>
        <div class="insights-stat">
          <div class="insights-stat__label rtl-text">{{ t('admin.topViewed') }}</div>
          <div class="insights-stat__value keep-ltr">{{ topViews.length }}</div>
        </div>
        <div class="insights-stat">
          <div class="insights-stat__label rtl-text">{{ t('admin.neglectedProducts') }}</div>
          <div class="insights-stat__value keep-ltr">{{ neglected.length }}</div>
        </div>
      </section>

      <section class="grid gap-4 xl:grid-cols-2">
        <div class="insights-panel">
          <div class="insights-panel__title rtl-text">🔥 {{ t('admin.topPurchased') }}</div>
          <div v-if="topPurchased.length===0" class="insights-muted rtl-text">—</div>
          <div v-else class="insights-list">
            <div v-for="x in topPurchased" :key="x.productId" class="insights-row">
              <div class="rtl-text font-black truncate">{{ x.title }}</div>
              <div class="keep-ltr font-black">{{ x.purchases }}</div>
            </div>
          </div>
        </div>

        <div class="insights-panel">
          <div class="insights-panel__title rtl-text">❤️ {{ t('admin.topFavorited') }}</div>
          <div v-if="topFavorites.length===0" class="insights-muted rtl-text">—</div>
          <div v-else class="insights-list">
            <div v-for="x in topFavorites" :key="x.productId" class="insights-row">
              <div class="rtl-text font-black truncate">{{ x.title }}</div>
              <div class="keep-ltr font-black">{{ x.favorites }}</div>
            </div>
          </div>
        </div>

        <div class="insights-panel">
          <div class="insights-panel__title rtl-text">👁️ {{ t('admin.topViewed') }}</div>
          <div v-if="topViews.length===0" class="insights-muted rtl-text">—</div>
          <div v-else class="insights-list">
            <div v-for="x in topViews" :key="x.productId" class="insights-row">
              <div class="rtl-text font-black truncate">{{ x.title }}</div>
              <div class="keep-ltr font-black">{{ x.views }}</div>
            </div>
          </div>
        </div>

        <div class="insights-panel">
          <div class="insights-panel__title rtl-text">💤 {{ t('admin.neglectedProducts') }}</div>
          <div v-if="neglected.length===0" class="insights-muted rtl-text">—</div>
          <div v-else class="insights-list">
            <div v-for="x in neglected" :key="x.productId" class="insights-row insights-row--stacked">
              <div class="rtl-text font-black truncate">{{ x.title }}</div>
              <div class="keep-ltr text-xs insights-muted">views: {{ x.views }} · fav: {{ x.favorites }} · buy: {{ x.purchases }}</div>
            </div>
          </div>
        </div>
      </section>

      <section class="insights-panel">
        <div class="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between mb-4">
          <div>
            <div class="insights-panel__title rtl-text">{{ t('admin.activityTitle') }}</div>
            <div class="mt-1 text-sm insights-muted rtl-text">{{ t('admin.activitySubtitle') }}</div>
          </div>
        </div>

        <div class="grid gap-4 xl:grid-cols-2">
          <div class="table-shell">
            <div class="table-title rtl-text">{{ t('admin.dailyTableTitle') }}</div>
            <div class="table-scroll">
              <table class="w-full table-fixed">
                <thead>
                  <tr class="table-head">
                    <th class="text-start keep-ltr">{{ t('admin.colDate') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colOrders') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colViews') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colFavorites') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colVisits') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="r in daily" :key="r.period" class="table-row">
                    <td class="keep-ltr text-xs">{{ r.period }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.orders }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.views }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.favorites }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.visits }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div class="table-shell">
            <div class="table-title rtl-text">{{ t('admin.monthlyTableTitle') }}</div>
            <div class="table-scroll">
              <table class="w-full table-fixed">
                <thead>
                  <tr class="table-head">
                    <th class="text-start keep-ltr">{{ t('admin.colMonth') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colOrders') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colViews') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colFavorites') }}</th>
                    <th class="text-center rtl-text">{{ t('admin.colVisits') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="r in monthly" :key="r.period" class="table-row">
                    <td class="keep-ltr text-xs">{{ r.period }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.orders }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.views }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.favorites }}</td>
                    <td class="text-center keep-ltr font-black">{{ r.visits }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div v-if="error" class="admin-error rtl-text mt-4">{{ error }}</div>
      </section>
    </template>
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
function extractErr(e: any) { return e?.data?.message || e?.message || t('common.requestFailed') }
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
  } catch (e:any) { error.value = extractErr(e) }
  finally { loading.value = false }
}
loadAll()
</script>

<style scoped>
.insights-hero,
.insights-panel,
.insights-stat,
.table-shell,
.insights-row{
  border: 1px solid rgba(var(--border), .96);
}
.insights-hero,
.insights-panel{
  border-radius: 28px;
  padding: 20px;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90));
  box-shadow: 0 18px 48px rgba(0,0,0,.12);
}
.insights-kicker{ font-size: 12px; font-weight: 900; letter-spacing: .16em; text-transform: uppercase; color: rgb(var(--primary)); }
.insights-refresh{ border-radius: 16px; padding: 11px 14px; font-weight: 800; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .9); }
.insights-stat{ border-radius: 22px; padding: 18px; background: rgba(var(--surface-rgb), .82); }
.insights-stat__label{ font-size: 12px; color: rgb(var(--muted)); }
.insights-stat__value{ margin-top: 8px; font-size: 30px; font-weight: 900; }
.insights-panel__title{ font-size: 18px; font-weight: 900; margin-bottom: 14px; }
.insights-muted{ color: rgb(var(--muted)); }
.insights-list{ display:grid; gap:10px; }
.insights-row{ display:flex; align-items:center; justify-content:space-between; gap:12px; border-radius: 18px; padding: 12px 14px; background: rgba(var(--surface-rgb), .72); }
.insights-row--stacked{ flex-direction: column; align-items: flex-start; }
.table-shell{ border-radius: 24px; padding: 14px; background: rgba(var(--surface-rgb), .72); }
.table-title{ font-size: 14px; font-weight: 900; margin-bottom: 10px; }
.table-scroll{ overflow:auto; }
.table-head th{ padding: 10px 8px; font-size: 11px; color: rgb(var(--muted)); text-transform: uppercase; }
.table-row td{ padding: 12px 8px; border-top: 1px solid rgba(var(--border), .96); }
.admin-error{ border-radius: 16px; border: 1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); padding: 12px 14px; }
:global(html.theme-light) .insights-hero,
:global(html.theme-light) .insights-panel,
:global(html.theme-light) .insights-stat{
  background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(252,245,249,.96));
  box-shadow: 0 18px 44px rgba(236,72,153,.08), 0 8px 24px rgba(24,24,24,.04);
}
:global(html.theme-light) .table-shell,
:global(html.theme-light) .insights-row{ background: rgba(255,255,255,.92); }
</style>
