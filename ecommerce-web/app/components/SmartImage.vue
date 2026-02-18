<template>
  <div :style="props.wrapperStyle"
      :class="['relative overflow-hidden', props.rounded, props.background, props.wrapperClass]">
    <img
      :src="currentSrc"
      :alt="props.alt"
      :loading="props.loading"
      :style="props.imgStyle"
      decoding="async"
      referrerpolicy="no-referrer"
      @load="onLoad"
      :class="[
        'block w-full h-full select-none transition-opacity duration-300',
        loaded ? 'opacity-100' : 'opacity-0',
        props.fit === 'contain' ? 'object-contain' : 'object-cover',
        props.imgClass,
      ]"
      @error="onError"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useApi } from '~/composables/useApi'

type SmartImageProps = {
  src: string
  alt?: string
  fit?: 'cover' | 'contain'
  loading?: 'lazy' | 'eager'
  wrapperClass?: string
  imgClass?: string
  imgStyle?: any
  wrapperStyle?: any
  rounded?: string
  background?: string
}

const props = withDefaults(defineProps<SmartImageProps>(), {
  alt: '',
  fit: 'cover',
  loading: 'lazy',
  wrapperClass: '',
  imgClass: '',
  imgStyle: undefined,
  wrapperStyle: undefined,
  rounded: 'rounded-xl',
  background: 'bg-transparent',
})

// Fallback (keeps UI stable if an image url is missing / broken)
const FALLBACK = '/img-placeholder.svg'

const api = useApi()

function resolveUrl(v: string | undefined | null) {
  const raw = (v || '').trim()
  if (!raw) return FALLBACK

  // Leave absolute URLs, data URIs, and local public assets as-is
  if (raw.startsWith('http://') || raw.startsWith('https://') || raw.startsWith('data:') || raw.startsWith('/_nuxt/') || raw.startsWith('/favicon') || raw.startsWith('/hero-placeholder')) {
    return raw
  }

  // If backend returns "/uploads/..." or "uploads/...", route via BFF proxy:
  // -> "/api/bff/uploads/..."
  return api.buildAssetUrl(raw)
}

const srcRef = ref(resolveUrl(props.src))
const loaded = ref(false)

watch(
  () => props.src,
  (v) => {
    srcRef.value = resolveUrl(v)
    loaded.value = false
  },
  { immediate: true },
)

const currentSrc = computed(() => srcRef.value || FALLBACK)

function onLoad() {
  loaded.value = true
}

function onError() {
  if (srcRef.value !== FALLBACK) srcRef.value = FALLBACK
  loaded.value = true
}
</script>
