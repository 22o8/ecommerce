<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

const el = ref<HTMLElement | null>(null)

function clamp01(v: number) {
  return Math.min(1, Math.max(0, v))
}

function lerp(a: number, b: number, t: number) {
  return a + (b - a) * t
}

function mixRGB(a: [number, number, number], b: [number, number, number], t: number): [number, number, number] {
  return [
    Math.round(lerp(a[0], b[0], t)),
    Math.round(lerp(a[1], b[1], t)),
    Math.round(lerp(a[2], b[2], t)),
  ]
}

function setRGBVar(name: string, rgb: [number, number, number]) {
  if (!el.value) return
  // Space-separated RGB numbers, used as: rgb(var(--ps-c1) / alpha)
  el.value.style.setProperty(name, `${rgb[0]} ${rgb[1]} ${rgb[2]}`)
}

function updateFromScroll() {
  const doc = document.documentElement
  const max = Math.max(1, doc.scrollHeight - doc.clientHeight)
  const p = clamp01(doc.scrollTop / max)

  // Soft palette: Pink -> Purple -> Gold
  const PINK: [number, number, number] = [255, 94, 176]
  const PURPLE: [number, number, number] = [166, 132, 255]
  const GOLD: [number, number, number] = [255, 205, 120]

  let c1: [number, number, number]
  let c2: [number, number, number]
  let c3: [number, number, number]

  if (p < 0.55) {
    const t = p / 0.55
    c1 = mixRGB(PINK, PURPLE, t)
    c2 = mixRGB(PINK, PURPLE, clamp01(t * 0.75))
    c3 = mixRGB(PURPLE, PURPLE, 0)
  } else {
    const t = (p - 0.55) / 0.45
    c1 = mixRGB(PURPLE, GOLD, t)
    c2 = mixRGB(PURPLE, GOLD, clamp01(t * 0.65))
    c3 = mixRGB(GOLD, GOLD, 0)
  }

  setRGBVar('--ps-c1', c1)
  setRGBVar('--ps-c2', c2)
  setRGBVar('--ps-c3', c3)

  // Very subtle parallax drift with scroll (keeps it alive but not distracting)
  if (el.value) {
    const y = Math.round(p * 120)
    el.value.style.setProperty('--ps-shift', `${y}px`)
  }
}

let raf = 0
function onScroll() {
  cancelAnimationFrame(raf)
  raf = requestAnimationFrame(updateFromScroll)
}

onMounted(() => {
  updateFromScroll()
  window.addEventListener('scroll', onScroll, { passive: true })
  window.addEventListener('resize', onScroll, { passive: true })
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', onScroll)
  window.removeEventListener('resize', onScroll)
  cancelAnimationFrame(raf)
})
</script>

<template>
  <div ref="el" class="paper-silk" aria-hidden="true" />
</template>

<style scoped>
.paper-silk{
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 0;
  /* Default vars */
  --ps-c1: 255 94 176;
  --ps-c2: 166 132 255;
  --ps-c3: 255 205 120;
  --ps-shift: 0px;
  /* intensity tuned per theme below */
  --ps-alpha: .18;
  --ps-texture: .12;
}

/*
  Layering strategy:
  - ::before = paper/silk texture (static)
  - ::after  = scroll-tinted soft color wash
  Both cover the entire page without any hard edges.
*/
.paper-silk::before{
  content: "";
  position: absolute;
  inset: -10%;
  /* Paper/Silk feel: micro fibers + soft folds */
  background-image:
    repeating-linear-gradient(
      0deg,
      rgba(255,255,255,.018) 0px,
      rgba(255,255,255,.018) 1px,
      rgba(0,0,0,0) 6px,
      rgba(0,0,0,0) 12px
    ),
    repeating-linear-gradient(
      90deg,
      rgba(255,255,255,.012) 0px,
      rgba(255,255,255,.012) 1px,
      rgba(0,0,0,0) 7px,
      rgba(0,0,0,0) 14px
    ),
    radial-gradient(1200px 800px at 20% 10%, rgba(255,255,255,.08), rgba(0,0,0,0) 60%),
    radial-gradient(1100px 900px at 80% 30%, rgba(255,255,255,.06), rgba(0,0,0,0) 62%),
    radial-gradient(900px 700px at 40% 85%, rgba(255,255,255,.05), rgba(0,0,0,0) 60%);
  transform: translate3d(0, calc(var(--ps-shift) * -0.15), 0);
  opacity: var(--ps-texture);
  filter: blur(0.2px);
  mix-blend-mode: overlay;
}

.paper-silk::after{
  content: "";
  position: absolute;
  inset: -18%;
  background-image:
    radial-gradient(900px 650px at 18% 18%, rgb(var(--ps-c1) / var(--ps-alpha)), transparent 62%),
    radial-gradient(1000px 700px at 78% 30%, rgb(var(--ps-c2) / calc(var(--ps-alpha) * .85)), transparent 66%),
    radial-gradient(950px 650px at 55% 82%, rgb(var(--ps-c3) / calc(var(--ps-alpha) * .75)), transparent 68%),
    linear-gradient(180deg, rgb(var(--ps-c1) / calc(var(--ps-alpha) * .35)) 0%, transparent 35%, rgb(var(--ps-c3) / calc(var(--ps-alpha) * .28)) 100%);
  transform: translate3d(0, calc(var(--ps-shift) * -0.35), 0);
  filter: blur(14px);
  opacity: 1;
}

:global(html.theme-dark) .paper-silk{
  /* Dark: slightly richer but still “خفيف” */
  --ps-alpha: .18;
  --ps-texture: .13;
  mix-blend-mode: screen;
}

:global(html.theme-light) .paper-silk{
  /* Light: much softer to avoid washing out */
  --ps-alpha: .10;
  --ps-texture: .08;
  mix-blend-mode: normal;
}
</style>
