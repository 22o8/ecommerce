<template>
  <div class="w-full">
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

    <div class="mt-6 grid gap-6 lg:grid-cols-3">
      <!-- Details -->
      <UiCard class="lg:col-span-2">
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
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.description') }}</label>
              <UiTextarea v-model="form.description" rows="5" :placeholder="t('admin.descriptionPlaceholder')" />
            </div>

            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" v-model="form.isActive" class="h-4 w-4" />
              {{ t('admin.isActive') }}
            </label>

            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" v-model="form.isFeatured" class="h-4 w-4" />
              {{ t('admin.isFeatured') }}
            </label>

            <div class="flex flex-wrap gap-2">
              <UiButton type="submit" :disabled="saving">{{ saving ? t('common.saving') : t('common.save') }}</UiButton>
              <UiButton variant="ghost" type="button" :disabled="saving" @click="resetForm">
                {{ t('common.reset') }}
              </UiButton>
            </div>
          </form>

          <div v-else class="text-white/70">{{ t('admin.productNotFound') }}</div>
        </UiCardContent>
      </UiCard>

      <!-- Images -->
      <UiCard>
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

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
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
  brandSlug: '',
  isActive: true,
  isFeatured: false,
})

const slugTouched = ref(false)
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

function resetForm() {
  if (!product.value) return
  // backend (ASP.NET) returns camelCase: title, priceUsd, brand, isPublished, isFeatured
  form.name = product.value.title || product.value.name || ''
  form.slug = product.value.slug || ''
  form.description = product.value.description || ''
  form.price = Number(product.value.priceUsd ?? product.value.price ?? 0)
  // we store brand slug/name in the same field; API expects "brand"
  form.brandSlug = product.value.brand || product.value.brandSlug || ''
  // If API returns brand NAME, map it to slug so the dropdown selects correctly.
  const match = brands.value.find((b: any) => b.slug === form.brandSlug || b.name === form.brandSlug)
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
    brands.value = await listBrands<any>()
  } catch (e: any) {
    toast.error(t('common.errorGeneric'))
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

  // ننتظر أولاً المنتج حتى يصير عندنا id صحيح للصور
  await Promise.all(tasks)

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
    const match = brands.value.find((b: any) => b.slug === form.brandSlug || b.name === form.brandSlug)
    await updateAdminProduct<any>(id.value, {
      // ✅ match backend DTO (UpsertProductRequest)
      title: form.name.trim(),
      slug: form.slug.trim(),
      description: form.description?.trim() || '',
      priceUsd: Number(form.price),
      // Backend validates brand by NAME; UI selects by slug.
      brand: match?.name || form.brandSlug,
      isPublished: Boolean(form.isActive),
      isFeatured: Boolean(form.isFeatured),
    })
    toast.success(t('common.saved'))
    await loadProduct()
  } catch (e: any) {
    toast.error(t('common.errorGeneric'))
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
    toast.error(t('admin.uploadImagesFailed'))
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
    toast.error(t('common.errorGeneric'))
  }
}

onMounted(async () => {
  await reloadAll()
})
</script>
