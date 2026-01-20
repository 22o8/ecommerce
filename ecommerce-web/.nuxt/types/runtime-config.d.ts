import { RuntimeConfig as UserRuntimeConfig, PublicRuntimeConfig as UserPublicRuntimeConfig } from 'nuxt/schema'
  interface SharedRuntimeConfig {
   app: {
      buildId: string,

      baseURL: string,

      buildAssetsDir: string,

      cdnURL: string,
   },

   apiOrigin: string,

   nitro: {
      envPrefix: string,
   },
  }
  interface SharedPublicRuntimeConfig {
   apiBase: string,

   apiOrigin: string,

   siteUrl: string,

   siteName: string,

   whatsappNumber: string,

   supportEmail: string,

   supportPhone: string,

   instagramUrl: string,

   heroImage: string,
  }
declare module '@nuxt/schema' {
  interface RuntimeConfig extends UserRuntimeConfig {}
  interface PublicRuntimeConfig extends UserPublicRuntimeConfig {}
}
declare module 'nuxt/schema' {
  interface RuntimeConfig extends SharedRuntimeConfig {}
  interface PublicRuntimeConfig extends SharedPublicRuntimeConfig {}
}
declare module 'vue' {
        interface ComponentCustomProperties {
          $config: UserRuntimeConfig
        }
      }