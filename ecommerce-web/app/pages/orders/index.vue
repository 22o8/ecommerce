<!-- app/pages/orders/index.vue -->
<template>
  <div class="space-y-6">
    <div class="card p-8">
      <h1 class="h2">My Orders</h1>
      <p class="mt-2 muted">All your orders and downloads</p>
    </div>

    <div v-if="pending" class="card p-8 muted">Loading...</div>
    <div v-else-if="error" class="card p-8" style="border-color: rgba(var(--danger), .35); background: rgba(var(--danger), .08);">
      {{ error }}
    </div>
    <div v-else class="card p-8">
      <div v-if="items.length === 0" class="muted">No orders yet.</div>
      <div v-else class="grid gap-3">
        <NuxtLink
          v-for="o in items"
          :key="o.id"
          :to="`/orders/${o.id}`"
          class="soft p-4 hover:opacity-95 transition"
        >
          <div class="flex items-center justify-between gap-3">
            <div>
              <div class="font-extrabold">{{ o.id }}</div>
              <div class="text-sm muted mt-1">{{ o.status || o.state || '—' }}</div>
            </div>
            <div class="badge">Open →</div>
          </div>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: ['auth'] })

const api = useApi()
const pending = ref(false)
const error = ref('')
const items = ref<any[]>([])

async function load() {
  pending.value = true
  error.value = ''
  try {
    // swagger: GET /api/Orders/my
    const res: any = await api.get('/Orders/my')
    items.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed to load orders'
  } finally {
    pending.value = false
  }
}

onMounted(load)
</script>
