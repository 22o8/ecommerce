<template>
  <div class="absolute inset-0 overflow-hidden">
    <span
      v-for="h in hearts"
      :key="h.id"
      class="heart"
      :style="{
        left: h.x + '%',
        animationDelay: h.delay + 's',
        animationDuration: h.dur + 's',
        opacity: h.opacity,
        transform: `scale(${h.scale})`,
      }"
    >♥</span>
  </div>
</template>

<script setup lang="ts">
const hearts = computed(() => {
  const count = 14
  return Array.from({ length: count }).map((_, i) => ({
    id: i,
    x: Math.random() * 100,
    delay: Math.random() * 4,
    dur: 8 + Math.random() * 6,
    scale: 0.7 + Math.random() * 0.9,
    opacity: 0.25 + Math.random() * 0.35,
  }))
})
</script>

<style scoped>
.heart {
  position: absolute;
  bottom: -40px;
  font-size: 22px;
  color: rgba(255, 105, 180, 0.7);
  animation: rise linear infinite;
}
@keyframes rise {
  0% { transform: translateY(0); }
  100% { transform: translateY(-120vh); }
}
</style>
