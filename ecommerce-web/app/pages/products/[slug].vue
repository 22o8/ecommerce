<template>
  <div class="grid gap-6">
    <UiButton variant="ghost" class="w-fit" @click="back">
      <Icon name="mdi:arrow-left" class="keep-ltr text-xl" />
      <span class="rtl-text">{{ t('backToProducts') }}</span>
    </UiButton>

    <div v-if="loading" class="grid gap-6 lg:grid-cols-2">
      <div class="card-soft p-4">
        <div class="skeleton h-[420px]" />
      </div>
      <div class="card-soft p-6 grid gap-3">
        <div class="skeleton h-7 w-3/4" />
        <div class="skeleton h-5 w-1/2" />
        <div class="skeleton h-24" />
        <div class="skeleton h-12" />
      </div>
    </div>

    <div v-else-if="!p" class="card-soft p-10 text-center">
      <Icon name="mdi:alert-circle-outline" class="text-4xl opacity-70 mx-auto" />
      <div class="mt-3 font-bold rtl-text">{{ t('notFound') }}</div>
    </div>

    <div v-else class="grid gap-6 lg:grid-cols-2">
      <div class="card-soft overflow-hidden">
        <div class="h-[420px] bg-surface-2 grid place-items-center">
          <img v-if="img" :src="img" class="h-full w-full object-cover" :alt="p.name" />
          <div v-else class="text-center grid gap-2 px-6">
            <Icon name="mdi:image-outline" class="text-4xl opacity-70 mx-auto" />
            <div class="text-sm text-muted rtl-text">{{ t('noImage') }}</div>
          </div>
        </div>
      </div>

      <div class="card-soft p-6 md:p-8 grid gap-4">
        <div>
          <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ p.name }}</h1>
          <div class="mt-2 text-muted rtl-text">{{ p.description || '' }}</div>
        </div>

        <div class="flex items-center justify-between">
          <div class="text-sm text-muted rtl-text">{{ t('price') }}</div>
          <div class="text-2xl font-black keep-ltr">{{ fmt(p.price) }}</div>
        </div>

        <div class="grad-line" />

        <div class="grid gap-3">
          <UiButton v-if="auth.isAuthed" @click="buy" :loading="buying">
            <Icon name="mdi:cart-outline" class="text-lg" />
            <span class="rtl-text">{{ t('buyNow') }}</span>
          </UiButton>

          <NuxtLink v-else to="/login">
            <UiButton>
              <Icon name="mdi:login-variant" class="text-lg" />
              <span class="rtl-text">{{ t('loginToBuy') }}</span>
            </UiButton>
          </NuxtLink>

          <a class="rounded-2xl border border-app bg-surface px-4 py-3 text-sm hover:bg-surface-2 transition keep-ltr" :href="waOrderLink" target="_blank" rel="noreferrer">
            <Icon name="mdi:whatsapp" class="inline-block text-lg align-middle" />
            <span class="ml-2 rtl-text">{{ t('whatsappOrder') }}</span>
          </a>

          <p v-if="msg" class="text-sm rtl-text" :class="ok ? 'text-[rgb(var(--success))]' : 'text-[rgb(var(--danger))]'">{{ msg }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import { useApi } from '~/composables/useApi'

const { t } = useI18n()
const auth = useAuthStore()
const api = useApi()
const route = useRoute()
const router = useRouter()
const config = useRuntimeConfig()

const loading = ref(true)
const buying = ref(false)
const msg = ref('')
const ok = ref(false)

const p = ref<any>(null)
const img = computed(() => api.buildAssetUrl(p.value?.images?.[0] || p.value?.imageUrl || p.value?.image || ''))

const waOrderLink = computed(() => {
  const n = String((config.public as any).whatsappNumber || '').replace(/[^0-9]/g,'')
  const text = encodeURIComponent(`Order: ${p.value?.name || ''} | Price: ${p.value?.price || ''}`)
  return n ? `https://wa.me/${n}?text=${text}` : '#'
})

function fmt(v:any){
  const n = Number(v||0)
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}

function back(){ router.push('/products') }

async function fetchProduct(){
  loading.value = true
  msg.value = ''
  try{
    // backend supports /Products/{id} and /Products/BySlug?slug=...
    const slug = String(route.params.slug || '')
    // try by slug endpoint first
    p.value = await api.get(`/Products/by-slug`, { slug }).catch(async () => {
      // fallback: treat as id
      return await api.get(`/Products/${slug}`)
    })
  }catch(e:any){
    p.value = null
  }finally{
    loading.value = false
  }
}

async function buy(){
  if(!p.value) return
  buying.value = true
  msg.value = ''
  try{
    // example order endpoint
    const res:any = await api.post('/Orders', { productId: p.value.id })
    ok.value = true
    msg.value = t('orderCreated')
    // optionally redirect
    router.push('/orders')
  }catch(e:any){
    ok.value = false
    msg.value = e?.data?.message || e?.message || t('buyFailed')
  }finally{
    buying.value = false
  }
}

onMounted(fetchProduct)
</script>
