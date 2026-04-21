<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold rtl-text">إدارة تصنيفات المتجر</h1>
        <p class="text-sm text-white/70 rtl-text">أضف التصنيفات الرئيسية للمتجر، وحدد إن كانت تحتاج تصنيفات دقيقة تظهر داخل دروب منيو منظم في الواجهة وفي نماذج المنتجات.</p>
      </div>
      <UiButton variant="secondary" @click="load">تحديث</UiButton>
    </div>

    <div class="grid gap-6 lg:grid-cols-3">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>{{ editingId ? 'تعديل التصنيف الرئيسي' : 'إضافة تصنيف رئيسي' }}</UiCardTitle>
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
              <UiInput v-model="form.key" placeholder="skin-care" />
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
              <input type="file" accept="image/*" @change="onPickFile($event, 'parent')" class="block w-full text-sm" />
              <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
            </div>
            <label class="flex items-center gap-2 text-sm">
              <input v-model="form.hasDetailSections" type="checkbox" class="h-4 w-4" />
              هذا التصنيف يحتاج تصنيفات دقيقة داخل دروب منيو
            </label>
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
          <UiCardTitle>التصنيفات الرئيسية</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div v-if="loading" class="text-white/70">جاري التحميل...</div>
          <div v-else class="grid gap-3">
            <div v-for="item in items" :key="item.id" class="rounded-3xl border border-white/10 bg-white/5 p-4">
              <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div class="flex items-center gap-3 min-w-0">
                  <div class="h-20 w-20 overflow-hidden rounded-2xl border border-white/10 bg-black/20">
                    <img v-if="item.imageUrl" :src="buildAssetUrl(item.imageUrl)" class="h-full w-full object-cover" />
                    <div v-else class="flex h-full w-full items-center justify-center text-lg font-black">{{ item.nameAr?.slice(0,1) }}</div>
                  </div>
                  <div class="min-w-0">
                    <div class="font-extrabold rtl-text">{{ item.nameAr }}</div>
                    <div class="text-xs text-white/60 keep-ltr">{{ item.key }}</div>
                    <div class="mt-1 text-sm text-white/70 rtl-text">{{ item.descriptionAr }}</div>
                    <div class="mt-2 flex flex-wrap gap-2 text-xs">
                      <span class="rounded-full border border-white/10 px-2 py-1">{{ item.hasDetailSections ? 'عنده تصنيفات دقيقة' : 'تصنيف مباشر' }}</span>
                      <span class="rounded-full border border-white/10 px-2 py-1">{{ item.childCount || 0 }} تصنيف دقيق</span>
                    </div>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <UiButton v-if="item.hasDetailSections" variant="ghost" @click="selectParent(item)">{{ selectedParentId === item.id ? 'إخفاء التصنيفات الدقيقة' : 'التصنيفات الدقيقة' }}</UiButton>
                  <UiButton variant="secondary" @click="editItem(item)">تعديل</UiButton>
                  <UiButton variant="destructive" @click="remove(item.id)">حذف</UiButton>
                </div>
              </div>
            </div>
          </div>
        </UiCardContent>
      </UiCard>
    </div>

    <UiCard v-if="selectedParent">
      <UiCardHeader>
        <UiCardTitle>التصنيفات الدقيقة داخل: {{ selectedParent.nameAr }}</UiCardTitle>
      </UiCardHeader>
      <UiCardContent>
        <div class="grid gap-6 lg:grid-cols-3">
          <div class="grid gap-3">
            <div class="grid gap-2">
              <label class="text-sm">الاسم بالعربي</label>
              <UiInput v-model="childForm.nameAr" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الاسم بالإنكليزي</label>
              <UiInput v-model="childForm.nameEn" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">المفتاح</label>
              <UiInput v-model="childForm.key" placeholder="face-wash" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الوصف</label>
              <UiInput v-model="childForm.descriptionAr" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الترتيب</label>
              <UiInput v-model.number="childForm.sortOrder" type="number" min="0" step="1" />
            </div>
            <div class="grid gap-2">
              <label class="text-sm">الصورة</label>
              <input type="file" accept="image/*" @change="onPickFile($event, 'child')" class="block w-full text-sm" />
              <UiInput v-model="childForm.imageUrl" placeholder="https://..." dir="ltr" />
            </div>
            <label class="flex items-center gap-2 text-sm">
              <input v-model="childForm.isActive" type="checkbox" class="h-4 w-4" />
              فعال
            </label>
            <div class="flex flex-wrap gap-2 pt-2">
              <UiButton @click="saveChild">{{ childEditingId ? 'حفظ التعديل' : 'إضافة التصنيف الدقيق' }}</UiButton>
              <UiButton variant="ghost" @click="resetChildForm">جديد</UiButton>
            </div>
          </div>

          <div class="lg:col-span-2 grid gap-3">
            <div v-if="childLoading" class="text-white/70">جاري تحميل التصنيفات الدقيقة...</div>
            <div v-else-if="childItems.length === 0" class="rounded-2xl border border-white/10 bg-white/5 p-4 text-white/70">لا توجد تصنيفات دقيقة بعد.</div>
            <div v-else v-for="item in childItems" :key="item.id" class="flex flex-col gap-3 rounded-3xl border border-white/10 bg-white/5 p-4 sm:flex-row sm:items-center sm:justify-between">
              <div class="flex items-center gap-3 min-w-0">
                <div class="h-16 w-16 overflow-hidden rounded-2xl border border-white/10 bg-black/20">
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
                <UiButton variant="secondary" @click="editChild(item)">تعديل</UiButton>
                <UiButton variant="destructive" @click="remove(item.id)">حذف</UiButton>
              </div>
            </div>
          </div>
        </div>
      </UiCardContent>
    </UiCard>
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
const section = 'regular'
const loading = ref(false)
const childLoading = ref(false)
const editingId = ref<string>('')
const childEditingId = ref<string>('')
const selectedParentId = ref<string>('')

