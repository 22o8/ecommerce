<template>
  <div class="ads-admin-page" dir="rtl">
    <section class="ads-header-card">
      <div>
        <p class="ads-eyebrow">مركز الإعلانات</p>
        <h1>إدارة الإعلانات</h1>
        <p>
          أنشئ إعلانك بخطوات واضحة. اختر النوع، حدّد مكان الظهور، ارفع الصورة أو الفيديو، ثم احفظ.
          أي إعلان محفوظ سيظهر فورًا في القائمة.
        </p>
      </div>

      <div class="ads-header-actions">
        <button type="button" class="ads-btn ghost" @click="loadAds" :disabled="loading">
          تحديث
        </button>
        <button type="button" class="ads-btn ghost" @click="resetForm">
          إعلان جديد
        </button>
        <button type="button" class="ads-btn danger" @click="deleteAllAds" :disabled="!ads.length || saving">
          حذف الكل
        </button>
      </div>
    </section>

    <section class="ads-help-card">
      <div class="help-title">
        <span>طريقة الاستخدام السريعة</span>
      </div>
      <ol>
        <li>اختر نوع الإعلان: سلايدر، بانر، إعلان منبثق، أو إعلان داخل منتج.</li>
        <li>اختر مكان الظهور المناسب. السلايدر فوق الهيرو هو أفضل مكان لصورة أو فيديو جمالي.</li>
        <li>ارفع صورة أو فيديو، أو ضع رابطًا مباشرًا من R2.</li>
        <li>اضغط حفظ. الإعلان سيظهر مباشرة في القائمة ويمكن تعديله أو تعطيله.</li>
      </ol>
    </section>

    <section class="ads-stats-grid">
      <article class="stat-card">
        <span>كل الإعلانات</span>
        <strong>{{ stats.total }}</strong>
      </article>
      <article class="stat-card">
        <span>مفعّلة</span>
        <strong>{{ stats.active }}</strong>
      </article>
      <article class="stat-card">
        <span>سلايدر</span>
        <strong>{{ stats.slider }}</strong>
      </article>
      <article class="stat-card">
        <span>بانر</span>
        <strong>{{ stats.banner }}</strong>
      </article>
      <article class="stat-card">
        <span>منبثقة</span>
        <strong>{{ stats.popup }}</strong>
      </article>
    </section>

    <section class="ads-workspace">
      <aside class="ads-list-panel">
        <div class="panel-heading">
          <div>
            <p class="ads-eyebrow">الإعلانات المحفوظة</p>
            <h2>قائمة الإعلانات</h2>
            <p>أي إعلان ينحفظ في قاعدة البيانات سيظهر هنا مباشرة.</p>
          </div>
        </div>

        <div class="ads-list-tools">
          <input
            v-model="search"
            class="ads-input"
            placeholder="ابحث بالعنوان، النوع، الموضع..."
          />
          <select v-model="filter" class="ads-input">
            <option value="all">كل الإعلانات</option>
            <option value="active">المفعلة فقط</option>
            <option value="disabled">المعطلة فقط</option>
            <option value="slider">سلايدر</option>
            <option value="banner">بانر</option>
            <option value="popup">منبثق</option>
            <option value="product">داخل منتج</option>
          </select>
        </div>

        <div v-if="loading" class="empty-state">
          جاري تحميل الإعلانات...
        </div>

        <div v-else-if="!ads.length" class="empty-state">
          <b>لا توجد إعلانات محفوظة بعد</b>
          <span>احفظ أول إعلان وسيظهر هنا مباشرة.</span>
        </div>

        <div v-else-if="!filteredAds.length" class="empty-state">
          <b>لا توجد نتائج حسب الفلتر الحالي</b>
          <span>عدد الإعلانات الكلي: {{ ads.length }}</span>
          <button type="button" class="ads-btn ghost" @click="filter = 'all'; search = ''">عرض الكل</button>
        </div>

        <div v-else class="ads-list">
          <article
            v-for="ad in filteredAds"
            :key="ad.id"
            class="ad-card"
            :class="{ selected: editingId === ad.id, fresh: lastSavedId === ad.id }"
          >
            <button type="button" class="ad-media" @click="editAd(ad)">
              <video
                v-if="isVideo(primaryMedia(ad))"
                :src="assetUrl(primaryMedia(ad))"
                muted
                playsinline
              />
              <img
                v-else-if="primaryMedia(ad)"
                :src="assetUrl(primaryMedia(ad))"
                alt=""
              />
              <span v-else class="media-placeholder">{{ shortType(ad.type) }}</span>
            </button>

            <div class="ad-info">
              <div class="ad-topline">
                <b>{{ ad.title || 'إعلان بدون عنوان' }}</b>
                <span :class="['status-pill', ad.isEnabled ? 'active' : 'off']">
                  {{ ad.isEnabled ? 'مفعل' : 'معطل' }}
                </span>
              </div>

              <p>{{ ad.subtitle || 'بدون وصف' }}</p>

              <div class="ad-tags">
                <span>{{ typeLabel(ad.type) }}</span>
                <span>{{ placementLabel(ad.placement) }}</span>
                <span>{{ mediaList(ad).length || 0 }} ملف</span>
              </div>

              <small v-if="ad.linkUrl" class="keep-ltr">{{ ad.linkUrl }}</small>
            </div>

            <div class="ad-actions">
              <button type="button" class="mini-btn" @click="editAd(ad)">تعديل</button>
              <button type="button" class="mini-btn" @click="toggleAd(ad)">
                {{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}
              </button>
              <button type="button" class="mini-btn danger" @click="deleteAd(ad)">حذف</button>
            </div>
          </article>
        </div>
      </aside>

      <form class="ads-editor-panel" @submit.prevent="saveAd">
        <div class="panel-heading editor-heading">
          <div>
            <p class="ads-eyebrow">محرر الإعلان</p>
            <h2>{{ editingId ? 'تعديل الإعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p>التصميم هنا مقسّم حتى يعرف الأدمن ماذا يفعل خطوة بخطوة.</p>
          </div>
          <span class="type-badge">{{ currentType.label }}</span>
        </div>

        <div class="step-box">
          <div class="step-number">1</div>
          <div class="step-content">
            <h3>نوع الإعلان</h3>
            <p>اختر شكل الإعلان الذي تريد إظهاره للزائر.</p>

            <div class="type-grid">
              <button
                v-for="type in adTypes"
                :key="type.value"
                type="button"
                class="type-card"
                :class="{ active: form.type === type.value }"
                @click="setType(type.value)"
              >
                <b>{{ type.label }}</b>
                <small>{{ type.hint }}</small>
              </button>
            </div>
          </div>
        </div>

        <div class="step-box">
          <div class="step-number">2</div>
          <div class="step-content">
            <h3>المكان والمحتوى</h3>
            <p>حدد أين يظهر الإعلان ثم اكتب عنواناً قصيراً وواضحاً.</p>

            <div class="form-row">
              <label>
                <span>موضع الظهور</span>
                <select v-model="form.placement" class="ads-input">
                  <option
                    v-for="place in placementOptions"
                    :key="place.value"
                    :value="place.value"
                  >
                    {{ place.label }}
                  </option>
                </select>
              </label>

              <label>
                <span>الترتيب</span>
                <input v-model.number="form.sortOrder" type="number" class="ads-input" />
              </label>
            </div>

            <label class="full-field">
              <span>العنوان</span>
              <input v-model="form.title" class="ads-input" placeholder="مثال: عروض العناية الكورية" />
            </label>

            <label class="full-field">
              <span>الوصف المختصر</span>
              <input v-model="form.subtitle" class="ads-input" placeholder="نص قصير يظهر تحت العنوان" />
            </label>

            <div v-if="form.type === 'product'" class="product-picker">
              <label class="full-field">
                <span>ابحث عن المنتج</span>
                <input v-model="productSearch" class="ads-input" placeholder="اكتب اسم المنتج أو البراند" />
              </label>

              <div class="product-results">
                <button
                  v-for="product in filteredProducts"
                  :key="product.id"
                  type="button"
                  class="product-result"
                  :class="{ active: form.productId === product.id }"
                  @click="selectProduct(product)"
                >
                  <img v-if="productImage(product)" :src="assetUrl(productImage(product))" alt="" />
                  <span v-else>{{ productName(product).slice(0, 1) }}</span>
                  <b>{{ productName(product) }}</b>
                  <small>{{ productBrand(product) }}</small>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="step-box">
          <div class="step-number">3</div>
          <div class="step-content">
            <h3>الصورة أو الفيديو والرابط</h3>
            <p>
              السلايدر والبانر يحتاجان صورة أو فيديو. الإعلان المنبثق يمكن أن يكون نصاً فقط.
            </p>

            <label class="upload-zone">
              <input
                type="file"
                accept="image/*,video/mp4,video/webm"
                :multiple="form.type === 'slider'"
                @change="onPickFile"
              />
              <b>{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع صورة أو فيديو' }}</b>
              <span>يفضل صور عريضة أو فيديو قصير خفيف.</span>
            </label>

            <label class="full-field">
              <span>رابط صورة/فيديو يدوي</span>
              <input v-model="form.imageUrl" class="ads-input keep-ltr" placeholder="https://..." @change="syncManualUrl" />
            </label>

            <label class="full-field">
              <span>الرابط عند الضغط</span>
              <input v-model="form.linkUrl" class="ads-input keep-ltr" placeholder="/products" />
            </label>

            <label class="toggle-field">
              <input v-model="form.isEnabled" type="checkbox" />
              <span>الإعلان مفعل ويظهر للزوار</span>
            </label>
          </div>
        </div>

        <section class="preview-box">
          <div>
            <p class="ads-eyebrow">معاينة قبل الحفظ</p>
            <h3>{{ form.title || 'عنوان الإعلان' }}</h3>
            <p>{{ form.subtitle || 'هنا يظهر وصف الإعلان للزائر.' }}</p>
            <div class="ad-tags">
              <span>{{ currentType.label }}</span>
              <span>{{ placementLabel(form.placement) }}</span>
            </div>
          </div>

          <div class="preview-media">
            <video
              v-if="isVideo(previewMedia)"
              :src="assetUrl(previewMedia)"
              muted
              controls
              playsinline
            />
            <img
              v-else-if="previewMedia"
              :src="assetUrl(previewMedia)"
              alt=""
            />
            <span v-else>لا توجد صورة أو فيديو للمعاينة</span>
          </div>
        </section>

        <div class="save-bar">
          <button type="button" class="ads-btn ghost" @click="resetForm">
            تفريغ الحقول
          </button>
          <button type="submit" class="ads-btn primary" :disabled="saving || uploading">
            {{ saving ? 'جاري الحفظ...' : editingId ? 'حفظ التعديل' : 'حفظ الإعلان' }}
          </button>
        </div>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

const toast = useToast()
const api = useApi()
const directUpload = useDirectAdminUpload()

type AdItem = {
  id: string
  type: string
  placement: string
  title: string
  subtitle: string
  imageUrl: string
  imageUrls: string[]
  linkUrl: string
  productId: string
  sortOrder: number
  isEnabled: boolean
  createdAt?: string | null
  updatedAt?: string | null
}

const loading = ref(false)
const saving = ref(false)
const uploading = ref(false)

const ads = ref<AdItem[]>([])
const products = ref<any[]>([])
const search = ref('')
const filter = ref('all')
const productSearch = ref('')
const editingId = ref<string | null>(null)
const lastSavedId = ref<string | null>(null)

const adTypes = [
  { value: 'slider', label: 'سلايدر', hint: 'صورة أو فيديو متحرك فوق الهيرو أو أعلى/آخر الصفحة' },
  { value: 'banner', label: 'بانر', hint: 'إعلان ثابت داخل الصفحة' },
  { value: 'popup', label: 'إعلان منبثق', hint: 'نافذة تظهر للزائر ويمكن أن تكون نصاً فقط' },
  { value: 'product', label: 'داخل منتج', hint: 'إعلان مربوط بمنتج محدد من البحث' },
]

const placements = [
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
  sortOrder: 0,
  isEnabled: true,
})

const currentType = computed(() => adTypes.find((x) => x.value === form.type) || adTypes[0])
const placementOptions = computed(() => placements.filter((x) => x.type === form.type))
const previewMedia = computed(() => mediaFromForm()[0] || '')

const stats = computed(() => ({
  total: ads.value.length,
  active: ads.value.filter((x) => x.isEnabled).length,
  slider: ads.value.filter((x) => x.type === 'slider').length,
  banner: ads.value.filter((x) => x.type === 'banner').length,
  popup: ads.value.filter((x) => x.type === 'popup').length,
}))

const filteredAds = computed(() => {
  const q = search.value.trim().toLowerCase()
  return ads.value.filter((ad) => {
    const matchesFilter =
      filter.value === 'all' ||
      (filter.value === 'active' && ad.isEnabled) ||
      (filter.value === 'disabled' && !ad.isEnabled) ||
      ad.type === filter.value

    if (!matchesFilter) return false
    if (!q) return true

    return [
      ad.title,
      ad.subtitle,
      ad.type,
      ad.placement,
      placementLabel(ad.placement),
      ad.linkUrl,
    ].filter(Boolean).join(' ').toLowerCase().includes(q)
  })
})

const filteredProducts = computed(() => {
  const q = productSearch.value.trim().toLowerCase()
  const list = Array.isArray(products.value) ? products.value : []
  if (!q) return list.slice(0, 10)
  return list
    .filter((p: any) => `${productName(p)} ${productBrand(p)} ${p?.slug || ''}`.toLowerCase().includes(q))
    .slice(0, 16)
})

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

function normalizeAds(res: any): AdItem[] {
  return unwrapList(res)
    .map((raw: any) => {
      const rawType = raw?.type ?? raw?.Type ?? 'banner'
      const type = typeof rawType === 'number'
        ? ['slider', 'banner', 'popup', 'product'][rawType] || 'banner'
        : String(rawType || 'banner').toLowerCase().trim()

      const rawUrls = raw?.imageUrls ?? raw?.ImageUrls ?? raw?.imageUrlsJson ?? raw?.ImageUrlsJson
      const imageUrls = Array.isArray(rawUrls)
        ? rawUrls
        : Array.isArray(rawUrls?.$values)
          ? rawUrls.$values
          : typeof rawUrls === 'string' && rawUrls.trim().startsWith('[')
            ? safeJsonArray(rawUrls)
            : []

      const imageUrl = raw?.imageUrl ?? raw?.ImageUrl ?? imageUrls[0] ?? ''

      return {
        id: String(raw?.id ?? raw?.Id ?? ''),
        type,
        placement: String(raw?.placement ?? raw?.Placement ?? 'home_top').trim(),
        title: raw?.title ?? raw?.Title ?? '',
        subtitle: raw?.subtitle ?? raw?.Subtitle ?? '',
        imageUrl,
        imageUrls: imageUrls.length ? imageUrls.filter(Boolean) : (imageUrl ? [imageUrl] : []),
        linkUrl: raw?.linkUrl ?? raw?.LinkUrl ?? '',
        productId: raw?.productId ?? raw?.ProductId ?? '',
        sortOrder: Number(raw?.sortOrder ?? raw?.SortOrder ?? 0),
        isEnabled: (raw?.isEnabled ?? raw?.IsEnabled) !== false,
        createdAt: raw?.createdAt ?? raw?.CreatedAt ?? null,
        updatedAt: raw?.updatedAt ?? raw?.UpdatedAt ?? null,
      }
    })
    .filter((x) => x.id)
}

function safeJsonArray(value: string) {
  try {
    const parsed = JSON.parse(value)
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

function normalizeProducts(res: any) {
  return unwrapList(res)
}

function assetUrl(url: string) {
  return api.buildAssetUrl(url || '')
}

function mediaFromForm() {
  const list = Array.isArray(form.imageUrls) ? form.imageUrls.filter(Boolean) : []
  if (list.length) return list
  return form.imageUrl ? [form.imageUrl] : []
}

function mediaList(ad: AdItem) {
  const list = Array.isArray(ad?.imageUrls) ? ad.imageUrls.filter(Boolean) : []
  if (list.length) return list
  return ad?.imageUrl ? [ad.imageUrl] : []
}

function primaryMedia(ad: AdItem) {
  return mediaList(ad)[0] || ''
}

function isVideo(url: string) {
  return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '')
}

function shortType(type: string) {
  if (type === 'slider') return 'S'
  if (type === 'banner') return 'B'
  if (type === 'popup') return 'P'
  return 'AD'
}

function typeLabel(type: string) {
  return adTypes.find((x) => x.value === type)?.label || type
}

function placementLabel(value: string) {
  return placements.find((x) => x.value === value)?.label || value || 'بدون موضع'
}

function productName(p: any) {
  return p?.title || p?.name || p?.nameAr || 'منتج بدون اسم'
}

function productBrand(p: any) {
  return p?.brand || p?.brandName || p?.brandSlug || ''
}

function productImage(p: any) {
  return p?.imageUrl || p?.primaryImageUrl || p?.thumbnailUrl || p?.images?.[0]?.url || p?.images?.[0]?.imageUrl || ''
}

function emitAdsChanged() {
  if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
}

async function loadAds() {
  loading.value = true
  try {
    const res = await api.get<any>('/admin/ads', { _ts: Date.now() }, { 'cache-control': 'no-cache' })
    ads.value = normalizeAds(res)
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر تحميل الإعلانات')
  } finally {
    loading.value = false
  }
}

async function loadProducts() {
  try {
    const res = await api.get<any>('/admin/products', { page: 1, pageSize: 300, _ts: Date.now() }, { 'cache-control': 'no-cache' })
    products.value = normalizeProducts(res)
  } catch {
    products.value = []
  }
}

function resetForm() {
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
    sortOrder: 0,
    isEnabled: true,
  })
  productSearch.value = ''
}

function setType(type: string) {
  form.type = type
  const first = placements.find((x) => x.type === type)
  form.placement = first?.value || 'home_top'
  if (type === 'popup') {
    form.linkUrl = form.linkUrl || '/products'
  }
  if (type === 'product') {
    form.placement = 'product_page'
  }
}

function selectProduct(product: any) {
  form.productId = product.id
  form.linkUrl = `/product/${product.id}`
  productSearch.value = productName(product)
}

function syncManualUrl() {
  if (form.imageUrl) form.imageUrls = [form.imageUrl]
}

function editAd(ad: AdItem) {
  editingId.value = ad.id
  Object.assign(form, {
    type: ad.type || 'banner',
    placement: ad.placement || 'home_top',
    title: ad.title || '',
    subtitle: ad.subtitle || '',
    imageUrl: ad.imageUrl || '',
    imageUrls: mediaList(ad),
    linkUrl: ad.linkUrl || '/products',
    productId: ad.productId || '',
    sortOrder: Number(ad.sortOrder || 0),
    isEnabled: ad.isEnabled !== false,
  })
  const product = products.value.find((p: any) => p.id === ad.productId)
  productSearch.value = product ? productName(product) : ''
  if (process.client) window.scrollTo({ top: 0, behavior: 'smooth' })
}

function buildPayload() {
  const media = mediaFromForm()
  return {
    type: form.type,
    placement: form.placement,
    title: form.title || '',
    subtitle: form.subtitle || null,
    imageUrl: media[0] || '',
    imageUrls: media,
    linkUrl: form.linkUrl || null,
    productId: form.type === 'product' && form.productId ? form.productId : null,
    sortOrder: Number(form.sortOrder || 0),
    isEnabled: Boolean(form.isEnabled),
    startAt: null,
    endAt: null,
  }
}

function mergeAd(raw: any) {
  const ad = normalizeAds([raw])[0]
  if (!ad) return
  const index = ads.value.findIndex((x) => x.id === ad.id)
  if (index >= 0) ads.value.splice(index, 1, ad)
  else ads.value.unshift(ad)

  filter.value = 'all'
  search.value = ''
  lastSavedId.value = ad.id
  setTimeout(() => {
    if (lastSavedId.value === ad.id) lastSavedId.value = null
  }, 3500)
}

async function saveAd() {
  const media = mediaFromForm()
  if (form.type !== 'popup' && media.length === 0) {
    toast.error('ارفع صورة أو فيديو أولاً')
    return
  }
  if (form.type === 'popup' && !String(form.title || form.subtitle || media[0] || '').trim()) {
    toast.error('اكتب عنواناً أو وصفاً للإعلان المنبثق')
    return
  }
  if (form.type === 'product' && !form.productId) {
    toast.error('اختر المنتج المرتبط بالإعلان')
    return
  }

  saving.value = true
  try {
    const payload = buildPayload()
    const saved = editingId.value
      ? await api.put<any>(`/admin/ads/${editingId.value}`, payload)
      : await api.post<any>('/admin/ads', payload)

    mergeAd(saved)
    await loadAds()
    emitAdsChanged()
    toast.success(editingId.value ? 'تم تحديث الإعلان' : 'تم حفظ الإعلان وظهر في القائمة')
    resetForm()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حفظ الإعلان')
  } finally {
    saving.value = false
  }
}

async function onPickFile(event: Event) {
  const input = event.target as HTMLInputElement
  const files = Array.from(input.files || [])
  if (!files.length) return

  uploading.value = true
  try {
    const urls: string[] = []
    for (const file of files) {
      const url = await directUpload.upload('admin/ads/upload', file, {
        maxMb: 150,
        fallbackToBff: true,
      })
      if (url) urls.push(url)
    }

    if (form.type === 'slider') {
      form.imageUrls = [...form.imageUrls, ...urls]
      form.imageUrl = form.imageUrls[0] || ''
    } else {
      form.imageUrl = urls[0] || form.imageUrl
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

async function toggleAd(ad: AdItem) {
  try {
    const payload = {
      type: ad.type,
      placement: ad.placement,
      title: ad.title,
      subtitle: ad.subtitle || null,
      imageUrl: primaryMedia(ad),
      imageUrls: mediaList(ad),
      linkUrl: ad.linkUrl || null,
      productId: ad.type === 'product' && ad.productId ? ad.productId : null,
      sortOrder: Number(ad.sortOrder || 0),
      isEnabled: !ad.isEnabled,
      startAt: null,
      endAt: null,
    }

    const saved = await api.put<any>(`/admin/ads/${ad.id}`, payload)
    mergeAd(saved)
    emitAdsChanged()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر تغيير حالة الإعلان')
  }
}

async function deleteAd(ad: AdItem) {
  if (!confirm('هل تريد حذف هذا الإعلان؟')) return
  try {
    await api.del(`/admin/ads/${ad.id}`)
    ads.value = ads.value.filter((x) => x.id !== ad.id)
    if (editingId.value === ad.id) resetForm()
    emitAdsChanged()
    toast.success('تم حذف الإعلان')
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حذف الإعلان')
  }
}

async function deleteAllAds() {
  if (!confirm('حذف كل الإعلانات؟')) return
  try {
    await api.del('/admin/ads')
    ads.value = []
    resetForm()
    emitAdsChanged()
    toast.success('تم حذف كل الإعلانات')
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حذف الإعلانات')
  }
}

onMounted(async () => {
  await Promise.all([loadAds(), loadProducts()])
})
</script>

<style scoped>
.ads-admin-page {
  display: grid;
  gap: 1.2rem;
  padding-bottom: 3rem;
}

.ads-header-card,
.ads-help-card,
.ads-list-panel,
.ads-editor-panel,
.preview-box {
  border: 1px solid rgba(var(--border), .76);
  background:
    radial-gradient(circle at top right, rgba(var(--primary), .12), transparent 35%),
    rgba(var(--surface-rgb), .9);
  border-radius: 30px;
  box-shadow: 0 22px 80px rgba(0, 0, 0, .16);
}

.ads-header-card {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.4rem;
}

.ads-eyebrow {
  margin: 0 0 .4rem;
  color: rgb(var(--primary));
  font-size: .75rem;
  font-weight: 1000;
}

.ads-header-card h1,
.panel-heading h2 {
  margin: 0;
  color: rgb(var(--text));
  font-size: clamp(1.45rem, 2.4vw, 2.6rem);
  font-weight: 1000;
  letter-spacing: -.04em;
}

.ads-header-card p,
.panel-heading p,
.ads-help-card,
.step-content p,
.preview-box p {
  color: rgb(var(--muted));
  line-height: 1.8;
}

.ads-header-actions {
  display: flex;
  flex-wrap: wrap;
  gap: .6rem;
  align-items: flex-start;
}

.ads-btn,
.mini-btn {
  border: 1px solid rgba(var(--border), .8);
  background: rgba(var(--surface-2-rgb), .78);
  color: rgb(var(--text));
  border-radius: 999px;
  padding: .72rem 1rem;
  font-weight: 900;
  cursor: pointer;
  transition: .2s ease;
}

.ads-btn:hover,
.mini-btn:hover {
  transform: translateY(-1px);
  border-color: rgba(var(--primary), .75);
}

.ads-btn.primary {
  background: rgb(var(--primary));
  color: rgb(var(--on-primary));
  border-color: transparent;
}

.ads-btn.danger,
.mini-btn.danger {
  border-color: rgba(239, 68, 68, .45);
  color: rgb(248, 113, 113);
  background: rgba(239, 68, 68, .1);
}

.ads-help-card {
  padding: 1rem 1.2rem;
}

.help-title {
  color: rgb(var(--text));
  font-weight: 1000;
  margin-bottom: .65rem;
}

.ads-help-card ol {
  margin: 0;
  padding-inline-start: 1.25rem;
  display: grid;
  gap: .45rem;
}

.ads-stats-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: .8rem;
}

.stat-card {
  border: 1px solid rgba(var(--border), .7);
  background: rgba(var(--surface-rgb), .72);
  border-radius: 22px;
  padding: 1rem;
}

.stat-card span {
  color: rgb(var(--muted));
  font-weight: 800;
}

.stat-card strong {
  display: block;
  margin-top: .35rem;
  color: rgb(var(--text));
  font-size: 2rem;
  line-height: 1;
}

.ads-workspace {
  display: grid;
  grid-template-columns: minmax(420px, 1fr) minmax(460px, 640px);
  gap: 1rem;
  align-items: start;
}

.ads-list-panel,
.ads-editor-panel {
  padding: 1.15rem;
}

.panel-heading {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}

.ads-list-tools {
  display: grid;
  grid-template-columns: 1fr 170px;
  gap: .65rem;
  margin-bottom: 1rem;
}

.ads-input {
  width: 100%;
  border: 1px solid rgba(var(--border), .82);
  background: rgba(var(--surface-2-rgb), .74);
  color: rgb(var(--text));
  border-radius: 18px;
  padding: .85rem 1rem;
  outline: none;
}

.ads-input:focus {
  border-color: rgba(var(--primary), .75);
  box-shadow: 0 0 0 4px rgba(var(--primary), .12);
}

.empty-state {
  min-height: 170px;
  border: 1px dashed rgba(var(--border), .75);
  border-radius: 24px;
  display: grid;
  place-items: center;
  gap: .45rem;
  text-align: center;
  color: rgb(var(--muted));
  padding: 1rem;
}

.ads-list {
  display: grid;
  gap: .8rem;
}

.ad-card {
  display: grid;
  grid-template-columns: 112px 1fr auto;
  gap: .85rem;
  align-items: center;
  border: 1px solid rgba(var(--border), .75);
  background: rgba(var(--surface-2-rgb), .66);
  border-radius: 24px;
  padding: .8rem;
  transition: .2s ease;
}

.ad-card:hover,
.ad-card.selected,
.ad-card.fresh {
  border-color: rgba(var(--primary), .75);
  background: rgba(var(--primary), .1);
  transform: translateY(-2px);
}

.ad-media {
  width: 112px;
  height: 88px;
  border: 1px solid rgba(var(--border), .7);
  background: rgba(var(--surface-rgb), .9);
  border-radius: 18px;
  overflow: hidden;
  display: grid;
  place-items: center;
  color: rgb(var(--primary));
  font-weight: 1000;
}

.ad-media img,
.ad-media video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.media-placeholder {
  width: 46px;
  height: 46px;
  border-radius: 16px;
  background: rgba(var(--primary), .14);
  display: grid;
  place-items: center;
}

.ad-info {
  min-width: 0;
}

.ad-topline {
  display: flex;
  flex-wrap: wrap;
  gap: .45rem;
  align-items: center;
}

.ad-topline b {
  color: rgb(var(--text));
  font-weight: 1000;
}

.ad-info p {
  margin: .3rem 0;
  color: rgb(var(--muted));
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ad-tags {
  display: flex;
  flex-wrap: wrap;
  gap: .35rem;
  margin: .45rem 0;
}

.ad-tags span,
.status-pill,
.type-badge {
  display: inline-flex;
  align-items: center;
  border: 1px solid rgba(var(--border), .78);
  background: rgba(var(--surface-rgb), .72);
  border-radius: 999px;
  padding: .28rem .58rem;
  color: rgb(var(--text));
  font-size: .75rem;
  font-weight: 900;
}

.status-pill.active {
  color: rgb(74, 222, 128);
  border-color: rgba(34, 197, 94, .45);
}

.status-pill.off {
  color: rgb(248, 113, 113);
  border-color: rgba(239, 68, 68, .45);
}

.ad-actions {
  display: grid;
  gap: .35rem;
}

.step-box {
  display: grid;
  grid-template-columns: 46px 1fr;
  gap: .9rem;
  padding: 1rem 0;
  border-top: 1px solid rgba(var(--border), .58);
}

.step-number {
  width: 42px;
  height: 42px;
  border-radius: 16px;
  display: grid;
  place-items: center;
  background: linear-gradient(135deg, rgb(var(--primary)), rgba(var(--primary), .65));
  color: white;
  font-weight: 1000;
}

.step-content {
  display: grid;
  gap: .9rem;
}

.step-content h3 {
  margin: .2rem 0 0;
  color: rgb(var(--text));
  font-size: 1rem;
  font-weight: 1000;
}

.type-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .75rem;
}

.type-card {
  text-align: start;
  border: 1px solid rgba(var(--border), .78);
  background: rgba(var(--surface-2-rgb), .68);
  color: rgb(var(--text));
  border-radius: 22px;
  padding: 1rem;
  display: grid;
  gap: .35rem;
  cursor: pointer;
  transition: .2s ease;
}

.type-card small {
  color: rgb(var(--muted));
  line-height: 1.6;
}

.type-card.active,
.type-card:hover {
  border-color: rgba(var(--primary), .8);
  background: rgba(var(--primary), .12);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 130px;
  gap: .75rem;
}

label span {
  display: block;
  color: rgb(var(--text));
  font-size: .86rem;
  font-weight: 900;
  margin-bottom: .35rem;
}

.full-field,
.toggle-field {
  display: block;
}

.toggle-field {
  display: inline-flex;
  align-items: center;
  gap: .55rem;
  color: rgb(var(--text));
  font-weight: 900;
}

.upload-zone {
  position: relative;
  min-height: 118px;
  border: 1px dashed rgba(var(--primary), .55);
  background: rgba(var(--primary), .08);
  border-radius: 24px;
  display: grid;
  place-items: center;
  text-align: center;
  gap: .3rem;
  color: rgb(var(--text));
  cursor: pointer;
}

.upload-zone input {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
}

.upload-zone span {
  color: rgb(var(--muted));
}

.product-results {
  display: grid;
  gap: .5rem;
  max-height: 250px;
  overflow: auto;
}

.product-result {
  display: grid;
  grid-template-columns: 44px 1fr auto;
  gap: .65rem;
  align-items: center;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-2-rgb), .65);
  color: rgb(var(--text));
  border-radius: 18px;
  padding: .55rem;
}

.product-result.active,
.product-result:hover {
  border-color: rgba(var(--primary), .75);
}

.product-result img,
.product-result > span:first-child {
  width: 44px;
  height: 44px;
  border-radius: 14px;
  object-fit: cover;
  background: rgba(var(--primary), .12);
  display: grid;
  place-items: center;
  font-weight: 1000;
}

.product-result small {
  color: rgb(var(--muted));
}

.preview-box {
  padding: 1rem;
  display: grid;
  grid-template-columns: 1fr 220px;
  gap: 1rem;
  align-items: center;
}

.preview-box h3 {
  color: rgb(var(--text));
  margin: 0;
}

.preview-media {
  min-height: 130px;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-2-rgb), .68);
  border-radius: 22px;
  display: grid;
  place-items: center;
  overflow: hidden;
  color: rgb(var(--muted));
  text-align: center;
  padding: .75rem;
}

