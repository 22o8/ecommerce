<!-- app/pages/admin/products/products.vue -->
<template>
  <div>
    <div class="flex flex-col md:flex-row md:items-end md:justify-between gap-4">
      <div>
        <h1 class="text-2xl md:text-3xl font-extrabold">{{ t('admin.products.title') }}</h1>
        <p class="admin-muted mt-1">{{ t('admin.productsPage.subtitle') }}</p>
      </div>

      <div class="flex items-center gap-2">
        <button class="btn-soft" type="button" @click="refresh" :disabled="loading">{{ t('common.refresh') }}</button>
        <button class="btn-primary" type="button" @click="openCreate">+ {{ t('admin.productsPage.newProduct') }}</button>
      </div>
    </div>

    <div class="mt-5 grid grid-cols-1 lg:grid-cols-[1fr_240px_140px] gap-3">
      <input v-model="q" class="inpt" :placeholder="t('admin.productsPage.searchPlaceholder')" />
      <select v-model="sort" class="inpt">
        <option value="">{{ t('admin.productsPage.sortDefault') }}</option>
        <option value="newest">{{ t('admin.productsPage.sortNewest') }}</option>
        <option value="oldest">{{ t('admin.productsPage.sortOldest') }}</option>
        <option value="price_desc">{{ t('admin.productsPage.sortPriceHigh') }}</option>
        <option value="price_asc">{{ t('admin.productsPage.sortPriceLow') }}</option>
      </select>
      <button class="btn-soft" @click="refresh">{{ t('admin.productsPage.searchButton') }}</button>
    </div>

    <div class="mt-5 admin-divider" />

    <div v-if="error" class="mt-4 alert-danger">
      {{ error }}
    </div>

    <div class="mt-5 overflow-auto">
      <table class="tbl min-w-[880px] w-full">
        <thead>
          <tr>
            <th>{{ t('admin.productsPage.tableTitle') }}</th>
            <th>{{ t('admin.productsPage.tableSlug') }}</th>
            <th class="text-right">{{ t('admin.products.price') }}</th>
            <th>{{ t('admin.status') }}</th>
            <th class="text-right">{{ t('admin.productsPage.tableCreated') }}</th>
            <th class="text-right">{{ t('common.actions') }}</th>
          </tr>
        </thead>

        <tbody v-if="!loading && items.length">
          <tr v-for="p in items" :key="p.id">
            <td class="font-semibold">{{ p.title }}</td>
            <td class="admin-muted">{{ p.slug }}</td>
            <td class="text-right font-semibold">${{ fmtMoney(p.priceUsd) }}</td>
            <td>
              <span :class="p.isPublished ? 'badge-success' : 'badge-muted'">
                {{ p.isPublished ? t('admin.productsPage.published') : t('admin.productsPage.draft') }}
              </span>
            </td>
            <td class="text-right admin-muted">{{ fmtDate(p.createdAt) }}</td>
            <td class="text-right">
              <div class="flex items-center justify-end gap-2">
                <button class="btn-mini" @click="openEdit(p)">{{ t('admin.productsPage.edit') }}</button>
                <button class="btn-mini" @click="togglePublish(p)">
                  {{ p.isPublished ? t('admin.productsPage.unpublish') : t('admin.productsPage.publish') }}
                </button>
                <button class="btn-mini-danger" @click="remove(p)">{{ t('common.delete') }}</button>
              </div>
            </td>
          </tr>
        </tbody>

        <tbody v-else-if="loading">
          <tr><td colspan="6" class="py-10 text-center admin-muted">{{ t('admin.productsPage.loading') }}</td></tr>
        </tbody>
        <tbody v-else>
          <tr><td colspan="6" class="py-10 text-center admin-muted">{{ t('admin.productsPage.empty') }}</td></tr>
        </tbody>
      </table>
    </div>

    <div class="mt-5 flex items-center justify-between">
      <div class="admin-muted text-sm">{{ t('admin.productsPage.total') }}: {{ totalCount }}</div>
      <div class="flex items-center gap-2">
        <button class="btn-soft" :disabled="page <= 1 || loading" @click="page--, refresh()">{{ t('common.prev') }}</button>
        <div class="admin-pill text-sm">{{ t('admin.productsPage.page') }} {{ page }}</div>
        <button class="btn-soft" :disabled="items.length < pageSize || loading" @click="page++, refresh()">{{ t('common.next') }}</button>
      </div>
    </div>

    <div v-if="modalOpen" class="modal">
      <div class="modal-card">
        <div class="flex items-start justify-between gap-3">
          <div>
            <div class="text-xl font-extrabold">{{ editing ? t('admin.productsPage.modalEdit') : t('admin.productsPage.modalNew') }}</div>
            <div class="admin-muted text-sm mt-1">{{ t('admin.productsPage.modalHint') }}</div>
          </div>
          <button class="icon-btn" @click="closeModal">✕</button>
        </div>

        <div class="mt-5 grid grid-cols-1 md:grid-cols-2 gap-3">
          <div>
            <label class="lbl">{{ t('admin.productsPage.tableTitle') }}</label>
            <input v-model="form.title" class="inpt" :placeholder="t('admin.productsPage.titlePlaceholder')" />
          </div>
          <div>
            <label class="lbl">{{ t('admin.productsPage.tableSlug') }}</label>
            <input v-model="form.slug" class="inpt" :placeholder="t('admin.productsPage.slugPlaceholder')" />
          </div>

          <div class="md:col-span-2">
            <label class="lbl">{{ t('admin.description') }}</label>
            <textarea v-model="form.description" class="inpt" rows="4" :placeholder="t('admin.productsPage.descriptionPlaceholder')" />
          </div>

          <div>
            <label class="lbl">{{ t('admin.productsPage.priceUsd') }}</label>
            <input v-model.number="form.priceUsd" type="number" class="inpt" min="0" step="1" />
          </div>

          <div class="flex items-center gap-3 pt-7">
            <input id="pub" type="checkbox" v-model="form.isPublished" class="h-4 w-4" />
            <label for="pub" class="text-sm font-semibold">{{ t('admin.productsPage.published') }}</label>
          </div>
        </div>

        <div v-if="modalError" class="mt-4 alert-danger">{{ modalError }}</div>

        <div class="mt-6 flex items-center justify-end gap-2">
          <button class="btn-soft" @click="closeModal">{{ t('cancel') }}</button>
          <button class="btn-primary" :disabled="saving" @click="save">
            {{ saving ? t('common.saving') : t('common.save') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useApi } from '~/composables/useApi'

const { t } = useI18n()

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
})

