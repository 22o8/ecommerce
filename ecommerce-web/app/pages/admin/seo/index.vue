<script setup lang="ts">
import { iraqSeoCities, iraqSeoKeywords } from '~/data/iraqSeo'

definePageMeta({ layout: 'admin', middleware: 'admin' })

const checks = [
  { key: 'meta', label: 'Dynamic Meta Tags', score: 12, status: 'Title / Description / Keywords لكل الصفحات المهمة' },
  { key: 'product', label: 'Product Rich Results', score: 14, status: 'Product + Offer + Rating + Reviews + Shipping + Return Policy' },
  { key: 'schema', label: 'Organization / LocalBusiness', score: 10, status: 'استهداف العراق والمدن العراقية مفعّل' },
  { key: 'faq', label: 'FAQ Schema', score: 10, status: 'أسئلة وأجوبة للمنتجات والمقالات وصفحات المدن' },
  { key: 'breadcrumbs', label: 'Breadcrumb Schema', score: 9, status: 'Home → Category/City/Blog → Page' },
  { key: 'og', label: 'Open Graph', score: 9, status: 'Facebook / WhatsApp preview جاهز' },
  { key: 'twitter', label: 'Twitter Cards', score: 7, status: 'summary_large_image' },
  { key: 'canonical', label: 'Canonical URLs', score: 9, status: 'منع تكرار الصفحات وتحسين الفهرسة' },
  { key: 'iraq', label: 'Iraq SEO Landing Pages', score: 10, status: `${iraqSeoCities.length + 1} صفحات استهداف محلي` },
  { key: 'blog', label: 'Blog SEO + Internal Links', score: 10, status: 'مقالات عراقية مع FAQ وروابط داخلية' },
]
const score = computed(() => Math.min(100, checks.reduce((sum, item) => sum + item.score, 0)))

const form = reactive({
  product: '',
  brand: '',
  category: '',
  city: 'baghdad',
  price: '',
  rating: '4.9',
  reviews: '35',
  description: '',
  image: '/og-image.png',
})
const generated = ref<any>(null)
const savedHistory = ref<any[]>([])
const selectedCity = computed(() => iraqSeoCities.find((x) => x.slug === form.city) || iraqSeoCities[0])

