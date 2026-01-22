<template>
  <div class="grid gap-1.5">
    <label v-if="label" class="text-sm font-semibold rtl-text">{{ label }}</label>
    <input
      :value="modelValue"
      :type="type"
      :autocomplete="autocomplete"
      :placeholder="placeholder"
      :class="[
        'w-full rounded-3xl border border-app bg-surface px-4 py-3 outline-none transition focus:border-[rgb(var(--primary))]',
        $attrs.class
      ]"
      @input="onInput"
    />
  </div>
</template>

<script setup lang="ts">
type Props = {
  modelValue: string
  label?: string
  type?: string
  placeholder?: string
  autocomplete?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  type: 'text',
  placeholder: '',
  autocomplete: 'off',
})

const emit = defineEmits<{
  (e: 'update:modelValue', v: string): void
}>()

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).value)
}
</script>
