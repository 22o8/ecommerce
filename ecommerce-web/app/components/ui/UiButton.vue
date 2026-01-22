<template>
  <button
    :type="type"
    class="inline-flex items-center justify-center gap-2 rounded-2xl px-4 py-2 text-sm font-semibold transition active:scale-[.99]"
    :class="classes"
    :disabled="disabled || loading"
  >
    <Icon v-if="loading" name="mdi:loading" class="text-lg animate-spin" />
    <slot />
  </button>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
  type?: 'button' | 'submit' | 'reset'
  disabled?: boolean
  loading?: boolean
}>(), { variant: 'primary', type: 'button', disabled: false, loading: false })

const classes = computed(() => {
  const base = 'border border-app'
  if (props.variant === 'primary') return `${base} bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] hover:opacity-95`
  if (props.variant === 'secondary') return `${base} bg-surface-2 hover:bg-[rgba(var(--text),.06)]`
  if (props.variant === 'ghost') return `border-transparent bg-transparent hover:bg-[rgba(var(--text),.06)]`
  if (props.variant === 'danger') return `${base} bg-[rgb(var(--danger))] text-white hover:opacity-95`
  return base
})
</script>
