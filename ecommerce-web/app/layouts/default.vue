<template>
  <div class="min-h-screen bg-app relative overflow-x-clip">
    <GlobalEffects />
    <AppNavbar />
    <GlobalAds />

    <main class="mx-auto max-w-7xl px-4 py-8">
      <slot />
    </main>

    <!-- Global product quick preview (so pop-up works on Home + any page) -->
    <ProductQuickPreviewModal />

    <AppFooter />

    <button
      v-show="showBackToTop"
      type="button"
      class="back-to-top-btn"
      @click="scrollToTop"
      :title="t('backToTop') || 'Back to top'"
      :aria-label="t('backToTop') || 'Back to top'"
    >
      <Icon name="mdi:arrow-up" class="back-to-top-icon" />
    </button>

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
import { computed, onMounted, onUnmounted, ref } from 'vue'
import AppNavbar from '~/components/AppNavbar.vue'
import AppFooter from '~/components/AppFooter.vue'
import ProductQuickPreviewModal from '~/components/ProductQuickPreviewModal.vue'
import GlobalEffects from '~/components/appearance/GlobalEffects.vue'
import GlobalAds from '~/components/appearance/GlobalAds.vue'
const { t } = useI18n()
const config = useRuntimeConfig()
const whats = String((config.public as any).whatsappNumber || '').trim()
const showBackToTop = ref(false)

const waLink = computed(() => {
  const n = whats.replace(/[^0-9]/g, '')
  return n ? `https://wa.me/${n}` : '#'
})

function handleScroll() {
  if (!import.meta.client) return

  const scrollTop = window.scrollY || document.documentElement.scrollTop || 0
  const docHeight = Math.max(
    document.body.scrollHeight,
    document.documentElement.scrollHeight,
    document.body.offsetHeight,
    document.documentElement.offsetHeight
  )
  const viewportHeight = window.innerHeight || document.documentElement.clientHeight || 0
  const distanceToBottom = docHeight - (scrollTop + viewportHeight)

  // Show only when the user is very close to the end of the page.
  const isNearPageEnd = distanceToBottom <= 24
  const hasScrollablePage = docHeight > viewportHeight + 120

  showBackToTop.value = hasScrollablePage && isNearPageEnd && scrollTop > 0
}

function scrollToTop() {
  if (!import.meta.client) return
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => {
  handleScroll()
  window.addEventListener('scroll', handleScroll, { passive: true })
  window.addEventListener('resize', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
  window.removeEventListener('resize', handleScroll)
})
</script>

<style scoped>
.back-to-top-btn{
  position: fixed;
  inset-inline-end: 1.25rem;
  bottom: 5.85rem;
  z-index: 55;
  width: 4.15rem;
  height: 4.15rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(var(--border), .9);
  background: rgba(var(--panel), .97);
  color: rgb(var(--text-strong));
  box-shadow: 0 18px 42px rgba(0, 0, 0, .18);
  backdrop-filter: blur(10px);
  transition: transform .18s ease, opacity .18s ease, box-shadow .18s ease, background .18s ease;
}
.back-to-top-btn:hover{
  transform: translateY(-2px) scale(1.03);
  box-shadow: 0 22px 48px rgba(0, 0, 0, .22);
}
.back-to-top-btn:active{
  transform: scale(.98);
}
.back-to-top-icon{
  font-size: 2rem;
  line-height: 1;
}
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: 1rem;
    bottom: 5.25rem;
    width: 3.9rem;
    height: 3.9rem;
  }
  .back-to-top-icon{ font-size: 1.85rem; }
}
</style>
