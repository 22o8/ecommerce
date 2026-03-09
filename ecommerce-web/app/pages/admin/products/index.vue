<template>
  <div class="space-y-5 admin-products-page">
    <section class="admin-products-hero">
      <div class="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
        <div>
          <div class="admin-products-kicker rtl-text">{{ t('admin.products.title') }}</div>
          <div class="mt-2 text-2xl font-black rtl-text sm:text-3xl">{{ t('admin.manageProducts') }}</div>
          <div class="mt-2 text-sm admin-muted rtl-text">{{ t('admin.productsHint') }}</div>
        </div>

        <div class="flex flex-wrap gap-2">
          <NuxtLink to="/admin/products/new" class="admin-primary">+ {{ t('admin.newProduct') }}</NuxtLink>
          <button class="admin-ghost" type="button" @click="fetchList(1)">{{ t('common.refresh') }}</button>
        </div>
      </div>

      <div class="mt-5 grid gap-3 md:grid-cols-3">
        <div class="hero-stat">
          <div class="hero-stat__label rtl-text">{{ t('admin.total') }}</div>
          <div class="hero-stat__value keep-ltr">{{ total }}</div>
        </div>
        <div class="hero-stat">
          <div class="hero-stat__label rtl-text">{{ t('admin.published') }}</div>
          <div class="hero-stat__value keep-ltr">{{ publishedCount }}</div>
        </div>
        <div class="hero-stat">
          <div class="hero-stat__label rtl-text">{{ t('admin.featured') }}</div>
          <div class="hero-stat__value keep-ltr">{{ featuredCount }}</div>
        </div>
      </div>
    </section>

    <section class="admin-box admin-filters-shell">
      <div class="grid gap-3 xl:grid-cols-[1.2fr_.7fr_.7fr_.5fr]">
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

      <div class="mt-4 flex flex-wrap items-center justify-between gap-3">
        <div class="text-sm admin-muted rtl-text">
          {{ t('admin.total') }}: <span class="font-bold text-[rgb(var(--text))]">{{ total }}</span>
          <span v-if="selectedIds.length" class="ml-2">• {{ t('admin.selected') }}: {{ selectedIds.length }}</span>
        </div>

        <div class="flex gap-2" v-if="selectedIds.length">
          <button class="admin-pill" type="button" @click="bulkPublish(true)" :disabled="pending">{{ t('admin.publish') }}</button>
          <button class="admin-pill" type="button" @click="bulkPublish(false)" :disabled="pending">{{ t('admin.unpublish') }}</button>
          <button class="admin-btn-danger" type="button" @click="bulkDelete" :disabled="pending">{{ t('admin.delete') }}</button>
        </div>
      </div>
    </section>

    <section class="admin-box overflow-hidden">
      <div v-if="loading" class="p-4 admin-muted rtl-text">{{ t('common.loading') }}</div>
      <div v-else-if="items.length === 0" class="p-8 text-center">
        <div class="text-lg font-black rtl-text">{{ t('admin.noProducts') }}</div>
        <div class="admin-muted mt-1 rtl-text">{{ t('admin.noProductsHint') }}</div>
        <NuxtLink to="/admin/products/new" class="admin-primary inline-flex mt-4">+ {{ t('admin.addFirstProduct') }}</NuxtLink>
      </div>

      <div v-else class="grid gap-3 p-2 sm:p-3">
        <div class="products-head hidden lg:grid">
          <div class="flex items-center gap-2">
            <input type="checkbox" :checked="allChecked" @change="toggleAll(($event.target as HTMLInputElement).checked)" />
            <span class="rtl-text">{{ t('admin.product') }}</span>
          </div>
          <div class="rtl-text">{{ t('common.price') }}</div>
          <div class="rtl-text">{{ t('admin.status') }}</div>
          <div class="text-right rtl-text">{{ t('common.actions') }}</div>
        </div>

        <div v-for="p in pagedItems" :key="p.id" class="product-row-card" role="button" tabindex="0" @click="goDetails(p.id)" @keydown.enter="goDetails(p.id)">
          <div class="product-row-card__main">
            <input type="checkbox" :checked="selectedIds.includes(p.id)" @click.stop @change.stop="toggleOne(p.id)" />
            <div class="thumb" aria-hidden="true"><div class="thumb-inner"><span class="thumb-dot" /></div></div>
            <div class="min-w-0">
              <div class="font-black truncate rtl-text">{{ p.title }}</div>
              <div class="text-xs admin-muted truncate keep-ltr">/{{ p.slug }}</div>
            </div>
          </div>

          <div class="product-row-card__price">
            <div class="font-black keep-ltr">{{ formatIqd(p.priceIqd || 0) }}</div>
            <div v-if="p.priceUsd" class="text-xs opacity-70 keep-ltr">${{ Number(p.priceUsd).toFixed(0) }}</div>
          </div>

          <div class="product-row-card__meta">
            <span :class="p.isPublished ? 'badge-on' : 'badge-off'">{{ p.isPublished ? t('admin.published') : t('admin.draft') }}</span>
            <span v-if="p.isFeatured" class="badge-featured"><span class="badge-icon">★</span><span class="badge-text">{{ t('admin.featured') }}</span></span>
          </div>

          <div class="actions-wrap" @click.stop>
            <NuxtLink class="admin-icon-btn" :to="`/admin/products/${p.id}`" :title="t('common.details')" :aria-label="t('common.details')" @click.stop>
              <Icon name="mdi:information-outline" class="text-lg" />
              <span class="btn-label">{{ t('common.details') }}</span>
            </NuxtLink>
            <button class="admin-icon-btn" type="button" @click.stop="quickToggle(p)" :disabled="pending">
              <Icon :name="p.isPublished ? 'mdi:eye-off-outline' : 'mdi:eye-outline'" class="text-lg" />
              <span class="btn-label">{{ p.isPublished ? t('admin.unpublish') : t('admin.publish') }}</span>
            </button>
            <button class="admin-icon-btn" type="button" @click.stop="toggleFeatured(p)" :disabled="pending">
              <Icon :name="p.isFeatured ? 'mdi:star-off-outline' : 'mdi:star-outline'" class="text-lg" />
              <span class="btn-label">{{ p.isFeatured ? t('admin.unfeature') : t('admin.feature') }}</span>
            </button>
            <button class="admin-icon-btn danger" type="button" @click.stop="removeOne(p)" :disabled="pending">
              <Icon name="mdi:trash-can-outline" class="text-lg" />
              <span class="btn-label">{{ t('admin.delete') }}</span>
            </button>
          </div>
        </div>

        <div class="p-2 sm:p-4 flex items-center justify-between gap-3">
          <div class="text-sm admin-muted rtl-text">{{ t('admin.page') }} {{ page }} / {{ totalPages }}</div>
          <div class="flex gap-2">
            <button class="admin-pill" type="button" :disabled="page<=1" @click="go(page-1)">{{ t('admin.prev') }}</button>
            <button class="admin-pill" type="button" :disabled="page>=totalPages" @click="go(page+1)">{{ t('admin.next') }}</button>
          </div>
        </div>
      </div>
    </section>

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

