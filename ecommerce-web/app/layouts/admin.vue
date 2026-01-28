<template>
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
    </div>
  </div>
</template>

<script setup lang="ts">
const open = ref(false)

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
}
</style>
