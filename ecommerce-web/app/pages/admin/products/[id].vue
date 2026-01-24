<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="admin-box flex items-center justify-between gap-3">
      <div>
        <div class="text-xl font-extrabold">Edit Product</div>
        <div class="text-sm admin-muted">{{ id }}</div>
      </div>

      <div class="flex gap-2">
        <NuxtLink to="/admin/products" class="admin-ghost">Back</NuxtLink>
        <button class="admin-btn-danger" type="button" @click="remove" :disabled="pending">
          Delete
        </button>
      </div>
    </div>

    <!-- Wizard Tabs -->
    <div class="admin-box">
      <div class="tabs">
        <button class="tab" :class="step===1?'tab-on':''" @click="step=1">1) Basics</button>
        <button class="tab" :class="step===2?'tab-on':''" @click="step=2">2) Pricing & Publish</button>
        <button class="tab" :class="step===3?'tab-on':''" @click="step=3">3) Gallery</button>
      </div>

      <div v-if="loading" class="admin-muted mt-4">Loading...</div>
      <div v-else class="mt-4">
        <!-- Step 1 -->
        <div v-if="step===1" class="space-y-3">
          <div class="grid gap-3 md:grid-cols-2">
            <div>
              <div class="label">Title</div>
              <input v-model="form.title" class="admin-input" @input="dirty=true" />
            </div>

            <div>
              <div class="label">Slug</div>
              <div class="flex gap-2">
                <input v-model="form.slug" class="admin-input" @input="dirty=true" />
                <button class="admin-pill" type="button" @click="autoSlug" :disabled="pending">Auto</button>
              </div>
            </div>
          </div>

          <div>
            <div class="label">Description</div>
            <textarea v-model="form.description" class="admin-input" rows="6" @input="dirty=true" />
          </div>

          <div class="flex justify-end gap-2">
            <button class="admin-primary" type="button" @click="step=2">Next →</button>
          </div>
        </div>

        <!-- Step 2 -->
        <div v-else-if="step===2" class="space-y-3">
          <div class="grid gap-3 md:grid-cols-3">
            <div>
              <div class="label">Price (USD)</div>
              <input v-model.number="form.priceUsd" type="number" class="admin-input" min="0" @input="dirty=true" />
            </div>

            <div>
              <div class="label">Rating Avg</div>
              <input v-model.number="form.ratingAvg" type="number" step="0.1" min="0" max="5" class="admin-input" @input="dirty=true" />
            </div>

            <div>
              <div class="label">Rating Count</div>
              <input v-model.number="form.ratingCount" type="number" min="0" class="admin-input" @input="dirty=true" />
            </div>
          </div>

          <label class="check">
            <input v-model="form.isPublished" type="checkbox" @change="dirty=true" />
            <span>Published</span>
          </label>

          <div class="flex justify-between gap-2">
            <button class="admin-ghost" type="button" @click="step=1">← Back</button>
            <button class="admin-primary" type="button" @click="step=3">Next →</button>
          </div>
        </div>

        <!-- Step 3 -->
        <div v-else class="space-y-3">
          <div class="flex items-center justify-between gap-3">
            <div>
              <div class="text-lg font-extrabold">Product Gallery</div>
              <div class="text-sm admin-muted">Drop, upload, reorder, delete</div>
            </div>
          </div>

          <!-- Dropzone -->
          <div
            class="dropzone"
            @dragover.prevent
            @drop.prevent="onDrop"
          >
            <div class="font-extrabold">اسحب الصور هنا</div>
            <div class="admin-muted text-sm mt-1">أو اختر صور من جهازك</div>

            <label class="admin-primary inline-flex mt-3 cursor-pointer">
              + Choose images
              <input class="hidden" type="file" multiple accept="image/*" @change="onPickFiles" />
            </label>
          </div>

          <!-- Upload queue -->
          <div v-if="queue.length" class="space-y-2">
            <div class="text-sm font-extrabold">Upload Queue</div>

            <div v-for="(q, i) in queue" :key="i" class="queue-item">
              <div class="flex items-center gap-3">
                <img :src="q.preview" class="queue-thumb" />
                <div class="min-w-0">
                  <div class="font-bold truncate">{{ q.file.name }}</div>
                  <div class="text-xs admin-muted">{{ q.status }}</div>
                </div>
              </div>
              <button class="mini-danger" type="button" @click="removeQueue(i)" :disabled="pending">Remove</button>
            </div>

            <div class="flex justify-end">
              <button class="admin-primary" type="button" @click="uploadQueue" :disabled="pending">
                {{ pending ? 'Uploading...' : 'Upload now' }}
              </button>
            </div>
          </div>

          <div v-if="galleryError" class="admin-error">{{ galleryError }}</div>
          <div v-if="gallerySuccess" class="admin-success">{{ gallerySuccess }}</div>

          <div v-if="imagesLoading" class="admin-muted">Loading images...</div>

          <!-- Images -->
          <div v-else>
            <div v-if="images.length === 0" class="admin-muted">No images yet.</div>
            <div v-else class="grid gap-3 md:grid-cols-4">
              <div v-for="(img, idx) in images" :key="img.id" class="img-card">
                <img class="img" :src="normalizeImgUrl(img.url)" />

                <div class="mt-2 flex items-center justify-between gap-2">
                  <button class="mini" type="button" @click="move(idx, -1)" :disabled="idx===0 || pending">↑</button>
                  <button class="mini" type="button" @click="move(idx, 1)" :disabled="idx===images.length-1 || pending">↓</button>

                  <button class="mini-danger" type="button" @click="deleteImage(img.id)" :disabled="pending">Delete</button>
                </div>
              </div>
            </div>

            <div v-if="images.length" class="mt-4 flex justify-end">
              <button class="admin-primary" type="button" @click="saveOrder" :disabled="pending">
                {{ pending ? 'Saving...' : 'Save Order' }}
              </button>
            </div>
          </div>

          <div class="flex justify-between gap-2">
            <button class="admin-ghost" type="button" @click="step=2">← Back</button>
          </div>
        </div>

        <div v-if="error" class="admin-error mt-3">{{ error }}</div>
        <div v-if="success" class="admin-success mt-3">{{ success }}</div>
      </div>
    </div>

    <!-- Sticky Save Bar -->
    <div class="stickybar">
      <div class="sticky-inner">
        <div class="text-sm">
          <span class="font-extrabold">Status:</span>
          <span class="ml-2" :class="dirty ? 'text-yellow-300' : 'text-green-300'">
            {{ dirty ? 'Unsaved changes' : 'Saved' }}
          </span>
        </div>

        <div class="flex gap-2">
          <button class="admin-ghost" type="button" @click="load" :disabled="pending">Reset</button>
          <button class="admin-primary" type="button" @click="save" :disabled="pending || loading">
            {{ pending ? 'Saving...' : 'Save' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

type ProductImage = { id: string; url: string }
type Product = {
  id: string
  title: string
  slug: string
  description: string
  priceUsd: number
  isPublished: boolean
  ratingAvg?: number
  ratingCount?: number
}

const api = useAdminApi()
const coreApi = useApi()
const route = useRoute()
const id = computed(() => String(route.params.id))

const step = ref<1|2|3>(1)

const loading = ref(true)
const pending = ref(false)
const error = ref('')
const success = ref('')
const dirty = ref(false)

const model = ref<Product | null>(null)
const form = reactive({
  title: '',
  slug: '',
  description: '',
  priceUsd: 0,
  isPublished: true,
  ratingAvg: 0,
  ratingCount: 0,
})

// gallery
const imagesLoading = ref(true)
const images = ref<ProductImage[]>([])
const galleryError = ref('')
const gallerySuccess = ref('')

// upload queue
const queue = ref<{ file: File; preview: string; status: string }[]>([])

function extractErr(e: any) {
  return e?.data?.message || e?.message || 'Request failed'
}

function slugify(v: string) {
  return (v || '')
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)+/g, '')
}

