<script setup lang="ts">
import { blogPosts } from '~/data/blogPosts'
useAdvancedSeo({
  title: 'مدونة العناية الكورية',
  description: 'مقالات DR SEOUL BEAUTY عن العناية الكورية، أفضل التونرات، السيرومات، واقي الشمس، ومقارنات البراندات.',
  keywords: ['مدونة العناية الكورية', 'korean skincare blog', 'DR SEOUL BEAUTY blog'],
  canonical: absoluteUrl('/blog'),
  schema: buildBreadcrumbSchema([{ name: 'Home', item: absoluteUrl('/') }, { name: 'Blog', item: absoluteUrl('/blog') }]),
})
</script>

<template>
  <main class="mx-auto max-w-6xl px-4 py-10">
    <h1 class="text-3xl font-extrabold text-[rgb(var(--text))] rtl-text">مدونة العناية الكورية</h1>
    <p class="mt-2 text-[rgb(var(--muted))] rtl-text">مقالات تساعدك تختار المنتج المناسب وتبني روتين عناية صحيح.</p>
    <div class="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
      <NuxtLink v-for="post in blogPosts" :key="post.slug" :to="`/blog/${post.slug}`" class="rounded-3xl border border-app bg-surface p-4 shadow-sm transition hover:-translate-y-1">
        <SmartImage :src="post.coverImage" :alt="post.title" :title="post.title" width="640" height="360" wrapper-class="aspect-[16/9]" fit="cover" />
        <h2 class="mt-4 text-lg font-extrabold rtl-text">{{ post.title }}</h2>
        <p class="mt-2 line-clamp-3 text-sm text-[rgb(var(--muted))] rtl-text">{{ post.metaDescription }}</p>
      </NuxtLink>
    </div>
  </main>
</template>
