<template>
<<<<<<< HEAD
  <div
    class="min-h-screen w-full bg-gray-50 text-gray-900
           dark:bg-[#020420] dark:text-white overflow-x-hidden"
  >
    <!-- Header -->
    <header
      class="sticky top-0 z-40
             border-b border-gray-200 dark:border-white/10
             bg-white/90 dark:bg-[#020420]/90 backdrop-blur
             px-3 sm:px-4 py-3"
      :style="safeAreaTopStyle"
    >
      <div class="flex items-center justify-between gap-2">
        <div class="flex items-center gap-2 min-w-0">
          <!-- Menu button (mobile) -->
          <button
            class="md:hidden inline-flex h-10 w-10 shrink-0 items-center justify-center
                   rounded-xl border border-gray-300 bg-white
                   dark:bg-white/5 dark:border-white/15
                   active:scale-[0.98] transition"
            @click="toggle()"
            aria-label="Open menu"
          >
            <span class="text-lg leading-none">☰</span>
          </button>

          <div class="min-w-0">
            <h1 class="text-sm sm:text-base font-semibold truncate">
              لوحة التحكم
            </h1>
            <p class="text-xs text-gray-500 dark:text-white/60 truncate -mt-0.5">
              {{ emailHint }}
            </p>
          </div>
        </div>

        <!-- Right actions -->
        <div class="flex items-center gap-2 shrink-0">
          <ThemeToggle />
          <NuxtLink
            to="/"
            class="text-sm font-medium text-primary hover:underline hidden sm:inline"
          >
            عرض الموقع
          </NuxtLink>

          <!-- Mobile: icon link only -->
          <NuxtLink
            to="/"
            class="sm:hidden inline-flex h-10 w-10 items-center justify-center
                   rounded-xl border border-gray-300 bg-white
                   dark:bg-white/5 dark:border-white/15"
            aria-label="View site"
          >
            ↗
          </NuxtLink>
        </div>
      </div>
    </header>

    <div class="relative flex">
      <!-- Overlay (mobile) -->
      <div
        v-if="open"
        class="fixed inset-0 z-40 bg-black/45 md:hidden"
        @click="close()"
      />

      <!-- Sidebar -->
      <aside
        class="fixed inset-y-0 left-0 z-50 w-[82vw] max-w-[320px]
               bg-white dark:bg-[#020420]
               border-r border-gray-200 dark:border-white/10
               transform transition-transform duration-300
               md:static md:translate-x-0 md:w-64
               shadow-2xl md:shadow-none"
        :class="open ? 'translate-x-0' : '-translate-x-full'"
        :style="safeAreaTopStyle"
      >
        <!-- Sidebar header (mobile only) -->
        <div class="md:hidden px-4 pt-4 pb-3 border-b border-gray-200 dark:border-white/10">
          <div class="flex items-center justify-between">
            <div class="text-sm font-semibold">القائمة</div>
            <button
              class="h-9 w-9 rounded-xl border border-gray-300 dark:border-white/15
                     bg-white dark:bg-white/5"
              @click="close()"
              aria-label="Close menu"
            >
              ✕
            </button>
          </div>
        </div>

        <nav class="flex flex-col gap-1 p-3 sm:p-4">
          <NuxtLink
            v-for="item in links"
            :key="item.to"
            :to="item.to"
            class="flex items-center gap-3 rounded-xl px-3 py-2.5
                   text-sm font-medium
                   border border-transparent
                   hover:bg-gray-100 hover:border-gray-200
                   dark:hover:bg-white/10 dark:hover:border-white/10
                   transition"
            active-class="is-active"
            @click="close()"
          >
            <span class="text-base">{{ item.icon }}</span>
            <span class="truncate">{{ item.label }}</span>
          </NuxtLink>
        </nav>

        <!-- Bottom area -->
        <div class="mt-auto p-3 sm:p-4">
          <NuxtLink
            to="/"
            class="w-full inline-flex items-center justify-center gap-2
                   rounded-xl border border-gray-200 bg-white
                   text-gray-800 hover:bg-gray-50
                   dark:bg-white/5 dark:text-white dark:border-white/10 dark:hover:bg-white/10
                   px-3 py-2.5 text-sm font-semibold transition"
            @click="close()"
          >
            <span>↩</span>
            <span>الرجوع للموقع</span>
          </NuxtLink>
        </div>
      </aside>

      <!-- Main content -->
      <main
        class="flex-1 min-h-[calc(100vh-64px)]
               px-3 sm:px-4 md:px-6
               py-4 md:py-6
               overflow-x-hidden"
      >
        <slot />
      </main>
=======
  <div class="min-h-screen bg-app text-fg">
    <!-- Header -->
    <header class="sticky top-0 z-40 border-b border-app bg-app/85 backdrop-blur supports-[backdrop-filter]:bg-app/70">
      <div class="mx-auto max-w-7xl px-3 sm:px-4 py-3 flex items-center gap-2">
        <!-- Menu button (mobile) -->
        <button
          class="md:hidden h-10 w-10 rounded-2xl border border-app bg-surface grid place-items-center"
          @click="open = !open"
          aria-label="Menu"
        >
          <Icon name="mdi:menu" class="text-xl" />
        </button>

        <div class="min-w-0">
          <div class="font-extrabold rtl-text truncate">{{ t('admin.title') }}</div>
          <div class="text-xs text-muted keep-ltr truncate">{{ auth.user?.email || '' }}</div>
        </div>

        <div class="flex-1" />

        <div class="flex items-center gap-2">
          <UiButton variant="ghost" class="px-2" @click="ui.toggleTheme()" :title="ui.theme === 'dark' ? t('theme.dark') : t('theme.light')">
            <Icon :name="ui.theme === 'dark' ? 'mdi:weather-night' : 'mdi:white-balance-sunny'" class="text-lg" />
          </UiButton>

          <NuxtLink to="/" class="hidden sm:block">
            <UiButton variant="secondary">
              <Icon name="mdi:web" class="text-lg" />
              <span class="rtl-text">{{ t('admin.viewSite') }}</span>
            </UiButton>
          </NuxtLink>
        </div>
      </div>
    </header>

    <div class="mx-auto max-w-7xl px-3 sm:px-4 py-4 md:py-6">
      <div class="flex gap-4">
        <!-- Sidebar -->
        <aside
          class="fixed inset-y-0 start-0 z-50 w-72 bg-surface border-e border-app
                 md:static md:w-72 md:translate-x-0
                 transition-transform duration-200"
          :class="open ? 'translate-x-0' : '-translate-x-full md:translate-x-0'"
        >
          <div class="p-4">
            <nav class="grid gap-2">
              <NuxtLink
                v-for="item in links"
                :key="item.to"
                :to="item.to"
                class="admin-link"
                @click="open = false"
              >
                <Icon :name="item.icon" class="text-xl" />
                <span class="rtl-text">{{ item.label }}</span>
              </NuxtLink>
            </nav>

            <div class="mt-4 pt-4 border-t border-app">
              <NuxtLink to="/" class="block" @click="open = false">
                <UiButton variant="ghost" class="w-full justify-center">
                  <Icon name="mdi:arrow-right" class="keep-ltr" />
                  <span class="rtl-text">{{ t('admin.backToSite') }}</span>
                </UiButton>
              </NuxtLink>
            </div>
          </div>
        </aside>

        <!-- Overlay (mobile) -->
        <div v-if="open" class="fixed inset-0 z-40 bg-black/40 md:hidden" @click="open = false" />

        <!-- Main -->
        <main class="flex-1 min-w-0">
          <slot />
        </main>
      </div>
>>>>>>> 0898076 (feat(admin): improve light theme + full users management)
    </div>
  </div>
</template>

<script setup lang="ts">
const open = ref(false)

<<<<<<< HEAD
// لو عندك auth store خليها ديناميكية، هسه خليتها نص ثابت حتى ما تخرب
const emailHint = 'test@g.com'

const links = [
  { to: '/admin', label: 'نظرة عامة', icon: '📊' },
  { to: '/admin/products', label: 'المنتجات', icon: '🛒' },
  { to: '/admin/orders', label: 'الطلبات', icon: '📦' },
  { to: '/admin/users', label: 'المستخدمين', icon: '👤' }
]

// قفل السكرول بالموبايل لما القائمة مفتوحة (هذا اللي كان يسبب “السكرول يوكف”/يتخرب)
watch(open, (v) => {
  if (!import.meta.client) return
  document.documentElement.style.overflow = v ? 'hidden' : ''
  document.body.style.overflow = v ? 'hidden' : ''
})

// اغلاق القائمة عند تغيير الصفحة
const route = useRoute()
watch(
  () => route.fullPath,
  () => close()
)

function close() {
  open.value = false
}
function toggle() {
  open.value = !open.value
}

// Safe area لأجهزة iPhone notch (يشتغل حتى لو المتصفح يدعم env())
const safeAreaTopStyle = computed(() => ({
  paddingTop: 'env(safe-area-inset-top)'
}))
</script>

<style scoped>
/* لون primary للايت/دارك */
.text-primary {
  color: #7c3aed;
}

/* Active link واضح بالثيم الأبيض خصوصاً */
.is-active {
  background-color: rgba(124, 58, 237, 0.12);
  color: #7c3aed;
  border-color: rgba(124, 58, 237, 0.22);
}

.dark .is-active {
  background-color: rgba(124, 58, 237, 0.18);
  color: rgb(224, 209, 255);
  border-color: rgba(255, 255, 255, 0.10);
=======
const ui = useUiStore()
const auth = useAuthStore()
const { t } = useI18n()

const open = ref(false)

const links = computed(() => [
  { to: '/admin', label: t('admin.overview'), icon: 'mdi:view-dashboard-outline' },
  { to: '/admin/products', label: t('admin.products'), icon: 'mdi:cube-outline' },
  { to: '/admin/orders', label: t('admin.orders'), icon: 'mdi:receipt-text-outline' },
  { to: '/admin/users', label: t('admin.users'), icon: 'mdi:account-multiple-outline' },
])
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
}
.admin-link:hover{ filter: brightness(1.03); }
.router-link-active{
  background: rgba(var(--primary), .12);
  border-color: rgba(var(--primary), .35);
  color: rgb(var(--fg));
>>>>>>> 0898076 (feat(admin): improve light theme + full users management)
}
</style>
