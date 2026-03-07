<script setup lang="ts">
import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'

type Brand = {
  id: string
  name: string
  slug: string
  logoUrl?: string | null
}

const props = defineProps<{ brands: (Brand | null | undefined)[]; showName?: boolean }>()

const showName = computed(() => props.showName === true)

const api = useApi()

function buildLogoCandidates(url?: string | null) {
  if (!url) return [] as string[]
  // جرّب نفس الرابط، وبعدها بدائل الامتداد الشائعة (حل عملي لمشكلة logo.jpg 404)
  const u = url.trim()
  const list = [u]
  if (u.endsWith('.jpg') || u.endsWith('.jpeg')) {
    list.push(u.replace(/\.jpe?g$/i, '.png'))
    list.push(u.replace(/\.jpe?g$/i, '.webp'))
  } else if (u.endsWith('.png')) {
    list.push(u.replace(/\.png$/i, '.webp'))
    list.push(u.replace(/\.png$/i, '.jpg'))
  } else if (u.endsWith('.webp')) {
    list.push(u.replace(/\.webp$/i, '.png'))
    list.push(u.replace(/\.webp$/i, '.jpg'))
  }
  return Array.from(new Set(list))
}

const logoTryIndex = ref<Record<string, number>>({})
const keyOf = (b: Brand, idx: number) => `${b.id}-${idx}`
const logoSrc = (b: Brand, idx: number) => {
  const key = keyOf(b, idx)
  const i = logoTryIndex.value[key] ?? 0
  const candidates = buildLogoCandidates(b.logoUrl)
  const chosen = candidates[i] || b.logoUrl || ''
  // مرّر روابط /uploads عبر الـ BFF حتى تشتغل على كل الأجهزة والـ VPN
  return api.buildAssetUrl(chosen)
}
const onLogoError = (b: Brand, idx: number) => {
  const key = keyOf(b, idx)
  const list = buildLogoCandidates(b.logoUrl)
  const i = logoTryIndex.value[key] ?? 0
  if (i + 1 < list.length) logoTryIndex.value[key] = i + 1
}

// نكرر القائمة حتى يصير تمرير مستمر + فلترة العناصر الناقصة (حماية SSR)
const clean = computed(() => (props.brands ?? []).filter((b): b is Brand => !!b && !!b.slug && !!b.id))
const loop = computed(() => [...clean.value, ...clean.value, ...clean.value])
</script>

<template>
  <section v-if="clean.length" class="mt-14">
    <div class="brand-strip-wrap">
      <div class="brand-strip">
        <NuxtLink
          v-for="(b, idx) in loop"
          :key="b.id + '-' + idx"
          :to="`/brands/${b.slug}`"
          class="brand-pill"
          :title="b.name"
        >
          <SmartImage
            v-if="b.logoUrl"
            :src="logoSrc(b, idx)"
            :alt="b.name"
            fit="cover"
            wrapper-class="brand-pill__img-wrap"
            img-class="brand-pill__img"
            loading="lazy"
            @error="onLogoError(b, idx)"
          />
          <div v-else class="brand-pill__fallback">{{ (b.name || '?').slice(0,1) }}</div>
        </NuxtLink>
      </div>
    </div>
  </section>
</template>

<style scoped>
.brand-strip-wrap{
  position: relative;
  overflow: hidden;
  padding: 10px 0 4px;
  mask-image: linear-gradient(to right, transparent, #000 8%, #000 92%, transparent);
  -webkit-mask-image: linear-gradient(to right, transparent, #000 8%, #000 92%, transparent);
}
.brand-strip{
  display:flex;
  align-items:center;
  gap:16px;
  width:max-content;
  animation: brand-marquee 24s linear infinite;
  will-change: transform;
}
.brand-pill{
  flex:0 0 auto;
  display:grid;
  place-items:center;
  width:74px;
  height:74px;
  border-radius:9999px;
  border:1px solid rgba(var(--border),.9);
  background: linear-gradient(180deg, rgba(var(--surface-1), .98), rgba(var(--surface-2), .82));
  box-shadow: 0 10px 28px rgba(0,0,0,.10);
  transition: transform .18s ease, border-color .2s ease, box-shadow .2s ease;
  text-decoration:none;
}
.brand-pill:hover{
  transform: translateY(-2px) scale(1.03);
  border-color: rgba(var(--primary), .5);
  box-shadow: 0 14px 34px rgba(0,0,0,.14);
}
.brand-pill__img-wrap{
  width:54px;
  height:54px;
  overflow:hidden;
  border-radius:16px;
  background: rgba(255,255,255,.04);
}
.brand-pill__img{
  width:100%;
  height:100%;
  object-fit:cover;
}
.brand-pill__fallback{
  display:grid;
  place-items:center;
  width:54px;
  height:54px;
  border-radius:16px;
  background: rgba(var(--primary), .14);
  color: rgb(var(--text));
  font-weight: 900;
}
@keyframes brand-marquee{
  from{ transform: translateX(calc(-33.333% - 10px)); }
  to{ transform: translateX(0); }
}
@media (max-width: 640px){
  .brand-strip-wrap{
    mask-image: linear-gradient(to right, transparent, #000 4%, #000 96%, transparent);
    -webkit-mask-image: linear-gradient(to right, transparent, #000 4%, #000 96%, transparent);
  }
  .brand-strip{ gap:12px; animation-duration: 18s; }
  .brand-pill{ width:64px; height:64px; }
  .brand-pill__img-wrap, .brand-pill__fallback{ width:46px; height:46px; }
}
</style>
