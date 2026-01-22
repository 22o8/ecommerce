<template>
<<<<<<< HEAD
  <div class="min-h-screen bg-admin">
    <!-- Topbar -->
    <header class="sticky top-0 z-50">
      <div class="mx-auto max-w-7xl px-4 pt-4">
        <div class="admin-card px-4 py-3 flex items-center justify-between gap-4">
          <div class="flex items-center gap-3">
            <NuxtLink to="/admin" class="flex items-center gap-3">
              <div
                class="h-10 w-10 rounded-2xl flex items-center justify-center font-black text-white"
                :style="{ background: `rgb(var(--primary))` }"
              >
                A
              </div>
              <div class="leading-tight">
                <div class="font-extrabold tracking-wide admin-text">Admin Panel</div>
                <div class="text-xs admin-muted -mt-0.5">Manage products & orders</div>
              </div>
            </NuxtLink>
          </div>

          <div class="flex items-center gap-2">
            <div class="hidden sm:flex items-center gap-2 admin-pill">
              <span class="admin-muted text-xs">Signed in</span>
              <span class="text-sm font-semibold admin-text">{{ auth.user?.fullName || 'Admin' }}</span>
            </div>

            <button class="icon-btn" type="button" @click="toggleTheme" title="Theme">
              {{ ui.theme === 'dark' ? '🌙' : '☀️' }}
            </button>

            <button class="icon-btn" type="button" @click="toggleLocale" title="Lang">
              {{ ui.locale.toUpperCase() }}
            </button>

            <button class="btn-danger" type="button" @click="logout">
              Logout
            </button>
          </div>
        </div>
      </div>
    </header>

    <!-- Body -->
    <div class="mx-auto max-w-7xl px-4 py-6 grid grid-cols-1 lg:grid-cols-[280px_1fr] gap-6">
      <!-- Sidebar -->
      <aside class="admin-card p-3 lg:sticky lg:top-[92px] h-fit" :class="sidebarOpen ? '' : 'hidden lg:block'">
        <div class="p-2">
          <div class="text-xs admin-muted">Navigation</div>
          <div class="mt-2 space-y-1">
            <NuxtLink to="/admin" class="side-link" :class="isActive('/admin') ? 'side-active' : ''">
              Overview
            </NuxtLink>
            <NuxtLink to="/admin/products" class="side-link" :class="isActive('/admin/products') ? 'side-active' : ''">
              Products
            </NuxtLink>
            <NuxtLink to="/admin/orders" class="side-link" :class="isActive('/admin/orders') ? 'side-active' : ''">
              Orders
            </NuxtLink>
            <NuxtLink
              to="/admin/commands"
              class="nav-item"
              >
               Commands
              </NuxtLink>
=======
  <div class="min-h-screen bg-app">
    <div class="mx-auto max-w-7xl px-4 py-6 grid gap-6 lg:grid-cols-[280px_1fr]">
      <aside class="card-soft p-4 h-fit sticky top-4">
        <div class="flex items-center gap-3">
          <div class="h-10 w-10 rounded-2xl bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] grid place-items-center font-black">
            <Icon name="mdi:view-dashboard-outline" class="text-xl" />
          </div>
          <div>
            <div class="font-extrabold rtl-text">{{ t('dashboard') }}</div>
            <div class="text-xs text-muted keep-ltr">{{ userLabel }}</div>
>>>>>>> 55e014a (Initial commit)
          </div>
        </div>

        <div class="mt-4 grad-line" />

        <nav class="mt-4 grid gap-2 text-sm">
          <NuxtLink to="/admin" class="navItem">
            <Icon name="mdi:chart-box-outline" class="text-lg" />
            <span class="rtl-text">{{ t('admin.overview') }}</span>
          </NuxtLink>

          <NuxtLink to="/admin/products" class="navItem">
            <Icon name="mdi:shopping-outline" class="text-lg" />
            <span class="rtl-text">{{ t('admin.products') }}</span>
          </NuxtLink>

          <NuxtLink to="/admin/orders" class="navItem">
            <Icon name="mdi:receipt-text-outline" class="text-lg" />
            <span class="rtl-text">{{ t('admin.orders') }}</span>
          </NuxtLink>

          <!-- ✅ تم حذف صفحة (موجهة أوامر / commands) بالكامل حسب طلبك -->
        </nav>

        <div class="mt-4 grad-line" />

        <div class="mt-4 grid gap-2">
          <UiButton variant="secondary" @click="goHome">
            <Icon name="mdi:arrow-left" class="keep-ltr" />
            <span class="rtl-text">{{ t('home') }}</span>
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
const auth = useAuthStore()
const { t } = useI18n()
const userLabel = computed(() => auth.user?.email || 'admin')
const router = useRouter()
function goHome(){ router.push('/') }
</script>

<style scoped>
.navItem{
  display:flex; align-items:center; gap:10px;
  padding: 12px 14px;
  border-radius: 16px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface));
  transition: .15s ease;
}
.navItem:hover{ background: rgb(var(--surface-2)); }
.router-link-active{ box-shadow: 0 0 0 4px rgba(var(--ring), .18); border-color: rgba(var(--ring), .55); }
</style>
