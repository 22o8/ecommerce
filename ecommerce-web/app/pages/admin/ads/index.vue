<template>
  <div class="ads-admin-page" dir="rtl">
    <section class="ads-hero-card">
      <div class="hero-copy">
        <span class="eyebrow">مركز الإعلانات</span>
        <h1>إدارة الإعلانات</h1>
        <p>
          أنشئ إعلاناً، اختَر مكان ظهوره، وارفع الصورة أو الفيديو. الإعلان يظهر مباشرة في القائمة اليمنى بعد الحفظ.
        </p>
      </div>

      <div class="hero-actions">
        <button type="button" class="soft-btn" @click="loadAll(true)">
          <Icon name="mdi:refresh" />
          تحديث
        </button>
        <button type="button" class="soft-btn primary" @click="startCreate">
          <Icon name="mdi:plus" />
          إعلان جديد
        </button>
        <button type="button" class="soft-btn danger" @click="deleteAllAds">
          <Icon name="mdi:trash-can-outline" />
          حذف الكل
        </button>
      </div>
    </section>

    <section class="ads-stats-grid">
      <article v-for="card in statCards" :key="card.label" class="stat-card">
        <Icon :name="card.icon" />
        <strong>{{ card.value }}</strong>
        <span>{{ card.label }}</span>
      </article>
    </section>

    <section v-if="errorMessage" class="ads-alert danger">
      <Icon name="mdi:alert-circle-outline" />
      <div>
        <b>تنبيه</b>
        <p>{{ errorMessage }}</p>
      </div>
      <button type="button" @click="loadAll(true)">إعادة المحاولة</button>
    </section>

    <section v-if="successMessage" class="ads-alert success">
      <Icon name="mdi:check-circle-outline" />
      <div>
        <b>تم بنجاح</b>
        <p>{{ successMessage }}</p>
      </div>
      <button type="button" @click="successMessage = ''">إغلاق</button>
    </section>

    <section class="ads-main-grid">
      <aside class="ads-library">
        <div class="panel-head">
          <div>
            <span class="eyebrow">الإعلانات المحفوظة</span>
            <h2>قائمة الإعلانات</h2>
            <p>أي إعلان تحفظه يظهر هنا فوراً، وتقدر تعدله أو توقفه أو تحذفه.</p>
          </div>
          <span class="library-count">{{ filteredAds.length }}</span>
        </div>

        <div class="library-tools">
          <label class="search-box">
            <Icon name="mdi:magnify" />
            <input v-model="search" placeholder="ابحث بعنوان الإعلان أو الموضع..." />
          </label>

          <select v-model="filter" class="select-box">
            <option value="all">كل الإعلانات</option>
            <option value="active">المفعلة</option>
            <option value="disabled">المعطلة</option>
            <option value="slider">سلايدر</option>
            <option value="banner">بانر</option>
            <option value="popup">منبثق</option>
            <option value="product">داخل منتج</option>
          </select>
        </div>

        <div v-if="loading" class="empty-state">
          <Icon name="mdi:loading" class="spin" />
          <b>جاري تحميل الإعلانات...</b>
        </div>

        <div v-else-if="!filteredAds.length" class="empty-state">
          <Icon name="mdi:bullhorn-outline" />
          <b>لا توجد إعلانات ظاهرة هنا</b>
          <p v-if="ads.length">
            يوجد {{ ads.length }} إعلان لكن الفلتر الحالي يخفيها. اختر "كل الإعلانات".
          </p>
          <p v-else>
            أنشئ أول إعلان من النموذج، وسيظهر هنا مباشرة بدون تحديث الصفحة.
          </p>
          <button v-if="filter !== 'all' || search" type="button" class="soft-btn" @click="clearFilters">
            عرض كل الإعلانات
          </button>
        </div>

        <div v-else class="ad-cards">
          <article
            v-for="ad in filteredAds"
            :key="ad._localKey"
            class="ad-card"
            :class="{ selected: selectedId === ad.id, fresh: freshId === ad.id }"
          >
            <button type="button" class="ad-thumb" @click="editAd(ad)">
              <video
                v-if="isVideo(primaryMedia(ad))"
                :src="asset(primaryMedia(ad))"
                muted
                playsinline
              />
              <img v-else-if="primaryMedia(ad)" :src="asset(primaryMedia(ad))" alt="" />
              <Icon v-else :name="typeIcon(ad.type)" />
            </button>

            <div class="ad-body">
              <div class="ad-title-row">
                <b>{{ ad.title || fallbackTitle(ad) }}</b>
                <span :class="['status-pill', ad.isEnabled ? 'active' : 'paused']">
                  {{ ad.isEnabled ? 'مفعل' : 'متوقف' }}
                </span>
              </div>

              <p>{{ ad.subtitle || 'بدون وصف مختصر' }}</p>

              <div class="ad-meta">
                <span>{{ typeLabel(ad.type) }}</span>
                <span>{{ placementLabel(ad.placement) }}</span>
                <span v-if="mediaList(ad).length">{{ mediaList(ad).length }} ملف</span>
              </div>

              <small class="ltr">{{ ad.linkUrl || 'بدون رابط' }}</small>

              <div class="ad-actions">
                <button type="button" @click="editAd(ad)">
                  <Icon name="mdi:pencil-outline" />
                  تعديل
                </button>
                <button type="button" @click="toggleAd(ad)">
                  <Icon :name="ad.isEnabled ? 'mdi:pause-circle-outline' : 'mdi:play-circle-outline'" />
                  {{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}
                </button>
                <button type="button" class="danger-text" @click="deleteAd(ad)">
                  <Icon name="mdi:delete-outline" />
                  حذف
                </button>
              </div>
            </div>
          </article>
        </div>
      </aside>

      <form class="ads-editor" @submit.prevent="saveAd">
        <div class="panel-head">
          <div>
            <span class="eyebrow">محرر الإعلان</span>
            <h2>{{ selectedId ? 'تعديل الإعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p>اتبع الخطوات: نوع الإعلان، مكان الظهور، المحتوى، ثم الحفظ.</p>
          </div>

          <button v-if="selectedId" type="button" class="soft-btn" @click="startCreate">
            <Icon name="mdi:plus" />
            جديد
          </button>
        </div>

        <div class="step-card">
          <div class="step-number">1</div>
          <div class="step-content">
            <h3>نوع الإعلان</h3>
            <p>اختَر النوع الذي تريد ظهوره للزائر.</p>

            <div class="ad-type-grid">
              <button
                v-for="type in adTypes"
                :key="type.value"
                type="button"
                class="type-choice"
                :class="{ active: form.type === type.value }"
                @click="changeType(type.value)"
              >
                <Icon :name="type.icon" />
                <b>{{ type.label }}</b>
                <small>{{ type.help }}</small>
              </button>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-number">2</div>
          <div class="step-content">
            <h3>مكان الظهور</h3>
            <p>المواضع تتغير حسب نوع الإعلان حتى لا يصير خلط.</p>

            <div class="form-grid">
              <label>
                <span>الموضع</span>
                <select v-model="form.placement" class="input">
                  <option v-for="p in placementsForType" :key="p.value" :value="p.value">
                    {{ p.label }}
                  </option>
                </select>
              </label>

              <label>
                <span>الترتيب</span>
                <input v-model.number="form.sortOrder" type="number" class="input" />
              </label>
            </div>

            <div v-if="form.type === 'product'" class="product-picker">
              <label>
                <span>اختيار المنتج</span>
                <div class="search-box solid">
                  <Icon name="mdi:magnify" />
                  <input v-model="productSearch" placeholder="ابحث عن اسم المنتج أو البراند..." />
                </div>
              </label>

              <div class="product-grid">
                <button
                  v-for="product in filteredProducts"
                  :key="product.id"
                  type="button"
                  class="product-card"
                  :class="{ active: form.productId === product.id }"
                  @click="selectProduct(product)"
                >
                  <img v-if="productImage(product)" :src="asset(productImage(product))" alt="" />
                  <span v-else class="product-fallback">
                    <Icon name="mdi:package-variant-closed" />
                  </span>
                  <b>{{ productName(product) }}</b>
                  <small>{{ productBrand(product) || 'بدون براند' }}</small>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="step-card">
          <div class="step-number">3</div>
          <div class="step-content">
            <h3>النص والصورة</h3>
            <p>المنبثق يمكن أن يكون نصاً فقط، أما السلايدر والبانر يفضّل لها صورة أو فيديو.</p>

            <label>
              <span>العنوان</span>
              <input v-model="form.title" class="input" placeholder="مثال: عروض العناية الكورية" />
            </label>

            <label>
              <span>الوصف المختصر</span>
              <input v-model="form.subtitle" class="input" placeholder="نص قصير يظهر تحت العنوان" />
            </label>

            <div class="upload-zone">
              <Icon name="mdi:cloud-upload-outline" />
              <b>{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع صورة أو فيديو' }}</b>
              <small>{{ form.type === 'slider' ? 'يمكن رفع أكثر من ملف للسلايدر' : 'ملف واحد يكفي' }}</small>
              <input
                type="file"
                accept="image/*,video/mp4,video/webm"
                :multiple="form.type === 'slider'"
                @change="pickFiles"
              />
            </div>

            <label>
              <span>رابط صورة/فيديو يدوي</span>
              <input v-model="form.imageUrl" class="input ltr" placeholder="https://..." />
            </label>

            <label>
              <span>الرابط عند الضغط</span>
              <input v-model="form.linkUrl" class="input ltr" placeholder="/products" />
            </label>

            <label class="check-row">
              <input v-model="form.isEnabled" type="checkbox" />
              <span>الإعلان مفعل ويظهر للزوار</span>
            </label>
          </div>
        </div>

        <div class="live-preview">
          <div>
            <span class="eyebrow">معاينة قبل الحفظ</span>
            <h3>{{ form.title || fallbackTitle(form) }}</h3>
            <p>{{ form.subtitle || 'هنا يظهر وصف الإعلان للزائر.' }}</p>
            <div class="ad-meta">
              <span>{{ typeLabel(form.type) }}</span>
              <span>{{ placementLabel(form.placement) }}</span>
            </div>
          </div>

          <div class="preview-media">
            <template v-if="previewMedia.length">
              <div v-for="(url, index) in previewMedia" :key="`${url}-${index}`" class="preview-tile">
                <video v-if="isVideo(url)" :src="asset(url)" muted playsinline controls />
                <img v-else :src="asset(url)" alt="" />
                <button type="button" @click="removeMedia(index)">×</button>
              </div>
            </template>
            <div v-else class="preview-empty">
              <Icon :name="typeIcon(form.type)" />
              <span>{{ form.type === 'popup' ? 'يمكن حفظ المنبثق كنص فقط' : 'ارفع صورة أو فيديو للمعاينة' }}</span>
            </div>
          </div>
        </div>

        <div class="editor-footer">
          <button type="button" class="soft-btn" @click="resetFormOnly">تفريغ الحقول</button>
          <button type="submit" class="save-btn" :disabled="saving || uploading">
            <Icon :name="saving ? 'mdi:loading' : 'mdi:content-save-outline'" :class="{ spin: saving }" />
            {{ saving ? 'جاري الحفظ...' : selectedId ? 'حفظ التعديل' : 'إنشاء الإعلان' }}
          </button>
        </div>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

const api = useApi()
const directUpload = useDirectAdminUpload()
const toast = useToast()

type AdType = 'slider' | 'banner' | 'popup' | 'product'

type NormalizedAd = {
  id: string
  _localKey: string
  type: AdType
  placement: string
  title: string
  subtitle: string
  imageUrl: string
  imageUrls: string[]
  linkUrl: string
  productId: string
  productTitle?: string
  sortOrder: number
  isEnabled: boolean
  startAt?: any
  endAt?: any
  createdAt?: any
  updatedAt?: any
}

const loading = ref(false)
const saving = ref(false)
const uploading = ref(false)
const ads = ref<NormalizedAd[]>([])
const products = ref<any[]>([])
const filter = ref('all')
const search = ref('')
const productSearch = ref('')
const selectedId = ref<string | null>(null)
const freshId = ref<string | null>(null)
const errorMessage = ref('')
const successMessage = ref('')

const adTypes = [
  { value: 'slider' as AdType, label: 'سلايدر', icon: 'mdi:view-carousel-outline', help: 'صور أو فيديو متحرك فوق الهيرو أو أعلى/آخر الصفحات' },
  { value: 'banner' as AdType, label: 'بانر', icon: 'mdi:image-outline', help: 'إعلان ثابت في موضع واضح داخل الصفحة' },
  { value: 'popup' as AdType, label: 'إعلان منبثق', icon: 'mdi:bell-ring-outline', help: 'نافذة تظهر للزائر ويمكن أن تكون نصاً فقط' },
  { value: 'product' as AdType, label: 'داخل منتج', icon: 'mdi:package-variant-closed', help: 'إعلان مرتبط بمنتج محدد من البحث' },
]

const placementOptions = [
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
  type: 'slider' as AdType,
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

const placementsForType = computed(() => placementOptions.filter((p) => p.type === form.type))

const statCards = computed(() => [
  { label: 'كل الإعلانات', value: ads.value.length, icon: 'mdi:bullhorn-outline' },
  { label: 'مفعلة', value: ads.value.filter((x) => x.isEnabled).length, icon: 'mdi:check-circle-outline' },
  { label: 'سلايدر', value: ads.value.filter((x) => x.type === 'slider').length, icon: 'mdi:view-carousel-outline' },
  { label: 'بانر', value: ads.value.filter((x) => x.type === 'banner').length, icon: 'mdi:image-outline' },
  { label: 'منبثقة', value: ads.value.filter((x) => x.type === 'popup').length, icon: 'mdi:bell-ring-outline' },
])

const filteredAds = computed(() => {
  const q = search.value.trim().toLowerCase()

  return ads.value.filter((ad) => {
    const matchesFilter =
      filter.value === 'all'
      || (filter.value === 'active' && ad.isEnabled)
      || (filter.value === 'disabled' && !ad.isEnabled)
      || ad.type === filter.value

    const haystack = `${ad.title} ${ad.subtitle} ${ad.placement} ${typeLabel(ad.type)} ${placementLabel(ad.placement)} ${ad.linkUrl}`.toLowerCase()
    const matchesSearch = !q || haystack.includes(q)

    return matchesFilter && matchesSearch
  })
})

const previewMedia = computed(() => {
  const list = [...(form.imageUrls || [])].filter(Boolean)
  if (!list.length && form.imageUrl) list.push(form.imageUrl)
  return Array.from(new Set(list))
})

const filteredProducts = computed(() => {
  const q = productSearch.value.trim().toLowerCase()
  const source = products.value || []
  if (!q) return source.slice(0, 12)

  return source
    .filter((p: any) => `${productName(p)} ${productBrand(p)} ${p.slug || ''}`.toLowerCase().includes(q))
    .slice(0, 18)
})

function clearFilters() {
  filter.value = 'all'
  search.value = ''
}

function typeIcon(type: string) {
  if (type === 'popup') return 'mdi:bell-ring-outline'
  if (type === 'product') return 'mdi:package-variant-closed'
  if (type === 'slider') return 'mdi:view-carousel-outline'
  return 'mdi:image-outline'
}

function typeLabel(type: string) {
  return adTypes.find((x) => x.value === type)?.label || type || 'إعلان'
}

function placementLabel(value: string) {
  return placementOptions.find((x) => x.value === value)?.label || value || 'بدون موضع'
}

function fallbackTitle(ad: any) {
  if (ad?.type === 'popup') return 'إعلان منبثق'
  if (ad?.type === 'product') return 'إعلان داخل منتج'
  if (ad?.type === 'slider') return 'سلايدر'
  return 'بانر'
}

function enumType(value: any): AdType {
  const raw = String(value ?? '').trim().toLowerCase()
  if (raw === '1') return 'popup'
  if (raw === '2') return 'banner'
  if (raw === '3') return 'product'
  if (raw === '4') return 'slider'
  if (['popup', 'banner', 'product', 'slider'].includes(raw)) return raw as AdType
  if (raw.includes('popup')) return 'popup'
  if (raw.includes('product')) return 'product'
  if (raw.includes('slider') || raw.includes('carousel')) return 'slider'
  return 'banner'
}

function unwrapArray(input: any): any[] {
  if (!input) return []

  if (typeof input === 'string') {
    try {
      return unwrapArray(JSON.parse(input))
    } catch {
      return []
    }
  }

  if (Array.isArray(input)) return input
  if (Array.isArray(input?.$values)) return input.$values
  if (Array.isArray(input?.items)) return input.items
  if (Array.isArray(input?.data)) return input.data
  if (Array.isArray(input?.value)) return input.value
  if (Array.isArray(input?.result)) return input.result
  if (Array.isArray(input?.ads)) return input.ads
  if (Array.isArray(input?.items?.$values)) return input.items.$values
  if (Array.isArray(input?.data?.$values)) return input.data.$values
  if (Array.isArray(input?.result?.$values)) return input.result.$values
  if (Array.isArray(input?.ads?.$values)) return input.ads.$values

  return []
}

function unwrapImageUrls(ad: any): string[] {
  const raw =
    ad?.imageUrls
    ?? ad?.ImageUrls
    ?? ad?.image_urls
    ?? ad?.ImageUrlsJson
    ?? ad?.imageUrlsJson
    ?? null

  let arr: any[] = []

  if (Array.isArray(raw)) arr = raw
  else if (Array.isArray(raw?.$values)) arr = raw.$values
  else if (typeof raw === 'string' && raw.trim().startsWith('[')) {
    try { arr = JSON.parse(raw) } catch { arr = [] }
  }

  const primary = ad?.imageUrl ?? ad?.ImageUrl ?? ''
  if (!arr.length && primary) arr = [primary]

  return Array.from(new Set(arr.map((x) => String(x || '').trim()).filter(Boolean)))
}

function normalizeAd(raw: any, index = 0): NormalizedAd | null {
  if (!raw || typeof raw !== 'object') return null

  const id = String(raw.id ?? raw.Id ?? raw.adId ?? raw.AdId ?? '').trim()
  const type = enumType(raw.type ?? raw.Type)
  const imageUrls = unwrapImageUrls(raw)
  const imageUrl = String(raw.imageUrl ?? raw.ImageUrl ?? imageUrls[0] ?? '').trim()
  const title = String(raw.title ?? raw.Title ?? '').trim()
  const subtitle = String(raw.subtitle ?? raw.Subtitle ?? '').trim()
  const placement = String(raw.placement ?? raw.Placement ?? defaultPlacement(type)).trim()
  const productId = String(raw.productId ?? raw.ProductId ?? '').trim()

  if (!id && !title && !subtitle && !imageUrl && !imageUrls.length) return null

  return {
    id: id || `local-${Date.now()}-${index}`,
    _localKey: id || `local-${Date.now()}-${index}`,
    type,
    placement,
    title,
    subtitle,
    imageUrl,
    imageUrls,
    linkUrl: String(raw.linkUrl ?? raw.LinkUrl ?? '').trim(),
    productId,
    sortOrder: Number(raw.sortOrder ?? raw.SortOrder ?? 0),
    isEnabled: (raw.isEnabled ?? raw.IsEnabled ?? true) !== false,
    createdAt: raw.createdAt ?? raw.CreatedAt,
    updatedAt: raw.updatedAt ?? raw.UpdatedAt,
  }
}

function normalizeAds(input: any): NormalizedAd[] {
  return unwrapArray(input)
    .map((x, i) => normalizeAd(x, i))
    .filter(Boolean) as NormalizedAd[]
}

function normalizeProducts(input: any) {
  return unwrapArray(input)
}

function mergeAds(remote: NormalizedAd[], keepCurrentIfRemoteEmpty = true) {
  if (!remote.length && keepCurrentIfRemoteEmpty && ads.value.length) return

  const map = new Map<string, NormalizedAd>()

  for (const ad of remote) map.set(ad.id, ad)
  for (const ad of ads.value) {
    if (!map.has(ad.id) && ad.id.startsWith('local-')) map.set(ad.id, ad)
  }

  ads.value = Array.from(map.values())
    .sort((a, b) => {
      if (a.type !== b.type) return a.type.localeCompare(b.type)
      if (a.placement !== b.placement) return a.placement.localeCompare(b.placement)
      return (a.sortOrder || 0) - (b.sortOrder || 0)
    })
}

async function loadAll(manual = false) {
  loading.value = true
  errorMessage.value = ''

  try {
    const [adminAds, activeAds, productsRes]: any[] = await Promise.all([
      $fetch('/api/bff/admin/ads', {
        query: { _ts: Date.now() },
        headers: { 'cache-control': 'no-cache' },
      }).catch((e) => ({ __error: e })),
      $fetch('/api/bff/ads/active', {
        query: { _ts: Date.now() },
        headers: { 'cache-control': 'no-cache' },
      }).catch(() => []),
      $fetch('/api/bff/admin/products', {
        query: { page: 1, pageSize: 300, _ts: Date.now() },
        headers: { 'cache-control': 'no-cache' },
      }).catch(() => []),
    ])

    if (adminAds?.__error) {
      throw adminAds.__error
    }

    const adminList = normalizeAds(adminAds)
    const activeList = normalizeAds(activeAds)
    const combined = [...adminList]

    for (const ad of activeList) {
      if (!combined.some((x) => x.id === ad.id)) combined.push(ad)
    }

    mergeAds(combined, true)
    products.value = normalizeProducts(productsRes)

    if (manual) {
      successMessage.value = `تم تحديث القائمة. عدد الإعلانات: ${ads.value.length}`
      setTimeout(() => { successMessage.value = '' }, 3000)
    }
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'تعذر تحميل الإعلانات من الخادم'
  } finally {
    loading.value = false
  }
}

function defaultPlacement(type: AdType) {
  if (type === 'slider') return 'home_hero_slider'
  if (type === 'popup') return 'popup'
  if (type === 'product') return 'product_page'
  return 'home_hero_top'
}

function changeType(type: AdType) {
  form.type = type
  form.placement = defaultPlacement(type)

  if (type === 'product') {
    form.linkUrl = form.productId ? `/product/${form.productId}` : '/products'
  }
}

function resetFormOnly() {
  form.title = ''
  form.subtitle = ''
  form.imageUrl = ''
  form.imageUrls = []
  form.linkUrl = form.type === 'product' && form.productId ? `/product/${form.productId}` : '/products'
  form.sortOrder = 0
  form.isEnabled = true
}

function startCreate() {
  selectedId.value = null
  productSearch.value = ''
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
}

function editAd(ad: NormalizedAd) {
  selectedId.value = ad.id
  Object.assign(form, {
    type: ad.type,
    placement: ad.placement || defaultPlacement(ad.type),
    title: ad.title || '',
    subtitle: ad.subtitle || '',
    imageUrl: ad.imageUrl || '',
    imageUrls: [...(ad.imageUrls || [])],
    linkUrl: ad.linkUrl || '/products',
    productId: ad.productId || '',
    productTitle: ad.productTitle || '',
    sortOrder: Number(ad.sortOrder || 0),
    isEnabled: ad.isEnabled !== false,
  })

  const product = products.value.find((p: any) => String(p.id) === String(ad.productId))
  productSearch.value = product ? productName(product) : ''
  if (process.client) window.scrollTo({ top: 0, behavior: 'smooth' })
}

function payload() {
  return {
    type: form.type,
    placement: form.placement || defaultPlacement(form.type),
    title: form.title || fallbackTitle(form),
    subtitle: form.subtitle || null,
    imageUrl: form.imageUrl || previewMedia.value[0] || '',
    imageUrls: previewMedia.value,
    linkUrl: form.linkUrl || null,
    productId: form.type === 'product' && form.productId ? form.productId : null,
    sortOrder: Number(form.sortOrder || 0),
    isEnabled: Boolean(form.isEnabled),
    startAt: null,
    endAt: null,
  }
}

function localAdFromPayload(saved: any, body: any): NormalizedAd {
  return normalizeAd(saved || { ...body, id: selectedId.value || `local-${Date.now()}` })!
}

async function saveAd() {
  errorMessage.value = ''
  successMessage.value = ''

  if (form.type !== 'popup' && !previewMedia.value.length) {
    errorMessage.value = 'ارفع صورة أو فيديو للبانر أو السلايدر أو إعلان المنتج.'
    return
  }

  if (form.type === 'popup' && !String(form.title || form.subtitle || previewMedia.value[0] || '').trim()) {
    errorMessage.value = 'الإعلان المنبثق يحتاج عنواناً أو وصفاً أو صورة.'
    return
  }

  if (form.type === 'product' && !form.productId) {
    errorMessage.value = 'اختر المنتج أولاً حتى يظهر الإعلان داخل صفحة المنتج.'
    return
  }

  saving.value = true

  try {
    const body = payload()
    const saved: any = selectedId.value && !selectedId.value.startsWith('local-')
      ? await $fetch(`/api/bff/admin/ads/${selectedId.value}`, { method: 'PUT', body })
      : await $fetch('/api/bff/admin/ads', { method: 'POST', body })

    const normalized = localAdFromPayload(saved, body)

    const idx = ads.value.findIndex((x) => x.id === normalized.id || (selectedId.value && x.id === selectedId.value))
    if (idx >= 0) ads.value.splice(idx, 1, normalized)
    else ads.value.unshift(normalized)

    freshId.value = normalized.id
    filter.value = 'all'
    search.value = ''
    selectedId.value = normalized.id
    successMessage.value = 'تم حفظ الإعلان وظهر في القائمة.'

    if (process.client) {
      window.dispatchEvent(new CustomEvent('ads:changed'))
    }

    setTimeout(() => {
      if (freshId.value === normalized.id) freshId.value = null
    }, 4000)

    await loadAll(false)
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'تعذر حفظ الإعلان'
  } finally {
    saving.value = false
  }
}

async function toggleAd(ad: NormalizedAd) {
  const body = {
    type: ad.type,
    placement: ad.placement,
    title: ad.title || fallbackTitle(ad),
    subtitle: ad.subtitle || null,
    imageUrl: ad.imageUrl || mediaList(ad)[0] || '',
    imageUrls: mediaList(ad),
    linkUrl: ad.linkUrl || null,
    productId: ad.type === 'product' && ad.productId ? ad.productId : null,
    sortOrder: ad.sortOrder || 0,
    isEnabled: !ad.isEnabled,
    startAt: null,
    endAt: null,
  }

  try {
    const updated: any = await $fetch(`/api/bff/admin/ads/${ad.id}`, { method: 'PUT', body })
    const normalized = localAdFromPayload(updated, body)
    const idx = ads.value.findIndex((x) => x.id === ad.id)
    if (idx >= 0) ads.value.splice(idx, 1, normalized)
    if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'تعذر تغيير حالة الإعلان'
  }
}

async function deleteAd(ad: NormalizedAd) {
  if (!confirm('هل تريد حذف هذا الإعلان؟')) return

  try {
    if (!ad.id.startsWith('local-')) {
      await $fetch(`/api/bff/admin/ads/${ad.id}`, { method: 'DELETE' })
    }
    ads.value = ads.value.filter((x) => x.id !== ad.id)
    if (selectedId.value === ad.id) startCreate()
    successMessage.value = 'تم حذف الإعلان.'
    if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'تعذر حذف الإعلان'
  }
}

async function deleteAllAds() {
  if (!confirm('سيتم حذف كل الإعلانات. هل أنت متأكد؟')) return

  try {
    await $fetch('/api/bff/admin/ads', { method: 'DELETE' })
    ads.value = []
    startCreate()
    successMessage.value = 'تم حذف جميع الإعلانات.'
    if (process.client) window.dispatchEvent(new CustomEvent('ads:changed'))
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'تعذر حذف الإعلانات'
  }
}

async function pickFiles(event: Event) {
  const input = event.target as HTMLInputElement
  const files = Array.from(input.files || [])
  if (!files.length) return

  uploading.value = true

  try {
    const uploaded: string[] = []

    for (const file of files) {
      const url = await directUpload.upload('admin/ads/upload', file, {
        maxMb: 150,
        fallbackToBff: true,
      })
      if (url) uploaded.push(url)
    }

    if (form.type === 'slider') {
      form.imageUrls = Array.from(new Set([...form.imageUrls, ...uploaded]))
      form.imageUrl = form.imageUrls[0] || ''
    } else {
      form.imageUrl = uploaded[0] || ''
      form.imageUrls = form.imageUrl ? [form.imageUrl] : []
    }

    successMessage.value = 'تم رفع الملف بنجاح.'
  } catch (e: any) {
    errorMessage.value = e?.data?.message || e?.message || 'تعذر رفع الملف'
  } finally {
    uploading.value = false
    input.value = ''
  }
}

function removeMedia(index: number) {
  const list = [...previewMedia.value]
  list.splice(index, 1)
  form.imageUrls = list
  form.imageUrl = list[0] || ''
}

function mediaList(ad: any): string[] {
  const list = Array.isArray(ad?.imageUrls) ? ad.imageUrls.filter(Boolean) : []
  if (!list.length && ad?.imageUrl) return [ad.imageUrl]
  return list
}

function primaryMedia(ad: any) {
  return mediaList(ad)[0] || ''
}

function isVideo(url: string) {
  return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '')
}

function asset(url: string) {
  return api.buildAssetUrl ? api.buildAssetUrl(url) : url
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

function selectProduct(product: any) {
  form.productId = product.id
  form.productTitle = productName(product)
  form.linkUrl = `/product/${product.id}`
  productSearch.value = productName(product)
}

watch(() => form.imageUrl, (value) => {
  if (value && !form.imageUrls.length) form.imageUrls = [value]
})

watch(() => form.type, () => {
  if (!placementsForType.value.some((p) => p.value === form.placement)) {
    form.placement = defaultPlacement(form.type)
  }
})

await loadAll(false)
</script>

<style scoped>
.ads-admin-page {
  display: grid;
  gap: 1.25rem;
  padding-bottom: 4rem;
}

.ads-hero-card,
.stat-card,
.ads-library,
.ads-editor,
.step-card,
.live-preview,
.ads-alert {
  border: 1px solid rgba(var(--border), .78);
  background:
    radial-gradient(circle at top right, rgba(var(--primary), .11), transparent 36%),
    rgba(var(--surface-rgb), .88);
  box-shadow: 0 24px 90px rgba(0, 0, 0, .17);
  border-radius: 30px;
}

.ads-hero-card {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.35rem;
}

.eyebrow {
  display: inline-flex;
  margin-bottom: .45rem;
  color: rgb(var(--primary));
  font-size: .72rem;
  font-weight: 1000;
}

h1,
h2,
h3,
p {
  margin: 0;
}

h1,
.panel-head h2 {
  color: rgb(var(--text));
  font-weight: 1000;
  letter-spacing: -.04em;
}

h1 {
  font-size: clamp(1.6rem, 3vw, 2.65rem);
}

.panel-head h2 {
  font-size: clamp(1.25rem, 2vw, 1.9rem);
}

.hero-copy p,
.panel-head p,
.step-content p,
.live-preview p,
.empty-state p {
  margin-top: .38rem;
  color: rgb(var(--muted));
  line-height: 1.8;
}

.hero-actions,
.ad-actions,
.editor-footer {
  display: flex;
  flex-wrap: wrap;
  gap: .55rem;
}

.soft-btn,
.save-btn,
.ad-actions button,
.ads-alert button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: .35rem;
  border: 1px solid rgba(var(--border), .78);
  background: rgba(var(--surface-2-rgb), .78);
  color: rgb(var(--text));
  border-radius: 999px;
  padding: .78rem 1rem;
  font-weight: 1000;
  transition: .2s ease;
}

.soft-btn:hover,
.ad-actions button:hover,
.ads-alert button:hover {
  transform: translateY(-1px);
  border-color: rgba(var(--primary), .55);
}

.soft-btn.primary,
.save-btn {
  border-color: rgba(var(--primary), .5);
  background: linear-gradient(135deg, rgb(var(--primary)), rgba(var(--primary), .75));
  color: white;
}

.soft-btn.danger,
.danger-text,
.ads-alert.danger button {
  border-color: rgba(239, 68, 68, .35);
  background: rgba(239, 68, 68, .1);
  color: rgb(248, 113, 113);
}

.ads-stats-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: .8rem;
}

.stat-card {
  display: grid;
  gap: .4rem;
  padding: 1rem;
}

.stat-card svg {
  width: 1.35rem;
  height: 1.35rem;
  color: rgb(var(--primary));
}

.stat-card strong {
  color: rgb(var(--text));
  font-size: 2rem;
  line-height: 1;
}

.stat-card span {
  color: rgb(var(--muted));
  font-size: .83rem;
  font-weight: 900;
}

.ads-alert {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: .75rem;
  padding: .9rem 1rem;
}

.ads-alert > svg {
  width: 1.6rem;
  height: 1.6rem;
}

.ads-alert.danger {
  border-color: rgba(239, 68, 68, .35);
}

.ads-alert.danger > svg {
  color: rgb(248, 113, 113);
}

.ads-alert.success {
  border-color: rgba(34, 197, 94, .35);
}

.ads-alert.success > svg {
  color: rgb(74, 222, 128);
}

.ads-alert b {
  color: rgb(var(--text));
}

.ads-alert p {
  color: rgb(var(--muted));
}

.ads-main-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(430px, 580px);
  gap: 1rem;
  align-items: start;
}

.ads-editor {
  grid-column: 2;
  grid-row: 1;
  padding: 1rem;
}

.ads-library {
  grid-column: 1;
  grid-row: 1;
  padding: 1rem;
  position: sticky;
  top: 1rem;
  max-height: calc(100vh - 2rem);
  overflow: auto;
}

.panel-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  margin-bottom: 1rem;
}

.library-count {
  display: grid;
  place-items: center;
  min-width: 42px;
  height: 42px;
  border-radius: 16px;
  background: rgba(var(--primary), .15);
  color: rgb(var(--primary));
  font-weight: 1000;
}

.library-tools {
  display: grid;
  grid-template-columns: 1fr 190px;
  gap: .65rem;
  margin-bottom: .9rem;
}

.search-box,
.input,
.select-box {
  width: 100%;
  border: 1px solid rgba(var(--border), .78);
  background: rgba(var(--surface-2-rgb), .78);
  color: rgb(var(--text));
  border-radius: 18px;
  padding: .82rem .95rem;
  outline: none;
}

.search-box {
  display: flex;
  align-items: center;
  gap: .5rem;
}

.search-box input {
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: rgb(var(--text));
}

.search-box.solid {
  padding: .2rem .75rem;
}

.input:focus,
.select-box:focus,
.search-box:focus-within {
  border-color: rgba(var(--primary), .65);
  box-shadow: 0 0 0 4px rgba(var(--primary), .1);
}

.ad-cards {
  display: grid;
  gap: .75rem;
}

.ad-card {
  display: grid;
  grid-template-columns: 96px 1fr;
  gap: .85rem;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-2-rgb), .58);
  border-radius: 24px;
  padding: .75rem;
  transition: .22s ease;
}

