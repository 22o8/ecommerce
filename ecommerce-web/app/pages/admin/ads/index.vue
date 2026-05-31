<template>
  <div class="ads-admin" dir="rtl">
    <section class="ads-page-head">
      <div>
        <p class="ads-kicker">مركز الإعلانات</p>
        <h1>إدارة الإعلانات</h1>
        <p>
          أنشئ إعلاناً واضحاً وحدد مكان ظهوره. بعد الحفظ سيظهر الإعلان فوراً في القائمة اليمنى بدون تحديث الصفحة.
        </p>
      </div>

      <div class="ads-head-actions">
        <button type="button" class="admin-secondary" @click="load">تحديث</button>
        <button type="button" class="admin-secondary" @click="startNewAd">إعلان جديد</button>
        <button type="button" class="admin-danger" @click="removeAll">حذف الكل</button>
      </div>
    </section>

    <section class="ads-stats">
      <div class="ads-stat-card">
        <strong>{{ stats.total }}</strong>
        <span>كل الإعلانات</span>
      </div>
      <div class="ads-stat-card">
        <strong>{{ stats.active }}</strong>
        <span>مفعلة</span>
      </div>
      <div class="ads-stat-card">
        <strong>{{ stats.slider }}</strong>
        <span>سلايدر</span>
      </div>
      <div class="ads-stat-card">
        <strong>{{ stats.banner }}</strong>
        <span>بانر</span>
      </div>
      <div class="ads-stat-card">
        <strong>{{ stats.popup }}</strong>
        <span>منبثقة</span>
      </div>
    </section>

    <section class="ads-workspace">
      <form class="ads-builder-card" @submit.prevent="saveAd">
        <div class="builder-top">
          <div>
            <p class="ads-kicker">محرر الإعلان</p>
            <h2>{{ editingId ? 'تعديل إعلان' : 'إنشاء إعلان جديد' }}</h2>
            <p>اختر النوع، حدد مكان الظهور، أضف المحتوى، ثم احفظ.</p>
          </div>
          <span class="type-chip">{{ activeType.label }}</span>
        </div>

        <div class="form-section">
          <div class="section-number">1</div>
          <div class="section-body">
            <h3>نوع الإعلان</h3>

            <div class="type-grid">
              <button
                v-for="type in adTypes"
                :key="type.value"
                type="button"
                class="type-option"
                :class="{ active: form.type === type.value }"
                @click="selectType(type.value)"
              >
                <Icon :name="type.icon" class="type-icon" />
                <b>{{ type.label }}</b>
                <small>{{ type.hint }}</small>
              </button>
            </div>
          </div>
        </div>

        <div class="form-section">
          <div class="section-number">2</div>
          <div class="section-body">
            <h3>المكان والمحتوى</h3>

            <div class="form-grid">
              <label class="field">
                <span>موضع الظهور</span>
                <select v-model="form.placement" class="admin-input">
                  <option v-for="p in placementOptions" :key="p.value" :value="p.value">
                    {{ p.label }}
                  </option>
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
              <span>الوصف المختصر</span>
              <input v-model="form.subtitle" class="admin-input" placeholder="نص قصير يظهر تحت العنوان" />
            </label>

            <div v-if="form.type === 'product'" class="product-picker">
              <label class="field">
                <span>ابحث عن المنتج</span>
                <input v-model="productQuery" class="admin-input" placeholder="اكتب اسم المنتج أو البراند..." />
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
                  <img v-if="productImage(p)" :src="api.buildAssetUrl(productImage(p))" alt="" />
                  <span v-else class="product-placeholder">
                    <Icon name="mdi:package-variant-closed" />
                  </span>
                  <b>{{ productName(p) }}</b>
                  <small>{{ productBrand(p) || 'بدون براند' }}</small>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="form-section">
          <div class="section-number">3</div>
          <div class="section-body">
            <h3>الصورة أو الفيديو والرابط</h3>

            <div class="upload-box">
              <Icon name="mdi:cloud-upload-outline" />
              <b>{{ uploading ? 'جاري الرفع...' : 'اضغط لرفع الصورة أو الفيديو' }}</b>
              <small>
                {{ form.type === 'popup' ? 'اختياري للإعلان المنبثق' : 'مطلوب للبانر أو السلايدر' }}
              </small>
              <input
                type="file"
                accept="image/*,video/mp4,video/webm"
                :multiple="form.type === 'slider'"
                @change="onPickFile"
              />
            </div>

            <label class="field">
              <span>رابط صورة أو فيديو يدوي، إن وجد</span>
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

        <div class="preview-card">
          <div>
            <p class="ads-kicker">معاينة سريعة</p>
            <h3>{{ form.title || 'عنوان الإعلان' }}</h3>
            <p>{{ form.subtitle || 'وصف الإعلان يظهر هنا قبل الحفظ.' }}</p>
          </div>

          <div class="preview-media">
            <template v-if="previewImages.length">
              <div v-for="(img, i) in previewImages" :key="`${img}-${i}`" class="preview-item">
                <video v-if="isVideoUrl(img)" :src="api.buildAssetUrl(img)" muted playsinline controls />
                <img v-else :src="api.buildAssetUrl(img)" alt="" />
                <button type="button" @click="removePreview(i)">×</button>
              </div>
            </template>
            <div v-else class="preview-empty">
              <Icon :name="form.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-outline'" />
              <span>{{ form.type === 'popup' ? 'الإعلان المنبثق يمكن أن يكون نصاً فقط' : 'ارفع صورة أو فيديو للمعاينة' }}</span>
            </div>
          </div>
        </div>

        <div class="save-bar">
          <button type="button" class="admin-secondary" @click="startNewAd">تفريغ</button>
          <UiButton type="submit" :disabled="saving || uploading">
            {{ saving ? 'جاري الحفظ...' : (editingId ? 'حفظ التعديل' : 'إنشاء الإعلان') }}
          </UiButton>
        </div>
      </form>

      <aside class="ads-list-card">
        <div class="list-top">
          <div>
            <p class="ads-kicker">الإعلانات المرفوعة</p>
            <h2>قائمة الإعلانات</h2>
            <p>كل إعلان محفوظ يظهر هنا فوراً، ويمكن تعديله أو تعطيله أو حذفه.</p>
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

        <div v-if="loading" class="ads-empty">جاري تحميل الإعلانات...</div>

        <div v-else-if="!filteredAds.length" class="ads-empty">
          لا توجد إعلانات حسب هذا الفلتر. اختر "كل الإعلانات" أو أنشئ إعلاناً جديداً.
        </div>

        <div v-else class="ads-list">
          <article
            v-for="ad in filteredAds"
            :key="ad.id || `${ad.type}-${ad.placement}-${ad.title}`"
            class="ad-item"
            :class="{ fresh: lastSavedId === ad.id }"
          >
            <button type="button" class="ad-media" @click="edit(ad)">
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
              <Icon
                v-else
                :name="ad.type === 'popup' ? 'mdi:bell-ring-outline' : 'mdi:image-off-outline'"
              />
            </button>

            <div class="ad-info">
              <div class="ad-info-head">
                <b>{{ ad.title || 'إعلان بدون عنوان' }}</b>
                <span :class="['status-pill', ad.isEnabled ? 'on' : 'off']">
                  {{ ad.isEnabled ? 'مفعل' : 'معطل' }}
                </span>
              </div>

              <p>{{ ad.subtitle || 'بدون وصف' }}</p>

              <div class="ad-tags">
                <span>{{ typeLabel(ad.type) }}</span>
                <span>{{ placementLabel(ad.placement) }}</span>
                <span v-if="ad.type === 'slider'">صور: {{ ad.imageUrls?.length || 0 }}</span>
              </div>

              <small class="keep-ltr">{{ ad.linkUrl || 'بدون رابط' }}</small>
            </div>

            <div class="ad-controls">
              <button type="button" class="admin-secondary" @click="edit(ad)">تعديل</button>
              <button type="button" class="admin-secondary" @click="toggleEnabled(ad)">
                {{ ad.isEnabled ? 'تعطيل' : 'تفعيل' }}
              </button>
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

