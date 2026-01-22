<template>
  <span
    class="inline-flex items-center justify-center leading-none"
    :class="props.class"
    :style="mergedStyle"
    aria-hidden="true"
    v-html="svg"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import mdi from '@iconify-json/mdi/icons.json'

const props = defineProps<{
  /** e.g. "mdi:account-lock-outline" or "account-lock-outline" */
  name: string
  class?: any
  style?: any
  size?: number | string
}>()

function normalizeName(n: string) {
  const raw = (n || '').trim()
  if (!raw) return ''
  // supports "mdi:xxx" and "xxx"
  return raw.includes(':') ? raw.split(':').slice(1).join(':') : raw
}

const svg = computed(() => {
  const key = normalizeName(props.name)
  if (!key) return ''
  const icons = (mdi as any)?.icons || {}
  const icon = icons[key]
  if (!icon?.body) return ''
  const w = icon.width ?? (mdi as any)?.width ?? 24
  const h = icon.height ?? (mdi as any)?.height ?? 24
  const viewBox = `0 0 ${w} ${h}`
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${viewBox}" width="1em" height="1em" fill="currentColor">${icon.body}</svg>`
})

const mergedStyle = computed(() => {
  const out: any = { ...(props.style || {}) }
  if (props.size) {
    const s = typeof props.size === 'number' ? `${props.size}px` : props.size
    out.fontSize = out.fontSize || s
    out.width = out.width || s
    out.height = out.height || s
  }
  return out
})
</script>
