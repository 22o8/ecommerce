<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

const wrapper = ref<HTMLElement | null>(null)
const pathMain = ref<SVGPathElement | null>(null)
const pathSpec = ref<SVGPathElement | null>(null)
const stopA = ref<SVGStopElement | null>(null)
const stopB = ref<SVGStopElement | null>(null)
const stopC = ref<SVGStopElement | null>(null)

let raf = 0
let start = 0

function lerp(a: number, b: number, t: number) {
  return a + (b - a) * t
}
function clamp01(v: number) {
  return Math.min(1, Math.max(0, v))
}
function smoothstep(t: number) {
  return t * t * (3 - 2 * t)
}
function interpHue(progress: number) {
  // Pink -> Purple -> Gold (smooth)
  const stops = [
    { p: 0.0, h: 330 },
    { p: 0.40, h: 285 },
    { p: 0.78, h: 45 },
    { p: 1.0, h: 45 },
  ]
  const p = clamp01(progress)
  for (let i = 0; i < stops.length - 1; i++) {
    const a = stops[i]
    const b = stops[i + 1]
    if (p >= a.p && p <= b.p) {
      const t = (p - a.p) / Math.max(0.0001, b.p - a.p)
      return lerp(a.h, b.h, smoothstep(t))
    }
  }
  return stops[stops.length - 1].h
}

function setStops(h: number, shimmer: number) {
  // rich silk-like gradient (slight hue offsets)
  const h1 = h
  const h2 = (h + 18) % 360
  const h3 = (h + 40) % 360

  // shimmer controls lightness/alpha
  const a = 0.32 + shimmer * 0.08
  const b = 0.22 + shimmer * 0.10
  const c = 0.14 + shimmer * 0.10

  if (stopA.value) stopA.value.setAttribute('stop-color', `hsla(${h1}, 92%, 72%, ${a.toFixed(3)})`)
  if (stopB.value) stopB.value.setAttribute('stop-color', `hsla(${h2}, 92%, 76%, ${b.toFixed(3)})`)
  if (stopC.value) stopC.value.setAttribute('stop-color', `hsla(${h3}, 92%, 82%, ${c.toFixed(3)})`)
}

function updatePath(progress: number, t: number) {
  const w = window.innerWidth
  const h = window.innerHeight

  // Fixed, full-viewport ribbon layer: keep SVG size stable and animate colors
  // based on scroll. This prevents section-limited backgrounds and avoids
  // visible hard edges.
  const H = h * 1.6

  // a gentle wave that shifts with time + scroll (no sharp edges)
  const amp = lerp(90, 140, progress)
  const phase = t * 0.0012 + progress * 1.6

  const x0 = -w * 0.15
  const x3 = w * 1.15
  const y0 = -h * 0.10
  const y3 = H + h * 0.15

  // Control points form a smooth S-curve
  const cx1 = w * 0.20 + Math.sin(phase) * amp
  const cy1 = H * 0.20 + Math.cos(phase * 0.9) * amp

  const cx2 = w * 0.80 + Math.cos(phase * 1.1) * amp
  const cy2 = H * 0.62 + Math.sin(phase * 1.0) * amp

  const d = `M ${x0.toFixed(1)} ${y0.toFixed(1)} C ${cx1.toFixed(1)} ${cy1.toFixed(1)}, ${cx2.toFixed(1)} ${cy2.toFixed(1)}, ${x3.toFixed(1)} ${y3.toFixed(1)}`

  // Specular highlight is a slightly offset copy
  const d2 = `M ${(x0 + 30).toFixed(1)} ${(y0 + 18).toFixed(1)} C ${(cx1 + 20).toFixed(1)} ${(cy1 + 10).toFixed(1)}, ${(cx2 + 14).toFixed(1)} ${(cy2 + 26).toFixed(1)}, ${(x3 - 12).toFixed(1)} ${(y3 + 8).toFixed(1)}`

  if (pathMain.value) pathMain.value.setAttribute('d', d)
  if (pathSpec.value) pathSpec.value.setAttribute('d', d2)

  // keep wrapper matching viewport so it covers the screen at all times
  if (wrapper.value) wrapper.value.style.height = `${h}px`
}

