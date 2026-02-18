<script setup lang="ts">
import { useHead, useCookie } from '#app'

type Locale = 'ar' | 'en'
const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
const locale = localeCookie.value === 'en' ? 'en' : 'ar'

useHead({
  htmlAttrs: {
    // Keep layout stable: always LTR and do not change structural classes when locale changes.
    lang: locale,
    dir: 'ltr',
    class: 'theme-light ltr',
  },
})
</script>

<template>
  <div class="min-h-screen bg-app">
    <ApiDebugBanner />
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>
