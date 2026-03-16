<template>
  <div class="space-y-5 products-admin-page">
    <!-- Header -->
    <div class="admin-box admin-box--hero">
      <div class="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div>
          <div class="text-xl font-extrabold">{{ t('admin.products.title') }}</div>
          <div class="text-sm admin-muted rtl-text">{{ t('admin.productsHint') }}</div>
        </div>

        <div class="flex gap-2 flex-wrap">
          <NuxtLink to="/admin/products/new" class="admin-primary">+ {{ t('admin.newProduct') }}</NuxtLink><button class="admin-ghost" type="button" @click="fetchList(1)">{{ t('common.refresh') }}</button></div>
      </div>

      <!-- Filters -->
      <div class="filter-grid mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
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
    <div class="admin-box admin-list-shell overflow-hidden">
      <div v-if="loading" class="p-4 admin-muted rtl-text">{{ t('common.loading') }}</div>

      <div v-else-if="items.length === 0" class="p-6 text-center">
        <div class="text-lg font-extrabold rtl-text">{{ t('admin.noProducts') }}</div>
        <div class="admin-muted mt-1 rtl-text">{{ t('admin.noProductsHint') }}</div>
        <NuxtLink to="/admin/products/new" class="admin-primary inline-flex mt-4">+ {{ t('admin.addFirstProduct') }}</NuxtLink>
      </div>

      <div v-else class="admin-table">
        <div class="admin-tr products-tr admin-th">
          <div class="flex items-center gap-2">
            <input type="checkbox" :checked="allChecked" @change="toggleAll(($event.target as HTMLInputElement).checked)" />
            <span class="rtl-text">{{ t('admin.product') }}</span>
          </div>
          <div class="rtl-text">{{ t('common.price') }}</div>
          <div class="rtl-text">{{ t('admin.status') }}</div>
          <div class="rtl-text">{{ t('admin.stockQuantity') }}</div>
          <div class="text-right rtl-text">{{ t('common.actions') }}</div>
        </div>

        <div
          v-for="p in pagedItems"
          :key="p.id"
          class="admin-tr products-tr cursor-pointer select-none"
          role="button"
          tabindex="0"
          @click="goDetails(p.id)"
          @keydown.enter="goDetails(p.id)"
        >
          <div class="flex items-center gap-3 min-w-0">
            <input
              type="checkbox"
              :checked="selectedIds.includes(p.id)"
              @click.stop
              @change.stop="toggleOne(p.id)"
            />

            <img
              v-if="resolveImage(p)"
              :src="resolveImage(p)"
              :alt="p.title"
              class="product-thumb"
              loading="lazy"
            />

            <div class="min-w-0">
              <div class="font-extrabold product-name">{{ p.title }}</div>
              <div class="mt-1 flex flex-wrap items-center gap-2">
                <div class="text-xs admin-muted product-slug">/{{ p.slug }}</div>
                <span class="product-category-chip rtl-text">{{ p.category || 'general' }}</span>
              </div>
            </div>
          </div>

          <div class="font-bold leading-tight">
            {{ formatIqd(p.priceIqd || 0) }}
            <div v-if="p.priceUsd" class="text-xs opacity-70">${{ Number(p.priceUsd).toFixed(0) }}</div>
          </div>

          <div>
            <span :class="p.isPublished ? 'badge-on' : 'badge-off'">
              {{ p.isPublished ? t('admin.published') : t('admin.draft') }}
            </span>

            <span v-if="p.isFeatured" class="badge-featured ml-2">
              <span class="badge-icon">★</span>
              <span class="badge-text">{{ t('admin.featured') }}</span>
            </span>
          </div>

          <div class="font-bold leading-tight stock-cell">
            <span class="keep-ltr stock-value">{{ p.stockQuantity ?? 0 }}</span>
            <div class="text-xs opacity-70 rtl-text">{{ (p.stockQuantity ?? 0) <= (p.lowStockThreshold ?? 0) ? 'منخفض' : 'مستقر' }}</div>
          </div>

          <div class="actions-wrap" @click.stop>
            <NuxtLink
              class="admin-icon-btn"
              :to="`/admin/products/${p.id}`"
              :title="t('common.details')"
              :aria-label="t('common.details')"
              @click.stop
            >
              <Icon name="mdi:information-outline" class="text-lg" />
              <span class="btn-label">{{ t('common.details') }}</span>
            </NuxtLink>

            <button
              class="admin-icon-btn"
              type="button"
              @click.stop="quickToggle(p)"
              :disabled="pending"
              :title="p.isPublished ? t('admin.unpublish') : t('admin.publish')"
              :aria-label="p.isPublished ? t('admin.unpublish') : t('admin.publish')"
            >
              <Icon :name="p.isPublished ? 'mdi:eye-off-outline' : 'mdi:eye-outline'" class="text-lg" />
              <span class="btn-label">{{ p.isPublished ? t('admin.unpublish') : t('admin.publish') }}</span>
            </button>

            <button
              class="admin-icon-btn"
              type="button"
              @click.stop="toggleFeatured(p)"
              :disabled="pending"
              :title="p.isFeatured ? t('admin.unfeature') : t('admin.feature')"
              :aria-label="p.isFeatured ? t('admin.unfeature') : t('admin.feature')"
            >
              <Icon :name="p.isFeatured ? 'mdi:star-off-outline' : 'mdi:star-outline'" class="text-lg" />
              <span class="btn-label">{{ p.isFeatured ? t('admin.unfeature') : t('admin.feature') }}</span>
            </button>

            <button
              class="admin-icon-btn danger"
              type="button"
              @click.stop="removeOne(p)"
              :disabled="pending"
              :title="t('admin.delete')"
              :aria-label="t('admin.delete')"
            >
              <Icon name="mdi:trash-can-outline" class="text-lg" />
              <span class="btn-label">{{ t('admin.delete') }}</span>
            </button>
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
import { useMoney } from '~/composables/useMoney'

