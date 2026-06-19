<!-- app/components/EmptyState.vue -->
<template>
  <div class="rounded-3xl border border-[rgba(var(--text),0.10)] bg-[rgba(var(--text),0.04)] p-8 text-center shadow-[var(--shadow3)]">
    <h3 class="text-lg font-black text-[rgb(var(--text-strong))]">{{ title }}</h3>
    <p v-if="desc" class="mt-2 text-sm text-[rgba(var(--text),0.72)]">{{ desc }}</p>
    <div v-if="$slots.default" class="mt-5">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ title: string; desc?: string }>()
</script>
