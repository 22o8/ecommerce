<template>
  <div class="min-h-screen bg-app relative overflow-x-clip">
    <GlobalEffects />
    <AppNavbar />
    <GlobalAds />

    <main class="mx-auto max-w-7xl px-4 py-8">
      <slot />
    </main>


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
          <Icon name="mdi:arrow-up-thin" class="back-to-top-icon" />
        </span>
      </button>
    </Transition>

    <!-- Floating WhatsApp -->
    <a
      v-if="whats"
      class="wa-fab"
      :class="{ 'wa-fab--hint': showWaHint }"
      :href="waLink"
      target="_blank"
      rel="noreferrer"
      :title="t('whatsappOrder')"
      :aria-label="t('whatsappOrder')"
    >
      <span class="wa-fab__label rtl-text" :class="{ 'wa-fab__label--visible': showWaHint }">استشارة</span>
      <span class="wa-fab__icon-wrap" aria-hidden="true">
        <Icon name="mdi:whatsapp" class="wa-fab__icon" />
      </span>
    </a>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import AppNavbar from '~/components/AppNavbar.vue'
import AppFooter from '~/components/AppFooter.vue'
import GlobalEffects from '~/components/appearance/GlobalEffects.vue'
import GlobalAds from '~/components/appearance/GlobalAds.vue'
const { t } = useI18n()
const config = useRuntimeConfig()
const whats = String((config.public as any).whatsappNumber || '').trim()
const showBackToTop = ref(false)
const showWaHint = ref(false)
let waInterval: ReturnType<typeof setInterval> | null = null
let waHideTimeout: ReturnType<typeof setTimeout> | null = null

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

function triggerWaHint() {
  showWaHint.value = true
  if (waHideTimeout) clearTimeout(waHideTimeout)
  waHideTimeout = setTimeout(() => {
    showWaHint.value = false
  }, 4200)
}

onMounted(() => {
  handleScroll()
  triggerWaHint()
  waInterval = setInterval(triggerWaHint, 30000)
  window.addEventListener('scroll', handleScroll, { passive: true })
  window.addEventListener('resize', handleScroll)
})

onUnmounted(() => {
  if (waInterval) clearInterval(waInterval)
  if (waHideTimeout) clearTimeout(waHideTimeout)
  window.removeEventListener('scroll', handleScroll)
  window.removeEventListener('resize', handleScroll)
})
</script>

<style scoped>
.back-to-top-btn{
  position: fixed;
  inset-inline-end: 1.15rem;
  bottom: 6.05rem;
  z-index: 55;
  width: 4.5rem;
  height: 4.5rem;
  border: 0;
  padding: 0;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: rgb(var(--text-strong));
  cursor: pointer;
  isolation: isolate;
  filter: drop-shadow(0 18px 34px rgba(0, 0, 0, .18));
  transition: transform .2s ease, filter .2s ease;
}
.back-to-top-glow{
  position: absolute;
  inset: 0;
  border-radius: inherit;
  background:
    radial-gradient(circle at 30% 28%, rgba(255,255,255,.92), rgba(255,255,255,.22) 26%, transparent 27%),
    linear-gradient(135deg, rgba(var(--primary), .96), rgba(var(--primary), .72));
  border: 1px solid rgba(var(--border), .72);
  box-shadow:
    inset 0 1px 0 rgba(255,255,255,.58),
    0 0 0 7px rgba(var(--primary), .08);
}
.back-to-top-inner{
  position: relative;
  z-index: 1;
  width: calc(100% - 10px);
  height: calc(100% - 10px);
  border-radius: inherit;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: rgba(255,255,255,.08);
  backdrop-filter: blur(8px);
}
.back-to-top-btn:hover{
  transform: translateY(-3px) scale(1.035);
  filter: drop-shadow(0 22px 40px rgba(0, 0, 0, .22));
}
.back-to-top-btn:active{
  transform: translateY(0) scale(.97);
}
.back-to-top-icon{
  font-size: 2.35rem;
  line-height: 1;
  font-weight: 900;
}
.back-to-top-fade-enter-active,
.back-to-top-fade-leave-active{
  transition: opacity .24s ease, transform .24s ease;
}
.back-to-top-fade-enter-from,
.back-to-top-fade-leave-to{
  opacity: 0;
  transform: translateY(12px) scale(.9);
}
@media (max-width: 768px){
  .back-to-top-btn{
    inset-inline-end: .95rem;
    bottom: 5.45rem;
    width: 4rem;
    height: 4rem;
  }
  .back-to-top-icon{ font-size: 2.05rem; }
}

.wa-fab{
  position: fixed;
  inset-inline-end: 1.15rem;
  bottom: 1.15rem;
  z-index: 56;
  display: inline-flex;
  align-items: center;
  gap: .6rem;
  text-decoration: none;
  color: #fff;
}
.wa-fab__label{
  max-width: 0;
  opacity: 0;
  overflow: hidden;
  white-space: nowrap;
  padding: 0;
  border-radius: 999px;
  background: rgba(8,10,18,.92);
  color: #fff;
  border: 1px solid rgba(255,255,255,.12);
  box-shadow: 0 18px 36px rgba(0,0,0,.18);
  transition: max-width .35s ease, opacity .25s ease, padding .35s ease, transform .35s ease;
  transform: translateX(.35rem);
  font-weight: 900;
  font-size: .96rem;
}
.wa-fab__label--visible{
  max-width: 8rem;
  opacity: 1;
  padding: .78rem 1rem;
  transform: translateX(0);
}
.wa-fab__icon-wrap{
  width: 3.9rem;
  height: 3.9rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #25D366, #1faa52);
  border: 1px solid rgba(255,255,255,.18);
  box-shadow: 0 18px 34px rgba(10,20,14,.24);
}
.wa-fab--hint .wa-fab__icon-wrap{
  animation: waPulse 1.35s ease-in-out 3;
}
.wa-fab__icon{ font-size: 2rem; line-height: 1; }
@keyframes waPulse{
  0%{ transform: scale(1); box-shadow: 0 18px 34px rgba(10,20,14,.24), 0 0 0 0 rgba(37,211,102,.36); }
  50%{ transform: scale(1.06); box-shadow: 0 18px 34px rgba(10,20,14,.24), 0 0 0 12px rgba(37,211,102,0); }
  100%{ transform: scale(1); box-shadow: 0 18px 34px rgba(10,20,14,.24), 0 0 0 0 rgba(37,211,102,0); }
}
@media (max-width: 768px){
  .wa-fab{ inset-inline-end: .95rem; bottom: .95rem; }
  .wa-fab__icon-wrap{ width: 3.5rem; height: 3.5rem; }
  .wa-fab__icon{ font-size: 1.8rem; }
  .wa-fab__label--visible{ padding: .72rem .95rem; font-size: .9rem; }
}

</style>
