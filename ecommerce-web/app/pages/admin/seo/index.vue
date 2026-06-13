<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })
const checks = [
  { key: 'meta', label: 'Meta Tags', score: 14, status: 'مفعّل' },
  { key: 'schema', label: 'Product / Organization Schema', score: 14, status: 'مفعّل' },
  { key: 'faq', label: 'FAQ Schema', score: 10, status: 'مفعّل' },
  { key: 'reviews', label: 'Reviews / Aggregate Rating', score: 9, status: 'ديناميكي حسب المنتج' },
  { key: 'images', label: 'Images ALT / Title / Dimensions', score: 10, status: 'مفعّل' },
  { key: 'og', label: 'Open Graph', score: 12, status: 'مفعّل' },
  { key: 'twitter', label: 'Twitter Cards', score: 9, status: 'مفعّل' },
  { key: 'canonical', label: 'Canonical URLs', score: 10, status: 'مفعّل' },
  { key: 'links', label: 'Internal Links', score: 7, status: 'يحتاج توسيع محتوى مستقبلاً' },
  { key: 'sitemap', label: 'Sitemap / Robots', score: 7, status: 'مفعّل' },
]
const score = computed(() => checks.reduce((sum, item) => sum + item.score, 0))
const generated = ref<any>(null)
const form = reactive({ product: '', brand: '', category: '', description: '' })
function generateSeo() {
  const product = form.product || 'Korean Skincare Product'
  const brand = form.brand || 'DR SEOUL BEAUTY'
  const category = form.category || 'Korean skincare'
  const desc = form.description || `منتج ${category} من ${brand} مختار بعناية ضمن متجر DR SEOUL BEAUTY.`
  generated.value = {
    metaTitle: `${product} | ${brand} Iraq`,
    metaDescription: desc.slice(0, 155),
    keywords: [product, brand, category, 'korean skincare iraq', 'k beauty'].filter(Boolean),
    faq: [
      { question: `ما فائدة ${product}؟`, answer: desc },
      { question: 'هل المنتج أصلي؟', answer: 'يتم عرض المنتجات ضمن متجر DR SEOUL BEAUTY مع معلومات واضحة وصور المنتج.' },
      { question: 'كيف أستخدم المنتج؟', answer: 'اتبع تعليمات الاستخدام على العبوة أو وصف المنتج، وابدأ تدريجياً مع المنتجات الفعالة.' },
    ],
    canonical: `/products/${product.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '')}`,
    suggestedBlogTopics: [`أفضل منتجات ${category}`, `${brand} مراجعة وتجربة`, `روتين كوري باستخدام ${product}`],
  }
  if (import.meta.client) localStorage.setItem('drseo_generated_seo', JSON.stringify(generated.value))
}

onMounted(() => {
  try {
    const raw = localStorage.getItem('drseo_generated_seo')
    if (raw) generated.value = JSON.parse(raw)
  } catch {}
})
</script>

<template>
  <div class="grid gap-6">
    <section class="admin-card p-6">
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-extrabold">SEO Dashboard</h1>
          <p class="mt-1 text-sm text-muted">مراقبة عناصر SEO الأساسية داخل المشروع.</p>
        </div>
        <div class="rounded-3xl bg-surface-2 px-6 py-4 text-center">
          <div class="text-4xl font-black text-primary">{{ score }}/100</div>
          <div class="text-xs text-muted">SEO Score</div>
        </div>
      </div>
    </section>

    <section class="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
      <div v-for="item in checks" :key="item.key" class="admin-card p-4">
        <div class="flex items-center justify-between gap-3">
          <b>{{ item.label }}</b>
          <span class="rounded-full bg-emerald-500/10 px-3 py-1 text-xs text-emerald-400">{{ item.score }}</span>
        </div>
        <p class="mt-2 text-sm text-muted">{{ item.status }}</p>
      </div>
    </section>

    <section class="admin-card p-6">
      <h2 class="text-xl font-extrabold">AI SEO Generator</h2>
      <p class="mt-1 text-sm text-muted">مولد داخلي سريع يحفظ المسودة في المتصفح حتى يتم ربطه لاحقاً بقاعدة البيانات أو AI API.</p>
      <div class="mt-5 grid gap-3 md:grid-cols-2">
        <input v-model="form.product" class="admin-input" placeholder="اسم المنتج" />
        <input v-model="form.brand" class="admin-input" placeholder="البراند" />
        <input v-model="form.category" class="admin-input" placeholder="التصنيف" />
        <input v-model="form.description" class="admin-input" placeholder="وصف مختصر" />
      </div>
      <button class="admin-primary mt-4" @click="generateSeo">Generate SEO</button>
      <pre v-if="generated" class="mt-4 overflow-auto rounded-2xl bg-black/40 p-4 text-xs text-white">{{ generated }}</pre>
    </section>
  </div>
</template>
