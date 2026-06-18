// nuxt.config.ts
export default defineNuxtConfig({
  compatibilityDate: '2026-01-05',

  modules: ['@pinia/nuxt', '@nuxtjs/tailwindcss', '@vueuse/nuxt', '@nuxt/image'],
  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    // Server-only backend origin (WITHOUT /api)
    apiOrigin:
      process.env.NUXT_API_ORIGIN ||
      (process.env.NUXT_PUBLIC_API_BASE
        ? process.env.NUXT_PUBLIC_API_BASE.replace(/\/api\/?$/, '')
        : undefined) ||
      // ✅ الافتراضي: Render (حسب بنية مشروعك)
      'https://ecommerce-api-22o8.fly.dev',

    public: {
      // سعر التحويل الافتراضي (قابل للتعديل من ENV)
      // ملاحظة: الأسعار المخزنة عندك حالياً تبدو بالدولار، وهنا نعرضها بالدينار.
      usdToIqdRate: Number(process.env.NUXT_PUBLIC_USD_TO_IQD_RATE || 1300),
      // WhatsApp number (بدون +). غيّره لاحقًا من ENV بدون تعديل كود.
      // Preferred: NUXT_PUBLIC_WHATSAPP_NUMBER=9647500157600
      whatsappPhone: process.env.NUXT_PUBLIC_WHATSAPP_PHONE || '',

      // ✅ الافتراضي (الأفضل على Vercel):
      // المتصفح يتكلم مع نفس الدومين /api/bff (Nuxt server) ثم Nuxt يمرر للباك
      // لتفادي CORS و مشاكل 404 لما ينحط أصل الباك بدون /api
      apiBase: process.env.NUXT_PUBLIC_API_BASE
        ? (process.env.NUXT_PUBLIC_API_BASE.startsWith('http') ? '/api/bff' : process.env.NUXT_PUBLIC_API_BASE)
        : '/api/bff',

      // Public backend origin (WITHOUT /api)
      apiOrigin:
        process.env.NUXT_API_ORIGIN ||
        (process.env.NUXT_PUBLIC_API_BASE
          ? process.env.NUXT_PUBLIC_API_BASE.replace(/\/api\/?$/, '')
          : undefined) ||
        'https://ecommerce-api-22o8.fly.dev',

      siteUrl: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000',
      siteName: process.env.NUXT_PUBLIC_SITE_NAME || 'DR SEOUL BEAUTY',

      // WhatsApp number (without +)
      // ✅ نقرأ من NUXT_PUBLIC_WHATSAPP_NUMBER (أو القديم NUXT_PUBLIC_WHATSAPP_PHONE) ونضع رقمك كافتراضي
      whatsappNumber:
        process.env.NUXT_PUBLIC_WHATSAPP_NUMBER ||
        process.env.NUXT_PUBLIC_WHATSAPP_PHONE ||
        '9647500157600',

      supportEmail: process.env.NUXT_PUBLIC_SUPPORT_EMAIL || 'dr@drseoulbeauty.store',
      supportPhone: process.env.NUXT_PUBLIC_SUPPORT_PHONE || '07500157600',
      instagramUrl: process.env.NUXT_PUBLIC_INSTAGRAM_URL || 'https://www.instagram.com/dr.seoul_beauty?utm_source=ig_web_button_share_sheet&igsh=ZDNlZDc0MzIxNw==',

      heroImage: process.env.NUXT_PUBLIC_HERO_IMAGE || '',
      gaId: process.env.NUXT_PUBLIC_GA_ID || '',
      r2PublicUrl: process.env.R2_PUBLIC_URL || process.env.NUXT_PUBLIC_R2_PUBLIC_URL || '',
      r2LegacyPublicUrl: process.env.R2_LEGACY_PUBLIC_URL || process.env.NUXT_PUBLIC_R2_LEGACY_PUBLIC_URL || 'https://pub-8972ea047b9a4be1a8fdddc3c996a48d.r2.dev',
    },
  },

  app: {
    pageTransition: { name: 'page', mode: 'out-in' },
    layoutTransition: { name: 'layout', mode: 'out-in' },
    head: {
      title: 'DR SEOUL BEAUTY',
      titleTemplate: '%s',
      // ✅ Arabic first (RTL)
      // Force LTR for all locales.
      htmlAttrs: { lang: 'ar', dir: 'rtl', class: 'theme-light' },
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'theme-color', content: '#a78bfa' },
        { name: 'description', content: 'DR SEOUL BEAUTY - متجر كوزمتك ومنتجات عناية مختارة بعناية.' },
        { property: 'og:type', content: 'website' },
        { property: 'og:title', content: 'DR SEOUL BEAUTY' },
        { property: 'og:description', content: 'متجر كوزمتك ومنتجات عناية مختارة بعناية.' },
        { property: 'og:image', content: '/og-image.png' },
        { name: 'twitter:card', content: 'summary_large_image' },
        { name: 'twitter:title', content: 'DR SEOUL BEAUTY' },
        { name: 'twitter:description', content: 'متجر كوزمتك ومنتجات عناية مختارة بعناية.' },
        { name: 'twitter:image', content: '/og-image.png' },
        { name: 'apple-mobile-web-app-capable', content: 'yes' },
        { name: 'apple-mobile-web-app-status-bar-style', content: 'black-translucent' },
        { name: 'apple-mobile-web-app-title', content: 'DR SEOUL BEAUTY' },
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico?v=drseoul-20260605' },
        { rel: 'shortcut icon', type: 'image/x-icon', href: '/favicon.ico?v=drseoul-20260605' },
        { rel: 'manifest', href: '/manifest.webmanifest' },
        { rel: 'apple-touch-icon', sizes: '180x180', href: '/apple-touch-icon.png' },
      ],
      script: [{ children: "if('serviceWorker' in navigator){window.addEventListener('load',()=>navigator.serviceWorker.register('/sw.js').catch(()=>{}))}" }],
    },
  },


  image: {
    quality: 80,
    format: ['avif', 'webp'],
    screens: {
      xs: 320,
      sm: 640,
      md: 768,
      lg: 1024,
      xl: 1280,
      xxl: 1536,
    },
    domains: ['drseoulbeauty.store', 'api.drseoulbeauty.store', 'ecommerce-api-22o8.fly.dev', 'img.drseoulbeauty.store', 'pub-8972ea047b9a4be1a8fdddc3c996a48d.r2.dev'],
  },

  nitro: {
    // ✅ أفضل وضوحًا للنشر على Vercel
    preset: 'vercel',
    compressPublicAssets: true,

    routeRules: {
      // ✅ صفحات ديناميكية لازم تكون دائمًا أحدث نسخة
      // حتى ما تحتاج Redeploy حتى يبان المنتج/التعديل.
      '/': { headers: { 'cache-control': 'no-store' } },
      '/products': { headers: { 'cache-control': 'public, max-age=120, s-maxage=300, stale-while-revalidate=600' } },
      '/products/**': { headers: { 'cache-control': 'public, max-age=120, s-maxage=300, stale-while-revalidate=600' } },
      '/iraq': { headers: { 'cache-control': 'no-store' } },
      '/iraq/**': { headers: { 'cache-control': 'no-store' } },
      '/intro': { headers: { 'cache-control': 'no-store' } },
      '/ios-install': { headers: { 'cache-control': 'no-store' } },
      '/services': { headers: { 'cache-control': 'no-store' } },
      '/services/**': { headers: { 'cache-control': 'no-store' } },

      // Long-term cache for built assets
      '/_nuxt/**': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },
      '/icons/**': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },
      '/api/bff/uploads/**': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },
      '/api/uploads/**': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },

      // ✅ كاش للصور
      '/**/*.png': { headers: { 'cache-control': 'public, max-age=2592000, stale-while-revalidate=86400' } },
      '/**/*.jpg': { headers: { 'cache-control': 'public, max-age=2592000, stale-while-revalidate=86400' } },
      '/**/*.jpeg': { headers: { 'cache-control': 'public, max-age=2592000, stale-while-revalidate=86400' } },
      '/**/*.webp': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },
      '/**/*.avif': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },
      '/**/*.svg': { headers: { 'cache-control': 'public, max-age=2592000' } },
      '/favicon.ico': { headers: { 'cache-control': 'no-cache, no-store, must-revalidate' } },
      '/**/*.ico': { headers: { 'cache-control': 'public, max-age=2592000' } },

      // NOTE: لا نستخدم SWR للمنتجات حتى ما يصير تأخير/بيانات قديمة
    },
  },

  experimental: {
    payloadExtraction: true,
    inlineSSRStyles: false,
  },

  vite: {
    define: {
      __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: false,
    },
  },
})