const adTypes = [
  { value: 'slider', label: 'سلايدر', icon: 'mdi:view-carousel-outline', hint: 'عدة صور أو فيديو يظهر أعلى أو آخر الصفحة' },
  { value: 'banner', label: 'بانر', icon: 'mdi:image-outline', hint: 'إعلان ثابت بصورة أو فيديو في موضع محدد' },
  { value: 'popup', label: 'إعلان منبثق', icon: 'mdi:bell-ring-outline', hint: 'نافذة تظهر فوق الموقع ويمكن أن تكون نصاً فقط' },
  { value: 'product', label: 'داخل منتج', icon: 'mdi:package-variant-closed', hint: 'إعلان يظهر داخل صفحة منتج تختاره' },
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

const filteredAds = computed(() => {
  if (filterType.value === 'all') return items.value
  if (filterType.value === 'active') return items.value.filter((x: any) => x.isEnabled)
  if (filterType.value === 'disabled') return items.value.filter((x: any) => !x.isEnabled)
  return items.value.filter((x: any) => x.type === filterType.value)
})

const filteredProducts = computed(() => {
  const q = productQuery.value.trim().toLowerCase()
  const source = products.value || []
  if (!q) return source.slice(0, 12)
  return source
    .filter((p: any) => `${productName(p)} ${p.slug || ''} ${productBrand(p)}`.toLowerCase().includes(q))
    .slice(0, 16)
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

function normalizeProducts(res: any) {
  return unwrapList(res)
}

function normalizeAds(res: any) {
  return unwrapList(res)
    .map((ad: any) => {
      const imageUrls = Array.isArray(ad?.imageUrls)
        ? ad.imageUrls
        : Array.isArray(ad?.imageUrls?.$values)
          ? ad.imageUrls.$values
          : (ad?.imageUrl ? [ad.imageUrl] : [])

      return {
        ...ad,
        id: ad?.id || ad?.Id,
        type: String(ad?.type || ad?.Type || 'banner').trim().toLowerCase(),
        placement: String(ad?.placement || ad?.Placement || 'home_top').trim(),
        title: ad?.title ?? ad?.Title ?? '',
        subtitle: ad?.subtitle ?? ad?.Subtitle ?? '',
        imageUrl: ad?.imageUrl ?? ad?.ImageUrl ?? '',
        imageUrls,
        linkUrl: ad?.linkUrl ?? ad?.LinkUrl ?? '',
        productId: ad?.productId ?? ad?.ProductId ?? '',
        sortOrder: Number(ad?.sortOrder ?? ad?.SortOrder ?? 0),
        isEnabled: (ad?.isEnabled ?? ad?.IsEnabled) !== false,
        createdAt: ad?.createdAt ?? ad?.CreatedAt,
        updatedAt: ad?.updatedAt ?? ad?.UpdatedAt,
      }
    })
    .filter((ad: any) => ad.id || ad.title || ad.imageUrl || ad.imageUrls?.length)
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

function primaryMedia(ad: any) {
  return String(ad?.imageUrl || ad?.imageUrls?.[0] || '')
}

function typeLabel(type: string) {
  return adTypes.find((x) => x.value === type)?.label || type
}
function placementLabel(p: string) {
  return allPlacements.find((x) => x.value === p)?.label || p
}
function isVideoUrl(url: string) {
  return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '')
}

async function load() {
  loading.value = true
  try {
    const [adsRes, productsRes]: any[] = await Promise.all([
      $fetch('/api/bff/admin/ads', {
        query: { _ts: Date.now() },
        headers: { 'cache-control': 'no-cache' },
      }),
      $fetch('/api/bff/admin/products', {
        query: { page: 1, pageSize: 300, _ts: Date.now() },
        headers: { 'cache-control': 'no-cache' },
      }).catch(() => []),
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
    productTitle: '',
    sortOrder: 0,
    isEnabled: true,
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
    isEnabled: ad.isEnabled !== false,
  })
  productQuery.value = form.productTitle
  window?.scrollTo?.({ top: 0, behavior: 'smooth' })
}

function selectType(type: string) {
  form.type = type
  const first = allPlacements.find((x) => x.type === type)
  form.placement = first?.value || 'home_top'

  if (type === 'popup' && !form.linkUrl) form.linkUrl = '/products'
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

function upsertAd(raw: any) {
  const normalized = normalizeAds([raw])[0]
  if (!normalized) return false

  const idx = items.value.findIndex((x: any) => x.id === normalized.id)
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

  if (
    form.type === 'popup'
    && !String(form.title || form.subtitle || form.imageUrl || '').trim()
    && !previewImages.value.length
  ) {
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

    if (!inserted && items.value.length > 0) {
      lastSavedId.value = items.value[0]?.id || null
    }

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
    items.value = items.value.filter((x: any) => x.id !== id)
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
    const body = {
      ...ad,
      imageUrls: ad.imageUrls || (ad.imageUrl ? [ad.imageUrl] : []),
      isEnabled: !ad.isEnabled,
      startAt: null,
      endAt: null,
    }

    const updated: any = await $fetch(`/api/bff/admin/ads/${ad.id}`, {
      method: 'PUT',
      body,
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
      const url = await directUpload.upload('admin/ads/upload', file, {
        maxMb: 150,
        fallbackToBff: true,
      })
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
.ads-admin {
  display: grid;
  gap: 1.25rem;
  padding-bottom: 3rem;
}

.ads-page-head,
.ads-builder-card,
.ads-list-card,
.preview-card {
  border: 1px solid rgba(var(--border), .85);
  background:
    radial-gradient(circle at top right, rgba(var(--primary), .12), transparent 34%),
    rgba(var(--surface-rgb), .9);
  border-radius: 30px;
  box-shadow: 0 24px 80px rgba(0, 0, 0, .18);
}

.ads-page-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.35rem;
}

.ads-kicker {
  display: inline-flex;
  margin: 0 0 .45rem;
  color: rgb(var(--primary));
  font-size: .72rem;
  font-weight: 1000;
}

.ads-page-head h1,
.builder-top h2,
.list-top h2 {
  margin: 0;
  color: rgb(var(--text));
  font-size: clamp(1.35rem, 2.4vw, 2.3rem);
  font-weight: 1000;
  letter-spacing: -.04em;
}

.ads-page-head p,
.builder-top p,
.list-top p,
.preview-card p {
  margin-top: .35rem;
  color: rgb(var(--muted));
  line-height: 1.8;
}

.ads-head-actions {
  display: flex;
  flex-wrap: wrap;
  gap: .6rem;
}

.ads-head-actions button,
.save-bar button,
.ad-controls button {
  border-radius: 999px;
  padding: .75rem 1rem;
  font-weight: 900;
}

.ads-stats {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: .8rem;
}

.ads-stat-card {
  border: 1px solid rgba(var(--border), .75);
  background: rgba(var(--surface-rgb), .72);
  border-radius: 24px;
  padding: 1rem;
}

.ads-stat-card strong {
  display: block;
  color: rgb(var(--text));
  font-size: 1.9rem;
  line-height: 1;
}

.ads-stat-card span {
  display: block;
  margin-top: .45rem;
  color: rgb(var(--muted));
  font-size: .85rem;
  font-weight: 800;
}

.ads-workspace {
  display: grid;
  grid-template-columns: minmax(420px, 620px) minmax(420px, 1fr);
  gap: 1rem;
  align-items: start;
}

.ads-builder-card,
.ads-list-card {
  padding: 1.2rem;
}

.builder-top,
.list-top {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.type-chip,
.status-pill,
.ad-tags span {
  display: inline-flex;
  align-items: center;
  border: 1px solid rgba(var(--border), .8);
  background: rgba(var(--surface-2-rgb), .78);
  border-radius: 999px;
  padding: .35rem .65rem;
  color: rgb(var(--text));
  font-size: .75rem;
  font-weight: 900;
  white-space: nowrap;
}

.form-section {
  display: grid;
  grid-template-columns: 46px 1fr;
  gap: .9rem;
  padding: 1rem 0;
  border-top: 1px solid rgba(var(--border), .55);
}

.section-number {
  display: grid;
  place-items: center;
  width: 40px;
  height: 40px;
  border-radius: 16px;
  background: linear-gradient(135deg, rgb(var(--primary)), rgba(var(--primary), .65));
  color: white;
  font-weight: 1000;
}

.section-body {
  display: grid;
  gap: .9rem;
}

.section-body h3 {
  margin: .35rem 0 0;
  color: rgb(var(--text));
  font-size: 1rem;
  font-weight: 1000;
}

.type-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .65rem;
}

.type-option {
  text-align: right;
  border: 1px solid rgba(var(--border), .75);
  background: rgba(var(--surface-2-rgb), .68);
  border-radius: 22px;
  padding: .9rem;
  color: rgb(var(--text));
  transition: .22s ease;
}

.type-option:hover,
.type-option.active {
  transform: translateY(-2px);
  border-color: rgba(var(--primary), .75);
  background: rgba(var(--primary), .12);
}

.type-icon {
  width: 1.35rem;
  height: 1.35rem;
  color: rgb(var(--primary));
}

.type-option b,
.type-option small {
  display: block;
}

.type-option small {
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

.admin-input {
  width: 100%;
  border: 1px solid rgba(var(--border), .85);
  background: rgba(var(--surface-2-rgb), .75);
  color: rgb(var(--text));
  border-radius: 18px;
  padding: .85rem 1rem;
  outline: none;
}

.admin-input:focus {
  border-color: rgba(var(--primary), .75);
  box-shadow: 0 0 0 4px rgba(var(--primary), .12);
}

.product-results {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: .55rem;
  max-height: 280px;
  overflow: auto;
  padding-inline-end: .25rem;
}

.product-result {
  display: grid;
  grid-template-columns: 48px 1fr;
  gap: .6rem;
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
.product-placeholder {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  object-fit: cover;
  background: rgba(var(--border), .22);
}

.product-result small {
  color: rgb(var(--muted));
}

.upload-box {
  position: relative;
  display: grid;
  place-items: center;
  gap: .35rem;
  min-height: 150px;
  border: 1px dashed rgba(var(--primary), .55);
  background: rgba(var(--primary), .08);
  border-radius: 26px;
  color: rgb(var(--text));
  text-align: center;
  cursor: pointer;
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
  gap: .55rem;
}

.preview-card {
  margin-top: 1rem;
  padding: 1rem;
}

.preview-card h3 {
  color: rgb(var(--text));
  font-weight: 1000;
  font-size: 1.4rem;
}

.preview-media {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
  gap: .65rem;
  margin-top: 1rem;
}

.preview-item {
  position: relative;
  overflow: hidden;
  border-radius: 20px;
  background: rgba(var(--surface-2-rgb), .85);
  min-height: 110px;
}

.preview-item img,
.preview-item video {
  width: 100%;
  height: 150px;
  object-fit: cover;
}

.preview-item button {
  position: absolute;
  top: .4rem;
  inset-inline-end: .4rem;
  width: 30px;
  height: 30px;
  border-radius: 999px;
  background: rgba(0, 0, 0, .62);
  color: white;
  font-weight: 1000;
}

.preview-empty {
  grid-column: 1 / -1;
  display: grid;
  place-items: center;
  gap: .45rem;
  min-height: 140px;
  border: 1px dashed rgba(var(--border), .75);
  border-radius: 22px;
  color: rgb(var(--muted));
}

.preview-empty svg {
  width: 2rem;
  height: 2rem;
}

.save-bar {
  position: sticky;
  bottom: .75rem;
  display: flex;
  justify-content: flex-end;
  gap: .65rem;
  margin-top: 1rem;
  padding: .75rem;
  border: 1px solid rgba(var(--border), .65);
  background: rgba(var(--surface-rgb), .88);
  backdrop-filter: blur(18px);
  border-radius: 24px;
}

.ads-list-card {
  position: sticky;
  top: 1rem;
  max-height: calc(100vh - 2rem);
  overflow: auto;
}

.list-top {
  position: sticky;
  top: 0;
  z-index: 2;
  padding-bottom: .8rem;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-rgb), .78));
  backdrop-filter: blur(14px);
}

.ads-empty {
  display: grid;
  place-items: center;
  min-height: 180px;
  border: 1px dashed rgba(var(--border), .75);
  border-radius: 26px;
  color: rgb(var(--muted));
  text-align: center;
  padding: 1rem;
}

.ads-list {
  display: grid;
  gap: .75rem;
}

.ad-item {
  display: grid;
  grid-template-columns: 86px 1fr auto;
  gap: .8rem;
  align-items: center;
  border: 1px solid rgba(var(--border), .78);
  background: rgba(var(--surface-2-rgb), .62);
  border-radius: 24px;
  padding: .75rem;
  transition: .25s ease;
}

.ad-item:hover,
.ad-item.fresh {
  border-color: rgba(var(--primary), .72);
  background: rgba(var(--primary), .1);
  transform: translateY(-2px);
}

.ad-media {
  display: grid;
  place-items: center;
  width: 86px;
  height: 74px;
  overflow: hidden;
  border: 1px solid rgba(var(--border), .7);
  background: rgba(var(--surface-rgb), .85);
  border-radius: 18px;
  color: rgb(var(--primary));
}

.ad-media img,
.ad-media video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.ad-media svg {
  width: 1.9rem;
  height: 1.9rem;
}

.ad-info {
  min-width: 0;
}

.ad-info-head {
  display: flex;
  flex-wrap: wrap;
  gap: .45rem;
  align-items: center;
}

.ad-info-head b {
  color: rgb(var(--text));
  font-weight: 1000;
}

.ad-info p {
  margin: .3rem 0;
  color: rgb(var(--muted));
  font-size: .85rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ad-tags {
  display: flex;
  flex-wrap: wrap;
  gap: .35rem;
  margin-bottom: .25rem;
}

.status-pill.on {
  border-color: rgba(34, 197, 94, .45);
  color: rgb(74, 222, 128);
}

.status-pill.off {
  border-color: rgba(239, 68, 68, .4);
  color: rgb(248, 113, 113);
}

.ad-controls {
  display: flex;
  flex-direction: column;
  gap: .35rem;
}

.keep-ltr {
  direction: ltr;
  unicode-bidi: plaintext;
}

@media (max-width: 1180px) {
  .ads-workspace {
    grid-template-columns: 1fr;
  }

  .ads-list-card {
    position: relative;
    top: auto;
    max-height: none;
  }
}

@media (max-width: 760px) {
  .ads-page-head,
  .builder-top,
  .list-top {
    flex-direction: column;
  }

  .ads-stats {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .form-grid,
  .type-grid,
  .product-results {
    grid-template-columns: 1fr;
  }

  .form-section {
    grid-template-columns: 1fr;
  }

  .ad-item {
    grid-template-columns: 1fr;
  }

  .ad-media {
    width: 100%;
    height: 180px;
  }

  .ad-controls {
    flex-direction: row;
    flex-wrap: wrap;
  }
}
</style>
