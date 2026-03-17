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
      <span class="back-to-top-btn__ring" aria-hidden="true"></span>
      <span class="back-to-top-btn__shine" aria-hidden="true"></span>
      <Icon name="mdi:chevron-double-up" class="back-to-top-icon" />
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
  showBackToTop.value = scrollTop > 420
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
  inset-inline-end: 7rem;
  bottom: 1.85rem;
  z-index: 55;
  width: 4.65rem;
  height: 4.65rem;
  border-radius: 1.6rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(var(--primary), .28);
  background:
    linear-gradient(180deg, rgba(var(--panel), .98), rgba(var(--panel), .90)),
    radial-gradient(circle at 30% 20%, rgba(var(--primary), .22), transparent 55%);
  color: rgb(var(--text-strong));
  box-shadow: 0 18px 42px rgba(0, 0, 0, .16);
  backdrop-filter: blur(14px);
  transition: transform .22s ease, opacity .22s ease, box-shadow .22s ease, border-color .22s ease, background .22s ease;
  overflow: hidden;
  cursor: pointer;
}
.back-to-top-btn:hover{
  transform: translateY(-6px) scale(1.04);
  border-color: rgba(var(--primary), .5);
  box-shadow: 0 26px 54px rgba(0, 0, 0, .22);
}
.back-to-top-btn:active{
  transform: translateY(-2px) scale(.97);
}
.back-to-top-btn:focus-visible{
  outline: 0;
  box-shadow:
    0 0 0 4px rgba(var(--primary), .18),
    0 24px 50px rgba(0, 0, 0, .18);
}
.back-to-top-btn__ring,
.back-to-top-btn__shine{
  position: absolute;
  pointer-events: none;
}
.back-to-top-btn__ring{
  inset: 8px;
  border-radius: 1.25rem;
  border: 1px solid rgba(var(--primary), .12);
}
.back-to-top-btn__shine{
  width: 4rem;
  height: 4rem;
  border-radius: 999px;
  top: -1.4rem;
  right: -1.1rem;
  background: rgba(255,255,255,.22);
  filter: blur(14px);
}
.back-to-top-icon{
  position: relative;
  z-index: 1;
  font-size: 2.1rem;
  line-height: 1;
  transition: transform .22s ease;
}
.back-to-top-btn:hover .back-to-top-icon{
  transform: translateY(-2px);
}
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: 6rem;
    bottom: 1.15rem;
    width: 4.1rem;
    height: 4.1rem;
    border-radius: 1.35rem;
  }
  .back-to-top-btn__ring{ border-radius: 1rem; }
  .back-to-top-icon{ font-size: 1.9rem; }
}
</style>
