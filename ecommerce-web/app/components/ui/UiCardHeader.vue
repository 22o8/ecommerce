<template>
  <div class="mb-3 flex items-start justify-between gap-3" :class="className">
    <slot />
  </div>
</template>

<script setup lang="ts">
defineProps<{ className?: string }>()
</script>
