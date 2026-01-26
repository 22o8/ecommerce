<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="admin-box">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="text-xl font-extrabold rtl-text">{{ t('admin.products') }}</div>
          <div class="text-sm admin-muted rtl-text">{{ t('admin.productsHint') }}</div>
        </div>

        <div class="flex gap-2">
          <NuxtLink to="/admin/products/new" class="admin-primary">+ {{ t('admin.newProduct') }}</NuxtLink>
          <button class="admin-ghost" type="button" @click="fetchList(1)">{{ t('common.refresh') }}</button>
        </div>
      </div>

      <!-- Filters -->
      <div class="mt-4 grid gap-2 md:grid-cols-4">
        <input v-model="q" class="admin-input" :placeholder="t('admin.searchProducts')" @keydown.enter="fetchList(1)" />

        <select v-model="status" class="admin-input" @change="fetchList(1)">
          <option value="">{{ t('admin.all') }}</option>
          <option value="published">{{ t('admin.published') }}</option>
          <option value="draft">{{ t('admin.draft') }}</option>
        </select>

        <select v-model="sort" class="admin-input" @change="fetchList(1)">
          <option value="newest">{{ t('admin.sortNewest') }}</option>
          <option value="oldest">{{ t('admin.sortOldest') }}</option>
          <option value="title">{{ t('admin.sortTitle') }}</option>
          <option value="priceHigh">{{ t('admin.sortPriceHigh') }}</option>
          <option value="priceLow">{{ t('admin.sortPriceLow') }}</option>
        </select>

        <button class="admin-ghost" type="button" @click="fetchList(1)">{{ t('admin.search') }}</button>
      </div>

      <!-- Bulk -->
      <div class="mt-4 flex flex-wrap items-center justify-between gap-2">
        <div class="text-sm admin-muted rtl-text">
          {{ t('admin.total') }}: <span class="font-bold text-white">{{ total }}</span>
          <span v-if="selectedIds.length" class="ml-2">• {{ t('admin.selected') }}: {{ selectedIds.length }}</span>
        </div>

        <div class="flex gap-2" v-if="selectedIds.length">
          <button class="admin-pill" type="button" @click="bulkPublish(true)" :disabled="pending">
            {{ t('admin.publish') }}
          </button>
          <button class="admin-pill" type="button" @click="bulkPublish(false)" :disabled="pending">
            {{ t('admin.unpublish') }}
          </button>
          <button class="admin-btn-danger" type="button" @click="bulkDelete" :disabled="pending">
            {{ t('admin.delete') }}
          </button>
        </div>
      </div>
    </div>

    <!-- List -->
    <div class="admin-box overflow-hidden">
      <div v-if="loading" class="p-4 admin-muted rtl-text">{{ t('common.loading') }}</div>

      <div v-else-if="items.length === 0" class="p-6 text-center">
        <div class="text-lg font-extrabold rtl-text">{{ t('admin.noProducts') }}</div>
        <div class="admin-muted mt-1 rtl-text">{{ t('admin.noProductsHint') }}</div>
        <NuxtLink to="/admin/products/new" class="admin-primary inline-flex mt-4">+ {{ t('admin.addFirstProduct') }}</NuxtLink>
      </div>

      <div v-else class="admin-table">
        <div class="admin-tr admin-th">
          <div class="flex items-center gap-2">
            <input type="checkbox" :checked="allChecked" @change="toggleAll(($event.target as HTMLInputElement).checked)" />
            <span class="rtl-text">{{ t('admin.product') }}</span>
          </div>
          <div class="rtl-text">{{ t('common.price') }}</div>
          <div class="rtl-text">{{ t('admin.status') }}</div>
          <div class="text-right rtl-text">{{ t('common.actions') }}</div>
        </div>

        <div v-for="p in pagedItems" :key="p.id" class="admin-tr">
          <div class="flex items-center gap-3 min-w-0">
            <input type="checkbox" :checked="selectedIds.includes(p.id)" @change="toggleOne(p.id)" />

            <div class="thumb">
              <div class="thumb-inner">
                <span class="text-xs admin-muted">IMG</span>
              </div>
            </div>

            <div class="min-w-0">
              <div class="font-extrabold truncate">{{ p.title }}</div>
              <div class="text-xs admin-muted truncate">/{{ p.slug }}</div>
            </div>
          </div>

          <div class="font-bold">${{ Number(p.priceUsd || 0).toFixed(0) }}</div>

          <div>
            <span :class="p.isPublished ? 'badge-on' : 'badge-off'">
              {{ p.isPublished ? t('admin.published') : t('admin.draft') }}
            </span>
          </div>

          <div class="flex justify-end gap-2">
            <NuxtLink class="admin-pill" :to="`/admin/products/${p.id}`">{{ t('common.details') }}</NuxtLink>
            <button class="admin-pill" type="button" @click="quickToggle(p)" :disabled="pending">
              {{ p.isPublished ? t('admin.unpublish') : t('admin.publish') }}
            </button>
            <button class="admin-btn-danger" type="button" @click="removeOne(p)" :disabled="pending">{{ t('admin.delete') }}</button>
          </div>
        </div>

        <!-- Pagination (client-side) -->
        <div class="p-4 flex items-center justify-between">
          <div class="text-sm admin-muted rtl-text">{{ t('admin.page') }} {{ page }} / {{ totalPages }}</div>
          <div class="flex gap-2">
            <button class="admin-pill" type="button" :disabled="page<=1" @click="go(page-1)">{{ t('admin.prev') }}</button>
            <button class="admin-pill" type="button" :disabled="page>=totalPages" @click="go(page+1)">{{ t('admin.next') }}</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="error" class="admin-error rtl-text">{{ error }}</div>
    <div v-if="success" class="admin-success rtl-text">{{ success }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { ref, computed, onMounted } from 'vue'
import { useAdminApi } from '~/composables/useAdminApi'
import { useI18n } from '~/composables/useI18n'

type Product = {
  id: string
  title: string
  slug: string
  priceUsd: number
  isPublished: boolean
}

const { t } = useI18n()
const api = useAdminApi()

const q = ref('')
const status = ref<'published' | 'draft' | ''>('')
const sort = ref<'newest' | 'oldest' | 'title' | 'priceHigh' | 'priceLow'>('newest')

const loading = ref(false)
const pending = ref(false)
const error = ref('')
const success = ref('')

const items = ref<Product[]>([])
const total = computed(() => items.value.length)

// pagination client-side
const page = ref(1)
const pageSize = ref(10)
const totalPages = computed(() => Math.max(1, Math.ceil(items.value.length / pageSize.value)))
const pagedItems = computed(() => items.value.slice((page.value - 1) * pageSize.value, page.value * pageSize.value))

// selection
const selectedIds = ref<string[]>([])
const allChecked = computed(() => items.value.length > 0 && selectedIds.value.length === items.value.length)

function extractErr(e: any) {
  return e?.data?.message || e?.message || t('common.requestFailed')
}

function toggleOne(id: string) {
  if (selectedIds.value.includes(id)) selectedIds.value = selectedIds.value.filter(x => x !== id)
  else selectedIds.value.push(id)
}

function toggleAll(v: boolean) {
  selectedIds.value = v ? items.value.map(x => x.id) : []
}

function go(p: number) {
  page.value = Math.min(Math.max(1, p), totalPages.value)
}

function applyClientFilters(list: Product[]) {
  let out = [...list]

  const qq = q.value.trim().toLowerCase()
  if (qq) out = out.filter(x => (x.title || '').toLowerCase().includes(qq) || (x.slug || '').toLowerCase().includes(qq))

  if (status.value === 'published') out = out.filter(x => !!x.isPublished)
  if (status.value === 'draft') out = out.filter(x => !x.isPublished)

  if (sort.value === 'title') out.sort((a,b) => (a.title||'').localeCompare(b.title||''))
  if (sort.value === 'oldest') out.sort((a,b) => String(a.id).localeCompare(String(b.id)))
  if (sort.value === 'newest') out.sort((a,b) => String(b.id).localeCompare(String(a.id)))
  if (sort.value === 'priceHigh') out.sort((a,b) => (b.priceUsd||0) - (a.priceUsd||0))
  if (sort.value === 'priceLow') out.sort((a,b) => (a.priceUsd||0) - (b.priceUsd||0))

  return out
}

async function fetchList(p = 1) {
  loading.value = true
  error.value = ''
  success.value = ''
  selectedIds.value = []
  try {
    const res = await api.listAdminProducts<any[]>()
    const list = Array.isArray(res) ? res : []
    items.value = applyClientFilters(list.map(x => ({
      id: String(x.id),
      title: String(x.title || ''),
      slug: String(x.slug || ''),
      priceUsd: Number(x.priceUsd || 0),
      isPublished: !!x.isPublished,
    })))

    page.value = p
    go(p)
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

async function quickToggle(p: Product) {
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    await api.updateAdminProduct(p.id, { ...p, isPublished: !p.isPublished })
    success.value = t('admin.updated')
    await fetchList(page.value)
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function removeOne(p: Product) {
  if (!confirm(t('admin.confirmDeleteOne').replace('{title}', p.title))) return
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    await api.deleteAdminProduct(p.id)
    success.value = t('admin.deleted')
    await fetchList(Math.min(page.value, totalPages.value))
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function bulkPublish(v: boolean) {
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    for (const id of selectedIds.value) {
      const p = items.value.find(x => x.id === id)
      if (!p) continue
      await api.updateAdminProduct(id, { ...p, isPublished: v })
    }
    success.value = t('admin.bulkUpdated')
    await fetchList(page.value)
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function bulkDelete() {
  if (!confirm(t('admin.confirmDeleteMany').replace('{count}', String(selectedIds.value.length)))) return
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    for (const id of selectedIds.value) {
      await api.deleteAdminProduct(id)
    }
    success.value = t('admin.bulkDeleted')
    await fetchList(Math.min(page.value, totalPages.value))
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

onMounted(() => fetchList(1))
</script>

<style scoped>
.admin-box{
  border-radius: 20px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  padding: 16px;
}
.admin-muted{ color: rgba(255,255,255,.65); }

.admin-input{
  width: 100%;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  padding: 10px 12px;
  color: rgba(255,255,255,.9);
  outline: none;
}
.admin-input:focus{
  border-color: rgba(99,102,241,.35);
  box-shadow: 0 0 0 3px rgba(99,102,241,.12);
}

.admin-primary{
  padding: 10px 12px;
  border-radius: 14px;
  background: rgba(99,102,241,.22);
  border: 1px solid rgba(99,102,241,.35);
  color: white;
  font-weight: 900;
}
.admin-ghost{
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.85);
  font-weight: 800;
}
.admin-pill{
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.9);
  font-weight: 800;
}
.admin-btn-danger{
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.14);
  color: rgba(255,255,255,.95);
  font-weight: 900;
}

.admin-table{ display: grid; }
.admin-tr{
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 2fr;
  gap: 12px;
  padding: 12px 16px;
  border-top: 1px solid rgba(255,255,255,.08);
}
.admin-th{
  border-top: none;
  background: rgba(0,0,0,.18);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: .08em;
  color: rgba(255,255,255,.65);
}

.badge-on{
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(16,185,129,.35);
  background: rgba(16,185,129,.14);
  font-weight: 800;
  display: inline-flex;
}
.badge-off{
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(255,255,255,.12);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.75);
  font-weight: 800;
  display: inline-flex;
}

.admin-error{
  border-radius: 16px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.10);
  padding: 12px 14px;
}
.admin-success{
  border-radius: 16px;
  border: 1px solid rgba(16,185,129,.35);
  background: rgba(16,185,129,.10);
  padding: 12px 14px;
}

.thumb{
  width: 46px;
  height: 46px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  flex: 0 0 auto;
  overflow: hidden;
}
.thumb-inner{
  width: 100%;
  height: 100%;
  display:flex;
  align-items:center;
  justify-content:center;
}
</style>
