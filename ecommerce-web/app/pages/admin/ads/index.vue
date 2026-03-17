<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

import UiButton from '~/components/ui/UiButton.vue'
import UiCard from '~/components/ui/UiCard.vue'
import UiCardHeader from '~/components/ui/UiCardHeader.vue'
import UiCardTitle from '~/components/ui/UiCardTitle.vue'
import UiCardContent from '~/components/ui/UiCardContent.vue'
import UiInput from '~/components/ui/UiInput.vue'

const { t } = useI18n()
const toast = useToast()
const api = useApi()

function emitAdsChanged() {
  if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
}

const loading = ref(true)
const items = ref<any[]>([])
const uploading = ref(false)

const placements = [
  { value: 'home_top_slider', label: 'سلايدر أعلى الرئيسية' },
  { value: 'home_top', label: 'بانر أعلى الرئيسية' },
  { value: 'popup', label: 'إعلان منبثق' },
  { value: 'product_page', label: 'داخل صفحة المنتج' },
]

const form = reactive({
  type: 'slider',
  placement: 'home_top_slider',
  title: '',
  subtitle: '',
  imageUrl: '',
  imageUrls: [] as string[],
  linkUrl: '',
  productId: '',
  sortOrder: 0,
  isEnabled: true,
})

const previewImages = computed(() => {
  const arr = Array.isArray(form.imageUrls) ? form.imageUrls.filter(Boolean) : []
  if (arr.length) return arr
  return form.imageUrl ? [form.imageUrl] : []
})

async function load() {
  loading.value = true
  try {
    const res: any = await $fetch('/api/bff/admin/ads', { timeout: 8000, query: { _ts: Date.now() }, headers: { 'cache-control': 'no-cache, no-store, must-revalidate', pragma: 'no-cache' } })
    items.value = Array.isArray(res) ? res : []
  } catch {
    items.value = []
  } finally {
    loading.value = false
  }
}

function resetForm() {
  Object.assign(form, { title: '', subtitle: '', imageUrl: '', imageUrls: [], linkUrl: '', productId: '', sortOrder: 0, isEnabled: true, type: 'slider', placement: 'home_top_slider' })
}

async function create() {
  try {
    await $fetch('/api/bff/admin/ads', {
      method: 'POST',
      body: {
        type: form.type,
        placement: form.placement,
        title: form.title,
        subtitle: form.subtitle || null,
        imageUrl: form.imageUrl,
        imageUrls: form.imageUrls,
        linkUrl: form.linkUrl || null,
        productId: form.productId ? form.productId : null,
        sortOrder: Number(form.sortOrder || 0),
        isEnabled: Boolean(form.isEnabled),
        startAt: null,
        endAt: null,
      },
    })
    toast.success('تم حفظ الإعلان')
    resetForm()
    await load()
    emitAdsChanged()
  } catch {
    toast.error('حصل خطأ أثناء الحفظ')
  }
}

async function remove(id: string) {
  if (!confirm('حذف الإعلان؟')) return
  try {
    await $fetch(`/api/bff/admin/ads/${id}`, { method: 'DELETE' })
    toast.success('تم الحذف')
    await load()
    emitAdsChanged()
  } catch {
    toast.error('تعذر الحذف')
  }
}

async function removeAll() {
  if (!confirm('حذف كل الإعلانات الحالية؟')) return
  try {
    await $fetch('/api/bff/admin/ads', { method: 'DELETE' })
    toast.success('تم حذف جميع الإعلانات')
    await load()
    emitAdsChanged()
  } catch {
    toast.error('تعذر حذف الكل')
  }
}

async function toggleEnabled(ad: any) {
  try {
    await $fetch(`/api/bff/admin/ads/${ad.id}`, {
      method: 'PUT',
      body: {
        type: ad.type,
        placement: ad.placement,
        title: ad.title,
        subtitle: ad.subtitle || null,
        imageUrl: ad.imageUrl,
        imageUrls: ad.imageUrls || (ad.imageUrl ? [ad.imageUrl] : []),
        linkUrl: ad.linkUrl || null,
        productId: ad.productId || null,
        sortOrder: Number(ad.sortOrder || 0),
        isEnabled: !ad.isEnabled,
        startAt: ad.startAt || null,
        endAt: ad.endAt || null,
      },
    })
    await load()
    emitAdsChanged()
  } catch {
    toast.error('تعذر تحديث الإعلان')
  }
}

async function onPickFile(e: Event) {
  const files = Array.from((e.target as HTMLInputElement)?.files || [])
  if (!files.length) return
  uploading.value = true
  try {
    const uploaded: string[] = []
    for (const file of files) {
      const fd = new FormData()
      fd.append('file', file)
      const res: any = await $fetch('/api/bff/admin/ads/upload', { method: 'POST', body: fd })
      const url = res?.url?.url || res?.url || res?.imageUrl || ''
      if (typeof url === 'string' && url) uploaded.push(url)
    }
    if (form.type === 'slider') {
      form.imageUrls = [...form.imageUrls, ...uploaded]
      form.imageUrl = form.imageUrls[0] || ''
    } else {
      form.imageUrl = uploaded[0] || ''
      form.imageUrls = uploaded.slice(0, 1)
    }
    if (uploaded.length) toast.success('تم رفع الصور')
  } catch {
    toast.error('تعذر رفع الصور')
  } finally {
    uploading.value = false
    ;(e.target as HTMLInputElement).value = ''
  }
}

