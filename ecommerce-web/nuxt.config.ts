// nuxt.config.ts
export default defineNuxtConfig({
  compatibilityDate: '2026-01-05',

  modules: ['@pinia/nuxt', '@nuxtjs/tailwindcss'],
  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    // ✅ Private (server only) base URL for ASP.NET API (WITHOUT trailing /api)
    // Example: https://localhost:7043
    // Server routes will append /api themselves.
    apiOrigin:
      process.env.NUXT_API_ORIGIN ||
      (process.env.NUXT_PUBLIC_API_BASE ? process.env.NUXT_PUBLIC_API_BASE.replace(/\/api\/?$/, '') : undefined) ||
      // الافتراضي يكون الدومن المنشور حتى ما تواجه مشكلة شهادة HTTPS المحلية
      'https://ecommerce-api-endk.onrender.com',
    public: {
      // Client requests should go through the Nuxt BFF (avoids CORS + keeps tokens server-side).
      apiBase: '/api/bff',
      // Exposed backend origin (WITHOUT /api). Useful for absolute urls when needed.
      apiOrigin:
        process.env.NUXT_API_ORIGIN ||
        (process.env.NUXT_PUBLIC_API_BASE
          ? process.env.NUXT_PUBLIC_API_BASE.replace(/\/api\/?$/, '')
          : undefined) ||
      // الافتراضي يكون الدومن المنشور حتى ما تواجه مشكلة شهادة HTTPS المحلية
      'https://ecommerce-api-endk.onrender.com',
      siteUrl: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000',
      siteName: process.env.NUXT_PUBLIC_SITE_NAME || 'Ecommerce',
      // رقم واتساب لاستلام الطلبات (بدون +). مثال: 9647xxxxxxxxx
      whatsappNumber: process.env.NUXT_PUBLIC_WHATSAPP_NUMBER || '9640000000000',

      // معلومات تواصل (تظهر في الفوتر والصفحة الرئيسية)
      supportEmail: process.env.NUXT_PUBLIC_SUPPORT_EMAIL || 'info@example.com',
      supportPhone: process.env.NUXT_PUBLIC_SUPPORT_PHONE || '9640000000000',
      instagramUrl: process.env.NUXT_PUBLIC_INSTAGRAM_URL || 'https://instagram.com/',

      // صور الهيرو (اختياري) — ضع روابط صورك هنا (أو مسارات من /public)
      // مثال: /hero/hero.jpg أو https://...
      heroImage: process.env.NUXT_PUBLIC_HERO_IMAGE || '',
    },
  },

  app: {
    head: {
      // الافتراضي عربي + اتجاه الصفحة LTR (حتى ما ينقلب التصميم عند تبديل اللغة)
      htmlAttrs: { lang: 'ar', dir: 'ltr', class: 'theme-light lang-ar' },
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'theme-color', content: '#ffffff' },
      ],
      link: [{ rel: 'icon', type: 'image/png', href: '/favicon.png' }],
    },
  },

  vite: {
    define: {
      __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: false,
    },
  },
})
