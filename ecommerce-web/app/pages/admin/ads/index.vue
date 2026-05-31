<template>
  <div class="ads-admin-page" dir="rtl">
    <section class="page-hero">
      <div>
        <p class="eyebrow">مركز الإعلانات</p>
        <h1>إدارة الإعلانات</h1>
        <p class="lead">
          أنشئ إعلانك، ارفعه، ثم راقبه فوراً داخل القائمة. هذه الصفحة تعرض كل إعلان محفوظ من قاعدة البيانات بدون إخفاء أو فلترة خاطئة.
        </p>
      </div>

      <div class="hero-actions">
        <button type="button" class="btn ghost" @click="load">تحديث القائمة</button>
        <button type="button" class="btn ghost" @click="startNewAd">إعلان جديد</button>
        <button type="button" class="btn danger" @click="removeAll">حذف الكل</button>
      </div>
    </section>

    <section class="stats-grid">
      <article class="stat-card">
        <Icon name="mdi:bullhorn-outline" />
        <strong>{{ stats.total }}</strong>
        <span>كل الإعلانات</span>
      </article>
      <article class="stat-card">
        <Icon name="mdi:check-decagram-outline" />
        <strong>{{ stats.active }}</strong>
        <span>مفعلة</span>
      </article>
      <article class="stat-card">
        <Icon name="mdi:view-carousel-outline" />
        <strong>{{ stats.slider }}</strong>
        <span>سلايدر</span>
      </article>
      <article class="stat-card">
        <Icon name="mdi:image-outline" />
        <strong>{{ stats.banner }}</strong>
        <span>بانر</span>
      </article>
      <article class="stat-card">
        <Icon name="mdi:bell-ring-outline" />
        <strong>{{ stats.popup }}</strong>
        <span>منبثقة</span>
      </article>
    </section>

    <section class="ads-list-panel">
      <div class="panel-head">
        <div>
          <p class="eyebrow">الإعلانات المحفوظة</p>
          <h2>قائمة الإعلانات</h2>
          <p>أي إعلان تحفظه يظهر هنا مباشرة. اضغط على كرت الإعلان للتعديل.</p>
        </div>

        <div class="list-tools">
          <input v-model="search" class="control" placeholder="بحث بعنوان الإعلان أو الموضع..." />
          <select v-model="filterType" class="control">
            <option value="all">كل الإعلانات</option>
            <option value="active">المفعلة فقط</option>
            <option value="disabled">المعطلة فقط</option>
            <option value="slider">سلايدر</option>
            <option value="banner">بانر</option>
            <option value="popup">منبثق</option>
            <option value="product">داخل منتج</option>
          </select>
        </div>
      </div>

      <div v-if="loading" class="empty-state">جاري تحميل الإعلانات...</div>

      <div v-else-if="!items.length" class="empty-state strong-empty">
        <Icon name="mdi:advertisements-off" />
        <b>لا توجد إعلانات محفوظة بعد</b>
        <span>أنشئ إعلاناً من الأسفل، وبعد الحفظ سيظهر هنا مباشرة.</span>
      </div>

      <div v-else-if="!visibleAds.length" class="empty-state">
        لا توجد إعلانات تطابق البحث أو الفلتر الحالي.
        <button type="button" class="inline-reset" @click="resetListFilters">عرض كل الإعلانات</button>
      </div>

      <div v-else class="ads-cards-grid">
        <article
          v-for="ad in visibleAds"
          :key="ad.id || `${ad.type}-${ad.placement}-${ad.title}-${ad.updatedAt}`"
          class="ad-card"
          :class="{ fresh: lastSavedId && ad.id === lastSavedId }"
        >
          <button type="button" class="media-box" @click="edit(ad)">
            <video
              v-if="isVideoUrl(primaryMedia(ad))"
              :src="api.buildAssetUrl(primaryMedia(ad))"
              muted
              playsinline
            />
            <img
              v-else-if="primaryMedia(ad)"
              :src="api.buildAssetUrl(primaryMedia(ad))"
              alt=""
            />
            <div v-else class="no-media">
              <Icon :name="ad.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-off-outline'" />
              <span>{{ ad.type === 'popup' ? 'منبثق نصي' : 'بدون صورة' }}</span>
            </div>
          </button>

          <div class="ad-main-info">
            <div class="ad-title-row">
              <h3>{{ ad.title || 'إعلان بدون عنوان' }}</h3>
              <span class="state" :class="ad.isEnabled ? 'enabled' : 'disabled'">
                {{ ad.isEnabled ? 'مفعل' : 'معطل' }}
              </span>
            </div>
            <p>{{ ad.subtitle || 'لا يوجد وصف مختصر' }}</p>
            <div class="chips">
              <span>{{ typeLabel(ad.type) }}</span>
              <span>{{ placementLabel(ad.placement) }}</span>
              <span v-if="ad.type === 'slider'">ملفات: {{ mediaList(ad).length }}</span>
            </div>
            <small class="ltr-text">{{ ad.linkUrl || 'بدون رابط' }}</small>
          </div>

          <div class="card-actions">
            <button type="button" class="btn tiny ghost" @click="edit(ad)">تعديل</button>
            <button type="button" class="btn tiny ghost" @click="toggleEnabled(ad)">
              {{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}
            </button>
            <button type="button" class="btn tiny danger" @click="remove(ad.id)">حذف</button>
          </div>
        </article>
      </div>
    </section>

    <section class="editor-layout">
      <form class="editor-card" @submit.prevent="saveAd">
        <div class="editor-head">
          <div>
            <p class="eyebrow">محرر الإعلان</p>
            <h2>{{ editingId ? 'تعديل الإعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p>اختر النوع، ثم الموضع، ثم أضف المحتوى والملف.</p>
          </div>
          <span class="type-current">{{ activeType.label }}</span>
        </div>

        <div class="step-block">
          <span class="step-number">1</span>
          <div class="step-content">
            <h3>نوع الإعلان</h3>
            <div class="type-grid">
              <button
                v-for="type in adTypes"
                :key="type.value"
                type="button"
                class="type-choice"
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

        <div class="step-block">
          <span class="step-number">2</span>
          <div class="step-content">
            <h3>المكان والمحتوى</h3>
            <div class="form-grid two">
              <label>
                <span>موضع الظهور</span>
                <select v-model="form.placement" class="control">
                  <option v-for="p in placementOptions" :key="p.value" :value="p.value">
                    {{ p.label }}
                  </option>
                </select>
              </label>
              <label>
                <span>الترتيب</span>
                <input v-model.number="form.sortOrder" type="number" class="control" />
              </label>
            </div>

            <label>
              <span>العنوان</span>
              <input v-model="form.title" class="control" placeholder="مثال: عروض العناية الكورية" />
            </label>

            <label>
              <span>الوصف المختصر</span>
              <input v-model="form.subtitle" class="control" placeholder="نص قصير يظهر تحت العنوان" />
            </label>

            <div v-if="form.type === 'product'" class="product-picker">
              <label>
                <span>ابحث عن المنتج</span>
                <input v-model="productQuery" class="control" placeholder="اكتب اسم المنتج أو البراند..." />
              </label>

              <div class="product-grid">
                <button
                  v-for="p in filteredProducts"
                  :key="p.id"
                  type="button"
                  class="product-tile"
                  :class="{ active: form.productId === p.id }"
                  @click="selectProduct(p)"
                >
                  <img v-if="productImage(p)" :src="api.buildAssetUrl(productImage(p))" alt="" />
                  <span v-else class="product-fallback"><Icon name="mdi:package-variant-closed" /></span>
                  <b>{{ productName(p) }}</b>
                  <small>{{ productBrand(p) || 'بدون براند' }}</small>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="step-block">
          <span class="step-number">3</span>
          <div class="step-content">
            <h3>الصورة أو الفيديو والرابط</h3>
            <div class="upload-area">
              <Icon name="mdi:cloud-upload-outline" />
              <b>{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع الصورة أو الفيديو' }}</b>
              <small>{{ form.type === 'popup' ? 'اختياري للإعلان المنبثق' : 'مطلوب للبانر أو السلايدر' }}</small>
              <input type="file" accept="image/*,video/mp4,video/webm" :multiple="form.type === 'slider'" @change="onPickFile" />
            </div>

            <label>
              <span>رابط صورة أو فيديو يدوي</span>
              <input v-model="form.imageUrl" class="control ltr-text" placeholder="https://..." />
            </label>

            <label>
              <span>الرابط عند الضغط</span>
              <input v-model="form.linkUrl" class="control ltr-text" placeholder="/products" />
            </label>

            <label class="checkbox-row">
              <input v-model="form.isEnabled" type="checkbox" />
              <span>الإعلان مفعل ويظهر للزوار</span>
            </label>
          </div>
        </div>

        <section class="live-preview">
          <div>
            <p class="eyebrow">معاينة قبل الحفظ</p>
            <h3>{{ form.title || 'عنوان الإعلان' }}</h3>
            <p>{{ form.subtitle || 'هنا يظهر وصف الإعلان للزائر.' }}</p>
            <small>{{ activeType.label }} · {{ placementLabel(form.placement) }}</small>
          </div>
          <div class="preview-media">
            <template v-if="previewImages.length">
              <div v-for="(img, i) in previewImages" :key="`${img}-${i}`" class="preview-file">
                <video v-if="isVideoUrl(img)" :src="api.buildAssetUrl(img)" muted playsinline controls />
                <img v-else :src="api.buildAssetUrl(img)" alt="" />
                <button type="button" @click="removePreview(i)">×</button>
              </div>
            </template>
            <div v-else class="preview-empty">
              <Icon :name="form.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-outline'" />
              <span>{{ form.type === 'popup' ? 'المنبثق يمكن أن يكون نصاً فقط' : 'ارفع ملفاً أو ضع رابطاً للمعاينة' }}</span>
            </div>
          </div>
        </section>

        <div class="bottom-actions">
          <button type="button" class="btn ghost" @click="startNewAd">تفريغ الحقول</button>
          <UiButton type="submit" :disabled="saving || uploading">
            {{ saving ? 'جاري الحفظ...' : (editingId ? 'حفظ التعديل' : 'إنشاء الإعلان') }}
          </UiButton>
        </div>
      </form>
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
const search = ref('')
const editingId = ref<string | null>(null)
const lastSavedId = ref<string | null>(null)

const adTypes = [
  { value: 'slider', label: 'سلايدر', icon: 'mdi:view-carousel-outline', hint: 'عدة صور أو فيديو أعلى أو آخر الصفحة' },
  { value: 'banner', label: 'بانر', icon: 'mdi:image-outline', hint: 'إعلان ثابت داخل موضع واضح' },
  { value: 'popup', label: 'إعلان منبثق', icon: 'mdi:bell-ring-outline', hint: 'نافذة تظهر للزائر ويمكن أن تكون نصاً فقط' },
  { value: 'product', label: 'داخل منتج', icon: 'mdi:package-variant-closed', hint: 'إعلان مرتبط بمنتج محدد' },
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
  active: items.value.filter((x) => x.isEnabled).length,
  slider: items.value.filter((x) => x.type === 'slider').length,
  banner: items.value.filter((x) => x.type === 'banner').length,
  popup: items.value.filter((x) => x.type === 'popup').length,
}))

const visibleAds = computed(() => {
  let source = [...items.value]
  if (filterType.value === 'active') source = source.filter((x) => x.isEnabled)
  else if (filterType.value === 'disabled') source = source.filter((x) => !x.isEnabled)
  else if (filterType.value !== 'all') source = source.filter((x) => x.type === filterType.value)

  const q = search.value.trim().toLowerCase()
  if (q) {
    source = source.filter((ad) => `${ad.title || ''} ${ad.subtitle || ''} ${ad.type || ''} ${ad.placement || ''} ${placementLabel(ad.placement)}`.toLowerCase().includes(q))
  }

  return source
})

const filteredProducts = computed(() => {
  const q = productQuery.value.trim().toLowerCase()
  const source = products.value || []
  if (!q) return source.slice(0, 12)
  return source.filter((p: any) => `${productName(p)} ${p.slug || ''} ${productBrand(p)}`.toLowerCase().includes(q)).slice(0, 16)
})

function emitAdsChanged() {
  if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
}

function resetListFilters() {
  filterType.value = 'all'
  search.value = ''
}

function unwrapList(res: any): any[] {
  if (Array.isArray(res)) return res
  if (Array.isArray(res?.items)) return res.items
  if (Array.isArray(res?.data)) return res.data
  if (Array.isArray(res?.value)) return res.value
  if (Array.isArray(res?.$values)) return res.$values
  if (Array.isArray(res?.items?.$values)) return res.items.$values
  if (Array.isArray(res?.data?.$values)) return res.data.$values
  if (Array.isArray(res?.result)) return res.result
  if (Array.isArray(res?.results)) return res.results
  return []
}

function mediaList(ad: any) {
  if (Array.isArray(ad?.imageUrls)) return ad.imageUrls.filter(Boolean)
  if (Array.isArray(ad?.imageUrls?.$values)) return ad.imageUrls.$values.filter(Boolean)
  if (typeof ad?.imageUrlsJson === 'string') {
    try {
      const parsed = JSON.parse(ad.imageUrlsJson)
      if (Array.isArray(parsed)) return parsed.filter(Boolean)
    } catch {}
  }
  if (ad?.imageUrl) return [ad.imageUrl]
  return []
}

function normalizeAds(res: any) {
  return unwrapList(res).map((ad: any) => {
    const typeRaw = ad?.type ?? ad?.Type ?? 'banner'
    const placementRaw = ad?.placement ?? ad?.Placement ?? 'home_top'
    const type = typeof typeRaw === 'number'
      ? ['slider', 'banner', 'popup', 'product'][typeRaw] || 'banner'
      : String(typeRaw).trim().toLowerCase()

    const imageUrl = ad?.imageUrl ?? ad?.ImageUrl ?? ''
    const imageUrls = mediaList({ ...ad, imageUrl })

    return {
      ...ad,
      id: String(ad?.id ?? ad?.Id ?? ''),
      type,
      placement: String(placementRaw || 'home_top').trim(),
      title: ad?.title ?? ad?.Title ?? '',
      subtitle: ad?.subtitle ?? ad?.Subtitle ?? '',
      imageUrl,
      imageUrls,
      linkUrl: ad?.linkUrl ?? ad?.LinkUrl ?? '',
      productId: ad?.productId ?? ad?.ProductId ?? '',
      sortOrder: Number(ad?.sortOrder ?? ad?.SortOrder ?? 0),
      isEnabled: (ad?.isEnabled ?? ad?.IsEnabled) !== false,
      createdAt: ad?.createdAt ?? ad?.CreatedAt,
      updatedAt: ad?.updatedAt ?? ad?.UpdatedAt,
    }
  }).filter((ad: any) => ad.id || ad.title || ad.imageUrl || ad.imageUrls?.length)
}

function normalizeProducts(res: any) {
  return unwrapList(res)
}

function productName(p: any) { return p?.title || p?.name || p?.nameAr || 'منتج بدون اسم' }
function productBrand(p: any) { return p?.brand || p?.brandName || p?.brandSlug || '' }
function productImage(p: any) { return p?.imageUrl || p?.primaryImageUrl || p?.thumbnailUrl || p?.images?.[0]?.url || p?.assets?.[0]?.url || '' }
function primaryMedia(ad: any) { return String(ad?.imageUrl || mediaList(ad)[0] || '') }
function typeLabel(type: string) { return adTypes.find((x) => x.value === type)?.label || type }
function placementLabel(p: string) { return allPlacements.find((x) => x.value === p)?.label || p || 'بدون موضع' }
function isVideoUrl(url: string) { return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '') }

async function load() {
  loading.value = true
  try {
    const [adsRes, productsRes]: any[] = await Promise.all([
      $fetch('/api/bff/admin/ads', { query: { _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }),
      $fetch('/api/bff/admin/products', { query: { page: 1, pageSize: 300, _ts: Date.now() }, headers: { 'cache-control': 'no-cache' } }).catch(() => []),
    ])
    items.value = normalizeAds(adsRes)
    products.value = normalizeProducts(productsRes)
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر تحميل الإعلانات')
  } finally {
    loading.value = false
  }
}

function startNewAd() {
  editingId.value = null
  Object.assign(form, {
    type: 'slider', placement: 'home_hero_slider', title: '', subtitle: '', imageUrl: '', imageUrls: [],
    linkUrl: '/products', productId: '', productTitle: '', sortOrder: 0, isEnabled: true,
  })
  productQuery.value = ''
}

function edit(ad: any) {
  editingId.value = ad.id
  const found = products.value.find((p: any) => p.id === ad.productId)
  Object.assign(form, {
    type: ad.type || 'banner', placement: ad.placement || 'home_top', title: ad.title || '', subtitle: ad.subtitle || '',
    imageUrl: ad.imageUrl || '', imageUrls: mediaList(ad), linkUrl: ad.linkUrl || '/products', productId: ad.productId || '',
    productTitle: found ? productName(found) : '', sortOrder: Number(ad.sortOrder || 0), isEnabled: ad.isEnabled !== false,
  })
  productQuery.value = form.productTitle
  window?.scrollTo?.({ top: document.body.scrollHeight, behavior: 'smooth' })
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

function insertOrReplace(raw: any) {
  const normalized = normalizeAds([raw])[0]
  if (!normalized) return
  const idx = items.value.findIndex((x) => String(x.id) === String(normalized.id))
  if (idx >= 0) items.value.splice(idx, 1, normalized)
  else items.value.unshift(normalized)
  resetListFilters()
  lastSavedId.value = normalized.id
  setTimeout(() => { if (lastSavedId.value === normalized.id) lastSavedId.value = null }, 4000)
}

async function saveAd() {
  if (form.type !== 'popup' && !previewImages.value.length) return toast.error('ارفع صورة أو فيديو واحد على الأقل')
  if (form.type === 'popup' && !String(form.title || form.subtitle || form.imageUrl || '').trim() && !previewImages.value.length) return toast.error('اكتب عنواناً أو وصفاً للإعلان المنبثق')
  if (form.type === 'product' && !form.productId) return toast.error('اختر المنتج من البحث أولاً')

  saving.value = true
  try {
    const body = payload()
    const saved: any = editingId.value
      ? await $fetch(`/api/bff/admin/ads/${editingId.value}`, { method: 'PUT', body })
      : await $fetch('/api/bff/admin/ads', { method: 'POST', body })

    insertOrReplace(saved)
    await load()
    if (items.value.length) lastSavedId.value = String(saved?.id || saved?.Id || items.value[0]?.id || '')
    emitAdsChanged()
    toast.success(editingId.value ? 'تم تحديث الإعلان' : 'تم إنشاء الإعلان وظهر في القائمة')
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
    items.value = items.value.filter((x) => String(x.id) !== String(id))
    emitAdsChanged()
    toast.success('تم حذف الإعلان')
  } catch { toast.error('تعذر الحذف') }
}

async function removeAll() {
  if (!confirm('حذف كل الإعلانات الحالية؟')) return
  try {
    await $fetch('/api/bff/admin/ads', { method: 'DELETE' })
    items.value = []
    emitAdsChanged()
    toast.success('تم حذف جميع الإعلانات')
  } catch { toast.error('تعذر حذف الكل') }
}

async function toggleEnabled(ad: any) {
  try {
    const updated: any = await $fetch(`/api/bff/admin/ads/${ad.id}`, {
      method: 'PUT',
      body: { ...ad, imageUrls: mediaList(ad), isEnabled: !ad.isEnabled, startAt: null, endAt: null },
    })
    insertOrReplace(updated)
    await load()
    emitAdsChanged()
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
.ads-admin-page { display: grid; gap: 1.25rem; padding-bottom: 4rem; }
.page-hero, .ads-list-panel, .editor-card, .live-preview {
  border: 1px solid rgba(var(--border), .8);
  border-radius: 32px;
  background: radial-gradient(circle at top right, rgba(var(--primary), .12), transparent 34%), rgba(var(--surface-rgb), .9);
  box-shadow: 0 24px 80px rgba(0,0,0,.18);
}
.page-hero { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; padding: 1.4rem; }
.eyebrow { color: rgb(var(--primary)); font-size: .72rem; font-weight: 1000; margin: 0 0 .45rem; }
h1, h2, h3 { color: rgb(var(--text)); margin: 0; font-weight: 1000; letter-spacing: -.04em; }
h1 { font-size: clamp(1.6rem, 3vw, 2.7rem); }
h2 { font-size: clamp(1.25rem, 2vw, 2rem); }
.lead, .page-hero p, .panel-head p, .editor-head p, .live-preview p { color: rgb(var(--muted)); line-height: 1.8; margin-top: .45rem; }
.hero-actions, .bottom-actions, .card-actions { display: flex; flex-wrap: wrap; gap: .55rem; }
.btn { border: 1px solid rgba(var(--border), .8); border-radius: 999px; padding: .75rem 1rem; font-weight: 900; color: rgb(var(--text)); background: rgba(var(--surface-2-rgb), .72); }
.btn.ghost:hover { border-color: rgba(var(--primary), .65); background: rgba(var(--primary), .12); }
.btn.danger { color: rgb(248,113,113); border-color: rgba(239,68,68,.4); background: rgba(239,68,68,.1); }
.btn.tiny { padding: .48rem .7rem; font-size: .8rem; }
.stats-grid { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: .8rem; }
.stat-card { border: 1px solid rgba(var(--border), .7); border-radius: 24px; padding: 1rem; background: rgba(var(--surface-rgb), .72); }
.stat-card svg { color: rgb(var(--primary)); width: 1.15rem; height: 1.15rem; }
.stat-card strong { display: block; margin-top: .55rem; color: rgb(var(--text)); font-size: 2rem; line-height: 1; }
.stat-card span { display: block; color: rgb(var(--muted)); margin-top: .35rem; font-weight: 800; }
.ads-list-panel { padding: 1.2rem; }
.panel-head { display: grid; grid-template-columns: 1fr minmax(320px, 520px); gap: 1rem; align-items: start; margin-bottom: 1rem; }
.list-tools { display: grid; grid-template-columns: 1fr 170px; gap: .65rem; }
.control { width: 100%; border: 1px solid rgba(var(--border), .85); border-radius: 18px; padding: .85rem 1rem; background: rgba(var(--surface-2-rgb), .78); color: rgb(var(--text)); outline: none; }
.control:focus { border-color: rgba(var(--primary), .7); box-shadow: 0 0 0 4px rgba(var(--primary), .12); }
.empty-state { min-height: 170px; display: grid; place-items: center; text-align: center; padding: 1rem; border: 1px dashed rgba(var(--border), .7); border-radius: 24px; color: rgb(var(--muted)); gap: .4rem; }
.strong-empty svg { width: 2rem; height: 2rem; color: rgb(var(--primary)); }
.inline-reset { color: rgb(var(--primary)); font-weight: 900; }
.ads-cards-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(330px, 1fr)); gap: .85rem; }
.ad-card { display: grid; grid-template-columns: 110px 1fr; gap: .8rem; position: relative; border: 1px solid rgba(var(--border), .75); border-radius: 24px; padding: .75rem; background: rgba(var(--surface-2-rgb), .65); transition: .22s ease; }
.ad-card:hover, .ad-card.fresh { transform: translateY(-2px); border-color: rgba(var(--primary), .75); background: rgba(var(--primary), .1); }
.media-box { display: grid; place-items: center; width: 110px; height: 92px; overflow: hidden; border-radius: 18px; border: 1px solid rgba(var(--border), .7); background: rgba(var(--surface-rgb), .82); color: rgb(var(--primary)); }
.media-box img, .media-box video { width: 100%; height: 100%; object-fit: cover; }
.no-media { display: grid; place-items: center; gap: .2rem; font-size: .75rem; }
.no-media svg { width: 1.7rem; height: 1.7rem; }
.ad-main-info { min-width: 0; }
.ad-title-row { display: flex; flex-wrap: wrap; gap: .5rem; align-items: center; justify-content: space-between; }
.ad-title-row h3 { font-size: 1rem; }
.ad-main-info p { color: rgb(var(--muted)); margin: .35rem 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.chips { display: flex; flex-wrap: wrap; gap: .35rem; margin: .45rem 0; }
.chips span, .state, .type-current { display: inline-flex; align-items: center; border: 1px solid rgba(var(--border), .75); border-radius: 999px; padding: .28rem .55rem; font-size: .73rem; font-weight: 900; background: rgba(var(--surface-rgb), .8); color: rgb(var(--text)); }
.state.enabled { color: rgb(74,222,128); border-color: rgba(34,197,94,.45); }
.state.disabled { color: rgb(248,113,113); border-color: rgba(239,68,68,.42); }
.ltr-text { direction: ltr; unicode-bidi: plaintext; }
.card-actions { grid-column: 1 / -1; justify-content: flex-end; border-top: 1px solid rgba(var(--border), .45); padding-top: .65rem; }
.editor-layout { display: grid; grid-template-columns: 1fr; }
.editor-card { padding: 1.2rem; }
.editor-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; margin-bottom: 1rem; }
.step-block { display: grid; grid-template-columns: 46px 1fr; gap: .9rem; padding: 1rem 0; border-top: 1px solid rgba(var(--border), .55); }
.step-number { display: grid; place-items: center; width: 40px; height: 40px; border-radius: 16px; background: linear-gradient(135deg, rgb(var(--primary)), rgba(var(--primary), .65)); color: white; font-weight: 1000; }
.step-content { display: grid; gap: .9rem; }
.type-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: .7rem; }
.type-choice { text-align: right; border: 1px solid rgba(var(--border), .75); border-radius: 22px; padding: .85rem; background: rgba(var(--surface-2-rgb), .68); color: rgb(var(--text)); transition: .22s ease; }
.type-choice:hover, .type-choice.active { transform: translateY(-2px); border-color: rgba(var(--primary), .75); background: rgba(var(--primary), .12); }
.type-choice svg { color: rgb(var(--primary)); width: 1.35rem; height: 1.35rem; }
.type-choice b, .type-choice small { display: block; }
.type-choice small { color: rgb(var(--muted)); margin-top: .35rem; line-height: 1.5; }
.form-grid.two { display: grid; grid-template-columns: 1fr 160px; gap: .75rem; }
label { display: grid; gap: .4rem; }
label span { color: rgb(var(--text)); font-size: .86rem; font-weight: 900; }
.product-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: .55rem; max-height: 260px; overflow: auto; }
.product-tile { display: grid; grid-template-columns: 48px 1fr; gap: .55rem; align-items: center; border: 1px solid rgba(var(--border), .72); border-radius: 18px; padding: .55rem; background: rgba(var(--surface-2-rgb), .65); color: rgb(var(--text)); text-align: right; }
.product-tile.active { border-color: rgb(var(--primary)); background: rgba(var(--primary), .12); }
.product-tile img, .product-fallback { width: 48px; height: 48px; object-fit: cover; border-radius: 14px; background: rgba(var(--border), .22); display: grid; place-items: center; }
.product-tile small { color: rgb(var(--muted)); }
.upload-area { position: relative; min-height: 145px; display: grid; place-items: center; gap: .35rem; border: 1px dashed rgba(var(--primary), .55); border-radius: 26px; background: rgba(var(--primary), .08); color: rgb(var(--text)); text-align: center; }
.upload-area svg { width: 2rem; height: 2rem; color: rgb(var(--primary)); }
.upload-area small { color: rgb(var(--muted)); }
.upload-area input { position: absolute; inset: 0; opacity: 0; cursor: pointer; }
.checkbox-row { display: inline-flex; align-items: center; gap: .55rem; }
.live-preview { margin-top: 1rem; padding: 1rem; }
.live-preview h3 { font-size: 1.35rem; }
.preview-media { margin-top: 1rem; display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: .65rem; }
.preview-file { position: relative; overflow: hidden; border-radius: 20px; background: rgba(var(--surface-2-rgb), .8); min-height: 120px; }
.preview-file img, .preview-file video { width: 100%; height: 160px; object-fit: cover; }
.preview-file button { position: absolute; top: .45rem; inset-inline-end: .45rem; width: 30px; height: 30px; border-radius: 999px; background: rgba(0,0,0,.65); color: white; font-weight: 1000; }
.preview-empty { min-height: 135px; display: grid; place-items: center; gap: .45rem; border: 1px dashed rgba(var(--border), .7); border-radius: 22px; color: rgb(var(--muted)); }
.preview-empty svg { width: 2rem; height: 2rem; }
.bottom-actions { position: sticky; bottom: .75rem; justify-content: flex-end; margin-top: 1rem; padding: .75rem; border: 1px solid rgba(var(--border), .6); border-radius: 24px; background: rgba(var(--surface-rgb), .88); backdrop-filter: blur(18px); }
@media (max-width: 1100px) { .panel-head { grid-template-columns: 1fr; } .type-grid, .stats-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 720px) { .page-hero, .editor-head { flex-direction: column; } .list-tools, .form-grid.two, .step-block, .type-grid, .stats-grid, .ads-cards-grid { grid-template-columns: 1fr; } .ad-card { grid-template-columns: 1fr; } .media-box { width: 100%; height: 180px; } .card-actions { justify-content: stretch; } .card-actions .btn { flex: 1; } }
</style>
