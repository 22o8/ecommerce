<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'
import { findBlogPost } from '~/data/blogPosts'
const route = useRoute()
const api = useApi()
const slug = computed(() => String(route.params.slug || ''))
const post = computed(() => findBlogPost(slug.value))
if (!post.value) throw createError({ statusCode: 404, statusMessage: 'Article not found' })

const { data: relatedProducts } = await useAsyncData(
  () => `blog-related-${slug.value}`,
  async () => {
    const keywords = post.value?.relatedProductKeywords || []
    const q = keywords[0] || post.value?.keywords?.[0] || ''
    const res = await api.get<any>('/Products', { page: 1, pageSize: 8, q, sort: 'new' }).catch(() => null)
    const items = Array.isArray(res?.items) ? res.items : []
    return items.slice(0, 4)
  },
  { watch: [slug] }
)

useAdvancedSeo({
  title: post.value!.metaTitle,
  description: post.value!.metaDescription,
  keywords: post.value!.keywords,
  canonical: absoluteUrl(`/blog/${post.value!.slug}`),
  image: post.value!.coverImage,
  type: 'article',
  schema: [
    {
      '@context': 'https://schema.org',
      '@type': 'BlogPosting',
      '@id': absoluteUrl(`/blog/${post.value!.slug}#article`),
      headline: post.value!.title,
      image: absoluteUrl(post.value!.coverImage),
      description: post.value!.metaDescription,
      url: absoluteUrl(`/blog/${post.value!.slug}`),
      inLanguage: 'ar-IQ',
      author: { '@type': 'Organization', name: 'DR SEOUL BEAUTY' },
      publisher: buildOrganizationSchema(),
      mainEntityOfPage: absoluteUrl(`/blog/${post.value!.slug}`),
      articleSection: 'Korean Skincare Iraq',
      keywords: post.value!.keywords.join(', '),
    },
    buildBreadcrumbSchema([{ name: 'Home', item: absoluteUrl('/') }, { name: 'Blog', item: absoluteUrl('/blog') }, { name: post.value!.title, item: absoluteUrl(`/blog/${post.value!.slug}`) }]),
    buildFaqSchema(post.value!.faq),
  ],
})
</script>

<template>
  <main v-if="post" class="mx-auto max-w-5xl px-4 py-10">
    <NuxtLink to="/blog" class="text-sm text-[rgb(var(--muted))]">← المدونة</NuxtLink>
    <article class="mx-auto max-w-3xl">
      <h1 class="mt-4 text-3xl font-extrabold leading-tight rtl-text sm:text-4xl">{{ post.title }}</h1>
      <p class="mt-4 text-[rgb(var(--muted))] rtl-text">{{ post.metaDescription }}</p>
      <SmartImage :src="post.coverImage" :alt="post.title" :title="post.title" width="1200" height="675" wrapper-class="mt-6 aspect-[16/9] rounded-3xl" fit="cover" />
      <div class="mt-5 flex flex-wrap gap-2">
        <span v-for="kw in post.keywords" :key="kw" class="rounded-full border border-app px-3 py-1 text-xs">{{ kw }}</span>
      </div>
      <div class="prose prose-neutral mt-8 max-w-none rtl-text text-[rgb(var(--text))]">
        <p>{{ post.content }}</p>
        <h2>روابط مفيدة داخل الموقع</h2>
        <ul>
          <li><NuxtLink to="/products">تصفح المنتجات الكورية</NuxtLink></li>
          <li><NuxtLink to="/brands">تصفح البراندات</NuxtLink></li>
          <li><NuxtLink to="/iraq">صفحات الاستهداف داخل العراق</NuxtLink></li>
        </ul>
        <h2>أسئلة شائعة</h2>
      </div>
      <div class="mt-5 grid gap-4">
        <div v-for="item in post.faq" :key="item.question" class="rounded-2xl border border-app bg-surface p-4">
          <h3 class="font-extrabold rtl-text">{{ item.question }}</h3>
          <p class="mt-2 text-[rgb(var(--muted))] rtl-text">{{ item.answer }}</p>
        </div>
      </div>
    </article>

    <section v-if="relatedProducts?.length" class="mt-12">
      <div class="flex items-center justify-between gap-3">
        <h2 class="text-2xl font-extrabold rtl-text">منتجات مرتبطة بالمقال</h2>
        <NuxtLink to="/products" class="text-sm font-bold text-primary">عرض كل المنتجات</NuxtLink>
      </div>
      <div class="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <ProductCard v-for="p in relatedProducts" :key="p.id || p.slug" :product="p" />
      </div>
    </section>
  </main>
</template>
