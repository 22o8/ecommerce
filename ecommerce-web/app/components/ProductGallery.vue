<template>
  <div class="grid gap-3">
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
          fit="contain"
          :img-style="imgStyle"
          wrapper-class="w-full h-full"
          img-class="w-full h-full"
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

	    <!-- تم حذف ملاحظة التفاعل لتجنب الزحمة خصوصاً على الهاتف -->
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
        <SmartImage class="thumbImg" :src="src" :alt="title || 'thumb'" fit="cover" wrapper-class="w-full h-full" img-class="w-full h-full" />
      </button>
    </div>

    <!-- Fullscreen slider -->
    <teleport to="body">
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
          </div>
        </div>
      </div>
    </teleport>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const props = defineProps<{
  // Accept plain urls OR objects coming from API (e.g. { url }, { path })
  images: any[]
  title?: string
}>()

const { buildAssetUrl } = useApi()

function normalizeSrc(v: any): string {
  if (!v) return ''
  // plain string
  if (typeof v === 'string') return buildAssetUrl(v)
  // common API shapes
  const maybe = v.url || v.path || v.src || v.imageUrl || ''
  return maybe ? buildAssetUrl(maybe) : ''
}

const images = computed(() => (props.images || []).map(normalizeSrc).filter(Boolean))

const placeholder = '/hero-placeholder.svg'
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

function onMove(e: MouseEvent) {
  const el = e.currentTarget as HTMLElement
  const r = el.getBoundingClientRect()
  const x = ((e.clientX - r.left) / r.width) * 100
  const y = ((e.clientY - r.top) / r.height) * 100
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
.fsStage{
  position:relative;
  border-radius: 18px;
  overflow:hidden;
  margin-top: 12px;
  /* Ensure the SmartImage wrapper has height on mobile/desktop */
  height: min(72vh, 680px);
  min-height: 320px;
}
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
