<template>
  <div class="ads-admin-page" dir="rtl">
    <header class="page-header">
      <div class="page-title">
        <p class="breadcrumb">الرئيسية · الإعلانات</p>
        <h1>إدارة الإعلانات</h1>
      </div>
      <div class="top-actions">
        <button type="button" class="header-btn" @click="loadAll">
          <Icon name="mdi:refresh" />
          <span>تحديث</span>
        </button>
        <button type="button" class="header-btn" @click="startNewAd">
          <Icon name="mdi:plus" />
          <span>إعلان جديد</span>
        </button>
        <button type="button" class="header-btn danger" @click="removeAll">
          <Icon name="mdi:trash-can-outline" />
          <span>حذف الكل</span>
        </button>
      </div>
    </header>

    <section class="guide-banner">
      <div class="guide-icon">
        <Icon name="mdi:information-outline" />
      </div>
      <div>
        <b>تعليمات إدارة الإعلانات</b>
        <p>اختر نوع الإعلان، املأ المحتوى، حدد مكان الظهور وحالة الإعلان، ثم احفظ. الإعلان المحفوظ يظهر فوراً في القائمة ويمكن تعديله أو تعطيله أو حذفه.</p>
      </div>
    </section>

    <section class="stats-grid">
      <article class="metric-card red">
        <div class="metric-icon"><Icon name="mdi:bullhorn-outline" /></div>
        <div>
          <span>إجمالي الإعلانات</span>
          <strong>{{ stats.total }}</strong>
          <small>كل الإعلانات</small>
        </div>
      </article>
      <article class="metric-card green">
        <div class="metric-icon"><Icon name="mdi:check-circle-outline" /></div>
        <div>
          <span>المفعلة</span>
          <strong>{{ stats.active }}</strong>
          <small>إعلانات نشطة</small>
        </div>
      </article>
      <article class="metric-card yellow">
        <div class="metric-icon"><Icon name="mdi:clock-outline" /></div>
        <div>
          <span>المنتظرة</span>
          <strong>0</strong>
          <small>بانتظار النشر</small>
        </div>
      </article>
      <article class="metric-card purple">
        <div class="metric-icon"><Icon name="mdi:archive-outline" /></div>
        <div>
          <span>الموقوفة</span>
          <strong>{{ stats.total - stats.active }}</strong>
          <small>إعلانات غير مفعلة</small>
        </div>
      </article>
    </section>

    <section class="editor-grid">
      <form class="panel editor-panel" @submit.prevent="saveAd">
        <div class="panel-title">
          <h2>إنشاء / تعديل إعلان</h2>
          <Icon name="mdi:bullhorn-outline" />
        </div>

        <div class="form-helper-card">
          <Icon name="mdi:lightbulb-on-outline" />
          <span>الحقول داخل الصناديق الداكنة، أما الأزرار فهي بنفسجية أو ملوّنة حسب حالتها حتى يكون الاستخدام واضحاً.</span>
        </div>

        <div class="form-grid">
          <label class="field full-width">
            <span class="field-label field-label--choice"><Icon name="mdi:cursor-default-click-outline" /> نوع الإعلان <small>هذه أزرار اختيار وليست نصوصاً</small></span>
            <div class="segmented-tabs segmented-tabs--clear">
              <button
                v-for="type in adTypes"
                :key="type.value"
                type="button"
                :class="['seg-tab', { active: form.type === type.value }]"
                @click="selectType(type.value)"
              >
                <Icon :name="type.icon" />
                <b>{{ type.label }}</b>
              </button>
            </div>
          </label>

          <label class="field">
            <span class="field-label"><Icon name="mdi:format-title" /> عنوان الإعلان</span>
            <input v-model="form.title" class="input-control" placeholder="اكتب عنوان الإعلان" />
          </label>

          <label class="field">
            <span class="field-label"><Icon name="mdi:map-marker-radius-outline" /> مكان الظهور</span>
            <select v-model="form.placement" class="input-control select-control">
              <option value="" disabled>اختر مكان الظهور</option>
              <option v-for="p in placementOptions" :key="p.value" :value="p.value">
                {{ p.label }}
              </option>
            </select>
          </label>

          <label class="field">
            <span class="field-label"><Icon name="mdi:link-variant" /> رابط الإعلان (اختياري)</span>
            <input v-model="form.linkUrl" class="input-control keep-ltr" placeholder="https://example.com أو /products" />
          </label>

          <label class="field">
            <span class="field-label"><Icon name="mdi:sort-numeric-ascending" /> ترتيب العرض</span>
            <input v-model.number="form.sortOrder" type="number" class="input-control" min="0" />
          </label>

          <label class="field full-width">
            <span class="field-label"><Icon name="mdi:text-short" /> الوصف المختصر</span>
            <input v-model="form.subtitle" class="input-control" placeholder="نص قصير يظهر تحت العنوان" />
          </label>

          <div v-if="form.type === 'welcome'" class="welcome-offer-fields full-width">
            <div class="form-helper-card">
              <Icon name="mdi:gift-open-outline" />
              <span>هذا الإعلان يظهر فقط بعد إنشاء حساب جديد، ويضيف النقاط/الكوبون للزبون مع إشعار داخل الحساب.</span>
            </div>
            <div class="form-grid compact-grid">
              <label class="field">
                <span class="field-label"><Icon name="mdi:ticket-percent-outline" /> كود الخصم</span>
                <input v-model="form.welcomeCouponCode" class="input-control keep-ltr" placeholder="مثال: WELCOME10" />
              </label>
              <label class="field">
                <span class="field-label"><Icon name="mdi:star-circle-outline" /> نقاط الترحيب</span>
                <input v-model.number="form.welcomePoints" type="number" min="0" class="input-control" />
              </label>
            </div>
          </div>

          <div v-if="form.type === 'product'" class="product-picker full-width">
            <label class="field">
              <span>اختيار المنتج</span>
              <input v-model="productQuery" class="input-control" placeholder="ابحث باسم المنتج أو البراند" />
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
                <span>
                  <b>{{ productName(p) }}</b>
                  <small>{{ productBrand(p) || 'بدون براند' }}</small>
                </span>
              </button>
            </div>
          </div>

          <div class="field full-width">
            <span class="field-label field-label--choice"><Icon name="mdi:toggle-switch-outline" /> حالة الإعلان <small>اختر حالة واضحة للإعلان</small></span>
            <div class="status-tabs status-tabs--clear">
              <button type="button" :class="['status-tab on', { active: form.isEnabled }]" @click="form.isEnabled = true">
                <Icon name="mdi:check-circle-outline" />
                مفعل
              </button>
              <button type="button" class="status-tab schedule" disabled>
                <Icon name="mdi:clock-outline" />
                مجدول
              </button>
              <button type="button" :class="['status-tab off', { active: !form.isEnabled }]" @click="form.isEnabled = false">
                <Icon name="mdi:close-circle-outline" />
                غير مفعل
              </button>
            </div>
          </div>
        </div>

        <div class="save-row">
          <button type="button" class="secondary-action" @click="startNewAd">
            <Icon name="mdi:refresh" />
            إعادة تعيين
          </button>
          <button type="button" class="secondary-action" @click="saveAd" :disabled="saving || uploading">
            <Icon name="mdi:content-save-plus-outline" />
            حفظ ومتابعة
          </button>
          <UiButton type="submit" :disabled="saving || uploading" class="primary-save">
            <Icon name="mdi:content-save-outline" />
            {{ saving ? 'جاري الحفظ...' : (editingId ? 'حفظ التعديل' : 'حفظ الإعلان') }}
          </UiButton>
        </div>
      </form>

      <aside class="panel upload-panel">
        <div class="panel-title">
          <h2>ملف الإعلان</h2>
          <Icon name="mdi:bullhorn-outline" />
        </div>

        <div class="upload-drop">
          <Icon name="mdi:cloud-upload-outline" />
          <b>{{ uploading ? 'جاري الرفع...' : 'منطقة رفع الصور والفيديو' }}</b>
          <span>اسحب الملف هنا أو استخدم زر الرفع الواضح</span>
          <button type="button" class="upload-btn"><Icon name="mdi:upload" /> اختر ملف</button>
          <input
            type="file"
            accept="image/*,video/mp4,video/webm"
            :multiple="form.type === 'slider'"
            @change="onPickFile"
          />
        </div>

        <p class="upload-note">الصيغ المدعومة: JPG, PNG, GIF, MP4, WebM. يفضّل ضغط الفيديو قبل الرفع.</p>

        <label class="field">
          <span class="field-label"><Icon name="mdi:link-box-outline" /> رابط صورة / فيديو يدوي</span>
          <input v-model="form.imageUrl" class="input-control keep-ltr" placeholder="https://..." />
        </label>

        <div class="ad-preview-card">
          <div class="preview-copy">
            <span>معاينة الإعلان</span>
            <h3>{{ form.title || 'إعلان بدون عنوان' }}</h3>
            <p>{{ form.subtitle || 'بدون وصف' }}</p>
            <b :class="form.isEnabled ? 'active-state' : 'disabled-state'">{{ form.isEnabled ? 'مفعل' : 'معطل' }}</b>
            <small class="keep-ltr">{{ form.linkUrl || '/products' }}</small>
          </div>
          <div class="preview-thumb">
            <template v-if="previewMedia.length">
              <video v-if="isVideoUrl(previewMedia[0])" :src="assetUrl(previewMedia[0])" muted playsinline controls />
              <img v-else :src="assetUrl(previewMedia[0])" alt="" />
            </template>
            <Icon v-else name="mdi:image-outline" />
          </div>
        </div>
      </aside>
    </section>

    <section class="panel table-panel">
      <div class="table-toolbar">
        <div>
          <h2>قائمة الإعلانات</h2>
          <p>كل إعلان محفوظ يظهر هنا مع أدوات المعاينة والتعديل والحذف.</p>
        </div>
        <div class="table-filters">
          <input v-model="search" class="input-control" placeholder="ابحث بالعنوان، النوع، المكان..." />
          <select v-model="filterType" class="input-control select-control">
            <option value="all">كل الإعلانات</option>
            <option value="active">المفعلة فقط</option>
            <option value="disabled">المعطلة فقط</option>
            <option value="slider">سلايدر</option>
            <option value="banner">بانر</option>
            <option value="popup">منبثق</option>
            <option value="product">داخل منتج</option>
            <option value="welcome">ترحيب مستخدم جديد</option>
          </select>
          <button type="button" class="secondary-action" @click="filterType = 'all'; search = ''">
            <Icon name="mdi:filter-off-outline" />
            إعادة الفلاتر
          </button>
        </div>
      </div>

      <div v-if="loading" class="empty-card">
        <Icon name="mdi:loading" />
        <b>جاري تحميل الإعلانات...</b>
      </div>

      <div v-else-if="!items.length" class="empty-card">
        <Icon name="mdi:bullhorn-variant-outline" />
        <b>لا توجد إعلانات محفوظة</b>
        <span>أنشئ أول إعلان من المحرر وسيظهر هنا مباشرة.</span>
      </div>

      <div v-else-if="!filteredAds.length" class="empty-card">
        <Icon name="mdi:filter-off-outline" />
        <b>الفلاتر تخفي الإعلانات</b>
        <span>عدد الإعلانات المحفوظة: {{ items.length }}</span>
        <button type="button" class="secondary-action" @click="filterType = 'all'; search = ''">عرض الكل</button>
      </div>

      <div v-else class="ads-table-wrap">
        <table class="ads-table">
          <thead>
            <tr>
              <th>#</th>
              <th>الصورة / الفيديو</th>
              <th>العنوان</th>
              <th>النوع</th>
              <th>مكان الظهور</th>
              <th>الحالة</th>
              <th>الترتيب</th>
              <th>تاريخ الإنشاء</th>
              <th>الإجراءات</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(ad, idx) in filteredAds"
              :key="ad.id"
              :class="{ selected: editingId === ad.id, fresh: lastSavedId === ad.id }"
            >
              <td>{{ idx + 1 }}</td>
              <td>
                <button type="button" class="table-media" @click="editAd(ad)">
                  <video v-if="isVideoUrl(primaryMedia(ad))" :src="assetUrl(primaryMedia(ad))" muted playsinline />
                  <img v-else-if="primaryMedia(ad)" :src="assetUrl(primaryMedia(ad))" alt="" />
                  <Icon v-else name="mdi:image-off-outline" />
                </button>
              </td>
              <td>
                <strong>{{ ad.title || 'إعلان بدون عنوان' }}</strong>
                <small>{{ ad.subtitle || 'بدون وصف' }}</small>
                <em class="keep-ltr">{{ ad.linkUrl || 'بدون رابط' }}</em>
              </td>
              <td><span class="pill purple-pill">{{ typeLabel(ad.type) }}</span></td>
              <td><span class="pill">{{ placementLabel(ad.placement) }}</span></td>
              <td>
                <span :class="['pill', ad.isEnabled ? 'green-pill' : 'red-pill']">
                  {{ ad.isEnabled ? 'مفعل' : 'معطل' }}
                </span>
              </td>
              <td>{{ ad.sortOrder }}</td>
              <td>{{ formatDate(ad.createdAt) }}</td>
              <td>
                <div class="table-actions">
                  <button type="button" class="icon-action blue" title="معاينة" @click="editAd(ad)"><Icon name="mdi:eye-outline" /></button>
                  <button type="button" class="icon-action amber" title="تعديل" @click="editAd(ad)"><Icon name="mdi:pencil-outline" /></button>
                  <button type="button" class="icon-action red" title="حذف" @click="removeAd(ad.id)"><Icon name="mdi:trash-can-outline" /></button>
                  <button type="button" class="mini-toggle" @click="toggleAd(ad)">
                    {{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
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
  { value: 'welcome', label: 'ترحيب مستخدم جديد', icon: 'mdi:gift-open-outline', hint: 'يظهر مرة واحدة بعد إنشاء الحساب ويمنح نقاط/كوبون' },
  { value: 'product', label: 'داخل منتج', icon: 'mdi:package-variant-closed', hint: 'إعلان مربوط بمنتج محدد من البحث' },
]

const allPlacements = [
  // السلايدر يبقى مخصصاً للمساحات الكبيرة، فوق الهيرو أو آخر الصفحة فقط.
  { value: 'home_hero_slider', label: 'سلايدر بداية الصفحة', type: 'slider' },
  { value: 'home_bottom_slider', label: 'سلايدر آخر الصفحة', type: 'slider' },
  // البانر حسب طلبك: بداية الصفحة أو آخر الصفحة فقط، بدون أعلى الصفحات/آخر الصفحات.
  { value: 'home_hero_top', label: 'بانر بداية الصفحة', type: 'banner' },
  { value: 'home_bottom', label: 'بانر آخر الصفحة', type: 'banner' },
  // المنبثق مستقل ويظهر فوق الموقع.
  { value: 'popup', label: 'إعلان منبثق عام', type: 'popup' },
  { value: 'home_popup', label: 'إعلان منبثق للواجهة فقط', type: 'popup' },
  { value: 'welcome_new_user', label: 'إعلان ترحيب للمستخدم الجديد فقط', type: 'welcome' },
  { value: 'product_page', label: 'إعلان داخل صفحة منتج', type: 'product' },
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
  isNewUserOnly: false,
  welcomeCouponCode: '',
  welcomePoints: 10,
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
      let normalizedType = typeof rawType === 'number'
        ? ['slider', 'banner', 'popup', 'product'][rawType] || 'banner'
        : String(rawType).trim().toLowerCase()
      const rawPlacement = String(ad?.placement ?? ad?.Placement ?? 'home_top').trim()
      if (rawPlacement === 'welcome_new_user' || ad?.isNewUserOnly || ad?.IsNewUserOnly) normalizedType = 'welcome'

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
        placement: rawPlacement,
        title: ad?.title ?? ad?.Title ?? '',
        subtitle: ad?.subtitle ?? ad?.Subtitle ?? '',
        imageUrl,
        imageUrls: imageUrls.length ? imageUrls : (imageUrl ? [imageUrl] : []),
        linkUrl: ad?.linkUrl ?? ad?.LinkUrl ?? '',
        productId: ad?.productId ?? ad?.ProductId ?? '',
        sortOrder: Number(ad?.sortOrder ?? ad?.SortOrder ?? 0),
        isEnabled: (ad?.isEnabled ?? ad?.IsEnabled) !== false,
        isNewUserOnly: (ad?.isNewUserOnly ?? ad?.IsNewUserOnly) === true || rawPlacement === 'welcome_new_user',
        welcomeCouponCode: ad?.welcomeCouponCode ?? ad?.WelcomeCouponCode ?? '',
        welcomePoints: Number(ad?.welcomePoints ?? ad?.WelcomePoints ?? 0),
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

function formatDate(value: any) {
  if (!value) return '-'
  try {
    return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value))
  } catch {
    return String(value).slice(0, 16)
  }
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
    isNewUserOnly: false,
    welcomeCouponCode: '',
    welcomePoints: 10,
  })
  productQuery.value = ''
}

