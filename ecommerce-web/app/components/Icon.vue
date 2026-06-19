<script setup lang="ts">
import { Icon as IconifyIcon } from '@iconify/vue'

defineProps<{
  name: string
  class?: string
}>()
</script>

<template>
  <ClientOnly>
    <IconifyIcon :icon="name" :class="class" />
  </ClientOnly>
</template>
