<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRuntimeConfig } from '#imports'
const { t } = useI18n()

const cfg = useRuntimeConfig()
const phone = computed(() => String(cfg.public.whatsappNumber || '').replace(/\D/g, ''))

const showTop = ref(false)
function onScroll() {
  showTop.value = window.scrollY > 500
}

function toTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const waLink = computed(() => {
  const p = phone.value
  if (!p) return '#'
  const text = encodeURIComponent(t('whatsapp.floatingInquiry'))
  return `https://wa.me/${p}?text=${text}`
})

onMounted(() => {
  onScroll()
  window.addEventListener('scroll', onScroll, { passive: true })
})
onUnmounted(() => window.removeEventListener('scroll', onScroll))
</script>

<template>
  <div class="floating keep-ltr">
    <a
      v-if="phone"
      :href="waLink"
      target="_blank"
      rel="noopener"
      class="fab fab-wa"
       :aria-label="t('common.whatsapp')"
      :title="t('common.whatsapp')"
    >
      <span class="icon">💬</span>
      <span class="label">{{ t('whatsapp.floatingLabel') }}</span>
    </a>

    <button
      v-show="showTop"
      type="button"
      class="fab fab-top"
      @click="toTop"
       :aria-label="t('common.backToTop')"
      :title="t('common.backToTop')"
    >
      <span class="icon">↑</span>
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
  gap: 10px;
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
  transition: transform .18s ease, box-shadow .18s ease, background .18s ease;
}

.fab:hover{ transform: translateY(-1px); box-shadow: var(--shadow1); }

.fab-wa{
  border-color: rgba(34,197,94,.35);
}

.fab-top{
  width: 44px;
  height: 44px;
  justify-content: center;
}

.icon{ font-size: 16px; }
.label{ font-size: 13px; }

@media (max-width: 520px){
  .label{ display: none; }
  .fab{ padding: 12px; }
}
</style>
