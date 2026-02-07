<template>
  <div class="space-y-4">
    <div class="admin-box max-w-4xl">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="text-2xl font-extrabold rtl-text">{{ t('admin.editBrand') }}</div>
          <div class="admin-muted rtl-text mt-1">ID: {{ id }}</div>
        </div>
        <div class="flex gap-2">
          <NuxtLink class="admin-ghost" to="/admin/brands">{{ t('admin.backToBrands') }}</NuxtLink>
        </div>
      </div>

      <div class="mt-6 grid gap-6 md:grid-cols-[220px,1fr]">
        <!-- Logo upload -->
        <div class="rounded-2xl border border-white/10 bg-white/[0.03] p-4">
          <div class="font-extrabold rtl-text">{{ t('admin.logo') }}</div>
          <div class="admin-muted text-sm rtl-text mt-1">{{ t('admin.logoHint') }}</div>

          <div class="mt-4 w-full aspect-square rounded-2xl overflow-hidden border border-white/10 bg-black/20 flex items-center justify-center">
            <SmartImage v-if="logoPreview" :src="logoPreview" :alt="name" class="w-full h-full object-cover" />
            <div v-else class="text-xs admin-muted">LOGO</div>
          </div>

          <input class="mt-4 block w-full text-sm" type="file" accept="image/*" @change="onPick" />

          <button class="admin-primary w-full mt-3" type="button" :disabled="!picked || pendingLogo" @click="upload">
            {{ pendingLogo ? t('common.uploading') : t('common.upload') }}
          </button>

          <div v-if="logoError" class="admin-error rtl-text mt-2">{{ logoError }}</div>
        </div>

        <!-- Details -->
        <div>
          <form class="grid gap-4" @submit.prevent="save">
            <div class="grid gap-2">
              <label class="admin-label rtl-text">{{ t('admin.name') }}</label>
              <input v-model="name" class="admin-input" required />
            </div>

            <div class="grid gap-2">
              <label class="admin-label rtl-text">{{ t('admin.slug') }}</label>
              <input v-model="slug" class="admin-input" @input="slugTouched = true" />
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
                {{ pending ? t('common.saving') : t('common.save') }}
              </button>
              <button class="admin-btn-danger" type="button" :disabled="pending" @click="disable">
                {{ t('admin.disable') }}
              </button>
            </div>

            <div v-if="error" class="admin-error rtl-text">{{ error }}</div>
            <div v-if="ok" class="admin-ok rtl-text">{{ ok }}</div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['auth'] })

import SmartImage from '~/components/SmartImage.vue'

const { t } = useI18n()
const route = useRoute()
const id = computed(() => String(route.params.id || ''))

const brands = useBrandsStore()
const { buildAssetUrl } = useApi()

const name = ref('')
const slug = ref('')
const description = ref('')
const isActive = ref(true)
const logoUrl = ref('')

const slugTouched = ref(false)

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

watch(name, (v) => {
  if (slugTouched.value) return
  const next = slugify(v)
  if (!slug.value || slug.value === slugify(slug.value)) {
    slug.value = next
  }
})

watch(slug, (v) => {
  if (!v) slugTouched.value = false
})

const picked = ref<File | null>(null)
const pending = ref(false)
const pendingLogo = ref(false)
const error = ref('')
const ok = ref('')
const logoError = ref('')

const logoPreview = computed(() => {
  if (picked.value) return URL.createObjectURL(picked.value)
  return logoUrl.value ? buildAssetUrl(logoUrl.value) : ''
})

await useAsyncData(`admin-brand-${id.value}`, async () => {
  const b: any = await useApi().get(`/admin/brands/${id.value}`)
  name.value = b?.name || ''
  slug.value = b?.slug || ''
  description.value = b?.description || ''
  isActive.value = !!b?.isActive
  logoUrl.value = b?.logoUrl || ''
  return true
})

const onPick = (e: Event) => {
  logoError.value = ''
  const f = (e.target as HTMLInputElement)?.files?.[0]
  picked.value = f || null
}

const upload = async () => {
  if (!picked.value) return
  logoError.value = ''
  pendingLogo.value = true
  try {
    const res: any = await brands.uploadLogo(id.value, picked.value)
    logoUrl.value = res?.logoUrl || logoUrl.value
    picked.value = null
  } catch (e: any) {
    logoError.value = e?.data?.message || e?.message || 'Error'
  } finally {
    pendingLogo.value = false
  }
}

const save = async () => {
  error.value = ''
  ok.value = ''
  pending.value = true
  try {
    await brands.updateBrand(id.value, {
      name: name.value,
      slug: slug.value,
      description: description.value,
      isActive: isActive.value,
    })
    ok.value = t('common.saved')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Error'
  } finally {
    pending.value = false
  }
}

const disable = async () => {
  isActive.value = false
  await save()
}
</script>
