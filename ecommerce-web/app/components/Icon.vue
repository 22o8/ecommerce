<template>
  <Iconify v-if="iconData" :icon="iconData" :class="props.class" :style="props.style" aria-hidden="true" />
  <span v-else aria-hidden="true" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Icon as Iconify } from '@iconify/vue'
import mdi from '@iconify-json/mdi/icons.json'

const props = defineProps<{
  /** e.g. "mdi:account-lock-outline" or "account-lock-outline" */
  name: string
  class?: any
  style?: any
}>()

type IconData = { body: string; width?: number; height?: number }

const iconData = computed<IconData | undefined>(() => {
  const raw = (props.name || '').trim()
  if (!raw) return undefined

  // Accept both "mdi:xxx" and "xxx"
  const key = raw.includes(':') ? raw.split(':').slice(1).join(':') : raw

  // iconify-json format: { prefix, icons: { [name]: { body, width, height } }, width, height }
  const icons = (mdi as any)?.icons || {}
  const commonW = (mdi as any)?.width
  const commonH = (mdi as any)?.height
  const found = icons[key]
  if (!found?.body) return undefined

  return {
    body: found.body,
    width: found.width ?? commonW,
    height: found.height ?? commonH,
  }
})
</script>
