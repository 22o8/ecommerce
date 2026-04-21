<template>
  <section class="mt-6 grid gap-6 xl:grid-cols-[280px_minmax(0,1fr)]">
    <aside class="listing-filters card-soft p-4 sm:p-5 xl:sticky xl:top-24 h-fit">
      <div class="flex items-center justify-between gap-3">
        <div>
          <div class="text-lg font-extrabold text-[rgb(var(--text))] rtl-text">{{ title || t('productsPage.filtersTitle') }}</div>
          <div class="mt-1 text-xs text-[rgb(var(--muted))] rtl-text">{{ hint || t('productsPage.filtersHint') }}</div>
        </div>
        <button type="button" class="btn-secondary px-3 py-2 text-sm" @click="$emit('reset')">إعادة</button>
      </div>
      <div class="mt-5 space-y-4">
        <div>
          <label class="listing-filter-label rtl-text">{{ t('productsPage.sort') }}</label>
          <select :value="sort" class="input products-select" @change="$emit('update:sort', ($event.target as HTMLSelectElement).value)">
            <option value="new">{{ t('productsPage.sortNewest') }}</option>
            <option value="oldest">{{ t('productsPage.sortOldest') || 'الأقدم' }}</option>
            <option value="alpha">{{ t('productsPage.sortAlphabetical') || 'حسب الأبجدية' }}</option>
            <option value="priceDesc">{{ t('productsPage.sortPriceDesc') }}</option>
            <option value="priceAsc">{{ t('productsPage.sortPriceAsc') }}</option>
          </select>
        </div>
        <div class="results-pill rtl-text">{{ t('productsPage.resultsCount', { count }) }}</div>
      </div>
    </aside>
    <div>
      <div class="products-toolbar card-soft p-4 sm:p-5">
        <div class="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <div class="text-lg font-extrabold text-[rgb(var(--text))] rtl-text">{{ heading }}</div>
            <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">{{ subheading }}</div>
          </div>
          <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-2 text-sm text-[rgb(var(--muted))]">
            <span class="h-2.5 w-2.5 rounded-full bg-[rgb(var(--primary))]" />
            <span class="rtl-text">{{ sortLabel }}</span>
          </div>
        </div>
      </div>
      <div v-if="loading && !items.length" class="mt-6">
        <div class="grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
          <div v-for="n in 6" :key="n" class="skeleton-card min-h-[320px] rounded-[1.75rem]" />
        </div>
      </div>
      <div v-else-if="items.length" class="mt-6 grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
        <ProductCard v-for="p in items" :key="p.id" :p="p" />
      </div>
      <div v-else class="mt-6 rounded-[1.5rem] border border-app bg-surface p-10 text-center text-[rgb(var(--muted))] rtl-text">
        {{ t('productsPage.emptyDesc') }}
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'
const { t } = useI18n()
defineProps<{
  items: any[]
  loading?: boolean
  count: number
  sort: string
  sortLabel: string
  heading: string
  subheading: string
  title?: string
  hint?: string
}>()
defineEmits<{
  (e:'update:sort', value:string): void
  (e:'reset'): void
}>()
</script>

<style scoped>
.listing-filters,.products-toolbar{border:1px solid rgba(var(--border), .92)}
.listing-filter-label{display:block;margin-bottom:8px;font-size:.85rem;font-weight:800;color:rgb(var(--text))}
.products-select{min-height:52px}
.results-pill{display:flex;align-items:center;justify-content:center;min-height:46px;padding:0 14px;border-radius:999px;border:1px solid rgba(var(--primary), .18);background:rgba(var(--primary), .08);color:rgb(var(--text));font-size:.9rem;font-weight:800}
@media (max-width: 1279px){.listing-filters{position:static}}
</style>
