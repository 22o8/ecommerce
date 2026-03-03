<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3 flex-wrap">
      <div>
        <h1 class="text-2xl font-extrabold text-zinc-900 dark:text-zinc-100">الإعلانات والثيمات</h1>
        <p class="text-sm text-zinc-600 dark:text-zinc-400">تحكم بالإعلانات والخلفيات والتأثيرات التي تظهر لكل الزبائن.</p>
      </div>
      <button
        class="px-4 py-2 rounded-2xl bg-zinc-900 text-white dark:bg-white dark:text-zinc-900 hover:opacity-90 transition"
        @click="save"
        :disabled="saving"
      >
        {{ saving ? 'جارِ الحفظ...' : 'حفظ' }}
      </button>
    </div>

    <div class="grid gap-6 lg:grid-cols-2">
      <!-- Themes -->
      <div class="rounded-3xl border border-zinc-200/70 dark:border-white/10 bg-white dark:bg-zinc-950/60 p-5">
        <h2 class="font-bold text-zinc-900 dark:text-zinc-100">الثيمات</h2>
        <p class="text-sm text-zinc-600 dark:text-zinc-400 mt-1">تقدر تفعل أكثر من ثيم بنفس الوقت.</p>

        <div class="mt-4 grid sm:grid-cols-2 gap-3">
          <label v-for="t in themeOptions" :key="t.key" class="group cursor-pointer rounded-2xl p-4 border border-zinc-200/70 dark:border-white/10 bg-zinc-50/70 dark:bg-white/5 hover:shadow-md transition">
            <div class="flex items-center gap-3">
              <input type="checkbox" class="h-5 w-5" v-model="draft.themes" :value="t.key" />
              <div>
                <div class="font-semibold text-zinc-900 dark:text-zinc-100">{{ t.label }}</div>
                <div class="text-xs text-zinc-600 dark:text-zinc-400">{{ t.hint }}</div>
              </div>
            </div>
          </label>
        </div>
      </div>

      <!-- Effects -->
      <div class="rounded-3xl border border-zinc-200/70 dark:border-white/10 bg-white dark:bg-zinc-950/60 p-5">
        <h2 class="font-bold text-zinc-900 dark:text-zinc-100">تأثيرات الخلفية</h2>
        <p class="text-sm text-zinc-600 dark:text-zinc-400 mt-1">تقدر تفعل أكثر من تأثير.</p>

        <div class="mt-4 grid sm:grid-cols-2 gap-3">
          <label v-for="e in effectOptions" :key="e.key" class="cursor-pointer rounded-2xl p-4 border border-zinc-200/70 dark:border-white/10 bg-zinc-50/70 dark:bg-white/5 hover:shadow-md transition">
            <div class="flex items-center gap-3">
              <input type="checkbox" class="h-5 w-5" v-model="draft.effects[e.key]" />
              <div>
                <div class="font-semibold text-zinc-900 dark:text-zinc-100">{{ e.label }}</div>
                <div class="text-xs text-zinc-600 dark:text-zinc-400">{{ e.hint }}</div>
              </div>
            </div>
          </label>
        </div>
      </div>
    </div>

    <!-- Ads -->
    <div class="rounded-3xl border border-zinc-200/70 dark:border-white/10 bg-white dark:bg-zinc-950/60 p-5">
      <div class="flex items-center justify-between gap-3 flex-wrap">
        <div>
          <h2 class="font-bold text-zinc-900 dark:text-zinc-100">الإعلانات</h2>
          <p class="text-sm text-zinc-600 dark:text-zinc-400 mt-1">رفع صورة أو ضع رابط صورة. تقدر تفعل أكثر من إعلان.</p>
        </div>
        <button class="px-4 py-2 rounded-2xl border border-zinc-200 dark:border-white/10 hover:bg-zinc-50 dark:hover:bg-white/5 transition" @click="addAd">+ إضافة إعلان</button>
      </div>

      <div class="mt-4 grid gap-4">
        <div v-for="(ad, idx) in draft.ads" :key="ad.id" class="rounded-3xl border border-zinc-200/70 dark:border-white/10 bg-zinc-50/70 dark:bg-white/5 p-4">
          <div class="flex items-start justify-between gap-3">
            <div class="flex items-center gap-3">
              <input type="checkbox" class="h-5 w-5 mt-1" v-model="ad.isActive" />
              <div>
                <div class="font-semibold text-zinc-900 dark:text-zinc-100">إعلان #{{ idx + 1 }}</div>
                <div class="text-xs text-zinc-600 dark:text-zinc-400">Popup يظهر بالصفحة الرئيسية مرة وحدة لكل تحديث.</div>
              </div>
            </div>
            <button class="text-red-600 hover:underline" @click="removeAd(ad.id)">حذف</button>
          </div>

          <div class="mt-4 grid lg:grid-cols-2 gap-4">
            <div class="space-y-3">
              <input v-model="ad.title" class="w-full rounded-2xl border border-zinc-200 dark:border-white/10 bg-white/80 dark:bg-zinc-900/50 px-3 py-2" placeholder="عنوان (اختياري)" />
              <input v-model="ad.linkUrl" class="w-full rounded-2xl border border-zinc-200 dark:border-white/10 bg-white/80 dark:bg-zinc-900/50 px-3 py-2" placeholder="رابط عند الضغط (اختياري)" />

              <div class="rounded-2xl border border-dashed border-zinc-300 dark:border-white/15 p-3">
                <div class="text-sm font-semibold text-zinc-900 dark:text-zinc-100">صورة الإعلان</div>
                <div class="text-xs text-zinc-600 dark:text-zinc-400 mt-1">إما ترفع صورة أو تلصق رابط مباشر.</div>

                <div class="mt-2 flex gap-2 flex-wrap">
                  <input v-model="ad.imageUrl" class="flex-1 min-w-[220px] rounded-2xl border border-zinc-200 dark:border-white/10 bg-white/80 dark:bg-zinc-900/50 px-3 py-2" placeholder="https://..." />
                  <label class="px-4 py-2 rounded-2xl bg-zinc-900 text-white dark:bg-white dark:text-zinc-900 cursor-pointer hover:opacity-90 transition">
                    رفع
                    <input type="file" accept="image/*" class="hidden" @change="(e) => uploadAdImage(e, ad)" />
                  </label>
                </div>
              </div>
            </div>

            <div class="rounded-3xl overflow-hidden border border-zinc-200/70 dark:border-white/10 bg-zinc-50 dark:bg-white/5 min-h-[160px]">
              <img v-if="ad.imageUrl" :src="ad.imageUrl" class="w-full h-full object-cover" />
              <div v-else class="h-full grid place-items-center text-sm text-zinc-500">معاينة الصورة</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="toast" class="fixed bottom-5 right-5 z-[90] rounded-2xl bg-zinc-900 text-white dark:bg-white dark:text-zinc-900 px-4 py-3 shadow-xl">
      {{ toast }}
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