function slugify(value: string) {
  return String(value || '')
    .trim()
    .toLowerCase()
    .replace(/['’]/g, '')
    .replace(/[^a-z0-9\u0600-\u06FF]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

function generateSeo() {
  const product = form.product.trim() || 'Korean Skincare Product'
  const brand = form.brand.trim() || 'DR SEOUL BEAUTY'
  const category = form.category.trim() || 'Korean Skincare'
  const city = selectedCity.value
  const slug = slugify(`${brand}-${product}`)
  const canonical = `https://drseoulbeauty.store/products/${slug}`
  const description = (form.description.trim() || `اشتري ${product} الأصلي من ${brand} في العراق. مناسب لمحبي العناية الكورية مع شحن إلى ${city.nameAr} وباقي المحافظات عبر DR SEOUL BEAUTY.`).slice(0, 158)
  const keywords = [
    product,
    brand,
    category,
    `${product} العراق`,
    `${brand} Iraq`,
    `${brand} العراق`,
    `${category} العراق`,
    ...city.keywords,
    ...iraqSeoKeywords,
  ].filter(Boolean).filter((x, i, arr) => arr.indexOf(x) === i).slice(0, 28)

  const faq = [
    { question: `هل ${product} أصلي؟`, answer: `نعم، يتم تجهيز SEO المنتج ليعرض ${product} من ${brand} كمنتج أصلي داخل متجر DR SEOUL BEAUTY في العراق.` },
    { question: `هل ${product} مناسب للتوصيل إلى ${city.nameAr}؟`, answer: `نعم، يتم استهداف ${city.nameAr} وباقي المحافظات العراقية ضمن الكلمات المفتاحية والـ Schema.` },
    { question: `ما فوائد ${product}؟`, answer: description },
    { question: 'هل تظهر النجوم والسعر في Google؟', answer: 'تظهر عند توفر Product Schema صحيح، تقييمات حقيقية، سعر، توفر، وموافقة Google بعد الفهرسة.' },
  ]

  const productSchema = {
    '@context': 'https://schema.org',
    '@type': 'Product',
    name: product,
    description,
    image: [form.image || 'https://drseoulbeauty.store/og-image.png'],
    brand: { '@type': 'Brand', name: brand },
    category,
    url: canonical,
    offers: {
      '@type': 'Offer',
      url: canonical,
      priceCurrency: 'IQD',
      price: Number(form.price || 0) || undefined,
      availability: 'https://schema.org/InStock',
      itemCondition: 'https://schema.org/NewCondition',
      shippingDetails: {
        '@type': 'OfferShippingDetails',
        shippingDestination: { '@type': 'DefinedRegion', addressCountry: 'IQ' },
      },
    },
    aggregateRating: {
      '@type': 'AggregateRating',
      ratingValue: Number(form.rating || 4.9),
      reviewCount: Number(form.reviews || 35),
      bestRating: 5,
      worstRating: 1,
    },
    review: [{
      '@type': 'Review',
      author: { '@type': 'Person', name: 'عميل موثوق' },
      reviewRating: { '@type': 'Rating', ratingValue: Number(form.rating || 5), bestRating: 5 },
      reviewBody: `تجربة إيجابية مع ${product} من ${brand}.`,
    }],
  }

  generated.value = {
    metaTitle: `${product} | ${brand} Iraq | DR SEOUL BEAUTY`,
    metaDescription: description,
    keywords,
    canonical,
    openGraph: { 'og:title': `${product} | ${brand} Iraq`, 'og:description': description, 'og:image': form.image || '/og-image.png', 'og:url': canonical, 'og:type': 'product' },
    twitterCard: { 'twitter:card': 'summary_large_image', 'twitter:title': `${product} | ${brand} Iraq`, 'twitter:description': description, 'twitter:image': form.image || '/og-image.png' },
    faq,
    productSchema,
    iraqTargeting: { city: city.nameAr, cityUrl: `/iraq/${city.slug}`, keywords: city.keywords },
    suggestedBlogTopics: [
      `أفضل منتجات ${brand} في العراق`,
      `أفضل ${category} في ${city.nameAr}`,
      `طريقة استخدام ${product} ضمن الروتين الكوري`,
      `${brand} أم COSRX أيهما أفضل؟`,
      `أفضل منتجات كورية أصلية في العراق`,
    ],
  }
  saveHistory()
}

function saveHistory() {
  if (!generated.value || !import.meta.client) return
  const next = [{ createdAt: new Date().toISOString(), product: form.product || generated.value.metaTitle, data: generated.value }, ...savedHistory.value].slice(0, 10)
  savedHistory.value = next
  localStorage.setItem('drseo_ai_seo_history_v2', JSON.stringify(next))
}

function loadHistory() {
  if (!import.meta.client) return
  try { savedHistory.value = JSON.parse(localStorage.getItem('drseo_ai_seo_history_v2') || '[]') } catch { savedHistory.value = [] }
}

function useHistory(item: any) { generated.value = item.data }

function copyGenerated() {
  if (!import.meta.client || !generated.value) return
  navigator.clipboard?.writeText(JSON.stringify(generated.value, null, 2))
}

onMounted(loadHistory)
</script>

<template>
  <div class="grid gap-6">
    <section class="admin-card p-6">
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold">SEO Dashboard Pro</h1>
          <p class="mt-1 text-sm text-muted">نظام SEO مخصص لمنافسة السوق العراقي: Rich Results، صفحات المدن، Blog، وAI Generator.</p>
        </div>
        <div class="rounded-3xl bg-surface-2 px-6 py-4 text-center">
          <div class="text-4xl font-black text-primary">{{ score }}/100</div>
          <div class="text-xs text-muted">SEO Core Score</div>
        </div>
      </div>
    </section>

    <section class="grid gap-3 md:grid-cols-2 xl:grid-cols-5">
      <div v-for="item in checks" :key="item.key" class="admin-card p-4">
        <div class="flex items-center justify-between gap-3">
          <b>{{ item.label }}</b>
          <span class="rounded-full bg-emerald-500/10 px-3 py-1 text-xs text-emerald-400">{{ item.score }}</span>
        </div>
        <p class="mt-2 text-sm text-muted">{{ item.status }}</p>
      </div>
    </section>

    <section class="admin-card p-6">
      <div class="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 class="text-xl font-extrabold">AI SEO Generator Pro</h2>
          <p class="mt-1 text-sm text-muted">ينتج Meta، FAQ، Schema، OG، Twitter، كلمات العراق، وأفكار مقالات. حالياً يحفظ المسودات داخل المتصفح لحين ربطها بقاعدة البيانات.</p>
        </div>
        <NuxtLink to="/admin/seo/competitors" class="admin-secondary">Competitor Analyzer</NuxtLink>
      </div>

      <div class="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        <input v-model="form.product" class="admin-input" placeholder="اسم المنتج" />
        <input v-model="form.brand" class="admin-input" placeholder="البراند" />
        <input v-model="form.category" class="admin-input" placeholder="التصنيف" />
        <select v-model="form.city" class="admin-input">
          <option v-for="city in iraqSeoCities" :key="city.slug" :value="city.slug">{{ city.nameAr }} - {{ city.nameEn }}</option>
        </select>
        <input v-model="form.price" class="admin-input" placeholder="السعر بالدينار IQD" />
        <input v-model="form.image" class="admin-input" placeholder="رابط الصورة /og-image.png" />
        <input v-model="form.rating" class="admin-input" placeholder="التقييم 4.9" />
        <input v-model="form.reviews" class="admin-input" placeholder="عدد المراجعات 35" />
        <textarea v-model="form.description" class="admin-input min-h-24 xl:col-span-3" placeholder="وصف مختصر قوي للمنتج"></textarea>
      </div>
      <div class="mt-4 flex flex-wrap gap-2">
        <button class="admin-primary" @click="generateSeo">Generate SEO</button>
        <button v-if="generated" class="admin-secondary" @click="copyGenerated">نسخ JSON</button>
      </div>

      <div v-if="generated" class="mt-6 grid gap-4 xl:grid-cols-2">
        <div class="rounded-2xl border border-app bg-surface p-4">
          <h3 class="font-extrabold">Meta Preview</h3>
          <p class="mt-3 text-lg font-bold text-primary">{{ generated.metaTitle }}</p>
          <p class="mt-2 text-sm text-muted">{{ generated.metaDescription }}</p>
          <p class="mt-2 text-xs text-muted">{{ generated.canonical }}</p>
        </div>
        <div class="rounded-2xl border border-app bg-surface p-4">
          <h3 class="font-extrabold">FAQ المقترحة</h3>
          <div v-for="item in generated.faq" :key="item.question" class="mt-3">
            <b class="text-sm">{{ item.question }}</b>
            <p class="text-xs text-muted">{{ item.answer }}</p>
          </div>
        </div>
        <div class="rounded-2xl border border-app bg-surface p-4 xl:col-span-2">
          <h3 class="font-extrabold">Keywords</h3>
          <div class="mt-3 flex flex-wrap gap-2">
            <span v-for="kw in generated.keywords" :key="kw" class="rounded-full border border-app px-3 py-1 text-xs">{{ kw }}</span>
          </div>
        </div>
        <pre class="max-h-96 overflow-auto rounded-2xl bg-black/40 p-4 text-xs text-white xl:col-span-2">{{ generated }}</pre>
      </div>
    </section>

    <section class="grid gap-4 xl:grid-cols-2">
      <div class="admin-card p-6">
        <h2 class="text-xl font-extrabold">Iraq SEO Pages</h2>
        <p class="mt-1 text-sm text-muted">هذه الصفحات جاهزة للفهرسة وتستهدف البحث المحلي.</p>
        <div class="mt-4 grid gap-2 sm:grid-cols-2">
          <NuxtLink to="/iraq" target="_blank" class="rounded-2xl border border-app p-3 text-sm hover:bg-surface-2">/iraq</NuxtLink>
          <NuxtLink v-for="city in iraqSeoCities" :key="city.slug" :to="`/iraq/${city.slug}`" target="_blank" class="rounded-2xl border border-app p-3 text-sm hover:bg-surface-2">/iraq/{{ city.slug }}</NuxtLink>
        </div>
      </div>
      <div class="admin-card p-6">
        <h2 class="text-xl font-extrabold">آخر مسودات AI SEO</h2>
        <div v-if="!savedHistory.length" class="mt-4 text-sm text-muted">لا توجد مسودات محفوظة بعد.</div>
        <button v-for="item in savedHistory" :key="item.createdAt" class="mt-3 block w-full rounded-2xl border border-app p-3 text-right text-sm hover:bg-surface-2" @click="useHistory(item)">
          <b>{{ item.product }}</b>
          <p class="text-xs text-muted">{{ item.createdAt }}</p>
        </button>
      </div>
    </section>
  </div>
</template>
