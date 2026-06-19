<template>
  <div class="admin-category-page space-y-6">
    <section class="admin-page-hero">
      <div class="min-w-0">
        <div class="admin-kicker rtl-text">
          <span class="admin-kicker__dot" />
          إدارة أقسام المتجر
        </div>
        <h1 class="mt-3 text-2xl font-black tracking-tight text-[rgb(var(--text))] sm:text-3xl rtl-text">
          التصنيفات الرئيسية والتصنيفات الدقيقة
        </h1>
        <p class="mt-2 max-w-4xl text-sm leading-7 text-[rgb(var(--muted))] rtl-text">
          رتّب أقسام المتجر من هنا: التصنيف المباشر يفتح صفحة منتجاته فوراً، والتصنيف الذي يحتوي أقساماً دقيقة يظهر كقائمة منظمة في الواجهة وداخل نماذج المنتجات.
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <UiButton v-if="selectedParent" variant="ghost" @click="clearSelection">إغلاق اللوحة</UiButton>
        <UiButton variant="secondary" @click="load">تحديث</UiButton>
      </div>
    </section>

    <section class="grid gap-3 sm:grid-cols-3">
      <div class="stat-card">
        <span>كل التصنيفات</span>
        <strong>{{ items.length }}</strong>
      </div>
      <div class="stat-card stat-card--detail">
        <span>تحتوي تصنيفات دقيقة</span>
        <strong>{{ detailedItems.length }}</strong>
      </div>
      <div class="stat-card stat-card--direct">
        <span>تصنيفات مباشرة</span>
        <strong>{{ directItems.length }}</strong>
      </div>
    </section>

    <section class="grid gap-6 xl:grid-cols-[420px_minmax(0,1fr)]">
      <UiCard class="category-editor-card">
        <UiCardHeader>
          <UiCardTitle>{{ editingId ? 'تعديل تصنيف رئيسي' : 'إضافة تصنيف رئيسي' }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-4">
            <div class="rounded-3xl border border-app bg-surface-2/60 p-4 text-sm leading-7 text-[rgb(var(--muted))] rtl-text">
              املأ بيانات التصنيف الرئيسي. إذا يحتاج أقساماً داخله، فعّل خيار التصنيفات الدقيقة ثم احفظ، بعدها افتح زر الإدارة الخاص به.
            </div>

            <div v-if="parentConflict" class="duplicate-alert rtl-text">
              <strong>تنبيه: يوجد تصنيف مشابه بالفعل</strong>
              <span>{{ conflictLabel(parentConflict.item) }} — السبب: {{ parentConflict.reason }}</span>
              <div class="duplicate-alert__actions">
                <UiButton variant="secondary" @click="goToConflict(parentConflict.item)">عرض مكانه</UiButton>
                <UiButton variant="ghost" @click="editItem(parentConflict.item)">تعديل الموجود</UiButton>
              </div>
            </div>

            <div class="grid gap-2">
              <label class="admin-label">الاسم بالعربي</label>
              <UiInput v-model="form.nameAr" placeholder="مثال: عناية البشرة" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الاسم بالإنكليزي</label>
              <UiInput v-model="form.nameEn" placeholder="Skin Care" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">المفتاح</label>
              <UiInput v-model="form.key" placeholder="skin-care" dir="ltr" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الوصف</label>
              <UiInput v-model="form.descriptionAr" placeholder="وصف قصير يظهر في الواجهة" />
            </div>
            <div class="grid gap-2 sm:grid-cols-2">
              <div class="grid gap-2">
                <label class="admin-label">الترتيب</label>
                <UiInput v-model.number="form.sortOrder" type="number" min="0" step="1" />
              </div>
              <label class="toggle-box mt-7">
                <input v-model="form.isActive" type="checkbox" />
                <span>فعال</span>
              </label>
            </div>

            <div class="grid gap-2">
              <label class="admin-label">الصورة</label>
              <div class="upload-line">
                <input type="file" accept="image/*" @change="onPickFile($event, 'parent')" class="block w-full text-sm" />
              </div>
              <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
            </div>

            <label class="detail-switch" :class="form.hasDetailSections ? 'is-on' : ''">
              <input v-model="form.hasDetailSections" type="checkbox" />
              <span class="detail-switch__mark" />
              <span>
                <strong>يحتاج تصنيفات دقيقة</strong>
                <small>يظهر كـ Dropdown في الواجهة، وتظهر أقسامه عند إضافة المنتج.</small>
              </span>
            </label>

            <div class="flex flex-wrap gap-2 pt-2">
              <UiButton @click="save">{{ editingId ? 'حفظ التعديل' : 'إنشاء التصنيف' }}</UiButton>
              <UiButton variant="ghost" @click="resetForm">تفريغ النموذج</UiButton>
            </div>
          </div>
        </UiCardContent>
      </UiCard>

      <div class="space-y-5 min-w-0">
        <UiCard>
          <UiCardContent>
            <div class="grid gap-3 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-center">
              <div class="relative">
                <UiInput v-model="search" placeholder="ابحث باسم التصنيف أو المفتاح..." />
              </div>
              <div class="filter-tabs">
                <button :class="filterMode === 'all' ? 'is-active' : ''" @click="filterMode = 'all'">الكل</button>
                <button :class="filterMode === 'detail' ? 'is-active' : ''" @click="filterMode = 'detail'">بتصنيفات دقيقة</button>
                <button :class="filterMode === 'direct' ? 'is-active' : ''" @click="filterMode = 'direct'">مباشر فقط</button>
              </div>
            </div>
          </UiCardContent>
        </UiCard>

        <UiCard v-if="loading">
          <UiCardContent>
            <div class="py-10 text-center text-[rgb(var(--muted))]">جاري التحميل...</div>
          </UiCardContent>
        </UiCard>

        <template v-else>
          <section v-if="showDetailSection" class="category-group category-group--detail">
            <div class="category-group__head">
              <div>
                <h2>تصنيفات تحتوي أقساماً دقيقة</h2>
                <p>هذه تظهر في الواجهة كقائمة منسدلة، ويجب اختيار قسم دقيق عند ربط المنتج بها.</p>
              </div>
              <span>{{ visibleDetailedItems.length }}</span>
            </div>

            <div v-if="visibleDetailedItems.length" class="grid gap-3">
              <article v-for="item in visibleDetailedItems" :id="`category-${item.id}`" :key="item.id" class="category-admin-card has-detail" :class="selectedParentId === item.id ? 'is-selected' : ''">
                <div class="category-admin-card__media" @click="selectParent(item)">
                  <img v-if="item.imageUrl" :src="buildAssetUrl(item.imageUrl)" alt="" />
                  <span v-else>{{ item.nameAr?.slice(0,1) }}</span>
                </div>

                <div class="category-admin-card__body">
                  <div class="flex flex-wrap items-start justify-between gap-3">
                    <div class="min-w-0">
                      <h3>{{ item.nameAr }}</h3>
                      <p class="keep-ltr">{{ item.key }}</p>
                      <small>{{ item.descriptionAr || 'بدون وصف' }}</small>
                    </div>
                    <span class="status-pill" :class="item.isActive ? 'is-active' : 'is-off'">{{ item.isActive ? 'فعال' : 'غير فعال' }}</span>
                  </div>

                  <div class="category-admin-card__meta">
                    <span>Dropdown</span>
                    <span>{{ item.childCount || 0 }} تصنيف دقيق</span>
                    <span>ترتيب {{ item.sortOrder || 0 }}</span>
                  </div>
                </div>

                <div class="category-admin-card__actions">
                  <UiButton variant="ghost" @click="selectParent(item)">إدارة التصنيفات الدقيقة</UiButton>
                  <UiButton variant="secondary" @click="editItem(item)">تعديل</UiButton>
                  <UiButton variant="danger" @click="remove(item.id)">حذف</UiButton>
                </div>
              </article>
            </div>
            <div v-else class="empty-panel">لا توجد نتائج ضمن هذا النوع.</div>
          </section>

          <section v-if="showDirectSection" class="category-group category-group--direct">
            <div class="category-group__head">
              <div>
                <h2>تصنيفات مباشرة</h2>
                <p>هذه تفتح صفحة منتجات التصنيف مباشرة بدون اختيار قسم دقيق.</p>
              </div>
              <span>{{ visibleDirectItems.length }}</span>
            </div>

            <div v-if="visibleDirectItems.length" class="grid gap-3">
              <article v-for="item in visibleDirectItems" :id="`category-${item.id}`" :key="item.id" class="category-admin-card is-direct">
                <div class="category-admin-card__media" @click="editItem(item)">
                  <img v-if="item.imageUrl" :src="buildAssetUrl(item.imageUrl)" alt="" />
                  <span v-else>{{ item.nameAr?.slice(0,1) }}</span>
                </div>

                <div class="category-admin-card__body">
                  <div class="flex flex-wrap items-start justify-between gap-3">
                    <div class="min-w-0">
                      <h3>{{ item.nameAr }}</h3>
                      <p class="keep-ltr">{{ item.key }}</p>
                      <small>{{ item.descriptionAr || 'بدون وصف' }}</small>
                    </div>
                    <span class="status-pill" :class="item.isActive ? 'is-active' : 'is-off'">{{ item.isActive ? 'فعال' : 'غير فعال' }}</span>
                  </div>

                  <div class="category-admin-card__meta">
                    <span>تصنيف مباشر</span>
                    <span>بدون أقسام دقيقة</span>
                    <span>ترتيب {{ item.sortOrder || 0 }}</span>
                  </div>
                </div>

                <div class="category-admin-card__actions">
                  <UiButton variant="secondary" @click="editItem(item)">تعديل</UiButton>
                  <UiButton variant="danger" @click="remove(item.id)">حذف</UiButton>
                </div>
              </article>
            </div>
            <div v-else class="empty-panel">لا توجد نتائج ضمن هذا النوع.</div>
          </section>
        </template>
      </div>
    </section>

    <Transition name="drawer-fade">
      <div v-if="selectedParent" class="drawer-backdrop" @click.self="clearSelection">
        <aside class="detail-drawer">
          <header class="detail-drawer__head">
            <div>
              <span class="admin-kicker rtl-text"><span class="admin-kicker__dot" /> لوحة التصنيفات الدقيقة</span>
              <h2>داخل: {{ selectedParent.nameAr }}</h2>
              <p>أي تصنيف تضيفه هنا يظهر داخل دروب منيو هذا القسم، وكذلك داخل اختيار التصنيف الدقيق في المنتج.</p>
            </div>
            <button type="button" class="drawer-close" @click="clearSelection">×</button>
          </header>

          <div class="detail-drawer__content">
            <section class="drawer-form">
              <h3>{{ childEditingId ? 'تعديل تصنيف دقيق' : 'إضافة تصنيف دقيق' }}</h3>
              <div v-if="childConflict" class="duplicate-alert duplicate-alert--compact rtl-text">
                <strong>تنبيه: يوجد تصنيف دقيق مشابه بالفعل</strong>
                <span>{{ conflictLabel(childConflict.item) }} — السبب: {{ childConflict.reason }}</span>
                <div class="duplicate-alert__actions">
                  <UiButton variant="secondary" @click="goToConflict(childConflict.item)">عرض مكانه</UiButton>
                  <UiButton variant="ghost" @click="editChild(childConflict.item)">تعديل الموجود</UiButton>
                </div>
              </div>
              <div class="grid gap-3 sm:grid-cols-2">
                <div class="grid gap-2">
                  <label class="admin-label">الاسم بالعربي</label>
                  <UiInput v-model="childForm.nameAr" />
                </div>
                <div class="grid gap-2">
                  <label class="admin-label">الاسم بالإنكليزي</label>
                  <UiInput v-model="childForm.nameEn" />
                </div>
                <div class="grid gap-2">
                  <label class="admin-label">المفتاح</label>
                  <UiInput v-model="childForm.key" placeholder="face-wash" dir="ltr" />
                </div>
                <div class="grid gap-2">
                  <label class="admin-label">الترتيب</label>
                  <UiInput v-model.number="childForm.sortOrder" type="number" min="0" step="1" />
                </div>
              </div>
              <div class="mt-3 grid gap-2">
                <label class="admin-label">الوصف</label>
                <UiInput v-model="childForm.descriptionAr" />
              </div>
              <div class="mt-3 grid gap-2">
                <label class="admin-label">الصورة</label>
                <div class="upload-line">
                  <input type="file" accept="image/*" @change="onPickFile($event, 'child')" class="block w-full text-sm" />
                </div>
                <UiInput v-model="childForm.imageUrl" placeholder="https://..." dir="ltr" />
              </div>

              <div class="mt-4 flex flex-wrap items-center justify-between gap-3">
                <label class="toggle-box">
                  <input v-model="childForm.isActive" type="checkbox" />
                  <span>فعال</span>
                </label>
                <div class="flex flex-wrap gap-2">
                  <UiButton @click="saveChild">{{ childEditingId ? 'حفظ التعديل' : 'إضافة التصنيف الدقيق' }}</UiButton>
                  <UiButton variant="ghost" @click="resetChildForm">جديد</UiButton>
                </div>
              </div>
            </section>

            <section class="drawer-list">
              <div class="drawer-list__head">
                <h3>التصنيفات الدقيقة المسجلة</h3>
                <span>{{ childItems.length }}</span>
              </div>

              <div v-if="childLoading" class="empty-panel">جاري تحميل التصنيفات الدقيقة...</div>
              <div v-else-if="childItems.length === 0" class="empty-panel">لا توجد تصنيفات دقيقة بعد.</div>
              <article v-else v-for="item in childItems" :id="`category-${item.id}`" :key="item.id" class="child-card">
                <div class="child-card__media">
                  <img v-if="item.imageUrl" :src="buildAssetUrl(item.imageUrl)" alt="" />
                  <span v-else>{{ item.nameAr?.slice(0,1) }}</span>
                </div>
                <div class="min-w-0 flex-1">
                  <h4>{{ item.nameAr }}</h4>
                  <p class="keep-ltr">{{ item.key }}</p>
                  <small>{{ item.descriptionAr || 'بدون وصف' }}</small>
                </div>
                <div class="child-card__actions">
                  <UiButton variant="secondary" @click="editChild(item)">تعديل</UiButton>
                  <UiButton variant="danger" @click="remove(item.id)">حذف</UiButton>
                </div>
              </article>
            </section>
          </div>
        </aside>
      </div>
    </Transition>
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
const search = ref('')
const filterMode = ref<'all' | 'detail' | 'direct'>('all')

const form = reactive({ key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null as string | null })
const childForm = reactive({ key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null as string | null })
const childItems = ref<any[]>([])

const selectedParent = computed(() => items.value.find((x: any) => x.id === selectedParentId.value) || null)
const sortedItems = computed(() => [...items.value].sort((a: any, b: any) => Number(a.sortOrder || 0) - Number(b.sortOrder || 0) || String(a.nameAr || '').localeCompare(String(b.nameAr || ''), 'ar')))
const detailedItems = computed(() => sortedItems.value.filter((x: any) => Boolean(x.hasDetailSections)))
const directItems = computed(() => sortedItems.value.filter((x: any) => !x.hasDetailSections))

function matchesSearch(item: any) {
  const q = search.value.trim().toLowerCase()
  if (!q) return true
  return [item.nameAr, item.nameEn, item.key, item.descriptionAr].some((v) => String(v || '').toLowerCase().includes(q))
}

const visibleDetailedItems = computed(() => detailedItems.value.filter(matchesSearch))
const visibleDirectItems = computed(() => directItems.value.filter(matchesSearch))
const showDetailSection = computed(() => filterMode.value === 'all' || filterMode.value === 'detail')
const showDirectSection = computed(() => filterMode.value === 'all' || filterMode.value === 'direct')

type ConflictResult = { item: any; reason: string } | null

const parentConflict = computed<ConflictResult>(() => findCategoryConflict(form, items.value, editingId.value))
const childConflict = computed<ConflictResult>(() => findCategoryConflict(childForm, childItems.value, childEditingId.value))

function normalizeDuplicateText(value: any) {
  return String(value || '')
    .trim()
    .toLowerCase()
    .replace(/[أإآ]/g, 'ا')
    .replace(/ى/g, 'ي')
    .replace(/ة/g, 'ه')
    .replace(/[^\p{L}\p{N}]+/gu, ' ')
    .replace(/\s+/g, ' ')
    .trim()
}

function normalizeSlugText(value: any) {
  return String(value || '').trim().toLowerCase().replace(/[_\s]+/g, '-').replace(/-+/g, '-').replace(/^-|-$/g, '')
}

function findCategoryConflict(source: any, list: any[], currentId = ''): ConflictResult {
  const key = normalizeSlugText(source.key)
  const nameAr = normalizeDuplicateText(source.nameAr)
  const nameEn = normalizeDuplicateText(source.nameEn)
  const desc = normalizeDuplicateText(source.descriptionAr)
  const values = [key, nameAr, nameEn].filter((x) => x.length >= 2)
  const descWords = desc.split(' ').filter((x) => x.length >= 4)

  for (const item of list || []) {
    if (!item?.id || item.id === currentId) continue
    const itemKey = normalizeSlugText(item.key)
    const itemNameAr = normalizeDuplicateText(item.nameAr)
    const itemNameEn = normalizeDuplicateText(item.nameEn)
    const itemDesc = normalizeDuplicateText(item.descriptionAr)
    const itemValues = [itemKey, itemNameAr, itemNameEn].filter((x) => x.length >= 2)

    if (key && itemKey && key === itemKey) return { item, reason: 'نفس المفتاح' }
    if (nameAr && itemNameAr && nameAr === itemNameAr) return { item, reason: 'نفس الاسم العربي' }
    if (nameEn && itemNameEn && nameEn === itemNameEn) return { item, reason: 'نفس الاسم الإنكليزي' }

    for (const value of values) {
      for (const itemValue of itemValues) {
        if (value.length >= 4 && itemValue.length >= 4 && (value.includes(itemValue) || itemValue.includes(value))) {
          return { item, reason: 'تشابه قوي في الاسم أو المفتاح' }
        }
      }
    }

    for (const word of descWords) {
      if ([itemKey, itemNameAr, itemNameEn, itemDesc].some((v) => v && v.includes(word))) {
        return { item, reason: `الكلمة المفتاحية "${word}" موجودة ضمن تصنيف آخر` }
      }
    }
  }
  return null
}

function conflictLabel(item: any) {
  const place = item?.parentId ? `داخل: ${selectedParent.value?.nameAr || 'تصنيف رئيسي'}` : (item?.hasDetailSections ? 'ضمن تصنيفات تحتوي أقسام دقيقة' : 'ضمن التصنيفات المباشرة')
  return `${item?.nameAr || 'تصنيف'} (${item?.key || 'بدون مفتاح'}) — ${place}`
}

async function goToConflict(item: any) {
  if (!item?.id) return
  search.value = ''
  filterMode.value = 'all'

  if (item.parentId) {
    const parent = items.value.find((x: any) => x.id === item.parentId)
    if (parent) {
      selectedParentId.value = parent.id
      await loadChildren(parent.id)
    }
  }

  await nextTick()
  document.getElementById(`category-${item.id}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' })
}

function resetForm() {
  editingId.value = ''
  Object.assign(form, { key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null })
}

function clearSelection() {
  selectedParentId.value = ''
  childItems.value = []
  resetChildForm()
}

function resetChildForm() {
  childEditingId.value = ''
  Object.assign(childForm, { key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: selectedParentId.value || null })
}

async function load() {
  loading.value = true
  try {
    items.value = await $fetch('/api/bff/admin/categories', { query: { _ts: Date.now(), section, rootsOnly: true } }) as any[]
    if (selectedParentId.value) {
      const stillExists = items.value.some((x: any) => x.id === selectedParentId.value && x.hasDetailSections)
      if (stillExists) await loadChildren(selectedParentId.value)
      else clearSelection()
    }
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
  if (!item?.hasDetailSections) {
    toast.error('فعّل التصنيفات الدقيقة لهذا القسم أولًا من التعديل')
    return
  }
  selectedParentId.value = item.id
  resetChildForm()
  loadChildren(item.id)
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
  window?.scrollTo?.({ top: 0, behavior: 'smooth' })
}

function editChild(item: any) {
  childEditingId.value = item.id
  selectedParentId.value = item.parentId || selectedParentId.value
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
  const conflict = parentConflict.value
  if (conflict) {
    toast.error(`يوجد تصنيف مشابه: ${conflict.item?.nameAr || conflict.item?.key}`)
    await goToConflict(conflict.item)
    return
  }
  try {
    const body = { ...form, section, parentId: null }
    const currentId = editingId.value
    if (editingId.value) {
      await $fetch(`/api/bff/admin/categories/${editingId.value}`, { method: 'PUT', body })
      toast.success('تم تحديث التصنيف')
    } else {
      await $fetch('/api/bff/admin/categories', { method: 'POST', body })
      toast.success('تمت إضافة التصنيف')
    }

    const shouldOpenDetails = Boolean(body.hasDetailSections && currentId)
    resetForm()
    await load()

    if (shouldOpenDetails && currentId) {
      selectedParentId.value = currentId
      resetChildForm()
      await loadChildren(currentId)
    }
  } catch (e: any) {
    if (e?.data?.conflict) {
      toast.error(`${e?.data?.message || 'يوجد تصنيف مشابه بالفعل'}: ${e.data.conflict.nameAr || e.data.conflict.key}`)
      await goToConflict(e.data.conflict)
      return
    }
    toast.error(e?.data?.message || e?.message || 'تعذر حفظ التصنيف')
  }
}

async function saveChild() {
  if (!selectedParentId.value) return toast.error('اختر تصنيفًا رئيسيًا أولًا')
  const conflict = childConflict.value
  if (conflict) {
    toast.error(`يوجد تصنيف دقيق مشابه: ${conflict.item?.nameAr || conflict.item?.key}`)
    await goToConflict(conflict.item)
    return
  }
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
    if (e?.data?.conflict) {
      toast.error(`${e?.data?.message || 'يوجد تصنيف دقيق مشابه بالفعل'}: ${e.data.conflict.nameAr || e.data.conflict.key}`)
      await goToConflict(e.data.conflict)
      return
    }
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

<style scoped>
.admin-category-page {
  --soft-border: rgba(var(--border), 0.9);
}
.admin-page-hero {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
  border: 1px solid var(--soft-border);
  background: linear-gradient(135deg, rgba(var(--surface), 0.92), rgba(var(--surface-2), 0.72));
  border-radius: 2rem;
  padding: 1.25rem;
  box-shadow: var(--shadow-soft);
}
.admin-kicker {
  display: inline-flex;
  align-items: center;
  gap: .5rem;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  color: rgb(var(--muted));
  border-radius: 999px;
  padding: .35rem .7rem;
  font-size: .75rem;
  font-weight: 800;
}
.admin-kicker__dot {
  width: .55rem;
  height: .55rem;
  border-radius: 999px;
  background: rgb(var(--primary));
  box-shadow: 0 0 0 5px rgba(var(--primary), .12);
}
.stat-card {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  border-radius: 1.5rem;
  padding: 1rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: rgb(var(--muted));
  box-shadow: var(--shadow-soft);
}
.stat-card strong {
  color: rgb(var(--text));
  font-size: 1.5rem;
}
.stat-card--detail strong { color: rgb(var(--primary)); }
.stat-card--direct strong { color: rgb(var(--text)); }
.category-editor-card {
  position: sticky;
  top: 1rem;
  height: fit-content;
}
.admin-label {
  font-size: .82rem;
  font-weight: 800;
  color: rgb(var(--text));
}
.upload-line {
  border: 1px dashed var(--soft-border);
  border-radius: 1rem;
  background: rgb(var(--surface-2));
  padding: .75rem;
}

.duplicate-alert {
  display: grid;
  gap: .65rem;
  border: 1px solid rgba(245, 158, 11, .45);
  background: rgba(245, 158, 11, .10);
  color: rgb(var(--text));
  border-radius: 1.2rem;
  padding: .9rem 1rem;
  font-size: .86rem;
  line-height: 1.8;
}
.duplicate-alert strong {
  color: rgb(var(--text));
  font-weight: 950;
}
.duplicate-alert span {
  color: rgb(var(--muted));
}
.duplicate-alert__actions {
  display: flex;
  flex-wrap: wrap;
  gap: .5rem;
}
.duplicate-alert--compact {
  margin: .75rem 0;
}
.toggle-box,
.detail-switch {
  display: flex;
  align-items: center;
  gap: .65rem;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.2rem;
  padding: .8rem .9rem;
  cursor: pointer;
}
.toggle-box input,
.detail-switch input {
  width: 1rem;
  height: 1rem;
  accent-color: rgb(var(--primary));
}
.detail-switch {
  align-items: flex-start;
}
.detail-switch__mark {
  width: .75rem;
  height: .75rem;
  margin-top: .25rem;
  border-radius: 999px;
  background: rgb(var(--muted));
}
.detail-switch.is-on {
  border-color: rgba(var(--primary), .45);
  background: rgba(var(--primary), .08);
}
.detail-switch.is-on .detail-switch__mark {
  background: rgb(var(--primary));
  box-shadow: 0 0 0 6px rgba(var(--primary), .14);
}
.detail-switch strong,
.detail-switch small {
  display: block;
}
.detail-switch strong {
  color: rgb(var(--text));
  font-weight: 900;
}
.detail-switch small {
  margin-top: .25rem;
  color: rgb(var(--muted));
  line-height: 1.6;
}
.filter-tabs {
  display: flex;
  flex-wrap: wrap;
  gap: .45rem;
  justify-content: flex-end;
}
.filter-tabs button {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  color: rgb(var(--muted));
  border-radius: 999px;
  padding: .65rem .9rem;
  font-size: .82rem;
  font-weight: 800;
  transition: .2s ease;
}
.filter-tabs button.is-active {
  color: rgb(var(--text));
  border-color: rgba(var(--primary), .5);
  background: rgba(var(--primary), .12);
}
.category-group {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  border-radius: 2rem;
  padding: 1rem;
  box-shadow: var(--shadow-soft);
}
.category-group__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: .25rem .25rem 1rem;
}
.category-group__head h2 {
  font-size: 1.05rem;
  font-weight: 950;
  color: rgb(var(--text));
}
.category-group__head p {
  margin-top: .35rem;
  font-size: .84rem;
  color: rgb(var(--muted));
  line-height: 1.7;
}
.category-group__head span {
  min-width: 2.4rem;
  text-align: center;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 999px;
  padding: .35rem .6rem;
  font-weight: 900;
}
.category-admin-card {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  gap: 1rem;
  align-items: center;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.6rem;
  padding: .9rem;
  transition: .2s ease;
}
.category-admin-card:hover,
.category-admin-card.is-selected {
  border-color: rgba(var(--primary), .45);
  transform: translateY(-1px);
}
.category-admin-card.has-detail {
  background: linear-gradient(135deg, rgba(var(--primary), .1), rgb(var(--surface-2)) 45%);
}
.category-admin-card__media {
  width: 5.25rem;
  height: 5.25rem;
  border-radius: 1.25rem;
  overflow: hidden;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  display: grid;
  place-items: center;
  font-weight: 950;
  cursor: pointer;
}
.category-admin-card__media img,
.child-card__media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.category-admin-card__body h3,
.child-card h4 {
  color: rgb(var(--text));
  font-weight: 950;
}
.category-admin-card__body p,
.child-card p {
  margin-top: .15rem;
  color: rgb(var(--muted));
  font-size: .78rem;
}
.category-admin-card__body small,
.child-card small {
  display: block;
  margin-top: .35rem;
  color: rgb(var(--muted));
  line-height: 1.55;
}
.status-pill {
  border: 1px solid var(--soft-border);
  border-radius: 999px;
  padding: .35rem .6rem;
  font-size: .72rem;
  font-weight: 900;
}
.status-pill.is-active {
  color: #23c483;
  border-color: rgba(35, 196, 131, .35);
  background: rgba(35, 196, 131, .08);
}
.status-pill.is-off {
  color: #f87171;
  border-color: rgba(248, 113, 113, .35);
  background: rgba(248, 113, 113, .08);
}
.category-admin-card__meta {
  display: flex;
  flex-wrap: wrap;
  gap: .45rem;
  margin-top: .75rem;
}
.category-admin-card__meta span {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  color: rgb(var(--muted));
  border-radius: 999px;
  padding: .28rem .55rem;
  font-size: .72rem;
  font-weight: 800;
}
.category-admin-card__actions,
.child-card__actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: .45rem;
}
.empty-panel {
  border: 1px dashed var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.4rem;
  padding: 1.25rem;
  text-align: center;
  color: rgb(var(--muted));
}
.drawer-backdrop {
  position: fixed;
  inset: 0;
  z-index: 80;
  background: rgba(0, 0, 0, .58);
  backdrop-filter: blur(8px);
  display: flex;
  justify-content: flex-start;
}
.detail-drawer {
  width: min(760px, 96vw);
  height: 100%;
  overflow-y: auto;
  background: rgb(var(--bg));
  border-inline-end: 1px solid var(--soft-border);
  box-shadow: 0 30px 80px rgba(0, 0, 0, .28);
  padding: 1.25rem;
}
.detail-drawer__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  border-radius: 1.8rem;
  padding: 1rem;
}
.detail-drawer__head h2 {
  margin-top: .75rem;
  font-size: 1.35rem;
  font-weight: 950;
  color: rgb(var(--text));
}
.detail-drawer__head p {
  margin-top: .35rem;
  color: rgb(var(--muted));
  font-size: .85rem;
  line-height: 1.7;
}
.drawer-close {
  width: 2.5rem;
  height: 2.5rem;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  color: rgb(var(--text));
  border-radius: 999px;
  font-size: 1.4rem;
  line-height: 1;
}
.detail-drawer__content {
  display: grid;
  gap: 1rem;
  margin-top: 1rem;
}
.drawer-form,
.drawer-list {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  border-radius: 1.8rem;
  padding: 1rem;
}
.drawer-form h3,
.drawer-list__head h3 {
  color: rgb(var(--text));
  font-weight: 950;
}
.drawer-list__head {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: .8rem;
}
.drawer-list__head span {
  border: 1px solid var(--soft-border);
  border-radius: 999px;
  padding: .2rem .6rem;
  font-weight: 900;
}
.child-card {
  display: flex;
  align-items: center;
  gap: .8rem;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.4rem;
  padding: .75rem;
  margin-top: .65rem;
}
.child-card__media {
  width: 4rem;
  height: 4rem;
  border: 1px solid var(--soft-border);
  border-radius: 1rem;
  overflow: hidden;
  background: rgb(var(--surface));
  display: grid;
  place-items: center;
  font-weight: 950;
}
.drawer-fade-enter-active,
.drawer-fade-leave-active {
  transition: opacity .2s ease;
}
.drawer-fade-enter-from,
.drawer-fade-leave-to {
  opacity: 0;
}
@media (max-width: 900px) {
  .admin-page-hero,
  .category-group__head {
    flex-direction: column;
    align-items: stretch;
  }
  .category-editor-card {
    position: static;
  }
  .category-admin-card {
    grid-template-columns: auto minmax(0, 1fr);
  }
  .category-admin-card__actions {
    grid-column: 1 / -1;
    justify-content: flex-start;
  }
  .filter-tabs {
    justify-content: flex-start;
  }
}
@media (max-width: 560px) {
  .category-admin-card,
  .child-card {
    grid-template-columns: 1fr;
    flex-direction: column;
    align-items: stretch;
  }
  .category-admin-card__media,
  .child-card__media {
    width: 100%;
    height: 9rem;
  }
  .detail-drawer {
    width: 100vw;
    padding: .85rem;
  }
}
</style>