.ad-card:hover,
.ad-card.selected,
.ad-card.fresh {
  border-color: rgba(var(--primary), .68);
  background: rgba(var(--primary), .1);
  transform: translateY(-2px);
}

.ad-thumb {
  display: grid;
  place-items: center;
  width: 96px;
  height: 92px;
  border: 1px solid rgba(var(--border), .65);
  background: rgba(var(--surface-rgb), .78);
  color: rgb(var(--primary));
  border-radius: 20px;
  overflow: hidden;
}

.ad-thumb img,
.ad-thumb video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.ad-thumb svg {
  width: 2rem;
  height: 2rem;
}

.ad-body {
  min-width: 0;
}

.ad-title-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: .45rem;
}

.ad-title-row b {
  color: rgb(var(--text));
  font-weight: 1000;
}

.ad-body p {
  margin-top: .35rem;
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
  margin-top: .55rem;
}

.ad-meta span,
.status-pill {
  display: inline-flex;
  align-items: center;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-rgb), .7);
  color: rgb(var(--text));
  border-radius: 999px;
  padding: .25rem .55rem;
  font-size: .73rem;
  font-weight: 900;
}

.status-pill.active {
  border-color: rgba(34, 197, 94, .42);
  color: rgb(74, 222, 128);
}

.status-pill.paused {
  border-color: rgba(239, 68, 68, .38);
  color: rgb(248, 113, 113);
}

