<template>
  <!--
    Icon component بدون أي طلبات شبكة.
    يدعم حالياً أيقونات MDI عبر صيغة name="mdi:icon-name".
  -->
  <Iconify
    v-if="icon"
    :icon="icon"
    :class="class"
    :style="style"
    aria-hidden="true"
  />
  <span v-else :class="class" :style="style" aria-hidden="true" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Icon as Iconify } from '@iconify/vue'
import mdiIcons from '@iconify-json/mdi/icons.json'

type Props = {
  name: string
  class?: any
  size?: string | number
}

const props = defineProps<Props>()

const icon = computed(() => {
  const raw = (props.name || '').trim()
  if (!raw) return null

  // نسمح بـ "mdi:account" أو "account" (افتراضياً mdi)
  const [prefixMaybe, nameMaybe] = raw.includes(':') ? raw.split(':', 2) : ['mdi', raw]
  const prefix = (prefixMaybe || 'mdi').toLowerCase()
  const key = (nameMaybe || '').trim()

  if (prefix !== 'mdi') return null
  // @ts-ignore
  const data = (mdiIcons as any)[key]
  if (!data) return null

  // Iconify component يقبل icon data object
  return data
})

const style = computed(() => {
  if (!props.size) return undefined
  const v = typeof props.size === 'number' ? `${props.size}px` : props.size
  return { width: v, height: v }
})
</script>
