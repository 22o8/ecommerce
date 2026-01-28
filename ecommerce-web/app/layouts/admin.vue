<template>
  <div class="min-h-screen bg-app">
    <div class="mx-auto max-w-7xl px-3 sm:px-4 py-4 sm:py-6 grid gap-4 sm:gap-6 lg:grid-cols-[280px_1fr]">
      <aside class="card-soft p-3 sm:p-4 h-fit lg:sticky lg:top-4">
        <div class="flex items-center gap-3">
          <div
            class="h-10 w-10 rounded-2xl bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] grid place-items-center font-black"
          >
            <Icon name="mdi:view-dashboard-outline" class="text-xl" />
          </div>
          <div>
            <div class="font-extrabold rtl-text">{{ t('dashboard') }}</div>
            <div class="text-xs text-muted keep-ltr">{{ userLabel }}</div>
          </div>
        </div>

        <div class="mt-4 grad-line" />

        <nav class="mt-4 grid grid-cols-2 gap-2 text-sm sm:grid-cols-1">
          <NuxtLink to="/admin" class="navItem" :class="{ active: route.path === '/admin' }">
            <Icon name="mdi:chart-box-outline" class="text-lg" />
            <span class="rtl-text">{{ t('admin.overview') }}</span>
          </NuxtLink>

          <NuxtLink
            to="/admin/products"
            class="navItem"
            :class="{ active: route.path.startsWith('/admin/products') }"
          >
            <Icon name="mdi:package-variant-closed" class="text-lg" />
            <span class="rtl-text">{{ t('admin.products') }}</span>
          </NuxtLink>

          <NuxtLink
            to="/admin/orders"
            class="navItem"
            :class="{ active: route.path.startsWith('/admin/orders') }"
          >
            <Icon name="mdi:receipt-text-outline" class="text-lg" />
            <span class="rtl-text">{{ t('admin.orders') }}</span>
          </NuxtLink>
        </nav>

        <div class="mt-4 grad-line" />

        <div class="mt-4 grid gap-2">
          <UiButton variant="secondary" @click="goHome">
            <Icon name="mdi:arrow-left" class="keep-ltr" />
            <span class="rtl-text">{{ t('nav.home') }}</span>
          </UiButton>
        </div>
      </aside>

      <section class="grid gap-6">
        <slot />
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'

definePageMeta({ middleware: ['auth', 'admin'] })

const route = useRoute() // ✅ هذا هو الإصلاح الرئيسي
const auth = useAuthStore()
const { t } = useI18n()

const userLabel = computed(() => auth.user?.email || 'admin')

const router = useRouter()
function goHome() {
  router.push('/')
}
</script>

<style scoped>
.navItem {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 14px;
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface));
  transition: 0.15s ease;
}

@media (max-width: 640px) {
  .navItem {
    padding: 10px 12px;
    border-radius: 14px;
    gap: 8px;
  }
}
.navItem:hover {
  background: rgb(var(--surface-2));
}
.router-link-active {
  box-shadow: 0 0 0 4px rgba(var(--ring), 0.18);
  border-color: rgba(var(--ring), 0.55);
}
</style>
