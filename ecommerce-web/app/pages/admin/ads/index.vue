<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

import UiButton from '~/components/ui/UiButton.vue'
import UiInput from '~/components/ui/UiInput.vue'

const toast = useToast()
const api = useApi()

function emitAdsChanged() {
  if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
}

const loading = ref(true)
const saving = ref(false)
const uploading = ref(false)
const items = ref<any[]>([])
const products = ref<any[]>([])
const productQuery = ref('')
const filterType = ref('all')
const editingId = ref<string | null>(null)

const adTypes = [
  { value: 'slider', label: 'سلايدر رئيسي', hint: 'عدة صور تظهر أعلى الصفحة الرئيسية' },
  { value: 'banner', label: 'بانر ثابت', hint: 'صورة واحدة في الصفحة الرئيسية' },
  { value: 'popup', label: 'إعلان منبثق', hint: 'يظهر للزائر فوق الصفحة' },
  { value: 'product', label: 'إعلان داخل منتج', hint: 'يرتبط بمنتج محدد من البحث' },
]

const placementOptions = computed(() => {
  if (form.type === 'slider') return [{ value: 'home_top_slider', label: 'أعلى الصفحة الرئيسية' }]
  if (form.type === 'popup') return [{ value: 'popup', label: 'منبثق في الرئيسية' }]
  if (form.type === 'product') return [{ value: 'product_page', label: 'داخل صفحة المنتج' }]
  return [
    { value: 'home_top', label: 'بانر أعلى الرئيسية' },
    { value: 'home_middle', label: 'منتصف الرئيسية' },
  ]
})

const form = reactive({
  type: 'slider',
  placement: 'home_top_slider',
  title: '',
  subtitle: '',
  imageUrl: '',
  imageUrls: [] as string[],
  linkUrl: '/products',
  productId: '',
  productTitle: '',
  sortOrder: 0,
  isEnabled: true,
})

const previewImages = computed(() => {
  const arr = Array.isArray(form.imageUrls) ? form.imageUrls.filter(Boolean) : []
  return arr.length ? arr : (form.imageUrl ? [form.imageUrl] : [])
})

const stats = computed(() => ({
  total: items.value.length,
  active: items.value.filter((x: any) => x.isEnabled).length,
  slider: items.value.filter((x: any) => x.type === 'slider').length,
  product: items.value.filter((x: any) => x.type === 'product').length,
}))

const filteredAds = computed(() => {
  if (filterType.value === 'all') return items.value
  if (filterType.value === 'active') return items.value.filter((x: any) => x.isEnabled)
  return items.value.filter((x: any) => x.type === filterType.value)
})

const filteredProducts = computed(() => {
  const q = productQuery.value.trim().toLowerCase()
  if (!q) return products.value.slice(0, 8)
  return products.value.filter((p: any) => {
    const txt = `${p.title || ''} ${p.slug || ''} ${p.brand || ''}`.toLowerCase()
    return txt.includes(q)
  }).slice(0, 12)
})

