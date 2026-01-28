<template>
  <div
    class="relative overflow-hidden"
    :class="wrapperClass"
    :style="wrapperStyle"
  >
    <!-- Blur placeholder until image loads -->
    <div
      v-if="!loaded"
      class="absolute inset-0"
      :style="placeholderStyle"
    />

    <img
      ref="imgEl"
      :key="src"
      :src="src"
      :alt="alt"
      class="absolute inset-0 h-full w-full transition duration-500"
      :class="imgClassComputed"
      :style="imgStyleComputed"
      :loading="loading"
      @load="onLoad"
      @error="onError"
    />

    <slot />
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    src: string
    alt?: string
    fit?: 'cover' | 'contain'
    loading?: 'lazy' | 'eager'
    wrapperClass?: string
    imgClass?: string
    rounded?: string
    background?: string
  }>(),
  {
    alt: '',
    fit: 'cover',
    loading: 'lazy',
    wrapperClass: '',
    imgClass: '',
    rounded: 'rounded-3xl',
    background: 'rgba(255,255,255,0.06)'
  }
)

const imgEl = ref<HTMLImageElement | null>(null)

const loaded = ref(false)
const failed = ref(false)

// Preload the image with a detached Image() so we never miss the load event.
// This fixes cases where the <img> load event is missed during hydration + cache.
let preloader: HTMLImageElement | null = null

function startPreload(src: string) {
  if (!src) return

  // cleanup previous preloader
  if (preloader) {
    preloader.onload = null
    preloader.onerror = null
    preloader = null
  }

  const im = new Image()
  preloader = im
  im.onload = () => {
    loaded.value = true
    failed.value = false
  }
  im.onerror = () => {
    loaded.value = true
    failed.value = true
  }
  im.src = src
}

async function syncIfAlreadyLoaded() {
  const el = imgEl.value
  if (!el) return
  // If the image is already cached/complete, the `load` event might not fire.
  if (el.complete && el.naturalWidth > 0) {
    loaded.value = true
    failed.value = false
    return
  }

  // Some browsers can miss `load` during hydration/caching; try decode()
  try {
    // decode exists in modern browsers
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const dec = (el as any).decode?.()
    if (dec && typeof dec.then === 'function') {
      await dec
      loaded.value = true
      failed.value = false
      return
    }
  } catch {
    // ignore
  }

  // Last-resort: don't keep the image hidden forever
  setTimeout(() => {
    if (!loaded.value) loaded.value = true
  }, 1200)
}

function onLoad() {
  loaded.value = true
}
function onError() {
  failed.value = true
  loaded.value = true
}

watch(
  () => props.src,
  async () => {
    // reset state for new source
    loaded.value = false
    failed.value = false
    await nextTick()
    // try both: DOM img checks + detached preloader
    syncIfAlreadyLoaded()
    startPreload(props.src)
  },
  { immediate: true }
)

onMounted(() => {
  syncIfAlreadyLoaded()
  startPreload(props.src)
})

const wrapperStyle = computed(() => ({
  background: props.background
}))

const placeholderStyle = computed(() => ({
  background: props.background,
  filter: 'blur(14px)',
  transform: 'scale(1.15)'
}))

const imgClassComputed = computed(() => {
  const base = `${props.rounded} ${props.imgClass}`.trim()
  const fit = props.fit === 'contain' ? 'object-contain' : 'object-cover'
  // Never fully hide the image (it can look "blank" if the load event is missed).
  // Instead keep it visible with a mild fade/blur until it's confirmed loaded.
  const vis = loaded.value ? 'opacity-100' : 'opacity-80'
  const blur = loaded.value ? '' : 'blur-[0.6px]'
  return `${base} ${fit} ${vis} ${blur}`.trim()
})

const imgStyleComputed = computed(() => {
  if (failed.value) {
    return {
      background: props.background
    }
  }
  return {}
})
</script>