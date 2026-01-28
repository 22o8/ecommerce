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

let preloader: HTMLImageElement | null = null

function startPreload(src: string) {
  // SSR guard
  if (!import.meta.client) return
  if (!src) return

  // extra guard: Image constructor exists only in browser
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const ImgCtor = (globalThis as any)?.Image
  if (!ImgCtor) return

  // cleanup previous preloader
  if (preloader) {
    preloader.onload = null
    preloader.onerror = null
    preloader = null
  }

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
  // SSR: don't block rendering
  if (!import.meta.client) {
    loaded.value = true
    return
  }

  const el = imgEl.value
  if (!el) return

  // If cached/complete, the load event may not fire
  if (el.complete && el.naturalWidth > 0) {
    loaded.value = true
    failed.value = false
    return
  }

  // Try decode()
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

  // Last-resort: avoid permanent placeholder
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
    loaded.value = false
    failed.value = false

    // SSR: no preload/decode
    if (!import.meta.client) {
      loaded.value = true
      return
    }

    await nextTick()
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
