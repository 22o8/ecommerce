<template>
  <canvas ref="c" class="w-full h-full" />
</template>

<script setup lang="ts">
const c = ref<HTMLCanvasElement | null>(null)
const { liteMode } = useMobilePerf()

type Flake = { x: number; y: number; r: number; v: number; d: number }
let raf = 0
let flakes: Flake[] = []

function resize() {
  const canvas = c.value
  if (!canvas) return
  const dpr = Math.max(1, window.devicePixelRatio || 1)
  canvas.width = Math.floor(window.innerWidth * dpr)
  canvas.height = Math.floor(window.innerHeight * dpr)
  canvas.style.width = window.innerWidth + 'px'
  canvas.style.height = window.innerHeight + 'px'
  // كثافة أعلى (لكن مع سقف حتى ما يأثر على الأداء)
  const count = liteMode.value
    ? Math.min(48, Math.floor(window.innerWidth / 22))
    : Math.min(220, Math.floor(window.innerWidth / 8))
  flakes = Array.from({ length: count }).map(() => ({
    x: Math.random() * canvas.width,
    y: Math.random() * canvas.height,
    r: (Math.random() * 2 + 0.8) * dpr,
    v: (Math.random() * 0.8 + 0.35) * dpr,
    d: Math.random() * Math.PI * 2,
  }))
}

function draw() {
  const canvas = c.value
  if (!canvas) return
  const ctx = canvas.getContext('2d')
  if (!ctx) return

  ctx.clearRect(0, 0, canvas.width, canvas.height)

  // بالـ Light نخلي الثلج أغمق شوي حتى يبين
  const rgb = getComputedStyle(document.documentElement)
    .getPropertyValue('--snow-rgb')
    .trim() || '255,255,255'
  const isLight = document.documentElement.classList.contains('theme-light')

  ctx.globalAlpha = isLight ? 0.45 : 0.55
  ctx.fillStyle = `rgb(${rgb})`
  ctx.shadowColor = isLight ? `rgba(${rgb},0.22)` : `rgba(${rgb},0.12)`
  ctx.shadowBlur = isLight ? 6 : 3
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
    // حركة أنعم + انحراف رياح خفيف
    f.y += f.v
    f.x += Math.sin(f.d) * 0.45 + 0.08
    f.d += 0.01
    if (f.y > h + 10) {
      f.y = -10
      f.x = Math.random() * w
    }
  }

  raf = requestAnimationFrame(draw)
}

onMounted(() => {
  resize()
  window.addEventListener('resize', resize)
  raf = requestAnimationFrame(draw)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', resize)
  cancelAnimationFrame(raf)
})
</script>
