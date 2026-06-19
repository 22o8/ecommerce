<!-- app/components/AppInput.vue -->
<script setup lang="ts">
const modelValue = defineModel<string>({ default: '' })

const props = defineProps<{
  label?: string
  type?: string
  placeholder?: string
  error?: string
  autocomplete?: string
}>()

const id = useId()
</script>

<template>
  <div>
    <label v-if="label" :for="id" class="text-sm opacity-75">{{ label }}</label>

    <input
      :id="id"
      v-model="modelValue"
      :type="type || 'text'"
      :placeholder="placeholder"
      :autocomplete="autocomplete"
      class="mt-1 w-full rounded-2xl border px-4 py-3 outline-none transition
             bg-[rgb(var(--card))] text-[rgb(var(--text))]
             border-[rgb(var(--border))]
             focus:border-[rgb(var(--ring))] focus:ring-2 focus:ring-[rgb(var(--ring))]/20"
    />

    <p v-if="error" class="mt-2 text-sm text-[rgb(var(--danger))]">
      {{ error }}
    </p>
  </div>
</template>
