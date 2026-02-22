<template>
  <div class="sparkles-layer" aria-hidden="true">
    <div ref="container" class="sparkles-container" />
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import lottie, { type AnimationItem } from 'lottie-web'

const container = ref<HTMLDivElement | null>(null)
let anim: AnimationItem | null = null

onMounted(() => {
  // Respect reduced motion
  if (typeof window !== 'undefined') {
    const reduce = window.matchMedia?.('(prefers-reduced-motion: reduce)')
    if (reduce?.matches) return
  }
  if (!container.value) return

  anim = lottie.loadAnimation({
    container: container.value,
    renderer: 'svg',
    loop: true,
    autoplay: true,
    path: '/lottie/background-sparkles.json',
    rendererSettings: {
      preserveAspectRatio: 'xMidYMid slice',
      progressiveLoad: true,
    },
  })

  // Slightly slower, more elegant
  anim.setSpeed(0.85)
})

onBeforeUnmount(() => {
  anim?.destroy()
  anim = null
})
</script>

<style scoped>
.sparkles-layer {
  position: fixed;
  inset: 0;
  z-index: 0;
  pointer-events: none;
  /* Keep it subtle across both themes */
  opacity: 0.42;
  mix-blend-mode: screen;
}

.sparkles-container {
  width: 100%;
  height: 100%;
  filter: blur(0.2px);
}

/* On light theme, reduce contrast so it doesn't look noisy */
:global(.theme-light) .sparkles-layer {
  opacity: 0.22;
  mix-blend-mode: multiply;
}
</style>
