import { computed, ref, onMounted, onBeforeUnmount } from 'vue'

export function useMobilePerf() {
  const width = ref(1280)
  const reducedMotion = ref(false)
  const saveData = ref(false)
  const coarse = ref(false)

  function readFlags() {
    if (!import.meta.client) return
    width.value = window.innerWidth || 1280
    reducedMotion.value = !!window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches
    coarse.value = !!window.matchMedia?.('(pointer: coarse)')?.matches
    saveData.value = !!(navigator as any)?.connection?.saveData
  }

  onMounted(() => {
    readFlags()
    window.addEventListener('resize', readFlags, { passive: true })
  })

  onBeforeUnmount(() => {
    if (!import.meta.client) return
    window.removeEventListener('resize', readFlags)
  })

  const isMobile = computed(() => width.value < 768)
  const isTablet = computed(() => width.value >= 768 && width.value < 1024)
  const liteMode = computed(() => isMobile.value || saveData.value || reducedMotion.value)

  return {
    width,
    isMobile,
    isTablet,
    reducedMotion,
    saveData,
    coarse,
    liteMode,
  }
}
