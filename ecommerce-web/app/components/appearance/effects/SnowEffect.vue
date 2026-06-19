<template>
  <canvas ref="c" class="h-full w-full" />
</template>

<script setup lang="ts">
const c = ref<HTMLCanvasElement | null>(null)
const { liteMode } = useMobilePerf()

type Flake = { x: number; y: number; r: number; v: number; d: number }
let raf = 0
let flakes: Flake[] = []
let ro: ResizeObserver | null = null

function getBox() {
  const canvas = c.value
  const host = canvas?.parentElement
  return host?.getBoundingClientRect() || { width: window.innerWidth, height: 360 }
}

function resize() {
  const canvas = c.value
  if (!canvas) return
  const box = getBox()
  const dpr = Math.max(1, window.devicePixelRatio || 1)
  canvas.width = Math.floor(box.width * dpr)
  canvas.height = Math.floor(box.height * dpr)
  canvas.style.width = box.width + 'px'
  canvas.style.height = box.height + 'px'
  const count = liteMode.value ? Math.min(18, Math.floor(box.width / 34)) : Math.min(40, Math.floor(box.width / 22))
  flakes = Array.from({ length: count }).map(() => ({
    x: Math.random() * canvas.width,
    y: Math.random() * canvas.height,
    r: (Math.random() * 1.4 + 0.7) * dpr,
    v: (Math.random() * 0.65 + 0.22) * dpr,
    d: Math.random() * Math.PI * 2,
  }))
}

function draw() {
  const canvas = c.value
  if (!canvas) return
  const ctx = canvas.getContext('2d')
  if (!ctx) return
  ctx.clearRect(0, 0, canvas.width, canvas.height)
  const rgb = getComputedStyle(document.documentElement).getPropertyValue('--snow-rgb').trim() || '255,255,255'
  const isLight = document.documentElement.classList.contains('theme-light')
  ctx.globalAlpha = isLight ? 0.38 : 0.5
  ctx.fillStyle = `rgb(${rgb})`
  ctx.shadowColor = isLight ? `rgba(${rgb},0.18)` : `rgba(${rgb},0.10)`
  ctx.shadowBlur = isLight ? 5 : 2
  for (const f of flakes) {
    ctx.beginPath()
    ctx.arc(f.x, f.y, f.r, 0, Math.PI * 2)
    ctx.fill()
  }
  ctx.globalAlpha = 1
  ctx.shadowBlur = 0
  const w = canvas.width
  const h = canvas.height
  for (const f of flakes) {
    f.y += f.v
    f.x += Math.sin(f.d) * 0.32
    f.d += 0.008
    if (f.y > h + 8) { f.y = -8; f.x = Math.random() * w }
  }
  raf = requestAnimationFrame(draw)
}

onMounted(() => {
  resize()
  const host = c.value?.parentElement
  if (host && 'ResizeObserver' in window) {
    ro = new ResizeObserver(() => resize())
    ro.observe(host)
  }
  window.addEventListener('resize', resize)
  raf = requestAnimationFrame(draw)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', resize)
  ro?.disconnect()
  cancelAnimationFrame(raf)
})
</script>
