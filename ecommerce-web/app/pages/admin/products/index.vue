<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'

const { t } = useI18n()
const api = useApi()

type Product = {
  id: string
  title: string
  slug: string
  description: string
  priceUsd: number
  images: string[]
  whatsappNumber: string | null
}

const loading = ref(false)
const error = ref<string | null>(null)
const products = ref<Product[]>([])

const showNew = ref(false)
const form = ref({
  title: '',
  slug: '',
  description: '',
  priceUsd: 0,
  imagesCsv: '',
  whatsappNumber: ''
})

const safeSlug = (s: string) =>
  s
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9\u0600-\u06FF\s-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')

watch(
  () => form.value.title,
  (v) => {
    if (!form.value.slug) form.value.slug = safeSlug(v)
  }
)

async function load() {
  loading.value = true
  error.value = null
  try {
    products.value = await api.get('/api/bff/admin/products')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed'
  } finally {
    loading.value = false
  }
}

async function create() {
  loading.value = true
  error.value = null
  try {
    const payload = {
      title: form.value.title,
      slug: form.value.slug || safeSlug(form.value.title),
      description: form.value.description,
      priceUsd: Number(form.value.priceUsd || 0),
      imagesCsv: form.value.imagesCsv,
      whatsappNumber: form.value.whatsappNumber || null
    }
    await api.post('/api/bff/admin/products', payload)
    showNew.value = false
    form.value = { title: '', slug: '', description: '', priceUsd: 0, imagesCsv: '', whatsappNumber: '' }
    await load()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed'
  } finally {
    loading.value = false
  }
}

async function remove(id: string) {
  if (!confirm(t('admin.confirmDelete'))) return
  loading.value = true
  error.value = null
  try {
    await api.del(`/api/bff/admin/products/${id}`)
    await load()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed'
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3">
      <h1 class="text-xl font-bold">{{ t('admin.products') }}</h1>

      <div class="flex items-center gap-2">
        <button class="btn-secondary" :disabled="loading" @click="load">{{ t('admin.refresh') }}</button>
        <button class="btn-primary" @click="showNew = true">{{ t('admin.newProduct') }}</button>
      </div>
    </div>

    <div v-if="error" class="rounded-xl border border-red-500/30 bg-red-500/10 p-3 text-sm text-red-200">
      {{ error }}
    </div>

    <div class="grid gap-3">
      <div
        v-for="p in products"
        :key="p.id"
        class="rounded-2xl border border-white/10 bg-white/5 p-4 flex items-center gap-4"
      >
        <div class="h-14 w-14 rounded-xl bg-white/10 overflow-hidden flex items-center justify-center">
          <img v-if="p.images?.[0]" :src="p.images[0]" class="h-full w-full object-cover" />
          <span v-else class="text-xs opacity-60">IMG</span>
        </div>

        <div class="flex-1 min-w-0">
          <div class="font-semibold truncate">{{ p.title }}</div>
          <div class="text-xs opacity-70 truncate">/{{ p.slug }} • {{ p.priceUsd }} US$</div>
        </div>

        <button class="btn-danger" :disabled="loading" @click="remove(p.id)">
          {{ t('admin.delete') }}
        </button>
      </div>

      <div v-if="!loading && products.length === 0" class="opacity-70 text-sm">
        لا توجد منتجات بعد.
      </div>
    </div>

    <!-- Modal -->
    <div v-if="showNew" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/60" @click="showNew = false"></div>
      <div class="relative w-full max-w-xl rounded-3xl border border-white/10 bg-zinc-950 p-5">
        <div class="flex items-center justify-between gap-3 mb-4">
          <div class="text-lg font-bold">{{ t('admin.newProduct') }}</div>
          <button class="btn-secondary" @click="showNew = false">✕</button>
        </div>

        <div class="grid gap-3">
          <input v-model="form.title" class="input" placeholder="عنوان المنتج" />
          <input v-model="form.slug" class="input" placeholder="slug (اختياري)" />
          <textarea v-model="form.description" class="input min-h-[110px]" placeholder="وصف المنتج"></textarea>
          <input v-model.number="form.priceUsd" class="input" type="number" step="0.01" placeholder="السعر بالدولار" />
          <input v-model="form.imagesCsv" class="input" placeholder="روابط الصور (افصلها بفاصلة ,)" />
          <input v-model="form.whatsappNumber" class="input" placeholder="رقم واتساب للمنتج (اختياري)" />
        </div>

        <div class="mt-5 flex items-center justify-end gap-2">
          <button class="btn-secondary" @click="showNew = false">إلغاء</button>
          <button class="btn-primary" :disabled="loading" @click="create">حفظ</button>
        </div>
      </div>
    </div>
  </div>
</template>
