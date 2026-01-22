<template>
  <div class="card-soft p-4 md:p-5" :class="className">
    <slot />
  </div>
</template>

<script setup lang="ts">
defineProps<{ className?: string }>()
</script>
