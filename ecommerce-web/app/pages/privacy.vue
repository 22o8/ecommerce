<!-- app/pages/privacy.vue -->
<template>
  <div class="mx-auto max-w-3xl rounded-3xl border border-white/10 bg-white/5 p-8">
    <h1 class="text-2xl font-black">{{ t('privacyTitle') }}</h1>
    <p class="mt-4 text-sm text-white/70">
      ضع نص سياسة الخصوصية هنا.
    </p>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from '~/composables/useI18n'
import { useSiteMeta } from '~/composables/useSiteMeta'

const { t } = useI18n()
useSiteMeta({ title: `${t('privacy')} | Ecommerce`, description: 'Privacy policy', path: '/privacy' })
</script>
