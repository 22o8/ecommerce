export type SeoInput = {
  title?: string
  description?: string
  keywords?: string[] | string
  image?: string
  url?: string
  type?: string
  canonical?: string
  noindex?: boolean
  schema?: any | any[]
}

const STORE_NAME = 'DR SEOUL BEAUTY'
const DEFAULT_DESC = 'متجر DR SEOUL BEAUTY لمنتجات العناية بالبشرة والتجميل الكوري الأصلية المختارة بعناية في العراق.'

function stripHtml(value?: string | null) {
  return String(value || '').replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim()
}

function truncate(value: string, max = 155) {
  const text = stripHtml(value)
  return text.length > max ? `${text.slice(0, max - 1).trim()}…` : text
}

export function absoluteUrl(path?: string | null) {
  const config = useRuntimeConfig()
  const siteUrl = String((config.public as any)?.siteUrl || 'https://drseoulbeauty.store').replace(/\/$/, '')
  const value = String(path || '').trim()
  if (!value) return siteUrl
  if (/^https?:\/\//i.test(value)) return value
  return `${siteUrl}${value.startsWith('/') ? value : `/${value}`}`
}

export function productSlug(product: any) {
  return String(product?.slug || product?.Slug || product?.id || product?.Id || '').trim()
}

export function productCanonical(product: any) {
  const slug = productSlug(product)
  return absoluteUrl(`/products/${encodeURIComponent(slug)}`)
}

export function productSeoKeywords(product: any) {
  const title = String(product?.title || product?.Title || product?.name || '').trim()
  const brand = String(product?.brand || product?.Brand || '').trim()
  const category = String(product?.category || product?.Category || '').trim()
  const sub = String(product?.subCategory || product?.SubCategory || '').trim()
  return [title, brand, category, sub, 'korean skincare', 'k beauty iraq', 'DR SEOUL BEAUTY'].filter(Boolean)
}

export function buildOrganizationSchema() {
  const config = useRuntimeConfig()
  const siteUrl = absoluteUrl('/')
  const instagram = String((config.public as any)?.instagramUrl || '').trim()
  const phone = String((config.public as any)?.supportPhone || '').trim()
  const email = String((config.public as any)?.supportEmail || '').trim()
  return {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: STORE_NAME,
    url: siteUrl,
    logo: absoluteUrl('/favicon.ico'),
    sameAs: [instagram].filter(Boolean),
    contactPoint: [{ '@type': 'ContactPoint', telephone: phone, email, contactType: 'customer support', areaServed: 'IQ', availableLanguage: ['Arabic', 'English'] }],
  }
}

export function buildBreadcrumbSchema(items: Array<{ name: string; item?: string }>) {
  return {
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: items.map((x, i) => ({ '@type': 'ListItem', position: i + 1, name: x.name, item: x.item || absoluteUrl('/') })),
  }
}

export function buildProductSchema(product: any, image?: string) {
  const title = String(product?.title || product?.Title || product?.name || STORE_NAME)
  const description = truncate(product?.description || product?.Description || DEFAULT_DESC, 500)
  const price = Number(product?.finalPriceIqd ?? product?.priceIqd ?? product?.PriceIqd ?? product?.price ?? 0)
  const ratingAvg = Number(product?.ratingAvg ?? product?.RatingAvg ?? 0)
  const ratingCount = Number(product?.ratingCount ?? product?.RatingCount ?? 0)
  const reviews = Array.isArray(product?.reviews) ? product.reviews : []
  return {
    '@context': 'https://schema.org',
    '@type': 'Product',
    name: title,
    description,
    image: image ? [image] : undefined,
    brand: { '@type': 'Brand', name: String(product?.brand || product?.Brand || STORE_NAME) },
    sku: String(product?.id || product?.Id || productSlug(product)),
    offers: {
      '@type': 'Offer',
      url: productCanonical(product),
      priceCurrency: 'IQD',
      price: price > 0 ? price : undefined,
      availability: Number(product?.stockQuantity ?? product?.StockQuantity ?? 0) > 0 ? 'https://schema.org/InStock' : 'https://schema.org/OutOfStock',
      itemCondition: 'https://schema.org/NewCondition',
    },
    aggregateRating: ratingCount > 0 ? { '@type': 'AggregateRating', ratingValue: ratingAvg || 5, reviewCount: ratingCount } : undefined,
    review: reviews.slice(0, 5).map((r: any) => ({ '@type': 'Review', reviewRating: { '@type': 'Rating', ratingValue: Number(r.rating || 5) }, author: { '@type': 'Person', name: r.userName || 'Customer' }, reviewBody: r.comment || '' })).filter((r: any) => r.reviewBody),
  }
}

export function buildFaqSchema(faq: Array<{ question: string; answer: string }>) {
  const valid = (faq || []).filter(x => x?.question && x?.answer)
  if (!valid.length) return null
  return { '@context': 'https://schema.org', '@type': 'FAQPage', mainEntity: valid.map(x => ({ '@type': 'Question', name: x.question, acceptedAnswer: { '@type': 'Answer', text: x.answer } })) }
}

export function useAdvancedSeo(input: SeoInput) {
  const config = useRuntimeConfig()
  const siteName = String((config.public as any)?.siteName || STORE_NAME)
  const title = input.title?.includes(siteName) ? input.title : `${input.title || siteName} | ${siteName}`
  const description = truncate(input.description || DEFAULT_DESC)
  const canonical = input.canonical || input.url || absoluteUrl('/')
  const image = absoluteUrl(input.image || '/og-image.png')
  const keywords = Array.isArray(input.keywords) ? input.keywords.join(', ') : String(input.keywords || 'DR SEOUL BEAUTY, Korean skincare Iraq, K beauty')
  const schemas = [buildOrganizationSchema(), ...(Array.isArray(input.schema) ? input.schema : (input.schema ? [input.schema] : []))].filter(Boolean)

  useHead({
    title,
    meta: [
      { name: 'description', content: description },
      { name: 'keywords', content: keywords },
      input.noindex ? { name: 'robots', content: 'noindex,nofollow' } : { name: 'robots', content: 'index,follow,max-image-preview:large' },
      { property: 'og:type', content: input.type || 'website' },
      { property: 'og:title', content: title },
      { property: 'og:description', content: description },
      { property: 'og:image', content: image },
      { property: 'og:url', content: canonical },
      { property: 'og:site_name', content: siteName },
      { name: 'twitter:card', content: 'summary_large_image' },
      { name: 'twitter:title', content: title },
      { name: 'twitter:description', content: description },
      { name: 'twitter:image', content: image },
    ],
    link: [{ rel: 'canonical', href: canonical }],
    script: schemas.map((schema) => ({ type: 'application/ld+json', children: JSON.stringify(schema).replace(/</g, '\\u003c') })),
  })
}

export function seoDescription(value?: string | null, fallback = DEFAULT_DESC) { return truncate(value || fallback) }
