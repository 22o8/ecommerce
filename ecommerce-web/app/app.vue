<script setup lang="ts">
import { useHead, useCookie } from '#app'
import PaperSilkBackground from '~/components/PaperSilkBackground.vue'

type Locale = 'ar' | 'en'
const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
const locale = localeCookie.value === 'en' ? 'en' : 'ar'

useHead({
  htmlAttrs: {
    // Keep layout stable: always LTR and do not change structural classes when locale changes.
    lang: locale,
    dir: 'ltr',
    // Theme classes are managed by the UI store (theme-light/theme-dark).
    class: 'ltr',
  },
})
</script>

<template>
  <div class="min-h-screen bg-app relative overflow-hidden">
    <!-- Global paper/silk texture + scroll-tinted overlay (covers full page/scroll) -->
    <PaperSilkBackground />

    <div class="relative z-10">
      <ApiDebugBanner />
      <NuxtLayout>
        <NuxtPage :transition="{ name: 'page', mode: 'out-in' }" />
      </NuxtLayout>
    </div>
  </div>
</template>
