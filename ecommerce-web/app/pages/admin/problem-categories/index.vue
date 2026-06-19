<template>
  <div class="admin-problem-category-page space-y-6">
    <section class="admin-page-hero">
      <div class="min-w-0">
        <div class="admin-kicker rtl-text">
          <span class="admin-kicker__dot" />
          إدارة حلول المشاكل
        </div>
        <h1 class="mt-3 text-2xl font-black tracking-tight text-[rgb(var(--text))] sm:text-3xl rtl-text">
          تصنيفات حل المشاكل والأقسام الدقيقة
        </h1>
        <p class="mt-2 max-w-4xl text-sm leading-7 text-[rgb(var(--muted))] rtl-text">
          نظّم تصنيفات حلول المشاكل بطريقة واضحة: التصنيف المباشر يفتح منتجاته فوراً، والتصنيف الذي يحتوي أقساماً دقيقة يظهر كصفحة ثانية منظمة مرتبطة بالمنتجات.
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <UiButton variant="secondary" @click="load">تحديث</UiButton>
      </div>
    </section>

    <section class="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <div class="stat-card">
        <span>كل التصنيفات</span>
        <strong>{{ items.length }}</strong>
      </div>
      <div class="stat-card stat-card--detail">
        <span>تحتوي أقسام دقيقة</span>
        <strong>{{ detailedItems.length }}</strong>
      </div>
      <div class="stat-card stat-card--direct">
        <span>تصنيفات مباشرة</span>
        <strong>{{ directItems.length }}</strong>
      </div>
      <div class="stat-card stat-card--children">
        <span>الأقسام الدقيقة الكلية</span>
        <strong>{{ totalChildren }}</strong>
      </div>
    </section>

    <section class="grid gap-6 xl:grid-cols-[420px_minmax(0,1fr)]">
      <UiCard class="category-editor-card">
        <UiCardHeader>
          <UiCardTitle>{{ editingId ? 'تعديل تصنيف حل مشكلة' : 'إضافة تصنيف حل مشكلة' }}</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-4">
            <div class="info-box rtl-text">
              اكتب بيانات التصنيف الرئيسي. إذا يحتاج أقساماً دقيقة، فعّل الخيار ثم احفظه، وبعدها افتح صفحة الأقسام الدقيقة الخاصة به.
            </div>

            <div class="grid gap-2">
              <label class="admin-label">الاسم بالعربي</label>
              <UiInput v-model="form.nameAr" placeholder="مثال: مشاكل الشعر" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الاسم بالإنكليزي</label>
              <UiInput v-model="form.nameEn" placeholder="Hair Problems" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">المفتاح</label>
              <UiInput v-model="form.key" placeholder="hair-care" dir="ltr" />
            </div>
            <div class="grid gap-2">
              <label class="admin-label">الوصف</label>
              <UiInput v-model="form.descriptionAr" placeholder="وصف مختصر يظهر للزبون" />
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
                <input type="file" accept="image/*" @change="onPickFile($event)" class="block w-full text-sm" />
              </div>
              <UiInput v-model="form.imageUrl" placeholder="https://..." dir="ltr" />
            </div>

            <label class="detail-switch" :class="form.hasDetailSections ? 'is-on' : ''">
              <input v-model="form.hasDetailSections" type="checkbox" />
              <span class="detail-switch__mark" />
              <span>
                <strong>يحتاج صفحة ثانية للأقسام الدقيقة</strong>
                <small>فعّل هذا الخيار إذا تريد داخل هذا التصنيف أقساماً أصغر، وكل قسم دقيق يرتبط بمنتجاته.</small>
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
              <UiInput v-model="search" placeholder="ابحث باسم التصنيف أو المفتاح..." />
              <div class="filter-tabs">
                <button :class="filterMode === 'all' ? 'is-active' : ''" @click="filterMode = 'all'">الكل</button>
                <button :class="filterMode === 'detail' ? 'is-active' : ''" @click="filterMode = 'detail'">عندها أقسام دقيقة</button>
                <button :class="filterMode === 'direct' ? 'is-active' : ''" @click="filterMode = 'direct'">مباشرة فقط</button>
                <button :class="filterMode === 'active' ? 'is-active' : ''" @click="filterMode = 'active'">مفعلة</button>
                <button :class="filterMode === 'inactive' ? 'is-active' : ''" @click="filterMode = 'inactive'">غير مفعلة</button>
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
                <p>هذه التصنيفات تفتح صفحة ثانية فيها أقسام دقيقة، ومنها يتم فصل المنتجات بدقة.</p>
              </div>
              <span>{{ visibleDetailedItems.length }}</span>
            </div>

            <div v-if="visibleDetailedItems.length" class="grid gap-3">
              <article v-for="item in visibleDetailedItems" :key="item.id" class="category-admin-card has-detail">
                <div class="category-admin-card__media">
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
                    <span>صفحة ثانية</span>
                    <span>{{ item.childCount || 0 }} قسم دقيق</span>
                    <span>ترتيب {{ item.sortOrder || 0 }}</span>
                  </div>
                </div>

                <div class="category-admin-card__actions">
                  <NuxtLink :to="`/admin/problem-categories/${item.id}/details`" class="action-link action-link--primary">إدارة الأقسام الدقيقة</NuxtLink>
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
                <p>هذه التصنيفات تعرض منتجاتها مباشرة بدون صفحة أقسام دقيقة.</p>
              </div>
              <span>{{ visibleDirectItems.length }}</span>
            </div>

            <div v-if="visibleDirectItems.length" class="grid gap-3">
              <article v-for="item in visibleDirectItems" :key="item.id" class="category-admin-card is-direct">
                <div class="category-admin-card__media">
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
                    <span>بدون صفحة ثانية</span>
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
const search = ref('')
const filterMode = ref<'all' | 'detail' | 'direct' | 'active' | 'inactive'>('all')

