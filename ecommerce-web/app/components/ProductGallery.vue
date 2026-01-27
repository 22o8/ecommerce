<template>
  <div class="grid gap-3">
    <!-- Main image (zoom on hover) -->
    <div
      class="relative overflow-hidden rounded-2xl border border-white/10 bg-black/20"
      @mousemove="onMove"
      @mouseleave="onLeave"
    >
      <button
        type="button"
        class="group block w-full"
        @click="openFs"
        :aria-label="`Open gallery fullscreen for ${title || 'product'}`"
      >
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
      </button>
    </div>

    <!-- Fullscreen slider -->
    <teleport to="body">
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