function removePreview(idx: number) {
  form.imageUrls.splice(idx, 1)
  form.imageUrl = form.imageUrls[0] || ''
}

watch(() => form.type, (v) => {
  if (v === 'slider') {
    form.placement = 'home_top_slider'
    if (form.imageUrl && !form.imageUrls.length) form.imageUrls = [form.imageUrl]
  } else {
    form.imageUrls = form.imageUrls.slice(0, 1)
    form.imageUrl = form.imageUrls[0] || form.imageUrl
    if (v === 'popup') form.placement = 'popup'
    if (v === 'banner') form.placement = 'home_top'
  }
})

onMounted(load)
</script>

<template>
  <div class="w-full">
    <div class="flex items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold">الإعلانات</h1>
        <p class="text-sm text-white/70">إدارة الإعلانات والسلايدر المتحرك مع بقاء الإعلان ظاهرًا حتى تقوم أنت بحذفه أو استبداله.</p>
      </div>
      <div class="flex items-center gap-2">
        <UiButton variant="secondary" :disabled="loading" @click="load">تحديث</UiButton>
        <UiButton variant="secondary" @click="removeAll">حذف الكل</UiButton>
      </div>
    </div>

    <div class="mt-6 grid gap-6 lg:grid-cols-3">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>إنشاء إعلان</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-3">
            <div class="grid gap-2">
              <label class="text-sm font-medium">النوع</label>
              <select v-model="form.type" class="h-10 w-full rounded-2xl border border-white/10 bg-white/5 px-3 text-sm outline-none">
                <option value="slider">سلايدر متحرك</option>
                <option value="banner">بانر</option>
                <option value="popup">منبثق</option>
                <option value="product">إعلان منتج</option>
              </select>
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">الموضع</label>
              <select v-model="form.placement" class="h-10 w-full rounded-2xl border border-white/10 bg-white/5 px-3 text-sm outline-none">
                <option v-for="p in placements" :key="p.value" :value="p.value">{{ p.label }}</option>
              </select>
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">العنوان</label>
              <UiInput v-model="form.title" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">العنوان الفرعي</label>
              <UiInput v-model="form.subtitle" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">الصور</label>
              <div class="flex items-center gap-2">
                <input type="file" accept="image/*" :multiple="form.type === 'slider'" class="text-xs" @change="onPickFile" />
                <span v-if="uploading" class="text-xs text-white/60">جاري الرفع...</span>
              </div>
              <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
              <div v-if="previewImages.length" class="grid grid-cols-3 gap-2">
                <div v-for="(img, idx) in previewImages" :key="`${img}-${idx}`" class="relative overflow-hidden rounded-2xl border border-white/10">
                  <img :src="api.buildAssetUrl(String(img))" class="h-24 w-full object-cover" />
                  <button type="button" class="absolute left-1 top-1 rounded-full bg-black/70 px-2 py-1 text-[10px]" @click="removePreview(idx)">حذف</button>
                </div>
              </div>
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">الرابط</label>
              <UiInput v-model="form.linkUrl" placeholder="/products" dir="ltr" />
            </div>

            <div v-if="form.type === 'product'" class="grid gap-2">
              <label class="text-sm font-medium">معرف المنتج</label>
              <UiInput v-model="form.productId" placeholder="guid" dir="ltr" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">الترتيب</label>
              <UiInput v-model.number="form.sortOrder" type="number" min="0" step="1" />
            </div>

            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" v-model="form.isEnabled" class="h-4 w-4" />
              مفعل
            </label>

            <UiButton type="button" @click="create">حفظ</UiButton>
          </div>
        </UiCardContent>
      </UiCard>

      <UiCard class="lg:col-span-2">
        <UiCardHeader>
          <UiCardTitle>قائمة الإعلانات</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div v-if="loading" class="text-white/70">جاري التحميل...</div>
          <div v-else-if="!items.length" class="text-white/60">لا توجد إعلانات بعد</div>
          <div v-else class="grid gap-3">
            <div v-for="ad in items" :key="ad.id" class="rounded-2xl border border-white/10 bg-white/5 p-3">
              <div class="flex items-start justify-between gap-3">
                <div class="flex items-center gap-3 min-w-0">
                  <img :src="api.buildAssetUrl(String(ad.imageUrl || ad.imageUrls?.[0] || ''))" class="h-12 w-12 rounded-2xl object-cover border border-white/10" />
                  <div class="min-w-0">
                    <div class="font-extrabold truncate">{{ ad.title || 'بدون عنوان' }}</div>
                    <div class="text-xs text-white/60 keep-ltr">{{ ad.type }} • {{ ad.placement }} • sort: {{ ad.sortOrder }}</div>
                    <div v-if="ad.type === 'slider'" class="text-xs text-emerald-300">عدد الصور: {{ ad.imageUrls?.length || 0 }}</div>
                  </div>
                </div>

                <div class="flex items-center gap-2">
                  <button type="button" class="rounded-xl border border-white/10 bg-white/5 px-3 py-1.5 text-xs hover:bg-white/10" @click="toggleEnabled(ad)">
                    {{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}
                  </button>
                  <button type="button" class="rounded-xl border border-white/10 bg-white/5 px-3 py-1.5 text-xs hover:bg-white/10" @click="remove(ad.id)">
                    حذف
                  </button>
                </div>
              </div>
            </div>
          </div>
        </UiCardContent>
      </UiCard>
    </div>
  </div>
</template>