type Product = { id: string; title: string; slug: string; priceIqd: number; priceUsd?: number; isPublished: boolean; isFeatured: boolean }
const { t } = useI18n()
const api = useAdminApi()
const { formatIqd } = useMoney()
const router = useRouter()
function goDetails(id: any) { const pid = typeof id === 'string' ? id : (id?.id ?? id?.value ?? ''); router.push(`/admin/products/${encodeURIComponent(String(pid))}`) }
const q = ref('')
const status = ref<'published' | 'draft' | ''>('')
const sort = ref<'newest' | 'oldest' | 'title' | 'priceHigh' | 'priceLow'>('newest')
const loading = ref(false)
const pending = ref(false)
const busyId = ref<string | null>(null)
const error = ref('')
const success = ref('')
const items = ref<Product[]>([])
const total = computed(() => items.value.length)
const publishedCount = computed(() => items.value.filter(x => x.isPublished).length)
const featuredCount = computed(() => items.value.filter(x => x.isFeatured).length)
const page = ref(1)
const pageSize = ref(10)
const totalPages = computed(() => Math.max(1, Math.ceil(items.value.length / pageSize.value)))
const pagedItems = computed(() => items.value.slice((page.value - 1) * pageSize.value, page.value * pageSize.value))
const selectedIds = ref<string[]>([])
const allChecked = computed(() => items.value.length > 0 && selectedIds.value.length === items.value.length)
function extractErr(e: any) { return e?.data?.message || e?.message || t('common.requestFailed') }
function toggleOne(id: string) { if (selectedIds.value.includes(id)) selectedIds.value = selectedIds.value.filter(x => x !== id); else selectedIds.value.push(id) }
function toggleAll(v: boolean) { selectedIds.value = v ? items.value.map(x => x.id) : [] }
function go(p: number) { page.value = Math.min(Math.max(1, p), totalPages.value) }
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
  loading.value = true; error.value=''; success.value=''; selectedIds.value=[]
  try {
    const res = await api.listAdminProducts<any[]>()
    const list = Array.isArray(res) ? res : []
    items.value = applyClientFilters(list.map(x => ({ id: String(x.id), title: String(x.name ?? x.title ?? ''), slug: String(x.slug || ''), priceIqd: Number((x.priceIqd ?? x.price ?? x.priceUsd) ?? 0), priceUsd: x.priceUsd == null ? undefined : Number(x.priceUsd), isPublished: !!(x.isActive ?? x.isPublished), isFeatured: !!x.isFeatured })))
    page.value = p; go(p)
  } catch (e:any) { error.value = extractErr(e) } finally { loading.value = false }
}
async function buildUpsertPayload(id: string, overrides: Partial<{ isPublished: boolean; isFeatured: boolean }> = {}) {
  const full: any = await api.getAdminProduct<any>(id)
  return { title: String(full.title || ''), slug: String(full.slug || ''), description: String(full.description || ''), priceUsd: Number(full.priceUsd ?? 0), brand: String(full.brand || ''), isPublished: overrides.isPublished ?? !!full.isPublished, isFeatured: overrides.isFeatured ?? !!full.isFeatured }
}
async function quickToggle(p: Product) { pending.value=true; error.value=''; success.value=''; try { const body = await buildUpsertPayload(p.id,{ isPublished: !p.isPublished}); await api.updateAdminProduct(p.id, body); success.value=t('admin.updated'); await fetchList(page.value)} catch(e:any){error.value=extractErr(e)} finally {pending.value=false} }
async function toggleFeatured(p: Product) { if (busyId.value) return; busyId.value = p.id; try { await api.setAdminProductFeatured(p.id, !p.isFeatured); p.isFeatured = !p.isFeatured } finally { busyId.value = null } }
async function removeOne(p: Product) { if (!confirm(t('admin.confirmDeleteOne').replace('{title}', p.title))) return; pending.value=true; error.value=''; success.value=''; try { await api.deleteAdminProduct(p.id); success.value=t('admin.deleted'); await fetchList(Math.min(page.value,totalPages.value)) } catch(e:any){ error.value=extractErr(e)} finally {pending.value=false} }
async function bulkPublish(v: boolean) { pending.value=true; error.value=''; success.value=''; try { for (const id of selectedIds.value) { const body = await buildUpsertPayload(id,{ isPublished:v }); await api.updateAdminProduct(id, body)} success.value=t('admin.bulkUpdated'); await fetchList(page.value)} catch(e:any){ error.value=extractErr(e)} finally {pending.value=false} }
async function bulkDelete() { if (!confirm(t('admin.confirmDeleteMany').replace('{count}', String(selectedIds.value.length)))) return; pending.value=true; error.value=''; success.value=''; try { for (const id of selectedIds.value) await api.deleteAdminProduct(id); success.value=t('admin.bulkDeleted'); await fetchList(Math.min(page.value,totalPages.value))} catch(e:any){ error.value=extractErr(e)} finally {pending.value=false} }
onMounted(() => fetchList(1))
</script>

