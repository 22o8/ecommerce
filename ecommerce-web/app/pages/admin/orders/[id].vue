<template>
  <div class="space-y-4">
    <div class="admin-box flex items-center justify-between gap-3">
      <div>
        <div class="text-xl font-extrabold rtl-text">{{ t('admin.orderDetails') }}</div>
        <div class="text-sm admin-muted keep-ltr">{{ id }}</div>
      </div>
      <NuxtLink to="/admin/orders" class="admin-ghost rtl-text">{{ t('common.back') }}</NuxtLink>
    </div>

    <div v-if="loading" class="admin-box admin-muted rtl-text">{{ t('common.loading') }}</div>

    <div v-else-if="!order" class="admin-box admin-muted rtl-text">{{ t('admin.orderNotFound') }}</div>
    <div v-else class="admin-box">
      <div class="grid gap-3 md:grid-cols-2">
        <div class="sub-box">
          <div class="label rtl-text">{{ t('admin.status') }}</div>
          <div class="font-extrabold">{{ order.status || order.state || '—' }}</div>
        </div>
        <div class="sub-box">
          <div class="label rtl-text">{{ t('admin.user') }}</div>
          <div class="font-extrabold">{{ order.userEmail || order.user?.email || '—' }}</div>
        </div>
      </div>

      <div class="mt-4 sub-box">
        <div class="label rtl-text">{{ t('admin.raw') }}</div>
        <pre class="text-xs whitespace-pre-wrap">{{ order }}</pre>
      </div>
    </div>

    <div v-if="error" class="admin-error">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { computed, ref, onMounted } from 'vue'
import { useRoute } from '#imports'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'

const { t } = useI18n()
const api = useApi()
const route = useRoute()
const id = computed(() => String(route.params.id))

const loading = ref(true)
const error = ref('')
const order = ref<any | null>(null)

async function load() {
  loading.value = true
  error.value = ''
  try {
    const res: any = await api.get('/admin/orders')
    const list = Array.isArray(res) ? res : (res.items || [])
    order.value = list.find((x: any) => x.id === id.value) || null
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('failedLoad')
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.admin-box{ border-radius: 20px; border: 1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); padding: 16px; }
.sub-box{ border-radius: 18px; border: 1px solid rgba(255,255,255,.10); background: rgba(0,0,0,.14); padding: 14px; }
.label{ font-size: 12px; letter-spacing: .08em; text-transform: uppercase; color: rgba(255,255,255,.65); margin-bottom: 6px; }
.admin-muted{ color: rgba(255,255,255,.65); }
.admin-ghost{ padding:10px 12px; border-radius:14px; border:1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); color: rgba(255,255,255,.85); font-weight:700; }
.admin-error{ border-radius:16px; border:1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); padding:12px 14px; }
</style>
