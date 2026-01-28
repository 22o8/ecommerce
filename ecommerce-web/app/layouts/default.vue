<template>
  <div class="min-h-screen bg-app">
    <AppNavbar />

    <main class="mx-auto max-w-7xl px-4 py-8">
      <slot />
    </main>

    <!-- Global product quick preview (so pop-up works on Home + any page) -->
    <ProductQuickPreviewModal />

    <AppFooter />

    <!-- Floating WhatsApp -->
    <a
      v-if="whats"
      class="fixed bottom-5 right-5 z-40 rounded-full border border-app bg-surface p-4 shadow-card hover:opacity-95 transition"
      :href="waLink"
      target="_blank"
      rel="noreferrer"
      :title="t('whatsappOrder')"
    >
      <Icon name="mdi:whatsapp" class="text-2xl" />
    </a>
  </div>
</template>

<script setup lang="ts">
import AppNavbar from '~/components/AppNavbar.vue'
import AppFooter from '~/components/AppFooter.vue'
import ProductQuickPreviewModal from '~/components/ProductQuickPreviewModal.vue'
const { t } = useI18n()
const config = useRuntimeConfig()
const whats = String((config.public as any).whatsappNumber || '').trim()
const waLink = computed(() => {
  const n = whats.replace(/[^0-9]/g, '')
  return n ? `https://wa.me/${n}` : '#'
})
</script>
