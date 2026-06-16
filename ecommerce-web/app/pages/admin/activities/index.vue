<template>
  <div class="w-full admin-activities-page">
    <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-extrabold">حركات الإدارة</h1>
        <p class="text-sm text-[rgb(var(--muted-2))]">سجل واضح لكل العمليات التي تحصل داخل لوحة الإدارة مع فحص سريع للتصنيفات الدقيقة.</p>
      </div>
      <div class="flex flex-wrap gap-2">
        <UiButton variant="secondary" :disabled="loading" @click="loadAll">تحديث</UiButton>
        <UiButton :disabled="healthLoading" @click="loadHealth">فحص التصنيفات</UiButton>
      </div>
    </div>

    <div class="mt-6 grid gap-4 lg:grid-cols-4">
      <UiCard class="lg:col-span-3 activity-card">
        <UiCardHeader>
          <UiCardTitle>آخر الحركات</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="mb-4 grid gap-3 md:grid-cols-3">
            <select v-model="filters.entityType" class="admin-input">
              <option value="">كل الأقسام</option>
              <option value="product">المنتجات</option>
              <option value="brand">البراندات</option>
              <option value="category">التصنيفات</option>
              <option value="problem_category">تصنيفات حل المشكلة</option>
              <option value="package">البكجات</option>
            </select>
            <select v-model="filters.action" class="admin-input">
              <option value="">كل الحركات</option>
              <option value="create">إضافة</option>
              <option value="update">تعديل</option>
              <option value="delete">حذف</option>
              <option value="upload_images">رفع صور</option>
              <option value="upload_logo">رفع شعار</option>
              <option value="feature">تمييز</option>
            </select>
            <UiButton variant="secondary" @click="loadAll">تطبيق الفلتر</UiButton>
          </div>

          <div v-if="loading" class="text-sm text-[rgb(var(--muted-2))]">جاري التحميل...</div>
          <div v-else-if="activities.length === 0" class="rounded-3xl border border-app bg-surface/60 p-6 text-center text-sm text-[rgb(var(--muted-2))]">
            لا توجد حركات مسجلة بعد. ستظهر الحركات الجديدة تلقائياً بعد أي إضافة أو تعديل.
          </div>
          <div v-else class="grid gap-3">
            <div v-for="a in activities" :key="a.id" class="activity-row">
              <div class="flex items-start gap-3">
                <div class="activity-icon">
                  <Icon :name="iconFor(a)" />
                </div>
                <div class="min-w-0 flex-1">
                  <div class="flex flex-wrap items-center gap-2">
                    <b class="truncate">{{ a.title }}</b>
                    <span class="activity-badge">{{ labelFor(a.entityType) }}</span>
                    <span class="activity-badge">{{ actionLabel(a.action) }}</span>
                  </div>
                  <p v-if="a.details" class="mt-1 text-sm text-[rgb(var(--muted-2))]">{{ a.details }}</p>
                  <div class="mt-2 flex flex-wrap gap-3 text-xs text-[rgb(var(--muted-2))]">
                    <span>الأدمن: {{ a.adminEmail || 'غير محدد' }}</span>
                    <span>{{ formatDate(a.createdAtUtc) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </UiCardContent>
      </UiCard>

      <UiCard class="activity-card">
        <UiCardHeader>
          <UiCardTitle>فحص التصنيفات</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-3 text-sm">
            <div class="rounded-3xl border border-app bg-surface/70 p-4">
              <div class="text-[rgb(var(--muted-2))]">عدد المنتجات</div>
              <div class="mt-1 text-2xl font-extrabold">{{ health?.totalProducts ?? '-' }}</div>
            </div>
            <div class="rounded-3xl border border-app bg-surface/70 p-4">
              <div class="text-[rgb(var(--muted-2))]">مشاكل التصنيف</div>
              <div class="mt-1 text-2xl font-extrabold" :class="health?.issuesCount ? 'text-amber-300' : 'text-emerald-300'">{{ health?.issuesCount ?? '-' }}</div>
            </div>
            <p class="text-xs text-[rgb(var(--muted-2))]">إذا ظهر رقم أكبر من صفر، افتح المنتجات الظاهرة تحت وعدّل التصنيف الرئيسي/الدقيق حتى يرجع الفحص صفر.</p>
          </div>
        </UiCardContent>
      </UiCard>
    </div>

    <UiCard v-if="health?.issues?.length" class="mt-5 activity-card">
      <UiCardHeader>
        <UiCardTitle>منتجات تحتاج مراجعة التصنيف</UiCardTitle>
      </UiCardHeader>
      <UiCardContent>
        <div class="grid gap-3">
          <div v-for="p in health.issues" :key="p.id" class="activity-row">
            <div class="flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
              <div>
                <b>{{ p.title }}</b>
                <div class="mt-1 text-xs text-[rgb(var(--muted-2))] keep-ltr">/{{ p.slug }}</div>
                <ul class="mt-2 list-disc pe-5 text-sm text-amber-200">
                  <li v-for="issue in p.issues" :key="issue">{{ issue }}</li>
                </ul>
              </div>
              <NuxtLink :to="`/admin/products/${p.id}`">
                <UiButton size="sm">تعديل المنتج</UiButton>
              </NuxtLink>
            </div>
          </div>
        </div>
      </UiCardContent>
    </UiCard>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

const { listAdminActivities, getCategoryHealth } = useAdminApi()
const loading = ref(false)
const healthLoading = ref(false)
const activities = ref<any[]>([])
const health = ref<any>(null)
const filters = reactive({ entityType: '', action: '' })

function labelFor(v: string) {
  return ({ product: 'منتج', brand: 'براند', category: 'تصنيف', problem_category: 'حل مشكلة', package: 'بكج' } as any)[v] || v
}
function actionLabel(v: string) {
  return ({ create: 'إضافة', update: 'تعديل', delete: 'حذف', upload_images: 'رفع صور', upload_logo: 'رفع شعار', feature: 'تمييز' } as any)[v] || v
}
function iconFor(a: any) {
  if (a.entityType === 'product') return 'mdi:cube-outline'
  if (a.entityType === 'brand') return 'mdi:storefront-outline'
  if (a.entityType === 'package') return 'mdi:package-variant-closed'
  if (a.entityType === 'problem_category') return 'mdi:medical-bag'
  return 'mdi:shape-outline'
}
function formatDate(v: string) {
  try { return new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(v)) } catch { return v }
}

async function loadAll() {
  loading.value = true
  try {
    const res: any = await listAdminActivities({ take: 150, entityType: filters.entityType || undefined, action: filters.action || undefined })
    activities.value = Array.isArray(res?.items) ? res.items : []
  } finally { loading.value = false }
}
async function loadHealth() {
  healthLoading.value = true
  try { health.value = await getCategoryHealth() } finally { healthLoading.value = false }
}

onMounted(async () => {
  await Promise.all([loadAll(), loadHealth()])
})
</script>

<style scoped>
.activity-card{ border-radius: 28px; border: 1px solid rgba(var(--border), .95); background: rgba(var(--surface-rgb), .86); }
.admin-input{ min-height: 46px; border-radius: 16px; border: 1px solid rgba(var(--border), .95); background: rgba(var(--surface-rgb), .75); padding: 0 .9rem; outline: none; }
.activity-row{ border:1px solid rgba(var(--border), .9); background: rgba(var(--surface-2-rgb), .55); border-radius: 22px; padding: 1rem; }
.activity-icon{ width: 42px; height: 42px; border-radius: 16px; display:grid; place-items:center; background: rgba(var(--primary), .18); color: rgb(var(--primary)); flex-shrink: 0; }
.activity-badge{ border:1px solid rgba(var(--border), .8); border-radius: 999px; padding: .18rem .55rem; font-size: .75rem; color: rgb(var(--muted-2)); }
</style>
