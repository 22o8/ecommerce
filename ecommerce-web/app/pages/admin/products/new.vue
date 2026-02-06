<template>
  <div class="w-full">
    <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold">{{ t('admin.addProduct') }}</h1>
        <p class="text-sm text-white/70">{{ t('admin.addProductHint') }}</p>
      </div>

      <div class="flex items-center gap-2">
        <UiButton variant="ghost" @click="navigateTo('/admin/products')">{{ t('common.back') }}</UiButton>
      </div>
    </div>

    <div class="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-3">
      <!-- Form -->
      <UiCard class="lg:col-span-2">
        <UiCardHeader>
          <UiCardTitle>{{ t('admin.productDetails') }}</UiCardTitle>
          <UiCardDescription>{{ t('admin.productDetailsHint') }}</UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <form class="grid grid-cols-1 gap-4 md:grid-cols-2" @submit.prevent="onCreate">
            <div class="md:col-span-2">
              <label class="mb-1 block text-sm text-white/80">{{ t('admin.brand') }}</label>
              <select
                v-model="form.brandSlug"
                class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none focus:border-white/20"
                required
              >
                <option value="" disabled>{{ t('admin.selectBrand') }}</option>
                <option v-for="b in brands" :key="b.id" :value="b.slug">
                  {{ b.name }} (/{{ b.slug }})
                </option>
              </select>
              <p v-if="brands.length === 0" class="mt-2 text-xs text-amber-200/90">
                {{ t('admin.noBrandsYet') }}
              </p>
            </div>

            <div>
              <label class="mb-1 block text-sm text-white/80">{{ t('admin.name') }}</label>
              <UiInput v-model="form.title" :placeholder="t('admin.namePlaceholder')" required />
            </div>

            <div>
              <label class="mb-1 block text-sm text-white/80">{{ t('admin.slug') }}</label>
              <UiInput v-model="form.slug" :placeholder="t('admin.slugPlaceholder')" required />
              <p class="mt-1 text-xs text-white/60">{{ t('admin.slugHint') }}</p>
            </div>

            <div class="md:col-span-2">
              <label class="mb-1 block text-sm text-white/80">{{ t('admin.description') }}</label>
              <textarea
                v-model="form.description"
                rows="4"
                class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none focus:border-white/20"
                :placeholder="t('admin.descriptionPlaceholder')"
              />
            </div>

            <div>
              <label class="mb-1 block text-sm text-white/80">{{ t('admin.price') }}</label>
              <UiInput v-model.number="form.price" type="number" min="0" step="0.01" />
            </div>

            <div class="flex items-end gap-3">
              <label class="flex cursor-pointer items-center gap-2 text-sm text-white/80">
                <input v-model="form.isActive" type="checkbox" class="h-4 w-4" />
                {{ t('common.active') }}
              </label>
            </div>

            <div class="md:col-span-2 flex flex-wrap items-center gap-2 pt-2">
              <UiButton :disabled="loading" type="submit">
                {{ loading ? t('common.saving') : t('common.create') }}
              </UiButton>
              <UiButton
                variant="secondary"
                :disabled="loading"
                type="button"
                @click="navigateTo('/admin/products')"
              >
                {{ t('common.cancel') }}
              </UiButton>
            </div>
          </form>
        </UiCardContent>
      </UiCard>

      <!-- Images -->
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>{{ t('admin.images') }}</UiCardTitle>
          <UiCardDescription>{{ t('admin.imagesCreateHint') }}</UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <div class="rounded-2xl border border-white/10 bg-white/5 p-4">
            <input
              ref="fileInput"
              type="file"
              accept="image/*"
              multiple
              class="hidden"
              @change="onPickFiles"
            />

            <div class="flex items-center justify-between gap-2">
              <div class="text-sm text-white/80">{{ t('admin.imagesSelected', { count: files.length }) }}</div>
              <UiButton size="sm" variant="ghost" type="button" @click="fileInput?.click()">
                {{ t('common.chooseFiles') }}
              </UiButton>
            </div>

            <div v-if="files.length" class="mt-4 grid grid-cols-3 gap-2">
              <div
                v-for="(f, idx) in files"
                :key="idx"
                class="group relative overflow-hidden rounded-xl border border-white/10 bg-black/20"
              >
                <img :src="f.preview" class="h-20 w-full object-cover" />
                <button
                  type="button"
                  class="absolute right-2 top-2 rounded-lg bg-black/60 px-2 py-1 text-xs opacity-0 transition group-hover:opacity-100"
                  @click="removeFile(idx)"
                >
                  {{ t('common.remove') }}
                </button>
              </div>
            </div>

            <p v-else class="mt-3 text-sm text-white/60">{{ t('admin.imagesEmpty') }}</p>
          </div>
        </UiCardContent>
      </UiCard>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

const { t } = useI18n()
const toast = useToast()
const { listBrands, createProduct, uploadProductImages } = useAdminApi()

type BrandItem = { id: string; slug: string; name: string }

const brands = ref<BrandItem[]>([])
const loading = ref(false)

// ✅ مفاتيح الفورم لازم تكون مطابقة لطلب الـ API (.NET)
// UpsertProductRequest: title, slug, description, priceUsd, isPublished, brand
const form = reactive({
  title: '',
  slug: '',
  description: '',
  priceUsd: 0,
  brandSlug: '',
  isPublished: true,
})

type PickedFile = { file: File; preview: string }
const files = ref<PickedFile[]>([])
const fileInput = ref<HTMLInputElement | null>(null)

onMounted(async () => {
  try {
    const res: any = await listBrands()
    brands.value = (res?.items || res || []) as BrandItem[]
  } catch (e: any) {
    toast.error(e?.message || t('common.error'))
  }
})

function onPickFiles(e: Event) {
  const input = e.target as HTMLInputElement
  const list = input.files ? Array.from(input.files) : []
  for (const f of list) {
    files.value.push({ file: f, preview: URL.createObjectURL(f) })
  }
  // reset to allow re-pick same files
  input.value = ''
}

function removeFile(idx: number) {
  const item = files.value[idx]
  if (item?.preview) URL.revokeObjectURL(item.preview)
  files.value.splice(idx, 1)
}

onBeforeUnmount(() => {
  for (const f of files.value) {
    if (f.preview) URL.revokeObjectURL(f.preview)
  }
})

async function onCreate() {
  if (loading.value) return
  loading.value = true
  try {
    const created: any = await createProduct({
      title: form.title,
      slug: form.slug,
      description: form.description,
      // تأكد أنه رقم (مو نص) حتى ما يرجع 400 من .NET
      priceUsd: Number(form.priceUsd ?? 0),
      brand: form.brandSlug,
      isPublished: !!form.isPublished,
    })

    const productId = created?.id || created?.productId

    if (productId && files.value.length) {
      await uploadProductImages(productId, files.value.map(x => x.file))
    }

    toast.success(t('common.saved'))
    if (productId) {
      await navigateTo(`/admin/products/${productId}`)
    } else {
      await navigateTo('/admin/products')
    }
  } catch (e: any) {
    toast.error(e?.message || t('common.error'))
  } finally {
    loading.value = false
  }
}
</script>
