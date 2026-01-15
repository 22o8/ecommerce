<template>
  <div class="space-y-4">
    <div class="admin-box flex items-center justify-between gap-3">
      <div>
        <div class="text-xl font-extrabold">Edit Service</div>
        <div class="text-sm admin-muted">{{ id }}</div>
      </div>

      <div class="flex gap-2">
        <NuxtLink to="/admin/services" class="admin-ghost">Back</NuxtLink>
        <button class="admin-btn-danger" type="button" @click="remove" :disabled="pending">Delete</button>
      </div>
    </div>

    <div v-if="loading" class="admin-box admin-muted">Loading...</div>
    <div v-else class="admin-box">
      <form class="space-y-4" @submit.prevent="save">
        <div class="grid gap-3 md:grid-cols-2">
          <div>
            <div class="label">Title</div>
            <input v-model="form.title" class="admin-input" />
          </div>
          <div>
            <div class="label">Slug</div>
            <input v-model="form.slug" class="admin-input" />
          </div>
        </div>

        <div>
          <div class="label">Description</div>
          <textarea v-model="form.description" class="admin-input" rows="5" />
        </div>

        <label class="check">
          <input v-model="form.isPublished" type="checkbox" />
          <span>Published</span>
        </label>

        <div class="admin-divider" />

        <div>
          <div class="font-extrabold">Packages</div>
          <div class="mt-3 space-y-3">
            <div v-for="(p, idx) in form.packages" :key="idx" class="sub-box">
              <div class="flex items-center justify-between">
                <div class="font-bold">Package #{{ idx + 1 }}</div>
                <button type="button" class="admin-btn-danger" @click="removePackage(idx)">Remove</button>
              </div>

              <div class="mt-3 grid gap-3 md:grid-cols-2">
                <div>
                  <div class="label">Name</div>
                  <input v-model="p.name" class="admin-input" />
                </div>
                <div>
                  <div class="label">Price USD</div>
                  <input v-model.number="p.priceUsd" type="number" class="admin-input" min="0" />
                </div>
                <div>
                  <div class="label">Delivery Days</div>
                  <input v-model.number="p.deliveryDays" type="number" class="admin-input" min="0" />
                </div>
                <div>
                  <div class="label">Features</div>
                  <input v-model="p.features" class="admin-input" />
                </div>
              </div>
            </div>

            <button type="button" class="admin-ghost" @click="addPackage">+ Add Package</button>
          </div>
        </div>

        <div class="admin-divider" />

        <div>
          <div class="font-extrabold">Requirements</div>
          <div class="mt-3 space-y-3">
            <div v-for="(r, idx) in form.requirements" :key="idx" class="sub-box">
              <div class="flex items-center justify-between">
                <div class="font-bold">Requirement #{{ idx + 1 }}</div>
                <button type="button" class="admin-btn-danger" @click="removeReq(idx)">Remove</button>
              </div>

              <div class="mt-3 grid gap-3 md:grid-cols-3">
                <div class="md:col-span-2">
                  <div class="label">Question</div>
                  <input v-model="r.question" class="admin-input" />
                </div>
                <div>
                  <div class="label">Order</div>
                  <input v-model.number="r.order" type="number" class="admin-input" min="0" />
                </div>
                <div class="md:col-span-3">
                  <label class="check">
                    <input v-model="r.isRequired" type="checkbox" />
                    <span>Required</span>
                  </label>
                </div>
              </div>
            </div>

            <button type="button" class="admin-ghost" @click="addReq">+ Add Requirement</button>
          </div>
        </div>

        <div class="flex gap-2">
          <button class="admin-primary" type="submit" :disabled="pending">
            {{ pending ? 'Saving...' : 'Save' }}
          </button>
        </div>

        <div v-if="error" class="admin-error">{{ error }}</div>
        <div v-if="success" class="admin-success">{{ success }}</div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

const api = useAdminApi()
const route = useRoute()
const id = computed(() => String(route.params.id))

const loading = ref(true)
const pending = ref(false)
const error = ref('')
const success = ref('')

const form = reactive({
  title: '',
  slug: '',
  description: '',
  isPublished: true,
  packages: [] as any[],
  requirements: [] as any[],
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    // swagger: GET /api/admin/services (ماكو get by id)
    // نجيب الكل ونفلتر
    const res: any = await api.get('/admin/services')
    const list = Array.isArray(res) ? res : (res.items || [])
    const found = list.find((x: any) => x.id === id.value)
    if (!found) throw new Error('Service not found')

    form.title = found.title
    form.slug = found.slug
    form.description = found.description
    form.isPublished = !!found.isPublished
    form.packages = Array.isArray(found.packages) ? structuredClone(found.packages) : []
    form.requirements = Array.isArray(found.requirements) ? structuredClone(found.requirements) : []
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Load failed'
  } finally {
    loading.value = false
  }
}

function addPackage() {
  form.packages.push({ name: '', priceUsd: 0, deliveryDays: 0, features: '' })
}
function removePackage(i: number) {
  form.packages.splice(i, 1)
}
function addReq() {
  form.requirements.push({ question: '', isRequired: true, order: form.requirements.length })
}
function removeReq(i: number) {
  form.requirements.splice(i, 1)
}

async function save() {
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    await api.put(`/admin/services/${id.value}`, { ...form })
    success.value = 'Saved.'
    await load()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Save failed'
  } finally {
    pending.value = false
  }
}

async function remove() {
  if (!confirm('Delete this service?')) return
  pending.value = true
  error.value = ''
  try {
    await api.del(`/admin/services/${id.value}`)
    await navigateTo('/admin/services')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Delete failed'
  } finally {
    pending.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.admin-box{ border-radius: 20px; border: 1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); padding: 16px; }
.sub-box{ border-radius: 18px; border: 1px solid rgba(255,255,255,.10); background: rgba(0,0,0,.14); padding: 14px; }
.admin-muted{ color: rgba(255,255,255,.65); }
.label{ font-size: 12px; letter-spacing: .08em; text-transform: uppercase; color: rgba(255,255,255,.65); margin-bottom: 6px; }
.admin-input{ width:100%; border-radius:14px; border:1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); padding:10px 12px; color: rgba(255,255,255,.9); outline:none; }
.admin-primary{ padding:10px 12px; border-radius:14px; background: rgba(99,102,241,.22); border:1px solid rgba(99,102,241,.35); color:white; font-weight:800; }
.admin-ghost{ padding:10px 12px; border-radius:14px; border:1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); color: rgba(255,255,255,.85); font-weight:700; }
.admin-btn-danger{ padding:8px 10px; border-radius:14px; border:1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.14); color: rgba(255,255,255,.95); }
.check{ display:flex; align-items:center; gap:10px; padding:10px 12px; border-radius:14px; border:1px solid rgba(255,255,255,.10); background: rgba(255,255,255,.06); color: rgba(255,255,255,.85); }
.admin-divider{ height:1px; background: rgba(255,255,255,.10); }
.admin-error{ border-radius:16px; border:1px solid rgba(239,68,68,.35); background: rgba(239,68,68,.10); padding:12px 14px; }
.admin-success{ border-radius:16px; border:1px solid rgba(16,185,129,.35); background: rgba(16,185,129,.10); padding:12px 14px; }
</style>
