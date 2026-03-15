<template>
  <div class="grid gap-6">
    <div>
      <h1 class="text-3xl font-black rtl-text text-[rgb(var(--text))]">
        {{ t('admin.couponsLabel') }}
      </h1>
      <p class="mt-1 text-sm text-muted rtl-text">
        إدارة الكوبونات بشكل واضح ودقيق مع شرح مختصر لكل قيمة.
      </p>
    </div>

    <div class="grid gap-5 xl:grid-cols-[520px_1fr]">
      <div class="rounded-[28px] border border-app bg-surface p-5 shadow-sm">
        <div class="mb-4 rounded-2xl border border-app bg-surface-2 p-4">
          <div class="text-sm font-bold rtl-text">قواعد الكوبون</div>
          <div class="mt-2 grid gap-2 text-sm text-muted rtl-text">
            <div>• اسم واضح للكوبون حتى تعرفه بسرعة داخل الإدارة.</div>
            <div>• كود الكوبون لا يمكن تكراره.</div>
            <div>• الكوبون لا يُعاد استخدامه من نفس الجهاز حتى لو تم إنشاء حساب جديد.</div>
            <div>• يمكنك تحديد مدة التفعيل والانتهاء، وبعد انتهائها يتوقف الكوبون تلقائيًا.</div>
          </div>
        </div>

        <div class="grid gap-4">
          <UiInput
            v-model="form.code"
            label="كود الكوبون"
            hint="هذا هو الكود الذي يكتبه الزبون داخل السلة، مثل: SAVE10 أو EYE30"
            placeholder="مثال: SAVE10"
          />

          <UiInput
            v-model="form.title"
            label="اسم الكوبون"
            hint="اسم داخلي واضح يظهر لك داخل الإدارة ويساعدك تميّز الكوبون بسرعة"
            placeholder="مثال: خصم منتجات العناية بالعين"
          />

          <div class="grid gap-4 md:grid-cols-2">
            <UiInput
              v-model.number="form.discountPercent"
              type="number"
              min="0"
              max="100"
              label="نسبة الخصم"
              hint="أدخل نسبة الخصم التي ستُخصم من الطلب. مثال: 10 يعني خصم 10%"
              placeholder="مثال: 10"
            />

            <UiInput
              v-model.number="form.maxUses"
              type="number"
              min="0"
              label="أقصى عدد للاستخدام"
              hint="هذا الرقم يحدد كم مرة يمكن استخدام الكوبون إجمالًا. إذا كان 0 يصبح بدون حد"
              placeholder="مثال: 100"
            />
          </div>

          <div class="grid gap-4 md:grid-cols-2">
            <div class="rounded-2xl border border-app bg-surface-2 p-4">
              <label class="mb-2 block text-sm font-bold rtl-text text-[rgb(var(--text))]">
                بداية التفعيل
              </label>
              <input
                v-model="form.startsAtUtc"
                type="datetime-local"
                class="w-full rounded-2xl border border-app bg-surface px-4 py-3 text-sm text-[rgb(var(--text))] outline-none transition focus:ring-0"
              />
              <p class="mt-2 text-xs text-muted rtl-text">
                اختياري. إذا تركته فارغًا يبدأ الكوبون مباشرة.
              </p>
            </div>

            <div class="rounded-2xl border border-app bg-surface-2 p-4">
              <label class="mb-2 block text-sm font-bold rtl-text text-[rgb(var(--text))]">
                نهاية التفعيل
              </label>
              <input
                v-model="form.endsAtUtc"
                type="datetime-local"
                class="w-full rounded-2xl border border-app bg-surface px-4 py-3 text-sm text-[rgb(var(--text))] outline-none transition focus:ring-0"
              />
              <p class="mt-2 text-xs text-muted rtl-text">
                اختياري. بعد هذا التاريخ يصبح الكوبون غير صالح تلقائيًا.
              </p>
            </div>
          </div>

          <label
            class="flex items-center gap-2 rounded-2xl border border-app bg-surface-2 px-4 py-3 text-sm text-[rgb(var(--text))]"
          >
            <input v-model="form.isActive" type="checkbox" />
            <span class="rtl-text font-semibold">الكوبون مفعّل حاليًا</span>
          </label>

          <div class="rounded-2xl border border-app bg-surface-2 p-4 text-sm text-muted rtl-text">
            يتم منع إعادة استخدام الكوبون من نفس الجهاز حتى لو تم إنشاء حساب جديد.
          </div>

          <div class="flex flex-wrap gap-3">
            <UiButton @click="save">
              {{ editingId ? 'حفظ التعديلات' : 'إنشاء الكوبون' }}
            </UiButton>
            <UiButton variant="ghost" @click="resetForm">إلغاء</UiButton>
          </div>
        </div>
      </div>

      <div class="rounded-[28px] border border-app bg-surface p-5 shadow-sm">
        <div class="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <div class="text-sm font-bold rtl-text text-[rgb(var(--text))]">قائمة الكوبونات</div>
            <div class="mt-1 text-sm text-muted rtl-text">
              {{ filteredItems.length }} من أصل {{ items.length }} كوبون
            </div>
          </div>

          <input
            v-model="search"
            class="w-full rounded-2xl border border-app bg-surface px-4 py-3 text-sm text-[rgb(var(--text))] outline-none transition focus:ring-0 md:w-[280px]"
            placeholder="بحث بالكود أو الاسم"
          />
        </div>

        <div
          v-if="!filteredItems.length"
          class="rounded-3xl border border-app bg-surface-2 p-8 text-center"
        >
          <div class="text-lg font-bold rtl-text text-[rgb(var(--text))]">
            لا توجد كوبونات مطابقة
          </div>
          <div class="mt-2 text-sm text-muted rtl-text">
            جرّب تغيير البحث أو أنشئ كوبون جديد من النموذج.
          </div>
        </div>

        <div v-else class="grid gap-3">
          <div
            v-for="item in filteredItems"
            :key="item.id"
            class="rounded-3xl border border-app bg-surface-2 p-4 transition hover:shadow-sm"
          >
            <div class="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
              <div class="min-w-0 flex-1">
                <div class="flex flex-wrap items-center gap-2">
                  <div class="font-black keep-ltr text-lg text-[rgb(var(--text))]">
                    {{ item.code }}
                  </div>

                  <span
                    class="rounded-full px-3 py-1 text-xs font-bold"
                    :class="
                      item.isActive
                        ? 'bg-emerald-500/15 text-emerald-600 dark:text-emerald-300'
                        : 'bg-red-500/15 text-red-600 dark:text-red-300'
                    "
                  >
                    {{ item.isActive ? 'مفعّل' : 'متوقف' }}
                  </span>
                </div>

                <div class="mt-1 rtl-text text-base font-bold text-[rgb(var(--text))]">
                  {{ item.title }}
                </div>

                <div class="mt-3 grid gap-2 sm:grid-cols-2 xl:grid-cols-3">
                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 text-sm rtl-text">
                    <div class="text-muted">نسبة الخصم</div>
                    <div class="mt-1 font-bold text-[rgb(var(--text))]">
                      {{ item.discountPercent }}%
                    </div>
                  </div>

                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 text-sm rtl-text">
                    <div class="text-muted">أقصى عدد للاستخدام</div>
                    <div class="mt-1 font-bold text-[rgb(var(--text))]">
                      {{ item.maxUses ?? 'غير محدود' }}
                    </div>
                  </div>

                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 text-sm rtl-text">
                    <div class="text-muted">عدد الاستخدامات</div>
                    <div class="mt-1 font-bold text-[rgb(var(--text))]">
                      {{ item.usedCount ?? 0 }}
                    </div>
                  </div>
                </div>

                <div class="mt-3 grid gap-2 sm:grid-cols-2 text-sm">
                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 rtl-text">
                    <div class="text-muted">بداية التفعيل</div>
                    <div class="mt-1 font-semibold text-[rgb(var(--text))]">
                      {{ item.startsAtUtc ? fmtDate(item.startsAtUtc) : 'مباشر' }}
                    </div>
                  </div>

                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 rtl-text">
                    <div class="text-muted">نهاية التفعيل</div>
                    <div class="mt-1 font-semibold text-[rgb(var(--text))]">
                      {{ item.endsAtUtc ? fmtDate(item.endsAtUtc) : 'بدون نهاية' }}
                    </div>
                  </div>
                </div>
              </div>

              <div class="flex shrink-0 gap-2 self-start">
                <UiButton size="sm" variant="ghost" @click="editItem(item)">تعديل</UiButton>
                <UiButton size="sm" variant="ghost" @click="removeItem(item.id)">حذف</UiButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