type Product = {
  id: string
  title: string
  slug: string
  priceIqd: number
  priceUsd?: number
  isPublished: boolean
  isFeatured: boolean
  stockQuantity?: number
  lowStockThreshold?: number
  category?: string
  subCategory?: string
  imageUrl?: string
}

const { t } = useI18n()
const api = useAdminApi()
const publicApi = useApi()
const { formatIqd } = useMoney()

const router = useRouter()

function goDetails(id: any) {
  const pid = typeof id === 'string' ? id : (id?.id ?? id?.value ?? '')
  router.push(`/admin/products/${encodeURIComponent(String(pid))}`)
}

const q = ref('')
const status = ref<'published' | 'draft' | ''>('')
const sort = ref<'newest' | 'oldest' | 'title' | 'priceHigh' | 'priceLow'>('newest')

const loading = ref(false)
const pending = ref(false)
// used to prevent double clicks on per-row actions (feature/publish toggles)
const busyId = ref<string | null>(null)
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

function resolveImage(p: Product) {
  return p.imageUrl || ''
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
      title: String(x.name ?? x.title ?? ''),
      slug: String(x.slug || ''),
      // Display IQD (fallback to whatever field exists)
      priceIqd: Number((x.priceIqd ?? x.price ?? x.priceUsd) ?? 0),
      // Keep USD if backend provides it (optional)
      priceUsd: x.priceUsd == null ? undefined : Number(x.priceUsd),
      isPublished: !!(x.isActive ?? x.isPublished),
      isFeatured: !!x.isFeatured,
      stockQuantity: Number(x.stockQuantity ?? 0),
      lowStockThreshold: Number(x.lowStockThreshold ?? 0),
      category: String(x.category || 'general'),
      subCategory: String(x.subCategory || ''),
      imageUrl: typeof x.coverImage === 'string'
        ? publicApi.buildAssetUrl(x.coverImage)
        : (typeof x.imageUrl === 'string'
            ? publicApi.buildAssetUrl(x.imageUrl)
            : (Array.isArray(x.images) && x.images[0]
                ? publicApi.buildAssetUrl(String(x.images[0]?.url || x.images[0]?.imageUrl || x.images[0]?.path || x.images[0]))
                : '')),
    })))

    page.value = p
    go(p)
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

