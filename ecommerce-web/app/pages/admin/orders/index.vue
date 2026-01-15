<template>
  <div class="space-y-4">
    <div class="admin-box">
      <div class="text-xl font-extrabold">Orders</div>
      <div class="text-sm admin-muted">View and manage orders</div>
    </div>

    <div class="admin-box overflow-hidden">
      <div class="admin-table">
        <div class="admin-tr admin-th">
          <div>ID</div>
          <div>Status</div>
          <div>User</div>
          <div class="text-right">Actions</div>
        </div>

        <div v-if="pending" class="p-4 admin-muted">Loading...</div>
        <div v-else-if="items.length === 0" class="p-4 admin-muted">No orders yet.</div>
        <div v-else>
          <div v-for="o in items" :key="o.id" class="admin-tr">
            <div class="font-semibold">{{ o.id }}</div>
            <div class="admin-muted text-sm">{{ o.status || o.state || '—' }}</div>
            <div class="admin-muted text-sm">{{ o.userEmail || o.user?.email || '—' }}</div>
            <div class="flex justify-end">
              <NuxtLink :to="`/admin/orders/${o.id}`" class="admin-pill">Details</NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="error" class="admin-error">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

const api = useAdminApi()
const pending = ref(false)
const error = ref('')
const items = ref<any[]>([])

async function load() {
  pending.value = true
  error.value = ''
  try {
    // swagger: GET /api/admin/orders (no params)
    const res: any = await api.get('/admin/orders')
    items.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed to load orders'
  } finally {
    pending.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.admin-box{ border-radius: 20px; border: 1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); padding: 16px; }
.admin-muted{ color: rgba(255,255,255,.65); }
.admin-pill{ padding:8px 10px; border-radius:14px; border:1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); color: rgba(255,255,255,.9); }
.admin-table{ display:grid; }
.admin-tr{ display:grid; grid-template-columns: 2fr 1fr 2fr 1fr; gap:12px; padding:12px 16px; border-top:1px solid rgba(255,255,255,.08); }
.admin-th{ border-top:none; background: rgba(0,0,0,.18); font-size:12px; text-transform:uppercase; letter-spacing:.08em; color: rgba(255,255,255,.65); }
.admin-error{ border-radius:16px; border:1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); padding:12px 14px; }
</style>
