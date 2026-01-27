<template>
  <div class="grid gap-3">
    <!-- Main stage -->
    <div
      class="stage"
      @mousemove="onMove"
      @mouseleave="onLeave"
      @click="openFs"
    >
      <SmartImage
        class="img"
        :src="current"
        :alt="title || 'Product'"
        :style="imgStyle"
        loading="lazy"
      />

      <button v-if="safeImages.length > 1" class="nav left" type="button" @click.stop="prev" aria-label="Prev">
        <Icon name="mdi:chevron-left" class="text-2xl" />
      </button>
      <button v-if="safeImages.length > 1" class="nav right" type="button" @click.stop="next" aria-label="Next">
        <Icon name="mdi:chevron-right" class="text-2xl" />
      </button>

      <div class="badge" v-if="safeImages.length">
        <span class="keep-ltr">{{ index + 1 }}/{{ safeImages.length }}</span>
      </div>
    </div>

    <div class="hint rtl-text">
      <Icon name="mdi:magnify-plus-outline" class="text-lg opacity-80" />
      <span>حرّك الماوس للتقريب — اضغط لعرض ملء الشاشة</span>
    </div>

    <!-- Thumbs -->
    <div v-if="safeImages.length > 1" class="thumbs" dir="ltr">
      <button
        v-for="(src,i) in safeImages"
        :key="src + i"
        type="button"
        class="thumb"
        :class="{ active: i === index }"
        @click="setIndex(i)"
        :aria-label="`Thumb ${i+1}`"
      >
        <SmartImage class="thumbImg" :src="src" :alt="title || 'thumb'" loading="lazy" />
      </button>
    </div>

    <!-- Fullscreen -->
    <teleport to="body">
      <div v-if="fsOpen" class="fs">
        <div class="fsBackdrop" @click="closeFs" />
        <div class="fsBody">
          <div class="fsTop">
            <div class="fsTitle rtl-text truncate">{{ title || '' }}</div>
            <button type="button" class="fsBtn" @click="closeFs" aria-label="Close">
              <Icon name="mdi:close" class="text-2xl" />
            </button>
          </div>

          <div
            class="fsStage"
            @touchstart.passive="onTouchStart"
            @touchend.passive="onTouchEnd"
          >
            <SmartImage class="fsImg" :src="current" :alt="title || 'Product'" />

            <button v-if="safeImages.length > 1" class="fsNav left" type="button" @click.stop="prev" aria-label="Prev">
              <Icon name="mdi:chevron-left" class="text-3xl" />
            </button>
            <button v-if="safeImages.length > 1" class="fsNav right" type="button" @click.stop="next" aria-label="Next">
              <Icon name="mdi:chevron-right" class="text-3xl" />
            </button>
          </div>

          <div class="fsDots" v-if="safeImages.length > 1">
            <button
              v-for="(_,i) in safeImages"
              :key="'d'+i"
              type="button"
              class="dot"
              :class="{ on: i === index }"
              @click="setIndex(i)"
              aria-label="Dot"
            />
          </div>
        </div>
      </div>
    </teleport>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const props = defineProps<{
  images?: string[]
  title?: string
}>()

const safeImages = computed(() => (props.images || []).filter(Boolean))
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

/** Hover zoom */
const zooming = ref(false)
const origin = reactive({ x: 50, y: 50 })
const scale = 1.35

const imgStyle = computed(() => {
  if (!zooming.value) return { transform: 'scale(1)', transformOrigin: '50% 50%' }
  return {
    transform: `scale(${scale})`,
    transformOrigin: `${origin.x}% ${origin.y}%`,
  }
})

function onMove(e: MouseEvent) {
  const el = e.currentTarget as HTMLElement
  const r = el.getBoundingClientRect()
  const x = ((e.clientX - r.left) / r.width) * 100
  const y = ((e.clientY - r.top) / r.height) * 100
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
const fsOpen = ref(false)
function openFs() {
  if (!safeImages.value.length) return
  fsOpen.value = true
}
function closeFs() {
  fsOpen.value = false
}

let startX = 0
function onTouchStart(ev: TouchEvent) {
  startX = ev.touches?.[0]?.clientX || 0
}
function onTouchEnd(ev: TouchEvent) {
  const endX = ev.changedTouches?.[0]?.clientX || 0
  const dx = endX - startX
  if (Math.abs(dx) > 50) dx < 0 ? next() : prev()
  startX = 0
}
</script>

<style scoped>
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
