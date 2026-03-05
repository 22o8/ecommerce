<template>
  <div class="absolute inset-0">
    <!-- Dark mode: neon + red glow -->
    <div class="hidden dark:block absolute inset-0">
      <div class="bf-neon absolute inset-0"></div>
      <div class="bf-glow absolute inset-0"></div>

      <!-- % particles -->
      <div class="absolute inset-0 overflow-hidden">
        <span v-for="n in percentCount" :key="n" class="bf-percent" :style="percentStyle(n)">%</span>
      </div>
    </div>

    <!-- Light mode: confetti + soft gradients -->
    <div class="block dark:hidden absolute inset-0">
      <div class="bf-light-grad absolute inset-0"></div>
      <div class="absolute inset-0 overflow-hidden">
        <span v-for="n in confettiCount" :key="n" class="bf-confetti" :style="confettiStyle(n)"></span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const isDark = useDark()

// رفع “لفل” التأثير (أكثر عناصر + توزيع أجمل)
const percentCount = computed(() => (isDark.value ? 28 : 0))
const confettiCount = computed(() => (isDark.value ? 0 : 44))

function clamp(n: number, min: number, max: number) {
  return Math.max(min, Math.min(max, n))
}

function percentStyle(n: number) {
  const x = (n * 37) % 100
  const s = 1 + ((n * 13) % 11) // 1..11
  const d = (n * 7) % 20
  return { '--x': String(x), '--s': String(s), '--d': String(d) } as any
}

function confettiStyle(n: number) {
  const x = (n * 23) % 100
  const w = 1 + ((n * 11) % 5)
  const h = 4 + ((n * 17) % 6)
  const r = (n * 29) % 180
  const d = (n * 5) % 30
  // خليه متوازن حتى ما يصير ازدحام
  return { '--x': String(x), '--w': String(clamp(w, 1, 5)), '--h': String(clamp(h, 4, 10)), '--r': String(r), '--d': String(d) } as any
}
</script>

<style scoped>
.bf-neon{
  background:
    repeating-linear-gradient(135deg, rgba(255,0,70,.10) 0 2px, transparent 2px 18px),
    radial-gradient(900px 500px at 10% 20%, rgba(255,0,70,.16), transparent 60%),
    radial-gradient(700px 420px at 80% 70%, rgba(255,0,70,.12), transparent 60%);
  opacity: .95;
  animation: bf-neon-move 8s ease-in-out infinite;
}
.bf-glow{
  box-shadow: inset 0 0 160px rgba(255,0,70,.20);
}

@keyframes bf-neon-move{
  0%{ transform: translate3d(0,0,0) scale(1); }
  50%{ transform: translate3d(-8px,6px,0) scale(1.02); }
  100%{ transform: translate3d(0,0,0) scale(1); }
}

.bf-percent{
  position:absolute;
  top: -20px;
  left: calc(var(--x) * 1%);
  font-weight: 900;
  font-size: calc(14px + var(--s) * 1px);
  color: rgba(255,0,70,.55);
  text-shadow: 0 0 12px rgba(255,0,70,.45);
  animation: bf-fall calc(7s + var(--d) * 0.2s) linear infinite;
  transform: translateY(-40px);
}

/* توزيع ثابت عبر nth-child */
/* التوزيع صار عبر style vars */

@keyframes bf-fall{
  0%{ transform: translateY(-60px) rotate(0deg); opacity: .1; }
  10%{ opacity: .8; }
  100%{ transform: translateY(120vh) rotate(90deg); opacity: .0; }
}

.bf-light-grad{
  background:
    radial-gradient(800px 450px at 20% 10%, rgba(255,100,120,.18), transparent 60%),
    radial-gradient(700px 420px at 80% 70%, rgba(255,200,80,.14), transparent 60%),
    linear-gradient(180deg, rgba(255,255,255,.0), rgba(255,255,255,.0));
}

.bf-confetti{
  position:absolute;
  top:-10px;
  left: calc(var(--x) * 1%);
  width: calc(4px + var(--w) * 1px);
  height: calc(8px + var(--h) * 1px);
  background: rgba(255,0,70,.35);
  border-radius: 2px;
  transform: rotate(calc(var(--r) * 1deg));
  animation: confetti-fall calc(6s + var(--d) * .25s) linear infinite;
  opacity:.85;
}

/* التوزيع صار عبر style vars */

@keyframes confetti-fall{
  0%{ transform: translateY(-20px) rotate(0deg); opacity: .0; }
  10%{ opacity: .9; }
  100%{ transform: translateY(120vh) rotate(180deg); opacity: .0; }
}
</style>