// Admin update endpoint expects the full UpsertProductRequest payload.
// Our list items are intentionally minimal, so for any toggle/update we fetch the full product first.
async function buildUpsertPayload(id: string, overrides: Partial<{ isPublished: boolean; isFeatured: boolean }> = {}) {
  const full: any = await api.getAdminProduct<any>(id)
  return {
    title: String(full.title || ''),
    slug: String(full.slug || ''),
    description: String(full.description || ''),
    priceUsd: Number(full.priceUsd ?? 0),
    priceIqd: Number(full.priceIqd ?? full.priceUsd ?? 0),
    brand: String(full.brand || ''),
    category: String(full.category || 'general'),
    subCategory: String(full.subCategory || ''),
    stockQuantity: Number(full.stockQuantity ?? 0),
    lowStockThreshold: Number(full.lowStockThreshold ?? 5),
    isPublished: overrides.isPublished ?? !!full.isPublished,
    isFeatured: overrides.isFeatured ?? !!full.isFeatured,
  }
}

async function quickToggle(p: Product) {
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    const body = await buildUpsertPayload(p.id, { isPublished: !p.isPublished })
    await api.updateAdminProduct(p.id, body)
    success.value = t('admin.updated')
    await fetchList(page.value)
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function toggleFeatured(p: Product) {
  if (busyId.value) return
  busyId.value = p.id
  try {
    await api.setAdminProductFeatured(p.id, !p.isFeatured)
    p.isFeatured = !p.isFeatured
  } finally {
    busyId.value = null
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
      const body = await buildUpsertPayload(id, { isPublished: v })
      await api.updateAdminProduct(id, body)
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
.products-admin-page{
  --soft-glow: 0 22px 60px rgba(12, 16, 32, .16);
}
.admin-box{
  border-radius: 28px;
  border: 1px solid rgba(var(--border), .95);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-rgb), .88));
  padding: 18px;
  box-shadow: var(--soft-glow);
}
.admin-box--hero{
  position: relative;
  overflow: hidden;
}
.admin-box--hero::after{
  content: '';
  position: absolute;
  inset: auto auto -70px -40px;
  width: 220px;
  height: 220px;
  border-radius: 999px;
  background: radial-gradient(circle, rgba(var(--primary), .14), transparent 68%);
  pointer-events: none;
}
.admin-list-shell{ padding: 10px; }
.admin-muted{ color: rgb(var(--muted)); }
.admin-input{ width: 100%; border-radius: 18px; border: 1px solid rgb(var(--border)); background: rgb(var(--surface-2)); padding: 12px 14px; color: rgb(var(--fg)); outline: none; min-height: 50px; }
.admin-input:focus{ border-color: rgba(99,102,241,.45); box-shadow: 0 0 0 4px rgba(99,102,241,.12); }
.admin-primary,.admin-ghost,.admin-pill,.admin-btn-danger{ display:inline-flex; align-items:center; justify-content:center; min-height:46px; padding: 10px 14px; border-radius: 16px; font-weight: 900; transition: transform .16s ease, border-color .16s ease, background .16s ease; }
.admin-primary{ background: linear-gradient(135deg, rgba(var(--primary), .24), rgba(var(--primary), .14)); border: 1px solid rgba(var(--primary), .38); color: rgb(var(--fg)); }
.admin-ghost{ border: 1px solid rgb(var(--border)); background: rgb(var(--surface)); color: rgb(var(--fg)); }
.admin-pill{ border: 1px solid rgb(var(--border)); background: rgb(var(--surface-2)); color: rgb(var(--fg)); }
.admin-btn-danger{ border: 1px solid rgba(239,68,68,.45); background: rgba(239,68,68,.14); color: rgb(var(--fg)); }
.admin-table{ display: grid; gap: 12px; }
.admin-tr{ display: grid; grid-template-columns: minmax(0,2.1fr) .95fr 1fr .8fr minmax(0,1.25fr); gap: 14px; padding: 16px; border: 1px solid rgba(var(--border), .88); border-radius: 22px; background: linear-gradient(180deg, rgba(var(--surface-rgb), .82), rgba(var(--surface-2-rgb), .88)); }
.admin-th{ border-radius: 18px; background: rgba(var(--surface-2-rgb), .95); font-size: 12px; text-transform: uppercase; letter-spacing: .08em; color: rgb(var(--muted)); }
.badge-on,.badge-off,.badge-featured{ padding: 6px 10px; border-radius: 999px; font-weight: 800; display: inline-flex; align-items:center; gap:6px; }
.badge-on{ border: 1px solid rgba(16,185,129,.40); background: rgba(16,185,129,.12); }
.badge-off{ border: 1px solid rgb(var(--border)); background: rgb(var(--surface)); color: rgb(var(--muted)); }
.badge-featured{ border: 1px solid rgba(99,102,241,.45); background: rgba(99,102,241,.12); }
.admin-error,.admin-success{ border-radius: 18px; padding: 13px 15px; }
.admin-error{ border: 1px solid rgba(239,68,68,.45); background: rgba(239,68,68,.10); }
.admin-success{ border: 1px solid rgba(16,185,129,.40); background: rgba(16,185,129,.10); }
.product-thumb{ width: 60px; height: 60px; border-radius: 20px; object-fit: cover; border: 1px solid rgba(var(--border), .95); flex: 0 0 auto; box-shadow: 0 12px 24px rgba(0,0,0,.16); }
.products-tr{align-items:center;}
.product-name{line-height:1.35;word-break:break-word;}
.product-slug{line-height:1.35;word-break:break-word;}
.product-category-chip{ display:inline-flex; padding:.3rem .6rem; border-radius:999px; border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-2-rgb), .9); font-size:.7rem; font-weight:800; color:rgb(var(--muted)); }
.stock-cell{display:grid;gap:4px;}
.stock-value{font-size:1.05rem;}
.actions-wrap{display:flex;flex-wrap:wrap;justify-content:flex-end;gap:8px;}
.admin-icon-btn{display:inline-flex;align-items:center;gap:6px;padding:9px 11px;border-radius:14px;border:1px solid rgb(var(--border));background:rgb(var(--surface-2));font-weight:800;}
.admin-icon-btn.danger{border-color:rgba(239,68,68,.35);background:rgba(239,68,68,.10);}
.btn-label{font-size:12px;}
.filter-grid{align-items:stretch;}
@media (max-width: 1100px){
  .admin-tr{ grid-template-columns:minmax(0,1.9fr) .9fr 1fr .8fr minmax(0,1.15fr); }
}
@media (max-width: 768px){
  .admin-box{ border-radius:24px; padding:14px; }
  .admin-list-shell{ padding: 0; border: none; box-shadow:none; background: transparent; }
  .admin-tr.products-tr.admin-th{display:none;}
  .admin-tr.products-tr{grid-template-columns:1fr;gap:12px;padding:14px;border-radius:22px;background:linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .95));}
  .admin-tr.products-tr > div:nth-child(2),
  .admin-tr.products-tr > div:nth-child(3),
  .admin-tr.products-tr > div:nth-child(4){padding-inline-start:0;}
  .admin-tr.products-tr > div:nth-child(2){display:flex;justify-content:space-between;align-items:center;font-size:15px;}
  .admin-tr.products-tr > div:nth-child(3){display:flex;align-items:center;gap:8px;flex-wrap:wrap;}
  .actions-wrap{justify-content:flex-start;}
  .btn-label{display:inline;}
  .product-thumb{ width:56px; height:56px; border-radius:18px; }
}
</style>