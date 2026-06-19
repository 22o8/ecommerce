<template>
  <div
    :style="props.wrapperStyle"
    :class="['relative overflow-hidden', props.rounded, props.background, props.wrapperClass]"
  >
    <!--
      مهم جداً:
      لا نستخدم NuxtImg هنا حتى لا تمر الصور عبر /_vercel/image.
      Vercel Image Optimization محدود بالخطة المجانية، وعند تجاوزه يظهر:
      OPTIMIZED_IMAGE_REQUEST_PAYMENT_REQUIRED
      لذلك نعرض الصور مباشرة من R2/Cloudflare: img.drseoulbeauty.store
    -->
    <img
      :src="currentSrc"
      :srcset="computedSrcset"
      :sizes="computedSizes"
      :alt="computedAlt"
      :loading="props.loading"
      :title="props.title || computedAlt"
      :width="props.width"
      :height="props.height"
      decoding="async"
      :fetchpriority="props.fetchpriority"
      :style="props.imgStyle"
      :class="[
        'block w-full h-full select-none',
        props.fit === 'contain' ? 'object-contain' : 'object-cover',
        props.imgClass,
      ]"
      @error="onError"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useApi } from '~/composables/useApi'

type SmartImageProps = {
  src: string
  alt?: string
  title?: string
  width?: number | string
  height?: number | string
  /** موجودة للتوافق مع الاستدعاءات القديمة فقط؛ لا نستخدمها مع img العادي */
  sizes?: string
  quality?: number | string
  format?: 'webp' | 'avif' | 'jpg' | 'png'
  fetchpriority?: 'high' | 'low' | 'auto'
  fit?: 'cover' | 'contain'
  loading?: 'lazy' | 'eager'
  wrapperClass?: string
  imgClass?: string
  imgStyle?: any
  wrapperStyle?: any
  rounded?: string
  background?: string
}

const props = withDefaults(defineProps<SmartImageProps>(), {
  alt: '',
  title: '',
  width: undefined,
  height: undefined,
  sizes: 'sm:100vw md:50vw lg:33vw',
  quality: 80,
  format: 'webp',
  fetchpriority: 'auto',
  fit: 'cover',
  loading: 'lazy',
  wrapperClass: '',
  imgClass: '',
  imgStyle: undefined,
  wrapperStyle: undefined,
  rounded: 'rounded-xl',
  background: 'bg-transparent',
})

const FALLBACK = '/img-placeholder.svg'
const api = useApi()

function resolveUrl(v: string | undefined | null) {
  const raw = (v || '').trim()
  if (!raw) return FALLBACK

  if (
    raw.startsWith('http://') ||
    raw.startsWith('https://') ||
    raw.startsWith('data:') ||
    raw.startsWith('/_nuxt/') ||
    raw.startsWith('/favicon') ||
    raw.startsWith('/hero-placeholder') ||
    raw.startsWith('/img-placeholder')
  ) {
    return raw
  }

  return api.buildAssetUrl(raw)
}

const srcRef = ref(resolveUrl(props.src))

watch(
  () => props.src,
  (v) => {
    srcRef.value = resolveUrl(v)
  },
  { immediate: true },
)

const currentSrc = computed(() => srcRef.value || FALLBACK)

function optimizedVariant(url: string, variant: 'thumb' | 'medium' | 'large') {
  if (!url || !url.includes('/optimized/') || !url.endsWith('.webp')) return ''
  return url
    .replace(/-(thumb|medium|large)\.webp($|\?)/i, `-${variant}.webp$2`)
}

const computedSrcset = computed(() => {
  const u = currentSrc.value
  const thumb = optimizedVariant(u, 'thumb')
  const medium = optimizedVariant(u, 'medium')
  const large = optimizedVariant(u, 'large')
  if (!thumb || !medium || !large) return undefined
  return `${thumb} 150w, ${medium} 600w, ${large} 1200w`
})

const computedSizes = computed(() => {
  if (!computedSrcset.value) return undefined
  const raw = String(props.sizes || '').trim()
  if (!raw || raw.includes(':')) return '(max-width: 640px) 150px, (max-width: 1024px) 600px, 1200px'
  return raw
})

const computedAlt = computed(() => props.alt || props.title || 'DR SEOUL BEAUTY image')

function onError() {
  if (srcRef.value !== FALLBACK) srcRef.value = FALLBACK
}
</script>
