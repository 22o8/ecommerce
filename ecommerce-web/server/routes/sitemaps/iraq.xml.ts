const citySlugs = ['baghdad', 'basra', 'erbil', 'najaf', 'karbala', 'mosul', 'kirkuk', 'anbar']
export default defineEventHandler((event) => {
  const site = siteUrlFromConfig()
  const urls = [
    { loc: `${site}/iraq`, priority: '0.9', changefreq: 'weekly' },
    ...citySlugs.map(slug => ({ loc: `${site}/iraq/${slug}`, priority: '0.82', changefreq: 'weekly' })),
  ]
  setHeader(event, 'content-type', 'application/xml; charset=utf-8')
  setHeader(event, 'cache-control', 'public, max-age=900, s-maxage=900')
  return urlset(urls)
})