import UiButton from '~/components/ui/UiButton.vue'
import UiInput from '~/components/ui/UiInput.vue'

const { t } = useI18n()
const api = useAdminApi()

const items = ref<any[]>([])
const editingId = ref<string>('')
const search = ref('')

const form = reactive({
  code: '',
  title: '',
  discountPercent: 0,
  fixedDiscountIqd: 0,
  minimumOrderIqd: 0,
  maxUses: 0 as any,
  isActive: true,
  startsAtUtc: '',
  endsAtUtc: ''
})

const filteredItems = computed(() => {
  const s = search.value.trim().toLowerCase()
  if (!s) return items.value

  return items.value.filter(
    (x: any) =>
      String(x.code || '')
        .toLowerCase()
        .includes(s) ||
      String(x.title || '')
        .toLowerCase()
        .includes(s)
  )
})

async function load() {
  items.value = (await api.get('/admin/coupons')) as any[]
}

function resetForm() {
  editingId.value = ''
  Object.assign(form, {
    code: '',
    title: '',
    discountPercent: 0,
    fixedDiscountIqd: 0,
    minimumOrderIqd: 0,
    maxUses: 0,
    isActive: true,
    startsAtUtc: '',
    endsAtUtc: ''
  })
}

function toIsoOrNull(v: string) {
  return v ? new Date(v).toISOString() : null
}

function fromIso(v?: string) {
  if (!v) return ''
  const d = new Date(v)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function save() {
  const payload = {
    ...form,
    fixedDiscountIqd: 0,
    minimumOrderIqd: 0,
    maxUses: Number(form.maxUses) || null,
    startsAtUtc: toIsoOrNull(form.startsAtUtc),
    endsAtUtc: toIsoOrNull(form.endsAtUtc)
  }

  if (editingId.value) {
    await api.put(`/admin/coupons/${editingId.value}`, payload)
  } else {
    await api.post('/admin/coupons', payload)
  }

  await load()
  resetForm()
}

function editItem(item: any) {
  editingId.value = item.id
  Object.assign(form, {
    code: item.code,
    title: item.title,
    discountPercent: item.discountPercent,
    fixedDiscountIqd: 0,
    minimumOrderIqd: 0,
    maxUses: item.maxUses || 0,
    isActive: item.isActive,
    startsAtUtc: fromIso(item.startsAtUtc),
    endsAtUtc: fromIso(item.endsAtUtc)
  })
}

async function removeItem(id: string) {
  await api.del(`/admin/coupons/${id}`)
  await load()
  if (editingId.value === id) resetForm()
}

function fmtDate(v?: string) {
  return v ? new Date(v).toLocaleString('ar-IQ') : ''
}

onMounted(load)
</script>