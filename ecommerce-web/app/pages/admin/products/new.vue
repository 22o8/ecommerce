<!-- app/pages/admin/products/new.vue -->
<template>
  <div class="space-y-4">
    <div class="admin-box">
      <div class="text-xl font-extrabold">New Product</div>
      <div class="text-sm admin-muted">Create a new product in seconds</div>
    </div>

    <div class="admin-box">
      <form class="space-y-3" @submit.prevent="create">
        <div class="grid gap-3 md:grid-cols-2">
          <div>
            <div class="label">Title</div>
            <input v-model="form.title" class="admin-input" placeholder="Website Development" />
          </div>

          <div>
            <div class="label">Slug</div>
            <input v-model="form.slug" class="admin-input" placeholder="website-development" />
          </div>
        </div>

        <div class="grid gap-3 md:grid-cols-2">

          <div>
            <div class="label">Brand</div>
            <select v-model="form.brand" class="admin-input" required>
              <option v-for="b in brands" :key="b.key" :value="b.key">{{ b.label }}</option>
            </select>
          </div>

        </div>

        <div>
          <div class="label">Description</div>
          <textarea v-model="form.description" class="admin-input" rows="5" placeholder="Short description..." />
        </div>

        <div class="grid gap-3 md:grid-cols-2">
          <div>
            <div class="label">Price (USD)</div>
            <input v-model.number="form.priceUsd" type="number" class="admin-input" />
          </div>

          <div class="flex items-end gap-2">
            <label class="check">
              <input v-model="form.isPublished" type="checkbox" />
              <span>Published</span>
            </label>
          </div>
        </div>

        <div class="flex gap-2">
          <button class="admin-primary" type="submit" :disabled="pending">
            {{ pending ? 'Saving...' : 'Create' }}
          </button>
          <NuxtLink to="/admin/products" class="admin-ghost">Cancel</NuxtLink>
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

const brands = ref<{ key: string; label: string }[]>([])

await useAsyncData('admin-brands', async () => {
  try {
    const res: any = await api.get('/Brands')
    brands.value = (res?.items || []).filter(Boolean)
    if (!form.brand && brands.value.length) form.brand = brands.value[0].key
  } catch (e) {
    brands.value = []
  }
  return true
})

const pending = ref(false)
const error = ref('')
const success = ref('')

const form = reactive({
  title: '',
  slug: '',
  description: '',
  priceUsd: 0,
  brand: '',
  isPublished: true,
})

function slugify(v: string) {
  return v
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)+/g, '')
}

watch(
  () => form.title,
  (v) => {
    if (!form.slug) form.slug = slugify(v || '')
  }
)

async function create() {
  error.value = ''
  success.value = ''
  if (!form.title.trim() || !form.slug.trim()) {
    error.value = 'Title and Slug are required'
    return
  }

  pending.value = true
  try {
    const res: any = await api.createProduct({ ...form })
    success.value = 'Created.'
    await navigateTo(`/admin/products/${res.id}`)
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Create failed'
  } finally {
    pending.value = false
  }
}
</script>

<style scoped>
.admin-box{
  border-radius: 20px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  padding: 16px;
}
.admin-muted{ color: rgba(255,255,255,.65); }

.label{
  font-size: 12px;
  letter-spacing: .08em;
  text-transform: uppercase;
  color: rgba(255,255,255,.65);
  margin-bottom: 6px;
}

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
  font-weight: 800;
}
.admin-ghost{
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.85);
  font-weight: 700;
}

.check{
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.85);
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
</style>
