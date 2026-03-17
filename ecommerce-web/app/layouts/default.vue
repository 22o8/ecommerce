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
        <span class="back-to-top-inner">
          <span class="back-to-top-badge" aria-hidden="true"></span>
          <Icon name="mdi:chevron-up" class="back-to-top-icon" />
        </span>
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

  // Show only when the user is close to the end of the page.
  const isNearPageEnd = distanceToBottom <= Math.max(120, viewportHeight * 0.14)
  const hasScrollablePage = docHeight > viewportHeight + 180

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
  inset-inline-end: 5.4rem;
  bottom: 1.65rem;
  z-index: 55;
  width: 4.2rem;
  height: 4.2rem;
  border: 0;
  padding: 0;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: rgb(var(--text));
  cursor: pointer;
  isolation: isolate;
  box-shadow: 0 18px 40px rgba(0, 0, 0, .16);
  transition: transform .22s ease, box-shadow .22s ease, opacity .22s ease;
}
.back-to-top-glow{
  position: absolute;
  inset: 0;
  border-radius: inherit;
  background:
    radial-gradient(circle at 28% 24%, rgba(255,255,255,.95), rgba(255,255,255,.18) 26%, transparent 27%),
    linear-gradient(180deg, rgba(var(--surface), .98), rgba(var(--surface), .94));
  border: 1px solid rgba(var(--border), .95);
  box-shadow:
    inset 0 1px 0 rgba(255,255,255,.72),
    inset 0 -10px 22px rgba(var(--primary), .06),
    0 0 0 6px rgba(var(--primary), .10);
}
.back-to-top-inner{
  position: relative;
  z-index: 1;
  width: calc(100% - 8px);
  height: calc(100% - 8px);
  border-radius: inherit;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(180deg, rgba(var(--bg), .72), rgba(var(--bg), .92));
  backdrop-filter: blur(12px);
  overflow: hidden;
}
.back-to-top-badge{
  position: absolute;
  top: .62rem;
  left: 50%;
  transform: translateX(-50%);
  width: .42rem;
  height: .42rem;
  border-radius: 999px;
  background: rgba(var(--primary), .9);
  box-shadow: 0 0 0 6px rgba(var(--primary), .12);
}
.back-to-top-btn:hover{
  transform: translateY(-4px) scale(1.04);
  box-shadow: 0 22px 46px rgba(0, 0, 0, .22);
}
.back-to-top-btn:active{
  transform: translateY(-1px) scale(.97);
}
.back-to-top-icon{
  font-size: 2.1rem;
  line-height: 1;
  font-weight: 900;
  color: rgb(var(--text-strong));
  transform: translateY(.06rem);
}
.back-to-top-fade-enter-active,
.back-to-top-fade-leave-active{
  transition: opacity .26s ease, transform .26s ease;
}
.back-to-top-fade-enter-from,
.back-to-top-fade-leave-to{
  opacity: 0;
  transform: translateY(14px) scale(.88);
}
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: 4.9rem;
    bottom: 1rem;
    width: 3.7rem;
    height: 3.7rem;
  }
  .back-to-top-badge{ top: .52rem; }
  .back-to-top-icon{ font-size: 1.9rem; }
}
</style>
