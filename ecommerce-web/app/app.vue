<script setup lang="ts">
import { useHead, useCookie } from '#app'
import { watch } from 'vue'

type Locale = 'ar' | 'en'
const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
const locale = localeCookie.value === 'en' ? 'en' : 'ar'

const auth = useAuthStore()
const fav = useFavoritesStore()

auth.initFromCookies()

watch(() => auth.isAuthed, async (v) => {
  if (!v) {
    fav.items = []
    return
  }

  try {
    await fav.load()
  } catch {
    // لا نسمح لأي 401 من المفضلة أن يحول الموقع كله إلى صفحة خطأ.
    fav.items = []
  }
}, { immediate: true })

useHead({
  title: 'DR SEOUL BEAUTY',
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
    <AppearanceIntroOverlay />
    <NuxtLayout>
      <NuxtPage :transition="{ name: 'page', mode: 'out-in' }" />
    </NuxtLayout>
  </div>
</template>
