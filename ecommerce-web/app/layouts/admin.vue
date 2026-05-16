<template>
  <div class="admin-layout-root min-h-screen w-full bg-app text-fg overflow-x-hidden">
    <!-- Header -->
    <header
      class="admin-topbar sticky top-0 z-40 border-b border-app bg-app/85 backdrop-blur supports-[backdrop-filter]:bg-app/70"
      :style="safeAreaTopStyle"
    >
      <div class="admin-content-wrap px-3 sm:px-4 py-3 flex items-center gap-2">
        <!-- Menu button (mobile) -->
        <button
          class="md:hidden inline-flex h-10 w-10 shrink-0 items-center justify-center
                 rounded-2xl border border-app bg-surface active:scale-[0.98] transition"
          @click="toggle()"
          aria-label="Menu"
        >
          <Icon name="mdi:menu" class="text-xl" />
        </button>

        <!-- Title + email -->
        <div class="min-w-0">
          <div class="font-extrabold truncate">
            {{ t('admin.title') }}
          </div>
          <div class="text-xs text-muted keep-ltr truncate">
            {{ auth.user?.email || '' }}
          </div>
        </div>

        <div class="flex-1" />

        <!-- Right actions -->
        <div class="flex items-center gap-1 sm:gap-2 shrink-0">
          <UiButton
            variant="ghost"
            class="px-2"
            @click="ui.toggleTheme()"
            :title="ui.theme === 'dark' ? t('theme.dark') : t('theme.light')"
          >
            <Icon :name="ui.theme === 'dark' ? 'mdi:weather-night' : 'mdi:white-balance-sunny'" class="text-lg" />
          </UiButton>

          <!-- Desktop "View site" -->
          <NuxtLink to="/" class="hidden sm:block">
            <UiButton variant="secondary">
              <Icon name="mdi:web" class="text-lg" />
              <span>{{ t('admin.viewSite') }}</span>
            </UiButton>
          </NuxtLink>

          <!-- Mobile icon only -->
          <NuxtLink
            to="/"
            class="sm:hidden inline-flex h-10 w-10 items-center justify-center
                   rounded-2xl border border-app bg-surface"
            aria-label="View site"
          >
            <Icon name="mdi:open-in-new" class="text-xl" />
          </NuxtLink>
        </div>
      </div>
    </header>

    <div class="admin-content-wrap px-3 sm:px-4 py-4 md:py-6">
      <div class="relative admin-shell gap-4 xl:gap-6">
        <!-- Overlay (mobile) -->
        <div
          v-if="open"
          class="fixed inset-0 z-40 bg-black/45 md:hidden"
          @click="close()"
        />

        <!-- Sidebar -->
        <aside
          class="fixed inset-y-0 start-0 z-50
                 w-[82vw] max-w-[320px]
                 admin-sidebar-shell shadow-2xl md:shadow-none
                 md:static md:w-[290px]
                 transition-transform duration-200"
          :class="open ? 'translate-x-0' : '-translate-x-full md:translate-x-0'"
          :style="safeAreaTopStyle"
        >
          <!-- Mobile sidebar header -->
          <div class="md:hidden px-4 pt-4 pb-3 border-b border-app flex items-center justify-between">
            <div class="text-sm font-extrabold">{{ t('admin.menu') || 'القائمة' }}</div>
            <button
              class="h-9 w-9 rounded-2xl border border-app bg-surface grid place-items-center"
              @click="close()"
              aria-label="Close"
            >
              <Icon name="mdi:close" class="text-xl" />
            </button>
          </div>

          <div class="p-3 sm:p-4">
            <nav class="grid gap-2">
              <NuxtLink
                v-for="item in links"
                :key="item.to"
                :to="item.to"
                class="admin-link"
                @click="close()"
              >
                <Icon :name="item.icon" class="text-xl" />
                <span class="truncate">{{ item.label }}</span>
              </NuxtLink>
            </nav>

            <div class="mt-4 pt-4 border-t border-app">
              <NuxtLink to="/" class="block" @click="close()">
                <UiButton variant="ghost" class="w-full justify-center">
                  <Icon name="mdi:arrow-right" class="keep-ltr" />
                  <span>{{ t('admin.backToSite') }}</span>
                </UiButton>
              </NuxtLink>
            </div>
          </div>
        </aside>

        <!-- Main -->
        <main class="flex-1 min-w-0 overflow-x-hidden admin-main-shell">
          <slot />
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'

const ui = useUiStore()
const auth = useAuthStore()
const { t } = useI18n()
const route = useRoute()

const open = ref(false)