<style scoped>
.admin-products-hero,
.admin-box,
.hero-stat,
.product-row-card,
.admin-icon-btn{
  border: 1px solid rgba(var(--border), .96);
}
.admin-products-hero,
.admin-box{ border-radius: 28px; padding: 20px; background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .90)); box-shadow: 0 18px 50px rgba(0,0,0,.12); }
.admin-products-kicker{ font-size: 12px; font-weight: 900; letter-spacing: .16em; text-transform: uppercase; color: rgb(var(--primary)); }
.admin-muted{ color: rgb(var(--muted)); }
.hero-stat{ border-radius: 22px; padding: 16px; background: rgba(var(--surface-rgb), .72); }
.hero-stat__label{ font-size: 12px; color: rgb(var(--muted)); }
.hero-stat__value{ margin-top: 8px; font-size: 26px; font-weight: 900; }
.admin-filters-shell{ padding-top: 16px; }
.admin-input{ width: 100%; border-radius: 18px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); padding: 12px 14px; color: rgb(var(--text)); outline: none; }
.admin-input:focus{ border-color: rgba(var(--primary), .45); box-shadow: 0 0 0 4px rgba(var(--primary), .12); }
.admin-primary{ padding: 11px 14px; border-radius: 16px; background: rgb(var(--primary)); color: #111; border: 1px solid rgba(var(--primary), .3); font-weight: 900; }
.admin-ghost{ padding: 11px 14px; border-radius: 16px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); color: rgb(var(--text)); font-weight: 800; }
.admin-pill{ padding: 9px 12px; border-radius: 999px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); color: rgb(var(--text)); font-weight: 800; }
.admin-btn-danger{ padding: 9px 12px; border-radius: 999px; border: 1px solid rgba(239,68,68,.45); background: rgba(239,68,68,.14); color: rgb(var(--text)); font-weight: 900; }
.products-head{ grid-template-columns: 2fr 1fr .9fr 1.6fr; gap: 14px; padding: 10px 14px; border-radius: 18px; background: rgba(var(--surface-rgb), .66); font-size: 12px; font-weight: 900; color: rgb(var(--muted)); }
.product-row-card{ display:grid; grid-template-columns: 2fr 1fr .9fr 1.6fr; gap: 14px; align-items:center; border-radius: 24px; padding: 16px; background: rgba(var(--surface-rgb), .72); transition: transform .2s ease, box-shadow .2s ease, border-color .2s ease; }
.product-row-card:hover{ transform: translateY(-2px); border-color: rgba(var(--primary), .28); box-shadow: 0 18px 40px rgba(0,0,0,.10); }
.product-row-card__main{ display:flex; align-items:center; gap: 12px; min-width: 0; }
.product-row-card__meta{ display:flex; align-items:center; gap: 8px; flex-wrap: wrap; }
.actions-wrap{ display:flex; align-items:center; justify-content:flex-end; gap: 8px; flex-wrap: wrap; }
.admin-icon-btn{ display:inline-flex; align-items:center; gap: 8px; border-radius: 16px; padding: 10px 12px; background: rgba(var(--surface-rgb), .92); font-weight: 800; }
.admin-icon-btn.danger{ border-color: rgba(239,68,68,.3); }
.badge-on,.badge-off,.badge-featured{ display:inline-flex; align-items:center; gap: 6px; padding: 6px 10px; border-radius:999px; font-weight: 900; }
.badge-on{ border:1px solid rgba(16,185,129,.30); background: rgba(16,185,129,.14); }
.badge-off{ border:1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); color: rgb(var(--muted)); }
.badge-featured{ border:1px solid rgba(var(--primary), .30); background: rgba(var(--primary), .14); }
.admin-error,.admin-success{ border-radius: 18px; padding: 12px 14px; }
.admin-error{ border:1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); }
.admin-success{ border:1px solid rgba(16,185,129,.35); background: rgba(16,185,129,.10); }
.thumb{ width: 52px; height: 52px; border-radius: 18px; border: 1px solid rgba(var(--border), .96); background: rgba(var(--surface-rgb), .92); flex: 0 0 auto; overflow:hidden; }
.thumb-inner{ width:100%; height:100%; display:grid; place-items:center; }
.thumb-dot{ width: 12px; height: 12px; border-radius: 999px; background: rgba(var(--primary), .35); box-shadow: 0 0 0 7px rgba(var(--primary), .12); }
@media (max-width: 1023px){
  .product-row-card{ grid-template-columns: 1fr; }
  .products-head{ display:none; }
  .actions-wrap{ justify-content:flex-start; }
}
:global(html.theme-light) .admin-products-hero,
:global(html.theme-light) .admin-box,
:global(html.theme-light) .hero-stat{ background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(252,245,249,.96)); box-shadow: 0 18px 44px rgba(236,72,153,.08), 0 8px 24px rgba(24,24,24,.04); }
:global(html.theme-light) .product-row-card,
:global(html.theme-light) .admin-icon-btn,
:global(html.theme-light) .products-head{ background: rgba(255,255,255,.92); }
</style>
