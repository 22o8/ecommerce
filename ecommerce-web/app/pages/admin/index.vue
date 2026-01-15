<!-- app/pages/admin/index.vue -->
<template>
  <div class="grid gap-4 md:grid-cols-3">
    <div class="admin-box">
      <div class="text-sm admin-muted">Total Products</div>
      <div class="text-3xl font-extrabold mt-2">{{ stats.products }}</div>
    </div>

    <div class="admin-box">
      <div class="text-sm admin-muted">Orders</div>
      <div class="text-3xl font-extrabold mt-2">{{ stats.orders }}</div>
    </div>

    <div class="admin-box">
      <div class="text-sm admin-muted">Services</div>
      <div class="text-3xl font-extrabold mt-2">{{ stats.services }}</div>
    </div>

    <div class="md:col-span-3 admin-box">
      <div class="flex items-center justify-between gap-2">
        <div>
          <div class="font-extrabold">Quick Actions</div>
          <div class="text-sm admin-muted">Create and manage content quickly</div>
        </div>

        <div class="flex gap-2">
          <NuxtLink to="/admin/products/new" class="admin-primary">+ New Product</NuxtLink>
          <NuxtLink to="/admin/services" class="admin-ghost">Manage Services</NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin' })

const stats = {
  products: 0,
  orders: 0,
  services: 0,
}
</script>

<style scoped>
.admin-box{
  border-radius: 20px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  padding: 16px;
}
.admin-muted{ color: rgba(255,255,255,.65); }

.admin-primary{
  padding: 10px 12px;
  border-radius: 14px;
  background: rgba(99,102,241,.22);
  border: 1px solid rgba(99,102,241,.35);
  color: white;
  font-weight: 700;
}
.admin-ghost{
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.85);
}
</style>
