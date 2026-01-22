<template>
  <label class="block">
    <span v-if="label" class="mb-2 block text-sm font-semibold rtl-text">{{ label }}</span>
    <input
      v-bind="$attrs"
      class="w-full rounded-2xl border border-app bg-surface px-4 py-2 text-sm outline-none transition focus:ring-0"
    />
    <p v-if="hint" class="mt-2 text-xs text-muted rtl-text">{{ hint }}</p>
  </label>
</template>

<script setup lang="ts">
defineProps<{ label?: string; hint?: string }>()
</script>
