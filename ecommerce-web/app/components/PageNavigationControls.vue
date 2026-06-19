<template>
  <nav class="page-nav-controls" aria-label="اختصارات التنقل">
    <Transition name="page-nav-pop">
      <button
        v-if="canGoBack"
        type="button"
        class="page-nav-btn page-nav-btn--back"
        @click="goBack"
        title="رجوع للصفحة السابقة"
        aria-label="رجوع للصفحة السابقة"
      >
        <Icon :name="isRtl ? 'mdi:arrow-right' : 'mdi:arrow-left'" class="page-nav-icon" />
        <span class="page-nav-label">رجوع</span>
      </button>
    </Transition>

    <Transition name="page-nav-pop">
      <button
        v-if="showTop"
        type="button"
        class="page-nav-btn page-nav-btn--top"
        @click="scrollToTop"
        title="العودة إلى بداية الصفحة"
        aria-label="العودة إلى بداية الصفحة"
      >
        <Icon name="mdi:arrow-up" class="page-nav-icon" />
      </button>
    </Transition>
  </nav>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from '#imports'

const router = useRouter()
const route = useRoute()
const showTop = ref(false)
const canGoBack = ref(false)
const isRtl = computed(() => true)

function updateBackState() {
  if (!import.meta.client) return
  canGoBack.value = window.history.length > 1 && route.path !== '/'
}

function handleScroll() {
  if (!import.meta.client) return

  const scrollTop = window.scrollY || document.documentElement.scrollTop || 0
  const docHeight = Math.max(
    document.body.scrollHeight,
    document.documentElement.scrollHeight,
    document.body.offsetHeight,
    document.documentElement.offsetHeight
  )
  const viewport = window.innerHeight || document.documentElement.clientHeight || 0
  const distanceToBottom = docHeight - (scrollTop + viewport)
  const longPage = docHeight > viewport + 220

  // يظهر سهم الصعود بمجرد نزول المستخدم مسافة واضحة، وليس فقط عند نهاية الصفحة.
  showTop.value = longPage && scrollTop > 360
}

function goBack() {
  if (!import.meta.client) return
  if (window.history.length > 1) router.back()
  else router.push('/')
}

function scrollToTop() {
  if (!import.meta.client) return
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

watch(() => route.fullPath, () => {
  updateBackState()
  setTimeout(handleScroll, 80)
})

onMounted(() => {
  updateBackState()
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
.page-nav-controls{
  position: fixed;
  inset-inline-start: 1.05rem;
  bottom: 1.05rem;
  z-index: 80;
  display: flex;
  align-items: center;
  gap: .65rem;
  pointer-events: none;
}
.page-nav-btn{
  pointer-events: auto;
  border: 1px solid rgba(var(--border), .78);
  color: rgb(var(--text-strong));
  background:
    linear-gradient(145deg, rgba(var(--surface), .92), rgba(var(--surface-2), .78));
  box-shadow: 0 20px 44px rgba(0,0,0,.18), inset 0 1px 0 rgba(255,255,255,.08);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  min-height: 3.05rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: .45rem;
  cursor: pointer;
  transition: transform .18s ease, border-color .18s ease, box-shadow .18s ease, background .18s ease;
}
.page-nav-btn:hover{
  transform: translateY(-2px);
  border-color: rgba(var(--primary), .58);
  box-shadow: 0 24px 54px rgba(0,0,0,.22), 0 0 0 5px rgba(var(--primary), .08);
}
.page-nav-btn:active{ transform: translateY(0) scale(.97); }
.page-nav-btn--back{ padding: 0 1rem; font-weight: 900; }
.page-nav-btn--top{
  width: 3.05rem;
  background:
    radial-gradient(circle at 30% 22%, rgba(255,255,255,.22), transparent 28%),
    linear-gradient(135deg, rgba(var(--primary), .98), rgba(var(--primary), .72));
  color: #fff;
}
.page-nav-icon{ font-size: 1.28rem; }
.page-nav-label{ font-size: .9rem; }
.page-nav-pop-enter-active,.page-nav-pop-leave-active{ transition: opacity .2s ease, transform .2s ease; }
.page-nav-pop-enter-from,.page-nav-pop-leave-to{ opacity: 0; transform: translateY(10px) scale(.94); }
@media (max-width: 768px){
  .page-nav-controls{
    inset-inline-start: .85rem;
    bottom: .85rem;
    gap: .5rem;
  }
  .page-nav-btn{ min-height: 2.8rem; }
  .page-nav-btn--back{ padding: 0 .82rem; }
  .page-nav-btn--top{ width: 2.8rem; }
  .page-nav-label{ font-size: .82rem; }
}
</style>
