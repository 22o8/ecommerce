export const SITE_FALLBACK = 'https://drseoulbeauty.store'

export function xmlEscape(value: string) {
  return String(value || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&apos;')
}

export function siteUrlFromConfig() {
  const config = useRuntimeConfig()
  return String((config.public as any)?.siteUrl || SITE_FALLBACK).replace(/\/$/, '')
}

export function backendOrigin() {
  const config = useRuntimeConfig()
  return String((config as any)?.apiOrigin || (config.public as any)?.apiOrigin || 'https://ecommerce-api-22o8.fly.dev').replace(/\/$/, '')
}

export function urlset(urls: Array<{ loc: string; lastmod?: string; changefreq?: string; priority?: string }>) {
  const now = new Date().toISOString()
  return `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n${urls.map(u => `<url><loc>${xmlEscape(u.loc)}</loc><lastmod>${xmlEscape(u.lastmod || now)}</lastmod><changefreq>${u.changefreq || 'weekly'}</changefreq><priority>${u.priority || '0.7'}</priority></url>`).join('\n')}\n</urlset>`
}

export async function safeFetchItems(path: string, query: Record<string, any> = {}) {
  try {
    const res: any = await $fetch(`${backendOrigin()}/api${path.startsWith('/') ? path : `/${path}`}`, { query })
    if (Array.isArray(res)) return res
    if (Array.isArray(res?.items)) return res.items
    if (Array.isArray(res?.data)) return res.data
    if (Array.isArray(res?.data?.items)) return res.data.items
  } catch {}
  return []
}