.preview-media img,
.preview-media video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.save-bar {
  position: sticky;
  bottom: 1rem;
  z-index: 5;
  display: flex;
  justify-content: flex-end;
  gap: .65rem;
  padding: .8rem;
  margin-top: 1rem;
  border: 1px solid rgba(var(--border), .7);
  background: rgba(var(--surface-rgb), .86);
  backdrop-filter: blur(16px);
  border-radius: 24px;
}

.keep-ltr {
  direction: ltr;
  unicode-bidi: plaintext;
}

@media (max-width: 1180px) {
  .ads-workspace {
    grid-template-columns: 1fr;
  }

  .ads-list-panel {
    order: 2;
  }

  .ads-editor-panel {
    order: 1;
  }
}

@media (max-width: 760px) {
  .ads-header-card {
    display: grid;
  }

  .ads-header-actions,
  .save-bar {
    justify-content: stretch;
  }

  .ads-header-actions .ads-btn,
  .save-bar .ads-btn {
    flex: 1;
  }

  .ads-stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .ads-list-tools,
  .form-row,
  .type-grid,
  .preview-box {
    grid-template-columns: 1fr;
  }

  .ad-card {
    grid-template-columns: 92px 1fr;
  }

  .ad-media {
    width: 92px;
    height: 76px;
  }

  .ad-actions {
    grid-column: 1 / -1;
    grid-template-columns: repeat(3, 1fr);
  }

  .step-box {
    grid-template-columns: 1fr;
  }
}
</style>
