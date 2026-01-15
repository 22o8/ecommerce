<template>
  <div class="space-y-4">
    <div class="admin-box">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="text-xl font-extrabold">Services</div>
          <div class="text-sm admin-muted">Create, publish and manage services</div>
        </div>

        <div class="flex gap-2">
          <button class="admin-primary" type="button" @click="openCreate">+ New Service</button>
        </div>
      </div>

      <div class="mt-4 grid gap-2 md:grid-cols-3">
        <input
          v-model="q"
          class="admin-input"
          placeholder="Search by title or slug..."
          @keydown.enter="fetchList(1)"
        />

        <select v-model="sort" class="admin-input" @change="fetchList(1)">
          <option value="">Default</option>
          <option value="newest">Newest</option>
          <option value="oldest">Oldest</option>
        </select>

        <button class="admin-ghost" type="button" @click="fetchList(1)">Search</button>
      </div>
    </div>

    <div class="admin-box overflow-hidden">
      <div class="admin-table">
        <div class="admin-tr admin-th">
          <div>Title</div>
          <div>Slug</div>
          <div>Status</div>
          <div class="text-right">Actions</div>
        </div>

        <div v-if="pending" class="p-4 admin-muted">Loading...</div>
        <div v-else-if="items.length === 0" class="p-4 admin-muted">No services yet.</div>
        <div v-else>
          <div v-for="s in items" :key="s.id" class="admin-tr">
            <div class="font-semibold">{{ s.title }}</div>
            <div class="admin-muted text-sm">{{ s.slug }}</div>
            <div>
              <span :class="s.isPublished ? 'badge-on' : 'badge-off'">
                {{ s.isPublished ? 'Published' : 'Draft' }}
              </span>
            </div>

            <div class="flex justify-end gap-2">
              <button class="admin-pill" type="button" @click="openEdit(s)">Edit</button>

              <button class="admin-pill" type="button" @click="togglePublish(s)" :disabled="actionLoadingId === s.id">
                {{ s.isPublished ? 'Unpublish' : 'Publish' }}
              </button>

              <button class="admin-btn-danger" type="button" @click="remove(s)" :disabled="actionLoadingId === s.id">
                Delete
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="p-4 flex items-center justify-between">
        <div class="text-sm admin-muted">Total: {{ totalCount }}</div>

        <div class="flex items-center gap-2">
          <button class="admin-pill" type="button" :disabled="page <= 1" @click="fetchList(page - 1)">Prev</button>
          <span class="text-sm admin-muted">Page {{ page }}</span>
          <button class="admin-pill" type="button" :disabled="page >= totalPages" @click="fetchList(page + 1)">Next</button>
        </div>
      </div>
    </div>

    <div v-if="error" class="admin-error">{{ error }}</div>
    <div v-if="success" class="admin-success">{{ success }}</div>

    <!-- Modal -->
    <div v-if="modalOpen" class="modal">
      <div class="modal-card">
        <div class="flex items-start justify-between gap-3">
          <div>
            <div class="text-xl font-extrabold">{{ editing ? 'Edit Service' : 'New Service' }}</div>
            <div class="admin-muted text-sm mt-1">Fill the details then save.</div>
          </div>
          <button class="icon-btn" @click="closeModal">✕</button>
        </div>

        <div class="mt-5 grid grid-cols-1 md:grid-cols-2 gap-3">
          <div>
            <label class="lbl">Title</label>
            <input v-model="form.title" class="inpt" placeholder="Consulting Package" />
          </div>

          <div>
            <label class="lbl">Slug</label>
            <input v-model="form.slug" class="inpt" placeholder="consulting-package" />
          </div>

          <div class="md:col-span-2">
            <label class="lbl">Description</label>
            <textarea v-model="form.description" class="inpt" rows="4" placeholder="Describe the service..." />
          </div>

          <div class="flex items-center gap-3 pt-2">
            <input id="pub" type="checkbox" v-model="form.isPublished" class="h-4 w-4" />
            <label for="pub" class="text-sm font-semibold">Published</label>
          </div>
        </div>

        <div v-if="modalError" class="mt-4 alert-danger">{{ modalError }}</div>

        <div class="mt-6 flex items-center justify-end gap-2">
          <button class="btn-soft" @click="closeModal">Cancel</button>
          <button class="btn-primary" :disabled="saving" @click="save">
            {{ saving ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

type Service = {
  id: string
  title: string
  slug: string
  description: string
  isPublished: boolean
  createdAt?: string
}

type Paged<T> = {
  page: number
  pageSize: number
  totalCount: number
  items: T[]
}

const api = useAdminApi()

const q = ref('')
const sort = ref('')
const page = ref(1)
const pageSize = ref(10)

const items = ref<Service[]>([])
const totalCount = ref(0)

const pending = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')
const actionLoadingId = ref<string | null>(null)

const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

const modalOpen = ref(false)
const editing = ref<Service | null>(null)
const modalError = ref('')

const form = reactive({
  title: '',
  slug: '',
  description: '',
  isPublished: true,
})

function closeModal() {
  modalOpen.value = false
  modalError.value = ''
}

function openCreate() {
  editing.value = null
  form.title = ''
  form.slug = ''
  form.description = ''
  form.isPublished = true
  modalOpen.value = true
}

function openEdit(s: Service) {
  editing.value = s
  form.title = s.title
  form.slug = s.slug
  form.description = s.description
  form.isPublished = s.isPublished
  modalOpen.value = true
}

function extractErr(e: any) {
  return e?.data?.message || e?.message || 'Request failed'
}

async function fetchList(p = 1) {
  pending.value = true
  error.value = ''
  success.value = ''

  try {
    // ✅ لازم تستخدم الدوال اللي رجعناها بـ useAdminApi
    const res: any = await api.listServices<any>({
      Page: p,
      PageSize: pageSize.value,
      Q: q.value || undefined,
      Sort: sort.value || undefined,
    })

    // بعض الباكات ترجع Array فقط، وبعضها ترجع Paged
    if (Array.isArray(res)) {
      page.value = 1
      totalCount.value = res.length
      items.value = res as Service[]
    } else {
      page.value = Number(res?.page || p)
      totalCount.value = Number(res?.totalCount || (res?.items?.length ?? 0))
      items.value = (res?.items || []) as Service[]
    }
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function save() {
  modalError.value = ''
  if (!form.title.trim() || !form.slug.trim()) {
    modalError.value = 'Title and Slug are required'
    return
  }

  saving.value = true
  try {
    if (!editing.value) {
      await api.createService({ ...form })
      success.value = 'Created.'
    } else {
      await api.updateService(editing.value.id, { ...form })
      success.value = 'Updated.'
    }

    closeModal()
    await fetchList(page.value)
  } catch (e: any) {
    modalError.value = extractErr(e)
  } finally {
    saving.value = false
  }
}

async function togglePublish(s: Service) {
  actionLoadingId.value = s.id
  error.value = ''
  success.value = ''
  try {
    await api.updateService(s.id, {
      title: s.title,
      slug: s.slug,
      description: s.description,
      isPublished: !s.isPublished,
    })
    success.value = 'Updated.'
    await fetchList(page.value)
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    actionLoadingId.value = null
  }
}

async function remove(s: Service) {
  if (!confirm(`Delete "${s.title}"?`)) return
  actionLoadingId.value = s.id
  error.value = ''
  success.value = ''

  try {
    await api.deleteService(s.id)
    success.value = 'Deleted.'
    await fetchList(Math.min(page.value, totalPages.value))
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    actionLoadingId.value = null
  }
}

onMounted(() => fetchList(1))
</script>

<style scoped>
/* نفس الستايل اللي عندك، بدون تغيير */
.admin-box{border-radius:20px;border:1px solid rgba(255,255,255,.10);background:rgba(255,255,255,.06);padding:16px;}
.admin-muted{color:rgba(255,255,255,.65);}
.admin-input{width:100%;border-radius:14px;border:1px solid rgba(255,255,255,.10);background:rgba(255,255,255,.06);padding:10px 12px;color:rgba(255,255,255,.9);outline:none;}
.admin-input:focus{border-color:rgba(99,102,241,.35);box-shadow:0 0 0 3px rgba(99,102,241,.12);}
.admin-primary{padding:10px 12px;border-radius:14px;background:rgba(99,102,241,.22);border:1px solid rgba(99,102,241,.35);color:white;font-weight:800;}
.admin-ghost{padding:10px 12px;border-radius:14px;border:1px solid rgba(255,255,255,.10);background:rgba(255,255,255,.06);color:rgba(255,255,255,.85);font-weight:700;}
.admin-pill{padding:8px 10px;border-radius:14px;border:1px solid rgba(255,255,255,.10);background:rgba(255,255,255,.06);color:rgba(255,255,255,.9);}
.admin-btn-danger{padding:8px 10px;border-radius:14px;border:1px solid rgba(239,68,68,.35);background:rgba(239,68,68,.14);color:rgba(255,255,255,.95);}
.admin-table{display:grid;}
.admin-tr{display:grid;grid-template-columns:2fr 2fr 1fr 2fr;gap:12px;padding:12px 16px;border-top:1px solid rgba(255,255,255,.08);}
.admin-th{border-top:none;background:rgba(0,0,0,.18);font-size:12px;text-transform:uppercase;letter-spacing:.08em;color:rgba(255,255,255,.65);}
.badge-on{padding:6px 10px;border-radius:999px;border:1px solid rgba(16,185,129,.35);background:rgba(16,185,129,.14);}
.badge-off{padding:6px 10px;border-radius:999px;border:1px solid rgba(255,255,255,.12);background:rgba(255,255,255,.06);color:rgba(255,255,255,.7);}
.admin-error{border-radius:16px;border:1px solid rgba(239,68,68,.35);background:rgba(239,68,68,.10);padding:12px 14px;}
.admin-success{border-radius:16px;border:1px solid rgba(16,185,129,.35);background:rgba(16,185,129,.10);padding:12px 14px;}
.modal{position:fixed;inset:0;background:rgba(2,6,23,.45);display:flex;align-items:center;justify-content:center;padding:18px;z-index:80;}
.modal-card{width:min(720px,100%);border-radius:24px;border:1px solid rgba(148,163,184,.18);background:rgba(15,18,28,.72);backdrop-filter:blur(14px);padding:16px;color:white;}
.inpt{width:100%;padding:12px 14px;border-radius:16px;border:1px solid rgba(148,163,184,.20);outline:none;background:rgba(255,255,255,.06);color:rgba(226,232,240,.92);}
.lbl{display:block;font-size:12px;letter-spacing:.08em;text-transform:uppercase;color:rgba(148,163,184,.90);margin-bottom:6px;}
.btn-soft{padding:10px 12px;border-radius:14px;border:1px solid rgba(255,255,255,.10);background:rgba(255,255,255,.06);color:rgba(255,255,255,.85);font-weight:800;}
.btn-primary{padding:10px 12px;border-radius:14px;background:rgba(99,102,241,.35);border:1px solid rgba(99,102,241,.45);color:white;font-weight:900;}
.alert-danger{padding:12px 14px;border-radius:16px;border:1px solid rgba(239,68,68,.22);background:rgba(239,68,68,.12);color:rgba(239,68,68,.95);font-weight:700;}
.icon-btn{width:40px;height:40px;border-radius:14px;border:1px solid rgba(255,255,255,.10);background:rgba(255,255,255,.06);}
</style>
