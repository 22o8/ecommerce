<template>
  <Transition name="scroll-top-fade">
    <button
      v-if="isVisible"
      type="button"
      class="scroll-top-floating"
      :aria-label="t('backToTop')"
      @click="scrollToTop"
    >
      <Icon name="mdi:arrow-up" class="scroll-top-floating__icon" />
    </button>
  </Transition>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

const { t } = useI18n()
const isVisible = ref(false)

function updateVisibility() {
  if (!import.meta.client) return
  const scrollTop = window.scrollY || document.documentElement.scrollTop || 0
  const viewportHeight = window.innerHeight || 0
  const docHeight = Math.max(
    document.body.scrollHeight,
    document.documentElement.scrollHeight,
    document.body.offsetHeight,
    document.documentElement.offsetHeight,
    document.body.clientHeight,
    document.documentElement.clientHeight,
  )

  const nearBottom = scrollTop + viewportHeight >= docHeight - Math.max(220, viewportHeight * 0.12)
  isVisible.value = scrollTop > 160 && nearBottom
}

function scrollToTop() {
  if (!import.meta.client) return
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => {
  updateVisibility()
  window.addEventListener('scroll', updateVisibility, { passive: true })
  window.addEventListener('resize', updateVisibility, { passive: true })
})

onBeforeUnmount(() => {
  if (!import.meta.client) return
  window.removeEventListener('scroll', updateVisibility)
  window.removeEventListener('resize', updateVisibility)
})
</script>

<style scoped>
.scroll-top-floating{
  position: fixed;
  inset-inline-end: clamp(14px, 2vw, 28px);
  bottom: max(18px, env(safe-area-inset-bottom, 0px) + 14px);
  z-index: 90;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 62px;
  height: 62px;
  border: 1px solid rgba(var(--border), .9);
  border-radius: 999px;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-rgb), .9));
  color: rgb(var(--text));
  box-shadow: 0 20px 48px rgba(0, 0, 0, .20), 0 0 0 8px rgba(var(--primary), .08);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  transition: transform .2s ease, box-shadow .2s ease, opacity .2s ease;
}
.scroll-top-floating:hover{
  transform: translateY(-3px) scale(1.03);
  box-shadow: 0 26px 54px rgba(0, 0, 0, .24), 0 0 0 10px rgba(var(--primary), .10);
}
.scroll-top-floating:active{
  transform: translateY(-1px) scale(.98);
}
.scroll-top-floating__icon{
  font-size: 2rem;
}
.scroll-top-fade-enter-active,
.scroll-top-fade-leave-active{
  transition: opacity .22s ease, transform .22s ease;
}
.scroll-top-fade-enter-from,
.scroll-top-fade-leave-to{
  opacity: 0;
  transform: translateY(12px) scale(.9);
}
:global(html.theme-dark) .scroll-top-floating{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .94));
  box-shadow: 0 18px 52px rgba(0,0,0,.42), 0 0 0 8px rgba(var(--primary), .10);
}
@media (max-width: 640px){
  .scroll-top-floating{
    width: 68px;
    height: 68px;
    inset-inline-end: 14px;
    bottom: max(14px, env(safe-area-inset-bottom, 0px) + 10px);
  }
  .scroll-top-floating__icon{
    font-size: 2.15rem;
  }
}
</style>
