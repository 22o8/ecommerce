<template>
  <div class="grid gap-4 sm:grid-cols-2 md:grid-cols-4">
    <div
      v-for="i in count"
      :key="i"
      class="rounded-2xl border border-white/10 bg-white/5 p-4"
    >
      <div class="mx-auto h-16 w-16 rounded-2xl bg-white/10 animate-pulse" />
      <div class="mt-3 mx-auto h-4 w-2/3 rounded bg-white/10 animate-pulse" />
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{ count?: number }>()
const count = computed(() => props.count ?? 8)
</script>
