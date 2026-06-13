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
const DEFAULT_DESC = 'متجر DR SEOUL BEAUTY لمنتجات العناية بالبشرة والتجميل الكوري الأصلية المختارة بعناية في العراق مع شحن لجميع المحافظات.'

function stripHtml(value?: string | null) {
  return String(value || '').replace(/<script[\s\S]*?<\/script>/gi, ' ').replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim()
}

function truncate(value: string, max = 155) {
  const text = stripHtml(value)
  return text.length > max ? `${text.slice(0, max - 1).trim()}…` : text
}

function compactObject(value: any): any {
  if (Array.isArray(value)) {
    return value.map(compactObject).filter((x) => x !== undefined && x !== null && x !== '' && !(Array.isArray(x) && !x.length))
  }
  if (value && typeof value === 'object') {
    const out: Record<string, any> = {}
    for (const [k, v] of Object.entries(value)) {
      const next = compactObject(v)
      if (next !== undefined && next !== null && next !== '' && !(Array.isArray(next) && !next.length)) out[k] = next
    }
    return out
  }
  return value
}

function normalizeKeyword(value?: string | null) {
  return stripHtml(value).toLowerCase().replace(/\s+/g, ' ').trim()
}

function unique(values: Array<string | undefined | null>) {
  const seen = new Set<string>()
  const out: string[] = []
  for (const value of values) {
    const text = stripHtml(value)
    const key = normalizeKeyword(text)
    if (!key || seen.has(key)) continue
    seen.add(key)
    out.push(text)
  }
  return out
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
  const extra = Array.isArray(product?.keywords) ? product.keywords : String(product?.keywords || '').split(/[،,\n]/)
  return unique([
    title,
    brand,
    category,
    sub,
    ...extra,
    `${title} العراق`,
    brand ? `${brand} Iraq` : '',
    brand ? `${brand} العراق` : '',
    category ? `${category} العراق` : '',
    'منتجات كورية اصلية العراق',
    'منتجات كورية بغداد',
    'العناية بالبشرة الكورية العراق',
    'Korean skincare Iraq',
    'K beauty Iraq',
    'Korean beauty Baghdad',
    STORE_NAME,
  ]).slice(0, 18)
}

