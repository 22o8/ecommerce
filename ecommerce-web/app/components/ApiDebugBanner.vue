<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useUiStore } from '~/stores/ui'

const ui = useUiStore()
const route = useRoute()

const enabled = computed(() => ui.apiDebug)
const err = computed(() => ui.lastApiError)

const copied = ref(false)
const { t } = useI18n()

function toggle() {
  ui.setApiDebugEnabled(!ui.apiDebug)
}

async function copyDetails() {
  try {
    const payload = JSON.stringify(err.value, null, 2)
    await navigator.clipboard.writeText(payload)
    copied.value = true
    setTimeout(() => (copied.value = false), 1200)
  } catch {
    // ignore
  }
}

onMounted(() => {
  // تفعيل سريع عبر ?debug=1
  const q = String((route.query.debug ?? '')).trim()
  if (q === '1' || q === 'true') ui.setApiDebugEnabled(true)
})
</script>

<template>
  <div v-if="enabled" class="sticky top-0 z-[60] px-3 py-2">
    <div class="rounded-2xl border border-black/10 dark:border-white/10 bg-white/90 dark:bg-black/60 backdrop-blur px-3 py-2 shadow-sm">
      <div class="flex items-start justify-between gap-3">
        <div class="min-w-0">
          <div class="flex items-center gap-2">
            <span class="text-sm font-semibold">{{ t('debug.title') }}</span>
            <span class="text-xs opacity-70">({{ t('debug.mode') }})</span>
          </div>

          <div v-if="err" class="mt-1 text-xs leading-5 opacity-90 break-words">
            <div>
              <span class="opacity-70">{{ t('debug.url') }}:</span>
              <span class="font-mono"> {{ err.url }} </span>
            </div>
            <div class="flex flex-wrap gap-x-3">
              <div><span class="opacity-70">{{ t('debug.method') }}:</span> <span class="font-mono">{{ err.method }}</span></div>
              <div v-if="err.status != null"><span class="opacity-70">{{ t('debug.status') }}:</span> <span class="font-mono">{{ err.status }}</span></div>
              <div><span class="opacity-70">{{ t('debug.at') }}:</span> <span class="font-mono">{{ err.at }}</span></div>
            </div>
            <div class="mt-1">
              <span class="opacity-70">{{ t('debug.message') }}:</span>
              <span class="font-mono"> {{ err.message }} </span>
            </div>
          </div>

          <div v-else class="mt-1 text-xs opacity-70">
            {{ t('debug.noErrors') }}
          </div>
        </div>

        <div class="flex shrink-0 items-center gap-2">
          <button
            type="button"
            class="rounded-xl px-3 py-1.5 text-xs border border-black/10 dark:border-white/10 hover:bg-black/5 dark:hover:bg-white/10"
            @click="toggle"
          >
            {{ t('debug.disable') }}
          </button>

          <button
            v-if="err"
            type="button"
            class="rounded-xl px-3 py-1.5 text-xs border border-black/10 dark:border-white/10 hover:bg-black/5 dark:hover:bg-white/10"
            @click="copyDetails"
          >
            {{ copied ? t('debug.copied') : t('debug.copyDetails') }}
          </button>

          <button
            v-if="err"
            type="button"
            class="rounded-xl px-3 py-1.5 text-xs border border-black/10 dark:border-white/10 hover:bg-black/5 dark:hover:bg-white/10"
            @click="ui.clearLastApiError()"
          >
            {{ t('debug.clear') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
