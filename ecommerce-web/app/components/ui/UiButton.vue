<template>
  <button
    :type="props.type"
    class="inline-flex items-center justify-center gap-2 rounded-2xl px-4 py-2 text-sm font-semibold transition will-change-transform select-none touch-manipulation hover:-translate-y-0.5 hover:shadow-md hover:shadow-[rgb(var(--primary))]/10 active:translate-y-0 active:scale-[.99] focus-visible:outline focus-visible:outline-2 focus-visible:outline-[rgb(var(--primary))]/35 disabled:opacity-60 disabled:pointer-events-none"
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
  const base = 'border border-app text-app'
  // Primary: يظل واضح في كلا الثيمين
  if (props.variant === 'primary') return `${base} bg-[rgb(var(--primary))] text-[rgb(var(--on-primary))] border-transparent hover:opacity-95`
  // Secondary: سطح + حد واضح
  if (props.variant === 'secondary') return `${base} bg-surface-2 hover:bg-[rgba(var(--text),.06)]`
  // Ghost: شفاف لكن بحد واضح
  if (props.variant === 'ghost') return `border border-app bg-transparent text-app hover:bg-[rgba(var(--text),.06)]`
  if (props.variant === 'danger') return `${base} bg-[rgb(var(--danger))] text-white hover:opacity-95`
  return base
})
</script>
