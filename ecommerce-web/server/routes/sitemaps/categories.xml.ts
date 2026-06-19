export default defineEventHandler(async (event) => {
  const site = siteUrlFromConfig()
  const regular = await safeFetchItems('/Categories', { type: 'regular' })
  const problem = await safeFetchItems('/Categories', { type: 'problem' })
  const urls = [
    ...regular.map((c: any) => ({ loc: `${site}/collections/${encodeURIComponent(String(c.key || c.Key || c.slug || c.Slug || c.id || c.Id).toLowerCase())}`, priority: '0.8' })),
    ...problem.map((c: any) => ({ loc: `${site}/problems/${encodeURIComponent(String(c.key || c.Key || c.slug || c.Slug || c.id || c.Id).toLowerCase())}`, priority: '0.75' })),
  ]
  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  return urlset(urls)
})
