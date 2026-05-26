<template>
  <div class="problem-details-page space-y-6">
    <section class="admin-page-hero">
      <div class="min-w-0">
        <NuxtLink to="/admin/problem-categories" class="back-link">← رجوع إلى تصنيفات حل المشاكل</NuxtLink>
        <div class="admin-kicker rtl-text mt-3">
          <span class="admin-kicker__dot" />
          صفحة الأقسام الدقيقة
        </div>
        <h1 class="mt-3 text-2xl font-black tracking-tight text-[rgb(var(--text))] sm:text-3xl rtl-text">
          {{ parentItem?.nameAr || 'التصنيف المحدد' }}
        </h1>
        <p class="mt-2 max-w-4xl text-sm leading-7 text-[rgb(var(--muted))] rtl-text">
          أضف وأدر الأقسام الدقيقة التابعة لهذا التصنيف. كل قسم دقيق يظهر للزبون في صفحة ثانية، ويمكن ربط المنتجات به مباشرة من نموذج المنتج.
        </p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <UiButton variant="secondary" @click="loadAll">تحديث</UiButton>
      </div>
    </section>

    <section class="grid gap-3 sm:grid-cols-3">
      <div class="stat-card">
        <span>الأقسام الدقيقة</span>
        <strong>{{ childItems.length }}</strong>
      </div>
      <div class="stat-card stat-card--active">
        <span>مفعلة</span>
        <strong>{{ activeCount }}</strong>
      </div>
      <div class="stat-card stat-card--off">
        <span>غير مفعلة</span>
        <strong>{{ inactiveCount }}</strong>
      </div>
    </section>

    <section class="grid gap-6 xl:grid-cols-[420px_minmax(0,1fr)]">
      <UiCard class="category-editor-card">
        <UiCardHeader>
          <UiCardTitle>{{ childEditingId ? 'تعديل قسم دقيق' : 'إضافة قسم دقيق' }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-4">
            <div class="info-box rtl-text">
              هذا النموذج يضيف أقساماً دقيقة داخل التصنيف الرئيسي المحدد فقط. لا يمكن إنشاء قسم دقيق بدون ارتباطه بهذا التصنيف.
            </div>

            <div class="grid gap-2">
              <label class="admin-label">الاسم بالعربي</label>
              <UiInput v-model="childForm.nameAr" placeholder="مثال: تساقط الشعر" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الاسم بالإنكليزي</label>
              <UiInput v-model="childForm.nameEn" placeholder="Hair Loss" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">المفتاح</label>
              <UiInput v-model="childForm.key" placeholder="hair-loss" dir="ltr" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الوصف</label>
              <UiInput v-model="childForm.descriptionAr" placeholder="وصف مختصر لهذا القسم" />
            </div>
            <div class="grid gap-2 sm:grid-cols-2">
              <div class="grid gap-2">
                <label class="admin-label">الترتيب</label>
                <UiInput v-model.number="childForm.sortOrder" type="number" min="0" step="1" />
              </div>
              <label class="toggle-box mt-7">
                <input v-model="childForm.isActive" type="checkbox" />
                <span>فعال</span>
              </label>
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الصورة</label>
              <div class="upload-line">
                <input type="file" accept="image/*" @change="onPickFile($event)" class="block w-full text-sm" />
              </div>
              <UiInput v-model="childForm.imageUrl" placeholder="https://..." dir="ltr" />
            </div>

            <div class="flex flex-wrap gap-2 pt-2">
              <UiButton @click="saveChild">{{ childEditingId ? 'حفظ التعديل' : 'إضافة القسم الدقيق' }}</UiButton>
              <UiButton variant="ghost" @click="resetChildForm">تفريغ النموذج</UiButton>
            </div>
          </div>
        </UiCardContent>
      </UiCard>

      <div class="space-y-5 min-w-0">
        <UiCard>
          <UiCardContent>
            <div class="grid gap-3 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-center">
              <UiInput v-model="search" placeholder="ابحث داخل الأقسام الدقيقة..." />
              <div class="filter-tabs">
                <button :class="filterMode === 'all' ? 'is-active' : ''" @click="filterMode = 'all'">الكل</button>
                <button :class="filterMode === 'active' ? 'is-active' : ''" @click="filterMode = 'active'">مفعلة</button>
                <button :class="filterMode === 'inactive' ? 'is-active' : ''" @click="filterMode = 'inactive'">غير مفعلة</button>
              </div>
            </div>
          </UiCardContent>
        </UiCard>

        <section class="details-group">
          <div class="details-group__head">
            <div>
              <h2>الأقسام الدقيقة المسجلة</h2>
              <p>هذه هي الأقسام التي تظهر داخل صفحة {{ parentItem?.nameAr || 'التصنيف' }} وفي اختيار المنتج.</p>
            </div>
            <span>{{ visibleChildren.length }}</span>
          </div>

          <div v-if="loading" class="empty-panel">جاري التحميل...</div>
          <div v-else-if="visibleChildren.length === 0" class="empty-panel">لا توجد أقسام دقيقة مطابقة.</div>
          <div v-else class="grid gap-3 md:grid-cols-2">
            <article v-for="item in visibleChildren" :key="item.id" class="child-card">
              <div class="child-card__media">
                <img v-if="item.imageUrl" :src="buildAssetUrl(item.imageUrl)" alt="" />
                <span v-else>{{ item.nameAr?.slice(0,1) }}</span>
              </div>
              <div class="child-card__body">
                <div class="flex items-start justify-between gap-3">
                  <div class="min-w-0">
                    <h3>{{ item.nameAr }}</h3>
                    <p class="keep-ltr">{{ item.key }}</p>
                  </div>
                  <span class="status-pill" :class="item.isActive ? 'is-active' : 'is-off'">{{ item.isActive ? 'فعال' : 'غير فعال' }}</span>
                </div>
                <small>{{ item.descriptionAr || 'بدون وصف' }}</small>
                <div class="child-card__meta">
                  <span>ترتيب {{ item.sortOrder || 0 }}</span>
                </div>
                <div class="child-card__actions">
                  <UiButton variant="secondary" @click="editChild(item)">تعديل</UiButton>
                  <UiButton variant="danger" @click="remove(item.id)">حذف</UiButton>
                </div>
              </div>
            </article>
          </div>
        </section>
      </div>
    </section>
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

const route = useRoute()
const toast = useToast()
const { buildAssetUrl } = useApi()
const section = 'problem'
const parentId = computed(() => String(route.params.id || ''))
const parentItem = ref<any | null>(null)
const childItems = ref<any[]>([])
const loading = ref(false)
const childEditingId = ref<string>('')
const search = ref('')
const filterMode = ref<'all' | 'active' | 'inactive'>('all')

const childForm = reactive({ key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: parentId.value || null as string | null })

const activeCount = computed(() => childItems.value.filter((x: any) => Boolean(x.isActive)).length)
const inactiveCount = computed(() => childItems.value.length - activeCount.value)
const sortedChildren = computed(() => [...childItems.value].sort((a: any, b: any) => Number(a.sortOrder || 0) - Number(b.sortOrder || 0) || String(a.nameAr || '').localeCompare(String(b.nameAr || ''), 'ar')))

function matchesSearch(item: any) {
  const q = search.value.trim().toLowerCase()
  if (!q) return true
  return [item.nameAr, item.nameEn, item.key, item.descriptionAr].some((v) => String(v || '').toLowerCase().includes(q))
}
function matchesFilter(item: any) {
  if (filterMode.value === 'active') return Boolean(item.isActive)
  if (filterMode.value === 'inactive') return !Boolean(item.isActive)
  return true
}
const visibleChildren = computed(() => sortedChildren.value.filter((item: any) => matchesSearch(item) && matchesFilter(item)))

function resetChildForm() {
  childEditingId.value = ''
  Object.assign(childForm, { key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: parentId.value || null })
}

async function loadParent() {
  const roots = await $fetch('/api/bff/admin/categories', { query: { _ts: Date.now(), section, rootsOnly: true } }) as any[]
  parentItem.value = roots.find((x: any) => String(x.id) === parentId.value) || null
}

async function loadChildren() {
  if (!parentId.value) return
  childItems.value = await $fetch('/api/bff/admin/categories', { query: { _ts: Date.now(), section, parentId: parentId.value } }) as any[]
}

async function loadAll() {
  loading.value = true
  try {
    await Promise.all([loadParent(), loadChildren()])
  } catch {
    parentItem.value = null
    childItems.value = []
  } finally {
    loading.value = false
  }
}

function editChild(item: any) {
  childEditingId.value = item.id
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
    parentId: parentId.value,
  })
  window?.scrollTo?.({ top: 0, behavior: 'smooth' })
}

