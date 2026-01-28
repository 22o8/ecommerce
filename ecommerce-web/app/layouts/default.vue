<template>
  <div
    class="min-h-screen w-full bg-gray-50 text-gray-900
           dark:bg-[#020420] dark:text-white"
  >
    <!-- Header -->
    <header
      class="sticky top-0 z-40 flex items-center justify-between
             border-b border-gray-200 dark:border-white/10
             bg-white/95 dark:bg-[#020420]/95 backdrop-blur
             px-4 py-3"
    >
      <div class="flex items-center gap-3">
        <!-- Menu button (mobile) -->
        <button
          class="md:hidden flex h-10 w-10 items-center justify-center
                 rounded-lg border border-gray-300 dark:border-white/20"
          @click="open = !open"
        >
          ☰
        </button>

        <h1 class="text-base font-semibold">
          Admin Panel
        </h1>
      </div>

      <!-- Right actions -->
      <div class="flex items-center gap-2">
        <ThemeToggle />
        <NuxtLink
          to="/"
          class="text-sm text-primary hover:underline"
        >
          View site
        </NuxtLink>
      </div>
    </header>

    <div class="flex">
      <!-- Sidebar -->
      <aside
        class="fixed inset-y-0 left-0 z-50 w-64
               bg-white dark:bg-[#020420]
               border-r border-gray-200 dark:border-white/10
               transform transition-transform duration-300
               md:static md:translate-x-0"
        :class="open ? 'translate-x-0' : '-translate-x-full'"
      >
        <nav class="flex flex-col gap-1 p-4">
          <NuxtLink
            v-for="item in links"
            :key="item.to"
            :to="item.to"
            class="flex items-center gap-3 rounded-lg px-3 py-2
                   text-sm font-medium
                   hover:bg-gray-100 dark:hover:bg-white/10"
            active-class="bg-primary/10 text-primary"
            @click="open = false"
          >
            <span>{{ item.icon }}</span>
            <span>{{ item.label }}</span>
          </NuxtLink>
        </nav>
      </aside>

      <!-- Overlay (mobile) -->
      <div
        v-if="open"
        class="fixed inset-0 z-40 bg-black/40 md:hidden"
        @click="open = false"
      />

      <!-- Main content -->
      <main
        class="flex-1 min-h-[calc(100vh-56px)]
               px-4 py-4 md:px-6 md:py-6
               overflow-x-hidden"
      >
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
const open = ref(false)

const links = [
  { to: '/admin', label: 'Dashboard', icon: '📊' },
  { to: '/admin/products', label: 'Products', icon: '🛒' },
  { to: '/admin/orders', label: 'Orders', icon: '📦' },
  { to: '/admin/users', label: 'Users', icon: '👤' }
]
</script>

<style scoped>
/* تحسين القراءة بالثيم الأبيض */
.bg-primary\/10 {
  background-color: rgba(124, 58, 237, 0.12);
}
.text-primary {
  color: #7c3aed;
}
</style>
