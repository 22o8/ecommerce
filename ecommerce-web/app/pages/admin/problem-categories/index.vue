<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold rtl-text">إدارة تصنيفات حل المشاكل</h1>
        <p class="text-sm text-white/70 rtl-text">أضف تصنيفات حل المشاكل مع الصورة، وستظهر تحت البراندات وفوق المنتجات المميزة، وكذلك في نماذج المنتجات.</p>
      </div>
      <UiButton variant="secondary" @click="load">تحديث</UiButton>
    </div>

    <div class="grid gap-6 lg:grid-cols-3">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>{{ editingId ? 'تعديل التصنيف' : 'إضافة تصنيف' }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-3">
            <div class="grid gap-2">
              <label class="text-sm">الاسم بالعربي</label>
              <UiInput v-model="form.nameAr" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الاسم بالإنكليزي</label>
              <UiInput v-model="form.nameEn" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">المفتاح</label>
              <UiInput v-model="form.key" placeholder="sunscreen" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الوصف</label>
              <UiInput v-model="form.descriptionAr" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الترتيب</label>
              <UiInput v-model.number="form.sortOrder" type="number" min="0" step="1" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الصورة</label>
              <input type="file" accept="image/*" @change="onPickFile" class="block w-full text-sm" />
              <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
              <div v-if="form.imageUrl" class="overflow-hidden rounded-2xl border border-white/10 bg-white/5 p-2">
                <img :src="buildAssetUrl(form.imageUrl)" class="h-40 w-full object-contain" />
              </div>
            </div>
            <label class="flex items-center gap-2 text-sm">
              <input v-model="form.isActive" type="checkbox" class="h-4 w-4" />
              فعال
            </label>
            <div class="flex flex-wrap gap-2 pt-2">
              <UiButton @click="save">{{ editingId ? 'حفظ التعديل' : 'إضافة' }}</UiButton>
              <UiButton variant="ghost" @click="resetForm">جديد</UiButton>
            </div>
          </div>
        </UiCardContent>
      </UiCard>

      <UiCard class="lg:col-span-2">
        <UiCardHeader>
          <UiCardTitle>التصنيفات المسجلة</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div v-if="loading" class="text-white/70">جاري التحميل...</div>
          <div v-else class="grid gap-3">
            <div v-for="item in items" :key="item.id" class="flex flex-col gap-3 rounded-3xl border border-white/10 bg-white/5 p-4 sm:flex-row sm:items-center sm:justify-between">
              <div class="flex items-center gap-3 min-w-0">
                <div class="h-20 w-20 overflow-hidden rounded-2xl border border-white/10 bg-black/20">
                  <img v-if="item.imageUrl" :src="buildAssetUrl(item.imageUrl)" class="h-full w-full object-cover" />
                  <div v-else class="flex h-full w-full items-center justify-center text-lg font-black">{{ item.nameAr?.slice(0,1) }}</div>
                </div>
                <div class="min-w-0">
                  <div class="font-extrabold rtl-text">{{ item.nameAr }}</div>
                  <div class="text-xs text-white/60 keep-ltr">{{ item.key }}</div>
                  <div class="mt-1 text-sm text-white/70 rtl-text">{{ item.descriptionAr }}</div>
                </div>
              </div>
              <div class="flex items-center gap-2">
                <UiButton variant="secondary" @click="editItem(item)">تعديل</UiButton>
                <UiButton variant="destructive" @click="remove(item.id)">حذف</UiButton>
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
import UiButton from '~/components/ui/UiButton.vue'
import UiCard from '~/components/ui/UiCard.vue'
import UiCardHeader from '~/components/ui/UiCardHeader.vue'
import UiCardTitle from '~/components/ui/UiCardTitle.vue'
import UiCardContent from '~/components/ui/UiCardContent.vue'
import UiInput from '~/components/ui/UiInput.vue'

const toast = useToast()
const { buildAssetUrl } = useApi()
const items = ref<any[]>([])
const section = 'problem'
const loading = ref(false)
const editingId = ref<string>('')
const form = reactive({ key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section })

function resetForm() {
  editingId.value = ''
  Object.assign(form, { key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section })
}

async function load() {
  loading.value = true
  try {
    items.value = await $fetch('/api/bff/admin/categories', { query: { _ts: Date.now(), section } }) as any[]
  } catch {
    items.value = []
  } finally {
    loading.value = false
  }
}

function editItem(item: any) {
  editingId.value = item.id
  Object.assign(form, {
    key: item.key || '',
    section: item.section || section,
    nameAr: item.nameAr || '',
    nameEn: item.nameEn || '',
    descriptionAr: item.descriptionAr || '',
    descriptionEn: item.descriptionEn || '',
    imageUrl: item.imageUrl || '',
    sortOrder: Number(item.sortOrder || 0),
    isActive: Boolean(item.isActive ?? true),
  })
}

async function save() {
  try {
    const body = { ...form, section }
    if (editingId.value) {
      await $fetch(`/api/bff/admin/categories/${editingId.value}`, { method: 'PUT', body })
      toast.success('تم تحديث التصنيف')
    } else {
      await $fetch('/api/bff/admin/categories', { method: 'POST', body })
      toast.success('تمت إضافة التصنيف')
    }
    resetForm()
    await load()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حفظ التصنيف')
  }
}

async function remove(id: string) {
  if (!confirm('حذف التصنيف؟')) return
  try {
    await $fetch(`/api/bff/admin/categories/${id}`, { method: 'DELETE' })
    toast.success('تم الحذف')
    if (editingId.value === id) resetForm()
    await load()
  } catch {
    toast.error('تعذر الحذف')
  }
}

async function onPickFile(e: Event) {
  const file = (e.target as HTMLInputElement)?.files?.[0]
  if (!file) return
  try {
    const fd = new FormData()
    fd.append('file', file)
    const res: any = await $fetch('/api/bff/admin/categories/upload', { method: 'POST', body: fd })
    form.imageUrl = res?.url || ''
    toast.success('تم رفع الصورة')
  } catch {
    toast.error('تعذر رفع الصورة')
  } finally {
    ;(e.target as HTMLInputElement).value = ''
  }
}

onMounted(load)
</script>
