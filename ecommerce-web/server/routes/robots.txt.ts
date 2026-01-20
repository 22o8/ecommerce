// server/routes/robots.txt.ts
export default defineEventHandler((event) => {
  const config = useRuntimeConfig()
  const siteUrl = String(config.public.siteUrl || 'http://localhost:3000')

  const txt = `User-agent: *
Allow: /
Sitemap: ${siteUrl}/sitemap.xml
`
  setHeader(event, 'content-type', 'text/plain; charset=utf-8')
  return txt
})
