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
    background: 'rgba(255,255,255,0.06)',
  }
)

const imgEl = ref<HTMLImageElement | null>(null)
const loaded = ref(false)
const failed = ref(false)

// مهم: على السيرفر ماكو DOM ولا Image()
// نخليها true حتى SSR ما يرمي 500 وما يظل placeholder
if (import.meta.server) {
  loaded.value = true
}

let preloader: HTMLImageElement | null = null

function cleanupPreloader() {
  if (!preloader) return
  preloader.onload = null
  preloader.onerror = null
  preloader = null
}

function startPreload(src: string) {
  // client فقط
  if (!import.meta.client) return
  if (!src) return

  cleanupPreloader()

  const im = new window.Image()
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
  // client فقط
  if (!import.meta.client) return

  const el = imgEl.value
  if (!el) return

  // إذا الصورة بالكاش
  if (el.complete && el.naturalWidth > 0) {
    loaded.value = true
    failed.value = false
    return
  }

  // جرّب decode() لو متوفر
  try {
    const anyEl = el as any
    const dec = anyEl.decode?.()
    if (dec && typeof dec.then === 'function') {
      await dec
      loaded.value = true
      failed.value = false
      return
    }
  } catch {
    // ignore
  }

  // fallback: لا تبقى مخفية للأبد
  window.setTimeout(() => {
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
  async (newSrc) => {
    // reset state for new source
    loaded.value = false
    failed.value = false

    // SSR: لا تسوي preload
    if (!import.meta.client) {
      loaded.value = true
      return
    }

    await nextTick()
    syncIfAlreadyLoaded()
    startPreload(newSrc)
  },
  { immediate: true }
)

onMounted(() => {
  // client فقط
  syncIfAlreadyLoaded()
  startPreload(props.src)
})

onBeforeUnmount(() => {
  cleanupPreloader()
})

const wrapperStyle = computed(() => ({
  background: props.background,
}))

const placeholderStyle = computed(() => ({
  background: props.background,
  filter: 'blur(14px)',
  transform: 'scale(1.15)',
}))

const imgClassComputed = computed(() => {
  const base = `${props.rounded} ${props.imgClass}`.trim()
  const fit = props.fit === 'contain' ? 'object-contain' : 'object-cover'
  const vis = loaded.value ? 'opacity-100' : 'opacity-80'
  const blur = loaded.value ? '' : 'blur-[0.6px]'
  return `${base} ${fit} ${vis} ${blur}`.trim()
})

const imgStyleComputed = computed(() => {
  if (failed.value) {
    return { background: props.background }
  }
  return {}
})
</script>
