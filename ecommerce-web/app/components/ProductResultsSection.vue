<template>
  <section class="mt-6 grid gap-6 xl:grid-cols-[280px_minmax(0,1fr)]">
    <aside class="card-soft border border-app p-5 xl:sticky xl:top-24 h-fit">
      <div class="flex items-center justify-between gap-3">
        <div>
          <div class="text-lg font-extrabold text-[rgb(var(--text))] rtl-text">{{ title }}</div>
          <div class="mt-1 text-xs text-[rgb(var(--muted))] rtl-text">{{ hint }}</div>
        </div>
        <button type="button" class="btn-secondary px-3 py-2 text-sm" @click="$emit('reset')">{{ clearLabel }}</button>
      </div>

      <div class="mt-5 grid gap-4">
        <div>
          <label class="mb-2 block text-sm font-extrabold text-[rgb(var(--text))] rtl-text">{{ sortLabel }}</label>
          <select v-model="localSort" class="input products-select" @change="emitSort">
            <option v-for="opt in options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>

        <div class="results-pill rtl-text">{{ countLabel }}: {{ count }}</div>
      </div>
    </aside>

    <div>
      <div class="card-soft border border-app p-5">
        <div class="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <div class="text-xl font-extrabold text-[rgb(var(--text))] rtl-text">{{ resultsTitle }}</div>
            <div class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">{{ countLabel }}: {{ count }}</div>
          </div>
          <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-2 text-sm text-[rgb(var(--muted))]">
            <span class="h-2.5 w-2.5 rounded-full bg-[rgb(var(--primary))]" />
            <span class="rtl-text">{{ activeSortLabel }}</span>
          </div>
        </div>
      </div>

      <div v-if="loading && !items.length" class="mt-6 grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
        <div v-for="n in 6" :key="n" class="skeleton-card min-h-[320px] rounded-[1.75rem]" />
      </div>

      <div v-else-if="items.length" class="mt-6 grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
        <ProductCard v-for="p in items" :key="p.id" :p="p" />
      </div>

      <div v-else class="mt-6 rounded-[1.5rem] border border-app bg-surface p-10 text-center text-[rgb(var(--muted))] rtl-text">
        {{ emptyText }}
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'

const props = defineProps<{
  items: any[]
  loading: boolean
  sort: string
  count: number
  title?: string
  hint?: string
  sortLabel?: string
  clearLabel?: string
  resultsTitle?: string
  countLabel?: string
  emptyText?: string
}>()

const emit = defineEmits<{
  (e: 'update:sort', value: string): void
  (e: 'reset'): void
}>()

const localSort = ref(props.sort)
watch(() => props.sort, (v) => { localSort.value = v })

const options = [
  { label: 'الأحدث', value: 'new' },
  { label: 'الأقدم', value: 'oldest' },
  { label: 'الترتيب الأبجدي', value: 'alphabetical' },
  { label: 'الأعلى سعراً', value: 'priceDesc' },
  { label: 'الأقل سعراً', value: 'priceAsc' },
]

const activeSortLabel = computed(() => options.find((x) => x.value === localSort.value)?.label || 'الأحدث')

function emitSort() {
  emit('update:sort', localSort.value)
}
</script>

<style scoped>
.products-select{ min-height: 52px; }
.results-pill{
  display:flex;
  align-items:center;
  justify-content:center;
  min-height:46px;
  padding:0 14px;
  border-radius:999px;
  border:1px solid rgba(var(--primary), .18);
  background:rgba(var(--primary), .08);
  color:rgb(var(--text));
  font-size:.9rem;
  font-weight:800;
}
</style>
