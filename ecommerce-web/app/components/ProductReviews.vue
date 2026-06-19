<template>
  <div>
    <div class="flex items-center justify-between gap-3">
      <div class="font-bold rtl-text">{{ t('reviews.title') }}</div>
      <div class="flex items-center gap-2">
        <div class="keep-ltr font-extrabold">{{ avg.toFixed(1) }}</div>
        <div class="flex items-center">
          <Icon
            v-for="i in 5"
            :key="i"
            :name="i <= Math.round(avg) ? 'mdi:star' : 'mdi:star-outline'"
            class="text-xl opacity-90"
          />
        </div>
      </div>
    </div>

    <div class="mt-3 grid gap-2">
      <div class="text-sm text-muted rtl-text" v-if="items.length === 0">{{ t('reviews.empty') }}</div>

      <div v-for="r in items" :key="r.id" class="rounded-2xl border border-app bg-surface/40 p-3">
        <div class="flex items-center justify-between gap-3">
          <div class="font-bold rtl-text">{{ r.name }}</div>
          <div class="flex items-center">
            <Icon
              v-for="i in 5"
              :key="i"
              :name="i <= r.rating ? 'mdi:star' : 'mdi:star-outline'"
              class="text-lg opacity-90"
            />
          </div>
        </div>
        <div class="mt-1 text-sm rtl-text" v-if="r.text">{{ r.text }}</div>
      </div>
    </div>

    <div class="mt-4 grid gap-2">
      <div class="font-bold rtl-text">{{ t('reviews.addTitle') }}</div>

      <div class="flex flex-wrap items-center gap-3">
        <input
          v-model="form.name"
          class="w-full md:w-64 rounded-2xl border border-app bg-surface px-4 py-2 text-sm rtl-text"
          :placeholder="t('reviews.name')"
        />
        <div class="flex items-center gap-1">
          <button
            v-for="i in 5"
            :key="i"
            class="p-1"
            @click="form.rating = i"
            :title="String(i)"
          >
            <Icon :name="i <= form.rating ? 'mdi:star' : 'mdi:star-outline'" class="text-2xl" />
          </button>
        </div>
      </div>

      <textarea
        v-model="form.text"
        rows="3"
        class="w-full rounded-2xl border border-app bg-surface px-4 py-2 text-sm rtl-text"
        :placeholder="t('reviews.comment')"
      />

      <div class="flex items-center gap-2">
        <UiButton @click="submit" :disabled="!canSubmit">
          <Icon name="mdi:send" class="text-lg" />
          <span class="rtl-text">{{ t('reviews.submit') }}</span>
        </UiButton>
        <UiButton variant="ghost" @click="clear" v-if="items.length">
          <Icon name="mdi:delete-outline" class="text-lg" />
          <span class="rtl-text">{{ t('reviews.clear') }}</span>
        </UiButton>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'

type Review = {
  id: string
  name: string
  rating: number
  text: string
  ts: number
}

const props = defineProps<{ productId?: string | number | null }>()
const { t } = useI18n()

const pid = computed(() => String(props.productId ?? ''))
const KEY = computed(() => `reviews:v1:${pid.value}`)

const items = ref<Review[]>([])

const form = reactive({
  name: '',
  rating: 5,
  text: ''
})

const canSubmit = computed(() => pid.value && form.name.trim().length >= 2 && form.rating >= 1 && form.rating <= 5)

const avg = computed(() => {
  if (items.value.length === 0) return 0
  const sum = items.value.reduce((a, b) => a + (Number(b.rating) || 0), 0)
  return sum / items.value.length
})

function load() {
  if (process.server) return
  if (!pid.value) return
  try {
    const raw = localStorage.getItem(KEY.value)
    items.value = raw ? JSON.parse(raw) : []
  } catch {
    items.value = []
  }
}

function save() {
  if (process.server) return
  if (!pid.value) return
  localStorage.setItem(KEY.value, JSON.stringify(items.value))
}

function submit() {
  if (!canSubmit.value) return
  const r: Review = {
    id: crypto?.randomUUID?.() || String(Date.now()),
    name: form.name.trim(),
    rating: Number(form.rating) || 5,
    text: form.text.trim(),
    ts: Date.now()
  }
  items.value = [r, ...items.value]
  save()
  form.text = ''
}

function clear() {
  items.value = []
  save()
}

watch(pid, () => load(), { immediate: true })
</script>