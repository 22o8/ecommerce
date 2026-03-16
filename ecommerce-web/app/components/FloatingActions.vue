<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRuntimeConfig } from '#imports'

const cfg = useRuntimeConfig()
const phone = computed(() => String(cfg.public.whatsappNumber || '').replace(/\D/g, ''))

const showTop = ref(false)
const scrollProgress = ref(0)

function onScroll() {
  const scrollTop = window.scrollY || window.pageYOffset || 0
  const doc = document.documentElement
  const scrollHeight = Math.max(doc.scrollHeight, document.body.scrollHeight)
  const viewport = window.innerHeight
  const maxScrollable = Math.max(scrollHeight - viewport, 1)

  scrollProgress.value = Math.min(100, Math.max(0, (scrollTop / maxScrollable) * 100))

  const nearBottom = scrollTop + viewport >= scrollHeight - 180
  showTop.value = nearBottom
}

function toTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const waLink = computed(() => {
  const p = phone.value
  if (!p) return '#'
  const text = encodeURIComponent('مرحبا، أريد الاستفسار عن المنتجات/الخدمات')
  return `https://wa.me/${p}?text=${text}`
})

const progressStyle = computed(() => ({
  background: `conic-gradient(rgba(var(--brand-primary), 1) ${scrollProgress.value}%, rgba(var(--border), .35) ${scrollProgress.value}% 100%)`,
}))

onMounted(() => {
  onScroll()
  window.addEventListener('scroll', onScroll, { passive: true })
  window.addEventListener('resize', onScroll, { passive: true })
})

onUnmounted(() => {
  window.removeEventListener('scroll', onScroll)
  window.removeEventListener('resize', onScroll)
})
</script>

<template>
  <div class="floating keep-ltr">
    <a
      v-if="phone"
      :href="waLink"
      target="_blank"
      rel="noopener"
      class="fab fab-wa"
      aria-label="WhatsApp"
      title="WhatsApp"
    >
      <span class="fab-wa__icon">
        <Icon name="mdi:whatsapp" />
      </span>
      <span class="label">واتساب</span>
    </a>

    <Transition name="float-reveal">
      <button
        v-if="showTop"
        type="button"
        class="top-wrap"
        @click="toTop"
        aria-label="Back to top"
        title="للأعلى"
      >
        <span class="progress-ring" :style="progressStyle"></span>
        <span class="fab fab-top">
          <Icon name="mdi:arrow-up" class="top-icon" />
        </span>
      </button>
    </Transition>
  </div>
</template>

<style scoped>
.floating{
  position: fixed;
  inset-inline-end: 18px;
  bottom: 18px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  z-index: 70;
}

.fab,
.top-wrap{
  -webkit-tap-highlight-color: transparent;
}

.fab{
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  border-radius: 999px;
  border: 1px solid rgba(var(--border), .9);
  background: rgba(var(--panel), .94);
  color: rgb(var(--text-strong));
  backdrop-filter: blur(16px);
  box-shadow: 0 18px 50px rgba(15, 23, 42, .16), 0 6px 18px rgba(15, 23, 42, .08);
  transition: transform .2s ease, box-shadow .2s ease, background .2s ease, border-color .2s ease;
}

.fab:hover{
  transform: translateY(-2px) scale(1.02);
  box-shadow: 0 22px 55px rgba(15, 23, 42, .2), 0 8px 20px rgba(15, 23, 42, .1);
}

.fab-wa{
  min-height: 58px;
  padding: 12px 16px 12px 12px;
  border-color: rgba(34, 197, 94, .24);
}

.fab-wa__icon{
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 999px;
  background: rgba(34, 197, 94, .14);
  color: rgb(22, 163, 74);
  font-size: 22px;
}

.label{
  font-size: 13px;
  font-weight: 900;
}

.top-wrap{
  position: relative;
  width: 72px;
  height: 72px;
  border: 0;
  background: transparent;
  padding: 0;
  cursor: pointer;
}

.progress-ring{
  position: absolute;
  inset: 0;
  border-radius: 999px;
  box-shadow: 0 16px 44px rgba(15, 23, 42, .16);
}

.progress-ring::after{
  content: '';
  position: absolute;
  inset: 4px;
  border-radius: 999px;
  background: rgba(var(--surface), .88);
  border: 1px solid rgba(var(--border), .8);
  backdrop-filter: blur(14px);
}

.fab-top{
  position: absolute;
  inset: 8px;
  width: auto;
  height: auto;
  border-radius: 999px;
  background: linear-gradient(135deg, rgba(var(--brand-primary), .18), rgba(var(--brand-primary), .08));
  border-color: rgba(var(--brand-primary), .22);
}

.top-icon{
  position: relative;
  z-index: 1;
  font-size: 34px;
  color: rgb(var(--brand-primary));
}

.float-reveal-enter-active,
.float-reveal-leave-active{
  transition: opacity .26s ease, transform .26s ease;
}

.float-reveal-enter-from,
.float-reveal-leave-to{
  opacity: 0;
  transform: translateY(10px) scale(.92);
}

@media (max-width: 768px){
  .floating{
    inset-inline-end: 14px;
    bottom: 14px;
    gap: 10px;
  }

  .fab-wa{
    min-height: 54px;
    padding: 11px 14px 11px 11px;
  }

  .top-wrap{
    width: 68px;
    height: 68px;
  }

  .top-icon{
    font-size: 32px;
  }
}

@media (max-width: 520px){
  .label{ display: none; }
  .fab-wa{
    width: 56px;
    min-height: 56px;
    padding: 0;
  }

  .fab-wa__icon{
    width: 100%;
    height: 100%;
    background: transparent;
    font-size: 26px;
  }

  .top-wrap{
    width: 64px;
    height: 64px;
  }

  .progress-ring::after{ inset: 4px; }
  .fab-top{ inset: 8px; }
  .top-icon{ font-size: 30px; }
}
</style>
