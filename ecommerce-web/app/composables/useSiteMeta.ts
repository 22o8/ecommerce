// app/composables/useSiteMeta.ts
import { useRuntimeConfig, useHead } from '#app'

export function useSiteMeta(input: {
  title: string
  description?: string
  path?: string
  image?: string
}) {
  const config = useRuntimeConfig()
  const siteUrl = String(config.public.siteUrl || 'http://localhost:3000')
  const siteName = String(config.public.siteName || 'Ecommerce')

  const url = input.path ? `${siteUrl}${input.path}` : siteUrl

  useHead({
    title: input.title,
    meta: [
      { name: 'description', content: input.description || '' },
      { property: 'og:site_name', content: siteName },
      { property: 'og:title', content: input.title },
      { property: 'og:description', content: input.description || '' },
      { property: 'og:url', content: url },
      { property: 'og:type', content: 'website' },
      ...(input.image ? [{ property: 'og:image', content: input.image }] : []),
      { name: 'twitter:card', content: 'summary_large_image' },
    ],
    link: [{ rel: 'canonical', href: url }],
  })
}
