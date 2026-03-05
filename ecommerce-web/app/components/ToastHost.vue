<template>
  <div class="pointer-events-none fixed bottom-5 right-5 z-[200] flex w-[92vw] max-w-md flex-col gap-3">
    <TransitionGroup name="toast" tag="div" class="flex flex-col gap-3">
      <div
        v-for="t in toasts"
        :key="t.id"
        class="pointer-events-auto rounded-2xl border px-4 py-3 shadow-xl backdrop-blur"
        :class="toastClass(t.type)"
      >
        <div class="flex items-start gap-3">
          <div class="mt-0.5 h-2.5 w-2.5 rounded-full" :class="dotClass(t.type)" />
          <div class="flex-1">
            <div v-if="t.title" class="text-sm font-extrabold">{{ t.title }}</div>
            <div class="text-sm font-semibold opacity-95">{{ t.message }}</div>
          </div>
          <button class="ml-2 text-sm opacity-70 hover:opacity-100" @click="remove(t.id)">
            ✕
          </button>
        </div>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import type { ToastItem } from '~/composables/useToast'

const { toasts, remove } = useToast()

function toastClass(type: ToastItem['type']) {
  if (type === 'success') return 'bg-zinc-900/90 text-white border-white/10 dark:bg-white/90 dark:text-zinc-900 dark:border-zinc-200/70'
  if (type === 'error') return 'bg-red-600/90 text-white border-white/10 dark:bg-red-600/90 dark:text-white'
  return 'bg-zinc-900/80 text-white border-white/10 dark:bg-zinc-950/80 dark:text-white dark:border-white/10'
}

function dotClass(type: ToastItem['type']) {
  if (type === 'success') return 'bg-emerald-400'
  if (type === 'error') return 'bg-white'
  return 'bg-sky-300'
}
</script>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.18s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateY(8px) scale(0.98);
}
.toast-leave-to {
  opacity: 0;
  transform: translateY(8px) scale(0.98);
}
</style>
