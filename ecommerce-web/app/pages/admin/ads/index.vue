<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

import UiButton from '~/components/ui/UiButton.vue'
import UiCard from '~/components/ui/UiCard.vue'
import UiCardHeader from '~/components/ui/UiCardHeader.vue'
import UiCardTitle from '~/components/ui/UiCardTitle.vue'
import UiCardContent from '~/components/ui/UiCardContent.vue'
import UiInput from '~/components/ui/UiInput.vue'

const { t } = useI18n()
const toast = useToast()

const loading = ref(true)
const items = ref<any[]>([])

const form = reactive({
  type: 'banner',
  placement: 'home_top',
  title: '',
  subtitle: '',
  imageUrl: '',
  linkUrl: '',
  productId: '',
  sortOrder: 0,
  isEnabled: true,
})

const uploading = ref(false)

async function load() {
  loading.value = true
  try {
    const res: any = await $fetch('/api/bff/admin/ads', { timeout: 8000 })
    items.value = Array.isArray(res) ? res : []
  } catch {
    items.value = []
  } finally {
    loading.value = false
  }
}

async function create() {
  try {
    await $fetch('/api/bff/admin/ads', {
      method: 'POST',
      body: {
        type: form.type,
        placement: form.placement,
        title: form.title,
        subtitle: form.subtitle || null,
        imageUrl: form.imageUrl,
        linkUrl: form.linkUrl || null,
        productId: form.productId ? form.productId : null,
        sortOrder: Number(form.sortOrder || 0),
        isEnabled: Boolean(form.isEnabled),
        startAt: null,
        endAt: null,
      },
    })
    toast.success(t('common.saved') || 'تم الحفظ')
    Object.assign(form, { title: '', subtitle: '', imageUrl: '', linkUrl: '', productId: '', sortOrder: 0, isEnabled: true })
    await load()
  } catch {
    toast.error(t('common.errorGeneric') || 'حصل خطأ')
  }
}

async function remove(id: string) {
  if (!confirm(t('admin.confirmDeleteAd') || 'حذف الإعلان؟')) return
  try {
    await $fetch(`/api/bff/admin/ads/${id}`, { method: 'DELETE' })
    toast.success(t('common.deleted') || 'تم الحذف')
    await load()
  } catch {
    toast.error(t('common.errorGeneric') || 'حصل خطأ')
  }
}

async function toggleEnabled(ad: any) {
  try {
    await $fetch(`/api/bff/admin/ads/${ad.id}`, {
      method: 'PUT',
      body: {
        type: ad.type,
        placement: ad.placement,
        title: ad.title,
        subtitle: ad.subtitle || null,
        imageUrl: ad.imageUrl,
        linkUrl: ad.linkUrl || null,
        productId: ad.productId || null,
        sortOrder: Number(ad.sortOrder || 0),
        isEnabled: !ad.isEnabled,
        startAt: ad.startAt || null,
        endAt: ad.endAt || null,
      },
    })
    await load()
  } catch {
    toast.error(t('common.errorGeneric') || 'حصل خطأ')
  }
}

async function onPickFile(e: Event) {
  const file = (e.target as HTMLInputElement)?.files?.[0]
  if (!file) return
  uploading.value = true
  try {
    const fd = new FormData()
    fd.append('file', file)
    const res: any = await $fetch('/api/bff/admin/ads/upload', {
      method: 'POST',
      body: fd,
    })
    form.imageUrl = res?.url || ''
  } catch {
    toast.error(t('common.errorGeneric') || 'حصل خطأ')
  } finally {
    uploading.value = false
    ;(e.target as HTMLInputElement).value = ''
  }
}

onMounted(load)
</script>

