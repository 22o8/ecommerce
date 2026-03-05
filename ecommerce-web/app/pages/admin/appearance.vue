<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3 flex-wrap">
      <div>
        <h1 class="text-2xl font-extrabold text-zinc-900 dark:text-zinc-100">الثيمات والتأثيرات</h1>
        <p class="text-sm text-zinc-600 dark:text-zinc-400">تحكم بالخلفيات والتأثيرات التي تظهر لكل الزبائن.</p>
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
  { key: 'blackFriday', label: 'Black Friday', hint: 'داكن: glow أحمر + neon / فاتح: confetti' },
]

const effectOptions = [
  { key: 'snow', label: 'ثلج', hint: 'ينزل بالخلفية' },
  { key: 'ramadan', label: 'رمضان', hint: 'هلال/فانوس حسب الثيم' },
  { key: 'eid', label: 'العيد', hint: 'Sparkles' },
  { key: 'christmas', label: 'كرسمس', hint: 'ثلج + لمعة' },
  { key: 'valentines', label: 'عيد الحب', hint: 'قلوب' },
  { key: 'blackFriday', label: 'Black Friday', hint: 'discount particles % + neon lines' },
]

type Draft = {
  themes: string[]
  effects: Record<string, boolean>
}

const draft = reactive<Draft>({
  themes: [...(store.data.themes || [])],
  effects: { ...(store.data.effects || {}) },
})

async function save() {
  saving.value = true
  try {
    const enabledThemes = draft.themes
    const enabledEffects = Object.entries(draft.effects)
      .filter(([, v]) => !!v)
      .map(([k]) => k)

    const payload = {
      isActive: true,
      enabledThemes,
      enabledEffects,
      // ads أصبحت صفحة مستقلة: /admin/ads
      ads: [],
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
