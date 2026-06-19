export default defineEventHandler(async (event) => {
  const site = siteUrlFromConfig()
  const items = await safeFetchItems('/Brands', { take: 1000 })
  const urls = items.map((b: any) => ({ loc: `${site}/brands/${encodeURIComponent(String(b.slug || b.Slug || b.name || b.Name).toLowerCase())}`, lastmod: b.updatedAt || b.UpdatedAt || b.createdAt || b.CreatedAt, changefreq: 'weekly', priority: '0.8' }))
  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  return urlset([{ loc: `${site}/brands`, priority: '0.8' }, ...urls])
})
