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
        @click.stop.prevent="scrollToTop"
        :title="t('backToTop') || 'Back to top'"
        :aria-label="t('backToTop') || 'Back to top'"
      >
        <span class="back-to-top-ring" aria-hidden="true"></span>
        <span class="back-to-top-content">
          <Icon name="mdi:arrow-up" class="back-to-top-icon" />
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
const scrollThreshold = 260

const waLink = computed(() => {
  const n = whats.replace(/[^0-9]/g, '')
  return n ? `https://wa.me/${n}` : '#'
})

function handleScroll() {
  if (!import.meta.client) return
  const scrollTop = window.scrollY || document.documentElement.scrollTop || document.body.scrollTop || 0
  showBackToTop.value = scrollTop > scrollThreshold
}

function scrollToTop() {
  if (!import.meta.client) return

  window.scrollTo({ top: 0, behavior: 'smooth' })

  window.requestAnimationFrame(() => {
    document.documentElement.scrollTo({ top: 0, behavior: 'smooth' })
    document.body.scrollTo({ top: 0, behavior: 'smooth' })
  })
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
  bottom: 1.75rem;
  z-index: 90;
  width: 4.4rem;
  height: 4.4rem;
  border: 0;
  padding: 0;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: linear-gradient(135deg, rgba(var(--panel), .96), rgba(var(--panel), .88));
  color: rgb(var(--text-strong));
  box-shadow: 0 18px 45px rgba(0, 0, 0, .18), inset 0 1px 0 rgba(255, 255, 255, .45);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  overflow: hidden;
  transition: transform .2s ease, box-shadow .2s ease, opacity .2s ease;
}
.back-to-top-btn::before{
  content: '';
  position: absolute;
  inset: 1px;
  border-radius: inherit;
  border: 1px solid rgba(var(--border), .72);
  pointer-events: none;
}
.back-to-top-btn:hover{
  transform: translateY(-3px) scale(1.03);
  box-shadow: 0 24px 55px rgba(0, 0, 0, .22), inset 0 1px 0 rgba(255, 255, 255, .55);
}
.back-to-top-btn:active{
  transform: translateY(0) scale(.97);
}
.back-to-top-btn:focus-visible{
  outline: 3px solid rgba(var(--primary), .28);
  outline-offset: 3px;
}
.back-to-top-ring{
  position: absolute;
  inset: 0;
  border-radius: inherit;
  background: radial-gradient(circle at 30% 25%, rgba(255,255,255,.55), transparent 36%), linear-gradient(135deg, rgba(var(--primary), .16), transparent 58%);
  pointer-events: none;
}
.back-to-top-content{
  position: relative;
  width: 3rem;
  height: 3rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: rgba(255,255,255,.48);
  box-shadow: inset 0 1px 0 rgba(255,255,255,.7);
}
.dark .back-to-top-content{
  background: rgba(255,255,255,.08);
}
.back-to-top-icon{
  font-size: 1.75rem;
  line-height: 1;
}
.back-to-top-fade-enter-active,
.back-to-top-fade-leave-active{
  transition: opacity .18s ease, transform .18s ease;
}
.back-to-top-fade-enter-from,
.back-to-top-fade-leave-to{
  opacity: 0;
  transform: translateY(10px) scale(.92);
}
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: 5.7rem;
    bottom: 1rem;
    width: 4rem;
    height: 4rem;
  }
  .back-to-top-content{
    width: 2.7rem;
    height: 2.7rem;
  }
  .back-to-top-icon{ font-size: 1.55rem; }
}
</style>
