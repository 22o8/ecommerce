// nuxt.config.ts
export default defineNuxtConfig({
  compatibilityDate: '2026-01-05',

  modules: ['@pinia/nuxt', '@nuxtjs/tailwindcss', '@vueuse/nuxt'],
  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    // Server-only backend origin (WITHOUT /api)
    apiOrigin:
      process.env.NUXT_API_ORIGIN ||
      ((process.env.NUXT_PUBLIC_API_ORIGIN || process.env.NUXT_PUBLIC_API_BASE)
        ? (process.env.NUXT_PUBLIC_API_ORIGIN || process.env.NUXT_PUBLIC_API_BASE).replace(/\/api\/?$/, '')
        : undefined) ||
      // ✅ الافتراضي: Render (حسب بنية مشروعك)
      'https://ecommerce-api-22o8.fly.dev',

    public: {
      whatsappPhone: process.env.NUXT_PUBLIC_WHATSAPP_PHONE || '',

      // ✅ client -> /api/bff (Nuxt server) -> API origin
      apiBase: '/api/bff',

      // Public backend origin (WITHOUT /api)
      apiOrigin:
        process.env.NUXT_API_ORIGIN ||
        ((process.env.NUXT_PUBLIC_API_ORIGIN || process.env.NUXT_PUBLIC_API_BASE)
          ? (process.env.NUXT_PUBLIC_API_ORIGIN || process.env.NUXT_PUBLIC_API_BASE).replace(/\/api\/?$/, '')
          : undefined) ||
        'https://ecommerce-api-22o8.fly.dev',

      siteUrl: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000',
      siteName: process.env.NUXT_PUBLIC_SITE_NAME || 'Ecommerce',

      // WhatsApp number (without +)
      whatsappNumber: process.env.NUXT_PUBLIC_WHATSAPP_NUMBER || '9640000000000',

      supportEmail: process.env.NUXT_PUBLIC_SUPPORT_EMAIL || 'info@example.com',
      supportPhone: process.env.NUXT_PUBLIC_SUPPORT_PHONE || '9640000000000',
      instagramUrl: process.env.NUXT_PUBLIC_INSTAGRAM_URL || 'https://instagram.com/',

      heroImage: process.env.NUXT_PUBLIC_HERO_IMAGE || '',
    },
  },

  app: {
    head: {
      // ✅ Arabic first (RTL)
      htmlAttrs: { lang: 'ar', dir: 'rtl', class: 'theme-light lang-ar' },
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'theme-color', content: '#ffffff' },
      ],
      link: [{ rel: 'icon', type: 'image/png', href: '/favicon.png' }],
    },
  },

  nitro: {
    // ✅ أفضل وضوحًا للنشر على Vercel
    preset: 'vercel',
    compressPublicAssets: true,

    routeRules: {
      // ✅ صفحات ديناميكية لازم تكون دائمًا أحدث نسخة
      // حتى ما تحتاج Redeploy حتى يبان المنتج/التعديل.
      '/': { headers: { 'cache-control': 'no-store' } },
      '/products': { headers: { 'cache-control': 'no-store' } },
      '/products/**': { headers: { 'cache-control': 'no-store' } },
      '/services': { headers: { 'cache-control': 'no-store' } },
      '/services/**': { headers: { 'cache-control': 'no-store' } },

      // Long-term cache for built assets
      '/_nuxt/**': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },
      '/icons/**': { headers: { 'cache-control': 'public, max-age=31536000, immutable' } },

      // ✅ كاش للصور
      '/**/*.png': { headers: { 'cache-control': 'public, max-age=2592000' } },
      '/**/*.jpg': { headers: { 'cache-control': 'public, max-age=2592000' } },
      '/**/*.jpeg': { headers: { 'cache-control': 'public, max-age=2592000' } },
      '/**/*.webp': { headers: { 'cache-control': 'public, max-age=2592000' } },
      '/**/*.svg': { headers: { 'cache-control': 'public, max-age=2592000' } },
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
