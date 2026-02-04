<template>
  <div class="min-h-screen w-full bg-app text-fg overflow-x-hidden">
    <!-- Header -->
    <header
      class="sticky top-0 z-40 border-b border-app bg-app/85 backdrop-blur supports-[backdrop-filter]:bg-app/70"
      :style="safeAreaTopStyle"
    >
      <div class="mx-auto max-w-7xl px-3 sm:px-4 py-3 flex items-center gap-2">
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
          <div class="font-extrabold rtl-text truncate">
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
              <span class="rtl-text">{{ t('admin.viewSite') }}</span>
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

    <div class="mx-auto max-w-7xl px-3 sm:px-4 py-4 md:py-6">
      <div class="relative flex gap-4">
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
                 bg-surface border-e border-app
                 shadow-2xl md:shadow-none
                 md:static md:w-72
                 transition-transform duration-200"
          :class="open ? 'translate-x-0' : '-translate-x-full md:translate-x-0'"
          :style="safeAreaTopStyle"
        >
          <!-- Mobile sidebar header -->
          <div class="md:hidden px-4 pt-4 pb-3 border-b border-app flex items-center justify-between">
            <div class="text-sm font-extrabold rtl-text">{{ t('admin.menu') || 'القائمة' }}</div>
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
                <span class="rtl-text truncate">{{ item.label }}</span>
              </NuxtLink>
            </nav>

            <div class="mt-4 pt-4 border-t border-app">
              <NuxtLink to="/" class="block" @click="close()">
                <UiButton variant="ghost" class="w-full justify-center">
                  <Icon name="mdi:arrow-right" class="keep-ltr" />
                  <span class="rtl-text">{{ t('admin.backToSite') }}</span>
                </UiButton>
              </NuxtLink>
            </div>
          </div>
        </aside>

        <!-- Main -->
        <main class="flex-1 min-w-0 overflow-x-hidden">
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
  { to: '/admin/products', label: t('admin.products'), icon: 'mdi:cube-outline' },
  { to: '/admin/brands', label: t('admin.brands'), icon: 'mdi:storefront-outline' },
  { to: '/admin/orders', label: t('admin.orders'), icon: 'mdi:receipt-text-outline' },
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
.admin-link{
  display:flex;
  align-items:center;
  gap:.75rem;
  padding:.85rem 1rem;
  border-radius:1.25rem;
  border:1px solid rgb(var(--border));
  background: rgb(var(--surface-2));
  transition: filter .15s ease, background .15s ease, border-color .15s ease;
}
.admin-link:hover{ filter: brightness(1.03); }

.router-link-active{
  background: rgba(var(--primary), .12);
  border-color: rgba(var(--primary), .35);
  color: rgb(var(--fg));
}
</style>
