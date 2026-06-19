<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useApi } from '~/composables/useApi'

type Brand = {
  id: string
  name: string
  slug: string
  logoUrl?: string | null
}

const props = defineProps<{
  brands: (Brand | null | undefined)[]
  radius?: number
  tiltDeg?: number
  speedSec?: number
}>()

const api = useApi()
const { locale } = useI18n()

const clean = computed(() => (props.brands ?? []).filter((b): b is Brand => !!b && !!b.slug && !!b.id))

const radius = computed(() => Number(props.radius ?? 280))
const tilt = computed(() => Number(props.tiltDeg ?? 14))
const speed = computed(() => Number(props.speedSec ?? 26))

const stepDeg = computed(() => (clean.value.length ? 360 / clean.value.length : 0))

function itemStyle(idx: number) {
  const deg = idx * stepDeg.value
  // rotateY: توزيع العناصر على دائرة
  // translateZ: نصف القطر
  return {
    // rotateY(deg) يوزع العناصر حول الحلقة
    // translateZ(radius) يبعدها عن المركز
    // rotateY(-deg) يخلي اللوغو دائماً مواجه للكاميرا (يمنع الاختفاء/الوميض خصوصاً على iOS)
    transform: `rotateY(${deg}deg) translateZ(${radius.value}px) rotateY(${-deg}deg)`,
  } as Record<string, string>
}

function logoUrl(b: Brand) {
  return api.buildAssetUrl(b.logoUrl || '')
}

// ==========================
// Mode: Orbit (3D) <-> Slider
// ==========================
const mode = ref<'orbit' | 'slider'>('orbit')
const isHovering = ref(false)
const isDragging = ref(false)

// rotation in degrees
const ry = ref(0)
let raf = 0
let lastTs = 0

// drag state
let dragStartX = 0
let dragStartRY = 0
let activePointerId: number | null = null

const labels = computed(() => {
  const isAr = String(locale.value || '').startsWith('ar')
  return {
    viewAll: isAr ? 'عرض الكل' : 'View all',
    back: isAr ? 'رجوع' : 'Back',
    hint: isAr ? 'اسحب للتحكم' : 'Drag to rotate',
  }
})

function toggleMode() {
  mode.value = mode.value === 'orbit' ? 'slider' : 'orbit'
}

function onPointerDown(e: PointerEvent) {
  if (mode.value !== 'orbit') return
  if ((e as any).button != null && (e as any).button !== 0) return
  activePointerId = e.pointerId
  isDragging.value = true
  dragStartX = e.clientX
  dragStartRY = ry.value
  ;(e.currentTarget as HTMLElement)?.setPointerCapture?.(e.pointerId)
}

function onPointerMove(e: PointerEvent) {
  if (!isDragging.value || activePointerId !== e.pointerId) return
  const dx = e.clientX - dragStartX
  ry.value = dragStartRY + dx * 0.35
}

function onPointerUp(e: PointerEvent) {
  if (activePointerId !== e.pointerId) return
  isDragging.value = false
  activePointerId = null
}

function tick(ts: number) {
  if (!lastTs) lastTs = ts
  const dt = ts - lastTs
  lastTs = ts

  if (mode.value === 'orbit' && !isDragging.value) {
    const degPerMs = 360 / (speed.value * 1000)
    const factor = isHovering.value ? 0.22 : 1
    ry.value = (ry.value + dt * degPerMs * factor) % 360
  }

  raf = requestAnimationFrame(tick)
}

onMounted(() => {
  raf = requestAnimationFrame(tick)
})

onBeforeUnmount(() => {
  if (raf) cancelAnimationFrame(raf)
})
</script>

