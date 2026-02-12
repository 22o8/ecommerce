<template>
  <div :class="['relative overflow-hidden', props.rounded, props.background, props.wrapperClass]">
    <img
      :src="props.src"
      :alt="props.alt"
      :loading="props.loading"
      :class="[
        'block w-full h-full select-none',
        props.fit === 'contain' ? 'object-contain' : 'object-cover',
        props.imgClass,
      ]"
    />
  </div>
</template>

<script setup lang="ts">
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
</script>
