const blogSlugs = [
  'best-korean-toner-iraq',
  'best-acne-serum-iraq',
  'korean-skincare-routine-iraq',
  'anua-vs-cosrx-iraq',
  'best-collagen-serum-iraq',
]
export default defineEventHandler((event) => {
  const site = siteUrlFromConfig()
  const urls = [
    { loc: `${site}/blog`, priority: '0.85', changefreq: 'weekly' },
    ...blogSlugs.map(slug => ({ loc: `${site}/blog/${slug}`, priority: '0.78', changefreq: 'monthly' })),
  ]
  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  setHeader(event, 'cache-control', 'public, max-age=900, s-maxage=900')
  return urlset(urls)
})
