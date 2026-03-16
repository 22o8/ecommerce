<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRuntimeConfig } from '#imports'

const cfg = useRuntimeConfig()
const phone = computed(() => String(cfg.public.whatsappNumber || '').replace(/\D/g, ''))

const showTop = ref(false)

function onScroll() {
  const scrollTop = window.scrollY || document.documentElement.scrollTop || 0
  const viewportHeight = window.innerHeight || document.documentElement.clientHeight || 0
  const fullHeight = Math.max(
    document.body.scrollHeight,
    document.documentElement.scrollHeight,
    document.body.offsetHeight,
    document.documentElement.offsetHeight
  )

  showTop.value = scrollTop + viewportHeight >= fullHeight - 220
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
      <span class="icon">💬</span>
      <span class="label">واتساب</span>
    </a>

    <button
      v-show="showTop"
      type="button"
      class="fab fab-top"
      @click="toTop"
      aria-label="Back to top"
      title="للأعلى"
    >
      <span class="icon icon-top">↑</span>
    </button>
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
  z-index: 80;
}

.fab{
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  border-radius: 999px;
  border: 1px solid rgb(var(--border));
  background: rgba(var(--panel), .96);
  box-shadow: 0 18px 38px rgba(0,0,0,.18);
  font-weight: 900;
  color: rgb(var(--text-strong));
  backdrop-filter: blur(14px);
  transition: transform .18s ease, box-shadow .18s ease, background .18s ease, opacity .18s ease;
}

.fab:hover{
  transform: translateY(-2px) scale(1.02);
  box-shadow: 0 22px 44px rgba(0,0,0,.24);
}

.fab-wa{
  padding: 14px 16px;
  border-color: rgba(34,197,94,.35);
}

.fab-top{
  width: 62px;
  height: 62px;
  padding: 0;
  border-color: rgba(var(--primary), .34);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .94));
}

.icon{ font-size: 18px; line-height: 1; }
.icon-top{ font-size: 30px; font-weight: 900; transform: translateY(-1px); }
.label{ font-size: 13px; }

:global(html.theme-light) .fab-top{
  background: rgba(255,255,255,.98);
}

@media (max-width: 640px){
  .floating{
    inset-inline-end: 14px;
    bottom: 14px;
    gap: 10px;
  }
  .fab-wa{
    padding: 13px;
  }
  .fab-top{
    width: 58px;
    height: 58px;
  }
  .icon-top{ font-size: 28px; }
  .label{ display: none; }
}
</style>
