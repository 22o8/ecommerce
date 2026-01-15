<!-- app/pages/service-requests/index.vue -->
<script setup lang="ts">
definePageMeta({ middleware: ['auth'] })

import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'
import { useSiteMeta } from '~/composables/useSiteMeta'

const api = useApi()
const { t } = useI18n()

useSiteMeta({
  title: `${t('serviceRequests')} | Ecommerce`,
  description: 'Your service requests',
  path: '/service-requests',
})

const pending = ref(false)
const error = ref('')
const raw = ref<any>(null)

function normalizeList(res: any) {
  if (Array.isArray(res)) return res
  if (res?.items && Array.isArray(res.items)) return res.items
  return []
}

async function load() {
  pending.value = true
  error.value = ''
  try {
    raw.value = await api.get<any>('/ServiceRequests/my')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('requestFailed')
  } finally {
    pending.value = false
  }
}

const items = computed(() => normalizeList(raw.value))

onMounted(load)
</script>

<template>
  <div class="space-y-6">
    <div class="card p-8">
      <h1 class="h2">{{ t('serviceRequests') }}</h1>
      <p class="mt-2 muted">All your submitted requests</p>
    </div>

    <div v-if="pending" class="card p-8 muted">{{ t('loading') }}</div>
    <div v-else-if="error" class="card p-8" style="border-color: rgba(var(--danger), .35); background: rgba(var(--danger), .08);">
      {{ error }}
    </div>
    <div v-else class="card p-8">
      <div v-if="!items.length" class="muted">No requests yet.</div>
      <div v-else class="grid gap-3">
        <div v-for="r in items" :key="r.id || JSON.stringify(r)" class="soft p-4">
          <div class="flex items-center justify-between gap-3">
            <div class="font-extrabold">{{ r.title || r.serviceTitle || r.id || 'Request' }}</div>
            <div class="badge">{{ r.status || r.state || '—' }}</div>
          </div>
          <pre class="mt-3 whitespace-pre-wrap text-xs muted">{{ r }}</pre>
        </div>
      </div>
    </div>
  </div>
</template>