<template>
  <section v-if="clean.length" class="orbit-wrap">
    <div class="orbit-head">
      <button type="button" class="orbit-toggle" @click="toggleMode">
        {{ mode === 'orbit' ? labels.viewAll : labels.back }}
      </button>
      <div v-if="mode === 'orbit'" class="orbit-hint">{{ labels.hint }}</div>
    </div>

    <Transition name="orbit-fade" mode="out-in">
      <!-- ORBIT MODE -->
      <div
        v-if="mode === 'orbit'"
        class="orbit-scene"
        :style="{ '--orbit-tilt': `${tilt}deg` } as any"
        @pointerenter="isHovering = true"
        @pointerleave="isHovering = false"
        @pointerdown.passive="onPointerDown"
        @pointermove.passive="onPointerMove"
        @pointerup.passive="onPointerUp"
        @pointercancel.passive="onPointerUp"
      >
        <div
          class="orbit-ring"
          :style="{ transform: `translateY(-4px) rotateX(${tilt}deg) rotateY(${ry}deg)` } as any"
        >
          <NuxtLink
            v-for="(b, idx) in clean"
            :key="b.id"
            :to="`/brands/${b.slug}`"
            class="orbit-item"
            :style="itemStyle(idx)"
            :aria-label="b.name"
            :title="b.name"
          >
            <div class="orbit-card">
              <img
                v-if="b.logoUrl"
                :src="logoUrl(b)"
                :alt="b.name"
                loading="lazy"
                class="orbit-img"
              />
              <div v-else class="orbit-fallback">Logo</div>
            </div>
          </NuxtLink>
        </div>
      </div>

      <!-- SLIDER MODE -->
      <div v-else class="brand-slider">
        <div class="brand-slider-track" aria-label="Brands">
          <NuxtLink
            v-for="b in clean"
            :key="b.id"
            :to="`/brands/${b.slug}`"
            class="brand-slide"
            :aria-label="b.name"
            :title="b.name"
          >
            <div class="orbit-card">
              <img
                v-if="b.logoUrl"
                :src="logoUrl(b)"
                :alt="b.name"
                loading="lazy"
                class="orbit-img"
              />
              <div v-else class="orbit-fallback">Logo</div>
            </div>
            <div class="brand-name">{{ b.name }}</div>
          </NuxtLink>
        </div>
      </div>
    </Transition>
  </section>
</template>

<style scoped>
.orbit-wrap{
  position: relative;
  /* خليه يطلع شوي لفوك حتى الجزء الخلفي "يدخل" خلف المنتجات */
  margin-top: -24px;
}

.orbit-head{
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.orbit-toggle{
  border: 1px solid rgb(var(--border));
  background: rgba(var(--surface-rgb), .72);
  color: rgb(var(--text));
  padding: 8px 12px;
  border-radius: 9999px;
  font-size: 12px;
  font-weight: 700;
  box-shadow: var(--shadow-card);
  transition: transform 160ms ease, background 160ms ease;
}
.orbit-toggle:hover{ transform: translateY(-1px); background: rgba(var(--surface-rgb), .9); }

.orbit-hint{
  font-size: 12px;
  color: rgba(var(--muted), .95);
}

.orbit-scene{
  position: relative;
  width: 100%;
  height: 260px;
  display: grid;
  place-items: center;
  perspective: 820px;
  touch-action: pan-y;
}

.orbit-ring{
  position: relative;
  will-change: transform;
  -webkit-transform-style: preserve-3d;
  width: 0;
  height: 0;
  transform-style: preserve-3d;
  /* الدوران صار controlled بالـ JS حتى نقدر نسوي hover slow + drag */
  transform: translateY(-4px) rotateX(var(--orbit-tilt)) rotateY(0deg);
}

.orbit-item{
  position: absolute;
  left: 50%;
  top: 50%;
  transform-style: preserve-3d;
  translate: -50% -50%;
  text-decoration: none;
}

.orbit-card{
  width: 76px;
  height: 76px;
  border-radius: 9999px;
  border: 1px solid rgb(var(--border));
  background: rgba(var(--surface-rgb), .96);
  box-shadow: var(--shadow-card);
  display: grid;
  place-items: center;
  overflow: hidden;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
  will-change: transform;
  transform: translateZ(0.1px);
  transition: transform 220ms ease, box-shadow 220ms ease;
}

.orbit-item:hover .orbit-card{
  transform: translateZ(0.1px) translateY(-3px) scale(1.03);
  box-shadow: 0 22px 70px rgba(0,0,0,.18);
}

.orbit-img{
  width: 100%;
  height: 100%;
  object-fit: cover;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
  transform: translateZ(0.2px);
}

.orbit-fallback{
  font-size: 12px;
  color: rgba(var(--muted), .95);
}

@media (prefers-reduced-motion: reduce){
  .orbit-hint{ display: none; }
}

@media (max-width: 640px){
  .orbit-scene{ height: 190px; }
  .orbit-card{ width: 66px; height: 66px; }
}

/* Slider mode */
.brand-slider{
  width: 100%;
  padding: 8px 0 2px;
}

.brand-slider-track{
  display: flex;
  gap: 14px;
  overflow-x: auto;
  padding: 6px 2px 10px;
  scroll-snap-type: x mandatory;
  -webkit-overflow-scrolling: touch;
}

.brand-slide{
  flex: 0 0 auto;
  width: 104px;
  display: grid;
  justify-items: center;
  gap: 8px;
  scroll-snap-align: center;
  text-decoration: none;
}

.brand-name{
  font-size: 12px;
  font-weight: 700;
  color: rgba(var(--text), .92);
  text-align: center;
  max-width: 104px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* transition */
.orbit-fade-enter-active,
.orbit-fade-leave-active{
  transition: opacity 180ms ease, transform 180ms ease;
}
.orbit-fade-enter-from,
.orbit-fade-leave-to{
  opacity: 0;
  transform: translateY(6px) scale(.98);
}
</style>
