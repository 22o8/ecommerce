export default defineEventHandler((event) => {
  const site = siteUrlFromConfig()
  const now = new Date().toISOString()
  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <sitemap><loc>${site}/sitemaps/products.xml</loc><lastmod>${now}</lastmod></sitemap>
  <sitemap><loc>${site}/sitemaps/categories.xml</loc><lastmod>${now}</lastmod></sitemap>
  <sitemap><loc>${site}/sitemaps/brands.xml</loc><lastmod>${now}</lastmod></sitemap>
  <sitemap><loc>${site}/sitemaps/blogs.xml</loc><lastmod>${now}</lastmod></sitemap>
</sitemapindex>`
  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  setHeader(event, 'cache-control', 'public, max-age=900, s-maxage=900')
  return xml
})
