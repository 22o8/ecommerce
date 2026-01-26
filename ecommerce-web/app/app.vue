<script setup lang="ts">
import { useHead, useCookie } from '#app'

type Locale = 'ar' | 'en'
const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
const locale = localeCookie.value === 'en' ? 'en' : 'ar'

useHead({
  htmlAttrs: {
    lang: locale,
    // ✅ نخلي اتجاه الصفحة ثابت LTR حتى ما يتغير ترتيب العناصر عند تبديل اللغة
    dir: 'ltr',
    class: `theme-light ${locale === 'ar' ? 'lang-ar' : 'lang-en'}`,
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