<template>
  <div class="w-full">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold">{{ t('admin.adsTitle') || 'الإعلانات' }}</h1>
        <p class="text-sm text-white/70">{{ t('admin.adsHintTypes') || 'Popup / Banner / Product Ads' }}</p>
      </div>
      <UiButton variant="secondary" :disabled="loading" @click="load">{{ t('common.refresh') || 'تحديث' }}</UiButton>
    </div>

    <div class="mt-6 grid gap-6 lg:grid-cols-3">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>{{ t('admin.createAd') || 'إنشاء إعلان' }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-3">
            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adType') || 'النوع' }}</label>
              <select v-model="form.type" class="h-10 w-full rounded-2xl border border-white/10 bg-white/5 px-3 text-sm outline-none">
                <option value="popup">{{ t('admin.adTypePopup') || 'Popup' }}</option>
                <option value="banner">{{ t('admin.adTypeBanner') || 'Banner' }}</option>
                <option value="product">{{ t('admin.adTypeProduct') || 'Product' }}</option>
              </select>
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adPlacement') || 'الموضع' }}</label>
              <UiInput v-model="form.placement" :placeholder="t('admin.adPlacementPlaceholder') || 'home_top'" dir="ltr" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adTitle') || 'العنوان' }}</label>
              <UiInput v-model="form.title" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adSubtitle') || 'العنوان الفرعي' }}</label>
              <UiInput v-model="form.subtitle" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adImage') || 'الصورة' }}</label>
              <div class="flex items-center gap-2">
                <input type="file" accept="image/*" class="text-xs" @change="onPickFile" />
                <span v-if="uploading" class="text-xs text-white/60">{{ t('common.uploading') || 'جاري الرفع...' }}</span>
              </div>
              <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
              <img v-if="form.imageUrl" :src="form.imageUrl" class="mt-1 h-24 w-full object-cover rounded-2xl border border-white/10" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adLink') || 'الرابط' }}</label>
              <UiInput v-model="form.linkUrl" :placeholder="t('admin.adLinkPlaceholder') || '/products'" dir="ltr" />
            </div>

            <div v-if="form.type === 'product'" class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adProductId') || 'معرف المنتج' }}</label>
              <UiInput v-model="form.productId" :placeholder="t('admin.adProductIdPlaceholder') || 'guid'" dir="ltr" />
            </div>

            <div class="grid gap-2">
              <label class="text-sm font-medium">{{ t('admin.adSortOrder') || 'الترتيب' }}</label>
              <UiInput v-model.number="form.sortOrder" type="number" min="0" step="1" />
            </div>

            <label class="flex items-center gap-2 text-sm">
              <input type="checkbox" v-model="form.isEnabled" class="h-4 w-4" />
              {{ t('common.enabled') || 'مفعل' }}
            </label>

            <UiButton type="button" @click="create">{{ t('common.save') || 'حفظ' }}</UiButton>
          </div>
        </UiCardContent>
      </UiCard>

      <UiCard class="lg:col-span-2">
        <UiCardHeader>
          <UiCardTitle>{{ t('admin.adsList') || 'قائمة الإعلانات' }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div v-if="loading" class="text-white/70">{{ t('common.loading') }}</div>
          <div v-else-if="!items.length" class="text-white/60">{{ t('admin.noAdsYet') || 'لا توجد إعلانات بعد' }}</div>
          <div v-else class="grid gap-3">
            <div v-for="ad in items" :key="ad.id" class="rounded-2xl border border-white/10 bg-white/5 p-3">
              <div class="flex items-start justify-between gap-3">
                <div class="flex items-center gap-3 min-w-0">
                  <img :src="ad.imageUrl" class="h-12 w-12 rounded-2xl object-cover border border-white/10" />
                  <div class="min-w-0">
                    <div class="font-extrabold truncate">{{ ad.title }}</div>
                    <div class="text-xs text-white/60 keep-ltr">{{ ad.type }} • {{ ad.placement }} • sort: {{ ad.sortOrder }}</div>
                  </div>
                </div>

                <div class="flex items-center gap-2">
                  <button
                    type="button"
                    class="rounded-xl border border-white/10 bg-white/5 px-3 py-1.5 text-xs hover:bg-white/10"
                    @click="toggleEnabled(ad)"
                  >
                    {{ ad.isEnabled ? (t('common.disable') || 'تعطيل') : (t('common.enable') || 'تفعيل') }}
                  </button>
                  <button
                    type="button"
                    class="rounded-xl border border-white/10 bg-white/5 px-3 py-1.5 text-xs hover:bg-white/10"
                    @click="remove(ad.id)"
                  >
                    {{ t('common.delete') || 'حذف' }}
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
