<!-- app/components/AppButton.vue -->
<template>
  <button
    :type="type"
    class="inline-flex items-center justify-center font-semibold transition rounded-2xl select-none"
    :class="[sizeClass, disabledOrLoading ? 'opacity-60 cursor-not-allowed' : 'hover:opacity-95']"
    :style="btnStyle"
    :disabled="disabledOrLoading"
  >
    <span v-if="loading" class="mr-2 inline-block h-4 w-4 rounded-full animate-spin"
      :style="{ border: '2px solid rgba(255,255,255,.25)', borderTopColor: 'rgba(255,255,255,.9)' }" />
    <slot />
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  type?: 'button' | 'submit' | 'reset'
  loading?: boolean
  disabled?: boolean
  variant?: 'primary' | 'ghost' | 'danger' | 'soft'
  size?: 'sm' | 'md' | 'lg'
}>(), {
  type: 'button',
  loading: false,
  disabled: false,
  variant: 'primary',
  size: 'md',
})

const disabledOrLoading = computed(() => props.disabled || props.loading)

const sizeClass = computed(() => {
  if (props.size === 'sm') return 'px-3 py-2 text-sm'
  if (props.size === 'lg') return 'px-5 py-3 text-base'
  return 'px-4 py-2.5 text-sm'
})

const btnStyle = computed(() => {
  // يعتمد على CSS vars (رح تعرفها بـ main.css أو داخل app.vue)
  switch (props.variant) {
    case 'primary':
      return {
        background: 'rgb(var(--primary))',
        color: 'white',
        border: '1px solid rgba(255,255,255,.10)',
      }
    case 'danger':
      return {
        background: 'rgb(var(--danger))',
        color: 'white',
        border: '1px solid rgba(255,255,255,.10)',
      }
    case 'soft':
      return {
        background: 'rgba(255,255,255,.06)',
        color: 'rgb(var(--text))',
        border: '1px solid rgb(var(--border))',
      }
    case 'ghost':
    default:
      return {
        background: 'transparent',
        color: 'rgb(var(--text))',
        border: '1px solid rgb(var(--border))',
      }
  }
})
</script>
