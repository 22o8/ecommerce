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
        v-show="showBackToTop"
        type="button"
        class="back-to-top-btn"
        @click="scrollToTop"
        :title="t('backToTop') || 'Back to top'"
        :aria-label="t('backToTop') || 'Back to top'"
      >
        <span class="back-to-top-btn__glow" aria-hidden="true"></span>
        <span class="back-to-top-btn__inner">
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

const waLink = computed(() => {
  const n = whats.replace(/[^0-9]/g, '')
  return n ? `https://wa.me/${n}` : '#'
})

function handleScroll() {
  if (!import.meta.client) return
  const scrollTop = window.scrollY || document.documentElement.scrollTop || 0
  showBackToTop.value = scrollTop > 320
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
  inset-inline-end: calc(1.25rem + 4.7rem + .9rem);
  bottom: 1.35rem;
  z-index: 58;
  width: 3.9rem;
  height: 3.9rem;
  border: 0;
  padding: 0;
  border-radius: 999px;
  display: inline-grid;
  place-items: center;
  cursor: pointer;
  background: transparent;
  color: rgb(var(--text-strong));
  transition: transform .22s ease, filter .22s ease, opacity .22s ease;
}
.back-to-top-btn__glow{
  position: absolute;
  inset: 0;
  border-radius: inherit;
  background:
    radial-gradient(circle at 30% 30%, rgba(255,255,255,.95), rgba(255,255,255,.15) 34%, transparent 35%),
    linear-gradient(180deg, rgba(var(--panel), .98), rgba(var(--panel), .88));
  border: 1px solid rgba(var(--border), .88);
  box-shadow:
    0 14px 34px rgba(0,0,0,.16),
    0 0 0 1px rgba(255,255,255,.18) inset,
    0 -10px 22px rgba(255,255,255,.14) inset;
  backdrop-filter: blur(16px);
}
.back-to-top-btn__inner{
  position: relative;
  z-index: 1;
  width: calc(100% - 10px);
  height: calc(100% - 10px);
  border-radius: inherit;
  display: grid;
  place-items: center;
  background: linear-gradient(180deg, rgba(255,255,255,.26), rgba(255,255,255,.06));
  transition: transform .22s ease, background .22s ease;
}
.back-to-top-btn:hover{
  transform: translateY(-4px);
  filter: saturate(1.05);
}
.back-to-top-btn:hover .back-to-top-btn__glow{
  box-shadow:
    0 18px 40px rgba(0,0,0,.2),
    0 0 0 1px rgba(255,255,255,.24) inset,
    0 -12px 24px rgba(255,255,255,.16) inset;
}
.back-to-top-btn:hover .back-to-top-btn__inner{
  transform: scale(1.03);
}
.back-to-top-btn:active{
  transform: translateY(-1px) scale(.97);
}
.back-to-top-icon{
  font-size: 1.55rem;
  line-height: 1;
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
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: calc(1rem + 4.25rem + .75rem);
    bottom: 1rem;
    width: 3.55rem;
    height: 3.55rem;
  }
  .back-to-top-icon{ font-size: 1.4rem; }
}
</style>
