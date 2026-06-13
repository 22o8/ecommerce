export default defineEventHandler(async (event) => {
  const site = siteUrlFromConfig()
  const items = await safeFetchItems('/Products', { page: 1, pageSize: 60, sort: 'new' })
  const urls = items.map((p: any) => ({ loc: `${site}/products/${encodeURIComponent(String(p.slug || p.Slug || p.id || p.Id))}`, lastmod: p.updatedAt || p.UpdatedAt || p.createdAt || p.CreatedAt, changefreq: 'daily', priority: '0.9' }))
  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  return urlset([{ loc: `${site}/products`, priority: '0.8', changefreq: 'daily' }, ...urls])
})