const store = useAppearanceStore()
await store.refresh()

const saving = ref(false)
const toast = ref<string | null>(null)

function notify(msg: string) {
  toast.value = msg
  setTimeout(() => (toast.value = null), 2200)
}

const themeOptions = [
  { key: 'ramadan', label: 'رمضان', hint: 'لمسات هلال/فانوس + لمعة' },
  { key: 'eid', label: 'العيد', hint: 'لمعات خفيفة' },
  { key: 'christmas', label: 'كرسمس', hint: 'ثلج بالخلفية' },
  { key: 'valentines', label: 'عيد الحب', hint: 'قلوب بالخلفية' },
]

const effectOptions = [
  { key: 'snow', label: 'ثلج', hint: 'ينزل بالخلفية' },
  { key: 'ramadan', label: 'رمضان', hint: 'هلال/فانوس حسب الثيم' },
  { key: 'eid', label: 'العيد', hint: 'Sparkles' },
  { key: 'christmas', label: 'كرسمس', hint: 'ثلج + لمعة' },
  { key: 'valentines', label: 'عيد الحب', hint: 'قلوب' },
]

type Draft = {
  themes: string[]
  effects: Record<string, boolean>
  ads: Array<{
    id: string
    title: string
    subtitle?: string | null
    imageUrl: string
    linkUrl?: string | null
    sortOrder?: number
    isActive: boolean
  }>
}

const draft = reactive<Draft>({
  themes: [...(store.data.themes || [])],
  effects: { ...(store.data.effects || {}) },
  ads: (store.data.ads || []).map((a) => ({ ...a })),
})

function addAd() {
  draft.ads.unshift({
    id: crypto.randomUUID(),
    title: '',
    subtitle: null,
    imageUrl: '',
    linkUrl: null,
    sortOrder: 0,
    isActive: true,
  })
}

function removeAd(id: string) {
  draft.ads = draft.ads.filter((x) => x.id !== id)
}

async function uploadAdImage(e: Event, ad: any) {
  const input = e.target as HTMLInputElement
  const f = input.files?.[0]
  if (!f) return

  const fd = new FormData()
  fd.append('file', f)

  try {
    const res: any = await $fetch('/api/bff/admin/appearance/upload', {
      method: 'POST',
      body: fd,
    })
    const u = res?.url
    if (u) {
      ad.imageUrl = typeof u === 'string' ? u : (typeof u?.url === 'string' ? u.url : '')
      notify('تم رفع الصورة')
    }
  } catch {
    notify('فشل رفع الصورة (تأكد من إعدادات ObjectStorage في Vercel)')
  } finally {
    input.value = ''
  }
}

async function save() {
  saving.value = true
  try {
    const enabledThemes = draft.themes
    const enabledEffects = Object.entries(draft.effects)
      .filter(([, v]) => !!v)
      .map(([k]) => k)

    // تأكيد أن imageUrl سترنغ حتى لا يظهر [object Object] ولا يسبب مشاكل بالسيرفر
    const ads = draft.ads.map((a, idx) => {
      const raw = (a as any).imageUrl
      const url = typeof raw === 'string' ? raw : (typeof raw?.url === 'string' ? raw.url : '')
      return {
      title: a.title?.trim() || 'Ad',
      subtitle: (a.subtitle ?? null) ? String(a.subtitle).trim() : null,
      imageUrl: url.trim() || '',
      linkUrl: (a.linkUrl ?? null) ? String(a.linkUrl).trim() : null,
      sortOrder: Number.isFinite(a.sortOrder as any) ? Number(a.sortOrder) : idx,
      isEnabled: !!a.isActive,
      }
    })

    const payload = {
      isActive: true,
      enabledThemes,
      enabledEffects,
      ads,
    }

    await $fetch('/api/bff/admin/appearance', { method: 'POST', body: payload })
    await store.refresh()
    notify('تم الحفظ')
  } catch {
    notify('فشل الحفظ (تأكد من cookies/admin + ObjectStorage env)')
  } finally {
    saving.value = false
  }
}
</script>
