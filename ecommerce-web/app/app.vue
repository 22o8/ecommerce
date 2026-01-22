<script setup lang="ts">
import { useHead } from '#app'

// SSR defaults (client will override via ui.initClient)
useHead({
  htmlAttrs: { lang: 'ar', dir: 'ltr', class: 'theme-light lang-ar' },
})
</script>

<template>
  <div class="min-h-screen bg-app">
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>
