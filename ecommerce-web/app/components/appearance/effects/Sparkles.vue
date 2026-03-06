<template>
  <div class="w-full h-full overflow-hidden">
    <span
      v-for="s in sparkles"
      :key="s.id"
      class="spark"
      :style="{
        left: s.x + '%',
        top: s.y + '%',
        animationDelay: s.delay + 's',
        animationDuration: s.dur + 's',
        '--s': s.scale,
      }"
    />
  </div>
</template>

<script setup lang="ts">
const sparkles = computed(() => {
  const count = 42
  return Array.from({ length: count }).map((_, i) => ({
    id: i,
    x: Math.random() * 100,
    y: Math.random() * 100,
    delay: Math.random() * 3.5,
    dur: 2.4 + Math.random() * 3.2,
    scale: 0.7 + Math.random() * 0.9,
  }))
})
</script>

<style scoped>
.spark {
  position: absolute;
  width: 6px;
  height: 6px;
  border-radius: 999px;
  background: radial-gradient(
    circle,
    rgba(var(--spark-rgb), 0.95),
    rgba(var(--spark-rgb), 0)
  );
  opacity: 0;
  animation-name: twinkle;
  animation-timing-function: ease-in-out;
  animation-iteration-count: infinite;
}
@keyframes twinkle {
  0%, 100% { opacity: 0; filter: blur(0px); transform: translate3d(0,0,0) scale(var(--s,1)); }
  50% { opacity: 0.95; filter: blur(0.8px); transform: translate3d(0,-6px,0) scale(var(--s,1)); }
}
</style>
