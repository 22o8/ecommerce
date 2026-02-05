<script setup lang="ts">
import { useHead, useCookie } from '#app'

type Locale = 'ar' | 'en'
const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
const locale = localeCookie.value === 'en' ? 'en' : 'ar'

useHead({
  htmlAttrs: {
    lang: locale,
    // Force LTR for all languages (do not switch direction when changing locale)
    dir: 'ltr',
    class: `theme-light ${locale === 'ar' ? 'lang-ar' : 'lang-en'} ltr`,
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
