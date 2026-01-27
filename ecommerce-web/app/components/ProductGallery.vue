<template>
  <div class="grid gap-3">
<<<<<<< HEAD
    <!-- Main image with zoom -->
    <div
      class="relative overflow-hidden rounded-3xl border border-white/10 bg-black/20 select-none"
      @mousemove="onMove"
      @mouseleave="onLeave"
      @mouseenter="onEnter"
      @click="openFs(activeIndex)"
      :style="{ aspectRatio: '4 / 3' }"
    >
      <SmartImage
        :src="activeSrc"
        :alt="title || 'Product image'"
        class="h-full w-full object-contain"
      />

      <!-- zoom lens layer -->
      <div
        class="pointer-events-none absolute inset-0 transition-opacity duration-200"
        :style="zoomStyle"
      />

      <!-- arrows -->
      <button
        v-if="images.length > 1"
        class="absolute left-3 top-1/2 -translate-y-1/2 rounded-2xl border border-white/15 bg-black/40 px-3 py-2 hover:bg-black/55 transition"
        @click.stop="prev"
        aria-label="Prev"
      >
        <Icon name="mdi:chevron-left" class="text-2xl" />
      </button>

      <button
        v-if="images.length > 1"
        class="absolute right-3 top-1/2 -translate-y-1/2 rounded-2xl border border-white/15 bg-black/40 px-3 py-2 hover:bg-black/55 transition"
        @click.stop="next"
        aria-label="Next"
      >
        <Icon name="mdi:chevron-right" class="text-2xl" />
      </button>

      <!-- hint -->
      <div class="absolute bottom-3 left-3 flex items-center gap-2 rounded-2xl border border-white/10 bg-black/35 px-3 py-2 text-xs">
        <Icon name="mdi:magnify-plus-outline" class="text-lg" />
        <span class="keep-ltr">Zoom / Fullscreen</span>
      </div>
    </div>

    <!-- thumbs -->
    <div v-if="images.length > 1" class="flex gap-2 overflow-auto pb-1">
      <button
        v-for="(src, i) in images"
        :key="src + i"
        class="shrink-0 rounded-2xl border transition overflow-hidden"
        :class="i === activeIndex ? 'border-white/40' : 'border-white/10 hover:border-white/25'"
        @click="activeIndex = i"
        :style="{ width: '84px', height: '64px' }"
      >
        <SmartImage :src="src" :alt="title || 'thumb'" class="h-full w-full object-cover" />
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
        type="button"
        class="thumb"
        :class="{ active: i===index }"
        @click="setIndex(i)"
      >
        <SmartImage class="thumbImg" :src="src" :alt="title || 'thumb'" />
>>>>>>> 7764c28 (fix: public products mapping + gallery + render build css)
      </button>
    </div>

    <!-- Fullscreen slider -->
    <teleport to="body">
<<<<<<< HEAD
      <div v-if="fsOpen" class="fixed inset-0 z-[200]">
        <div class="absolute inset-0 bg-black/80" @click="closeFs" />
        <div class="absolute inset-0 flex items-center justify-center p-3">
          <div
            class="relative w-full max-w-6xl rounded-3xl border border-white/10 bg-black/40 overflow-hidden"
            @touchstart.passive="onTouchStart"
            @touchend.passive="onTouchEnd"
          >
            <div class="flex items-center justify-between px-4 py-3 border-b border-white/10">
              <div class="font-bold truncate rtl-text">{{ title || '' }}</div>
              <button
                class="rounded-2xl border border-white/15 bg-black/35 px-3 py-2 hover:bg-black/55 transition"
                @click="closeFs"
              >
                <Icon name="mdi:close" class="text-2xl" />
              </button>
            </div>

            <div class="relative grid place-items-center p-3" :style="{ height: '72vh' }">
              <SmartImage
                :src="activeSrc"
                :alt="title || 'image'"
                class="max-h-full max-w-full object-contain"
              />

              <button
                v-if="images.length > 1"
                class="absolute left-3 top-1/2 -translate-y-1/2 rounded-2xl border border-white/15 bg-black/40 px-3 py-2 hover:bg-black/55 transition"
                @click.stop="prev"
              >
                <Icon name="mdi:chevron-left" class="text-3xl" />
              </button>

              <button
                v-if="images.length > 1"
                class="absolute right-3 top-1/2 -translate-y-1/2 rounded-2xl border border-white/15 bg-black/40 px-3 py-2 hover:bg-black/55 transition"
                @click.stop="next"
              >
                <Icon name="mdi:chevron-right" class="text-3xl" />
              </button>
            </div>

            <div class="px-4 pb-4 flex items-center justify-between text-sm text-white/70">
              <div class="keep-ltr">{{ activeIndex + 1 }} / {{ images.length }}</div>
              <div class="keep-ltr">Swipe on mobile</div>
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
>>>>>>> 7764c28 (fix: public products mapping + gallery + render build css)
          </div>
        </div>
      </div>
    </teleport>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const props = defineProps<{
  images: string[]
  title?: string
}>()

