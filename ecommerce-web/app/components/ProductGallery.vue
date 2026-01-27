<template>
  <div class="grid gap-3">
<<<<<<< Updated upstream
    <!-- Main image (zoom on hover) -->
    <div
      class="relative overflow-hidden rounded-2xl border border-white/10 bg-black/20"
      @mousemove="onMove"
      @mouseleave="onLeave"
    >
      <button
=======
    <div class="main">
      <div
        class="stage"
        @mousemove="onMove"
        @mouseleave="onLeave"
        @wheel.passive="onWheel"
        @touchstart.passive="onTouchStart"
        @touchmove.passive="onTouchMove"
        @touchend.passive="onTouchEnd"
      >
        <SmartImage
          class="img"
          :class="{ zooming: zoomed }"
          :src="current"
          :alt="title || 'Product'"
          :style="imgStyle"
          @click="openFullscreen"
        />
        <button v-if="images.length>1" class="nav left" type="button" @click.stop="prev" aria-label="Prev">
          <Icon name="mdi:chevron-left" class="text-2xl" />
        </button>
        <button v-if="images.length>1" class="nav right" type="button" @click.stop="next" aria-label="Next">
          <Icon name="mdi:chevron-right" class="text-2xl" />
        </button>

        <div class="badge" v-if="images.length">
          <span class="keep-ltr">{{ index+1 }}/{{ images.length }}</span>
        </div>
      </div>

      <div class="hint rtl-text">
        <Icon name="mdi:magnify-plus-outline" class="text-lg opacity-80" />
        <span>حرّك الماوس للتقريب — اضغط لعرض ملء الشاشة</span>
      </div>
    </div>

    <div v-if="images.length>1" class="thumbs" dir="ltr">
      <button
        v-for="(src,i) in images"
        :key="src + i"
>>>>>>> Stashed changes
        type="button"
        class="group block w-full"
        @click="openFs"
        :aria-label="`Open gallery fullscreen for ${title || 'product'}`"
      >
<<<<<<< Updated upstream
        <img
          :src="current"
          :alt="title || 'Product image'"
          class="h-[340px] w-full select-none object-contain md:h-[420px] transition-transform duration-150 will-change-transform"
          :style="zoomStyle"
          loading="lazy"
          draggable="false"
        />

        <!-- hint -->
        <div class="pointer-events-none absolute inset-x-0 bottom-0 p-3">
          <div
            class="inline-flex items-center gap-2 rounded-xl border border-white/10 bg-black/40 px-3 py-2 text-xs text-white/90 opacity-0 backdrop-blur-sm transition group-hover:opacity-100"
          >
            <span>🔍</span>
            <span class="keep-ltr">Zoom + Fullscreen</span>
          </div>
        </div>
      </button>

      <!-- nav arrows -->
      <div v-if="safeImages.length > 1" class="absolute inset-0 flex items-center justify-between p-2">
        <button
          type="button"
          class="rounded-xl border border-white/10 bg-black/35 px-3 py-2 text-white backdrop-blur-sm transition hover:bg-black/50"
          @click.stop="prev"
          aria-label="Previous image"
        >
          ‹
        </button>
        <button
          type="button"
          class="rounded-xl border border-white/10 bg-black/35 px-3 py-2 text-white backdrop-blur-sm transition hover:bg-black/50"
          @click.stop="next"
          aria-label="Next image"
        >
          ›
        </button>
      </div>
    </div>

    <!-- Thumbs -->
    <div v-if="safeImages.length > 1" class="flex gap-2 overflow-auto pb-1">
      <button
        v-for="(img, i) in safeImages"
        :key="img + i"
        type="button"
        class="relative shrink-0 overflow-hidden rounded-xl border transition"
        :class="i === index ? 'border-white/40' : 'border-white/10 hover:border-white/25'"
        @click="setIndex(i)"
        :aria-label="`Select image ${i + 1}`"
      >
        <img
          :src="img"
          :alt="`${title || 'Product'} thumbnail ${i + 1}`"
          class="h-16 w-20 object-cover"
          loading="lazy"
          draggable="false"
        />
        <span
          v-if="i === index"
          class="absolute inset-0 ring-2 ring-white/30"
        />
=======
        <SmartImage class="thumbImg" :src="src" :alt="title || 'thumb'" />
>>>>>>> Stashed changes
      </button>
    </div>

    <!-- Fullscreen slider -->
    <teleport to="body">
