<script setup lang="ts">
const route = useRoute()
const category = computed(() => String(route.params.category || ''))
useAdvancedSeo({
  title: `${category.value} منتجات عناية كورية`,
  description: `تصفح منتجات ${category.value} ضمن متجر DR SEOUL BEAUTY للعناية الكورية بالبشرة والتجميل.`,
  keywords: [category.value, 'korean skincare iraq', 'DR SEOUL BEAUTY'],
  canonical: absoluteUrl(`/collections/${encodeURIComponent(category.value)}`),
  schema: buildBreadcrumbSchema([{ name: 'Home', item: absoluteUrl('/') }, { name: 'Collections', item: absoluteUrl('/products') }, { name: category.value, item: absoluteUrl(`/collections/${encodeURIComponent(category.value)}`) }]),
})
await navigateTo(`/categories/${encodeURIComponent(category.value)}`, { replace: true })
</script>
<template><div /></template>
