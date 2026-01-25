<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="admin-box">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="text-xl font-extrabold">Products</div>
          <div class="text-sm admin-muted">Create, publish and manage products</div>
        </div>

        <div class="flex gap-2">
          <NuxtLink to="/admin/products/new" class="admin-primary">+ New Product</NuxtLink>
          <button class="admin-ghost" type="button" @click="fetchList(1)">Refresh</button>
        </div>
      </div>

      <!-- Filters -->
      <div class="mt-4 grid gap-2 md:grid-cols-4">
        <input
          v-model="q"
          class="admin-input"
          placeholder="Search title or slug..."
          @keydown.enter="fetchList(1)"
        />

        <select v-model="status" class="admin-input" @change="fetchList(1)">
          <option value="">All</option>
          <option value="published">Published</option>
          <option value="draft">Draft</option>
        </select>

        <select v-model="sort" class="admin-input" @change="fetchList(1)">
          <option value="newest">Newest</option>
          <option value="oldest">Oldest</option>
          <option value="title">Title</option>
          <option value="priceHigh">Price: High</option>
          <option value="priceLow">Price: Low</option>
        </select>

        <button class="admin-ghost" type="button" @click="fetchList(1)">Search</button>
      </div>

      <!-- Bulk -->
      <div class="mt-4 flex flex-wrap items-center justify-between gap-2">
        <div class="text-sm admin-muted">
          Total: <span class="font-bold text-white">{{ total }}</span>
          <span v-if="selectedIds.length" class="ml-2">• Selected: {{ selectedIds.length }}</span>
        </div>

        <div class="flex gap-2" v-if="selectedIds.length">
          <button class="admin-pill" type="button" @click="bulkPublish(true)" :disabled="pending">
            Publish
          </button>
          <button class="admin-pill" type="button" @click="bulkPublish(false)" :disabled="pending">
            Unpublish
          </button>
          <button class="admin-btn-danger" type="button" @click="bulkDelete" :disabled="pending">
            Delete
          </button>
        </div>
      </div>
    </div>

    <!-- List -->
    <div class="admin-box overflow-hidden">
      <div v-if="loading" class="p-4 admin-muted">Loading...</div>

      <div v-else-if="items.length === 0" class="p-6 text-center">
        <div class="text-lg font-extrabold">No products yet</div>
        <div class="admin-muted mt-1">Create your first product in one minute.</div>
        <NuxtLink to="/admin/products/new" class="admin-primary inline-flex mt-4">+ Add first product</NuxtLink>
      </div>

      <div v-else class="admin-table">
        <div class="admin-tr admin-th">
          <div class="flex items-center gap-2">
            <input
              type="checkbox"
              :checked="allChecked"
              @change="toggleAll(($event.target as HTMLInputElement).checked)"
            />
            <span>Product</span>
          </div>
          <div>Price</div>
          <div>Status</div>
          <div class="text-right">Actions</div>
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
              {{ p.isPublished ? 'Published' : 'Draft' }}
            </span>
          </div>

          <div class="flex justify-end gap-2">
            <NuxtLink class="admin-pill" :to="`/admin/products/${p.id}`">Edit</NuxtLink>
            <button class="admin-pill" type="button" @click="quickToggle(p)" :disabled="pending">
              {{ p.isPublished ? 'Unpublish' : 'Publish' }}
            </button>
            <button class="admin-btn-danger" type="button" @click="removeOne(p)" :disabled="pending">
              Delete
            </button>
          </div>
        </div>

        <!-- Pagination -->
        <div class="p-4 flex items-center justify-between">
          <div class="text-sm admin-muted">Page {{ page }} / {{ totalPages }}</div>
          <div class="flex gap-2">
            <button class="admin-pill" type="button" :disabled="page <= 1" @click="go(page - 1)">
              Prev
            </button>
            <button class="admin-pill" type="button" :disabled="page >= totalPages" @click="go(page + 1)">
              Next
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="error" class="admin-error">{{ error }}</div>
    <div v-if="success" class="admin-success">{{ success }}</div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

type Product = {
  id: string
  title: string
  slug: string
  priceUsd: number
  isPublished: boolean
}

// Nuxt auto-import (من app/composables)
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
const pagedItems = computed(() =>
  items.value.slice((page.value - 1) * pageSize.value, page.value * pageSize.value)
)

// selection
const selectedIds = ref<string[]>([])
const allChecked = computed(() => items.value.length > 0 && selectedIds.value.length === items.value.length)

function extractErr(e: any) {
  return e?.data?.message || e?.message || 'Request failed'
}

function toggleOne(id: string) {
  if (selectedIds.value.includes(id)) selectedIds.value = selectedIds.value.filter((x) => x !== id)
  else selectedIds.value.push(id)
}

function toggleAll(v: boolean) {
  selectedIds.value = v ? items.value.map((x) => x.id) : []
}

