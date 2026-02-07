<template>
  <div class="admin-box max-w-3xl">
    <div class="text-2xl font-extrabold rtl-text">{{ t('admin.newBrand') }}</div>
    <div class="admin-muted rtl-text mt-1">{{ t('admin.newBrandHint') }}</div>

    <form class="mt-6 grid gap-4" @submit.prevent="submit">
      <div class="grid gap-2">
        <label class="admin-label rtl-text">{{ t('admin.name') }}</label>
        <input v-model="name" class="admin-input" required />
      </div>

      <div class="grid gap-2">
        <label class="admin-label rtl-text">{{ t('admin.slug') }}</label>
        <input v-model="slug" class="admin-input" :placeholder="t('admin.slugHint')" @input="slugTouched = true" />
      </div>

      <div class="grid gap-2">
        <label class="admin-label rtl-text">{{ t('admin.description') }}</label>
        <textarea v-model="description" class="admin-input min-h-[120px]" />
      </div>

      <label class="flex items-center gap-2">
        <input type="checkbox" v-model="isActive" />
        <span class="rtl-text">{{ t('admin.active') }}</span>
      </label>

      <div class="flex gap-2">
        <button class="admin-primary" type="submit" :disabled="pending">
          {{ pending ? t('common.saving') : t('common.create') }}
        </button>
        <NuxtLink class="admin-ghost" to="/admin/brands">{{ t('common.cancel') }}</NuxtLink>
      </div>

      <div v-if="error" class="admin-error rtl-text">{{ error }}</div>
    </form>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['auth'] })

const { t } = useI18n()
const router = useRouter()
const brands = useBrandsStore()

const name = ref('')
const slug = ref('')
const description = ref('')
const isActive = ref(true)

// ✅ توليد slug تلقائياً من الاسم (ويظل قابل للتعديل اليدوي)
const slugTouched = ref(false)
const slugify = (value: string) =>
  value
    .toLowerCase()
    .trim()
    .replace(/['"`]/g, '')
    .replace(/[^a-z0-9\u0600-\u06FF]+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '')

watch(name, (v) => {
  if (!slugTouched.value || !slug.value) slug.value = slugify(v || '')
})
watch(slug, (v) => {
  if (!v) slugTouched.value = false
})

const pending = ref(false)
const error = ref('')

const submit = async () => {
  error.value = ''
  pending.value = true
  try {
    const res: any = await brands.createBrand({
      name: name.value,
      slug: slug.value || undefined,
      description: description.value || undefined,
      isActive: isActive.value,
    })
    const id = res?.id
    if (id) return router.push(`/admin/brands/${id}`)
    router.push('/admin/brands')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Error'
  } finally {
    pending.value = false
  }
}
</script>