const form = reactive({ key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null as string | null })
const childForm = reactive({ key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null as string | null })
const childItems = ref<any[]>([])

const selectedParent = computed(() => items.value.find((x: any) => x.id === selectedParentId.value) || null)

function resetForm() {
  editingId.value = ''
  Object.assign(form, { key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null })
}

function resetChildForm() {
  childEditingId.value = ''
  Object.assign(childForm, { key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: selectedParentId.value || null })
}

async function load() {
  loading.value = true
  try {
    items.value = await $fetch('/api/bff/admin/categories', { query: { _ts: Date.now(), section, rootsOnly: true } }) as any[]
    if (selectedParentId.value) await loadChildren(selectedParentId.value)
  } catch {
    items.value = []
  } finally {
    loading.value = false
  }
}

async function loadChildren(parentId: string) {
  if (!parentId) return
  childLoading.value = true
  try {
    childItems.value = await $fetch('/api/bff/admin/categories', { query: { _ts: Date.now(), section, parentId } }) as any[]
  } catch {
    childItems.value = []
  } finally {
    childLoading.value = false
  }
}

function selectParent(item: any) {
  selectedParentId.value = selectedParentId.value === item.id ? '' : item.id
  resetChildForm()
  if (selectedParentId.value) loadChildren(selectedParentId.value)
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
    hasDetailSections: Boolean(item.hasDetailSections ?? false),
    parentId: null,
  })
}

function editChild(item: any) {
  childEditingId.value = item.id
  selectedParentId.value = item.parentId
  Object.assign(childForm, {
    key: item.key || '',
    section: item.section || section,
    nameAr: item.nameAr || '',
    nameEn: item.nameEn || '',
    descriptionAr: item.descriptionAr || '',
    descriptionEn: item.descriptionEn || '',
    imageUrl: item.imageUrl || '',
    sortOrder: Number(item.sortOrder || 0),
    isActive: Boolean(item.isActive ?? true),
    hasDetailSections: false,
    parentId: item.parentId || selectedParentId.value,
  })
}

async function save() {
  try {
    const body = { ...form, section, parentId: null }
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

async function saveChild() {
  if (!selectedParentId.value) return toast.error('اختر تصنيفًا رئيسيًا أولًا')
  try {
    const body = { ...childForm, section, parentId: selectedParentId.value, hasDetailSections: false }
    if (childEditingId.value) {
      await $fetch(`/api/bff/admin/categories/${childEditingId.value}`, { method: 'PUT', body })
      toast.success('تم تحديث التصنيف الدقيق')
    } else {
      await $fetch('/api/bff/admin/categories', { method: 'POST', body })
      toast.success('تمت إضافة التصنيف الدقيق')
    }
    resetChildForm()
    await loadChildren(selectedParentId.value)
    await load()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حفظ التصنيف الدقيق')
  }
}

async function remove(id: string) {
  if (!confirm('حذف هذا العنصر؟')) return
  try {
    await $fetch(`/api/bff/admin/categories/${id}`, { method: 'DELETE' })
    toast.success('تم الحذف')
    if (editingId.value === id) resetForm()
    if (childEditingId.value === id) resetChildForm()
    await load()
    if (selectedParentId.value) await loadChildren(selectedParentId.value)
  } catch (e: any) {
    toast.error(e?.data?.message || 'تعذر الحذف')
  }
}

async function onPickFile(e: Event, target: 'parent' | 'child') {
  const file = (e.target as HTMLInputElement)?.files?.[0]
  if (!file) return
  try {
    const fd = new FormData()
    fd.append('file', file)
    const res: any = await $fetch('/api/bff/admin/categories/upload', { method: 'POST', body: fd })
    if (target === 'parent') form.imageUrl = res?.url || ''
    else childForm.imageUrl = res?.url || ''
    toast.success('تم رفع الصورة')
  } catch {
    toast.error('تعذر رفع الصورة')
  } finally {
    ;(e.target as HTMLInputElement).value = ''
  }
}

onMounted(load)
</script>
