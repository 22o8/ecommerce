<template>
  <div class="grid gap-4 sm:grid-cols-2 md:grid-cols-4">
    <div
      v-for="i in count"
      :key="i"
      class="rounded-2xl border border-white/10 bg-white/5 p-4"
    >
      <div class="h-44 w-full rounded-xl bg-white/10 animate-pulse" />
      <div class="mt-4 h-4 w-4/5 rounded bg-white/10 animate-pulse" />
      <div class="mt-2 h-4 w-2/5 rounded bg-white/10 animate-pulse" />
      <div class="mt-4 h-9 w-full rounded-xl bg-white/10 animate-pulse" />
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{ count?: number }>()
const count = computed(() => props.count ?? 8)
</script>
