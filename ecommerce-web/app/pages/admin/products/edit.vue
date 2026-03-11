<!-- pages/admin/products/edit.vue -->
<template>
  <div class="wrap">
    <h1 class="title">{{ t('admin.editProductPage.title') }}</h1>

    <div v-if="loading" class="box">{{ t('admin.editProductPage.loading') }}</div>
    <div v-else class="box">
      <div class="row">
        <label class="lbl">{{ t('admin.editProductPage.name') }}</label>
        <input v-model="product.name" class="inp" :placeholder="t('admin.editProductPage.namePlaceholder')" />
      </div>

      <div class="row">
        <label class="lbl">{{ t('admin.editProductPage.price') }}</label>
        <input v-model.number="product.price" class="inp" type="number" placeholder="0" />
      </div>

      <div class="row">
        <label class="lbl">{{ t('admin.editProductPage.description') }}</label>
        <textarea v-model="product.description" class="inp" rows="4" :placeholder="t('admin.editProductPage.descriptionPlaceholder')"></textarea>
      </div>

      <div class="row">
        <label class="lbl">{{ t('admin.editProductPage.images') }}</label>

        <UploadImages
          v-model="product.images"
          :product-id="product.id"
        />

        <p class="note">
          {{ t('admin.editProductPage.note') }}
        </p>
      </div>

      <div class="actions">
        <button class="btn" :disabled="saving" @click="save">
          {{ saving ? t('admin.editProductPage.saving') : t('admin.editProductPage.save') }}
        </button>
      </div>

      <div v-if="msg" class="msg">{{ msg }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UploadImages from '~/components/UploadImages.vue'
const { t } = useI18n()

type ProductDto = {
  id: number
  name: string
  price: number
  description: string
  images: string[]
}

const route = useRoute()
const id = Number(route.query.id || route.params.id || 0)

const { get, put } = useApi()

const loading = ref(true)
const saving = ref(false)
const msg = ref('')

const product = reactive<ProductDto>({
  id: id || 0,
  name: '',
  price: 0,
  description: '',
  images: [],
})

const load = async () => {
  loading.value = true
  msg.value = ''
  try {
    const data = await get<ProductDto>(`/Products/${product.id}`)
    product.id = data.id
    product.name = data.name || ''
    product.price = Number(data.price || 0)
    product.description = data.description || ''
    product.images = Array.isArray((data as any).images) ? (data as any).images : []
  } catch (e: any) {
    msg.value = e?.message || t('admin.editProductPage.loadFailed')
  } finally {
    loading.value = false
  }
}

const save = async () => {
  saving.value = true
  msg.value = ''
  try {
    await put(`/Products/${product.id}`, {
      id: product.id,
      name: product.name,
      price: product.price,
      description: product.description,
      images: product.images,
    })
    msg.value = t('admin.editProductPage.saved')
  } catch (e: any) {
    msg.value = e?.message || t('admin.editProductPage.saveFailed')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.wrap { max-width: 980px; margin: 0 auto; padding: 20px; }
.title { font-size: 22px; font-weight: 800; margin-bottom: 12px; }
.box { border: 1px solid #eee; border-radius: 16px; padding: 16px; background: #fff; }
.row { margin-bottom: 14px; }
.lbl { display: block; font-weight: 700; margin-bottom: 8px; }
.inp {
  width: 100%;
  border: 1px solid #ddd;
  border-radius: 12px;
  padding: 10px 12px;
  outline: none;
}
.actions { margin-top: 16px; display: flex; justify-content: flex-end; }
.btn { border-radius: 12px; padding: 10px 14px; border: 1px solid #ddd; background: #fff; cursor: pointer; font-weight: 800; }
.btn:disabled { opacity: .6; cursor: not-allowed; }
.msg { margin-top: 12px; padding: 10px 12px; border-radius: 12px; background: #f7f7f7; }
.note { margin-top: 10px; font-size: 13px; opacity: .8; line-height: 1.7; }
</style>
