const blogSlugs = ['best-korean-toner', 'best-acne-serum', 'korean-skincare-routine', 'anua-vs-cosrx']
export default defineEventHandler((event) => {
  const site = siteUrlFromConfig()
  const urls = [{ loc: `${site}/blog`, priority: '0.8', changefreq: 'weekly' }, ...blogSlugs.map(slug => ({ loc: `${site}/blog/${slug}`, priority: '0.75', changefreq: 'monthly' }))]
  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  return urlset(urls)
})
