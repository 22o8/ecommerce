<template>
  <div class="ads-admin" dir="rtl">
    <section class="ads-hero-card">
      <div>
        <p class="ads-kicker">مركز الإعلانات</p>
        <h1>إدارة الإعلانات</h1>
        <p>
          أنشئ إعلاناً، حدّد نوعه ومكان ظهوره، ثم احفظ. أي إعلان محفوظ يظهر مباشرة في القائمة ويمكن تعديله أو تعطيله أو حذفه.
        </p>
      </div>

      <div class="hero-actions">
        <button type="button" class="admin-secondary" @click="loadAll">تحديث</button>
        <button type="button" class="admin-secondary" @click="startNewAd">إعلان جديد</button>
        <button type="button" class="admin-danger" @click="removeAll">حذف الكل</button>
      </div>
    </section>

    <section class="ads-stats-grid">
      <article class="ads-stat">
        <Icon name="mdi:bullhorn-outline" />
        <strong>{{ stats.total }}</strong>
        <span>كل الإعلانات</span>
      </article>
      <article class="ads-stat">
        <Icon name="mdi:check-circle-outline" />
        <strong>{{ stats.active }}</strong>
        <span>مفعلة</span>
      </article>
      <article class="ads-stat">
        <Icon name="mdi:view-carousel-outline" />
        <strong>{{ stats.slider }}</strong>
        <span>سلايدر</span>
      </article>
      <article class="ads-stat">
        <Icon name="mdi:image-outline" />
        <strong>{{ stats.banner }}</strong>
        <span>بانر</span>
      </article>
      <article class="ads-stat">
        <Icon name="mdi:bell-ring-outline" />
        <strong>{{ stats.popup }}</strong>
        <span>منبثقة</span>
      </article>
    </section>

    <section class="ads-main-grid">
      <aside class="ads-list-panel">
        <div class="panel-head">
          <div>
            <p class="ads-kicker">الإعلانات المحفوظة</p>
            <h2>قائمة الإعلانات</h2>
            <p>القائمة تقرأ مباشرة من قاعدة البيانات. إذا كان الإعلان محفوظاً سيظهر هنا.</p>
          </div>
        </div>

        <div class="list-toolbar">
          <input
            v-model="search"
            class="admin-input"
            placeholder="ابحث بالعنوان، النوع، الموضع..."
          />
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

        <div v-if="loading" class="ads-empty">
          <Icon name="mdi:loading" />
          <span>جاري تحميل الإعلانات...</span>
        </div>

        <div v-else-if="!items.length" class="ads-empty">
          <Icon name="mdi:bullhorn-variant-outline" />
          <b>لا توجد إعلانات محفوظة بعد</b>
          <span>أنشئ إعلاناً من المحرر وسيظهر هنا مباشرة.</span>
        </div>

        <div v-else-if="!filteredAds.length" class="ads-empty">
          <Icon name="mdi:filter-off-outline" />
          <b>لا توجد نتائج لهذا الفلتر</b>
          <span>عدد الإعلانات الكلي: {{ items.length }}</span>
          <button type="button" class="admin-secondary" @click="filterType = 'all'; search = ''">عرض الكل</button>
        </div>

        <div v-else class="ads-list">
          <article
            v-for="ad in filteredAds"
            :key="ad.id"
            class="ad-card"
            :class="{ selected: editingId === ad.id, fresh: lastSavedId === ad.id }"
          >
            <button type="button" class="ad-preview" @click="editAd(ad)">
              <video
                v-if="isVideoUrl(primaryMedia(ad))"
                :src="assetUrl(primaryMedia(ad))"
                muted
                playsinline
              />
              <img
                v-else-if="primaryMedia(ad)"
                :src="assetUrl(primaryMedia(ad))"
                alt=""
              />
              <Icon v-else :name="ad.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-off-outline'" />
            </button>

            <div class="ad-content">
              <div class="ad-title-row">
                <b>{{ ad.title || 'إعلان بدون عنوان' }}</b>
                <span :class="['state-pill', ad.isEnabled ? 'on' : 'off']">
                  {{ ad.isEnabled ? 'مفعل' : 'معطل' }}
                </span>
              </div>

              <p>{{ ad.subtitle || 'بدون وصف' }}</p>

              <div class="ad-meta">
                <span>{{ typeLabel(ad.type) }}</span>
                <span>{{ placementLabel(ad.placement) }}</span>
                <span v-if="ad.type === 'slider'">{{ mediaList(ad).length }} ملف</span>
              </div>

              <small class="keep-ltr">{{ ad.linkUrl || 'بدون رابط' }}</small>
            </div>

            <div class="ad-actions">
              <button type="button" class="admin-secondary" @click="editAd(ad)">تعديل</button>
              <button type="button" class="admin-secondary" @click="toggleAd(ad)">
                {{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}
              </button>
              <button type="button" class="admin-danger" @click="removeAd(ad.id)">حذف</button>
            </div>
          </article>
        </div>
      </aside>

      <form class="ads-editor-panel" @submit.prevent="saveAd">
        <div class="panel-head editor-head">
          <div>
            <p class="ads-kicker">محرر الإعلان</p>
            <h2>{{ editingId ? 'تعديل إعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p>اتبع الخطوات: النوع، المكان، المحتوى، ثم الحفظ.</p>
          </div>
          <span class="type-badge">{{ currentType.label }}</span>
        </div>

        <div class="wizard-section">
          <div class="wizard-number">1</div>
          <div class="wizard-body">
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

        <div class="wizard-section">
          <div class="wizard-number">2</div>
          <div class="wizard-body">
            <h3>مكان الظهور والمحتوى</h3>

            <div class="form-grid">
              <label class="field">
                <span>موضع الظهور</span>
                <select v-model="form.placement" class="admin-input">
                  <option v-for="p in placementOptions" :key="p.value" :value="p.value">
                    {{ p.label }}
                  </option>
                </select>
              </label>

              <label class="field short">
                <span>الترتيب</span>
                <input v-model.number="form.sortOrder" type="number" class="admin-input" />
              </label>
            </div>

            <label class="field">
              <span>العنوان</span>
              <input v-model="form.title" class="admin-input" placeholder="مثال: عروض العناية الكورية" />
            </label>

            <label class="field">
              <span>الوصف المختصر</span>
              <input v-model="form.subtitle" class="admin-input" placeholder="نص قصير يظهر تحت العنوان" />
            </label>

            <div v-if="form.type === 'product'" class="product-picker">
              <label class="field">
                <span>اختيار المنتج</span>
                <input v-model="productQuery" class="admin-input" placeholder="ابحث باسم المنتج أو البراند" />
              </label>

              <div class="product-results">
                <button
                  v-for="p in filteredProducts"
                  :key="p.id"
                  type="button"
                  class="product-result"
                  :class="{ active: form.productId === p.id }"
                  @click="selectProduct(p)"
                >
                  <img v-if="productImage(p)" :src="assetUrl(productImage(p))" alt="" />
                  <Icon v-else name="mdi:package-variant-closed" />
                  <b>{{ productName(p) }}</b>
                  <small>{{ productBrand(p) || 'بدون براند' }}</small>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="wizard-section">
          <div class="wizard-number">3</div>
          <div class="wizard-body">
            <h3>الصورة أو الفيديو والرابط</h3>

            <div class="upload-box">
              <Icon name="mdi:cloud-upload-outline" />
              <b>{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع صورة أو فيديو' }}</b>
              <small>{{ form.type === 'popup' ? 'اختياري للمنبثق' : 'مطلوب للسلايدر أو البانر' }}</small>
              <input
                type="file"
                accept="image/*,video/mp4,video/webm"
                :multiple="form.type === 'slider'"
                @change="onPickFile"
              />
            </div>

            <label class="field">
              <span>رابط صورة/فيديو يدوي</span>
              <input v-model="form.imageUrl" class="admin-input keep-ltr" placeholder="https://..." />
            </label>

            <label class="field">
              <span>الرابط عند الضغط</span>
              <input v-model="form.linkUrl" class="admin-input keep-ltr" placeholder="/products" />
            </label>

            <label class="toggle-row">
              <input v-model="form.isEnabled" type="checkbox" />
              <span>الإعلان مفعل ويظهر للزوار</span>
            </label>
          </div>
        </div>

        <section class="preview-box">
          <div>
            <p class="ads-kicker">معاينة قبل الحفظ</p>
            <h3>{{ form.title || 'عنوان الإعلان' }}</h3>
            <p>{{ form.subtitle || 'هنا يظهر وصف الإعلان للزائر.' }}</p>
            <div class="ad-meta">
              <span>{{ currentType.label }}</span>
              <span>{{ placementLabel(form.placement) }}</span>
            </div>
          </div>

          <div class="preview-media">
            <template v-if="previewMedia.length">
              <div v-for="(url, idx) in previewMedia" :key="`${url}-${idx}`" class="preview-item">
                <video v-if="isVideoUrl(url)" :src="assetUrl(url)" muted playsinline controls />
                <img v-else :src="assetUrl(url)" alt="" />
                <button type="button" @click="removePreview(idx)">×</button>
              </div>
            </template>
            <div v-else class="preview-empty">
              <Icon :name="form.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-outline'" />
              <span>{{ form.type === 'popup' ? 'يمكن حفظ المنبثق كنص فقط' : 'ارفع صورة أو فيديو للمعاينة' }}</span>
            </div>
          </div>
        </section>

        <div class="sticky-save-bar">
          <button type="button" class="admin-secondary" @click="startNewAd">تفريغ الحقول</button>
          <UiButton type="submit" :disabled="saving || uploading">
            {{ saving ? 'جاري الحفظ...' : (editingId ? 'حفظ التعديل' : 'حفظ الإعلان') }}
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
const search = ref('')
const productQuery = ref('')
const filterType = ref('all')
const editingId = ref<string | null>(null)
const lastSavedId = ref<string | null>(null)

const adTypes = [
  { value: 'slider', label: 'سلايدر', icon: 'mdi:view-carousel-outline', hint: 'صور أو فيديو متحرك فوق الهيرو أو أعلى/آخر الصفحة' },
  { value: 'banner', label: 'بانر', icon: 'mdi:image-outline', hint: 'إعلان ثابت داخل الصفحة' },
  { value: 'popup', label: 'إعلان منبثق', icon: 'mdi:bell-ring-outline', hint: 'نافذة تظهر للزائر ويمكن أن تكون نصاً فقط' },
  { value: 'product', label: 'داخل منتج', icon: 'mdi:package-variant-closed', hint: 'إعلان مربوط بمنتج محدد من البحث' },
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
  sortOrder: 0,
  isEnabled: true,
})

const currentType = computed(() => adTypes.find((x) => x.value === form.type) || adTypes[0])
const placementOptions = computed(() => allPlacements.filter((p) => p.type === form.type))

const stats = computed(() => ({
  total: items.value.length,
  active: items.value.filter((x) => x.isEnabled).length,
  slider: items.value.filter((x) => x.type === 'slider').length,
  banner: items.value.filter((x) => x.type === 'banner').length,
  popup: items.value.filter((x) => x.type === 'popup').length,
}))

const previewMedia = computed(() => mediaFromForm())

const filteredAds = computed(() => {
  const q = search.value.trim().toLowerCase()
  return items.value.filter((ad: any) => {
    const okType = filterType.value === 'all'
      || (filterType.value === 'active' && ad.isEnabled)
      || (filterType.value === 'disabled' && !ad.isEnabled)
      || ad.type === filterType.value

    if (!okType) return false
    if (!q) return true

    return [ad.title, ad.subtitle, ad.type, ad.placement, ad.linkUrl]
      .filter(Boolean)
      .join(' ')
      .toLowerCase()
      .includes(q)
  })
})

const filteredProducts = computed(() => {
  const q = productQuery.value.trim().toLowerCase()
  const source = products.value || []
  if (!q) return source.slice(0, 12)
  return source
    .filter((p: any) => `${productName(p)} ${p.slug || ''} ${productBrand(p)}`.toLowerCase().includes(q))
    .slice(0, 18)
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

function normalizeAds(res: any): any[] {
  return unwrapList(res)
    .map((ad: any) => {
      const rawType = ad?.type ?? ad?.Type ?? 'banner'
      const normalizedType = typeof rawType === 'number'
        ? ['slider', 'banner', 'popup', 'product'][rawType] || 'banner'
        : String(rawType).trim().toLowerCase()

      const rawImageUrls = ad?.imageUrls ?? ad?.ImageUrls
      const imageUrls = Array.isArray(rawImageUrls)
        ? rawImageUrls
        : Array.isArray(rawImageUrls?.$values)
          ? rawImageUrls.$values
          : []
      const imageUrl = ad?.imageUrl ?? ad?.ImageUrl ?? imageUrls[0] ?? ''

      return {
        id: String(ad?.id ?? ad?.Id ?? crypto.randomUUID()),
        type: normalizedType,
        placement: String(ad?.placement ?? ad?.Placement ?? 'home_top').trim(),
        title: ad?.title ?? ad?.Title ?? '',
        subtitle: ad?.subtitle ?? ad?.Subtitle ?? '',
        imageUrl,
        imageUrls: imageUrls.length ? imageUrls : (imageUrl ? [imageUrl] : []),
        linkUrl: ad?.linkUrl ?? ad?.LinkUrl ?? '',
        productId: ad?.productId ?? ad?.ProductId ?? '',
        sortOrder: Number(ad?.sortOrder ?? ad?.SortOrder ?? 0),
        isEnabled: (ad?.isEnabled ?? ad?.IsEnabled) !== false,
        createdAt: ad?.createdAt ?? ad?.CreatedAt ?? null,
        updatedAt: ad?.updatedAt ?? ad?.UpdatedAt ?? null,
      }
    })
    .filter((ad: any) => ad.id)
}

function normalizeProducts(res: any): any[] {
  return unwrapList(res)
}

function mediaFromForm() {
  const list = Array.isArray(form.imageUrls) ? form.imageUrls.filter(Boolean) : []
  if (list.length) return list
  return form.imageUrl ? [form.imageUrl] : []
}

function mediaList(ad: any) {
  const list = Array.isArray(ad?.imageUrls) ? ad.imageUrls.filter(Boolean) : []
  if (list.length) return list
  return ad?.imageUrl ? [ad.imageUrl] : []
}

function primaryMedia(ad: any) {
  return mediaList(ad)[0] || ''
}

function assetUrl(url: string) {
  return api.buildAssetUrl(url || '')
}

function productName(p: any) {
  return p?.title || p?.name || p?.nameAr || 'منتج بدون اسم'
}
function productBrand(p: any) {
  return p?.brand || p?.brandName || p?.brandSlug || ''
}
function productImage(p: any) {
  return p?.imageUrl || p?.primaryImageUrl || p?.thumbnailUrl || p?.images?.[0]?.url || p?.assets?.[0]?.url || ''
}

function isVideoUrl(url: string) {
  return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '')
}

function typeLabel(type: string) {
  return adTypes.find((x) => x.value === type)?.label || type
}
function placementLabel(placement: string) {
  return allPlacements.find((x) => x.value === placement)?.label || placement
}

function emitAdsChanged() {
  if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
}

async function loadAll() {
  loading.value = true
  try {
    const [adsRes, productsRes]: any[] = await Promise.all([
      api.get('/admin/ads', { _ts: Date.now() }, { 'cache-control': 'no-cache' }),
      api.get('/admin/products', { page: 1, pageSize: 300, _ts: Date.now() }, { 'cache-control': 'no-cache' }).catch(() => []),
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
  productQuery.value = ''
}

function selectType(type: string) {
  form.type = type
  form.placement = allPlacements.find((p) => p.type === type)?.value || 'home_top'
  if (type === 'popup' && !form.linkUrl) form.linkUrl = '/products'
  if (type === 'product') form.linkUrl = form.productId ? `/product/${form.productId}` : '/products'
}

function selectProduct(product: any) {
  form.productId = product.id
  form.linkUrl = `/product/${product.id}`
  productQuery.value = productName(product)
}

function editAd(ad: any) {
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

  const selected = products.value.find((p: any) => p.id === ad.productId)
  productQuery.value = selected ? productName(selected) : ''
  if (process.client) window.scrollTo({ top: 0, behavior: 'smooth' })
}

function buildPayload() {
  const images = mediaFromForm()
  return {
    type: form.type,
    placement: form.placement,
    title: form.title || '',
    subtitle: form.subtitle || null,
    imageUrl: images[0] || '',
    imageUrls: images,
    linkUrl: form.linkUrl || null,
    productId: form.type === 'product' && form.productId ? form.productId : null,
    sortOrder: Number(form.sortOrder || 0),
    isEnabled: Boolean(form.isEnabled),
    startAt: null,
    endAt: null,
  }
}

function mergeSaved(raw: any) {
  const normalized = normalizeAds([raw])[0]
  if (!normalized) return
  const index = items.value.findIndex((x) => x.id === normalized.id)
  if (index >= 0) items.value.splice(index, 1, normalized)
  else items.value.unshift(normalized)
  lastSavedId.value = normalized.id
  filterType.value = 'all'
  search.value = ''
  setTimeout(() => {
    if (lastSavedId.value === normalized.id) lastSavedId.value = null
  }, 3500)
}

async function saveAd() {
  if (form.type !== 'popup' && mediaFromForm().length === 0) {
    toast.error('ارفع صورة أو فيديو واحد على الأقل')
    return
  }
  if (form.type === 'popup' && !String(form.title || form.subtitle || form.imageUrl).trim() && mediaFromForm().length === 0) {
    toast.error('اكتب عنواناً أو وصفاً للإعلان المنبثق')
    return
  }
  if (form.type === 'product' && !form.productId) {
    toast.error('اختر المنتج أولاً')
    return
  }

  saving.value = true
  try {
    const body = buildPayload()
    const saved: any = editingId.value
      ? await api.put(`/admin/ads/${editingId.value}`, body)
      : await api.post('/admin/ads', body)

    mergeSaved(saved)
    await loadAll()
    emitAdsChanged()
    toast.success(editingId.value ? 'تم تحديث الإعلان' : 'تم حفظ الإعلان وظهر في القائمة')
    startNewAd()
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
      const url = await directUpload.upload('admin/ads/upload', file, { maxMb: 150, fallbackToBff: true })
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

function removePreview(index: number) {
  const arr = [...mediaFromForm()]
  arr.splice(index, 1)
  form.imageUrls = arr
  form.imageUrl = arr[0] || ''
}

async function toggleAd(ad: any) {
  try {
    const body = {
      ...ad,
      imageUrls: mediaList(ad),
      imageUrl: primaryMedia(ad),
      isEnabled: !ad.isEnabled,
      startAt: null,
      endAt: null,
    }
    const saved: any = await api.put(`/admin/ads/${ad.id}`, body)
    mergeSaved(saved)
    await loadAll()
    emitAdsChanged()
  } catch {
    toast.error('تعذر تحديث الإعلان')
  }
}

async function removeAd(id: string) {
  if (!id || !confirm('هل تريد حذف هذا الإعلان؟')) return
  try {
    await api.del(`/admin/ads/${id}`)
    items.value = items.value.filter((x) => x.id !== id)
    emitAdsChanged()
    toast.success('تم حذف الإعلان')
  } catch {
    toast.error('تعذر حذف الإعلان')
  }
}

async function removeAll() {
  if (!confirm('هل تريد حذف كل الإعلانات؟')) return
  try {
    await api.del('/admin/ads')
    items.value = []
    emitAdsChanged()
    toast.success('تم حذف كل الإعلانات')
  } catch {
    toast.error('تعذر حذف الكل')
  }
}

watch(() => form.imageUrl, (url) => {
  if (url && form.imageUrls.length === 0) form.imageUrls = [url]
})

watch(() => form.type, () => {
  if (!placementOptions.value.some((p) => p.value === form.placement)) {
    form.placement = placementOptions.value[0]?.value || 'home_top'
  }
})

await loadAll()
</script>

<style scoped>
.ads-admin {
  display: grid;
  gap: 1.25rem;
  padding-bottom: 3rem;
}

.ads-hero-card,
.ads-editor-panel,
.ads-list-panel,
.preview-box {
  border: 1px solid rgba(var(--border), .78);
  background:
    radial-gradient(circle at top right, rgba(var(--primary), .12), transparent 34%),
    rgba(var(--surface-rgb), .9);
  border-radius: 32px;
  box-shadow: 0 24px 90px rgba(0, 0, 0, .18);
}

.ads-hero-card {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.4rem;
}

.ads-kicker {
  display: inline-flex;
  margin: 0 0 .45rem;
  color: rgb(var(--primary));
  font-size: .72rem;
  font-weight: 1000;
}

.ads-hero-card h1,
.panel-head h2 {
  margin: 0;
  color: rgb(var(--text));
  font-size: clamp(1.35rem, 2.4vw, 2.4rem);
  font-weight: 1000;
  letter-spacing: -.04em;
}

.ads-hero-card p,
.panel-head p,
.preview-box p {
  margin-top: .35rem;
  color: rgb(var(--muted));
  line-height: 1.8;
}

.hero-actions {
  display: flex;
  flex-wrap: wrap;
  gap: .6rem;
  align-items: flex-start;
}

.hero-actions button,
.ad-actions button,
.sticky-save-bar button {
  border-radius: 999px;
  padding: .75rem 1rem;
  font-weight: 900;
}

.ads-stats-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: .8rem;
}

.ads-stat {
  border: 1px solid rgba(var(--border), .7);
  background: rgba(var(--surface-rgb), .72);
  border-radius: 24px;
  padding: 1rem;
}

.ads-stat svg {
  color: rgb(var(--primary));
}

.ads-stat strong {
  display: block;
  margin-top: .45rem;
  color: rgb(var(--text));
  font-size: 2rem;
  line-height: 1;
}

.ads-stat span {
  display: block;
  margin-top: .4rem;
  color: rgb(var(--muted));
  font-weight: 800;
}

.ads-main-grid {
  display: grid;
  grid-template-columns: minmax(430px, 1fr) minmax(430px, 650px);
  gap: 1rem;
  align-items: start;
}

.ads-list-panel,
.ads-editor-panel {
  padding: 1.2rem;
}

.panel-head,
.editor-head {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}

.type-badge,
.state-pill,
.ad-meta span {
  display: inline-flex;
  align-items: center;
  border: 1px solid rgba(var(--border), .78);
  background: rgba(var(--surface-2-rgb), .72);
  border-radius: 999px;
  padding: .35rem .65rem;
  color: rgb(var(--text));
  font-size: .75rem;
  font-weight: 900;
  white-space: nowrap;
}

.list-toolbar {
  display: grid;
  grid-template-columns: 1fr 180px;
  gap: .65rem;
  margin-bottom: 1rem;
}

.admin-input {
  width: 100%;
  border: 1px solid rgba(var(--border), .82);
  background: rgba(var(--surface-2-rgb), .74);
  color: rgb(var(--text));
  border-radius: 18px;
  padding: .85rem 1rem;
  outline: none;
}

.admin-input:focus {
  border-color: rgba(var(--primary), .75);
  box-shadow: 0 0 0 4px rgba(var(--primary), .12);
}

.ads-empty {
  display: grid;
  place-items: center;
  gap: .55rem;
  min-height: 190px;
  border: 1px dashed rgba(var(--border), .75);
  border-radius: 26px;
  color: rgb(var(--muted));
  text-align: center;
  padding: 1rem;
}

.ads-empty svg {
  width: 2rem;
  height: 2rem;
  color: rgb(var(--primary));
}

.ads-list {
  display: grid;
  gap: .8rem;
}

.ad-card {
  display: grid;
  grid-template-columns: 110px 1fr auto;
  gap: .85rem;
  align-items: center;
  border: 1px solid rgba(var(--border), .75);
  background: rgba(var(--surface-2-rgb), .64);
  border-radius: 26px;
  padding: .85rem;
  transition: .22s ease;
}

.ad-card:hover,
.ad-card.selected,
.ad-card.fresh {
  transform: translateY(-2px);
  border-color: rgba(var(--primary), .75);
  background: rgba(var(--primary), .1);
}

.ad-preview {
  display: grid;
  place-items: center;
  width: 110px;
  height: 86px;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-rgb), .9);
  border-radius: 20px;
  overflow: hidden;
  color: rgb(var(--primary));
}

.ad-preview img,
.ad-preview video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.ad-preview svg {
  width: 2rem;
  height: 2rem;
}

.ad-title-row {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: .45rem;
}

.ad-title-row b {
  color: rgb(var(--text));
  font-weight: 1000;
}

.ad-content p {
  margin: .3rem 0;
  color: rgb(var(--muted));
  font-size: .86rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ad-meta {
  display: flex;
  flex-wrap: wrap;
  gap: .35rem;
  margin: .4rem 0;
}

.state-pill.on {
  border-color: rgba(34, 197, 94, .45);
  color: rgb(74, 222, 128);
}

.state-pill.off {
  border-color: rgba(239, 68, 68, .45);
  color: rgb(248, 113, 113);
}

.ad-actions {
  display: grid;
  gap: .35rem;
}

.wizard-section {
  display: grid;
  grid-template-columns: 48px 1fr;
  gap: .9rem;
  padding: 1rem 0;
  border-top: 1px solid rgba(var(--border), .55);
}

.wizard-number {
  display: grid;
  place-items: center;
  width: 42px;
  height: 42px;
  border-radius: 16px;
  background: linear-gradient(135deg, rgb(var(--primary)), rgba(var(--primary), .65));
  color: white;
  font-weight: 1000;
}

.wizard-body {
  display: grid;
  gap: .9rem;
}

.wizard-body h3 {
  margin: .35rem 0 0;
  color: rgb(var(--text));
  font-weight: 1000;
}

.type-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .65rem;
}

.type-card {
  text-align: right;
  border: 1px solid rgba(var(--border), .75);
  background: rgba(var(--surface-2-rgb), .66);
  border-radius: 22px;
  padding: .9rem;
  color: rgb(var(--text));
  transition: .22s ease;
}

.type-card:hover,
.type-card.active {
  transform: translateY(-2px);
  border-color: rgba(var(--primary), .76);
  background: rgba(var(--primary), .12);
}

.type-card svg {
  width: 1.35rem;
  height: 1.35rem;
  color: rgb(var(--primary));
}

.type-card b,
.type-card small {
  display: block;
}

.type-card small {
  margin-top: .35rem;
  color: rgb(var(--muted));
  line-height: 1.5;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 150px;
  gap: .75rem;
}

.field {
  display: grid;
  gap: .4rem;
}

.field span,
.toggle-row span {
  color: rgb(var(--text));
  font-size: .85rem;
  font-weight: 900;
}

.product-results {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .55rem;
  max-height: 270px;
  overflow: auto;
}

.product-result {
  display: grid;
  grid-template-columns: 52px 1fr;
  gap: .55rem;
  align-items: center;
  text-align: right;
  border: 1px solid rgba(var(--border), .75);
  background: rgba(var(--surface-2-rgb), .65);
  color: rgb(var(--text));
  border-radius: 18px;
  padding: .55rem;
}

.product-result.active {
  border-color: rgb(var(--primary));
  background: rgba(var(--primary), .13);
}

.product-result img,
.product-result svg {
  width: 52px;
  height: 52px;
  border-radius: 14px;
  object-fit: cover;
}

.product-result small {
  color: rgb(var(--muted));
}

.upload-box {
  position: relative;
  display: grid;
  place-items: center;
  gap: .35rem;
  min-height: 155px;
  border: 1px dashed rgba(var(--primary), .55);
  background: rgba(var(--primary), .08);
  border-radius: 26px;
  text-align: center;
  color: rgb(var(--text));
}

.upload-box svg {
  width: 2rem;
  height: 2rem;
  color: rgb(var(--primary));
}

.upload-box small {
  color: rgb(var(--muted));
}

.upload-box input {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
}

.toggle-row {
  display: inline-flex;
  align-items: center;
  gap: .5rem;
}

.preview-box {
  margin-top: 1rem;
  padding: 1rem;
}

.preview-box h3 {
  margin: 0;
  color: rgb(var(--text));
  font-size: 1.35rem;
  font-weight: 1000;
}

.preview-media {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: .65rem;
  margin-top: 1rem;
}

.preview-item {
  position: relative;
  overflow: hidden;
  border-radius: 20px;
  min-height: 120px;
  background: rgba(var(--surface-2-rgb), .85);
}

.preview-item img,
.preview-item video {
  width: 100%;
  height: 160px;
  object-fit: cover;
}

.preview-item button {
  position: absolute;
  top: .45rem;
  inset-inline-end: .45rem;
  width: 32px;
  height: 32px;
  border-radius: 999px;
  color: white;
  background: rgba(0, 0, 0, .64);
  font-weight: 1000;
}

.preview-empty {
  grid-column: 1 / -1;
  display: grid;
  place-items: center;
  gap: .4rem;
  min-height: 130px;
  border: 1px dashed rgba(var(--border), .75);
  border-radius: 22px;
  color: rgb(var(--muted));
}

.preview-empty svg {
  width: 2rem;
  height: 2rem;
  color: rgb(var(--primary));
}

.sticky-save-bar {
  position: sticky;
  bottom: .75rem;
  display: flex;
  justify-content: flex-end;
  gap: .65rem;
  margin-top: 1rem;
  padding: .75rem;
  border: 1px solid rgba(var(--border), .65);
  background: rgba(var(--surface-rgb), .9);
  backdrop-filter: blur(18px);
  border-radius: 24px;
}

.keep-ltr {
  direction: ltr;
  unicode-bidi: plaintext;
}

@media (max-width: 1200px) {
  .ads-main-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 760px) {
  .ads-hero-card,
  .panel-head,
  .editor-head {
    flex-direction: column;
  }

  .ads-stats-grid,
  .type-grid,
  .form-grid,
  .list-toolbar,
  .product-results {
    grid-template-columns: 1fr;
  }

  .wizard-section,
  .ad-card {
    grid-template-columns: 1fr;
  }

  .ad-preview {
    width: 100%;
    height: 180px;
  }

  .ad-actions {
    display: flex;
    flex-wrap: wrap;
  }
}
</style>
