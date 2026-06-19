<template>
  <div
    ref="el"
    class="reveal"
    :class="{ 'reveal--in': inView }"
    :data-parity="parity"
    :style="style"
  >
    <slot />
  </div>
</template>

<script setup lang="ts">
// Nuxt auto-imports `ref`, `computed`, `onMounted`.
const props = defineProps<{ parity?: 0 | 1; delay?: number }>()

const parity = computed(() => (props.parity ?? 0) as 0 | 1)
const style = computed(() => {
  const d = Number(props.delay ?? 0)
  return d > 0 ? { transitionDelay: `${d}ms` } : undefined
})

const el = ref<HTMLElement | null>(null)
const inView = ref(false)

onMounted(() => {
  if (!el.value) return

  const obs = new IntersectionObserver(
    (entries) => {
      for (const e of entries) {
        if (e.isIntersecting) {
          inView.value = true
          obs.disconnect()
          break
        }
      }
    },
    { threshold: 0.15, rootMargin: '0px 0px -10% 0px' }
  )

  obs.observe(el.value)
})
</script>

<style scoped>
/* styles live in global css (main.css) */
</style>
