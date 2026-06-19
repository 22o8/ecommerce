<template>
  <div class="container mx-auto px-4 py-8 sm:py-10">
    <template v-if="!hasActiveDetailRoute">
      <section class="card-soft overflow-hidden p-6 sm:p-8">
        <div class="grid gap-6 lg:grid-cols-[1.2fr_.8fr] lg:items-end">
          <div>
            <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-1 text-xs font-bold text-[rgb(var(--muted))]">
              <span class="h-2 w-2 rounded-full bg-[rgb(var(--primary))]" />
              {{ categoryLabel }}
            </div>
            <h1 class="mt-4 text-3xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-5xl rtl-text">
              {{ categoryLabel }}
            </h1>
            <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">
              {{ categoryDescription }}
            </p>
          </div>
        </div>
      </section>

      <section v-if="loadingChildren" class="mt-6 card-soft p-8 text-center text-sm text-[rgb(var(--muted))] rtl-text">
        {{ t('common.loading') }}
      </section>

      <section v-else-if="childSections.length" class="mt-6">
        <div class="card-soft p-5 sm:p-6">
          <div class="flex items-center justify-between gap-3">
            <div>
              <div class="text-xl font-extrabold text-[rgb(var(--text))] rtl-text">الأقسام الدقيقة</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">
                اختر القسم الأنسب للمشكلة حتى تظهر لك المنتجات المناسبة مباشرة.
              </div>
            </div>
          </div>

          <div class="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
            <NuxtLink
              v-for="child in childSections"
              :key="child.id || child.key"
              :to="buildDetailRoute(child)"
              class="group overflow-hidden rounded-[1.75rem] border border-app bg-surface transition hover:-translate-y-1 hover:shadow-soft"
            >
              <div class="aspect-square overflow-hidden bg-surface-2">
                <img
                  v-if="child.imageUrl"
                  :src="buildAssetUrl(child.imageUrl)"
                  :alt="child.nameAr"
                  class="h-full w-full object-cover transition duration-500 group-hover:scale-105"
                />
                <div
                  v-else
                  class="flex h-full w-full items-center justify-center text-4xl font-black text-[rgb(var(--text))]"
                >
                  {{ child.nameAr?.slice(0, 1) }}
                </div>
              </div>

              <div class="p-4 text-center">
                <div class="text-base font-extrabold text-[rgb(var(--text))] rtl-text">
                  {{ child.nameAr }}
                </div>
                <div class="mt-1 text-xs text-[rgb(var(--muted))] rtl-text">
                  {{ child.descriptionAr || 'عرض المنتجات المناسبة لهذا القسم' }}
                </div>
              </div>
            </NuxtLink>
          </div>
        </div>
      </section>

      <section v-else class="mt-6">
        <div class="card-soft p-5 sm:p-6">
          <div class="mb-5 flex items-center justify-between gap-3">
            <div>
              <div class="text-xl font-extrabold text-[rgb(var(--text))] rtl-text">المنتجات المناسبة</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">
                {{ t('productsPage.resultsCount', { count: products.totalCount || products.items.length || 0 }) }}
              </div>
            </div>
            <button type="button" class="btn-secondary px-4 py-2" @click="router.push('/#categories')">
              عودة
            </button>
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
        </div>
      </section>
    </template>

    <NuxtPage v-else :page-key="route.fullPath" />
  </div>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { problemCategories, fetchCategories, fetchProblemChildren } = useCategories()
const products = useProductsStore()
const { buildAssetUrl } = useApi()

const categoryKey = computed(() => String(route.params.category || '').toLowerCase())
const hasActiveDetailRoute = computed(() => typeof route.params.detail === 'string' && String(route.params.detail).length > 0)
const loadingChildren = ref(true)
const childSections = ref<any[]>([])

function buildDetailRoute(child: any) {
  const childKey = String(child?.key || '').toLowerCase()
  return `/problems/${encodeURIComponent(categoryKey.value)}/${encodeURIComponent(childKey)}`
}

await useAsyncData(
  () => `problem-root:${categoryKey.value}`,
  async () => {
    await fetchCategories(false, 'problem')
    return true
  },
  { watch: [categoryKey] }
)

const categoryItem = computed(
  () => (problemCategories.value || []).find((c: any) => String(c.key || '').toLowerCase() === categoryKey.value) || null
)

const categoryLabel = computed(() => categoryItem.value?.nameAr || categoryKey.value)
const categoryDescription = computed(
  () => categoryItem.value?.descriptionAr || 'اختر القسم المناسب لتظهر لك الحلول الدقيقة الخاصة بهذه المشكلة.'
)

async function loadChildren() {
  if (hasActiveDetailRoute.value) return

  loadingChildren.value = true

  try {
    const parentId = String(categoryItem.value?.id || '')
    childSections.value = parentId ? await fetchProblemChildren(parentId) : []

    if (!childSections.value.length) {
      await products.fetch({
        page: 1,
        pageSize: 24,
        sort: 'new',
        problemCategory: categoryKey.value
      })
    }
  } finally {
    loadingChildren.value = false
  }
}

watch(categoryItem, () => {
  loadChildren()
}, { immediate: true })
</script>