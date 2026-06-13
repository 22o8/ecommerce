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


function addMonthsIso(months = 6) {
  const d = new Date()
  d.setMonth(d.getMonth() + months)
  return d.toISOString().slice(0, 10)
}

function safeRatingValue(value: any, fallback = 5) {
  const n = Number(value)
  if (!Number.isFinite(n) || n <= 0) return fallback
  return Math.max(1, Math.min(5, n))
}

function reviewAuthorName(r: any) {
  return stripHtml(r?.userName || r?.UserName || r?.name || r?.Name || r?.author || r?.Author || 'عميل موثوق')
}

function reviewText(r: any, productTitle: string) {
  return stripHtml(r?.comment || r?.Comment || r?.text || r?.Text || r?.body || r?.Body || `تقييم إيجابي لمنتج ${productTitle} من ${STORE_NAME}.`)
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
  const title = stripHtml(product?.title || product?.Title || product?.name || STORE_NAME)
  const reviews = Array.isArray(product?.reviews) ? product.reviews : []
  return reviews.slice(0, 8).map((r: any) => compactObject({
    '@type': 'Review',
    name: `تقييم ${title}`,
    reviewRating: {
      '@type': 'Rating',
      ratingValue: safeRatingValue(r.rating || r.Rating || 5),
      bestRating: 5,
      worstRating: 1,
    },
    author: { '@type': 'Person', name: reviewAuthorName(r) },
    datePublished: r.createdAt || r.CreatedAt || r.updatedAt || r.UpdatedAt,
    reviewBody: reviewText(r, title),
  })).filter((r: any) => r.reviewBody)
}

export function buildProductSchema(product: any, image?: string) {
  const title = stripHtml(product?.title || product?.Title || product?.name || STORE_NAME)
  const brandName = stripHtml(product?.brand || product?.Brand || STORE_NAME)
  const categoryName = stripHtml(product?.subCategory || product?.SubCategory || product?.category || product?.Category || 'Korean Skincare')
  const description = truncate(
    product?.seoDescription ||
    product?.metaDescription ||
    product?.description ||
    product?.Description ||
    `اشتري ${title} الأصلي من ${brandName} في العراق عبر ${STORE_NAME}. منتجات كورية أصلية للعناية بالبشرة مع شحن سريع داخل العراق وبغداد.`,
    500
  )
  const price = Number(product?.finalPriceIqd ?? product?.priceIqd ?? product?.PriceIqd ?? product?.price ?? 0)
  const ratingAvg = Number(product?.ratingAvg ?? product?.RatingAvg ?? product?.averageRating ?? 0)
  const ratingCount = Number(product?.ratingCount ?? product?.RatingCount ?? product?.reviewCount ?? 0)
  const stock = Number(product?.stockQuantity ?? product?.StockQuantity ?? product?.quantity ?? 0)
  const images = productImages(product, image)
  const review = productReviewSchema(product)
  const canonical = productCanonical(product)
  const hasRating = ratingCount > 0 || review.length > 0
  const effectiveReviewCount = Math.max(ratingCount, review.length)

  return compactObject({
    '@context': 'https://schema.org',
    '@type': 'Product',
    '@id': `${canonical}#product`,
    name: title,
    alternateName: unique([
      `${title} العراق`,
      `${title} بغداد`,
      `${brandName} Iraq`,
      `${brandName} بغداد`,
    ]),
    description,
    image: images.length ? images : [resolveSeoImage(image || '/og-image.png')],
    brand: { '@type': 'Brand', name: brandName },
    sku: String(product?.sku || product?.Sku || product?.id || product?.Id || productSlug(product)),
    mpn: String(product?.mpn || product?.Mpn || product?.id || product?.Id || productSlug(product)),
    category: categoryName,
    url: canonical,
    inLanguage: 'ar-IQ',
    countryOfOrigin: { '@type': 'Country', name: 'Korea' },
    audience: {
      '@type': 'PeopleAudience',
      suggestedGender: 'Female',
      geographicArea: { '@type': 'Country', name: 'Iraq' },
    },
    offers: {
      '@type': 'Offer',
      '@id': `${canonical}#offer`,
      url: canonical,
      priceCurrency: 'IQD',
      price: price > 0 ? String(Math.round(price)) : undefined,
      priceValidUntil: addMonthsIso(6),
      availability: stock > 0 ? 'https://schema.org/InStock' : 'https://schema.org/OutOfStock',
      itemCondition: 'https://schema.org/NewCondition',
      seller: { '@id': absoluteUrl('/#organization') },
      areaServed: [
        { '@type': 'Country', name: 'Iraq' },
        { '@type': 'City', name: 'Baghdad' },
      ],
      availableDeliveryMethod: 'https://schema.org/ParcelService',
      shippingDetails: {
        '@type': 'OfferShippingDetails',
        shippingDestination: { '@type': 'DefinedRegion', addressCountry: 'IQ' },
        deliveryTime: {
          '@type': 'ShippingDeliveryTime',
          handlingTime: { '@type': 'QuantitativeValue', minValue: 0, maxValue: 1, unitCode: 'DAY' },
          transitTime: { '@type': 'QuantitativeValue', minValue: 1, maxValue: 4, unitCode: 'DAY' },
        },
      },
      hasMerchantReturnPolicy: {
        '@type': 'MerchantReturnPolicy',
        applicableCountry: 'IQ',
        returnPolicyCategory: 'https://schema.org/MerchantReturnFiniteReturnWindow',
        merchantReturnDays: 3,
        returnMethod: 'https://schema.org/ReturnByMail',
        returnFees: 'https://schema.org/FreeReturn',
      },
    },
    aggregateRating: hasRating ? {
      '@type': 'AggregateRating',
      ratingValue: safeRatingValue(ratingAvg || 5).toFixed(1),
      reviewCount: effectiveReviewCount || 1,
      ratingCount: effectiveReviewCount || 1,
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
      textContent: JSON.stringify(schema).replace(/</g, '\\u003c'),
      tagPosition: 'head',
    })),
  })
}

export function seoDescription(value?: string | null, fallback = DEFAULT_DESC) { return truncate(value || fallback) }
