<template>
  <div class="ads-admin" dir="rtl">
    <section class="ads-header admin-panel">
      <div>
        <p class="ads-eyebrow">مركز الإعلانات</p>
        <h1>إدارة الإعلانات</h1>
        <p>أنشئ إعلانك، ارفع الصورة أو الفيديو، ثم احفظ. الإعلان يظهر فوراً في القائمة بدون تحديث الصفحة.</p>
      </div>
      <div class="header-actions">
        <button class="admin-secondary" type="button" @click="load">تحديث</button>
        <button class="admin-secondary" type="button" @click="startNewAd">إعلان جديد</button>
        <button class="admin-danger" type="button" @click="removeAll">حذف الكل</button>
      </div>
    </section>

    <section class="stats-grid">
      <div class="stat-card"><b>{{ stats.total }}</b><span>كل الإعلانات</span></div>
      <div class="stat-card"><b>{{ stats.active }}</b><span>مفعلة</span></div>
      <div class="stat-card"><b>{{ stats.slider }}</b><span>سلايدر</span></div>
      <div class="stat-card"><b>{{ stats.banner }}</b><span>بانر</span></div>
      <div class="stat-card"><b>{{ stats.popup }}</b><span>منبثقة</span></div>
    </section>

    <section class="ads-layout">
      <form class="ad-editor admin-panel" @submit.prevent="saveAd">
        <div class="editor-title">
          <div>
            <p class="ads-eyebrow">محرر الإعلان</p>
            <h2>{{ editingId ? 'تعديل إعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p>اختر النوع، المكان، المحتوى، ثم احفظ.</p>
          </div>
          <span class="type-badge">{{ activeType.label }}</span>
        </div>

        <div class="editor-step">
          <div class="step-number">1</div>
          <div class="step-content">
            <h3>نوع الإعلان</h3>
            <div class="type-grid">
              <button
                v-for="type in adTypes"
                :key="type.value"
                type="button"
                class="type-card"
                :class="{ active: form.type === type.value }"
                @click="selectType(type.value)"
              >
                <Icon :name="type.icon" />
                <b>{{ type.label }}</b>
                <small>{{ type.hint }}</small>
              </button>
            </div>
          </div>
        </div>

        <div class="editor-step">
          <div class="step-number">2</div>
          <div class="step-content">
            <h3>المكان والمحتوى</h3>
            <div class="form-grid">
              <label class="field">
                <span>موضع الظهور</span>
                <select v-model="form.placement" class="admin-input">
                  <option v-for="p in placementOptions" :key="p.value" :value="p.value">{{ p.label }}</option>
                </select>
              </label>
              <label class="field">
                <span>الترتيب</span>
                <input v-model.number="form.sortOrder" type="number" class="admin-input" />
              </label>
            </div>

            <label class="field">
              <span>العنوان</span>
              <input v-model="form.title" class="admin-input" placeholder="مثال: عروض العناية الكورية" />
            </label>

            <label class="field">
              <span>العنوان الفرعي</span>
              <input v-model="form.subtitle" class="admin-input" placeholder="نص قصير يظهر تحت العنوان" />
            </label>

            <div v-if="form.type === 'product'" class="product-picker">
              <label class="field">
                <span>ابحث عن المنتج</span>
                <input v-model="productQuery" class="admin-input" placeholder="اكتب اسم المنتج أو البراند أو slug" />
              </label>
              <div class="product-list">
                <button
                  v-for="p in filteredProducts"
                  :key="p.id"
                  type="button"
                  class="product-item"
                  :class="{ active: form.productId === p.id }"
                  @click="selectProduct(p)"
                >
                  <img v-if="productImage(p)" :src="api.buildAssetUrl(productImage(p))" alt="" />
                  <span v-else class="product-empty"><Icon name="mdi:image-off-outline" /></span>
                  <span>
                    <b>{{ productName(p) }}</b>
                    <small>{{ productBrand(p) || p.slug || 'منتج' }}</small>
                  </span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="editor-step">
          <div class="step-number">3</div>
          <div class="step-content">
            <h3>الصور أو الفيديو والرابط</h3>
            <label class="upload-zone">
              <input
                type="file"
                accept="image/*,video/mp4,video/webm,video/ogg"
                :multiple="form.type === 'slider'"
                @change="onPickFile"
              />
              <Icon name="mdi:cloud-upload-outline" />
              <b>{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع الصور أو الفيديو' }}</b>
              <small>{{ form.type === 'popup' ? 'اختياري: المنبثق يمكن أن يكون نصاً فقط' : 'الصورة أو الفيديو مطلوبة للسلايدر والبانر' }}</small>
            </label>

            <label class="field">
              <span>رابط صورة أو فيديو يدوي</span>
              <input v-model="form.imageUrl" class="admin-input keep-ltr" placeholder="https://..." />
            </label>

            <label class="field">
              <span>الرابط عند الضغط</span>
              <input v-model="form.linkUrl" class="admin-input keep-ltr" placeholder="/products" />
            </label>

            <label class="enable-row">
              <input v-model="form.isEnabled" type="checkbox" />
              <span>الإعلان مفعل ويظهر للزوار</span>
            </label>

            <div class="preview-box">
              <div class="preview-media">
                <template v-if="previewImages.length">
                  <div v-for="(img, idx) in previewImages" :key="`${img}-${idx}`" class="preview-item">
                    <video v-if="isVideoUrl(img)" :src="api.buildAssetUrl(img)" muted playsinline controls />
                    <img v-else :src="api.buildAssetUrl(img)" alt="" />
                    <button type="button" @click="removePreview(idx)">×</button>
                  </div>
                </template>
                <div v-else class="preview-empty">
                  <Icon :name="form.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-outline'" />
                  <span>{{ form.type === 'popup' ? 'منبثق نصي بدون صورة' : 'لا توجد صورة بعد' }}</span>
                </div>
              </div>
              <div>
                <p class="ads-eyebrow">معاينة</p>
                <h3>{{ form.title || 'عنوان الإعلان' }}</h3>
                <p>{{ form.subtitle || placementLabel(form.placement) }}</p>
              </div>
            </div>
          </div>
        </div>

        <div class="save-actions">
          <button type="button" class="admin-secondary" @click="startNewAd">تفريغ</button>
          <UiButton type="submit" :disabled="saving || uploading">
            {{ saving ? 'جاري الحفظ...' : (editingId ? 'حفظ التعديل' : 'إنشاء الإعلان') }}
          </UiButton>
        </div>
      </form>

      <aside class="ads-list-panel admin-panel">
        <div class="list-head">
          <div>
            <p class="ads-eyebrow">الإعلانات المرفوعة</p>
            <h2>قائمة الإعلانات</h2>
            <p>أي إعلان محفوظ يظهر هنا فوراً ويمكن تعديله أو تعطيله أو حذفه.</p>
          </div>
          <select v-model="filterType" class="admin-input">
            <option value="all">كل الإعلانات</option>
            <option value="active">المفعلة فقط</option>
            <option value="disabled">المعطلة فقط</option>
            <option value="slider">سلايدر</option>
            <option value="banner">بانر</option>
            <option value="popup">منبثق</option>
            <option value="product">داخل منتج</option>
          </select>
        </div>

        <div v-if="loadError" class="error-box">
          {{ loadError }}
          <button type="button" class="admin-secondary" @click="load">إعادة المحاولة</button>
        </div>

        <div v-else-if="loading" class="empty-box">جاري تحميل الإعلانات...</div>

        <div v-else-if="!items.length" class="empty-box">
          لا توجد إعلانات محفوظة حالياً. أنشئ إعلاناً وسيظهر هنا مباشرة.
        </div>

        <div v-else-if="!visibleAds.length" class="empty-box">
          لا توجد نتائج لهذا الفلتر. عدد الإعلانات الكلي: {{ items.length }}.
          <button type="button" class="admin-secondary" @click="filterType = 'all'">عرض الكل</button>
        </div>

        <div v-else class="ads-list">
          <article
            v-for="ad in visibleAds"
            :key="ad.id || `${ad.type}-${ad.placement}-${ad.title}-${ad.createdAt}`"
            class="ad-card"
            :class="{ fresh: lastSavedId && lastSavedId === ad.id }"
          >
            <button type="button" class="ad-thumb" @click="edit(ad)">
              <video v-if="isVideoUrl(primaryMedia(ad))" :src="api.buildAssetUrl(primaryMedia(ad))" muted playsinline />
              <img v-else-if="primaryMedia(ad)" :src="api.buildAssetUrl(primaryMedia(ad))" alt="" />
              <Icon v-else :name="ad.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-off-outline'" />
            </button>

            <div class="ad-body">
              <div class="ad-title-row">
                <b>{{ ad.title || 'إعلان بدون عنوان' }}</b>
                <span class="status" :class="ad.isEnabled ? 'on' : 'off'">{{ ad.isEnabled ? 'مفعل' : 'معطل' }}</span>
              </div>
              <p>{{ ad.subtitle || 'بدون وصف' }}</p>
              <div class="ad-tags">
                <span>{{ typeLabel(ad.type) }}</span>
                <span>{{ placementLabel(ad.placement) }}</span>
                <span v-if="ad.type === 'slider'">{{ ad.imageUrls.length }} ملفات</span>
              </div>
              <small class="keep-ltr">{{ ad.linkUrl || 'بدون رابط' }}</small>
            </div>

            <div class="ad-controls">
              <button type="button" class="admin-secondary" @click="edit(ad)">تعديل</button>
              <button type="button" class="admin-secondary" @click="toggleEnabled(ad)">{{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}</button>
              <button type="button" class="admin-danger" @click="remove(ad.id)">حذف</button>
            </div>
          </article>
        </div>
      </aside>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

import UiButton from '~/components/ui/UiButton.vue'

const toast = useToast()
const api = useApi()
const directUpload = useDirectAdminUpload()

const loading = ref(true)
const saving = ref(false)
const uploading = ref(false)
const items = ref<any[]>([])
const products = ref<any[]>([])
const productQuery = ref('')
const filterType = ref('all')
const editingId = ref<string | null>(null)
const lastSavedId = ref<string | null>(null)
const loadError = ref('')

const adTypes = [
  { value: 'slider', label: 'سلايدر', icon: 'mdi:view-carousel-outline', hint: 'عدة صور أو فيديو يظهر فوق الهيرو أو آخر الصفحة' },
  { value: 'banner', label: 'بانر', icon: 'mdi:image-outline', hint: 'إعلان ثابت بصورة أو فيديو في موضع محدد' },
  { value: 'popup', label: 'إعلان منبثق', icon: 'mdi:bell-ring-outline', hint: 'نافذة تظهر فوق الموقع ويمكن أن تكون نصاً فقط' },
  { value: 'product', label: 'داخل منتج', icon: 'mdi:package-variant-closed', hint: 'إعلان داخل صفحة منتج تختاره بالبحث' },
]

const allPlacements = [
  { value: 'home_hero_slider', label: 'سلايدر فوق الهيرو / بداية الصفحة', type: 'slider' },
  { value: 'home_top_slider', label: 'سلايدر أعلى الرئيسية', type: 'slider' },
  { value: 'home_bottom_slider', label: 'سلايدر آخر الرئيسية', type: 'slider' },
  { value: 'page_top_slider', label: 'سلايدر أعلى الصفحات', type: 'slider' },
  { value: 'page_bottom_slider', label: 'سلايدر آخر الصفحات', type: 'slider' },
  { value: 'home_hero_top', label: 'بانر فوق الهيرو / بداية الصفحة', type: 'banner' },
  { value: 'home_top', label: 'بانر أعلى الرئيسية', type: 'banner' },
  { value: 'home_middle', label: 'بانر منتصف الرئيسية', type: 'banner' },
  { value: 'home_bottom', label: 'بانر آخر الرئيسية', type: 'banner' },
  { value: 'page_top', label: 'بانر أعلى الصفحات', type: 'banner' },
  { value: 'page_bottom', label: 'بانر آخر الصفحات', type: 'banner' },
  { value: 'popup', label: 'منبثق عام لكل الموقع', type: 'popup' },
  { value: 'home_popup', label: 'منبثق في الصفحة الرئيسية فقط', type: 'popup' },
  { value: 'product_page', label: 'داخل صفحة المنتج', type: 'product' },
]

const form = reactive({
  type: 'slider',
  placement: 'home_hero_slider',
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

const placementOptions = computed(() => allPlacements.filter((x) => x.type === form.type))
const activeType = computed(() => adTypes.find((x) => x.value === form.type) || adTypes[0])

const previewImages = computed(() => {
  const arr = Array.isArray(form.imageUrls) ? form.imageUrls.filter(Boolean) : []
  if (arr.length) return arr
  return form.imageUrl ? [form.imageUrl] : []
})

const stats = computed(() => ({
  total: items.value.length,
  active: items.value.filter((x: any) => x.isEnabled).length,
  slider: items.value.filter((x: any) => x.type === 'slider').length,
  banner: items.value.filter((x: any) => x.type === 'banner').length,
  popup: items.value.filter((x: any) => x.type === 'popup').length,
}))

const visibleAds = computed(() => {
  const list = Array.isArray(items.value) ? items.value : []
  if (filterType.value === 'all') return list
  if (filterType.value === 'active') return list.filter((x: any) => x.isEnabled)
  if (filterType.value === 'disabled') return list.filter((x: any) => !x.isEnabled)
  return list.filter((x: any) => x.type === filterType.value)
})

const filteredProducts = computed(() => {
  const q = productQuery.value.trim().toLowerCase()
  const source = Array.isArray(products.value) ? products.value : []
  if (!q) return source.slice(0, 12)
  return source.filter((p: any) => `${productName(p)} ${p.slug || ''} ${productBrand(p)}`.toLowerCase().includes(q)).slice(0, 16)
})

function emitAdsChanged() {
  if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
}

function unwrapList(res: any): any[] {
  if (Array.isArray(res)) return res
  if (Array.isArray(res?.items)) return res.items
  if (Array.isArray(res?.data)) return res.data
  if (Array.isArray(res?.value)) return res.value
  if (Array.isArray(res?.$values)) return res.$values
  if (Array.isArray(res?.items?.$values)) return res.items.$values
  if (Array.isArray(res?.data?.$values)) return res.data.$values
  return []
}

function typeFromAny(v: any) {
  const raw = String(v ?? '').trim().toLowerCase()
  if (raw === '1') return 'popup'
  if (raw === '2') return 'banner'
  if (raw === '3') return 'product'
  if (raw === '4') return 'slider'
  if (raw.includes('popup')) return 'popup'
  if (raw.includes('banner')) return 'banner'
  if (raw.includes('product')) return 'product'
  if (raw.includes('slider') || raw.includes('carousel')) return 'slider'
  return raw || 'banner'
}

function normalizeImageUrls(ad: any): string[] {
  const direct = ad?.imageUrls ?? ad?.ImageUrls
  const primary = ad?.imageUrl ?? ad?.ImageUrl ?? ''

  if (Array.isArray(direct)) return direct.filter(Boolean).map(String)
  if (Array.isArray(direct?.$values)) return direct.$values.filter(Boolean).map(String)
  if (typeof direct === 'string' && direct.trim()) {
    try {
      const parsed = JSON.parse(direct)
      if (Array.isArray(parsed)) return parsed.filter(Boolean).map(String)
    } catch {}
  }
  return primary ? [String(primary)] : []
}

function normalizeAds(res: any) {
  return unwrapList(res)
    .map((ad: any) => {
      const imageUrls = normalizeImageUrls(ad)
      const id = ad?.id ?? ad?.Id ?? ad?.ID ?? ''
      return {
        ...ad,
        id,
        type: typeFromAny(ad?.type ?? ad?.Type),
        placement: String(ad?.placement ?? ad?.Placement ?? 'home_top').trim(),
        title: ad?.title ?? ad?.Title ?? '',
        subtitle: ad?.subtitle ?? ad?.Subtitle ?? '',
        imageUrl: ad?.imageUrl ?? ad?.ImageUrl ?? imageUrls[0] ?? '',
        imageUrls,
        linkUrl: ad?.linkUrl ?? ad?.LinkUrl ?? '',
        productId: ad?.productId ?? ad?.ProductId ?? '',
        sortOrder: Number(ad?.sortOrder ?? ad?.SortOrder ?? 0),
        isEnabled: (ad?.isEnabled ?? ad?.IsEnabled) !== false,
        createdAt: ad?.createdAt ?? ad?.CreatedAt ?? '',
        updatedAt: ad?.updatedAt ?? ad?.UpdatedAt ?? '',
      }
    })
    .filter((ad: any) => ad.id || ad.title || ad.imageUrl || ad.imageUrls.length)
}

function normalizeProducts(res: any) {
  return unwrapList(res)
}

function productName(p: any) {
  return p?.title || p?.Title || p?.name || p?.nameAr || 'منتج بدون اسم'
}
function productBrand(p: any) {
  return p?.brand || p?.Brand || p?.brandName || p?.brandSlug || ''
}
function productImage(p: any) {
  return p?.imageUrl || p?.ImageUrl || p?.primaryImageUrl || p?.thumbnailUrl || p?.images?.[0]?.url || p?.assets?.[0]?.url || ''
}
function primaryMedia(ad: any) {
  return String(ad?.imageUrl || ad?.imageUrls?.[0] || '')
}
function isVideoUrl(url: string) {
  return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '')
}
function typeLabel(type: string) {
  return adTypes.find((x) => x.value === type)?.label || type
}
function placementLabel(p: string) {
  return allPlacements.find((x) => x.value === p)?.label || p
}

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    const [adsRes, productsRes]: any[] = await Promise.all([
      $fetch('/api/bff/admin/ads', { query: { _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }),
      $fetch('/api/bff/admin/products', { query: { page: 1, pageSize: 300, _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }).catch(() => []),
    ])

    items.value = normalizeAds(adsRes)
    products.value = normalizeProducts(productsRes)
  } catch (e: any) {
    loadError.value = e?.data?.message || e?.message || 'تعذر تحميل الإعلانات'
    toast.error(loadError.value)
  } finally {
    loading.value = false
  }
}

function startNewAd() {
  editingId.value = null
  Object.assign(form, {
    type: 'slider',
    placement: 'home_hero_slider',
    title: '',
    subtitle: '',
    imageUrl: '',
    imageUrls: [],
    linkUrl: '/products',
    productId: '',
    productTitle: '',
    sortOrder: 0,
    isEnabled: true,
  })
  productQuery.value = ''
}

function edit(ad: any) {
  editingId.value = ad.id || null
  const found = products.value.find((p: any) => String(p.id || p.Id) === String(ad.productId))
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
  const id = p?.id || p?.Id
  form.productId = id
  form.productTitle = productName(p)
  form.linkUrl = `/product/${id}`
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

function upsertAd(raw: any) {
  const normalized = normalizeAds([raw])[0]
  if (!normalized) return false
  const idx = items.value.findIndex((x: any) => String(x.id) === String(normalized.id))
  if (idx >= 0) items.value.splice(idx, 1, normalized)
  else items.value.unshift(normalized)
  filterType.value = 'all'
  lastSavedId.value = normalized.id || null
  setTimeout(() => {
    if (lastSavedId.value === normalized.id) lastSavedId.value = null
  }, 3500)
  return true
}

async function saveAd() {
  if (form.type !== 'popup' && !previewImages.value.length) {
    toast.error('ارفع صورة أو فيديو واحد على الأقل')
    return
  }
  if (form.type === 'popup' && !String(form.title || form.subtitle || form.imageUrl || '').trim() && !previewImages.value.length) {
    toast.error('اكتب عنواناً أو وصفاً للإعلان المنبثق')
    return
  }
  if (form.type === 'product' && !form.productId) {
    toast.error('اختر المنتج من البحث أولاً')
    return
  }

  saving.value = true
  try {
    const body = payload()
    const saved: any = editingId.value
      ? await $fetch(`/api/bff/admin/ads/${editingId.value}`, { method: 'PUT', body })
      : await $fetch('/api/bff/admin/ads', { method: 'POST', body })

    const inserted = upsertAd(saved)
    await load()
    if (inserted && saved) upsertAd(saved)

    emitAdsChanged()
    toast.success(editingId.value ? 'تم تحديث الإعلان وظهر في القائمة' : 'تم إنشاء الإعلان وظهر في القائمة')
    startNewAd()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حفظ الإعلان')
  } finally {
    saving.value = false
  }
}

async function remove(id: string) {
  if (!id || !confirm('حذف الإعلان؟')) return
  try {
    await $fetch(`/api/bff/admin/ads/${id}`, { method: 'DELETE' })
    items.value = items.value.filter((x: any) => String(x.id) !== String(id))
    emitAdsChanged()
    toast.success('تم حذف الإعلان')
  } catch {
    toast.error('تعذر الحذف')
  }
}

async function removeAll() {
  if (!confirm('حذف كل الإعلانات الحالية؟')) return
  try {
    await $fetch('/api/bff/admin/ads', { method: 'DELETE' })
    items.value = []
    emitAdsChanged()
    toast.success('تم حذف جميع الإعلانات')
  } catch {
    toast.error('تعذر حذف الكل')
  }
}

async function toggleEnabled(ad: any) {
  try {
    const updated: any = await $fetch(`/api/bff/admin/ads/${ad.id}`, {
      method: 'PUT',
      body: {
        ...ad,
        type: ad.type,
        placement: ad.placement,
        imageUrls: ad.imageUrls || (ad.imageUrl ? [ad.imageUrl] : []),
        isEnabled: !ad.isEnabled,
        startAt: null,
        endAt: null,
      },
    })
    upsertAd(updated)
    await load()
    emitAdsChanged()
  } catch {
    toast.error('تعذر تحديث الإعلان')
  }
}

async function onPickFile(e: Event) {
  const input = e.target as HTMLInputElement
  const files = Array.from(input.files || [])
  if (!files.length) return
  uploading.value = true
  try {
    const uploaded: string[] = []
    for (const file of files) {
      const url = await directUpload.upload('admin/ads/upload', file, { maxMb: 150, fallbackToBff: true })
      if (url) uploaded.push(url)
    }

    if (form.type === 'slider') {
      form.imageUrls = [...form.imageUrls, ...uploaded]
      form.imageUrl = form.imageUrls[0] || ''
    } else {
      form.imageUrl = uploaded[0] || form.imageUrl
      form.imageUrls = form.imageUrl ? [form.imageUrl] : []
    }
    toast.success('تم رفع الملف')
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر رفع الملف')
  } finally {
    uploading.value = false
    input.value = ''
  }
}

function removePreview(idx: number) {
  const arr = [...previewImages.value]
  arr.splice(idx, 1)
  form.imageUrls = arr
  form.imageUrl = arr[0] || ''
}

watch(() => form.imageUrl, (v) => {
  if (v && !form.imageUrls.length) form.imageUrls = [v]
})

watch(() => form.type, () => {
  if (!placementOptions.value.some((p) => p.value === form.placement)) {
    form.placement = placementOptions.value[0]?.value || 'home_top'
  }
})

await load()
</script>

<style scoped>
.ads-admin { display: grid; gap: 1.15rem; padding-bottom: 3rem; }
.ads-header,
.ad-editor,
.ads-list-panel,
.preview-box { border-radius: 30px; border: 1px solid rgba(var(--border), .82); background: radial-gradient(circle at top right, rgba(var(--primary), .12), transparent 34%), rgba(var(--surface-rgb), .9); box-shadow: 0 24px 80px rgba(0, 0, 0, .18); }
.ads-header { display: flex; justify-content: space-between; gap: 1rem; align-items: flex-start; padding: 1.35rem; }
.ads-eyebrow { margin: 0 0 .35rem; color: rgb(var(--primary)); font-size: .75rem; font-weight: 1000; }
.ads-header h1,
.editor-title h2,
.list-head h2 { margin: 0; color: rgb(var(--text)); font-size: clamp(1.35rem, 2.4vw, 2.4rem); font-weight: 1000; letter-spacing: -.04em; }
.ads-header p,
.editor-title p,
.list-head p,
.preview-box p { margin-top: .35rem; color: rgb(var(--muted)); line-height: 1.8; }
.header-actions { display: flex; flex-wrap: wrap; gap: .6rem; }
.header-actions button,
.save-actions button,
.ad-controls button,
.error-box button,
.empty-box button { border-radius: 999px; padding: .72rem 1rem; font-weight: 900; }
.stats-grid { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: .75rem; }
.stat-card { border: 1px solid rgba(var(--border), .72); background: rgba(var(--surface-rgb), .76); border-radius: 24px; padding: 1rem; }
.stat-card b { display: block; color: rgb(var(--text)); font-size: 1.8rem; line-height: 1; }
.stat-card span { display: block; margin-top: .45rem; color: rgb(var(--muted)); font-size: .85rem; font-weight: 800; }
.ads-layout { display: grid; grid-template-columns: minmax(420px, 620px) minmax(420px, 1fr); gap: 1rem; align-items: start; }
.ad-editor,
.ads-list-panel { padding: 1.1rem; }
.editor-title,
.list-head { display: flex; justify-content: space-between; gap: 1rem; align-items: flex-start; margin-bottom: 1rem; }
.type-badge,
.status,
.ad-tags span { display: inline-flex; align-items: center; border: 1px solid rgba(var(--border), .8); background: rgba(var(--surface-2-rgb), .75); border-radius: 999px; padding: .35rem .65rem; color: rgb(var(--text)); font-size: .75rem; font-weight: 900; white-space: nowrap; }
.editor-step { display: grid; grid-template-columns: 44px 1fr; gap: .85rem; padding: 1rem 0; border-top: 1px solid rgba(var(--border), .52); }
.step-number { display: grid; place-items: center; width: 38px; height: 38px; border-radius: 15px; background: linear-gradient(135deg, rgb(var(--primary)), rgba(var(--primary), .64)); color: white; font-weight: 1000; }
.step-content { display: grid; gap: .85rem; }
.step-content h3 { margin: .35rem 0 0; color: rgb(var(--text)); font-size: 1rem; font-weight: 1000; }
.type-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .65rem; }
.type-card { text-align: right; border: 1px solid rgba(var(--border), .75); background: rgba(var(--surface-2-rgb), .66); border-radius: 22px; padding: .9rem; color: rgb(var(--text)); transition: .22s ease; }
.type-card:hover,
.type-card.active { transform: translateY(-2px); border-color: rgba(var(--primary), .72); background: rgba(var(--primary), .12); }
.type-card svg { width: 1.35rem; height: 1.35rem; color: rgb(var(--primary)); }
.type-card b,
.type-card small { display: block; }
.type-card small { margin-top: .35rem; color: rgb(var(--muted)); line-height: 1.5; }
.form-grid { display: grid; grid-template-columns: 1fr 145px; gap: .75rem; }
.field { display: grid; gap: .4rem; }
.field span,
.enable-row span { color: rgb(var(--text)); font-size: .85rem; font-weight: 900; }
.admin-input { width: 100%; border: 1px solid rgba(var(--border), .84); background: rgba(var(--surface-2-rgb), .76); color: rgb(var(--text)); border-radius: 18px; padding: .85rem 1rem; outline: none; }
.admin-input:focus { border-color: rgba(var(--primary), .72); box-shadow: 0 0 0 4px rgba(var(--primary), .12); }
.product-list { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .55rem; max-height: 280px; overflow: auto; }
.product-item { display: grid; grid-template-columns: 48px 1fr; gap: .6rem; align-items: center; text-align: right; border: 1px solid rgba(var(--border), .72); background: rgba(var(--surface-2-rgb), .63); color: rgb(var(--text)); border-radius: 18px; padding: .55rem; }
.product-item.active { border-color: rgb(var(--primary)); background: rgba(var(--primary), .13); }
.product-item img,
.product-empty { width: 48px; height: 48px; border-radius: 14px; object-fit: cover; background: rgba(var(--border), .22); display: grid; place-items: center; }
.product-item small { color: rgb(var(--muted)); }
.upload-zone { position: relative; display: grid; place-items: center; gap: .35rem; min-height: 142px; border: 1px dashed rgba(var(--primary), .56); background: rgba(var(--primary), .08); border-radius: 26px; color: rgb(var(--text)); text-align: center; cursor: pointer; }
.upload-zone svg { width: 2rem; height: 2rem; color: rgb(var(--primary)); }
.upload-zone small { color: rgb(var(--muted)); }
.upload-zone input { position: absolute; inset: 0; opacity: 0; cursor: pointer; }
.enable-row { display: inline-flex; align-items: center; gap: .55rem; }
.preview-box { display: grid; grid-template-columns: 190px 1fr; gap: .8rem; align-items: center; padding: .85rem; }
.preview-media { display: grid; grid-template-columns: repeat(auto-fit, minmax(80px, 1fr)); gap: .45rem; }
.preview-item { position: relative; overflow: hidden; border-radius: 18px; background: rgba(var(--surface-2-rgb), .85); min-height: 90px; }
.preview-item img,
.preview-item video { width: 100%; height: 110px; object-fit: cover; }
.preview-item button { position: absolute; top: .3rem; inset-inline-end: .3rem; width: 28px; height: 28px; border-radius: 999px; background: rgba(0, 0, 0, .65); color: white; font-weight: 1000; }
.preview-empty { display: grid; place-items: center; gap: .35rem; min-height: 110px; border: 1px dashed rgba(var(--border), .72); border-radius: 20px; color: rgb(var(--muted)); }
.preview-empty svg { width: 2rem; height: 2rem; }
.save-actions { position: sticky; bottom: .75rem; display: flex; justify-content: flex-end; gap: .65rem; margin-top: 1rem; padding: .75rem; border: 1px solid rgba(var(--border), .65); background: rgba(var(--surface-rgb), .9); backdrop-filter: blur(18px); border-radius: 24px; }
.ads-list-panel { position: sticky; top: 1rem; max-height: calc(100vh - 2rem); overflow: auto; }
.list-head { position: sticky; top: 0; z-index: 2; padding-bottom: .8rem; background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-rgb), .78)); backdrop-filter: blur(14px); }
.error-box,
.empty-box { display: grid; place-items: center; gap: .75rem; min-height: 180px; border: 1px dashed rgba(var(--border), .72); border-radius: 26px; color: rgb(var(--muted)); text-align: center; padding: 1rem; }
.ads-list { display: grid; gap: .75rem; }
.ad-card { display: grid; grid-template-columns: 90px 1fr auto; gap: .8rem; align-items: center; border: 1px solid rgba(var(--border), .78); background: rgba(var(--surface-2-rgb), .62); border-radius: 24px; padding: .75rem; transition: .25s ease; min-width: 0; }
.ad-card:hover,
.ad-card.fresh { border-color: rgba(var(--primary), .72); background: rgba(var(--primary), .1); transform: translateY(-2px); }
.ad-thumb { display: grid; place-items: center; width: 90px; height: 78px; overflow: hidden; border: 1px solid rgba(var(--border), .7); background: rgba(var(--surface-rgb), .85); border-radius: 18px; color: rgb(var(--primary)); }
.ad-thumb img,
.ad-thumb video { width: 100%; height: 100%; object-fit: cover; }
.ad-thumb svg { width: 1.9rem; height: 1.9rem; }
.ad-body { min-width: 0; }
.ad-title-row { display: flex; flex-wrap: wrap; gap: .45rem; align-items: center; }
.ad-title-row b { color: rgb(var(--text)); font-weight: 1000; }
.ad-body p { margin: .3rem 0; color: rgb(var(--muted)); font-size: .85rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.ad-tags { display: flex; flex-wrap: wrap; gap: .35rem; margin-bottom: .25rem; }
.status.on { border-color: rgba(34, 197, 94, .45); color: rgb(74, 222, 128); }
.status.off { border-color: rgba(239, 68, 68, .4); color: rgb(248, 113, 113); }
.ad-controls { display: flex; flex-direction: column; gap: .35rem; }
.keep-ltr { direction: ltr; unicode-bidi: plaintext; }
@media (max-width: 1180px) { .ads-layout { grid-template-columns: 1fr; } .ads-list-panel { position: relative; top: auto; max-height: none; } }
@media (max-width: 760px) { .ads-header, .editor-title, .list-head { flex-direction: column; } .stats-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } .form-grid, .type-grid, .product-list, .preview-box { grid-template-columns: 1fr; } .editor-step { grid-template-columns: 1fr; } .ad-card { grid-template-columns: 1fr; } .ad-thumb { width: 100%; height: 180px; } .ad-controls { flex-direction: row; flex-wrap: wrap; } }
</style>
