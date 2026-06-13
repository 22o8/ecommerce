<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'
import { findIraqSeoCity, iraqSeoCities, iraqSeoKeywords } from '~/data/iraqSeo'

const route = useRoute()
const api = useApi()
const config = useRuntimeConfig()
const citySlug = computed(() => String(route.params.city || ''))
const city = computed(() => findIraqSeoCity(citySlug.value))
if (!city.value) throw createError({ statusCode: 404, statusMessage: 'City SEO page not found' })

const { data: products } = await useAsyncData(
  () => `iraq-city-products-${citySlug.value}`,
  async () => {
    const res = await api.get<any>('/Products', { page: 1, pageSize: 8, sort: 'new' }).catch(() => null)
    return Array.isArray(res?.items) ? res.items : []
  },
  { watch: [citySlug] }
)

useAdvancedSeo({
  title: city.value!.title,
  description: city.value!.description,
  keywords: [...city.value!.keywords, ...iraqSeoKeywords],
  canonical: absoluteUrl(`/iraq/${city.value!.slug}`),
  image: '/og-image.png',
  schema: [
    {
      '@context': 'https://schema.org',
      '@type': 'BeautySupplyStore',
      '@id': absoluteUrl(`/iraq/${city.value!.slug}#localbusiness`),
      name: `DR SEOUL BEAUTY ${city.value!.nameEn}`,
      url: absoluteUrl(`/iraq/${city.value!.slug}`),
      image: absoluteUrl('/og-image.png'),
      areaServed: [{ '@type': 'City', name: city.value!.nameEn }, { '@type': 'Country', name: 'Iraq' }],
      address: { '@type': 'PostalAddress', addressLocality: city.value!.nameEn, addressCountry: 'IQ' },
      priceRange: 'IQD',
      telephone: String(config.public.supportPhone || ''),
      email: String(config.public.supportEmail || ''),
      sameAs: [String(config.public.instagramUrl || '')].filter(Boolean),
      openingHoursSpecification: [{ '@type': 'OpeningHoursSpecification', dayOfWeek: ['Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday'], opens: '10:00', closes: '23:00' }],
      makesOffer: city.value!.keywords.slice(0, 6).map((kw: string) => ({ '@type': 'Offer', itemOffered: { '@type': 'Product', name: kw }, areaServed: city.value!.nameEn, priceCurrency: 'IQD' })),
    },
    buildCollectionPageSchema({ name: city.value!.title, description: city.value!.description, url: absoluteUrl(`/iraq/${city.value!.slug}`), image: '/og-image.png' }),
    buildBreadcrumbSchema([{ name: 'Home', item: absoluteUrl('/') }, { name: 'Iraq', item: absoluteUrl('/iraq') }, { name: city.value!.nameAr, item: absoluteUrl(`/iraq/${city.value!.slug}`) }]),
    buildFaqSchema(city.value!.faq),
  ],
})
</script>

<template>
  <main v-if="city" class="mx-auto max-w-6xl px-4 py-10">
    <NuxtLink to="/iraq" class="text-sm text-[rgb(var(--muted))]">← صفحات العراق</NuxtLink>
    <section class="mt-4 rounded-[2rem] border border-app bg-surface p-6 sm:p-10">
      <p class="text-sm font-bold text-primary">{{ city.nameEn }} SEO Landing Page</p>
      <h1 class="mt-3 text-3xl font-black leading-tight sm:text-5xl rtl-text">{{ city.title }}</h1>
      <p class="mt-4 max-w-3xl text-[rgb(var(--muted))] rtl-text">{{ city.description }}</p>
      <div class="mt-6 flex flex-wrap gap-2">
        <span v-for="kw in city.keywords" :key="kw" class="rounded-full border border-app px-3 py-1 text-sm">{{ kw }}</span>
      </div>
    </section>

    <section class="mt-10">
      <div class="flex items-center justify-between gap-3">
        <h2 class="text-2xl font-extrabold rtl-text">منتجات مقترحة في {{ city.nameAr }}</h2>
        <NuxtLink to="/products" class="text-sm font-bold text-primary">عرض الكل</NuxtLink>
      </div>
      <div class="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <ProductCard v-for="p in products || []" :key="p.id || p.slug" :product="p" />
      </div>
    </section>

    <section class="mt-10 grid gap-4 md:grid-cols-2">
      <div v-for="item in city.faq" :key="item.question" class="rounded-3xl border border-app bg-surface p-5">
        <h3 class="font-extrabold rtl-text">{{ item.question }}</h3>
        <p class="mt-2 text-sm text-[rgb(var(--muted))] rtl-text">{{ item.answer }}</p>
      </div>
    </section>
  </main>
</template>