const links = computed(() => [
  { to: '/admin', label: t('admin.overview'), icon: 'mdi:view-dashboard-outline' },
  // `admin.products` and `admin.brands` are objects (they contain title/hint/etc),
  // so we must use a string key for the sidebar label.
  { to: '/admin/products', label: t('admin.productsLabel'), icon: 'mdi:cube-outline' },
  { to: '/admin/brands', label: t('admin.brandsLabel'), icon: 'mdi:storefront-outline' },
  { to: '/admin/orders', label: t('admin.orders'), icon: 'mdi:receipt-text-outline' },
  { to: '/admin/insights', label: t('admin.insights'), icon: 'mdi:chart-areaspline' },
  { to: '/admin/coupons', label: t('admin.couponsLabel') || 'Coupons', icon: 'mdi:ticket-percent-outline' },
  { to: '/admin/ads', label: t('admin.adsLabel') || 'Ads', icon: 'mdi:bullhorn-outline' },
  { to: '/admin/categories', label: t('admin.categoriesLabel') || 'التصنيفات', icon: 'mdi:shape-outline' },
  { to: '/admin/problem-categories', label: t('admin.problemCategoriesLabel') || 'تصنيفات حل المشاكل', icon: 'mdi:medical-bag' },
  { to: '/admin/appearance', label: t('admin.appearanceLabel'), icon: 'mdi:palette-outline' },
  { to: '/admin/users', label: t('admin.users.title'), icon: 'mdi:account-multiple-outline' },
])

function close() {
  open.value = false
}
function toggle() {
  open.value = !open.value
}

/**
 * قفل سكرول الصفحة فقط أثناء فتح المنيو (حتى ما يصير “السكرول يوكف” بشكل غريب)
 */
watch(open, (v) => {
  if (!import.meta.client) return
  document.documentElement.style.overflow = v ? 'hidden' : ''
  document.body.style.overflow = v ? 'hidden' : ''
})

/** اغلاق المنيو عند تغيير الراوت */
watch(
  () => route.fullPath,
  () => close()
)

/** Safe area لأجهزة الآيفون */
const safeAreaTopStyle = computed(() => ({
  paddingTop: 'env(safe-area-inset-top)'
}))
</script>

<style scoped>
.admin-shell{
  display:flex;
  align-items:flex-start;
}
.admin-sidebar-shell{
  overflow:hidden;
  border-inline-end: 1px solid rgba(var(--border), .95);
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-rgb), .92)),
    radial-gradient(circle at top, rgba(var(--primary), .10), transparent 42%);
  border-radius: 30px;
}
.admin-sidebar-shell::before{
  content:'';
  position:absolute;
  inset:0;
  pointer-events:none;
  background: linear-gradient(180deg, rgba(255,255,255,.035), transparent 24%, transparent 76%, rgba(255,255,255,.02));
}
.admin-main-shell{
  position:relative;
}
.admin-main-shell::before{
  content:'';
  position:absolute;
  inset:0;
  border-radius: 30px;
  pointer-events:none;
  background-image:
    linear-gradient(rgba(var(--border), .22) 1px, transparent 1px),
    linear-gradient(90deg, rgba(var(--border), .18) 1px, transparent 1px);
  background-size: 100% 84px, 84px 100%;
  mask-image: linear-gradient(180deg, rgba(0,0,0,.22), transparent 22%, transparent 78%, rgba(0,0,0,.12));
  opacity:.22;
}
.admin-link{
  position:relative;
  display:flex;
  align-items:center;
  gap:.8rem;
  padding: 1rem 1.05rem;
  border-radius: 1.35rem;
  border:1px solid rgba(var(--border), .92);
  background: linear-gradient(180deg, rgba(var(--surface-2-rgb), .92), rgba(var(--surface-rgb), .88));
  box-shadow: 0 14px 34px rgba(8,10,20,.08);
  transition: transform .18s ease, border-color .18s ease, background .18s ease, box-shadow .18s ease;
}
.admin-link::after{
  content:'';
  position:absolute;
  inset-inline-start: 12px;
  inset-inline-end: 12px;
  bottom:0;
  height:1px;
  background: linear-gradient(90deg, transparent, rgba(var(--border), .55), transparent);
}
.admin-link:hover{
  transform: translateY(-1px);
  border-color: rgba(var(--primary), .24);
  box-shadow: 0 18px 38px rgba(var(--primary), .08);
}
.router-link-active{
  background: linear-gradient(180deg, rgba(var(--primary), .18), rgba(var(--primary), .11));
  border-color: rgba(var(--primary), .42);
  color: rgb(var(--fg));
  box-shadow: 0 18px 40px rgba(var(--primary), .14);
}
.router-link-active::before{
  content:'';
  position:absolute;
  inset-inline-start: 0;
  top:14%;
  bottom:14%;
  width:4px;
  border-radius:999px;
  background: rgb(var(--primary));
}
@media (max-width: 767px){
  .admin-sidebar-shell{ border-radius: 0 24px 24px 0; }
  .admin-main-shell::before{ display:none; }
}
</style>
