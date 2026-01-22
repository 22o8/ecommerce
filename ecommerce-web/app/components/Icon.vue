<template>
  <!-- iconify-icon supports names like "mdi:account" -->
  <iconify-icon
    :icon="name"
    :class="cls"
    :style="styleObj"
    aria-hidden="true"
  />
</template>

<script setup lang="ts">
const props = defineProps<{
  name: string
  class?: string
  size?: string | number
}>()

const cls = computed(() => props.class || '')

const styleObj = computed(() => {
  const s = props.size
  if (s === undefined || s === null || s === '') return {}
  const v = typeof s === 'number' ? `${s}px` : String(s)
  return { width: v, height: v }
})
</script>
