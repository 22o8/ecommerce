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
        transform: `scale(${s.scale})`,
      }"
    />
  </div>
</template>

<script setup lang="ts">
const sparkles = computed(() => {
  const count = 26
  return Array.from({ length: count }).map((_, i) => ({
    id: i,
    x: Math.random() * 100,
    y: Math.random() * 100,
    delay: Math.random() * 4,
    dur: 2.8 + Math.random() * 3.5,
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
  background: radial-gradient(circle, rgba(255,255,255,0.95), rgba(255,255,255,0));
  opacity: 0;
  animation-name: twinkle;
  animation-timing-function: ease-in-out;
  animation-iteration-count: infinite;
}
@keyframes twinkle {
  0%, 100% { opacity: 0; filter: blur(0px); }
  50% { opacity: 0.9; filter: blur(0.6px); }
}
</style>
