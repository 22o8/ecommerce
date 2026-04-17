<template>
  <div class="container mx-auto px-4 py-8 sm:py-10">
    <section class="card-soft overflow-hidden p-6 sm:p-8">
      <div class="flex flex-wrap items-center gap-2 text-sm text-[rgb(var(--muted))]">
        <NuxtLink :to="`/problems/${encodeURIComponent(categoryKey)}`" class="hover:text-[rgb(var(--text))]">{{ categoryLabel }}</NuxtLink>
        <span>/</span>
        <span class="text-[rgb(var(--text))]">{{ detailLabel }}</span>
      </div>
      <h1 class="mt-4 text-3xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-5xl rtl-text">{{ detailLabel }}</h1>
      <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">{{ detailDescription }}</p>
    </section>

    <section class="mt-6 card-soft p-5 sm:p-6">
      <div class="mb-5 flex items-center justify-between gap-3">
        <div>
          <div class="text-xl font-extrabold text-[rgb(var(--text))] rtl-text">المنتجات المناسبة</div>
          <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">{{ t('productsPage.resultsCount', { count: products.totalCount || products.items.length || 0 }) }}</div>
        </div>
      </div>

      <div v-if="products.loading && products.items.length === 0" class="grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
        <div v-for="n in 6" :key="n" class="skeleton-card min-h-[320px] rounded-[1.75rem]" />
      </div>
      <div v-else-if="products.items.length" class="grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
        <ProductCard v-for="p in products.items" :key="p.id" :p="p" />
      </div>
      <div v-else class="rounded-[1.5rem] border border-app bg-surface p-10 text-center text-[rgb(var(--muted))] rtl-text">
        {{ t('productsPage.emptyDesc') }}
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'

const { t } = useI18n()
const route = useRoute()
const { problemCategories, fetchCategories, fetchProblemChildren } = useCategories()
const products = useProductsStore()

const categoryKey = computed(() => String(route.params.category || '').toLowerCase())
const detailKey = computed(() => String(route.params.detail || '').toLowerCase())
const childSections = ref<any[]>([])

await useAsyncData(`problem-detail:${categoryKey.value}:${detailKey.value}`, async () => {
  await fetchCategories(false, 'problem')
  const parent = (problemCategories.value || []).find((c: any) => String(c.key || '').toLowerCase() === categoryKey.value)
  if (parent?.id) childSections.value = await fetchProblemChildren(String(parent.id))
  await products.fetch({ page: 1, pageSize: 24, sort: 'new', problemCategory: categoryKey.value, problemSubCategory: detailKey.value })
  return true
}, { watch: [categoryKey, detailKey] })

const categoryLabel = computed(() => (problemCategories.value || []).find((c: any) => String(c.key || '').toLowerCase() === categoryKey.value)?.nameAr || categoryKey.value)
const detailItem = computed(() => (childSections.value || []).find((c: any) => String(c.key || '').toLowerCase() === detailKey.value) || null)
const detailLabel = computed(() => detailItem.value?.nameAr || detailKey.value)
const detailDescription = computed(() => detailItem.value?.descriptionAr || 'هذه المنتجات مرتبطة بهذا القسم الدقيق ضمن حلول المشكلة.')
</script>
