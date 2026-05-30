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
  { value: 'slider', label: 'سلايدر رئيسي', icon: 'mdi:view-carousel-outline', hint: 'عدة صور في واجهة المتجر' },
  { value: 'banner', label: 'بانر ثابت', icon: 'mdi:image-outline', hint: 'صورة واحدة لقسم محدد' },
  { value: 'popup', label: 'إعلان منبثق', icon: 'mdi:tooltip-image-outline', hint: 'يظهر فوق الصفحة للزائر' },
  { value: 'product', label: 'داخل منتج', icon: 'mdi:package-variant-closed', hint: 'يرتبط بمنتج تختاره بالبحث' },
]

const placementOptions = computed(() => {
  if (form.type === 'slider') return [{ value: 'home_top_slider', label: 'أعلى الصفحة الرئيسية' }]
  if (form.type === 'popup') return [{ value: 'popup', label: 'نافذة منبثقة' }]
  if (form.type === 'product') return [{ value: 'product_page', label: 'داخل صفحة المنتج' }]
  return [
    { value: 'home_top', label: 'أعلى الصفحة الرئيسية' },
    { value: 'home_middle', label: 'منتصف الصفحة الرئيسية' },
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

const activeType = computed(() => adTypes.find((x) => x.value === form.type) || adTypes[0])
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
function productImage(p: any) {
  return p?.imageUrl || p?.primaryImageUrl || p?.thumbnailUrl || p?.images?.[0]?.url || p?.assets?.[0]?.url || ''
}

const filteredProducts = computed(() => {
  const q = productQuery.value.trim().toLowerCase()
  const source = products.value || []
  if (!q) return source.slice(0, 10)
  return source.filter((p: any) => {
    const txt = `${productName(p)} ${p.slug || ''} ${productBrand(p)}`.toLowerCase()
    return txt.includes(q)
  }).slice(0, 14)
})

async function load() {
  loading.value = true
  try {
    const [adsRes, productsRes]: any[] = await Promise.all([
      $fetch('/api/bff/admin/ads', { timeout: 10000, query: { _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }),
      $fetch('/api/bff/admin/products', { timeout: 10000, query: { page: 1, pageSize: 250, _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }).catch(() => []),
    ])
    items.value = Array.isArray(adsRes) ? adsRes : []
    products.value = normalizeProducts(productsRes)
  } catch {
    items.value = []
    products.value = []
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
    isEnabled: !!ad.isEnabled,
  })
  productQuery.value = form.productTitle
  window?.scrollTo?.({ top: 0, behavior: 'smooth' })
}

function selectType(type: string) {
  form.type = type
}

function selectProduct(p: any) {
  form.productId = p.id
  form.productTitle = productName(p)
  form.linkUrl = `/product/${p.id}`
  productQuery.value = productName(p)
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
    toast.error('اختر المنتج من البحث أولاً')
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
  <div class="ads-admin space-y-6">
    <section class="ads-hero admin-panel">
      <div>
        <div class="eyebrow rtl-text">مركز الإعلانات</div>
        <h1 class="ads-title rtl-text">إدارة الإعلانات</h1>
        <p class="ads-subtitle rtl-text">ارفع الصور، اختر نوع الإعلان، اربطه بمنتج عند الحاجة، وشاهد كل شيء من شاشة واحدة بدون معرفات يدوية.</p>
      </div>
      <div class="ads-hero__actions">
        <NuxtLink to="/admin/appearance" class="admin-secondary px-4 py-2 rtl-text">الشعار والاستفتاحية</NuxtLink>
        <UiButton variant="secondary" :disabled="loading" @click="load">تحديث</UiButton>
        <UiButton variant="secondary" @click="removeAll">حذف الكل</UiButton>
      </div>
    </section>

    <section class="grid gap-3 md:grid-cols-4">
      <div class="ad-stat"><b>{{ stats.total }}</b><span>كل الإعلانات</span></div>
      <div class="ad-stat"><b>{{ stats.active }}</b><span>مفعلة</span></div>
      <div class="ad-stat"><b>{{ stats.slider }}</b><span>سلايدر</span></div>
      <div class="ad-stat"><b>{{ stats.product }}</b><span>داخل منتجات</span></div>
    </section>

    <section class="grid gap-6 2xl:grid-cols-[minmax(440px,560px)_1fr]">
      <div class="admin-panel ad-builder">
        <div class="builder-head">
          <div>
            <h2 class="rtl-text">{{ editingId ? 'تعديل الإعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p class="rtl-text">اتبع الخطوات من الأعلى للأسفل. الحقول تتغير حسب نوع الإعلان.</p>
          </div>
          <button v-if="editingId" type="button" class="admin-secondary px-3 py-2" @click="resetForm">إعلان جديد</button>
        </div>

        <div class="step-card">
          <div class="step-label">1</div>
          <div class="min-w-0 flex-1">
            <div class="step-title rtl-text">نوع الإعلان</div>
            <div class="ad-type-grid">
              <button v-for="type in adTypes" :key="type.value" type="button" class="ad-type" :class="form.type === type.value ? 'is-active' : ''" @click="selectType(type.value)">
                <Icon :name="type.icon" class="ad-type__icon" />
                <span>{{ type.label }}</span>
                <small>{{ type.hint }}</small>
              </button>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-label">2</div>
          <div class="grid flex-1 gap-4">
            <div class="step-title rtl-text">المحتوى والمكان</div>
            <div class="grid gap-4 sm:grid-cols-2">
              <div class="grid gap-2">
                <label class="admin-label">الموضع</label>
                <select v-model="form.placement" class="admin-input h-12">
                  <option v-for="p in placementOptions" :key="p.value" :value="p.value">{{ p.label }}</option>
                </select>
              </div>
              <div class="grid gap-2">
                <label class="admin-label">الترتيب</label>
                <UiInput v-model.number="form.sortOrder" type="number" min="0" step="1" />
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
          </div>
        </div>

        <div v-if="form.type === 'product'" class="step-card product-search-step">
          <div class="step-label">3</div>
          <div class="grid flex-1 gap-3">
            <div>
              <div class="step-title rtl-text">اختيار المنتج</div>
              <p class="admin-muted text-xs rtl-text">ابحث عن المنتج ثم اضغط عليه لربط الإعلان بداخله.</p>
            </div>
            <UiInput v-model="productQuery" placeholder="ابحث بالاسم أو السلك أو البراند" />
            <div class="product-search-list">
              <button v-for="p in filteredProducts" :key="p.id" type="button" class="product-pick" :class="form.productId === p.id ? 'is-active' : ''" @click="selectProduct(p)">
                <img v-if="productImage(p)" :src="api.buildAssetUrl(productImage(p))" alt="" />
                <div v-else class="product-pick__empty"><Icon name="mdi:package-variant" /></div>
                <div class="min-w-0 flex-1">
                  <b>{{ productName(p) }}</b>
                  <small>{{ p.slug }} <span v-if="productBrand(p)">• {{ productBrand(p) }}</span></small>
                </div>
                <Icon v-if="form.productId === p.id" name="mdi:check-circle" class="text-xl text-emerald-400" />
              </button>
            </div>
            <div v-if="form.productId" class="selected-product rtl-text">تم اختيار: {{ form.productTitle }}</div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-label">{{ form.type === 'product' ? '4' : '3' }}</div>
          <div class="grid flex-1 gap-3">
            <div class="step-title rtl-text">الصور والرابط</div>
            <label class="upload-zone">
              <input type="file" accept="image/*" :multiple="form.type === 'slider'" class="hidden" @change="onPickFile" />
              <Icon name="mdi:cloud-upload-outline" class="text-3xl text-[rgb(var(--primary))]" />
              <span class="font-extrabold">{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع الصور' }}</span>
              <small>{{ form.type === 'slider' ? 'يمكن رفع أكثر من صورة للسلايدر' : 'صورة واحدة تكفي لهذا النوع' }}</small>
            </label>
            <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
            <div v-if="previewImages.length" class="preview-grid">
              <div v-for="(img, idx) in previewImages" :key="`${img}-${idx}`" class="preview-card">
                <img :src="api.buildAssetUrl(String(img))" />
                <button type="button" @click="removePreview(idx)">حذف</button>
              </div>
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الرابط</label>
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
            <p class="admin-muted text-sm rtl-text">راجع، عدّل، عطّل أو احذف الإعلانات بسهولة.</p>
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
                <span class="pill">{{ ad.type }}</span>
                <span class="pill">{{ ad.placement }}</span>
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

<style scoped>
.ads-hero{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; padding:1.35rem; border-radius:30px; }
.eyebrow{ display:inline-flex; border:1px solid rgba(var(--primary),.35); background:rgba(var(--primary),.08); color:rgb(var(--primary)); border-radius:999px; padding:.35rem .7rem; font-size:.72rem; font-weight:1000; }
.ads-title{ margin-top:.8rem; font-size:clamp(1.7rem,3vw,2.6rem); font-weight:1000; letter-spacing:-.04em; color:rgb(var(--text)); }
.ads-subtitle{ margin-top:.35rem; color:rgb(var(--muted)); max-width:760px; line-height:1.9; }
.ads-hero__actions{ display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; }
.ad-stat{ border:1px solid rgba(var(--border), .9); background:linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .82)); border-radius:26px; padding:1rem 1.1rem; box-shadow:var(--shadow-soft); }
.ad-stat b{ display:block; font-size:1.85rem; color:rgb(var(--text)); line-height:1; } .ad-stat span{ color:rgb(var(--muted)); font-size:.84rem; font-weight:900; }
.ad-builder,.ad-list-panel{ padding:1rem; border-radius:30px; }
.builder-head,.ad-list-head{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; margin-bottom:1rem; }
.builder-head h2,.ad-list-head h2{ font-size:1.25rem; font-weight:1000; color:rgb(var(--text)); }
.builder-head p{ margin-top:.2rem; color:rgb(var(--muted)); font-size:.82rem; }
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
