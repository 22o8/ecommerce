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
    // ⚠️ لا نثبت class هنا حتى لا نمسح classes التي يطبّقها uiStore + appearance plugin
    // (مثل theme-dark/theme-light + theme-ramadan/theme-blackFriday ...)
  },
})
</script>

<template>
  <div class="min-h-screen bg-app">
    <ApiDebugBanner />
    <ToastHost />
    <NuxtLayout>
      <NuxtPage :transition="{ name: 'page', mode: 'out-in' }" />
    </NuxtLayout>
    <ScrollToTopButton />
  </div>
</template>
