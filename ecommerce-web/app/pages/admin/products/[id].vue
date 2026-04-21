<template>
  <div class="w-full admin-product-form-page">
    <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold">{{ t('admin.editProduct') }}</h1>
        <p class="text-sm text-white/70" v-if="product">#{{ product.id }}</p>
      </div>

      <div class="flex flex-wrap gap-2">
        <UiButton variant="ghost" @click="router.push('/admin/products')">{{ t('common.back') }}</UiButton>
        <UiButton variant="secondary" :disabled="loading" @click="reloadAll">
          {{ loading ? t('common.loading') : t('common.refresh') }}
        </UiButton>
        <UiButton variant="destructive" :disabled="loading" @click="confirmDelete">
          {{ t('common.delete') }}
        </UiButton>
      </div>
    </div>

    <div class="mt-6 grid gap-5 lg:grid-cols-3">
      <!-- Details -->
      <UiCard class="form-shell lg:col-span-2">
        <UiCardHeader>
          <UiCardTitle>{{ t('admin.productDetails') }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div v-if="loading" class="text-white/70">{{ t('common.loading') }}</div>

          <form v-else-if="product" class="grid gap-4" @submit.prevent="onSave">
            <div class="grid gap-4 md:grid-cols-2">
              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.name') }}</label>
                <UiInput v-model="form.name" :placeholder="t('admin.namePlaceholder')" />
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.slug') }}</label>
                <UiInput
                  v-model="form.slug"
                  :placeholder="t('admin.slugPlaceholder')"
                  dir="ltr"
                  @update:modelValue="() => (slugTouched = true)"
                />
                <div class="text-xs text-white/60">{{ t('admin.slugHint') }}</div>
              </div>
            </div>

            <div class="grid gap-4 md:grid-cols-2">
              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.brand') }}</label>
                <select
                  v-model="form.brandSlug"
                  class="h-10 w-full rounded-2xl border border-white/10 bg-white/5 px-3 text-sm outline-none focus:border-white/20"
                >
                  <option :value="''" disabled>{{ t('admin.selectBrand') }}</option>
                  <option v-for="b in brands" :key="b.slug" :value="b.slug">
                    {{ b.name }} ({{ b.slug }})
                  </option>
                </select>
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.price') }}</label>
                <UiInput v-model.number="form.price" type="number" min="0" step="0.01" />
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.discount') || 'Discount %' }}</label>
                <UiInput v-model.number="form.discountPercent" type="number" min="0" max="100" step="1" />
                <div class="text-xs text-white/60 keep-ltr">
                  {{ (t('admin.finalPrice') || 'Final') }}: {{ formatIqd(finalPrice) }}
                </div>
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.category') }}</label>
                <select v-model="form.category" class="h-10 w-full rounded-2xl border border-white/10 bg-white/5 px-3 text-sm outline-none focus:border-white/20">
                  <option v-for="c in categoryOptions" :key="c.key" :value="c.key">{{ c.nameAr }}</option>
                </select>
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.preciseCategory') || 'التصنيف الدقيق' }}</label>
                <select v-model="form.subCategory" class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none focus:border-white/20">
                <option value="">{{ t('admin.preciseCategoryPlaceholder') || 'اختر تصنيفًا دقيقًا' }}</option>
                <option v-for="c in categorySubCategoryOptions" :key="c.key" :value="c.key">{{ c.nameAr }}</option>
              </select>
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.problemCategory') || 'تصنيف حل المشكلة' }}</label>
                <select v-model="form.problemCategory" class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none focus:border-white/20">
                  <option value="">{{ t('admin.problemCategoryPlaceholder') || 'اختر تصنيف حل المشكلة' }}</option>
                  <option v-for="c in problemCategoryOptions" :key="c.key" :value="c.key">{{ c.nameAr }}</option>
                </select>
              </div>

              <div v-if="problemSubCategoryOptions.length" class="grid gap-2">
                <label class="text-sm font-medium">القسم الدقيق لحل المشكلة</label>
                <select v-model="form.problemSubCategory" class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none focus:border-white/20">
                  <option value="">اختر القسم الدقيق</option>
                  <option v-for="c in problemSubCategoryOptions" :key="c.key" :value="c.key">{{ c.nameAr }}</option>
                </select>
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.stockQuantity') }}</label>
                <UiInput v-model.number="form.stockQuantity" type="number" min="0" step="1" />
              </div>

              <div class="grid gap-2">
                <label class="text-sm font-medium">{{ t('admin.lowStockThreshold') }}</label>
                <UiInput v-model.number="form.lowStockThreshold" type="number" min="0" step="1" />
              </div>
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.description') }}</label>
              <UiTextarea v-model="form.description" rows="5" :placeholder="t('admin.descriptionPlaceholder')" />
            </div>

            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" v-model="form.isCouponAllowed" class="h-4 w-4" />
              {{ t('admin.couponAllowed') || 'يسمح بالكوبون' }}
            </label>

            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" v-model="form.isActive" class="h-4 w-4" />
              {{ t('admin.isActive') }}
            </label>

            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" v-model="form.isFeatured" class="h-4 w-4" />
              {{ t('admin.isFeatured') }}
            </label>

            <div class="flex flex-wrap gap-2">
              <!-- ✅ ضمان عمل زر الحفظ على الموبايل (أحيانًا submit داخل مكوّنات مخصّصة ما يتفعّل) -->
              <UiButton type="button" :disabled="saving" @click="onSave">{{ saving ? t('common.saving') : t('common.save') }}</UiButton>
              <UiButton variant="ghost" type="button" :disabled="saving" @click="resetForm">
                {{ t('common.reset') }}
              </UiButton>
            </div>
          </form>

          <div v-else class="text-white/70">{{ t('admin.productNotFound') }}</div>
        </UiCardContent>
      </UiCard>

      <!-- Images -->
      <UiCard class="side-shell">
        <UiCardHeader>
          <UiCardTitle>{{ t('admin.images') }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-3">
            <div class="text-sm text-white/70">{{ t('admin.imagesHint') }}</div>

            <div class="flex flex-wrap gap-2">
              <input
                ref="fileInput"
                type="file"
                accept="image/*"
                multiple
                class="hidden"
                @change="onFilesPicked"
              />
              <UiButton variant="secondary" type="button" :disabled="!product" @click="fileInput?.click()">
                {{ t('common.chooseFiles') }}
              </UiButton>
              <UiButton
                type="button"
                :disabled="!product || uploading || selectedFiles.length === 0"
                @click="uploadSelected"
              >
                {{ uploading ? t('common.uploading') : t('common.upload') }}
              </UiButton>
            </div>

            <div v-if="selectedFiles.length" class="grid gap-2">
              <div class="text-xs text-white/60">{{ t('admin.selectedImages') }} ({{ selectedFiles.length }})</div>
              <div class="flex flex-wrap gap-2">
                <div
                  v-for="(f, idx) in selectedFiles"
                  :key="f.name + idx"
                  class="rounded-xl border border-white/10 bg-white/5 px-2 py-1 text-xs"
                >
                  {{ f.name }}
                  <button class="ms-2 text-white/60 hover:text-white" type="button" @click="removeSelected(idx)">×</button>
                </div>
              </div>
            </div>

            <div class="border-t border-white/10 pt-3" />

            <div v-if="imagesLoading" class="text-sm text-white/70">{{ t('common.loading') }}</div>
            <div v-else class="grid gap-2">
              <div class="text-xs text-white/60">{{ t('admin.currentImages') }} ({{ images.length }})</div>

              <div v-if="images.length === 0" class="text-sm text-white/60">
                {{ t('admin.noImages') }}
              </div>

              <div v-else class="grid grid-cols-3 gap-2">
                <div v-for="img in images" :key="img.id" class="relative overflow-hidden rounded-2xl border border-white/10 bg-white/5">
                  <img :src="resolveUploadUrl(img.url || img.imageUrl || img.path)" class="h-24 w-full object-cover" />
                  <button
                    type="button"
                    class="absolute right-1 top-1 rounded-lg bg-black/60 px-2 py-1 text-xs text-white hover:bg-black/80"
                    @click="deleteImage(img.id)"
                  >
                    {{ t('common.delete') }}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </UiCardContent>
      </UiCard>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

import { formatIqd } from '~/composables/useMoney'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const { categories, problemCategories, fetchCategories } = useCategories()
const toast = useToast()
const api = useApi()
const imgVer = ref(0)

const { listBrands, getAdminProduct, updateAdminProduct, deleteAdminProduct, getProductImages, uploadProductImages, deleteProductImage } =
  useAdminApi()

const id = computed(() => String(route.params.id || ''))

const loading = ref(true)
const saving = ref(false)
const uploading = ref(false)

const product = ref<any>(null)
const brands = ref<Array<{ id: string; slug: string; name: string }>>([])

const form = reactive({
  name: '',
  slug: '',
  description: '',
  price: 0,
  discountPercent: 0,
  brandSlug: '',
  isActive: true,
  isFeatured: false,
  category: 'general',
  subCategory: '',
  problemCategory: '',
  problemSubCategory: '',
  stockQuantity: 0,
  lowStockThreshold: 5,
  isCouponAllowed: true,
})

const finalPrice = computed(() => {
  const price = Number(form.price || 0)
  const d = Math.max(0, Math.min(100, Number(form.discountPercent || 0)))
  return d > 0 ? (price * (100 - d) / 100) : price
})

const slugTouched = ref(false)
const categoryOptions = computed(() => (categories.value && categories.value.length ? categories.value : [{ key: 'general', nameAr: 'عام', id: '', hasDetailSections: false, childCount: 0 }]).map((c:any) => ({ key: String(c.key || ''), nameAr: String(c.nameAr || c.key || ''), id: String(c.id || ''), hasDetailSections: Boolean(c.hasDetailSections ?? false), childCount: Number(c.childCount ?? 0) })))
const selectedCategoryMeta = computed(() => categoryOptions.value.find((x:any) => x.key === String(form.category || '')) || null)
const selectedCategoryHasDetailSections = computed(() => Boolean(selectedCategoryMeta.value?.hasDetailSections || Number(selectedCategoryMeta.value?.childCount || 0) > 0))
const categorySubCategoryItems = ref<any[]>([])
const categorySubCategoryOptions = computed(() => (categorySubCategoryItems.value || []).map((c:any) => ({ key: String(c.key || ''), nameAr: String(c.nameAr || c.key || '') })))

async function loadCategorySubCategories() {
  form.subCategory = categorySubCategoryOptions.value.some((x:any) => x.key === form.subCategory) ? form.subCategory : ''
  const selected = (categories.value || []).find((x:any) => String(x.key || '') === String(form.category || ''))
  if (!selected?.id || (!selected?.hasDetailSections && Number(selected?.childCount || 0) <= 0)) {
    categorySubCategoryItems.value = []
    form.subCategory = ''
    return
  }
  try {
    const res: any = await $fetch('/api/bff/categories/active', { query: { section: 'regular', parentId: selected.id, _ts: Date.now() } })
    categorySubCategoryItems.value = Array.isArray(res) ? res : []
    if (!categorySubCategoryItems.value.some((x:any) => String(x.key || '') === String(form.subCategory || ''))) form.subCategory = ''
  } catch {
    categorySubCategoryItems.value = []
    form.subCategory = ''
  }
}

watch(() => form.category, () => { loadCategorySubCategories() })
const problemCategoryOptions = computed(() => (problemCategories.value || []).map((c:any) => ({ key: String(c.key || ''), nameAr: String(c.nameAr || c.key || ''), id: String(c.id || ''), hasDetailSections: Boolean(c.hasDetailSections ?? false), childCount: Number(c.childCount ?? 0) })))
const selectedProblemCategoryMeta = computed(() => problemCategoryOptions.value.find((x:any) => x.key === String(form.problemCategory || '')) || null)
const selectedProblemCategoryHasDetailSections = computed(() => Boolean(selectedProblemCategoryMeta.value?.hasDetailSections || Number(selectedProblemCategoryMeta.value?.childCount || 0) > 0))
const problemSubCategoryItems = ref<any[]>([])
const problemSubCategoryOptions = computed(() => (problemSubCategoryItems.value || []).map((c:any) => ({ key: String(c.key || ''), nameAr: String(c.nameAr || c.key || '') })))


async function loadProblemSubCategories() {
  form.problemSubCategory = problemSubCategoryOptions.value.some((x:any) => x.key === form.problemSubCategory) ? form.problemSubCategory : ''
  const selected = (problemCategories.value || []).find((x:any) => String(x.key || '') === String(form.problemCategory || ''))
  if (!selected?.id || (!selected?.hasDetailSections && Number(selected?.childCount || 0) <= 0)) {
    problemSubCategoryItems.value = []
    form.problemSubCategory = ''
    return
  }
  try {
    const res: any = await $fetch('/api/bff/categories/active', { query: { section: 'problem', parentId: selected.id, _ts: Date.now() } })
    problemSubCategoryItems.value = Array.isArray(res) ? res : []
    if (!problemSubCategoryItems.value.some((x:any) => String(x.key || '') === String(form.problemSubCategory || ''))) form.problemSubCategory = ''
  } catch {
    problemSubCategoryItems.value = []
    form.problemSubCategory = ''
  }
}

watch(() => form.problemCategory, () => { loadProblemSubCategories() })

const autoSlugBase = ref('')

const slugify = (input: string) => {
  return (input || '')
    .toString()
    .trim()
    .toLowerCase()
    .replace(/['"`]/g, '')
    .replace(/[^a-z0-9\u0600-\u06FF]+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '')
}

const selectedFiles = ref<File[]>([])
const fileInput = ref<HTMLInputElement | null>(null)

const imagesLoading = ref(false)
const images = ref<any[]>([])

async function resetForm() {
  if (!product.value) return
  // backend (ASP.NET) returns camelCase: title, priceIqd, brand, isPublished, isFeatured
  form.name = product.value.title || product.value.name || ''
  form.slug = product.value.slug || ''
  form.description = product.value.description || ''
  form.price = Number(product.value.priceIqd ?? product.value.price ?? 0)
  form.discountPercent = Number(product.value.discountPercent ?? 0)
  // we store brand slug/name in the same field; API expects "brand"
  form.brandSlug = product.value.brand || product.value.brandSlug || ''
  form.category = product.value.category || 'general'
  form.subCategory = product.value.subCategory || ''
  form.problemCategory = product.value.problemCategory || ''
  form.problemSubCategory = product.value.problemSubCategory || ''
  await loadCategorySubCategories()
  form.stockQuantity = Number(product.value.stockQuantity ?? 0)
  form.lowStockThreshold = Number(product.value.lowStockThreshold ?? 5)
  form.isCouponAllowed = Boolean(product.value.isCouponAllowed ?? true)
  // If API returns brand NAME, map it to slug so the dropdown selects correctly.
  const brandList = Array.isArray(brands.value) ? brands.value : []
  const match = brandList.find((b: any) => b.slug === form.brandSlug || b.name === form.brandSlug)
  if (match) form.brandSlug = match.slug
  form.isActive = Boolean(product.value.isPublished ?? product.value.isActive ?? true)
  form.isFeatured = Boolean((product.value as any).isFeatured ?? false)

  // للـ slug التلقائي: لا نعتبره "معدل" إلا إذا المستخدم لمس حقل slug
  slugTouched.value = false
  autoSlugBase.value = slugify(form.name)
}

watch(
  () => form.name,
  (val) => {
    if (slugTouched.value) return
    const next = slugify(val)
    if (!form.slug || form.slug === autoSlugBase.value) {
      form.slug = next
      autoSlugBase.value = next
    }
  }
)

watch(
  () => form.slug,
  (val) => {
    if (!val) slugTouched.value = false
  }
)

function resolveUploadUrl(path?: string) {
  if (!path) return ''
  // Normalize both relative (/uploads/...) and absolute URLs to a safe URL that works on https + SSR.
  const normalized = api.buildAssetUrl(path)
  // Cache buster after upload/delete (prevents browser showing stale/blank thumbnails)
  const sep = normalized.includes('?') ? '&' : '?'
  return `${normalized}${sep}v=${imgVer.value}`
}

async function loadBrands() {
  try {
    const res: any = await listBrands<any>()
    brands.value = Array.isArray(res) ? res : (Array.isArray(res?.items) ? res.items : (Array.isArray(res?.data) ? res.data : []))
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || t('common.errorGeneric'))
  }
}

async function loadProduct() {
  loading.value = true
  try {
    product.value = await getAdminProduct<any>(id.value)
    resetForm()
  } catch (e: any) {
    product.value = null
    toast.error(t('admin.loadProductFailed'))
  } finally {
    loading.value = false
  }
}

async function loadImages() {
  if (!product.value) return
  imagesLoading.value = true
  try {
    const res: any = await getProductImages<any>(id.value)
    // Support {items:[]} or []
    images.value = Array.isArray(res) ? res : res?.items || []
  } catch (e: any) {
    images.value = []
  } finally {
    imagesLoading.value = false
  }
}

async function reloadAll() {
  loading.value = true
  // ملاحظة: سابقاً كان يوجد متغير error، لكن الآن نعتمد على toast/errorMsg فقط.
  // لذلك لا نستخدم error.value هنا حتى لا يحصل خطأ (error is not defined).

  // لا نخلي فشل brands يمنع تحميل المنتج أو الصور (خصوصاً على الموبايل/شبكات ضعيفة)
  const tasks: Array<Promise<any>> = []

  tasks.push(
    loadBrands().catch((e) => {
      console.warn('[admin] loadBrands failed', e)
    })
  )

  tasks.push(
    loadProduct().catch((e) => {
      console.warn('[admin] loadProduct failed', e)
    })
  )

  tasks.push(fetchCategories(false, 'regular').catch(() => {}))
  tasks.push(fetchCategories(false, 'problem').catch(() => {}))

  // ننتظر أولاً المنتج حتى يصير عندنا id صحيح للصور
  await Promise.all(tasks)
  await loadProblemSubCategories()

  await loadImages().catch((e) => {
    console.warn('[admin] loadImages failed', e)
  })

  loading.value = false
}


function validate() {
  if (!form.name.trim()) return t('admin.validationName')
  if (!form.slug.trim()) return t('admin.validationSlug')
  if (!form.brandSlug.trim()) return t('admin.validationBrand')
  if (Number.isNaN(Number(form.price)) || Number(form.price) < 0) return t('admin.validationPrice')
  return ''
}

async function onSave() {
  const err = validate()
  if (err) return toast.error(err)

  saving.value = true
  try {
    const brandList = Array.isArray(brands.value) ? brands.value : []
    const match = brandList.find((b: any) => b.slug === form.brandSlug || b.name === form.brandSlug)
    await updateAdminProduct<any>(id.value, {
      // ✅ match backend DTO (UpsertProductRequest)
      title: form.name.trim(),
      slug: form.slug.trim(),
      description: form.description?.trim() || '',
      priceIqd: Number(form.price),
      discountPercent: Math.max(0, Math.min(100, Number(form.discountPercent || 0))),
      // Backend validates brand by SLUG; if list not loaded keep current slug.
      brand: match?.slug || form.brandSlug,
      category: form.category,
      subCategory: form.subCategory,
      problemCategory: form.problemCategory,
      problemSubCategory: form.problemSubCategory,
      stockQuantity: Number(form.stockQuantity ?? 0),
      lowStockThreshold: Number(form.lowStockThreshold ?? 0),
      isCouponAllowed: Boolean(form.isCouponAllowed),
      isPublished: Boolean(form.isActive),
      isFeatured: Boolean(form.isFeatured),
    })
    toast.success(t('common.saved'))
    await Promise.all([loadProduct(), fetchCategories(false, 'regular'), fetchCategories(false, 'problem')])
    await loadCategorySubCategories()
    await loadProblemSubCategories()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || t('common.errorGeneric'))
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!product.value) return
  if (!confirm(t('admin.deleteProductConfirm'))) return

  loading.value = true
  try {
    await deleteAdminProduct<any>(id.value)
    toast.success(t('common.deleted'))
    router.push('/admin/products')
  } catch (e: any) {
    toast.error(t('admin.deleteProductFailed'))
  } finally {
    loading.value = false
  }
}

function onFilesPicked(e: Event) {
  const files = Array.from((e.target as HTMLInputElement).files || [])
  if (files.length) selectedFiles.value = files
}

function removeSelected(idx: number) {
  selectedFiles.value.splice(idx, 1)
}

async function uploadSelected() {
  if (!product.value || selectedFiles.value.length === 0) return
  uploading.value = true
  try {
    await uploadProductImages<any>(id.value, selectedFiles.value)
    toast.success(t('common.uploaded'))
    selectedFiles.value = []
    if (fileInput.value) fileInput.value.value = ''
    await loadImages()
    imgVer.value++
  } catch (e: any) {
    toast.error(e?.data?.message || t('admin.uploadImagesFailed'))
  } finally {
    uploading.value = false
  }
}

async function deleteImage(imageId: string) {
  if (!product.value) return
  if (!confirm(t('admin.deleteImageConfirm'))) return
  try {
    await deleteProductImage<any>(id.value, imageId)
    toast.success(t('common.deleted'))
    await loadImages()
    imgVer.value++
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || t('common.errorGeneric'))
  }
}

onMounted(async () => {
  await reloadAll()
})
</script>

<style scoped>
.admin-product-form-page :deep(.form-shell),
.admin-product-form-page :deep(.side-shell){
  border-radius: 28px;
  border: 1px solid rgba(var(--border), .95);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .97), rgba(var(--surface-rgb), .90));
  box-shadow: 0 22px 60px rgba(12, 16, 32, .14);
}
.admin-product-form-page :deep(.form-shell > div:first-child),
.admin-product-form-page :deep(.side-shell > div:first-child){
  border-bottom-color: rgba(var(--border), .85);
}
.admin-product-form-page :deep(input),
.admin-product-form-page :deep(select),
.admin-product-form-page :deep(textarea){
  min-height: 46px;
  border-radius: 16px;
}
@media (max-width: 768px){
  .admin-product-form-page :deep(.form-shell),
  .admin-product-form-page :deep(.side-shell){ border-radius: 22px; }
}
</style>
