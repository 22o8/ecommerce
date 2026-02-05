<script setup lang="ts">
import { useHead, useCookie } from '#app'

type Locale = 'ar' | 'en'
const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
const locale = localeCookie.value === 'en' ? 'en' : 'ar'

useHead({
  htmlAttrs: {
    dir: 'ltr',
    lang: 'ar', // أو 'en' حسب الافتراضي
  },
})
</script>

<template>
  <div class="min-h-screen bg-app">
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>
