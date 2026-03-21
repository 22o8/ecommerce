<template>
  <footer class="mt-12 border-t border-app">
    <div class="mx-auto max-w-7xl px-4 py-10 grid gap-8 md:grid-cols-3">
      <div class="grid gap-3">
        <div class="flex items-center gap-3">
          <div class="h-10 w-10 rounded-2xl bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] grid place-items-center font-black overflow-hidden">
            <img v-if="siteLogoSrc !== '#'" :src="siteLogoSrc" alt="Site logo" class="h-full w-full object-cover" />
            <Icon v-else name="mdi:storefront-outline" class="text-xl" />
          </div>
        </div>
      </div>

      <div class="grid gap-2 text-sm">
        <div class="font-bold rtl-text">{{ t('quickLinks') }}</div>
        	<NuxtLink to="/products" class="text-muted hover:opacity-90 rtl-text">{{ t('nav.products') }}</NuxtLink>
        <NuxtLink to="/orders" class="text-muted hover:opacity-90 rtl-text">{{ t('myOrders') }}</NuxtLink>
        <NuxtLink to="/contact" class="text-muted hover:opacity-90 rtl-text">{{ t('contact') }}</NuxtLink>
        <NuxtLink to="/terms" class="text-muted hover:opacity-90 rtl-text">{{ t('terms') }}</NuxtLink>
        <NuxtLink to="/privacy" class="text-muted hover:opacity-90 rtl-text">{{ t('privacy') }}</NuxtLink>
      </div>

      <div class="grid gap-3 text-sm">
        <div class="font-bold rtl-text">{{ t('contact') }}</div>
        <a class="text-muted hover:opacity-90 keep-ltr" :href="`tel:${supportPhone}`">
          <Icon name="mdi:phone-outline" class="inline-block text-lg align-middle" />
          <span class="ml-2">{{ supportPhone }}</span>
        </a>
        <a class="text-muted hover:opacity-90 keep-ltr" :href="`mailto:${supportEmail}`">
          <Icon name="mdi:email-outline" class="inline-block text-lg align-middle" />
          <span class="ml-2">{{ supportEmail }}</span>
        </a>
        <a class="text-muted hover:opacity-90 keep-ltr" :href="instagramUrl" target="_blank" rel="noreferrer">
          <Icon name="mdi:instagram" class="inline-block text-lg align-middle" />
          <span class="ml-2">Instagram</span>
        </a>
      </div>
    </div>

    <div class="grad-line mx-auto max-w-7xl" />
    <div class="mx-auto max-w-7xl px-4 py-6 text-xs text-muted flex items-center justify-between">
      <div class="rtl-text">© {{ new Date().getFullYear() }}</div>
      <div class="footer-end-mark" aria-hidden="true"></div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import siteLogoSrc from '~/assets/img/site-logo.jpg'
const { t } = useI18n()
const config = useRuntimeConfig()
const supportEmail = String((config.public as any).supportEmail || '')
const supportPhone = String((config.public as any).supportPhone || '')
const instagramUrl = String((config.public as any).instagramUrl || '')

</script>


<style scoped>
.footer-end-mark{
  width: 2.85rem;
  height: 2.85rem;
  border-radius: 999px;
  border: 1px dashed rgba(var(--border), .55);
  opacity: .35;
}
</style>