type Product = {
  id: string
  title: string
  slug: string
  description: string
  priceUsd: number
  isPublished: boolean
  createdAt: string
}

const api = useApi()

const loading = ref(false)
const saving = ref(false)
const error = ref('')
const modalError = ref('')

const q = ref('')
const sort = ref('')
const page = ref(1)
const pageSize = ref(10)

const items = ref<Product[]>([])
const totalCount = ref(0)

const modalOpen = ref(false)
const editing = ref<Product | null>(null)

const form = reactive({
  title: '',
  slug: '',
  description: '',
  priceUsd: 0,
  isPublished: true,
})

function fmtDate(v: string) {
  try { return new Date(v).toLocaleString() } catch { return v }
}
function fmtMoney(n: number) {
  return Number.isFinite(n) ? n.toFixed(2) : '0.00'
}

async function fetchList() {
  loading.value = true
  error.value = ''
  try {
    const res: any = await api.get('/admin/products', {
      query: { Page: page.value, PageSize: pageSize.value, Q: q.value || undefined, Sort: sort.value || undefined },
    })
    items.value = res?.items || res || []
    totalCount.value = res?.totalCount ?? items.value.length
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('admin.productsPage.loadFailed')
  } finally {
    loading.value = false
  }
}

function refresh() {
  page.value = Math.max(1, page.value)
  fetchList()
}

function openCreate() {
  editing.value = null
  modalError.value = ''
  form.title = ''
  form.slug = ''
  form.description = ''
  form.priceUsd = 0
  form.isPublished = true
  modalOpen.value = true
}

function openEdit(p: Product) {
  editing.value = p
  modalError.value = ''
  form.title = p.title
  form.slug = p.slug
  form.description = p.description
  form.priceUsd = p.priceUsd
  form.isPublished = p.isPublished
  modalOpen.value = true
}

function closeModal() {
  modalOpen.value = false
}

