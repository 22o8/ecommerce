<!-- app/pages/products/index.vue -->
<script setup lang="ts">
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'

const api = useApi()
const { t } = useI18n()

const q = ref('')
const sort = ref<'new' | 'priceAsc' | 'priceDesc'>('new')
const view = ref<'grid' | 'list'>('grid')

const page = ref(1)
const pageSize = 12

const sortValue = computed(() => {
  if (sort.value === 'priceAsc') return 'price:asc'
  if (sort.value === 'priceDesc') return 'price:desc'
  return 'new'
})

const { data, pending, refresh } = await useAsyncData(
  'products',
  async () => {
    return await api.get<any>('/Products', {
      page: page.value,
      pageSize,
      q: q.value || undefined,
      sort: sortValue.value,
    })
  },
  { watch: [page, sort] },
)

const items = computed(() => data.value?.items || [])
const total = computed(() => data.value?.totalCount || 0)
const pages = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))

function doSearch() {
  page.value = 1
  refresh()
}

function clearSearch() {
  q.value = ''
  doSearch()
}
</script>

<template>
  <div class="grid gap-6">
    <section class="card p-8">
      <div class="flex items-start justify-between gap-4 flex-wrap">
        <div class="rtl-text">
          <h1 class="h2">{{ t('productsPage.title') }}</h1>
          <p class="mt-1 muted text-sm">{{ t('productsPage.subtitle') }}</p>
        </div>

        <div class="flex gap-2 flex-wrap keep-ltr">
          <div class="relative">
            <input
              v-model.trim="q"
              class="input"
              style="min-width: 280px; padding-left: 40px;"
              :placeholder="t('productsPage.searchPlaceholder')"
              @keyup.enter="doSearch"
            />
            <span class="absolute left-3 top-1/2 -translate-y-1/2 opacity-60">🔎</span>
          </div>

          <select v-model="sort" class="input" style="width: 200px;">
            <option value="new">{{ t('productsPage.sort.new') }}</option>
            <option value="priceAsc">{{ t('productsPage.sort.priceAsc') }}</option>
            <option value="priceDesc">{{ t('productsPage.sort.priceDesc') }}</option>
          </select>

          <button
            type="button"
            class="px-4 py-3 rounded-2xl font-extrabold"
            :style="{ background: `rgb(var(--primary))`, color: 'white' }"
            @click="doSearch"
          >
            {{ t('productsPage.search') }}
          </button>

          <button
            v-if="q"
            type="button"
            class="px-4 py-3 rounded-2xl font-extrabold border"
            :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
            @click="clearSearch"
          >
            ✕
          </button>

          <div class="flex rounded-2xl overflow-hidden border" :style="{ borderColor: 'rgb(var(--border))' }">
            <button
              type="button"
              class="px-3 py-3 font-extrabold"
              :style="view === 'grid' ? { background: 'rgba(var(--text),.06)' } : { background: 'transparent' }"
              @click="view = 'grid'"
              title="Grid"
            >
              ⬚
            </button>
            <button
              type="button"
              class="px-3 py-3 font-extrabold"
              :style="view === 'list' ? { background: 'rgba(var(--text),.06)' } : { background: 'transparent' }"
              @click="view = 'list'"
              title="List"
            >
              ≡
            </button>
          </div>
        </div>
      </div>

      <div class="hr my-6"></div>

      <div
        v-if="pending"
        :class="view === 'grid' ? 'grid gap-4 sm:grid-cols-2 lg:grid-cols-3' : 'grid gap-3'"
      >
        <div v-for="i in 12" :key="i" class="soft p-5 animate-pulse">
          <div class="h-4 w-2/3 rounded bg-black/10 dark:bg-white/10"></div>
          <div class="h-3 w-1/2 rounded bg-black/10 dark:bg-white/10 mt-3"></div>
          <div class="h-10 rounded bg-black/10 dark:bg-white/10 mt-6"></div>
        </div>
      </div>

      <div
        v-else
        :class="view === 'grid' ? 'grid gap-4 sm:grid-cols-2 lg:grid-cols-3' : 'grid gap-3'"
      >
        <ProductCard
          v-for="p in items"
          :key="p.id"
          :item="p"
          :compact="view === 'list'"
        />

        <div v-if="!items.length" class="soft p-6">
          <div class="font-extrabold rtl-text">{{ t('productsPage.emptyTitle') }}</div>
          <div class="text-sm muted mt-1 rtl-text">{{ t('productsPage.emptyDesc') }}</div>
        </div>
      </div>

      <div class="mt-8 flex items-center justify-between gap-3 flex-wrap keep-ltr">
        <div class="text-sm muted">
          {{ t('productsPage.total') }}: <b>{{ total }}</b>
        </div>

        <div class="flex items-center gap-2">
          <button
            class="px-3 py-2 rounded-xl border"
            :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
            :disabled="page <= 1"
            @click="page--"
          >
            ← {{ t('productsPage.prev') }}
          </button>

          <div class="badge">
            {{ t('productsPage.page') }} {{ page }} / {{ pages }}
          </div>

          <button
            class="px-3 py-2 rounded-xl border"
            :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
            :disabled="page >= pages"
            @click="page++"
          >
            {{ t('productsPage.next') }} →
          </button>
        </div>
      </div>
    </section>
  </div>
</template>
