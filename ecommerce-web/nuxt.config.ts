export default defineNuxtConfig({
  compatibilityDate: '2026-01-05',

  modules: [
    '@pinia/nuxt',
    '@nuxtjs/tailwindcss',
  ],

  runtimeConfig: {
    // Server-only (Nitro)
    apiOrigin:
      process.env.API_ORIGIN ||
      process.env.NUXT_API_ORIGIN ||
      process.env.NUXT_PUBLIC_API_ORIGIN ||
      'https://ecommerce-api-endk.onrender.com',

    public: {
      // Client-side
      apiOrigin:
        process.env.API_ORIGIN ||
        process.env.NUXT_PUBLIC_API_ORIGIN ||
        'https://ecommerce-api-endk.onrender.com',
    },
  },
})