async function saveChild() {
  if (!parentId.value) return toast.error('لم يتم تحديد التصنيف الرئيسي')
  try {
    const body = { ...childForm, section, parentId: parentId.value, hasDetailSections: false }
    if (childEditingId.value) {
      await $fetch(`/api/bff/admin/categories/${childEditingId.value}`, { method: 'PUT', body })
      toast.success('تم تحديث القسم الدقيق')
    } else {
      await $fetch('/api/bff/admin/categories', { method: 'POST', body })
      toast.success('تمت إضافة القسم الدقيق')
    }
    resetChildForm()
    await loadAll()
  } catch (e: any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حفظ القسم الدقيق')
  }
}

async function remove(id: string) {
  if (!confirm('حذف هذا القسم الدقيق؟')) return
  try {
    await $fetch(`/api/bff/admin/categories/${id}`, { method: 'DELETE' })
    toast.success('تم الحذف')
    if (childEditingId.value === id) resetChildForm()
    await loadAll()
  } catch (e: any) {
    toast.error(e?.data?.message || 'تعذر الحذف')
  }
}

async function onPickFile(e: Event) {
  const file = (e.target as HTMLInputElement)?.files?.[0]
  if (!file) return
  try {
    const fd = new FormData()
    fd.append('file', file)
    const res: any = await $fetch('/api/bff/admin/categories/upload', { method: 'POST', body: fd })
    childForm.imageUrl = res?.url || ''
    toast.success('تم رفع الصورة')
  } catch {
    toast.error('تعذر رفع الصورة')
  } finally {
    ;(e.target as HTMLInputElement).value = ''
  }
}

