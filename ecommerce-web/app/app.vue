<script setup lang="ts">
import { useHead } from '#app'

// ✅ افتراضي: عربي + Light
// نخلي اتجاه الصفحة LTR دائمًا حتى ما يتحرك التصميم يمين/يسار عند تغيير اللغة.
// للغة العربية نعالج اتجاه/محاذاة النصوص عبر CSS (بدون قلب ترتيب العناصر).
useHead({
  htmlAttrs: {
    lang: 'ar',
    dir: 'ltr',
    class: 'theme-light lang-ar',
  },
})
</script>

<template>
  <div class="min-h-screen bg-ambient text-[rgb(var(--text))]">
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>