function selectType(type: string) {
  form.type = type
  form.placement = allPlacements.find((p) => p.type === type)?.value || 'home_top'
  if (type === 'popup' && !form.linkUrl) form.linkUrl = '/products'
  if (type === 'welcome') { form.placement = 'welcome_new_user'; form.isNewUserOnly = true; if (!form.title) form.title = 'مبروك!'; if (!form.subtitle) form.subtitle = 'حصلت على خصم و10 نقاط، استمتع بالتسوق داخل التطبيق.' }
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
    isNewUserOnly: ad.isNewUserOnly === true || ad.placement === 'welcome_new_user',
    welcomeCouponCode: ad.welcomeCouponCode || '',
    welcomePoints: Number(ad.welcomePoints || 0),
  })

  const selected = products.value.find((p: any) => p.id === ad.productId)
  productQuery.value = selected ? productName(selected) : ''
  if (process.client) window.scrollTo({ top: 0, behavior: 'smooth' })
}

function buildPayload() {
  const images = mediaFromForm()
  return {
    type: form.type === 'welcome' ? 'popup' : form.type,
    placement: form.type === 'welcome' ? 'welcome_new_user' : form.placement,
    title: form.title || '',
    subtitle: form.subtitle || null,
    imageUrl: images[0] || '',
    imageUrls: images,
    linkUrl: form.linkUrl || null,
    productId: form.type === 'product' && form.productId ? form.productId : null,
    sortOrder: Number(form.sortOrder || 0),
    isEnabled: Boolean(form.isEnabled),
    isNewUserOnly: form.type === 'welcome' || Boolean(form.isNewUserOnly),
    welcomeCouponCode: form.welcomeCouponCode || null,
    welcomePoints: Number(form.welcomePoints || 0),
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
  if (!['popup','welcome'].includes(form.type) && mediaFromForm().length === 0) {
    toast.error('ارفع صورة أو فيديو واحد على الأقل')
    return
  }
  if (['popup','welcome'].includes(form.type) && !String(form.title || form.subtitle || form.imageUrl).trim() && mediaFromForm().length === 0) {
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
      isNewUserOnly: ad.isNewUserOnly === true || ad.placement === 'welcome_new_user',
      welcomeCouponCode: ad.welcomeCouponCode || null,
      welcomePoints: Number(ad.welcomePoints || 0),
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
.ads-admin-page {
  --admin-bg: #070d16;
  --admin-panel: rgba(13, 22, 35, .88);
  --admin-panel-2: rgba(16, 27, 43, .72);
  --admin-border: rgba(119, 136, 170, .22);
  --admin-text: #eef4ff;
  --admin-muted: #9fb0c8;
  --admin-purple: #8b5cf6;
  --admin-purple-2: #a855f7;
  --admin-blue: #3b82f6;
  --admin-green: #22c55e;
  --admin-red: #ef4444;
  --admin-yellow: #f59e0b;
  display: grid;
  gap: 1.15rem;
  color: var(--admin-text);
  padding-bottom: 2.5rem;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.breadcrumb,
.eyebrow {
  margin: 0 0 .35rem;
  color: var(--admin-muted);
  font-size: .78rem;
  font-weight: 800;
}

.page-title h1,
.panel-title h2,
.table-toolbar h2 {
  margin: 0;
  color: var(--admin-text);
  font-size: clamp(1.45rem, 2vw, 2.2rem);
  font-weight: 1000;
  letter-spacing: -.04em;
}

.top-actions {
  display: flex;
  gap: .55rem;
  flex-wrap: wrap;
}

.header-btn,
.secondary-action,
.upload-btn,
.icon-action,
.mini-toggle {
  border: 1px solid var(--admin-border);
  background: linear-gradient(180deg, rgba(24, 35, 53, .92), rgba(11, 19, 31, .9));
  color: var(--admin-text);
  border-radius: 12px;
  min-height: 42px;
  padding: .65rem .9rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: .45rem;
  font-weight: 900;
  transition: .18s ease;
  box-shadow: inset 0 1px 0 rgba(255,255,255,.04);
}
.header-btn:hover,
.secondary-action:hover,
.upload-btn:hover,
.icon-action:hover,
.mini-toggle:hover {
  transform: translateY(-1px);
  border-color: rgba(139, 92, 246, .65);
  background: rgba(139, 92, 246, .14);
}
.header-btn.danger {
  border-color: rgba(239, 68, 68, .4);
  color: #ff8d8d;
  background: rgba(239, 68, 68, .12);
}

.guide-banner,
.panel,
.metric-card {
  border: 1px solid var(--admin-border);
  background:
    radial-gradient(circle at top right, rgba(139, 92, 246, .12), transparent 36%),
    linear-gradient(145deg, rgba(12, 21, 34, .96), rgba(8, 14, 24, .94));
  border-radius: 16px;
  box-shadow: 0 18px 70px rgba(0,0,0,.24);
}

.guide-banner {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.1rem;
  border-color: rgba(59, 130, 246, .45);
  background: linear-gradient(135deg, rgba(20, 37, 68, .86), rgba(23, 23, 57, .7));
}
.guide-icon {
  width: 44px;
  height: 44px;
  border-radius: 14px;
  display: grid;
  place-items: center;
  color: #9dbdff;
  background: rgba(59, 130, 246, .18);
}
.guide-banner p {
  margin: .25rem 0 0;
  color: var(--admin-muted);
  line-height: 1.8;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: .9rem;
}
.metric-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.1rem;
}
.metric-icon {
  width: 58px;
  height: 58px;
  border-radius: 16px;
  display: grid;
  place-items: center;
  font-size: 1.55rem;
}
.metric-card.red .metric-icon { background: rgba(239, 68, 68, .18); color: #ff8383; }
.metric-card.green .metric-icon { background: rgba(34, 197, 94, .18); color: #50e68b; }
.metric-card.yellow .metric-icon { background: rgba(245, 158, 11, .18); color: #ffc655; }
.metric-card.purple .metric-icon { background: rgba(139, 92, 246, .2); color: #c4a7ff; }
.metric-card span,
.metric-card small {
  display: block;
  color: var(--admin-muted);
  font-size: .83rem;
}
.metric-card strong {
  display: block;
  margin: .1rem 0;
  font-size: 2rem;
  line-height: 1;
  color: var(--admin-text);
}

.editor-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.08fr) minmax(360px, .78fr);
  gap: .9rem;
}
.panel {
  padding: 1rem;
}
.panel-title,
.table-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  margin-bottom: 1rem;
}
.panel-title svg,
.table-toolbar svg { color: #a9c3ff; }

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .85rem 1rem;
}
.full-width { grid-column: 1 / -1; }
.field {
  display: grid;
  gap: .42rem;
}
.field > span {
  color: var(--admin-text);
  font-size: .86rem;
  font-weight: 950;
}
.input-control {
  width: 100%;
  min-height: 46px;
  border: 1px solid rgba(119, 136, 170, .28);
  background: rgba(8, 15, 26, .78);
  color: var(--admin-text);
  border-radius: 12px;
  padding: .78rem .9rem;
  outline: none;
  transition: .18s ease;
}
.input-control::placeholder { color: rgba(159, 176, 200, .58); }
.input-control:focus {
  border-color: rgba(139, 92, 246, .78);
  box-shadow: 0 0 0 4px rgba(139, 92, 246, .13);
}
.select-control { cursor: pointer; }
.keep-ltr { direction: ltr; unicode-bidi: plaintext; text-align: left; }

.segmented-tabs,
.status-tabs {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  border: 1px solid rgba(119, 136, 170, .28);
  border-radius: 13px;
  overflow: hidden;
  background: rgba(8, 15, 26, .6);
}
.seg-tab,
.status-tab {
  min-height: 50px;
  border-inline-start: 1px solid rgba(119, 136, 170, .22);
  color: var(--admin-text);
  background: transparent;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: .45rem;
  font-weight: 950;
}
.seg-tab:first-child,
.status-tab:first-child { border-inline-start: 0; }
.seg-tab.active {
  background: linear-gradient(135deg, var(--admin-purple), var(--admin-purple-2));
  color: white;
}
.status-tabs { grid-template-columns: repeat(3, 1fr); }
.status-tab.on.active { background: rgba(34, 197, 94, .14); color: #52ee91; box-shadow: inset 0 0 0 1px rgba(34, 197, 94, .7); }
.status-tab.schedule { color: #ffcc5c; background: rgba(245, 158, 11, .08); opacity: .86; cursor: not-allowed; }
.status-tab.off.active { background: rgba(239, 68, 68, .14); color: #ff7b7b; box-shadow: inset 0 0 0 1px rgba(239, 68, 68, .7); }

.product-results {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .55rem;
  max-height: 230px;
  overflow: auto;
}
.product-result {
  display: grid;
  grid-template-columns: 50px 1fr;
  gap: .55rem;
  align-items: center;
  text-align: right;
  border: 1px solid rgba(119,136,170,.24);
  background: rgba(9, 16, 27, .72);
  color: var(--admin-text);
  border-radius: 14px;
  padding: .5rem;
}
.product-result.active { border-color: var(--admin-purple); background: rgba(139, 92, 246, .12); }
.product-result img,
.product-result svg { width: 50px; height: 50px; border-radius: 12px; object-fit: cover; }
.product-result small { color: var(--admin-muted); }

.save-row {
  display: grid;
  grid-template-columns: 1fr 1fr 1.2fr;
  gap: .65rem;
  margin-top: 1rem;
}
.primary-save {
  background: linear-gradient(135deg, var(--admin-purple), var(--admin-purple-2)) !important;
  border-color: transparent !important;
  color: white !important;
}

.upload-drop {
  position: relative;
  display: grid;
  place-items: center;
  gap: .35rem;
  min-height: 168px;
  border: 1px dashed rgba(168, 85, 247, .68);
  background:
    radial-gradient(circle, rgba(139, 92, 246, .16), transparent 70%),
    rgba(139, 92, 246, .07);
  border-radius: 14px;
  text-align: center;
  color: var(--admin-text);
  overflow: hidden;
}
.upload-drop > svg { width: 2.3rem; height: 2.3rem; color: #b98cff; }
.upload-drop span,
.upload-note { color: var(--admin-muted); }
.upload-drop input { position:absolute; inset:0; opacity:0; cursor:pointer; }
.upload-btn {
  min-height: 38px;
  background: linear-gradient(135deg, var(--admin-purple), var(--admin-purple-2));
  border-color: transparent;
}
.upload-note {
  margin: .65rem 0 .85rem;
  text-align: center;
  font-size: .8rem;
}

.ad-preview-card {
  display: grid;
  grid-template-columns: 1fr 180px;
  gap: 1rem;
  align-items: center;
  margin-top: 1rem;
  padding: 1rem;
  border: 1px solid rgba(245, 158, 11, .34);
  background: rgba(15, 24, 39, .8);
  border-radius: 14px;
}
.preview-copy span { color: var(--admin-muted); font-size: .78rem; font-weight: 800; }
.preview-copy h3 { margin: .35rem 0 .25rem; font-size: 1.25rem; }
.preview-copy p { margin: 0 0 .5rem; color: var(--admin-muted); }
.preview-copy small { display:block; margin-top:.4rem; color:#61d4ff; }
.preview-thumb {
  display:grid; place-items:center;
  min-height: 112px;
  border-radius: 12px;
  overflow:hidden;
  background: rgba(8, 15, 26, .75);
  color: var(--admin-purple);
}
.preview-thumb img,
.preview-thumb video { width:100%; height:130px; object-fit:cover; }
.preview-thumb svg { width: 2rem; height: 2rem; }
.active-state,
.disabled-state,
.pill {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  padding: .28rem .62rem;
  font-size: .78rem;
  font-weight: 950;
  border: 1px solid rgba(119,136,170,.24);
  background: rgba(9,16,27,.72);
  color: var(--admin-muted);
}
.active-state,
.green-pill { color: #4ade80; background: rgba(34,197,94,.12); border-color: rgba(34,197,94,.35); }
.disabled-state,
.red-pill { color: #ff7b7b; background: rgba(239,68,68,.12); border-color: rgba(239,68,68,.38); }
.purple-pill { color: #c4a7ff; background: rgba(139,92,246,.14); border-color: rgba(139,92,246,.38); }

.table-panel { padding: 0; overflow: hidden; }
.table-toolbar { padding: 1rem 1rem 0; }
.table-toolbar p { margin: .3rem 0 0; color: var(--admin-muted); }
.table-filters { display:grid; grid-template-columns: minmax(220px, 1fr) 170px 145px; gap:.55rem; min-width:min(680px, 100%); }
.ads-table-wrap { overflow-x:auto; }
.ads-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 980px;
}
.ads-table th,
.ads-table td {
  border-top: 1px solid rgba(119,136,170,.18);
  padding: .82rem .9rem;
  text-align: right;
  vertical-align: middle;
}
.ads-table th {
  color: var(--admin-muted);
  background: rgba(17, 29, 46, .86);
  font-size: .8rem;
  font-weight: 1000;
}
.ads-table tbody tr { transition: .18s ease; }
.ads-table tbody tr:hover,
.ads-table tbody tr.selected,
.ads-table tbody tr.fresh { background: rgba(139, 92, 246, .08); }
.ads-table td strong { display:block; color:var(--admin-text); }
.ads-table td small,
.ads-table td em { display:block; color:var(--admin-muted); margin-top:.25rem; font-style:normal; }
.table-media {
  width: 78px;
  height: 58px;
  border-radius: 10px;
  overflow: hidden;
  display: grid;
  place-items: center;
  background: rgba(8, 15, 26, .85);
  color: var(--admin-purple);
  border: 1px solid rgba(119,136,170,.22);
}
.table-media img,
.table-media video { width:100%; height:100%; object-fit:cover; }
.table-actions { display:flex; align-items:center; gap:.35rem; flex-wrap:wrap; }
.icon-action { width:36px; height:36px; min-height:36px; padding:0; border-radius:9px; }
.icon-action.blue { color:#93c5fd; border-color:rgba(59,130,246,.38); }
.icon-action.amber { color:#fbbf24; border-color:rgba(245,158,11,.4); }
.icon-action.red { color:#fb7185; border-color:rgba(239,68,68,.4); }
.mini-toggle { min-height:36px; padding:.35rem .7rem; }

.empty-card {
  display:grid;
  place-items:center;
  gap:.5rem;
  min-height:180px;
  color:var(--admin-muted);
  text-align:center;
  padding:1rem;
}
.empty-card svg { color:var(--admin-purple); width:2rem; height:2rem; }
.empty-card b { color:var(--admin-text); }


.form-helper-card{
  display:flex; align-items:center; gap:.65rem;
  margin-bottom:1rem; padding:.8rem .95rem;
  border:1px solid rgba(139,92,246,.32);
  border-radius:14px;
  color:#d7c8ff;
  background:linear-gradient(135deg, rgba(139,92,246,.14), rgba(59,130,246,.08));
  font-weight:800;
}
.form-helper-card svg{ color:#b98cff; font-size:1.25rem; }
.field-label{
  display:flex; align-items:center; gap:.45rem;
  color:#eef4ff !important; font-size:.9rem; font-weight:1000;
}
.field-label svg{ color:#a78bfa; font-size:1.05rem; }
.field-label small{
  margin-inline-start:.35rem; color:#9fb0c8; font-size:.72rem; font-weight:800;
}
.field-label--choice{ color:#ffffff !important; }
.segmented-tabs--clear{
  padding:.35rem; gap:.35rem; border-radius:16px;
  background:rgba(5,11,21,.9);
  border:1px solid rgba(139,92,246,.26);
}
.segmented-tabs--clear .seg-tab{
  border:1px solid rgba(119,136,170,.22) !important;
  border-radius:12px;
  background:linear-gradient(180deg, rgba(23,34,55,.82), rgba(10,17,28,.82));
  color:#cbd7ea;
}
.segmented-tabs--clear .seg-tab.active{
  color:#fff;
  border-color:rgba(167,139,250,.65) !important;
  background:linear-gradient(135deg,#7c3aed,#a855f7);
  box-shadow:0 12px 32px rgba(139,92,246,.22), inset 0 1px 0 rgba(255,255,255,.18);
}
.status-tabs--clear{
  padding:.35rem; gap:.35rem; border-radius:16px;
  background:rgba(5,11,21,.9);
}
.status-tabs--clear .status-tab{
  border:1px solid rgba(119,136,170,.22) !important;
  border-radius:12px;
}
.upload-panel{ position:relative; }
.upload-panel::before{
  content:'ملف الإعلان'; position:absolute; top:12px; inset-inline-start:1rem;
  padding:.25rem .55rem; border-radius:999px;
  background:rgba(139,92,246,.14); color:#c4a7ff; font-size:.72rem; font-weight:1000;
}
.upload-drop{
  box-shadow:inset 0 0 0 1px rgba(168,85,247,.18), 0 18px 46px rgba(139,92,246,.08);
}
.upload-drop:hover{
  border-color:rgba(196,167,255,.95);
  background:radial-gradient(circle, rgba(139,92,246,.22), transparent 70%), rgba(139,92,246,.10);
}

@media (max-width: 1180px) {
  .editor-grid { grid-template-columns: 1fr; }
  .stats-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .table-toolbar { flex-direction: column; }
  .table-filters { min-width: 0; width: 100%; }
}
@media (max-width: 760px) {
  .page-header,
  .guide-banner,
  .metric-card,
  .panel-title { flex-direction: column; align-items: stretch; }
  .top-actions,
  .save-row,
  .form-grid,
  .stats-grid,
  .table-filters,
  .ad-preview-card,
  .segmented-tabs,
  .status-tabs,
  .product-results { grid-template-columns: 1fr; }
  .top-actions { display:grid; }
  .seg-tab,
  .status-tab { border-inline-start: 0; border-top: 1px solid rgba(119,136,170,.18); }
  .seg-tab:first-child,
  .status-tab:first-child { border-top:0; }
}
</style>