async function save() {
  modalError.value = ''
  if (!form.title.trim() || !form.slug.trim()) {
    modalError.value = t('admin.productsPage.titleSlugRequired')
    return
  }

  saving.value = true
  try {
    if (!editing.value) {
      await api.post('/admin/products', { ...form })
    } else {
      await api.put(`/admin/products/${editing.value.id}`, { ...form })
    }
    modalOpen.value = false
    await fetchList()
  } catch (e: any) {
    modalError.value = e?.data?.message || e?.message || t('admin.productsPage.saveFailed')
  } finally {
    saving.value = false
  }
}

async function togglePublish(p: Product) {
  try {
    await api.put(`/api/admin/products/${p.id}`, {
      title: p.title,
      slug: p.slug,
      description: p.description,
      priceUsd: p.priceUsd,
      isPublished: !p.isPublished,
    })
    await fetchList()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('admin.productsPage.updateFailed')
  }
}

async function remove(p: Product) {
  if (!confirm(t('admin.productsPage.deleteConfirm', { title: p.title }))) return
  try {
    await api.del(`/api/admin/products/${p.id}`)
    await fetchList()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('admin.productsPage.deleteFailed')
  }
}

onMounted(fetchList)
</script>

<style scoped>
.tbl th{
  text-align:left;
  font-size: 12px;
  letter-spacing: .12em;
  text-transform: uppercase;
  color: rgba(100,116,139,.95);
  padding: 14px 14px;
  border-bottom: 1px solid rgba(148,163,184,.18);
}
.tbl td{
  padding: 14px 14px;
  border-bottom: 1px solid rgba(148,163,184,.12);
  vertical-align: middle;
}
html[data-theme="dark"] .tbl th{ color: rgba(148,163,184,.90); }
html[data-theme="dark"] .tbl td{ color: rgba(226,232,240,.92); }

.inpt{
  width:100%;
  padding: 12px 14px;
  border-radius: 16px;
  border: 1px solid rgba(148,163,184,.20);
  outline: none;
}
html[data-theme="dark"] .inpt{ background: rgba(255,255,255,.06); color: rgba(226,232,240,.92); }

.lbl{ display:block; font-size:12px; letter-spacing:.08em; text-transform:uppercase; color: rgba(100,116,139,.95); margin-bottom: 6px; }
html[data-theme="dark"] .lbl{ color: rgba(148,163,184,.90); }

.badge-success{
  display:inline-flex; align-items:center; justify-content:center;
  padding: 6px 10px; border-radius: 999px;
  background: rgba(34,197,94,.16);
  border: 1px solid rgba(34,197,94,.25);
  font-weight: 800; font-size: 12px;
}
.badge-muted{
  display:inline-flex; align-items:center; justify-content:center;
  padding: 6px 10px; border-radius: 999px;
  background: rgba(148,163,184,.14);
  border: 1px solid rgba(148,163,184,.20);
  font-weight: 800; font-size: 12px;
}

.btn-mini{
  padding: 8px 10px;
  border-radius: 12px;
  border: 1px solid rgba(148,163,184,.20);
  background: rgba(255,255,255,.55);
  font-weight: 800;
}
html[data-theme="dark"] .btn-mini{ background: rgba(255,255,255,.06); color: rgba(226,232,240,.92); }

.btn-mini-danger{
  padding: 8px 10px;
  border-radius: 12px;
  border: 1px solid rgba(239,68,68,.22);
  background: rgba(239,68,68,.14);
  color: rgb(239,68,68);
  font-weight: 900;
}

.alert-danger{
  padding: 12px 14px;
  border-radius: 16px;
  border: 1px solid rgba(239,68,68,.22);
  background: rgba(239,68,68,.12);
  color: rgba(239,68,68,.95);
  font-weight: 700;
}

.modal{
  position: fixed;
  inset: 0;
  background: rgba(2,6,23,.45);
  display:flex;
  align-items:center;
  justify-content:center;
  padding: 18px;
  z-index: 80;
}
.modal-card{
  width: min(720px, 100%);
  border-radius: 24px;
  border: 1px solid rgba(148,163,184,.18);
  background: rgba(255,255,255,.86);
  backdrop-filter: blur(14px);
  padding: 16px;
}
html[data-theme="dark"] .modal-card{
  background: rgba(15,18,28,.72);
}
</style>
