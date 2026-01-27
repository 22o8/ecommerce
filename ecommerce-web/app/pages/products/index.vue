<template>
  <div class="grid gap-6">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ t('productsPage.title') }}</h1>
        <p class="text-muted rtl-text">{{ t('productsPage.subtitle') }}</p>
      </div>

      <div class="flex flex-wrap gap-2">
        <div class="flex items-center gap-2 rounded-2xl border border-app bg-surface px-3 py-2">
          <Icon name="mdi:magnify" class="text-lg opacity-70" />
          <input
            v-model="q"
            class="bg-transparent outline-none text-sm rtl-text w-56"
            :placeholder="t('productsPage.searchPlaceholder')"
            @keydown.enter="apply()"
          />
        </div>

        <select v-model="sort" class="rounded-2xl border border-app bg-surface px-4 py-2 text-sm">
          <option value="new">{{ t('productsPage.sort.new') }}</option>
          <option value="priceAsc">{{ t('productsPage.sort.priceAsc') }}</option>
          <option value="priceDesc">{{ t('productsPage.sort.priceDesc') }}</option>
        </select>

        <UiButton variant="secondary" @click="apply">
          <Icon name="mdi:filter-outline" class="text-lg" />
          <span class="rtl-text">{{ t('productsPage.search') }}</span>
        </UiButton>
      </div>
    </div>

    <div v-if="loading" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div v-for="i in 9" :key="i" class="card-soft p-4 grid gap-3">
        <div class="skeleton h-44" />
        <div class="skeleton h-5 w-3/4" />
        <div class="skeleton h-4 w-1/2" />
        <div class="skeleton h-10" />
      </div>
    </div>

    <div v-else-if="items.length === 0" class="card-soft p-10 text-center">
      <Icon name="mdi:database-off-outline" class="text-4xl opacity-70 mx-auto" />
      <div class="mt-3 font-bold rtl-text">{{ t('productsPage.emptyTitle') }}</div>
      <div class="mt-1 text-sm text-muted rtl-text">{{ t('productsPage.emptyDesc') }}</div>
    </div>

    <div v-else>
      <MasonryGrid :items="items" :columns="masonryCols" :gap="16" v-slot="{ item }">
        <ProductCard :p="item" />
      </MasonryGrid>
    </div>

    <ProductQuickPreviewModal />

    <div class="flex items-center justify-between gap-3 pt-2">
      <UiButton variant="ghost" @click="prev" :disabled="page<=1">
        <Icon name="mdi:chevron-left" class="text-xl keep-ltr" />
        <span class="rtl-text">{{ t('productsPage.prev') }}</span>
      </UiButton>

      <UiBadge>
        <span class="rtl-text">{{ t('productsPage.page') }}</span>
        <span class="keep-ltr"> {{ page }} </span>
      </UiBadge>

      <UiButton variant="ghost" @click="next" :disabled="items.length < pageSize">
        <span class="rtl-text">{{ t('productsPage.next') }}</span>
        <Icon name="mdi:chevron-right" class="text-xl keep-ltr" />
      </UiButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiBadge from '~/components/ui/UiBadge.vue'
import ProductCard from '~/components/ProductCard.vue'
import MasonryGrid from '~/components/MasonryGrid.vue'
import ProductQuickPreviewModal from '~/components/ProductQuickPreviewModal.vue'
import { useProductsStore } from '~/stores/products'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const products = useProductsStore()

const pageSize = 12
const page = computed(() => Number(route.query.page || 1))
// v-model needs writable refs; computed() without setter throws and breaks rendering.
const q = ref(String(route.query.q || ''))
const sort = ref(String(route.query.sort || 'new'))

watch(
  () => route.query.q,
  (v) => {
    q.value = String(v || '')
  }
)

watch(
  () => route.query.sort,
  (v) => {
    sort.value = String(v || 'new')
  }
)

const masonryCols = ref(3)

function updateMasonryCols() {
  if (process.server) return
  const w = window.innerWidth
  masonryCols.value = w < 640 ? 1 : w < 1024 ? 2 : 3
}

onMounted(() => {
  updateMasonryCols()
  window.addEventListener('resize', updateMasonryCols)
})

onBeforeUnmount(() => {
  if (process.server) return
  window.removeEventListener('resize', updateMasonryCols)
})

const items = computed(() => products.items)

// SSR + CSR: حمّل المنتجات بشكل مضمون من أول تحميل
const { pending: loading } = await useAsyncData(
  () => `products:${route.fullPath}`,
  async () => {
    await products.fetch({ page: page.value, pageSize, q: q.value, sort: sort.value })
    return true
  },
  { watch: [() => route.fullPath] }
)

function apply(){
  router.push({ query: { ...(q.value ? { q: q.value } : {}), sort: sort.value, page: 1 } })
}

function prev(){
  if (page.value <= 1) return
  router.push({ query: { ...(q.value ? { q: q.value } : {}), sort: sort.value, page: page.value - 1 } })
}

function next(){
  router.push({ query: { ...(q.value ? { q: q.value } : {}), sort: sort.value, page: page.value + 1 } })
}
</script>
