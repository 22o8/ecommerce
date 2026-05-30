<template>
  <div class="ads-admin space-y-6">
    <section class="admin-panel ads-hero">
      <div>
        <div class="eyebrow rtl-text">مركز الإعلانات</div>
        <h1 class="ads-title rtl-text">إدارة الإعلانات</h1>
        <p class="ads-subtitle rtl-text">أنشئ سلايدر، بانر أعلى الصفحة أو آخرها، إعلان منبثق، أو إعلان مرتبط بمنتج محدد بدون كتابة معرفات يدوية.</p>
      </div>
      <div class="ads-hero__actions">
        <button class="admin-secondary px-4 py-3" type="button" @click="load">تحديث</button>
        <button class="admin-secondary px-4 py-3" type="button" @click="resetForm">إعلان جديد</button>
        <button class="admin-danger px-4 py-3" type="button" @click="removeAll">حذف الكل</button>
      </div>
    </section>

    <section class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <div class="ad-stat"><b>{{ stats.total }}</b><span>كل الإعلانات</span></div>
      <div class="ad-stat"><b>{{ stats.active }}</b><span>مفعلة</span></div>
      <div class="ad-stat"><b>{{ stats.slider }}</b><span>سلايدر</span></div>
      <div class="ad-stat"><b>{{ stats.popup }}</b><span>منبثقة</span></div>
    </section>

    <section class="grid gap-6 xl:grid-cols-[minmax(420px,620px)_1fr]">
      <div class="admin-panel ad-builder">
        <div class="builder-head">
          <div>
            <h2 class="rtl-text">{{ editingId ? 'تعديل الإعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p class="rtl-text">اتبع الخطوات: نوع الإعلان، المكان، المحتوى، ثم الحفظ.</p>
          </div>
          <span class="builder-badge">{{ activeType.label }}</span>
        </div>

        <div class="step-card">
          <div class="step-label">1</div>
          <div class="min-w-0 flex-1">
            <h3 class="step-title rtl-text">نوع الإعلان</h3>
            <div class="ad-type-grid">
              <button
                v-for="type in adTypes"
                :key="type.value"
                type="button"
                class="ad-type"
                :class="form.type === type.value ? 'is-active' : ''"
                @click="selectType(type.value)"
              >
                <Icon :name="type.icon" class="ad-type__icon" />
                <span class="rtl-text">{{ type.label }}</span>
                <small class="rtl-text">{{ type.hint }}</small>
              </button>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-label">2</div>
          <div class="grid min-w-0 flex-1 gap-4">
            <h3 class="step-title rtl-text">المكان والمحتوى</h3>
            <div class="grid gap-4 sm:grid-cols-2">
              <div class="grid gap-2">
                <label class="admin-label">موضع الظهور</label>
                <select v-model="form.placement" class="admin-input h-12">
                  <option v-for="p in placementOptions" :key="p.value" :value="p.value">{{ p.label }}</option>
                </select>
              </div>
              <div class="grid gap-2">
                <label class="admin-label">الترتيب</label>
                <UiInput v-model="form.sortOrder" type="number" />
              </div>
            </div>

            <div class="grid gap-2">
              <label class="admin-label">العنوان</label>
              <UiInput v-model="form.title" placeholder="مثال: عروض العناية الكورية" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">العنوان الفرعي</label>
              <UiInput v-model="form.subtitle" placeholder="نص قصير يظهر تحت العنوان" />
            </div>

            <div v-if="form.type === 'product'" class="grid gap-3">
              <label class="admin-label">اختيار المنتج</label>
              <UiInput v-model="productQuery" placeholder="ابحث باسم المنتج أو البراند أو slug" />
              <div class="product-search-list">
                <button
                  v-for="p in filteredProducts"
                  :key="p.id"
                  type="button"
                  class="product-pick"
                  :class="form.productId === p.id ? 'is-active' : ''"
                  @click="selectProduct(p)"
                >
                  <img v-if="productImage(p)" :src="api.buildAssetUrl(productImage(p))" />
                  <span v-else class="product-pick__empty"><Icon name="mdi:image-off-outline" /></span>
                  <span class="min-w-0 flex-1">
                    <b>{{ productName(p) }}</b>
                    <small>{{ productBrand(p) || p.slug || 'منتج' }}</small>
                  </span>
                  <Icon v-if="form.productId === p.id" name="mdi:check-circle" class="text-xl text-emerald-400" />
                </button>
              </div>
              <div v-if="form.productId" class="selected-product rtl-text">تم اختيار: {{ form.productTitle }}</div>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-label">3</div>
          <div class="grid min-w-0 flex-1 gap-4">
            <h3 class="step-title rtl-text">الصور والرابط</h3>
            <label class="upload-zone">
              <input type="file" accept="image/*" :multiple="form.type === 'slider'" class="hidden" @change="onPickFile" />
              <Icon name="mdi:cloud-upload-outline" class="text-3xl text-[rgb(var(--primary))]" />
              <span class="font-extrabold">{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع الصور' }}</span>
              <small>{{ form.type === 'slider' ? 'يمكن رفع أكثر من صورة للسلايدر' : 'صورة واحدة تكفي لهذا النوع' }}</small>
            </label>
            <UiInput v-model="form.imageUrl" placeholder="رابط الصورة اليدوي إن وجد" dir="ltr" />
            <div v-if="previewImages.length" class="preview-grid">
              <div v-for="(img, idx) in previewImages" :key="`${img}-${idx}`" class="preview-card">
                <img :src="api.buildAssetUrl(String(img))" />
                <button type="button" @click="removePreview(idx)">حذف</button>
              </div>
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الرابط عند الضغط</label>
              <UiInput v-model="form.linkUrl" placeholder="/products أو رابط خارجي" dir="ltr" />
            </div>
            <label class="enable-toggle">
              <input type="checkbox" v-model="form.isEnabled" />
              <span>الإعلان مفعل ويظهر في المتجر</span>
            </label>
            <UiButton type="button" class="min-h-12" :disabled="saving || uploading" @click="saveAd">
              {{ saving ? 'جاري الحفظ...' : (editingId ? 'حفظ التعديل' : 'إنشاء الإعلان') }}
            </UiButton>
          </div>
        </div>
      </div>

      <div class="admin-panel ad-list-panel">
        <div class="ad-list-head">
          <div>
            <h2 class="rtl-text">قائمة الإعلانات</h2>
            <p class="admin-muted text-sm rtl-text">تظهر الإعلانات هنا مباشرة بعد الحفظ أو الرفع.</p>
          </div>
          <select v-model="filterType" class="admin-input h-11 w-full sm:w-56">
            <option value="all">كل الإعلانات</option>
            <option value="active">المفعلة فقط</option>
            <option value="disabled">المعطلة فقط</option>
            <option value="slider">سلايدر</option>
            <option value="banner">بانر</option>
            <option value="popup">منبثق</option>
            <option value="product">داخل منتج</option>
          </select>
        </div>

        <div v-if="loading" class="empty-box">جاري التحميل...</div>
        <div v-else-if="!filteredAds.length" class="empty-box">لا توجد إعلانات حسب هذا الفلتر</div>
        <div v-else class="grid gap-3">
          <article v-for="ad in filteredAds" :key="ad.id" class="ad-row">
            <img :src="api.buildAssetUrl(String(ad.imageUrl || ad.imageUrls?.[0] || ''))" />
            <div class="min-w-0 flex-1">
              <div class="flex flex-wrap items-center gap-2">
                <b class="truncate">{{ ad.title || 'بدون عنوان' }}</b>
                <span class="pill">{{ typeLabel(ad.type) }}</span>
                <span class="pill">{{ placementLabel(ad.placement) }}</span>
                <span class="pill" :class="ad.isEnabled ? 'is-good' : 'is-off'">{{ ad.isEnabled ? 'مفعل' : 'معطل' }}</span>
              </div>
              <p class="mt-1 truncate text-xs text-[rgb(var(--muted))] keep-ltr">{{ ad.linkUrl || 'بدون رابط' }}</p>
              <p v-if="ad.type === 'slider'" class="mt-1 text-xs font-bold text-[rgb(var(--primary))]">عدد الصور: {{ ad.imageUrls?.length || 0 }}</p>
            </div>
            <div class="ad-actions">
              <button class="admin-secondary px-3 py-2" @click="edit(ad)">تعديل</button>
              <button class="admin-secondary px-3 py-2" @click="toggleEnabled(ad)">{{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}</button>
              <button class="admin-danger px-3 py-2" @click="remove(ad.id)">حذف</button>
            </div>
          </article>
        </div>
      </div>
    </section>
  </div>
</template>

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
  { value: 'slider', label: 'سلايدر', icon: 'mdi:view-carousel-outline', hint: 'عدة صور تتحرك في أعلى أو آخر الصفحة' },
  { value: 'banner', label: 'بانر', icon: 'mdi:image-outline', hint: 'صورة ثابتة في موضع تختاره' },
  { value: 'popup', label: 'منبثق', icon: 'mdi:tooltip-image-outline', hint: 'نافذة تظهر للزائر في الصفحة الرئيسية' },
  { value: 'product', label: 'داخل منتج', icon: 'mdi:package-variant-closed', hint: 'إعلان مرتبط بمنتج تختاره بالبحث' },
]

const allPlacements = [
  { value: 'home_top_slider', label: 'سلايدر أعلى الرئيسية', type: 'slider' },
  { value: 'home_bottom_slider', label: 'سلايدر آخر الرئيسية', type: 'slider' },
  { value: 'page_top_slider', label: 'سلايدر أعلى الصفحات', type: 'slider' },
  { value: 'page_bottom_slider', label: 'سلايدر آخر الصفحات', type: 'slider' },
  { value: 'home_top', label: 'بانر أعلى الرئيسية', type: 'banner' },
  { value: 'home_middle', label: 'بانر منتصف الرئيسية', type: 'banner' },
  { value: 'home_bottom', label: 'بانر آخر الرئيسية', type: 'banner' },
  { value: 'page_top', label: 'بانر أعلى الصفحات', type: 'banner' },
  { value: 'page_bottom', label: 'بانر آخر الصفحات', type: 'banner' },
  { value: 'popup', label: 'إعلان منبثق', type: 'popup' },
  { value: 'product_page', label: 'داخل صفحة المنتج', type: 'product' },
]
const placementOptions = computed(() => allPlacements.filter((x) => x.type === form.type))

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

const activeType = computed(() => adTypes.find((x) => x.value === form.type) || adTypes[0])
const previewImages = computed(() => {
  const arr = Array.isArray(form.imageUrls) ? form.imageUrls.filter(Boolean) : []
  return arr.length ? arr : (form.imageUrl ? [form.imageUrl] : [])
})
const stats = computed(() => ({
  total: items.value.length,
  active: items.value.filter((x: any) => x.isEnabled).length,
  slider: items.value.filter((x: any) => x.type === 'slider').length,
  popup: items.value.filter((x: any) => x.type === 'popup').length,
}))
const filteredAds = computed(() => {
  if (filterType.value === 'all') return items.value
  if (filterType.value === 'active') return items.value.filter((x: any) => x.isEnabled)
  if (filterType.value === 'disabled') return items.value.filter((x: any) => !x.isEnabled)
  return items.value.filter((x: any) => x.type === filterType.value)
})

function normalizeProducts(res: any) {
  if (Array.isArray(res)) return res
  if (Array.isArray(res?.items)) return res.items
  if (Array.isArray(res?.data)) return res.data
  return []
}
function productName(p: any) { return p?.title || p?.name || p?.nameAr || 'منتج بدون اسم' }
function productBrand(p: any) { return p?.brand || p?.brandName || p?.brandSlug || '' }
function productImage(p: any) { return p?.imageUrl || p?.primaryImageUrl || p?.thumbnailUrl || p?.images?.[0]?.url || p?.assets?.[0]?.url || '' }
const filteredProducts = computed(() => {
  const q = productQuery.value.trim().toLowerCase()
  const source = products.value || []
  if (!q) return source.slice(0, 12)
  return source.filter((p: any) => `${productName(p)} ${p.slug || ''} ${productBrand(p)}`.toLowerCase().includes(q)).slice(0, 16)
})

async function load() {
  loading.value = true
  try {
    const [adsRes, productsRes]: any[] = await Promise.all([
      $fetch('/api/bff/admin/ads', { query: { _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }),
      $fetch('/api/bff/admin/products', { query: { page: 1, pageSize: 300, _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }).catch(() => []),
    ])
    items.value = Array.isArray(adsRes) ? adsRes : []
    products.value = normalizeProducts(productsRes)
  } catch {
    items.value = []
    products.value = []
  } finally { loading.value = false }
}

function resetForm() {
  editingId.value = null
  Object.assign(form, { type: 'slider', placement: 'home_top_slider', title: '', subtitle: '', imageUrl: '', imageUrls: [], linkUrl: '/products', productId: '', productTitle: '', sortOrder: 0, isEnabled: true })
  productQuery.value = ''
}
function edit(ad: any) {
  editingId.value = ad.id
  const found = products.value.find((p: any) => p.id === ad.productId)
  Object.assign(form, {
    type: ad.type || 'banner',
    placement: ad.placement || 'home_top',
    title: ad.title || '',
    subtitle: ad.subtitle || '',
    imageUrl: ad.imageUrl || '',
    imageUrls: Array.isArray(ad.imageUrls) ? [...ad.imageUrls] : (ad.imageUrl ? [ad.imageUrl] : []),
    linkUrl: ad.linkUrl || '/products',
    productId: ad.productId || '',
    productTitle: found ? productName(found) : '',
    sortOrder: Number(ad.sortOrder || 0),
    isEnabled: ad.isEnabled !== false,
  })
  productQuery.value = form.productTitle
  window?.scrollTo?.({ top: 0, behavior: 'smooth' })
}
function selectType(type: string) {
  form.type = type
  const first = allPlacements.find((x) => x.type === type)
  form.placement = first?.value || 'home_top'
  if (type === 'product') form.linkUrl = form.productId ? `/product/${form.productId}` : '/products'
}
function selectProduct(p: any) {
  form.productId = p.id
  form.productTitle = productName(p)
  form.linkUrl = `/product/${p.id}`
  productQuery.value = productName(p)
}
function payload() {
  return { type: form.type, placement: form.placement, title: form.title, subtitle: form.subtitle || null, imageUrl: form.imageUrl, imageUrls: form.imageUrls, linkUrl: form.linkUrl || null, productId: form.type === 'product' && form.productId ? form.productId : null, sortOrder: Number(form.sortOrder || 0), isEnabled: Boolean(form.isEnabled), startAt: null, endAt: null }
}
async function saveAd() {
  if (!previewImages.value.length) return toast.error('ارفع صورة واحدة على الأقل')
  if (form.type === 'product' && !form.productId) return toast.error('اختر المنتج من البحث أولاً')
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
  } catch { toast.error('تعذر حفظ الإعلان') }
  finally { saving.value = false }
}
async function remove(id: string) {
  if (!confirm('حذف الإعلان؟')) return
  try { await $fetch(`/api/bff/admin/ads/${id}`, { method: 'DELETE' }); toast.success('تم الحذف'); await load(); emitAdsChanged() }
  catch { toast.error('تعذر الحذف') }
}
async function removeAll() {
  if (!confirm('حذف كل الإعلانات الحالية؟')) return
  try { await $fetch('/api/bff/admin/ads', { method: 'DELETE' }); toast.success('تم حذف جميع الإعلانات'); await load(); emitAdsChanged() }
  catch { toast.error('تعذر حذف الكل') }
}
async function toggleEnabled(ad: any) {
  try {
    await $fetch(`/api/bff/admin/ads/${ad.id}`, { method: 'PUT', body: { ...ad, imageUrls: ad.imageUrls || (ad.imageUrl ? [ad.imageUrl] : []), isEnabled: !ad.isEnabled, startAt: ad.startAt || null, endAt: ad.endAt || null } })
    await load(); emitAdsChanged()
  } catch { toast.error('تعذر تحديث الإعلان') }
}
async function onPickFile(e: Event) {
  const input = e.target as HTMLInputElement
  const files = Array.from(input.files || [])
  if (!files.length) return
  uploading.value = true
  try {
    const uploaded: string[] = []
    for (const file of files) {
      const fd = new FormData()
      fd.append('file', file)
      const res: any = await $fetch('/api/bff/admin/ads/upload', { method: 'POST', body: fd })
      const url = res?.url?.url || res?.url || res?.imageUrl || ''
      if (url) uploaded.push(url)
    }
    if (form.type === 'slider') form.imageUrls = [...form.imageUrls, ...uploaded]
    else {
      form.imageUrl = uploaded[0] || form.imageUrl
      form.imageUrls = form.imageUrl ? [form.imageUrl] : []
    }
    toast.success('تم رفع الصور')
  } catch { toast.error('تعذر رفع الصور') }
  finally { uploading.value = false; input.value = '' }
}
function removePreview(idx: number) {
  const arr = [...previewImages.value]
  arr.splice(idx, 1)
  form.imageUrls = arr
  form.imageUrl = arr[0] || ''
}
function typeLabel(type: string) { return adTypes.find((x) => x.value === type)?.label || type }
function placementLabel(p: string) { return allPlacements.find((x) => x.value === p)?.label || p }

watch(() => form.imageUrl, (v) => { if (v && !form.imageUrls.length) form.imageUrls = [v] })
watch(() => form.type, (v) => { if (!placementOptions.value.some((p) => p.value === form.placement)) form.placement = placementOptions.value[0]?.value || 'home_top' })
await load()
</script>

<style scoped>
.ads-hero{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; padding:1.35rem; border-radius:30px; }
.eyebrow{ display:inline-flex; border:1px solid rgba(var(--primary),.35); background:rgba(var(--primary),.08); color:rgb(var(--primary)); border-radius:999px; padding:.35rem .7rem; font-size:.72rem; font-weight:1000; }
.ads-title{ margin-top:.8rem; font-size:clamp(1.7rem,3vw,2.6rem); font-weight:1000; letter-spacing:-.04em; color:rgb(var(--text)); }
.ads-subtitle{ margin-top:.35rem; color:rgb(var(--muted)); max-width:820px; line-height:1.9; }
.ads-hero__actions{ display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; }
.ad-stat{ border:1px solid rgba(var(--border), .9); background:linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .82)); border-radius:26px; padding:1rem 1.1rem; box-shadow:var(--shadow-soft); }
.ad-stat b{ display:block; font-size:1.85rem; color:rgb(var(--text)); line-height:1; } .ad-stat span{ color:rgb(var(--muted)); font-size:.84rem; font-weight:900; }
.ad-builder,.ad-list-panel{ padding:1rem; border-radius:30px; }
.builder-head,.ad-list-head{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; margin-bottom:1rem; }
.builder-head h2,.ad-list-head h2{ font-size:1.25rem; font-weight:1000; color:rgb(var(--text)); }
.builder-head p{ margin-top:.2rem; color:rgb(var(--muted)); font-size:.82rem; }
.builder-badge{ display:inline-flex; border-radius:999px; border:1px solid rgba(var(--primary),.28); background:rgba(var(--primary),.10); color:rgb(var(--primary)); padding:.45rem .8rem; font-weight:1000; font-size:.8rem; }
.step-card{ display:flex; gap:1rem; border:1px solid rgba(var(--border),.86); background:rgba(var(--surface-2-rgb),.48); border-radius:26px; padding:1rem; margin-top:1rem; }
.step-label{ display:grid; place-items:center; width:36px; height:36px; flex:0 0 auto; border-radius:50%; background:rgb(var(--primary)); color:#050509; font-weight:1000; box-shadow:0 10px 24px rgba(var(--primary),.24); }
.step-title{ margin-bottom:.75rem; color:rgb(var(--text)); font-size:1rem; font-weight:1000; }
.ad-type-grid{ display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:.65rem; }
.ad-type{ display:grid; gap:.25rem; text-align:start; border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-rgb), .72); border-radius:22px; padding:.9rem; transition:.2s ease; }
.ad-type__icon{ font-size:1.35rem; color:rgb(var(--primary)); }
.ad-type span{ font-weight:1000; color:rgb(var(--text)); } .ad-type small{ color:rgb(var(--muted)); line-height:1.6; font-size:.74rem; }
.ad-type:hover,.ad-type.is-active{ border-color:rgba(var(--primary), .55); background:rgba(var(--primary), .12); transform:translateY(-2px); }
.upload-zone{ display:grid; place-items:center; gap:.35rem; min-height:132px; border:1px dashed rgba(var(--primary), .44); background:linear-gradient(180deg, rgba(var(--primary), .08), rgba(var(--surface-rgb), .45)); border-radius:24px; cursor:pointer; text-align:center; color:rgb(var(--text)); }
.upload-zone small{ color:rgb(var(--muted)); }
.preview-grid{ display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:.65rem; }
.preview-card{ position:relative; overflow:hidden; border-radius:20px; border:1px solid rgba(var(--border),.9); background:rgb(var(--surface-2)); }
.preview-card img{ width:100%; height:120px; object-fit:cover; display:block; }
.preview-card button{ position:absolute; inset-inline-start:.4rem; top:.4rem; border-radius:999px; background:rgba(0,0,0,.68); color:white; font-size:.7rem; padding:.25rem .55rem; }
.enable-toggle{ display:flex; align-items:center; gap:.55rem; width:max-content; border:1px solid rgba(var(--border),.9); background:rgba(var(--surface-rgb),.7); border-radius:18px; padding:.7rem .9rem; color:rgb(var(--text)); font-size:.9rem; font-weight:900; }
.product-search-list{ max-height:370px; overflow:auto; display:grid; gap:.45rem; border:1px solid rgba(var(--border),.9); background:rgba(var(--surface-rgb),.55); padding:.55rem; border-radius:22px; }
.product-pick{ display:flex; align-items:center; gap:.75rem; width:100%; text-align:start; border-radius:18px; padding:.6rem; color:rgb(var(--text)); border:1px solid transparent; }
.product-pick:hover,.product-pick.is-active{ background:rgba(var(--primary), .12); border-color:rgba(var(--primary),.22); }
.product-pick img,.product-pick__empty{ width:52px; height:52px; border-radius:16px; object-fit:cover; border:1px solid rgba(var(--border),.8); background:rgb(var(--surface-2)); display:grid; place-items:center; color:rgb(var(--muted)); flex:0 0 auto; }
.product-pick b{ display:block; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.product-pick small{ display:block; margin-top:.2rem; color:rgb(var(--muted)); font-size:.75rem; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.selected-product{ border:1px solid rgba(52,211,153,.25); background:rgba(52,211,153,.10); color:rgb(52 211 153); border-radius:18px; padding:.7rem .9rem; font-size:.8rem; font-weight:900; }
.ad-row{ display:flex; align-items:center; gap:.9rem; border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-2-rgb), .56); border-radius:24px; padding:.85rem; }
.ad-row > img{ width:86px; height:86px; object-fit:cover; border-radius:20px; border:1px solid rgba(var(--border),.9); background:rgb(var(--surface-2)); }
.pill{ border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-rgb), .75); border-radius:999px; padding:.28rem .55rem; font-size:.72rem; color:rgb(var(--muted)); font-weight:900; }
.pill.is-good{ color:rgb(52 211 153); border-color:rgba(52,211,153,.22); } .pill.is-off{ color:rgb(248 113 113); border-color:rgba(248,113,113,.22); }
.ad-actions{ display:flex; gap:.45rem; flex-wrap:wrap; justify-content:flex-end; }
.empty-box{ display:grid; place-items:center; min-height:240px; border:1px dashed rgba(var(--border), .9); border-radius:26px; color:rgb(var(--muted)); }
@media(max-width:920px){ .ads-hero,.builder-head,.ad-list-head{ flex-direction:column; } .ads-hero__actions{ justify-content:flex-start; } .ad-type-grid{ grid-template-columns:1fr; } .step-card{ flex-direction:column; } .ad-row{ align-items:flex-start; flex-direction:column; } .ad-row > img{ width:100%; height:190px; } .ad-actions{ justify-content:flex-start; } }
</style>
