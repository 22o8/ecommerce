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
  const list = buildLogoCandidates(b.logoUrl)
  const i = logoTryIndex.value[key] ?? 0
  const picked = list[i]
  return picked ? api.buildAssetUrl(picked) : ''
}
const onLogoError = (b: Brand, idx: number) => {
  const key = keyOf(b, idx)
  const list = buildLogoCandidates(b.logoUrl)
  const i = logoTryIndex.value[key] ?? 0
  if (i + 1 < list.length) logoTryIndex.value[key] = i + 1
}

// نكرر القائمة حتى يصير تمرير مستمر + فلترة العناصر الناقصة (حماية SSR)
const clean = computed(() => (props.brands ?? []).filter((b): b is Brand => !!b && !!b.slug && !!b.id))
const loop = computed(() => [...clean.value, ...clean.value])
</script>

<template>
  <section v-if="clean.length" class="mt-16">
    <div class="mt-8 overflow-hidden">
      <div class="marquee">
        <div class="marquee__track">
          <NuxtLink
            v-for="(b, idx) in loop"
            :key="b.id + '-' + idx"
            :to="`/brands/${b.slug}`"
            class="marquee__item"
          >
            <div class="marquee__card">
              <div class="marquee__logo">
                <img
                  v-if="b.logoUrl"
                  :src="logoSrc(b, idx)"
                  :alt="b.name"
                  class="h-12 w-12 rounded-2xl object-cover"
                  loading="lazy"
                  @error="onLogoError(b, idx)"
                />
                <div v-else class="h-10 w-10 rounded-xl bg-black/5" />
              </div>
              <span v-if="showName" class="marquee__name">{{ b.name }}</span>
            </div>
          </NuxtLink>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.marquee{
  --gap: 14px;
  --speed: 28s;
  position: relative;
}

.marquee__track{
  display: flex;
  gap: var(--gap);
  width: max-content;
  padding: 6px var(--gap);
  animation: marquee var(--speed) linear infinite;
}

.marquee__item{ text-decoration:none; }

.marquee__card{
  min-width: 68px;
  width: 68px;
  height: 68px;
  padding: 0;
  border-radius: 9999px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--surface-1));
  box-shadow: var(--shadow-card);
}

.marquee__card:hover{
  transform: translateY(-2px);
  box-shadow: 0 18px 55px rgba(0,0,0,.10);
}

.marquee__name{
  font-weight: 700;
  color: rgb(var(--text));
  white-space: nowrap;
}

@keyframes marquee{
  from{ transform: translateX(-50%); }
  to{ transform: translateX(0); }
}

/* احترام تفضيل تقليل الحركة */
@media (prefers-reduced-motion: reduce){
  .marquee__track{ animation: none; }
}
</style>
