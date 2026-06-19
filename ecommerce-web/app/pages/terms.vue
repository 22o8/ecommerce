<template>
  <div class="card-soft p-6 md:p-10">
    <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ t('terms') }}</h1>
    <div class="mt-3 text-muted rtl-text">
      <slot />
      <p v-if="placeholder">{{ placeholder }}</p>
    </div>
    <div v-if="$route.path==='/'" />
    <div class="mt-6">
      <NuxtLink to="/">
        <UiButton variant="secondary">
          <Icon name="mdi:home-outline" class="text-lg" />
          <span class="rtl-text">{{ t('nav.home') }}</span>
        </UiButton>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
const { t } = useI18n()
const placeholder = computed(() => {
  const map:any = {
    'terms': t('termsPlaceholder'),
    'privacy': t('privacyPlaceholder'),
    'account': t('accountPlaceholder'),
    'notFound': t('notFound'),
  }
  return map['terms'] || ''
})
</script>