function autoSlug() {
  form.slug = slugify(form.title)
  dirty.value = true
}

function normalizeImgUrl(url: string) {
  // نخلي كل صور uploads تمر عبر proxy الداخلي حتى تشتغل على localhost:3000
  // وتتفادى مشاكل SSL / CORS
  return coreApi.buildAssetUrl(url)
}

async function load() {
  loading.value = true
  error.value = ''
  success.value = ''
  try {
    // swagger عندك list فقط -> نفلتر بالـ id
    const res: any[] = await api.listAdminProducts<any[]>()
    const found = (Array.isArray(res) ? res : []).find(x => String(x.id) === id.value)
    if (!found) throw new Error('Product not found')

    model.value = {
      id: String(found.id),
      title: String(found.title || ''),
      slug: String(found.slug || ''),
      description: String(found.description || ''),
      priceUsd: Number(found.priceUsd || 0),
      isPublished: !!found.isPublished,
      ratingAvg: Number(found.ratingAvg || 0),
      ratingCount: Number(found.ratingCount || 0),
    }

    form.title = model.value.title
    form.slug = model.value.slug
    form.description = model.value.description
    form.priceUsd = model.value.priceUsd
    form.isPublished = model.value.isPublished
    form.ratingAvg = model.value.ratingAvg || 0
    form.ratingCount = model.value.ratingCount || 0

    dirty.value = false
    await loadImages()
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

async function loadImages() {
  imagesLoading.value = true
  galleryError.value = ''
  try {
    const res:any = await api.getProductImages(id.value)
    // Backend returns: { items: [...] }
    const list = Array.isArray(res) ? res : (res?.items || [])
    images.value = list.map((x:any) => ({ id: String(x.id), url: String(x.url) }))
  } catch (e:any) {
    galleryError.value = extractErr(e)
  } finally {
    imagesLoading.value = false
  }
}

async function save() {
  pending.value = true
  error.value = ''
  success.value = ''
  try {
    await api.updateAdminProduct(id.value, { ...form })
    success.value = 'Saved.'
    dirty.value = false
    await load()
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function remove() {
  if (!confirm('Delete this product?')) return
  pending.value = true
  error.value = ''
  try {
    await api.deleteAdminProduct(id.value)
    await navigateTo('/admin/products')
  } catch (e:any) {
    error.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

function pushToQueue(files: File[]) {
  for (const f of files) {
    if (!f.type.startsWith('image/')) continue
    queue.value.push({ file: f, preview: URL.createObjectURL(f), status: 'Ready' })
  }
}

function onPickFiles(ev: Event) {
  const input = ev.target as HTMLInputElement
  const files = input.files ? Array.from(input.files) : []
  input.value = ''
  pushToQueue(files)
}

function onDrop(ev: DragEvent) {
  const files = ev.dataTransfer?.files ? Array.from(ev.dataTransfer.files) : []
  pushToQueue(files)
}

function removeQueue(i: number) {
  const it = queue.value[i]
  if (it?.preview) URL.revokeObjectURL(it.preview)
  queue.value.splice(i, 1)
}

async function uploadQueue() {
  if (!queue.value.length) return
  pending.value = true
  galleryError.value = ''
  gallerySuccess.value = ''
  try {
    for (const it of queue.value) {
      it.status = 'Uploading...'
      await api.uploadProductImage(id.value, it.file, '') // alt اختياري
      it.status = 'Done'
    }
    gallerySuccess.value = 'Uploaded.'
    // نظّف queue
    for (const it of queue.value) if (it.preview) URL.revokeObjectURL(it.preview)
    queue.value = []
    await loadImages()
  } catch (e:any) {
    galleryError.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

function move(index: number, dir: -1 | 1) {
  const arr = images.value.slice()
  const to = index + dir
  if (to < 0 || to >= arr.length) return
  ;[arr[index], arr[to]] = [arr[to], arr[index]]
  images.value = arr
}

async function saveOrder() {
  pending.value = true
  galleryError.value = ''
  gallerySuccess.value = ''
  try {
    await api.reorderProductImages(id.value, images.value.map(x => x.id))
    gallerySuccess.value = 'Order saved.'
    await loadImages()
  } catch (e:any) {
    galleryError.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

async function deleteImage(imageId: string) {
  if (!confirm('Delete this image?')) return
  pending.value = true
  galleryError.value = ''
  gallerySuccess.value = ''
  try {
    await api.deleteProductImage(id.value, imageId)
    gallerySuccess.value = 'Deleted.'
    await loadImages()
  } catch (e:any) {
    galleryError.value = extractErr(e)
  } finally {
    pending.value = false
  }
}

onMounted(load)
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
  padding: 10px 12px;
  border-radius: 14px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.14);
  color: rgba(255,255,255,.95);
  font-weight: 900;
}
.check{
  display:flex;
  align-items:center;
  gap:10px;
  padding:10px 12px;
  border-radius:14px;
  border:1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
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

.tabs{
  display:flex;
  gap:10px;
  flex-wrap:wrap;
}
.tab{
  padding:10px 12px;
  border-radius:14px;
  border:1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  font-weight:900;
  color: rgba(255,255,255,.85);
}
.tab-on{
  border-color: rgba(99,102,241,.45);
  background: rgba(99,102,241,.20);
  color: white;
}

.dropzone{
  border-radius: 20px;
  border: 1px dashed rgba(255,255,255,.18);
  background: rgba(0,0,0,.12);
  padding: 18px;
  text-align:center;
}

.queue-item{
  display:flex;
  align-items:center;
  justify-content:space-between;
  gap:12px;
  padding: 12px;
  border-radius: 18px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(0,0,0,.12);
}
.queue-thumb{
  width: 48px;
  height: 48px;
  border-radius: 14px;
  object-fit: cover;
  border: 1px solid rgba(255,255,255,.10);
}

.img-card{
  border-radius: 18px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(0,0,0,.14);
  padding: 12px;
}
.img{
  width: 100%;
  height: 180px;
  object-fit: cover;
  border-radius: 14px;
  border: 1px solid rgba(255,255,255,.08);
}
.mini{
  padding: 6px 10px;
  border-radius: 12px;
  border: 1px solid rgba(255,255,255,.10);
  background: rgba(255,255,255,.06);
  color: rgba(255,255,255,.85);
  font-weight: 900;
}
.mini-danger{
  padding: 6px 10px;
  border-radius: 12px;
  border: 1px solid rgba(239,68,68,.35);
  background: rgba(239,68,68,.14);
  color: rgba(255,255,255,.95);
  font-weight: 900;
}

.stickybar{
  position: sticky;
  bottom: 12px;
  z-index: 60;
}
.sticky-inner{
  border-radius: 18px;
  border: 1px solid rgba(148,163,184,.18);
  background: rgba(15,18,28,.72);
  backdrop-filter: blur(14px);
  padding: 12px 14px;
  display:flex;
  align-items:center;
  justify-content:space-between;
  gap:12px;
}
</style>
