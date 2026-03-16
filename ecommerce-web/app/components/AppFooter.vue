<template>
  <footer class="mt-16 px-3 pb-6 sm:px-4">
    <div class="footer-shell mx-auto max-w-7xl overflow-hidden rounded-[30px] border border-app">
      <div class="footer-shell__glow footer-shell__glow--one" />
      <div class="footer-shell__glow footer-shell__glow--two" />

      <div class="relative z-[1] mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <div class="grid gap-8 md:grid-cols-[1.2fr_.9fr_.9fr]">
          <div class="grid gap-4">
            <div class="flex items-center gap-3">
              <div class="footer-brand__logo">
                <img :src="siteLogoSrc" alt="Site logo" class="h-full w-full object-cover" />
              </div>
              <div>
                <div class="text-lg font-extrabold tracking-[0.04em]">ECOMMERCE</div>
                <div class="text-sm text-muted rtl-text">{{ t('tagline') }}</div>
              </div>
            </div>
            <p class="max-w-md text-sm leading-7 text-muted rtl-text">
              {{ t('homeHero.subtitle') }}
            </p>
          </div>

          <div class="grid gap-2 text-sm">
            <div class="font-bold rtl-text">{{ t('quickLinks') }}</div>
            <NuxtLink to="/products" class="footer-link rtl-text">{{ t('nav.products') }}</NuxtLink>
            <NuxtLink to="/orders" class="footer-link rtl-text">{{ t('myOrders') }}</NuxtLink>
            <NuxtLink to="/contact" class="footer-link rtl-text">{{ t('contact') }}</NuxtLink>
            <NuxtLink to="/terms" class="footer-link rtl-text">{{ t('terms') }}</NuxtLink>
            <NuxtLink to="/privacy" class="footer-link rtl-text">{{ t('privacy') }}</NuxtLink>
          </div>

          <div class="grid gap-3 text-sm">
            <div class="font-bold rtl-text">{{ t('contact') }}</div>
            <a class="footer-link keep-ltr" :href="`tel:${supportPhone}`">
              <Icon name="mdi:phone-outline" class="inline-block text-lg align-middle" />
              <span class="ml-2">{{ supportPhone }}</span>
            </a>
            <a class="footer-link keep-ltr" :href="`mailto:${supportEmail}`">
              <Icon name="mdi:email-outline" class="inline-block text-lg align-middle" />
              <span class="ml-2">{{ supportEmail }}</span>
            </a>
            <a class="footer-link keep-ltr" :href="instagramUrl" target="_blank" rel="noreferrer">
              <Icon name="mdi:instagram" class="inline-block text-lg align-middle" />
              <span class="ml-2">Instagram</span>
            </a>
          </div>
        </div>

        <div class="mt-8 flex flex-col gap-4 border-t border-app/80 pt-6 text-xs text-muted sm:flex-row sm:items-center sm:justify-between">
          <div class="rtl-text">© {{ new Date().getFullYear() }} ECOMMERCE</div>
          <a class="keep-ltr text-muted hover:opacity-90" href="#" @click.prevent="toTop">
            <Icon name="mdi:arrow-up" class="inline-block text-lg" />
          </a>
        </div>
      </div>
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

function toTop() { if (import.meta.client) window.scrollTo({ top: 0, behavior: 'smooth' }) }
</script>

<style scoped>
.footer-shell{
  position: relative;
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .94), rgba(var(--surface-2-rgb), .84)),
    linear-gradient(135deg, rgba(var(--primary), .06), transparent 32%, rgba(var(--cta-glow-2), .06) 100%);
  box-shadow: 0 20px 50px rgba(3, 8, 20, .18);
}
.footer-shell__glow{
  position:absolute;
  border-radius:999px;
  filter:blur(18px);
  pointer-events:none;
}
.footer-shell__glow--one{
  inset-inline-start:-40px;
  top:-34px;
  width:180px;
  height:120px;
  background: radial-gradient(circle, rgba(var(--primary), .14), transparent 72%);
}
.footer-shell__glow--two{
  inset-inline-end:0;
  bottom:-40px;
  width:220px;
  height:120px;
  background: radial-gradient(circle, rgba(var(--cta-glow-2), .10), transparent 72%);
}
.footer-brand__logo{
  width:48px;
  height:48px;
  overflow:hidden;
  border-radius:18px;
  border:1px solid rgba(255,255,255,.14);
  box-shadow: 0 12px 24px rgba(0,0,0,.18);
}
.footer-link{
  color: rgb(var(--muted));
  transition: opacity .2s ease, color .2s ease, transform .2s ease;
}
.footer-link:hover{
  color: rgb(var(--text));
  transform: translateX(-2px);
}
:global(html.theme-light) .footer-shell{
  background:
    linear-gradient(180deg, rgba(255,255,255,.96), rgba(255,247,252,.92)),
    linear-gradient(135deg, rgba(232,91,154,.06), transparent 32%, rgba(246,180,212,.06) 100%);
  box-shadow: 0 18px 44px rgba(232,91,154,.10), 0 10px 26px rgba(18,18,18,.05);
}
</style>
