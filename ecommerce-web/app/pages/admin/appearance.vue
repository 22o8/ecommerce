<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between gap-3 flex-wrap">
      <div>
        <h1 class="text-2xl font-extrabold text-zinc-900 dark:text-zinc-100">{{ t('admin.appearance.title') || 'الثيمات والتأثيرات' }}</h1>
        <p class="text-sm text-zinc-600 dark:text-zinc-400">{{ t('admin.appearance.subtitle') || 'تحكم بالخلفيات والتأثيرات التي تظهر لكل الزبائن.' }}</p>
      </div>
      <button
        class="px-4 py-2 rounded-2xl bg-zinc-900 text-white dark:bg-white dark:text-zinc-900 hover:opacity-90 transition"
        @click="save"
        :disabled="saving"
      >
        {{ saving ? (t('common.saving') || 'جارِ الحفظ...') : (t('common.save') || 'حفظ') }}
      </button>
    </div>

    <div class="grid gap-6 lg:grid-cols-2">
      <!-- Themes -->
      <div class="rounded-3xl border border-zinc-200/70 dark:border-white/10 bg-white dark:bg-zinc-950/60 p-5">
        <h2 class="font-bold text-zinc-900 dark:text-zinc-100">{{ t('admin.appearance.themes') || 'الثيمات' }}</h2>
        <p class="text-sm text-zinc-600 dark:text-zinc-400 mt-1">{{ t('admin.appearance.themesHint') || 'تقدر تفعل أكثر من ثيم بنفس الوقت.' }}</p>

        <div class="mt-4 grid sm:grid-cols-2 gap-3">
          <label v-for="opt in themeOptions" :key="opt.key" class="group cursor-pointer rounded-2xl p-4 border border-zinc-200/70 dark:border-white/10 bg-zinc-50/70 dark:bg-white/5 hover:shadow-md transition">
            <div class="flex items-center gap-3">
              <input type="checkbox" class="h-5 w-5" v-model="draft.themes" :value="opt.key" />
              <div>
                <div class="font-semibold text-zinc-900 dark:text-zinc-100">{{ t(opt.labelKey) }}</div>
                <div class="text-xs text-zinc-600 dark:text-zinc-400">{{ t(opt.hintKey) }}</div>
              </div>
            </div>
          </label>
        </div>
      </div>

      <!-- Effects -->
      <div class="rounded-3xl border border-zinc-200/70 dark:border-white/10 bg-white dark:bg-zinc-950/60 p-5">
        <h2 class="font-bold text-zinc-900 dark:text-zinc-100">{{ t('admin.appearance.effects') || 'تأثيرات الخلفية' }}</h2>
        <p class="text-sm text-zinc-600 dark:text-zinc-400 mt-1">{{ t('admin.appearance.effectsHint') || 'تقدر تفعل أكثر من تأثير.' }}</p>

        <div class="mt-4 grid sm:grid-cols-2 gap-3">
          <label v-for="e in effectOptions" :key="e.key" class="cursor-pointer rounded-2xl p-4 border border-zinc-200/70 dark:border-white/10 bg-zinc-50/70 dark:bg-white/5 hover:shadow-md transition">
            <div class="flex items-center gap-3">
              <input type="checkbox" class="h-5 w-5" v-model="draft.effects[e.key]" />
              <div>
                <div class="font-semibold text-zinc-900 dark:text-zinc-100">{{ e.label || t(e.labelKey) }}</div>
                <div class="text-xs text-zinc-600 dark:text-zinc-400">{{ e.hint || t(e.hintKey) }}</div>
              </div>
            </div>
          </label>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

const { t } = useI18n()

const store = useAppearanceStore()
if (!store.loaded) await store.refresh()

const saving = ref(false)
const toast = useToast()

const themeOptions = [
  { key: 'ramadan', labelKey: 'season.ramadan', hintKey: 'seasonHints.ramadan' },
  { key: 'eid', labelKey: 'season.eid', hintKey: 'seasonHints.eid' },
  { key: 'christmas', labelKey: 'season.christmas', hintKey: 'seasonHints.christmas' },
  { key: 'valentines', labelKey: 'season.valentines', hintKey: 'seasonHints.valentines' },
  { key: 'blackFriday', labelKey: 'season.blackFriday', hintKey: 'seasonHints.blackFridayTheme' },
]

const effectOptions = [
  { key: 'snow', labelKey: 'season.snow', hintKey: 'seasonHints.snow' },
  { key: 'ramadan', labelKey: 'season.ramadan', hintKey: 'seasonHints.ramadanEffect' },
  { key: 'eid', labelKey: 'season.eid', hintKey: 'seasonHints.eidEffect' },
  { key: 'christmas', labelKey: 'season.christmas', hintKey: 'seasonHints.christmasEffect' },
  { key: 'valentines', labelKey: 'season.valentines', hintKey: 'seasonHints.valentinesEffect' },
  { key: 'blackFriday', labelKey: 'season.blackFriday', hintKey: 'seasonHints.blackFridayEffect' },
  { key: 'rosesEdge', label: 'الشكل الثاني', hint: 'ورود وردية ثابتة على أطراف الصفحة في الثيمين.' },
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
    toast.success(t('common.saved') || 'تم الحفظ')
  } catch {
    toast.error(t('common.requestFailed') || 'فشل الطلب')
  } finally {
    saving.value = false
  }
}
</script>