<<<<<<< Updated upstream
      <div v-if="fsOpen" class="fixed inset-0 z-[100]">
        <div class="absolute inset-0 bg-black/85" @click="closeFs"></div>

        <div
          class="absolute inset-0 flex items-center justify-center p-4"
          @touchstart.passive="onTouchStart"
          @touchmove.passive="onTouchMove"
          @touchend.passive="onTouchEnd"
        >
          <div class="relative w-full max-w-6xl">
            <div class="flex items-center justify-between gap-3 pb-3">
              <div class="text-white/90 font-semibold truncate rtl-text">
                {{ title || '' }}
              </div>
              <button
                type="button"
                class="rounded-xl border border-white/10 bg-white/5 px-3 py-2 text-white hover:bg-white/10 transition"
                @click="closeFs"
                aria-label="Close fullscreen"
              >
                ✕
              </button>
            </div>

            <div class="relative overflow-hidden rounded-2xl border border-white/10 bg-black/30">
              <img
                :src="current"
                :alt="title || 'Product image fullscreen'"
                class="h-[70vh] w-full select-none object-contain"
                draggable="false"
              />

              <div v-if="safeImages.length > 1" class="absolute inset-0 flex items-center justify-between p-2">
                <button
                  type="button"
                  class="rounded-xl border border-white/10 bg-black/35 px-4 py-3 text-white backdrop-blur-sm transition hover:bg-black/50"
                  @click.stop="prev"
                  aria-label="Previous image"
                >
                  ‹
                </button>
                <button
                  type="button"
                  class="rounded-xl border border-white/10 bg-black/35 px-4 py-3 text-white backdrop-blur-sm transition hover:bg-black/50"
                  @click.stop="next"
                  aria-label="Next image"
                >
                  ›
                </button>
              </div>

              <div class="absolute bottom-3 left-1/2 -translate-x-1/2">
                <div class="rounded-xl border border-white/10 bg-black/35 px-3 py-1 text-xs text-white/90 backdrop-blur-sm keep-ltr">
                  {{ index + 1 }} / {{ safeImages.length }}
                </div>
              </div>
            </div>

            <div class="pt-3 text-center text-xs text-white/70 keep-ltr">
              Swipe on mobile • Click outside to close
            </div>
=======
      <div v-if="fsOpen" class="fs">
        <div class="fsBackdrop" @click="fsClose" />
        <div class="fsBody">
          <div class="fsTop">
            <div class="fsTitle rtl-text truncate">{{ title }}</div>
            <button type="button" class="fsBtn" @click="fsClose" aria-label="Close">
              <Icon name="mdi:close" class="text-2xl" />
            </button>
          </div>

          <div
            class="fsStage"
            @touchstart.passive="onFsTouchStart"
            @touchmove.passive="onFsTouchMove"
            @touchend.passive="onFsTouchEnd"
          >
            <SmartImage class="fsImg" :src="current" :alt="title || 'Product'" />

            <button v-if="images.length>1" class="fsNav left" type="button" @click.stop="prev" aria-label="Prev">
              <Icon name="mdi:chevron-left" class="text-3xl" />
            </button>
            <button v-if="images.length>1" class="fsNav right" type="button" @click.stop="next" aria-label="Next">
              <Icon name="mdi:chevron-right" class="text-3xl" />
            </button>
          </div>

          <div class="fsDots" v-if="images.length>1">
            <button v-for="(src,i) in images" :key="'d'+i" type="button" class="dot" :class="{ on: i===index }" @click="setIndex(i)" />
>>>>>>> Stashed changes
          </div>
        </div>
      </div>
    </teleport>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  images?: string[]
  title?: string
}>()

<<<<<<< Updated upstream
const safeImages = computed(() => (props.images || []).filter(Boolean))
=======
const images = computed(() => (props.images || []).filter(Boolean))
>>>>>>> Stashed changes
const index = ref(0)

watch(
  () => safeImages.value,
  (arr) => {
    if (!arr.length) index.value = 0
    if (index.value >= arr.length) index.value = 0
  },
  { immediate: true }
)

const current = computed(() => safeImages.value[index.value] || '/hero-placeholder.svg')

function setIndex(i: number) {
  index.value = Math.max(0, Math.min(i, safeImages.value.length - 1))
}
function next() {
  if (!safeImages.value.length) return
  index.value = (index.value + 1) % safeImages.value.length
}
function prev() {
  if (!safeImages.value.length) return
  index.value = (index.value - 1 + safeImages.value.length) % safeImages.value.length
}

/** Zoom on hover */
const zooming = ref(false)
const origin = reactive({ x: 50, y: 50 })

const zoomStyle = computed(() => {
  if (!zooming.value) return { transform: 'scale(1)', transformOrigin: '50% 50%' }
  return {
    transform: 'scale(1.35)',
    transformOrigin: `${origin.x}% ${origin.y}%`,
  }
})

function onMove(e: MouseEvent) {
  const el = e.currentTarget as HTMLElement
  if (!el) return
  const r = el.getBoundingClientRect()
  const x = ((e.clientX - r.left) / r.width) * 100
  const y = ((e.clientY - r.top) / r.height) * 100
<<<<<<< Updated upstream
  origin.x = Math.max(0, Math.min(100, x))
  origin.y = Math.max(0, Math.min(100, y))
  zooming.value = true
}
function onLeave() {
  zooming.value = false
  origin.x = 50
  origin.y = 50
}

