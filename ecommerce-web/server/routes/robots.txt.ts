export default defineEventHandler((event) => {
  const siteUrl = siteUrlFromConfig()
  const txt = `User-agent: *
Allow: /
Disallow: /admin
Disallow: /cart
Disallow: /checkout
Disallow: /orders

Sitemap: ${siteUrl}/sitemap.xml
`
  setHeader(event, 'content-type', 'text/plain; charset=utf-8')
  return txt
})
