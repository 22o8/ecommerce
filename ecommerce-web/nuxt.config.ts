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
      'https://localhost:7043',
    public: {
      // Client requests should go through the Nuxt BFF (avoids CORS + keeps tokens server-side).
      apiBase: '/api/bff',
      // Exposed backend origin (WITHOUT /api). Useful for absolute urls when needed.
      apiOrigin:
        process.env.NUXT_API_ORIGIN ||
        (process.env.NUXT_PUBLIC_API_BASE
          ? process.env.NUXT_PUBLIC_API_BASE.replace(/\/api\/?$/, '')
          : undefined) ||
        'https://localhost:7043',
      siteUrl: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000',
      siteName: process.env.NUXT_PUBLIC_SITE_NAME || 'Ecommerce',
      // رقم واتساب لاستلام الطلبات (بدون +). مثال: 9647xxxxxxxxx
      whatsappNumber: process.env.NUXT_PUBLIC_WHATSAPP_NUMBER || '9640000000000',
    },
  },

  app: {
    head: {
      htmlAttrs: { lang: 'en', dir: 'ltr' },
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'theme-color', content: '#0b1220' },
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