<<<<<<< HEAD
const activeIndex = ref(0)
const fsOpen = ref(false)

const activeSrc = computed(() => props.images?.[activeIndex.value] || '')

function prev() {
  if (!props.images?.length) return
  activeIndex.value = (activeIndex.value - 1 + props.images.length) % props.images.length
}
function next() {
  if (!props.images?.length) return
  activeIndex.value = (activeIndex.value + 1) % props.images.length
}

function openFs(i: number) {
  activeIndex.value = i
  fsOpen.value = true
  document.documentElement.style.overflow = 'hidden'
}
function closeFs() {
  fsOpen.value = false
  document.documentElement.style.overflow = ''
}

const zoom = reactive({
  on: false,
  x: 50,
  y: 50,
})

const zoomStyle = computed(() => {
  if (!zoom.on) return { opacity: '0' }
  // subtle zoom overlay effect (no cropping the base image)
  return {
    opacity: '1',
    backgroundImage: `url("${activeSrc.value}")`,
    backgroundRepeat: 'no-repeat',
    backgroundSize: '180%',
    backgroundPosition: `${zoom.x}% ${zoom.y}%`,
    mixBlendMode: 'screen',
    filter: 'contrast(1.05) saturate(1.02)',
  } as any
})

function onEnter() {
  zoom.on = true
}
function onLeave() {
  zoom.on = false
}
=======
const images = computed(() => (props.images || []).filter(Boolean))
const index = ref(0)

watch(images, (arr) => {
  if (!arr.length) index.value = 0
  else if (index.value >= arr.length) index.value = 0
}, { immediate: true })

const current = computed(() => images.value[index.value] || '')

function setIndex(i: number) {
  index.value = Math.min(Math.max(i, 0), images.value.length - 1)
}
function next() { if (images.value.length) setIndex((index.value + 1) % images.value.length) }
function prev() { if (images.value.length) setIndex((index.value - 1 + images.value.length) % images.value.length) }

// Hover zoom
const zoomed = ref(false)
const origin = ref({ x: 50, y: 50 })
const scale = ref(1)

const imgStyle = computed(() => ({
  transformOrigin: `${origin.value.x}% ${origin.value.y}%`,
  transform: `scale(${zoomed.value ? scale.value : 1})`,
}))

>>>>>>> 7764c28 (fix: public products mapping + gallery + render build css)
function onMove(e: MouseEvent) {
  const el = e.currentTarget as HTMLElement
  const r = el.getBoundingClientRect()
  const x = ((e.clientX - r.left) / r.width) * 100
  const y = ((e.clientY - r.top) / r.height) * 100
<<<<<<< HEAD
  zoom.x = Math.max(0, Math.min(100, x))
  zoom.y = Math.max(0, Math.min(100, y))
}

// swipe
let touchX = 0
function onTouchStart(e: TouchEvent) {
  touchX = e.changedTouches[0]?.clientX ?? 0
}
function onTouchEnd(e: TouchEvent) {
  const endX = e.changedTouches[0]?.clientX ?? 0
  const dx = endX - touchX
  if (Math.abs(dx) < 40) return
  if (dx > 0) prev()
  else next()
}

watch(activeSrc, () => {
  // reset zoom when image changes
  zoom.on = false
})
</script>
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
const fsOpen = ref(false)
function openFullscreen() { fsOpen.value = true }
function fsClose() { fsOpen.value = false }

// Fullscreen swipe
let fsStartX = 0
function onFsTouchStart(ev: TouchEvent){ fsStartX = ev.touches?.[0]?.clientX || 0 }
function onFsTouchMove(_ev: TouchEvent){}
function onFsTouchEnd(ev: TouchEvent){
  const endX = ev.changedTouches?.[0]?.clientX || 0
  const dx = endX - fsStartX
  if (Math.abs(dx) > 40) dx < 0 ? next() : prev()
}
</script>

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
>>>>>>> 7764c28 (fix: public products mapping + gallery + render build css)
