<script setup lang="ts">
import { findBlogPost } from '~/data/blogPosts'
const route = useRoute()
const slug = computed(() => String(route.params.slug || ''))
const post = computed(() => findBlogPost(slug.value))
if (!post.value) throw createError({ statusCode: 404, statusMessage: 'Article not found' })
useAdvancedSeo({
  title: post.value!.metaTitle,
  description: post.value!.metaDescription,
  keywords: post.value!.keywords,
  canonical: absoluteUrl(`/blog/${post.value!.slug}`),
  image: post.value!.coverImage,
  type: 'article',
  schema: [
    { '@context': 'https://schema.org', '@type': 'BlogPosting', headline: post.value!.title, image: absoluteUrl(post.value!.coverImage), description: post.value!.metaDescription, url: absoluteUrl(`/blog/${post.value!.slug}`), publisher: buildOrganizationSchema() },
    buildBreadcrumbSchema([{ name: 'Home', item: absoluteUrl('/') }, { name: 'Blog', item: absoluteUrl('/blog') }, { name: post.value!.title, item: absoluteUrl(`/blog/${post.value!.slug}`) }]),
    buildFaqSchema(post.value!.faq),
  ],
})
</script>

<template>
  <main v-if="post" class="mx-auto max-w-3xl px-4 py-10">
    <NuxtLink to="/blog" class="text-sm text-[rgb(var(--muted))]">← المدونة</NuxtLink>
    <h1 class="mt-4 text-3xl font-extrabold leading-tight rtl-text">{{ post.title }}</h1>
    <SmartImage :src="post.coverImage" :alt="post.title" :title="post.title" width="1200" height="675" wrapper-class="mt-6 aspect-[16/9] rounded-3xl" fit="cover" />
    <article class="prose prose-neutral mt-8 max-w-none rtl-text text-[rgb(var(--text))]">
      <p>{{ post.content }}</p>
      <h2>أسئلة شائعة</h2>
      <div v-for="item in post.faq" :key="item.question" class="mt-4 rounded-2xl border border-app bg-surface p-4">
        <h3 class="font-extrabold">{{ item.question }}</h3>
        <p class="mt-2 text-[rgb(var(--muted))]">{{ item.answer }}</p>
      </div>
    </article>
  </main>
</template>
