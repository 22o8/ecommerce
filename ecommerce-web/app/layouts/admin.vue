<template>
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
          </div>
        </div>

        <div class="mt-3 admin-divider" />

        <div class="p-2">
          <div class="text-xs admin-muted">Quick actions</div>
          <div class="mt-2 grid grid-cols-1 gap-2">
            <NuxtLink to="/admin/products" class="btn-primary text-center">Add / Edit Products</NuxtLink>
          </div>
        </div>
      </aside>

      <!-- Content -->
      <main class="admin-card p-5 md:p-6">
        <slot />
      </main>
    </div>

    <footer class="py-10 text-center text-sm admin-muted">
      © {{ new Date().getFullYear() }} — Admin Dashboard
    </footer>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useUiStore } from '~/stores/ui'
import { useAuthStore } from '~/stores/auth'
import { useI18n } from '~/composables/useI18n'
const ui = useUiStore()
const auth = useAuthStore()
const { setLocale } = useI18n()
const route = useRoute()
const sidebarOpen = ref(false)

function isActive(path: string) {
  return route.path === path || (path !== '/admin' && route.path.startsWith(path))
}

function toggleTheme() {
  ui.toggleTheme()
}

function toggleLocale() {
  const next = ui.locale === 'en' ? 'ar' : 'en'
  setLocale(next)
  ui.setLocale(next) // dir ثابت ltr
}

async function logout() {
  await auth.logout()
}

onMounted(() => ui.initClient())
</script>

<style>
/* =========================================
   Admin tokens (يدعم data-theme و theme-dark)
   ========================================= */
:root{
  --primary: 99 102 241;   /* indigo */
  --success: 34 197 94;
  --danger: 239 68 68;

  /* LIGHT defaults */
  --adm-bg: 245 247 252;
  --adm-bg2: 238 242 255;

  --adm-panel: 255 255 255;
  --adm-panel-alpha: .88;

  --adm-border: 226 232 240;
  --adm-border-alpha: 1;

  --adm-text: 15 23 42;
  --adm-muted: 71 85 105;

  --adm-soft: 15 23 42 / .04;

  --adm-shadow: 0 18px 50px rgba(2,6,23,.08);
}

/* LIGHT overrides (إذا الثيم ينكتب بالطريقتين) */
html[data-theme="light"], html.theme-light{
  color-scheme: light;
}

/* DARK overrides (إذا الثيم ينكتب بالطريقتين) */
html[data-theme="dark"], html.theme-dark{
  color-scheme: dark;

  --adm-bg: 7 10 18;
  --adm-bg2: 10 14 26;

  /* الأهم: لوح/كارد غامق حتى ما يطلع أبيض */
  --adm-panel: 14 18 32;
  --adm-panel-alpha: .78;

  --adm-border: 255 255 255;
  --adm-border-alpha: .10;

  --adm-text: 236 244 255;
  --adm-muted: 164 180 205;

  --adm-soft: 255 255 255 / .07;

  --adm-shadow: 0 22px 80px rgba(0,0,0,.55);
}

/* =========================================
   Background
   ========================================= */
.bg-admin{
  background:
    radial-gradient(1100px 560px at 18% -10%, rgba(99,102,241,.22), transparent 58%),
    radial-gradient(900px 520px at 92% 8%, rgba(56,189,248,.12), transparent 55%),
    radial-gradient(900px 520px at 50% 120%, rgba(34,197,94,.10), transparent 55%),
    linear-gradient(to bottom, rgb(var(--adm-bg)), rgb(var(--adm-bg2)));
}

/* =========================================
   Card
   ========================================= */
.admin-card{
  border: 1px solid rgba(var(--adm-border), var(--adm-border-alpha));
  background: rgba(var(--adm-panel), var(--adm-panel-alpha));
  backdrop-filter: blur(14px);
  border-radius: 24px;
  box-shadow: var(--adm-shadow);
  color: rgb(var(--adm-text));
}

.admin-text{ color: rgb(var(--adm-text)) !important; }
.admin-muted{ color: rgb(var(--adm-muted)) !important; }

.admin-divider{
  height: 1px;
  background: rgba(var(--adm-border), var(--adm-border-alpha));
}

/* =========================================
   Buttons / pill
   ========================================= */
.icon-btn{
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(var(--adm-border), var(--adm-border-alpha));
  background: rgba(var(--adm-soft));
  color: rgb(var(--adm-text));
  transition: transform .12s ease, background .12s ease, border-color .12s ease, opacity .12s ease;
}
.icon-btn:hover{
  border-color: rgba(var(--primary), .35);
  background: rgba(var(--primary), .10);
}
.icon-btn:active{ transform: scale(.98); }

.admin-pill{
  border: 1px solid rgba(var(--adm-border), var(--adm-border-alpha));
  background: rgba(var(--adm-soft));
  padding: 8px 12px;
  border-radius: 999px;
}

/* =========================================
   Sidebar links
   ========================================= */
.side-link{
  display:block;
  padding: 10px 12px;
  border-radius: 14px;
  color: rgb(var(--adm-text));
  border: 1px solid transparent;
  transition: background .12s ease, border-color .12s ease;
}
.side-link:hover{
  background: rgba(var(--primary), .10);
  border-color: rgba(var(--primary), .22);
}
.side-active{
  background: rgba(var(--primary), .16);
  border-color: rgba(var(--primary), .30);
  font-weight: 900;
}

/* =========================================
   Action buttons
   ========================================= */
.btn-primary{
  padding: 10px 14px;
  border-radius: 14px;
  font-weight: 900;
  color: #fff;
  background: rgb(var(--primary));
  box-shadow: 0 10px 24px rgba(99,102,241,.22);
  transition: transform .12s ease, opacity .12s ease;
}
.btn-primary:hover{ opacity: .95; }
.btn-primary:active{ transform: scale(.99); }

.btn-soft{
  padding: 10px 14px;
  border-radius: 14px;
  font-weight: 900;
  border: 1px solid rgba(var(--adm-border), var(--adm-border-alpha));
  background: rgba(var(--adm-soft));
  color: rgb(var(--adm-text));
  transition: transform .12s ease, border-color .12s ease, background .12s ease;
}
.btn-soft:hover{
  border-color: rgba(var(--primary), .28);
  background: rgba(var(--primary), .08);
}
.btn-soft:active{ transform: scale(.99); }

.btn-danger{
  padding: 10px 14px;
  border-radius: 14px;
  font-weight: 900;
  color: #fff;
  background: rgb(var(--danger));
  box-shadow: 0 10px 24px rgba(239,68,68,.20);
  transition: transform .12s ease, opacity .12s ease;
}
.btn-danger:hover{ opacity: .95; }
.btn-danger:active{ transform: scale(.99); }
</style>
