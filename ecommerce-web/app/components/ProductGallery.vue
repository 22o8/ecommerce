<template>
  <div class="grid gap-3">
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
      </button>
    </div>

    <!-- Fullscreen slider -->
    <teleport to="body">
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
function onMove(e: MouseEvent) {
  const el = e.currentTarget as HTMLElement
  const r = el.getBoundingClientRect()
  const x = ((e.clientX - r.left) / r.width) * 100
  const y = ((e.clientY - r.top) / r.height) * 100
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
