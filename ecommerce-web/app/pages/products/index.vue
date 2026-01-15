<!-- app/pages/products/index.vue -->
<script setup lang="ts">
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'

const api = useApi()

const q = ref('')
const sort = ref<'new' | 'priceAsc' | 'priceDesc'>('new')
const page = ref(1)
const pageSize = 12

const sortValue = computed(() => {
  if (sort.value === 'priceAsc') return 'price:asc'
  if (sort.value === 'priceDesc') return 'price:desc'
  return 'new'
})

const { data, pending, refresh } = await useAsyncData('products', async () => {
  return await api.get<any>('/Products', {
    page: page.value,
    pageSize,
    q: q.value || undefined,
    sort: sortValue.value,
  })
}, { watch: [page, sort] })

const items = computed(() => data.value?.items || [])
const total = computed(() => data.value?.totalCount || 0)
const pages = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))

function doSearch() {
  page.value = 1
  refresh()
}
</script>

<template>
  <div class="grid gap-6">
    <section class="card p-8">
      <div class="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <h1 class="h2">Products</h1>
          <p class="mt-1 muted text-sm">Search, sort and browse your items</p>
        </div>

        <div class="flex gap-2 flex-wrap">
          <input
            v-model.trim="q"
            class="input"
            style="min-width: 260px;"
            placeholder="Search products..."
            @keyup.enter="doSearch"
          />

          <select v-model="sort" class="input" style="width: 180px;">
            <option value="new">Newest</option>
            <option value="priceAsc">Price: Low</option>
            <option value="priceDesc">Price: High</option>
          </select>

          <button type="button" class="px-4 py-3 rounded-2xl font-extrabold"
            :style="{ background: `rgb(var(--primary))`, color: 'white' }"
            @click="doSearch"
          >
            Search
          </button>
        </div>
      </div>

      <div class="hr my-6"></div>

      <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <template v-if="pending">
          <div v-for="i in 12" :key="i" class="soft p-5 animate-pulse">
            <div class="h-4 w-2/3 rounded bg-black/10 dark:bg-white/10"></div>
            <div class="h-3 w-1/2 rounded bg-black/10 dark:bg-white/10 mt-3"></div>
            <div class="h-10 rounded bg-black/10 dark:bg-white/10 mt-6"></div>
          </div>
        </template>
        <template v-else>
          <ProductCard v-for="p in items" :key="p.id" :item="p" />
          <div v-if="!items.length" class="soft p-6 sm:col-span-2 lg:col-span-3">
            <div class="font-extrabold">No products found</div>
            <div class="text-sm muted mt-1">جرّب بحث ثاني أو تأكد عندك منتجات منشورة.</div>
          </div>
        </template>
      </div>

      <div class="mt-8 flex items-center justify-between gap-3 flex-wrap">
        <div class="text-sm muted">
          Total: <b>{{ total }}</b>
        </div>

        <div class="flex items-center gap-2">
          <button class="px-3 py-2 rounded-xl border"
            :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(255,255,255,.04)' }"
            :disabled="page <= 1"
            @click="page--"
          >
            ← Prev
          </button>

          <div class="badge">
            Page {{ page }} / {{ pages }}
          </div>

          <button class="px-3 py-2 rounded-xl border"
            :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(255,255,255,.04)' }"
            :disabled="page >= pages"
            @click="page++"
          >
            Next →
          </button>
        </div>
      </div>
    </section>
  </div>
</template>
