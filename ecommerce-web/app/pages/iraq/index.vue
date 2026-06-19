<script setup lang="ts">
import { iraqSeoCities, iraqSeoKeywords } from '~/data/iraqSeo'

const faq = [
  { question: 'هل DR SEOUL BEAUTY يستهدف السوق العراقي؟', answer: 'نعم، تم تجهيز صفحات SEO مخصصة للعراق والمدن العراقية مع كلمات مفتاحية عربية وإنجليزية.' },
  { question: 'ما أهم كلمات البحث المستهدفة؟', answer: 'منتجات كورية العراق، Korean skincare Iraq، كريم كوري بغداد، سيروم كوري العراق، ومنتجات كورية أصلية العراق.' },
  { question: 'هل صفحات المدن تساعد بالظهور في Google؟', answer: 'نعم، لأنها تمنح Google صفحات واضحة لكل مدينة وكلمات محلية دقيقة تزيد فرصة الظهور.' },
]

useAdvancedSeo({
  title: 'منتجات العناية بالبشرة الكورية الأصلية في العراق',
  description: 'DR SEOUL BEAUTY متجر منتجات كورية أصلية للعناية بالبشرة والتجميل في العراق مع صفحات SEO مخصصة لبغداد والبصرة وأربيل والنجف وكربلاء والموصل.',
  keywords: iraqSeoKeywords,
  canonical: absoluteUrl('/iraq'),
  image: '/og-image.png',
  schema: [
    {
      '@context': 'https://schema.org',
      '@type': 'BeautySupplyStore',
      '@id': absoluteUrl('/iraq#localbusiness'),
      name: 'DR SEOUL BEAUTY Iraq',
      url: absoluteUrl('/iraq'),
      image: absoluteUrl('/og-image.png'),
      areaServed: ['Iraq', ...iraqSeoCities.map((x) => x.nameEn)],
      address: { '@type': 'PostalAddress', addressCountry: 'IQ' },
      priceRange: 'IQD',
      sameAs: [],
    },
    buildBreadcrumbSchema([{ name: 'Home', item: absoluteUrl('/') }, { name: 'Iraq', item: absoluteUrl('/iraq') }]),
    buildFaqSchema(faq),
  ],
})
</script>

<template>
  <main class="mx-auto max-w-6xl px-4 py-10">
    <section class="rounded-[2rem] border border-app bg-surface p-6 sm:p-10">
      <p class="text-sm font-bold text-primary">Korean Skincare Iraq</p>
      <h1 class="mt-3 text-3xl font-black leading-tight sm:text-5xl rtl-text">منتجات العناية بالبشرة الكورية الأصلية في العراق</h1>
      <p class="mt-4 max-w-3xl text-[rgb(var(--muted))] rtl-text">صفحة مخصصة لتقوية ظهور DR SEOUL BEAUTY في نتائج Google داخل العراق، مع استهداف مباشر لكلمات المنتجات الكورية الأصلية، العناية بالبشرة الكورية، والبحث المحلي حسب المدينة.</p>
      <div class="mt-6 flex flex-wrap gap-2">
        <NuxtLink to="/products" class="btn btn-primary">تسوّق المنتجات</NuxtLink>
        <NuxtLink to="/blog" class="btn">اقرأ المدونة</NuxtLink>
      </div>
    </section>

    <section class="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <NuxtLink v-for="city in iraqSeoCities" :key="city.slug" :to="`/iraq/${city.slug}`" class="rounded-3xl border border-app bg-surface p-5 transition hover:-translate-y-1 hover:shadow-lg">
        <h2 class="text-xl font-extrabold rtl-text">{{ city.nameAr }}</h2>
        <p class="mt-2 text-sm text-[rgb(var(--muted))]">{{ city.nameEn }}</p>
        <p class="mt-3 line-clamp-3 text-sm rtl-text">{{ city.description }}</p>
      </NuxtLink>
    </section>

    <section class="mt-10 rounded-[2rem] border border-app bg-surface p-6">
      <h2 class="text-2xl font-extrabold rtl-text">الكلمات المفتاحية الأساسية للعراق</h2>
      <div class="mt-4 flex flex-wrap gap-2">
        <span v-for="kw in iraqSeoKeywords" :key="kw" class="rounded-full border border-app px-3 py-1 text-sm">{{ kw }}</span>
      </div>
    </section>

    <section class="mt-10 grid gap-4 md:grid-cols-3">
      <div v-for="item in faq" :key="item.question" class="rounded-3xl border border-app bg-surface p-5">
        <h3 class="font-extrabold rtl-text">{{ item.question }}</h3>
        <p class="mt-2 text-sm text-[rgb(var(--muted))] rtl-text">{{ item.answer }}</p>
      </div>
    </section>
  </main>
</template>
