// server/routes/sitemap.xml.ts
export default defineEventHandler((event) => {
  const config = useRuntimeConfig()
  const siteUrl = String(config.public.siteUrl || 'http://localhost:3000')

  // روابط ثابتة (المنتجات الديناميكية نضيفها لاحقاً إذا وفّرت endpoint للـ slugs)
  const routes = ['/', '/products', '/login', '/contact', '/terms', '/privacy']

  const now = new Date().toISOString()

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
${routes
  .map(
    (r) => `<url>
  <loc>${siteUrl}${r}</loc>
  <lastmod>${now}</lastmod>
  <changefreq>weekly</changefreq>
  <priority>${r === '/' ? '1.0' : '0.7'}</priority>
</url>`
  )
  .join('\n')}
</urlset>`

  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  return xml
})