.ad-body small {
  display: block;
  margin-top: .45rem;
  color: rgb(var(--muted));
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ad-actions {
  margin-top: .65rem;
}

.ad-actions button {
  padding: .52rem .7rem;
  font-size: .78rem;
}

.step-card {
  display: grid;
  grid-template-columns: 48px 1fr;
  gap: .85rem;
  padding: .95rem;
  margin-top: .85rem;
}

.step-number {
  display: grid;
  place-items: center;
  width: 42px;
  height: 42px;
  border-radius: 16px;
  color: white;
  background: linear-gradient(135deg, rgb(var(--primary)), rgba(var(--primary), .65));
  font-weight: 1000;
}

.step-content {
  display: grid;
  gap: .75rem;
}

.step-content h3,
.live-preview h3 {
  color: rgb(var(--text));
  font-size: 1.05rem;
  font-weight: 1000;
}

.ad-type-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .6rem;
}

.type-choice {
  display: grid;
  gap: .35rem;
  text-align: right;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-2-rgb), .62);
  color: rgb(var(--text));
  border-radius: 20px;
  padding: .85rem;
  transition: .2s ease;
}

.type-choice:hover,
.type-choice.active {
  transform: translateY(-2px);
  border-color: rgba(var(--primary), .65);
  background: rgba(var(--primary), .12);
}