async function load() {
  loading.value = true
  try {
    const [adsRes, productsRes]: any[] = await Promise.all([
      $fetch('/api/bff/admin/ads', { timeout: 8000, query: { _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }),
      $fetch('/api/bff/admin/products', { timeout: 8000, query: { _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }).catch(() => []),
    ])
    items.value = Array.isArray(adsRes) ? adsRes : []
    products.value = Array.isArray(productsRes) ? productsRes : []
  } catch {
    items.value = []
  } finally {
    loading.value = false
  }
}

function resetForm() {
  editingId.value = null
  Object.assign(form, {
    type: 'slider', placement: 'home_top_slider', title: '', subtitle: '', imageUrl: '', imageUrls: [], linkUrl: '/products',
    productId: '', productTitle: '', sortOrder: 0, isEnabled: true,
  })
  productQuery.value = ''
}

function edit(ad: any) {
  editingId.value = ad.id
  Object.assign(form, {
    type: ad.type || 'banner',
    placement: ad.placement || 'home_top',
    title: ad.title || '',
    subtitle: ad.subtitle || '',
    imageUrl: ad.imageUrl || '',
    imageUrls: Array.isArray(ad.imageUrls) ? [...ad.imageUrls] : (ad.imageUrl ? [ad.imageUrl] : []),
    linkUrl: ad.linkUrl || '/products',
    productId: ad.productId || '',
    productTitle: products.value.find((p: any) => p.id === ad.productId)?.title || '',
    sortOrder: Number(ad.sortOrder || 0),
    isEnabled: !!ad.isEnabled,
  })
  window?.scrollTo?.({ top: 0, behavior: 'smooth' })
}

function selectType(type: string) {
  form.type = type
}

function selectProduct(p: any) {
  form.productId = p.id
  form.productTitle = p.title
  form.linkUrl = `/product/${p.id}`
  productQuery.value = p.title
}

function payload() {
  return {
    type: form.type,
    placement: form.placement,
    title: form.title,
    subtitle: form.subtitle || null,
    imageUrl: form.imageUrl,
    imageUrls: form.imageUrls,
    linkUrl: form.linkUrl || null,
    productId: form.type === 'product' && form.productId ? form.productId : null,
    sortOrder: Number(form.sortOrder || 0),
    isEnabled: Boolean(form.isEnabled),
    startAt: null,
    endAt: null,
  }
}

async function saveAd() {
  if (!previewImages.value.length) {
    toast.error('ارفع صورة واحدة على الأقل')
    return
  }
  if (form.type === 'product' && !form.productId) {
    toast.error('اختر المنتج الذي سيظهر الإعلان داخله')
    return
  }
  saving.value = true
  try {
    if (editingId.value) {
      await $fetch(`/api/bff/admin/ads/${editingId.value}`, { method: 'PUT', body: payload() })
      toast.success('تم تحديث الإعلان')
    } else {
      await $fetch('/api/bff/admin/ads', { method: 'POST', body: payload() })
      toast.success('تم إنشاء الإعلان')
    }
    resetForm()
    await load()
    emitAdsChanged()
  } catch {
    toast.error('تعذر حفظ الإعلان')
  } finally {
    saving.value = false
  }
}

async function remove(id: string) {
  if (!confirm('حذف الإعلان؟')) return
  try {
    await $fetch(`/api/bff/admin/ads/${id}`, { method: 'DELETE' })
    toast.success('تم الحذف')
    await load()
    emitAdsChanged()
  } catch { toast.error('تعذر الحذف') }
}

async function removeAll() {
  if (!confirm('حذف كل الإعلانات الحالية؟')) return
  try {
    await $fetch('/api/bff/admin/ads', { method: 'DELETE' })
    toast.success('تم حذف جميع الإعلانات')
    await load()
    emitAdsChanged()
  } catch { toast.error('تعذر حذف الكل') }
}

async function toggleEnabled(ad: any) {
  try {
    await $fetch(`/api/bff/admin/ads/${ad.id}`, {
      method: 'PUT',
      body: { ...ad, imageUrls: ad.imageUrls || (ad.imageUrl ? [ad.imageUrl] : []), isEnabled: !ad.isEnabled, startAt: ad.startAt || null, endAt: ad.endAt || null },
    })
    await load()
    emitAdsChanged()
  } catch { toast.error('تعذر تحديث الإعلان') }
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
      form.imageUrl = uploaded[0] || form.imageUrl
      form.imageUrls = form.imageUrl ? [form.imageUrl] : []
    }
    if (uploaded.length) toast.success('تم رفع الصور')
  } catch { toast.error('تعذر رفع الصور') }
  finally { uploading.value = false; (e.target as HTMLInputElement).value = '' }
}

function removePreview(idx: number) {
  form.imageUrls.splice(idx, 1)
  form.imageUrl = form.imageUrls[0] || ''
}

watch(() => form.type, (v) => {
  if (v === 'slider') form.placement = 'home_top_slider'
  else if (v === 'popup') form.placement = 'popup'
  else if (v === 'product') form.placement = 'product_page'
  else form.placement = 'home_top'
  if (v !== 'slider') form.imageUrls = form.imageUrl ? [form.imageUrl] : form.imageUrls.slice(0, 1)
})

