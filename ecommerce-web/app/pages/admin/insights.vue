<template>
  <div class="space-y-4">
    <div class="admin-box flex items-center justify-between gap-3">
      <div>
        <div class="text-xl font-extrabold rtl-text">Analytics / Insights</div>
        <div class="text-sm admin-muted rtl-text">أكثر المنتجات (شراء/مفضلة/مشاهدة) + نشاط يومي/شهري</div>
      </div>
      <button class="admin-ghost" type="button" @click="loadAll" :disabled="loading">
        {{ t('common.refresh') }}
      </button>
    </div>

    <div v-if="loading" class="admin-box admin-muted rtl-text">{{ t('common.loading') }}</div>
    <div v-else class="grid gap-4 lg:grid-cols-2">
      <div class="admin-box">
        <div class="font-extrabold rtl-text mb-3">🔥 الأكثر شراءً</div>
        <div v-if="topPurchased.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in topPurchased" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black">{{ x.purchases }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="font-extrabold rtl-text mb-3">❤️ الأكثر مفضلة</div>
        <div v-if="topFavorites.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in topFavorites" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black">{{ x.favorites }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="font-extrabold rtl-text mb-3">👀 الأكثر مشاهدة</div>
        <div v-if="topViews.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in topViews" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr font-black">{{ x.views }}</div>
          </div>
        </div>
      </div>

      <div class="admin-box">
        <div class="font-extrabold rtl-text mb-3">💤 المنتجات المهملة</div>
        <div v-if="neglected.length===0" class="admin-muted rtl-text">—</div>
        <div v-else class="grid gap-2">
          <div v-for="x in neglected" :key="x.productId" class="flex items-center justify-between gap-3">
            <div class="rtl-text font-bold truncate">{{ x.title }}</div>
            <div class="keep-ltr text-xs text-muted">views: {{ x.views }} · fav: {{ x.favorites }} · buy: {{ x.purchases }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="admin-box">
      <div class="font-extrabold rtl-text mb-3">📊 النشاط (يومي / شهري)</div>

      <div class="grid gap-4 md:grid-cols-2">
        <div class="sub-box">
          <div class="label rtl-text mb-2">يومي (آخر 30 يوم)</div>
          <div class="grid gap-2">
            <div class="grid grid-cols-[120px_1fr_1fr_1fr] gap-2 text-xs admin-muted">
              <div class="keep-ltr">date</div>
              <div class="text-center rtl-text">طلبات</div>
              <div class="text-center rtl-text">مشاهدات</div>
              <div class="text-center rtl-text">مفضلة</div>
            </div>
            <div v-for="r in daily" :key="r.period" class="grid grid-cols-[120px_1fr_1fr_1fr] gap-2 items-center py-2 border-t border-app">
              <div class="keep-ltr text-xs">{{ r.period }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.orders }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.views }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.favorites }}</div>
            </div>
          </div>
        </div>

        <div class="sub-box">
          <div class="label rtl-text mb-2">شهري (آخر 12 شهر)</div>
          <div class="grid gap-2">
            <div class="grid grid-cols-[120px_1fr_1fr_1fr] gap-2 text-xs admin-muted">
              <div class="keep-ltr">month</div>
              <div class="text-center rtl-text">طلبات</div>
              <div class="text-center rtl-text">مشاهدات</div>
              <div class="text-center rtl-text">مفضلة</div>
            </div>
            <div v-for="r in monthly" :key="r.period" class="grid grid-cols-[120px_1fr_1fr_1fr] gap-2 items-center py-2 border-t border-app">
              <div class="keep-ltr text-xs">{{ r.period }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.orders }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.views }}</div>
              <div class="text-center keep-ltr font-bold">{{ r.favorites }}</div>
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
.admin-box{
  border-radius: 20px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface));
  padding: 16px;
}
.sub-box{
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  padding: 14px;
}
.label{ font-size: 12px; letter-spacing: .08em; text-transform: uppercase; color: rgb(var(--muted)); }
.admin-muted{ color: rgb(var(--muted)); }
.admin-ghost{
  padding: 10px 12px;
  border-radius: 14px;
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
</style>
