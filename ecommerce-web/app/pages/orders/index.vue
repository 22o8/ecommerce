<template>
  <div class="grid gap-6">
    <div class="flex items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ t('myOrders') }}</h1>
        <p class="text-muted rtl-text">{{ t('ordersSubtitle') }}</p>
      </div>
      <NuxtLink to="/products">
        <UiButton variant="secondary">
          <Icon name="mdi:shopping-search-outline" class="text-lg" />
          <span class="rtl-text">{{ t('browseProducts') }}</span>
        </UiButton>
      </NuxtLink>
    </div>

    <div v-if="pending" class="card-soft p-6">
      <div class="skeleton h-6 w-1/3" />
      <div class="mt-4 grid gap-3">
        <div v-for="i in 4" :key="i" class="skeleton h-16" />
      </div>
    </div>

    <div v-else-if="error" class="card-soft p-6 border" :style="{ borderColor: 'rgba(var(--danger),.35)', background: 'rgba(var(--danger),.08)' }">
      <div class="font-bold rtl-text">{{ t('error') }}</div>
      <div class="text-sm rtl-text mt-1">{{ error }}</div>
    </div>

    <div v-else class="card-soft p-6">
      <div v-if="items.length === 0" class="text-muted rtl-text">{{ t('ordersEmpty') }}</div>

      <div v-else class="grid gap-3">
        <NuxtLink
          v-for="o in items"
          :key="o.id"
          :to="`/orders/${o.id}`"
          class="rounded-3xl border border-app bg-surface p-4 hover:bg-surface-2 transition"
        >
          <div class="flex items-center justify-between gap-3">
            <div class="grid gap-1">
              <div class="font-extrabold keep-ltr">{{ o.id }}</div>
              <div class="text-sm text-muted rtl-text">{{ o.status || o.state || '—' }}</div>
            </div>
            <div class="inline-flex items-center gap-2 text-sm text-muted">
              <span class="rtl-text">{{ t('open') }}</span>
              <Icon name="mdi:arrow-right" class="keep-ltr" />
            </div>
          </div>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: ['auth'] })
import UiButton from '~/components/ui/UiButton.vue'
const { t } = useI18n()
const api = useApi()
const pending = ref(false)
const error = ref('')
const items = ref<any[]>([])

async function load() {
  pending.value = true
  error.value = ''
  try {
    const res: any = await api.get('/Orders/my')
    items.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('failedLoad')
  } finally {
    pending.value = false
  }
}
onMounted(load)
</script>