export function resolveSeoImage(image?: string | null) {
  const raw = String(image || '').trim()
  if (!raw) return absoluteUrl('/og-image.png')
  if (/^https?:\/\//i.test(raw)) return raw
  return absoluteUrl(raw)
}

export function productImages(product: any, fallback?: string) {
  const rawImages = Array.isArray(product?.images) ? product.images : []
  const urls = rawImages
    .map((x: any) => String(x?.url || x?.Url || x || '').trim())
    .filter(Boolean)
  const cover = String(product?.coverImage || product?.CoverImage || product?.imageUrl || product?.ImageUrl || fallback || '').trim()
  return unique([cover, ...urls]).map(resolveSeoImage)
}

export function buildOrganizationSchema() {
  const config = useRuntimeConfig()
  const instagram = String((config.public as any)?.instagramUrl || '').trim()
  const phone = String((config.public as any)?.supportPhone || '').trim()
  const email = String((config.public as any)?.supportEmail || '').trim()
  return compactObject({
    '@context': 'https://schema.org',
    '@type': 'Organization',
    '@id': absoluteUrl('/#organization'),
    name: STORE_NAME,
    alternateName: ['Dr.Seoul Beauty', 'DR SEOUL BEAUTY Iraq'],
    url: absoluteUrl('/'),
    logo: absoluteUrl('/favicon.ico'),
    image: absoluteUrl('/og-image.png'),
    sameAs: [instagram],
    contactPoint: [{
      '@type': 'ContactPoint',
      telephone: phone,
      email,
      contactType: 'customer support',
      areaServed: 'IQ',
      availableLanguage: ['Arabic', 'English'],
    }],
    address: {
      '@type': 'PostalAddress',
      addressCountry: 'IQ',
    },
  })
}

export function buildWebSiteSchema() {
  return compactObject({
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    '@id': absoluteUrl('/#website'),
    name: STORE_NAME,
    url: absoluteUrl('/'),
    publisher: { '@id': absoluteUrl('/#organization') },
    potentialAction: {
      '@type': 'SearchAction',
      target: absoluteUrl('/products?q={search_term_string}'),
      'query-input': 'required name=search_term_string',
    },
  })
}

export function buildBreadcrumbSchema(items: Array<{ name: string; item?: string }>) {
  return compactObject({
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: items
      .filter((x) => x?.name)
      .map((x, i) => ({ '@type': 'ListItem', position: i + 1, name: stripHtml(x.name), item: x.item || absoluteUrl('/') })),
  })
}

function productReviewSchema(product: any) {
  const reviews = Array.isArray(product?.reviews) ? product.reviews : []
  return reviews.slice(0, 5).map((r: any) => compactObject({
    '@type': 'Review',
    reviewRating: { '@type': 'Rating', ratingValue: Number(r.rating || r.Rating || 5), bestRating: 5, worstRating: 1 },
    author: { '@type': 'Person', name: stripHtml(r.userName || r.UserName || r.author || r.Author || 'Customer') },
    datePublished: r.createdAt || r.CreatedAt,
    reviewBody: stripHtml(r.comment || r.Comment || r.body || ''),
  })).filter((r: any) => r.reviewBody)
}

export function buildProductSchema(product: any, image?: string) {
  const title = stripHtml(product?.title || product?.Title || product?.name || STORE_NAME)
  const description = truncate(product?.seoDescription || product?.metaDescription || product?.description || product?.Description || DEFAULT_DESC, 500)
  const price = Number(product?.finalPriceIqd ?? product?.priceIqd ?? product?.PriceIqd ?? product?.price ?? 0)
  const ratingAvg = Number(product?.ratingAvg ?? product?.RatingAvg ?? product?.averageRating ?? 0)
  const ratingCount = Number(product?.ratingCount ?? product?.RatingCount ?? product?.reviewCount ?? 0)
  const stock = Number(product?.stockQuantity ?? product?.StockQuantity ?? product?.quantity ?? 0)
  const images = productImages(product, image)
  const review = productReviewSchema(product)

  return compactObject({
    '@context': 'https://schema.org',
    '@type': 'Product',
    '@id': `${productCanonical(product)}#product`,
    name: title,
    description,
    image: images.length ? images : [resolveSeoImage(image || '/og-image.png')],
    brand: { '@type': 'Brand', name: stripHtml(product?.brand || product?.Brand || STORE_NAME) },
    sku: String(product?.sku || product?.Sku || product?.id || product?.Id || productSlug(product)),
    mpn: String(product?.mpn || product?.Mpn || product?.id || product?.Id || productSlug(product)),
    category: stripHtml(product?.subCategory || product?.SubCategory || product?.category || product?.Category || 'Korean Skincare'),
    url: productCanonical(product),
    offers: {
      '@type': 'Offer',
      url: productCanonical(product),
      priceCurrency: 'IQD',
      price: price > 0 ? String(Math.round(price)) : undefined,
      availability: stock > 0 ? 'https://schema.org/InStock' : 'https://schema.org/OutOfStock',
      itemCondition: 'https://schema.org/NewCondition',
      seller: { '@id': absoluteUrl('/#organization') },
      areaServed: 'IQ',
    },
    aggregateRating: ratingCount > 0 ? {
      '@type': 'AggregateRating',
      ratingValue: Math.max(1, Math.min(5, Number(ratingAvg || 5))).toFixed(1),
      reviewCount: ratingCount,
      bestRating: 5,
      worstRating: 1,
    } : undefined,
    review,
  })
}

export function buildFaqSchema(faq: Array<{ question: string; answer: string }>) {
  const valid = (faq || [])
    .map((x) => ({ question: stripHtml(x?.question), answer: stripHtml(x?.answer) }))
    .filter((x) => x.question && x.answer)
    .slice(0, 8)
  if (!valid.length) return null
  return compactObject({
    '@context': 'https://schema.org',
    '@type': 'FAQPage',
    mainEntity: valid.map((x) => ({ '@type': 'Question', name: x.question, acceptedAnswer: { '@type': 'Answer', text: x.answer } })),
  })
}

export function buildCollectionPageSchema(input: { name: string; description?: string; url: string; image?: string }) {
  return compactObject({
    '@context': 'https://schema.org',
    '@type': 'CollectionPage',
    name: stripHtml(input.name),
    description: truncate(input.description || DEFAULT_DESC, 300),
    url: input.url,
    image: input.image ? resolveSeoImage(input.image) : undefined,
    isPartOf: { '@id': absoluteUrl('/#website') },
  })
}

export function useAdvancedSeo(input: SeoInput) {
  const config = useRuntimeConfig()
  const siteName = String((config.public as any)?.siteName || STORE_NAME)
  const titleText = stripHtml(input.title || siteName)
  const title = titleText.includes(siteName) ? titleText : `${titleText} | ${siteName}`
  const description = truncate(input.description || DEFAULT_DESC)
  const canonical = input.canonical || input.url || absoluteUrl('/')
  const image = resolveSeoImage(input.image || '/og-image.png')
  const keywords = Array.isArray(input.keywords) ? unique(input.keywords).join(', ') : String(input.keywords || 'DR SEOUL BEAUTY, Korean skincare Iraq, K beauty Iraq')
  const schemas = [buildOrganizationSchema(), buildWebSiteSchema(), ...(Array.isArray(input.schema) ? input.schema : (input.schema ? [input.schema] : []))]
    .filter(Boolean)
    .map(compactObject)

  useHead({
    title,
    meta: [
      { name: 'description', content: description },
      { name: 'keywords', content: keywords },
      input.noindex ? { name: 'robots', content: 'noindex,nofollow' } : { name: 'robots', content: 'index,follow,max-image-preview:large,max-snippet:-1,max-video-preview:-1' },
      { property: 'og:locale', content: 'ar_IQ' },
      { property: 'og:type', content: input.type || 'website' },
      { property: 'og:title', content: title },
      { property: 'og:description', content: description },
      { property: 'og:image', content: image },
      { property: 'og:image:secure_url', content: image },
      { property: 'og:url', content: canonical },
      { property: 'og:site_name', content: siteName },
      { name: 'twitter:card', content: 'summary_large_image' },
      { name: 'twitter:title', content: title },
      { name: 'twitter:description', content: description },
      { name: 'twitter:image', content: image },
    ],
    link: [{ rel: 'canonical', href: canonical }],
    script: schemas.map((schema, index) => ({
      key: `ld-json-${index}-${String(schema?.['@type'] || 'schema').toLowerCase()}`,
      type: 'application/ld+json',
      innerHTML: JSON.stringify(schema).replace(/</g, '\\u003c'),
    })),
  })
}

export function seoDescription(value?: string | null, fallback = DEFAULT_DESC) { return truncate(value || fallback) }
