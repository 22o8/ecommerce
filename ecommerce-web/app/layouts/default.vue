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
        <span class="back-to-top-glow" aria-hidden="true"></span>
        <Icon name="mdi:arrow-up-thin" class="back-to-top-icon" />
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

  showBackToTop.value = scrollTop > 520 || distanceToBottom <= 260
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
  inset-inline-end: 1.15rem;
  bottom: 6rem;
  z-index: 80;
  width: 4.5rem;
  height: 4.5rem;
  border: 0;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  isolation: isolate;
  overflow: hidden;
  cursor: pointer;
  color: #ffffff;
  background: linear-gradient(135deg, rgba(17, 24, 39, .96), rgba(59, 130, 246, .88));
  box-shadow: 0 18px 44px rgba(15, 23, 42, .34), 0 0 0 1px rgba(255,255,255,.12) inset;
  backdrop-filter: blur(12px);
  transition: transform .22s ease, box-shadow .22s ease, opacity .22s ease, filter .22s ease;
}
.back-to-top-btn::after{
  content: '';
  position: absolute;
  inset: 4px;
  border-radius: inherit;
  background: linear-gradient(180deg, rgba(255,255,255,.18), rgba(255,255,255,.02));
  z-index: 0;
}
.back-to-top-glow{
  position: absolute;
  inset: -20%;
  background: radial-gradient(circle, rgba(255,255,255,.28) 0%, rgba(255,255,255,0) 65%);
  opacity: .9;
  z-index: 0;
  animation: backToTopPulse 2.2s ease-in-out infinite;
}
.back-to-top-btn:hover{
  transform: translateY(-4px) scale(1.06);
  box-shadow: 0 24px 52px rgba(15, 23, 42, .42), 0 0 0 1px rgba(255,255,255,.18) inset;
  filter: saturate(1.08);
}
.back-to-top-btn:active{
  transform: scale(.97);
}
.back-to-top-btn:focus-visible{
  outline: 3px solid rgba(59, 130, 246, .35);
  outline-offset: 4px;
}
.back-to-top-icon{
  position: relative;
  z-index: 1;
  font-size: 2.35rem;
  line-height: 1;
  filter: drop-shadow(0 4px 10px rgba(0, 0, 0, .28));
}
.back-to-top-fade-enter-active,
.back-to-top-fade-leave-active{
  transition: opacity .2s ease, transform .2s ease;
}
.back-to-top-fade-enter-from,
.back-to-top-fade-leave-to{
  opacity: 0;
  transform: translateY(10px) scale(.92);
}
@keyframes backToTopPulse{
  0%, 100% { transform: scale(.96); opacity: .82; }
  50% { transform: scale(1.08); opacity: 1; }
}
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: .95rem;
    bottom: 5.65rem;
    width: 4.1rem;
    height: 4.1rem;
  }
  .back-to-top-icon{ font-size: 2.1rem; }
}
</style>