/** Fullscreen + swipe */
=======
  origin.value = { x: Math.max(0, Math.min(100, x)), y: Math.max(0, Math.min(100, y)) }
  zoomed.value = true
}
function onLeave() { zoomed.value = false; scale.value = 1 }
function onWheel(e: WheelEvent) {
  // optional: ctrl+wheel to change zoom level
  if (!e.ctrlKey) return
  const delta = e.deltaY > 0 ? -0.1 : 0.1
  scale.value = Math.max(1, Math.min(3, +(scale.value + delta).toFixed(2)))
  zoomed.value = scale.value > 1
}

// Touch swipe (inline)
let tStartX = 0
function onTouchStart(ev: TouchEvent) { tStartX = ev.touches?.[0]?.clientX || 0 }
function onTouchMove(_ev: TouchEvent) {}
function onTouchEnd(ev: TouchEvent) {
  const endX = ev.changedTouches?.[0]?.clientX || 0
  const dx = endX - tStartX
  if (Math.abs(dx) > 40) dx < 0 ? next() : prev()
}

// Fullscreen
>>>>>>> Stashed changes
const fsOpen = ref(false)
function openFs() {
  if (!safeImages.value.length) return
  fsOpen.value = true
}
function closeFs() {
  fsOpen.value = false
}

const touch = reactive({ startX: 0, dx: 0 })
function onTouchStart(ev: TouchEvent) {
  touch.startX = ev.touches[0]?.clientX || 0
  touch.dx = 0
}
function onTouchMove(ev: TouchEvent) {
  const x = ev.touches[0]?.clientX || 0
  touch.dx = x - touch.startX
}
function onTouchEnd() {
  const threshold = 60
  if (touch.dx > threshold) prev()
  else if (touch.dx < -threshold) next()
  touch.startX = 0
  touch.dx = 0
}
</script>
<<<<<<< Updated upstream
=======

<style scoped>
.main{ display:grid; gap:10px; }
.stage{
  position:relative;
  border-radius: 18px;
  overflow:hidden;
  border: 1px solid rgba(255,255,255,.12);
  background: rgba(255,255,255,.04);
  aspect-ratio: 16/11;
  cursor: zoom-in;
}
.img{ width:100%; height:100%; object-fit: contain; display:block; transition: transform .15s ease; }
.nav{
  position:absolute; top:50%; transform: translateY(-50%);
  width:42px; height:42px; border-radius: 14px;
  display:flex; align-items:center; justify-content:center;
  background: rgba(0,0,0,.35); border: 1px solid rgba(255,255,255,.18);
}
.nav.left{ left:12px; }
.nav.right{ right:12px; }
.badge{
  position:absolute; bottom:10px; left:10px;
  font-size: 12px; padding:6px 10px; border-radius: 999px;
  background: rgba(0,0,0,.35); border: 1px solid rgba(255,255,255,.15);
}
.hint{ display:flex; gap:8px; align-items:center; font-size:12px; opacity:.85; }

.thumbs{ display:flex; gap:10px; overflow:auto; padding-bottom:2px; }
.thumb{
  width:76px; height:56px; border-radius: 14px; overflow:hidden;
  border: 1px solid rgba(255,255,255,.12);
  background: rgba(255,255,255,.04);
  flex: 0 0 auto;
}
.thumb.active{ border-color: rgba(167,139,250,.85); }
.thumbImg{ width:100%; height:100%; object-fit: cover; display:block; }

.fs{ position:fixed; inset:0; z-index: 200; }
.fsBackdrop{ position:absolute; inset:0; background: rgba(0,0,0,.72); }
.fsBody{ position:absolute; inset:0; display:grid; grid-template-rows: auto 1fr auto; padding: 14px; }
.fsTop{
  display:flex; align-items:center; justify-content:space-between; gap:12px;
  padding: 10px 12px; border-radius: 16px;
  background: rgba(20,20,24,.9); border: 1px solid rgba(255,255,255,.12);
}
.fsTitle{ font-weight:800; }
.fsBtn{
  width:44px; height:44px; border-radius: 14px;
  display:flex; align-items:center; justify-content:center;
  background: rgba(255,255,255,.06); border: 1px solid rgba(255,255,255,.12);
}
.fsStage{ position:relative; border-radius: 18px; overflow:hidden; margin-top: 12px; }
.fsImg{ width:100%; height:100%; object-fit: contain; display:block; background: rgba(0,0,0,.2); }
.fsNav{
  position:absolute; top:50%; transform: translateY(-50%);
  width:52px; height:52px; border-radius: 16px;
  display:flex; align-items:center; justify-content:center;
  background: rgba(0,0,0,.35); border: 1px solid rgba(255,255,255,.18);
}
.fsNav.left{ left:14px; }
.fsNav.right{ right:14px; }
.fsDots{ display:flex; justify-content:center; gap:8px; padding: 12px 0 0; }
.dot{
  width:8px; height:8px; border-radius:999px;
  background: rgba(255,255,255,.25); border: 1px solid rgba(255,255,255,.20);
}
.dot.on{ background: rgba(167,139,250,.9); border-color: rgba(167,139,250,.9); }
</style>
>>>>>>> Stashed changes
