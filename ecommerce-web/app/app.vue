<script setup lang="ts">
import { useHead, useCookie, useRoute } from '#app'
import { nextTick, watch } from 'vue'

type Locale = 'ar' | 'en'
const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
const locale = localeCookie.value === 'en' ? 'en' : 'ar'
const route = useRoute()

useHead({
  htmlAttrs: {
    lang: locale,
    dir: 'ltr',
  },
})

if (import.meta.client) {
  if ('scrollRestoration' in window.history) {
    window.history.scrollRestoration = 'manual'
  }

  watch(
    () => route.fullPath,
    async (newPath) => {
      await nextTick()
      if (newPath.includes('#')) return
      window.scrollTo({ top: 0, left: 0, behavior: 'auto' })
      document.documentElement.scrollTop = 0
      document.body.scrollTop = 0
    },
    { flush: 'post' }
  )
}
</script>

<template>
  <div class="min-h-screen bg-app">
    <ApiDebugBanner />
    <ToastHost />
    <NuxtLayout>
      <NuxtPage :transition="{ name: 'page', mode: 'out-in' }" />
    </NuxtLayout>
  </div>
</template>