.type-choice svg {
  width: 1.35rem;
  height: 1.35rem;
  color: rgb(var(--primary));
}

.type-choice b {
  font-weight: 1000;
}

.type-choice small {
  color: rgb(var(--muted));
  line-height: 1.55;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 140px;
  gap: .65rem;
}

label {
  display: grid;
  gap: .38rem;
  color: rgb(var(--text));
  font-size: .86rem;
  font-weight: 900;
}

.product-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .55rem;
  max-height: 260px;
  overflow: auto;
  padding-inline-end: .25rem;
}

.product-card {
  display: grid;
  grid-template-columns: 52px 1fr;
  gap: .55rem;
  align-items: center;
  text-align: right;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-2-rgb), .62);
  color: rgb(var(--text));
  border-radius: 18px;
  padding: .55rem;
}

.product-card.active {
  border-color: rgba(var(--primary), .75);
  background: rgba(var(--primary), .12);
}

.product-card img,
.product-fallback {
  width: 52px;
  height: 52px;
  border-radius: 15px;
  object-fit: cover;
  background: rgba(var(--border), .18);
}

.product-fallback {
  display: grid;
  place-items: center;
}

.product-card small {
  color: rgb(var(--muted));
}

.upload-zone {
  position: relative;
  display: grid;
  place-items: center;
  gap: .35rem;
  min-height: 142px;
  border: 1px dashed rgba(var(--primary), .58);
  background: rgba(var(--primary), .08);
  color: rgb(var(--text));
  border-radius: 24px;
  text-align: center;
  overflow: hidden;
}

