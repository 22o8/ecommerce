<!-- app/components/SkeletonGrid.vue -->
<template>
  <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
    <div
      v-for="i in count"
      :key="i"
      class="rounded-3xl border border-[rgba(var(--text),0.10)] bg-[rgba(var(--text),0.04)] p-5 shadow-[var(--shadow3)]"
    >
      <div class="h-4 w-2/3 animate-pulse rounded bg-[rgba(var(--text),0.10)]"></div>
      <div class="mt-3 h-3 w-1/2 animate-pulse rounded bg-[rgba(var(--text),0.08)]"></div>
      <div class="mt-5 h-10 w-full animate-pulse rounded-xl bg-[rgba(var(--text),0.10)]"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ count?: number }>()
</script>