onMounted(load)
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-extrabold text-[rgb(var(--text))] rtl-text">إدارة الإعلانات</h1>
        <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">أنشئ سلايدر أو بانر أو إعلان منبثق أو إعلان مرتبط بمنتج بدون كتابة معرفات يدوية.</p>
      </div>
      <div class="flex items-center gap-2">
        <UiButton variant="secondary" :disabled="loading" @click="load">تحديث</UiButton>
        <UiButton variant="secondary" @click="removeAll">حذف الكل</UiButton>
      </div>
    </div>

    <div class="grid gap-3 sm:grid-cols-4">
      <div class="ad-stat"><b>{{ stats.total }}</b><span>كل الإعلانات</span></div>
      <div class="ad-stat"><b>{{ stats.active }}</b><span>مفعلة</span></div>
      <div class="ad-stat"><b>{{ stats.slider }}</b><span>سلايدر</span></div>
      <div class="ad-stat"><b>{{ stats.product }}</b><span>إعلانات منتجات</span></div>
    </div>

    <div class="grid gap-6 xl:grid-cols-[minmax(380px,480px)_1fr]">
      <section class="admin-panel p-5">
        <div class="mb-4 flex items-center justify-between gap-3">
          <div>
            <h2 class="text-lg font-extrabold rtl-text">{{ editingId ? 'تعديل إعلان' : 'إنشاء إعلان' }}</h2>
            <p class="text-xs text-[rgb(var(--muted))] rtl-text">اختر النوع، ارفع الصورة، ثم حدد الرابط أو المنتج.</p>
          </div>
          <button v-if="editingId" type="button" class="admin-secondary px-3 py-2" @click="resetForm">جديد</button>
        </div>

        <div class="grid gap-3 sm:grid-cols-2">
          <button v-for="type in adTypes" :key="type.value" type="button" class="ad-type" :class="form.type === type.value ? 'is-active' : ''" @click="selectType(type.value)">
            <span>{{ type.label }}</span><small>{{ type.hint }}</small>
          </button>
        </div>

        <div class="mt-5 grid gap-4">
          <div class="grid gap-2">
            <label class="admin-label">الموضع</label>
            <select v-model="form.placement" class="admin-input h-12">
              <option v-for="p in placementOptions" :key="p.value" :value="p.value">{{ p.label }}</option>
            </select>
          </div>

          <div class="grid gap-2">
            <label class="admin-label">العنوان</label>
            <UiInput v-model="form.title" placeholder="مثال: عروض العناية الكورية" />
          </div>

          <div class="grid gap-2">
            <label class="admin-label">العنوان الفرعي</label>
            <UiInput v-model="form.subtitle" placeholder="نص قصير يظهر تحت العنوان" />
          </div>

          <div v-if="form.type === 'product'" class="grid gap-2">
            <label class="admin-label">اختر المنتج</label>
            <UiInput v-model="productQuery" placeholder="ابحث بالاسم أو السلك أو البراند" />
            <div class="max-h-64 overflow-auto rounded-2xl border border-app bg-surface p-2">
              <button v-for="p in filteredProducts" :key="p.id" type="button" class="product-pick" :class="form.productId === p.id ? 'is-active' : ''" @click="selectProduct(p)">
                <span class="font-extrabold">{{ p.title }}</span>
                <small>{{ p.slug }} • {{ p.brand }}</small>
              </button>
            </div>
            <div v-if="form.productId" class="rounded-2xl border border-emerald-500/25 bg-emerald-500/10 px-3 py-2 text-xs text-emerald-300 rtl-text">تم اختيار: {{ form.productTitle }}</div>
          </div>

          <div class="grid gap-2">
            <label class="admin-label">الصور</label>
            <label class="upload-zone">
              <input type="file" accept="image/*" :multiple="form.type === 'slider'" class="hidden" @change="onPickFile" />
              <span class="font-extrabold">{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع الصور' }}</span>
              <small>{{ form.type === 'slider' ? 'يمكن رفع أكثر من صورة للسلايدر' : 'صورة واحدة تكفي لهذا النوع' }}</small>
            </label>
            <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
            <div v-if="previewImages.length" class="grid grid-cols-3 gap-2">
              <div v-for="(img, idx) in previewImages" :key="`${img}-${idx}`" class="relative overflow-hidden rounded-2xl border border-app bg-surface-2">
                <img :src="api.buildAssetUrl(String(img))" class="h-24 w-full object-cover" />
                <button type="button" class="absolute left-1 top-1 rounded-full bg-black/70 px-2 py-1 text-[10px] text-white" @click="removePreview(idx)">حذف</button>
              </div>
            </div>
          </div>

          <div class="grid gap-2">
            <label class="admin-label">الرابط</label>
            <UiInput v-model="form.linkUrl" placeholder="/products أو رابط خارجي" dir="ltr" />
          </div>

          <div class="grid gap-2">
            <label class="admin-label">الترتيب</label>
            <UiInput v-model.number="form.sortOrder" type="number" min="0" step="1" />
          </div>

          <label class="flex w-max items-center gap-2 rounded-2xl border border-app bg-surface-2 px-3 py-2 text-sm font-bold">
            <input type="checkbox" v-model="form.isEnabled" class="h-4 w-4" /> مفعل
          </label>

          <UiButton type="button" :disabled="saving || uploading" @click="saveAd">{{ saving ? 'جاري الحفظ...' : (editingId ? 'حفظ التعديل' : 'إنشاء الإعلان') }}</UiButton>
        </div>
      </section>

      <section class="admin-panel p-5">
        <div class="mb-4 flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 class="text-lg font-extrabold rtl-text">قائمة الإعلانات</h2>
            <p class="text-xs text-[rgb(var(--muted))] rtl-text">فلتر حسب النوع أو عدل الإعلان مباشرة.</p>
          </div>
          <select v-model="filterType" class="admin-input h-11 w-48">
            <option value="all">الكل</option><option value="active">المفعلة فقط</option><option value="slider">سلايدر</option><option value="banner">بانر</option><option value="popup">منبثق</option><option value="product">منتج</option>
          </select>
        </div>

        <div v-if="loading" class="admin-muted">جاري التحميل...</div>
        <div v-else-if="!filteredAds.length" class="empty-box">لا توجد إعلانات حسب هذا الفلتر</div>
        <div v-else class="grid gap-3">
          <div v-for="ad in filteredAds" :key="ad.id" class="ad-row">
            <img :src="api.buildAssetUrl(String(ad.imageUrl || ad.imageUrls?.[0] || ''))" class="h-20 w-20 rounded-2xl object-cover border border-app bg-surface-2" />
            <div class="min-w-0 flex-1">
              <div class="flex flex-wrap items-center gap-2">
                <b class="truncate">{{ ad.title || 'بدون عنوان' }}</b>
                <span class="pill">{{ ad.type }}</span>
                <span class="pill">{{ ad.placement }}</span>
                <span class="pill" :class="ad.isEnabled ? 'is-good' : 'is-off'">{{ ad.isEnabled ? 'مفعل' : 'معطل' }}</span>
              </div>
              <p class="mt-1 truncate text-xs text-[rgb(var(--muted))] keep-ltr">{{ ad.linkUrl || 'بدون رابط' }}</p>
              <p v-if="ad.type === 'slider'" class="mt-1 text-xs text-[rgb(var(--primary))]">عدد الصور: {{ ad.imageUrls?.length || 0 }}</p>
            </div>
            <div class="flex flex-wrap items-center gap-2">
              <button class="admin-secondary px-3 py-2" @click="edit(ad)">تعديل</button>
              <button class="admin-secondary px-3 py-2" @click="toggleEnabled(ad)">{{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}</button>
              <button class="admin-danger px-3 py-2" @click="remove(ad.id)">حذف</button>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.ad-stat{ border:1px solid rgba(var(--border), .9); background:linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .82)); border-radius:24px; padding:18px; box-shadow:var(--shadow-soft); }