const form = reactive({ key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null as string | null })

const sortedItems = computed(() => [...items.value].sort((a: any, b: any) => Number(a.sortOrder || 0) - Number(b.sortOrder || 0) || String(a.nameAr || '').localeCompare(String(b.nameAr || ''), 'ar')))
const detailedItems = computed(() => sortedItems.value.filter((x: any) => Boolean(x.hasDetailSections)))
const directItems = computed(() => sortedItems.value.filter((x: any) => !x.hasDetailSections))
const totalChildren = computed(() => items.value.reduce((sum: number, item: any) => sum + Number(item.childCount || 0), 0))

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

const visibleDetailedItems = computed(() => detailedItems.value.filter((item: any) => matchesSearch(item) && matchesFilter(item)))
const visibleDirectItems = computed(() => directItems.value.filter((item: any) => matchesSearch(item) && matchesFilter(item)))
const showDetailSection = computed(() => filterMode.value === 'all' || filterMode.value === 'detail' || filterMode.value === 'active' || filterMode.value === 'inactive')
const showDirectSection = computed(() => filterMode.value === 'all' || filterMode.value === 'direct' || filterMode.value === 'active' || filterMode.value === 'inactive')

function resetForm() {
  editingId.value = ''
  Object.assign(form, { key: '', nameAr: '', nameEn: '', descriptionAr: '', descriptionEn: '', imageUrl: '', sortOrder: 0, isActive: true, section, hasDetailSections: false, parentId: null })
}

async function load() {
  loading.value = true
  try {
    items.value = await $fetch('/api/bff/admin/categories', { query: { _ts: Date.now(), section, rootsOnly: true } }) as any[]
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
    hasDetailSections: Boolean(item.hasDetailSections ?? false),
    parentId: null,
  })
  window?.scrollTo?.({ top: 0, behavior: 'smooth' })
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

async function remove(id: string) {
  if (!confirm('حذف هذا التصنيف؟ إذا يحتوي أقساماً دقيقة يجب حذفها أولاً.')) return
  try {
    await $fetch(`/api/bff/admin/categories/${id}`, { method: 'DELETE' })
    toast.success('تم الحذف')
    if (editingId.value === id) resetForm()
    await load()
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

<style scoped>
.admin-problem-category-page {
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
.stat-card--detail strong,
.stat-card--children strong { color: rgb(var(--primary)); }
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
.category-admin-card:hover {
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
}
.category-admin-card__media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.category-admin-card__body h3 {
  color: rgb(var(--text));
  font-weight: 950;
}
.category-admin-card__body p {
  margin-top: .15rem;
  color: rgb(var(--muted));
  font-size: .78rem;
}
.category-admin-card__body small {
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
.category-admin-card__actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: .45rem;
}
.action-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 1rem;
  border: 1px solid var(--soft-border);
  padding: .55rem .85rem;
  font-size: .82rem;
  font-weight: 900;
  transition: .2s ease;
}
.action-link--primary {
  color: rgb(var(--text));
  border-color: rgba(var(--primary), .45);
  background: rgba(var(--primary), .12);
}
.action-link:hover {
  transform: translateY(-1px);
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
  .category-admin-card {
    grid-template-columns: 1fr;
  }
  .category-admin-card__media {
    width: 100%;
    height: 9rem;
  }
}
</style>