function go(p: number) {
  page.value = Math.min(Math.max(1, p), totalPages.value)
}

function applyClientFilters(list: Product[]) {
  let out = [...list]

  const qq = q.value.trim().toLowerCase()
  if (qq) {
    out = out.filter(
      (x) =>
        (x.title || '').toLowerCase().includes(qq) || (x.slug || '').toLowerCase().includes(qq)
    )
  }

  if (status.value === 'published') out = out.filter((x) => !!x.isPublished)
  if (status.value === 'draft') out = out.filter((x) => !x.isPublished)

  if (sort.value === 'title') out.sort((a, b) => (a.title || '').localeCompare(b.title || ''))
  if (sort.value === 'oldest') out.sort((a, b) => String(a.id).localeCompare(String(b.id)))
  if (sort.value === 'newest') out.sort((a, b) => String(b.id).localeCompare(String(a.id)))
  if (sort.value === 'priceHigh') out.sort((a, b) => (b.priceUsd || 0) - (a.priceUsd || 0))
  if (sort.value === 'priceLow') out.sort((a, b) => (a.priceUsd || 0) - (b.priceUsd || 0))

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

    items.value = applyClientFilters(
      list.map((x) => ({
        id: String(x.id),
        title: String(x.title || ''),
        slug: String(x.slug || ''),
        priceUsd: Number(x.priceUsd || 0),
        isPublished: !!x.isPublished,
      }))
    )

    page.value = p
    go(p)
  } catch (e: any) {
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
    success.value = 'Updated.'
    await fetchList(page.value)
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function removeOne(p: Product) {
  if (!confirm(`Delete "${p.title}"?`)) return
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    await api.deleteAdminProduct(p.id)
    success.value = 'Deleted.'
    await fetchList(Math.min(page.value, totalPages.value))
  } catch (e: any) {
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
      const p = items.value.find((x) => x.id === id)
      if (!p) continue
      await api.updateAdminProduct(id, { ...p, isPublished: v })
    }
    success.value = 'Bulk updated.'
    await fetchList(page.value)
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function bulkDelete() {
  if (!confirm(`Delete ${selectedIds.value.length} products?`)) return
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    for (const id of selectedIds.value) {
      await api.deleteAdminProduct(id)
    }
    success.value = 'Bulk deleted.'
    await fetchList(Math.min(page.value, totalPages.value))
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

onMounted(() => fetchList(1))
</script>

<style scoped>
.admin-box {
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.06);
  padding: 16px;
}
.admin-muted {
  color: rgba(255, 255, 255, 0.65);
}

.admin-input {
  width: 100%;
  border-radius: 14px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.06);
  padding: 10px 12px;
  color: rgba(255, 255, 255, 0.9);
  outline: none;
}
.admin-input:focus {
  border-color: rgba(99, 102, 241, 0.35);
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.12);
}

.admin-primary {
  padding: 10px 12px;
  border-radius: 14px;
  background: rgba(99, 102, 241, 0.22);
  border: 1px solid rgba(99, 102, 241, 0.35);
  color: white;
  font-weight: 900;
}
.admin-ghost {
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.06);
  color: rgba(255, 255, 255, 0.85);
  font-weight: 800;
}
.admin-pill {
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.06);
  color: rgba(255, 255, 255, 0.9);
  font-weight: 800;
}
.admin-btn-danger {
  padding: 8px 10px;
  border-radius: 14px;
  border: 1px solid rgba(239, 68, 68, 0.35);
  background: rgba(239, 68, 68, 0.14);
  color: rgba(255, 255, 255, 0.95);
  font-weight: 900;
}

.admin-table {
  display: grid;
}
.admin-tr {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 2fr;
  gap: 12px;
  padding: 12px 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}
.admin-th {
  border-top: none;
  background: rgba(0, 0, 0, 0.18);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: rgba(255, 255, 255, 0.65);
}

.badge-on {
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(16, 185, 129, 0.35);
  background: rgba(16, 185, 129, 0.14);
  font-weight: 800;
}
.badge-off {
  padding: 6px 10px;
  border-radius: 999px;
  border: 1px solid rgba(255, 255, 255, 0.12);
  background: rgba(255, 255, 255, 0.06);
  color: rgba(255, 255, 255, 0.75);
  font-weight: 800;
}

.admin-error {
  border-radius: 16px;
  border: 1px solid rgba(239, 68, 68, 0.35);
  background: rgba(239, 68, 68, 0.1);
  padding: 12px 14px;
}
.admin-success {
  border-radius: 16px;
  border: 1px solid rgba(16, 185, 129, 0.35);
  background: rgba(16, 185, 129, 0.1);
  padding: 12px 14px;
}

.thumb {
  width: 46px;
  height: 46px;
  border-radius: 14px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.06);
  flex: 0 0 auto;
  overflow: hidden;
}
.thumb-inner {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
