export default defineEventHandler(async (event) => {
  const site = siteUrlFromConfig()

  // اجلب كل المنتجات قدر الإمكان بدل أول 60 فقط حتى لا ينسى Google أي منتج داخل السايت ماب.
  const all: any[] = []
  const seen = new Set<string>()
  const pageSize = 500

  for (let page = 1; page <= 20; page++) {
    const batch = await safeFetchItems('/Products', { page, pageSize, sort: 'new' })
    if (!batch.length) break

    let added = 0
    for (const p of batch) {
      const key = String(p.id || p.Id || p.slug || p.Slug || '')
      if (!key || seen.has(key)) continue
      seen.add(key)
      all.push(p)
      added += 1
    }

    // إذا رجع أقل من pageSize أو endpoint رجع نفس البيانات، نوقف.
    if (batch.length < pageSize || added === 0) break
  }

  const urls = all
    .map((p: any) => {
      const slug = String(p.slug || p.Slug || p.id || p.Id || '').trim()
      if (!slug) return null
      return {
        loc: `${site}/products/${encodeURIComponent(slug)}`,
        lastmod: p.updatedAt || p.UpdatedAt || p.createdAt || p.CreatedAt,
        changefreq: 'daily',
        priority: '0.9',
      }
    })
    .filter(Boolean) as Array<{ loc: string; lastmod?: string; changefreq?: string; priority?: string }>

  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  setHeader(event, 'cache-control', 'public, max-age=300, s-maxage=300')
  return urlset([{ loc: `${site}/products`, priority: '0.8', changefreq: 'daily' }, ...urls])
})
