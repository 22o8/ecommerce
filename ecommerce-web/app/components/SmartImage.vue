<template>
  <div :class="['relative overflow-hidden', props.rounded, props.background, props.wrapperClass]">
    <img
      :src="currentSrc"
      :alt="props.alt"
      :loading="props.loading"
      :class="[
        'block w-full h-full select-none',
        props.fit === 'contain' ? 'object-contain' : 'object-cover',
        props.imgClass,
      ]"
      @error="onError"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

type SmartImageProps = {
  src: string
  alt?: string
  fit?: 'cover' | 'contain'
  loading?: 'lazy' | 'eager'
  wrapperClass?: string
  imgClass?: string
  rounded?: string
  background?: string
}

const props = withDefaults(defineProps<SmartImageProps>(), {
  alt: '',
  fit: 'cover',
  loading: 'lazy',
  wrapperClass: '',
  imgClass: '',
  rounded: 'rounded-xl',
  background: 'bg-transparent',
})

// We always keep a safe fallback. This avoids broken UI when an asset fails to load.
// (Also helps when an old product points to a missing file.)
const FALLBACK = '/hero-placeholder.svg'

const srcRef = ref(props.src)
watch(
  () => props.src,
  (v) => {
    srcRef.value = v || FALLBACK
  },
  { immediate: true },
)

const currentSrc = computed(() => srcRef.value || FALLBACK)

function onError() {
  if (srcRef.value !== FALLBACK) srcRef.value = FALLBACK
}
</script>
