<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRuntimeConfig } from '#imports'

const cfg = useRuntimeConfig()
const phone = computed(() => String(cfg.public.whatsappNumber || '').replace(/\D/g, ''))

const showTop = ref(false)

function onScroll() {
  const scrollTop = window.scrollY || document.documentElement.scrollTop || 0
  const viewportHeight = window.innerHeight || document.documentElement.clientHeight || 0
  const documentHeight = Math.max(
    document.body.scrollHeight,
    document.documentElement.scrollHeight,
    document.body.offsetHeight,
    document.documentElement.offsetHeight,
    document.body.clientHeight,
    document.documentElement.clientHeight,
  )

  const reachedBottomZone = scrollTop + viewportHeight >= documentHeight - 180
  showTop.value = reachedBottomZone
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
  z-index: 60;
}

.fab{
  display: inline-flex;
  align-items: center;
  gap: 10px;
  border-radius: 999px;
  padding: 12px 14px;
  border: 1px solid rgb(var(--border));
  background: rgba(var(--panel), .92);
  box-shadow: var(--shadow2);
  font-weight: 900;
  color: rgb(var(--text-strong));
  backdrop-filter: blur(14px);
  transition: transform .18s ease, box-shadow .18s ease, background .18s ease, opacity .18s ease;
}

.fab:hover{ transform: translateY(-2px) scale(1.02); box-shadow: var(--shadow1); }

.fab-wa{
  border-color: rgba(34,197,94,.35);
}

.fab-top{
  width: 64px;
  height: 64px;
  justify-content: center;
  padding: 0;
  border-width: 2px;
  border-color: rgba(var(--text-strong), .14);
  background: rgba(var(--panel), .98);
  box-shadow: 0 16px 36px rgba(0,0,0,.18);
}

.icon{ font-size: 16px; }
.icon-top{
  font-size: 34px;
  line-height: 1;
  font-weight: 900;
}
.label{ font-size: 13px; }

@media (max-width: 520px){
  .floating{
    inset-inline-end: 14px;
    bottom: 14px;
  }

  .label{ display: none; }
  .fab{ padding: 12px; }

  .fab-top{
    width: 58px;
    height: 58px;
    padding: 0;
  }

  .icon-top{
    font-size: 32px;
  }
}
</style>
