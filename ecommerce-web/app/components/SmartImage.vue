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

const loaded = ref(false)
const failed = ref(false)

function onLoad() {
  loaded.value = true
}
function onError() {
  failed.value = true
  loaded.value = true
}

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
  const blur = loaded.value ? 'opacity-100' : 'opacity-0'
  return `${base} ${fit} ${blur}`.trim()
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