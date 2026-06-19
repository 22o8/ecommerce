<template>
  <div class="grid gap-6">
    <UiButton variant="ghost" class="w-fit" @click="back">
      <Icon name="mdi:arrow-left" class="keep-ltr text-xl" />
      <span class="rtl-text">{{ t('myOrders') }}</span>
    </UiButton>

    <div v-if="pending" class="card-soft p-6">
      <div class="skeleton h-6 w-1/2" />
      <div class="mt-4 grid gap-3">
        <div class="skeleton h-20" />
        <div class="skeleton h-20" />
      </div>
    </div>

    <div v-else-if="error" class="card-soft p-6 border" :style="{ borderColor: 'rgba(var(--danger),.35)', background: 'rgba(var(--danger),.08)' }">
      <div class="font-bold rtl-text">{{ t('error') }}</div>
      <div class="text-sm rtl-text mt-1">{{ error }}</div>
    </div>

    <div v-else class="card-soft p-6">
      <div class="flex items-start justify-between gap-3">
        <div>
          <div class="text-sm text-muted rtl-text">{{ t('order') }}</div>
          <div class="font-black keep-ltr text-xl">{{ item?.id }}</div>
          <div class="mt-2 text-sm text-muted rtl-text">{{ item?.status || item?.state || '—' }}</div>
        </div>
        <UiBadge>
          <Icon name="mdi:calendar-outline" />
          <span class="keep-ltr">{{ (item?.createdAt || item?.createdOn || '').toString().slice(0,10) }}</span>
        </UiBadge>
      </div>

      <div class="mt-6 grid gap-3">
        <div class="rounded-3xl border border-app bg-surface p-4">
          <div class="font-bold rtl-text">{{ t('details') }}</div>
          <pre class="mt-2 text-xs whitespace-pre-wrap keep-ltr text-muted">{{ item }}</pre>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: ['auth'] })
import UiButton from '~/components/ui/UiButton.vue'
import UiBadge from '~/components/ui/UiBadge.vue'
const { t } = useI18n()
const api = useApi()
const route = useRoute()
const router = useRouter()
const pending = ref(false)
const error = ref('')
const item = ref<any>(null)

function back(){ router.push('/orders') }

async function load(){
  pending.value = true
  error.value = ''
  try{
    item.value = await api.get(`/Orders/${route.params.id}`)
  }catch(e:any){
    error.value = e?.data?.message || e?.message || t('failedLoad')
  }finally{
    pending.value = false
  }
}
onMounted(load)
</script>
