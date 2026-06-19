<template>
  <div
    class="masonry"
    :style="{
      columnCount: columns,
      columnGap: gapPx
    }"
  >
    <div
      v-for="(item, idx) in items"
      :key="getKey?.(item) ?? idx"
      class="break-inside-avoid mb-4"
    >
      <slot :item="item" :index="idx" />
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{ 
  items: any[]
  columns?: number
  gap?: number
  getKey?: (item: any) => string | number
}>()

const columns = computed(() => props.columns ?? 3)
const gapPx = computed(() => `${props.gap ?? 16}px`)
</script>

<style scoped>
.masonry {
  width: 100%;
}
@media (max-width: 1024px) {
  .masonry {
    column-count: 2 !important;
  }
}
@media (max-width: 640px) {
  .masonry {
    column-count: 1 !important;
  }
}
</style>
