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

// ✅ SSR guard: لا تستخدم Image / window بالسيرفر
const isClient = typeof window !== 'undefined'

// Preload the image with a detached Image() so we never miss the load event.
// This fixes cases where the <img> load event is missed during hydration + cache.
let preloader: HTMLImageElement | null = null

function cleanupPreloader() {
  if (!preloader) return
  preloader.onload = null
  preloader.onerror = null
  preloader = null
}

function startPreload(src: string) {
  if (!src) return

  // ✅ على السيرفر لا تسوي preload نهائياً (هذا سبب 500)
  if (!isClient) return

  cleanupPreloader()

  // ✅ استخدم window.Image حتى نتأكد هو متوفر بالمتصفح
  const ImgCtor = (window as any).Image
  if (!ImgCtor) return

  const im = new ImgCtor() as HTMLImageElement
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

  // ✅ على السيرفر خلّه loaded حتى ما تصير شاشة بيضة / crash
  if (!isClient) {
    loaded.value = true
    failed.value = false
    return
  }

  // If the image is already cached/complete, the `load` event might not fire.
  if (el.complete && el.naturalWidth > 0) {
    loaded.value = true
    failed.value = false
    return
  }

  // Some browsers can miss `load` during hydration/caching; try decode()
  try {
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
  failed.value = false
}
function onError() {
  failed.value = true
  loaded.value = true
}

watch(
  () => props.src,
  async (v) => {
    // reset state for new source
    loaded.value = false
    failed.value = false

    // ✅ إذا ماكو رابط، لا تخلي placeholder للأبد
    if (!v) {
      loaded.value = true
      return
    }

    await nextTick()

    // ✅ try both: DOM img checks + detached preloader (client only)
    syncIfAlreadyLoaded()
    startPreload(v)
  },
  { immediate: true }
)

onMounted(() => {
  syncIfAlreadyLoaded()
  startPreload(props.src)
})

onBeforeUnmount(() => {
  cleanupPreloader()
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
