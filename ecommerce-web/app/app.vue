<script setup lang="ts">
import { useHead, useCookie } from '#app'
import BackgroundSparkles from '~/components/BackgroundSparkles.vue'

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
  <div class="min-h-screen bg-app relative overflow-hidden">
    <!-- Solid theme background + subtle sparkles overlay (full viewport) -->
    <BackgroundSparkles />

    <div class="relative z-10">
      <ApiDebugBanner />
      <NuxtLayout>
        <NuxtPage :transition="{ name: 'page', mode: 'out-in' }" />
      </NuxtLayout>
    </div>
  </div>
</template>