.upload-zone svg {
  width: 2rem;
  height: 2rem;
  color: rgb(var(--primary));
}

.upload-zone small {
  color: rgb(var(--muted));
}

.upload-zone input {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
}

.check-row {
  display: flex;
  align-items: center;
  gap: .55rem;
}

.live-preview {
  display: grid;
  gap: .9rem;
  padding: 1rem;
  margin-top: .85rem;
}

.preview-media {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: .6rem;
}

.preview-tile {
  position: relative;
  overflow: hidden;
  border: 1px solid rgba(var(--border), .72);
  background: rgba(var(--surface-2-rgb), .78);
  border-radius: 20px;
  min-height: 130px;
}

.preview-tile img,
.preview-tile video {
  width: 100%;
  height: 150px;
  object-fit: cover;
}

.preview-tile button {
  position: absolute;
  top: .42rem;
  inset-inline-end: .42rem;
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;
  border-radius: 999px;
  background: rgba(0, 0, 0, .65);
  color: white;
  font-weight: 1000;
}

.preview-empty,
.empty-state {
  display: grid;
  place-items: center;
  gap: .55rem;
  min-height: 170px;
  border: 1px dashed rgba(var(--border), .75);
  border-radius: 22px;
  color: rgb(var(--muted));
  text-align: center;
  padding: 1rem;
}

