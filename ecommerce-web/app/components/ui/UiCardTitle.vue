<template>
  <h3 class="text-base font-semibold" :class="className">
    <slot />
  </h3>
</template>

<script setup lang="ts">
defineProps<{ className?: string }>()
</script>
