<template>
  <div class="space-y-4">
    <div class="admin-box">
      <div class="text-xl font-extrabold">New Service</div>
      <div class="text-sm admin-muted">Create a service with packages and requirements</div>
    </div>

    <div class="admin-box">
      <form class="space-y-4" @submit.prevent="create">
        <div class="grid gap-3 md:grid-cols-2">
          <div>
            <div class="label">Title</div>
            <input v-model="form.title" class="admin-input" placeholder="SEO Optimization" />
          </div>
          <div>
            <div class="label">Slug</div>
            <input v-model="form.slug" class="admin-input" placeholder="seo-optimization" />
          </div>
        </div>

        <div>
          <div class="label">Description</div>
          <textarea v-model="form.description" class="admin-input" rows="5" placeholder="Service description..." />
        </div>

        <div class="flex items-center gap-3">
          <label class="check">
            <input v-model="form.isPublished" type="checkbox" />
            <span>Published</span>
          </label>
        </div>

        <div class="admin-divider" />

        <!-- Packages -->
        <div>
          <div class="font-extrabold">Packages</div>
          <div class="text-sm admin-muted mt-1">At least 1 package is recommended</div>

          <div class="mt-3 space-y-3">
            <div v-for="(p, idx) in form.packages" :key="idx" class="sub-box">
              <div class="flex items-center justify-between">
                <div class="font-bold">Package #{{ idx + 1 }}</div>
                <button type="button" class="admin-btn-danger" @click="removePackage(idx)">Remove</button>
              </div>

              <div class="mt-3 grid gap-3 md:grid-cols-2">
                <div>
                  <div class="label">Name</div>
                  <input v-model="p.name" class="admin-input" placeholder="Basic" />
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
                  <input v-model="p.features" class="admin-input" placeholder="Feature list..." />
                </div>
              </div>
            </div>

            <button type="button" class="admin-ghost" @click="addPackage">+ Add Package</button>
          </div>
        </div>

        <div class="admin-divider" />

        <!-- Requirements -->
        <div>
          <div class="font-extrabold">Requirements</div>
          <div class="text-sm admin-muted mt-1">Questions user must answer</div>

          <div class="mt-3 space-y-3">
            <div v-for="(r, idx) in form.requirements" :key="idx" class="sub-box">
              <div class="flex items-center justify-between">
                <div class="font-bold">Requirement #{{ idx + 1 }}</div>
                <button type="button" class="admin-btn-danger" @click="removeReq(idx)">Remove</button>
              </div>

              <div class="mt-3 grid gap-3 md:grid-cols-3">
                <div class="md:col-span-2">
                  <div class="label">Question</div>
                  <input v-model="r.question" class="admin-input" placeholder="What is your website URL?" />
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
            {{ pending ? 'Saving...' : 'Create' }}
          </button>
          <NuxtLink to="/admin/services" class="admin-ghost">Cancel</NuxtLink>
        </div>

        <div v-if="error" class="admin-error">{{ error }}</div>
        <div v-if="success" class="admin-success">{{ success }}</div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

type ServicePackageForm = {
  name: string
  priceUsd: number
  deliveryDays: number
  features: string
}

type ServiceRequirementForm = {
  question: string
  isRequired: boolean
  order: number
}

type ServiceForm = {
  title: string
  slug: string
  description: string
  isPublished: boolean
  packages: ServicePackageForm[]
  requirements: ServiceRequirementForm[]
}

const api = useAdminApi()

const pending = ref(false)
const error = ref('')
const success = ref('')

const form = reactive<ServiceForm>({
  title: '',
  slug: '',
  description: '',
  isPublished: true,
  packages: [{ name: 'Basic', priceUsd: 0, deliveryDays: 1, features: '' }],
  requirements: [],
})

function slugify(v: string) {
  return v.toLowerCase().trim().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)+/g, '')
}

watch(
  () => form.title,
  (v) => {
    if (!form.slug) form.slug = slugify(v || '')
  }
)

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

function extractErr(e: any) {
  return e?.data?.message || e?.message || 'Create failed'
}

async function create() {
  error.value = ''
  success.value = ''

  if (!form.title.trim() || !form.slug.trim()) {
    error.value = 'Title and Slug are required'
    return
  }

  pending.value = true
  try {
    const res: any = await api.post('/admin/services', { ...form })
    success.value = 'Created.'
    const id = res?.id
    if (id) await navigateTo(`/admin/services/${id}`)
    else await navigateTo('/admin/services')
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}
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