.preview-empty svg,
.empty-state svg {
  width: 2rem;
  height: 2rem;
  color: rgb(var(--primary));
}

.editor-footer {
  position: sticky;
  bottom: .75rem;
  justify-content: flex-end;
  margin-top: .9rem;
  padding: .75rem;
  border: 1px solid rgba(var(--border), .7);
  background: rgba(var(--surface-rgb), .88);
  backdrop-filter: blur(18px);
  border-radius: 24px;
}

.ltr {
  direction: ltr;
  unicode-bidi: plaintext;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(1turn); }
}

@media (max-width: 1220px) {
  .ads-main-grid {
    grid-template-columns: 1fr;
  }

  .ads-editor,
  .ads-library {
    grid-column: auto;
    grid-row: auto;
  }

  .ads-library {
    position: relative;
    top: auto;
    max-height: none;
  }
}

@media (max-width: 760px) {
  .ads-hero-card,
  .panel-head,
  .ads-alert {
    flex-direction: column;
  }

  .ads-stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .library-tools,
  .form-grid,
  .ad-type-grid,
  .product-grid {
    grid-template-columns: 1fr;
  }

  .step-card {
    grid-template-columns: 1fr;
  }

  .ad-card {
    grid-template-columns: 1fr;
  }

  .ad-thumb {
    width: 100%;
    height: 190px;
  }
}
</style>