onMounted(loadAll)
</script>

<style scoped>
.problem-details-page {
  --soft-border: rgba(var(--border), 0.9);
}
.admin-page-hero {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
  border: 1px solid var(--soft-border);
  background: linear-gradient(135deg, rgba(var(--surface), 0.94), rgba(var(--surface-2), 0.74));
  border-radius: 2rem;
  padding: 1.25rem;
  box-shadow: var(--shadow-soft);
}
.back-link {
  display: inline-flex;
  align-items: center;
  border: 1px solid var(--soft-border);
  border-radius: 999px;
  padding: .45rem .8rem;
  color: rgb(var(--muted));
  background: rgb(var(--surface));
  font-size: .82rem;
  font-weight: 900;
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
.info-box {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.25rem;
  padding: .9rem;
  color: rgb(var(--muted));
  font-size: .86rem;
  line-height: 1.8;
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
.stat-card--active strong { color: #23c483; }
.stat-card--off strong { color: #f87171; }
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
.toggle-box {
  display: flex;
  align-items: center;
  gap: .65rem;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.2rem;
  padding: .8rem .9rem;
  cursor: pointer;
}
.toggle-box input {
  width: 1rem;
  height: 1rem;
  accent-color: rgb(var(--primary));
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
.details-group {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  border-radius: 2rem;
  padding: 1rem;
  box-shadow: var(--shadow-soft);
}
.details-group__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: .25rem .25rem 1rem;
}
.details-group__head h2 {
  font-size: 1.05rem;
  font-weight: 950;
  color: rgb(var(--text));
}
.details-group__head p {
  margin-top: .35rem;
  font-size: .84rem;
  color: rgb(var(--muted));
  line-height: 1.7;
}
.details-group__head span {
  min-width: 2.4rem;
  text-align: center;
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 999px;
  padding: .35rem .6rem;
  font-weight: 900;
}
.child-card {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.6rem;
  overflow: hidden;
  transition: .2s ease;
}
.child-card:hover {
  border-color: rgba(var(--primary), .45);
  transform: translateY(-1px);
}
.child-card__media {
  height: 11rem;
  background: rgb(var(--surface));
  display: grid;
  place-items: center;
  font-weight: 950;
  font-size: 2rem;
  color: rgb(var(--text));
}
.child-card__media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.child-card__body {
  padding: 1rem;
}
.child-card__body h3 {
  color: rgb(var(--text));
  font-weight: 950;
}
.child-card__body p {
  margin-top: .15rem;
  color: rgb(var(--muted));
  font-size: .78rem;
}
.child-card__body small {
  display: block;
  margin-top: .5rem;
  color: rgb(var(--muted));
  line-height: 1.6;
}
.child-card__meta {
  display: flex;
  gap: .45rem;
  margin-top: .75rem;
}
.child-card__meta span {
  border: 1px solid var(--soft-border);
  background: rgb(var(--surface));
  color: rgb(var(--muted));
  border-radius: 999px;
  padding: .28rem .55rem;
  font-size: .72rem;
  font-weight: 800;
}
.child-card__actions {
  display: flex;
  flex-wrap: wrap;
  gap: .45rem;
  margin-top: 1rem;
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
.empty-panel {
  border: 1px dashed var(--soft-border);
  background: rgb(var(--surface-2));
  border-radius: 1.4rem;
  padding: 1.25rem;
  text-align: center;
  color: rgb(var(--muted));
}
@media (max-width: 900px) {
  .admin-page-hero,
  .details-group__head {
    flex-direction: column;
    align-items: stretch;
  }
  .category-editor-card {
    position: static;
  }
  .filter-tabs {
    justify-content: flex-start;
  }
}
</style>
