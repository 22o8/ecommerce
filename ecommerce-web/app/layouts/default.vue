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

    <Transition name="back-to-top-fade">
      <button
        v-if="showBackToTop"
        type="button"
        class="back-to-top-btn"
        @click="scrollToTop"
        :title="t('backToTop') || 'Back to top'"
        :aria-label="t('backToTop') || 'Back to top'"
      >
        <span class="back-to-top-btn__glow" aria-hidden="true"></span>
        <Icon name="mdi:chevron-double-up" class="back-to-top-icon" />
      </button>
    </Transition>

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
  showBackToTop.value = distanceToBottom <= 220
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
  width: 4.6rem;
  height: 4.6rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(var(--border), .72);
  background:
    radial-gradient(circle at 30% 30%, rgba(255,255,255,.22), transparent 42%),
    linear-gradient(180deg, rgba(var(--panel), .98), rgba(var(--panel), .92));
  color: rgb(var(--text-strong));
  box-shadow:
    0 18px 44px rgba(0, 0, 0, .18),
    0 0 0 1px rgba(255,255,255,.05) inset;
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  transition: transform .22s ease, box-shadow .22s ease, background .22s ease, opacity .22s ease;
  overflow: hidden;
}
.back-to-top-btn__glow{
  position: absolute;
  inset: 0;
  border-radius: inherit;
  background: radial-gradient(circle at center, rgba(var(--primary), .22), transparent 68%);
  opacity: .95;
  pointer-events: none;
}
.back-to-top-btn:hover{
  transform: translateY(-3px) scale(1.04);
  box-shadow:
    0 24px 54px rgba(0, 0, 0, .24),
    0 0 0 1px rgba(255,255,255,.08) inset;
}
.back-to-top-btn:active{
  transform: translateY(-1px) scale(.98);
}
.back-to-top-icon{
  position: relative;
  z-index: 1;
  font-size: 2.25rem;
  line-height: 1;
  filter: drop-shadow(0 2px 6px rgba(0,0,0,.18));
}
.back-to-top-fade-enter-active,
.back-to-top-fade-leave-active{
  transition: opacity .22s ease, transform .22s ease;
}
.back-to-top-fade-enter-from,
.back-to-top-fade-leave-to{
  opacity: 0;
  transform: translateY(10px) scale(.9);
}
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: 1rem;
    bottom: 5.25rem;
    width: 4.2rem;
    height: 4.2rem;
  }
  .back-to-top-icon{
    font-size: 2.05rem;
  }
}
</style>