.ad-stat b{ display:block; font-size:1.55rem; color:rgb(var(--text)); } .ad-stat span{ color:rgb(var(--muted)); font-size:.86rem; font-weight:800; }
.ad-type{ text-align:start; border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-2-rgb), .72); border-radius:22px; padding:14px; transition:.2s ease; }
.ad-type span{ display:block; font-weight:1000; color:rgb(var(--text)); } .ad-type small{ display:block; margin-top:5px; color:rgb(var(--muted)); line-height:1.6; }
.ad-type.is-active{ border-color:rgba(var(--primary), .55); background:rgba(var(--primary), .12); transform:translateY(-2px); }
.upload-zone{ display:grid; place-items:center; gap:6px; min-height:112px; border:1px dashed rgba(var(--primary), .42); background:rgba(var(--primary), .07); border-radius:24px; cursor:pointer; text-align:center; color:rgb(var(--text)); }
.upload-zone small{ color:rgb(var(--muted)); }
.product-pick{ width:100%; display:grid; gap:3px; text-align:start; border-radius:16px; padding:10px 12px; color:rgb(var(--text)); }
.product-pick:hover,.product-pick.is-active{ background:rgba(var(--primary), .12); }
.product-pick small{ color:rgb(var(--muted)); }
.ad-row{ display:flex; align-items:center; gap:14px; border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-2-rgb), .56); border-radius:24px; padding:12px; }
.pill{ border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-rgb), .75); border-radius:999px; padding:.28rem .55rem; font-size:.72rem; color:rgb(var(--muted)); font-weight:900; }
.pill.is-good{ color:rgb(52 211 153); border-color:rgba(52,211,153,.22); } .pill.is-off{ color:rgb(248 113 113); border-color:rgba(248,113,113,.22); }
.empty-box{ display:grid; place-items:center; min-height:220px; border:1px dashed rgba(var(--border), .9); border-radius:26px; color:rgb(var(--muted)); }
@media(max-width:760px){ .ad-row{ align-items:flex-start; flex-direction:column; } .ad-row img{ width:100%; height:180px; } }
</style>
