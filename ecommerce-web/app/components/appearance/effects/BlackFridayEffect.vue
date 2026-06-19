<template>
  <div class="absolute inset-0 overflow-hidden">
    <div class="bf-soft-glow absolute inset-0" :class="themeClass"></div>

    <div class="absolute inset-0 overflow-hidden">
      <span
        v-for="n in percentCount"
        :key="n"
        class="bf-percent"
        :class="themeClass"
        :style="percentStyle(n)"
      >%</span>
    </div>
  </div>
</template>

<script setup lang="ts">
const ui = useUiStore()
const { liteMode } = useMobilePerf()

const isDarkTheme = computed(() => ui.theme === 'dark')
const themeClass = computed(() => (isDarkTheme.value ? 'is-dark' : 'is-light'))
const percentCount = computed(() => {
  if (isDarkTheme.value) return liteMode.value ? 10 : 26
  return liteMode.value ? 8 : 18
})

function percentStyle(n: number) {
  const x = (n * 37) % 100
  const s = 2 + ((n * 13) % 10)
  const d = (n * 7) % 20
  const drift = ((n * 9) % 18) - 9
  const delay = ((n * 3) % 12) * -0.55
  return {
    '--x': String(x),
    '--s': String(s),
    '--d': String(d),
    '--drift': String(drift),
    '--delay': `${delay}s`,
  } as any
}
</script>

<style scoped>
.bf-soft-glow{
  opacity: .95;
}
.bf-soft-glow.is-dark{
  background:
    radial-gradient(900px 460px at 12% 18%, rgba(255, 0, 70, .14), transparent 62%),
    radial-gradient(760px 380px at 86% 26%, rgba(126, 34, 206, .12), transparent 64%),
    radial-gradient(820px 420px at 50% 100%, rgba(255, 0, 70, .08), transparent 66%);
}
.bf-soft-glow.is-light{
  background:
    radial-gradient(820px 420px at 12% 18%, rgba(255, 64, 129, .10), transparent 62%),
    radial-gradient(760px 380px at 86% 26%, rgba(244, 114, 182, .08), transparent 64%),
    radial-gradient(760px 380px at 50% 100%, rgba(251, 113, 133, .06), transparent 66%);
}

.bf-percent{
  position: absolute;
  top: -28px;
  left: calc(var(--x) * 1%);
  font-weight: 900;
  font-size: calc(12px + var(--s) * 1px);
  animation: bf-fall calc(7s + var(--d) * 0.24s) linear infinite;
  animation-delay: var(--delay);
  transform: translate3d(0, -40px, 0);
  user-select: none;
}

.bf-percent.is-dark{
  color: rgba(255, 36, 98, .52);
  text-shadow:
    0 0 10px rgba(255, 0, 70, .34),
    0 0 24px rgba(168, 85, 247, .12);
}

.bf-percent.is-light{
  color: rgba(236, 72, 153, .28);
  text-shadow: 0 0 8px rgba(244, 114, 182, .16);
}

@keyframes bf-fall {
  0% {
    transform: translate3d(0, -60px, 0) rotate(0deg);
    opacity: 0;
  }
  12% {
    opacity: .85;
  }
  100% {
    transform: translate3d(calc(var(--drift) * 1px), 120vh, 0) rotate(90deg);
    opacity: 0;
  }
}
</style>
