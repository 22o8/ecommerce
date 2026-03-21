<script setup lang="ts">
const route = useRoute()
const id = String(route.params.slug || '')
await navigateTo(id ? `/product/${id}` : '/products', { replace: true })
</script>

<template>
  <div />
</template>