function loop(ts: number) {
  if (!start) start = ts
  const elapsed = ts - start

  const doc = document.documentElement
  const max = Math.max(1, doc.scrollHeight - doc.clientHeight)
  const p = clamp01(doc.scrollTop / max)

  const hue = interpHue(p)

  // shimmer: small breathing + scroll reactive
  const shimmer = 0.5 + 0.5 * Math.sin(elapsed * 0.0022)
  setStops(hue, shimmer)

  updatePath(p, elapsed)

  raf = requestAnimationFrame(loop)
}

onMounted(() => {
  raf = requestAnimationFrame(loop)
})

onBeforeUnmount(() => {
  cancelAnimationFrame(raf)
})
</script>

<template>
  <div ref="wrapper" class="wave-ribbon" aria-hidden="true">
    <svg class="wave-ribbon__svg" :width="'100%'" :height="'100%'" preserveAspectRatio="none">
      <defs>
        <linearGradient id="ribbonGrad" x1="0" y1="0" x2="1" y2="1">
          <stop ref="stopA" offset="0%" stop-color="hsla(330, 92%, 72%, .30)" />
          <stop ref="stopB" offset="55%" stop-color="hsla(285, 92%, 76%, .22)" />
          <stop ref="stopC" offset="100%" stop-color="hsla(45, 92%, 82%, .14)" />
        </linearGradient>

        <filter id="ribbonBlur" x="-40%" y="-40%" width="180%" height="180%">
          <feGaussianBlur stdDeviation="18" result="b" />
          <feColorMatrix
            in="b"
            type="matrix"
            values="
              1 0 0 0 0
              0 1 0 0 0
              0 0 1 0 0
              0 0 0 1 0"
          />
        </filter>

        <filter id="ribbonSpec" x="-40%" y="-40%" width="180%" height="180%">
          <feGaussianBlur stdDeviation="10" result="b" />
        </filter>

        <!-- Fade edges so no hard boundaries -->
        <mask id="ribbonMask">
          <rect width="100%" height="100%" fill="url(#fadeGrad)" />
        </mask>
        <radialGradient id="fadeGrad" cx="50%" cy="45%" r="75%">
          <stop offset="0%" stop-color="white" stop-opacity="1" />
          <stop offset="60%" stop-color="white" stop-opacity=".85" />
          <stop offset="100%" stop-color="black" stop-opacity="0" />
        </radialGradient>
      </defs>

      <g mask="url(#ribbonMask)">
        <path
          ref="pathMain"
          d="M -200 -100 C 300 200, 900 600, 1400 1200"
          fill="none"
          stroke="url(#ribbonGrad)"
          stroke-width="260"
          stroke-linecap="round"
          stroke-linejoin="round"
          filter="url(#ribbonBlur)"
          opacity="0.95"
        />
        <path
          ref="pathSpec"
          d="M -170 -70 C 320 220, 920 620, 1380 1230"
          fill="none"
          stroke="hsla(0,0%,100%,.20)"
          stroke-width="90"
          stroke-linecap="round"
          stroke-linejoin="round"
          filter="url(#ribbonSpec)"
          opacity="0.9"
        />
      </g>
    </svg>
  </div>
</template>

<style scoped>
.wave-ribbon{
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 0;
  /* keep it behind content but above page background */
  mix-blend-mode: screen;
}
.wave-ribbon__svg{
  position: absolute;
  inset: -18vh -14vw;
  width: 128vw;
  height: calc(100% + 36vh);
  overflow: visible;
}

:global(html.theme-light) .wave-ribbon{
  /* light theme: don't wash the UI, just a subtle silky tint */
  mix-blend-mode: normal;
  opacity: .18;
}
:global(html.theme-dark) .wave-ribbon{
  mix-blend-mode: screen;
  opacity: .95;
}

</style>
