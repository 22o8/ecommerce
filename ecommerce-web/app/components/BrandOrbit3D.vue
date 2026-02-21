<script setup lang="ts">
import { computed } from 'vue'
import { useApi } from '~/composables/useApi'

type Brand = {
  id: string
  name: string
  slug: string
  logoUrl?: string | null
}

const props = defineProps<{
  brands: (Brand | null | undefined)[]
  radius?: number
  tiltDeg?: number
  speedSec?: number
}>()

const api = useApi()

const clean = computed(() => (props.brands ?? []).filter((b): b is Brand => !!b && !!b.slug && !!b.id))

const radius = computed(() => Number(props.radius ?? 240))
const tilt = computed(() => Number(props.tiltDeg ?? 18))
const speed = computed(() => Number(props.speedSec ?? 18))

const stepDeg = computed(() => (clean.value.length ? 360 / clean.value.length : 0))

function itemStyle(idx: number) {
  const deg = idx * stepDeg.value
  // rotateY: توزيع العناصر على دائرة
  // translateZ: نصف القطر
  return {
    transform: `rotateY(${deg}deg) translateZ(${radius.value}px)`,
  } as Record<string, string>
}

function logoUrl(b: Brand) {
  return api.buildAssetUrl(b.logoUrl || '')
}
</script>

<template>
  <section v-if="clean.length" class="orbit-wrap">
    <!--
      ✅ حلقة 3D: الجزء الخلفي يبدو أعلى بسبب tilt + perspective.
      ✅ الدائرة خلف المحتوى بصرياً (z-index منخفض) لكن روابطها شغالة.
    -->
    <div
      class="orbit-scene"
      :style="{ '--orbit-tilt': `${tilt}deg`, '--orbit-speed': `${speed}s` } as any"
    >
      <div class="orbit-ring">
        <NuxtLink
          v-for="(b, idx) in clean"
          :key="b.id"
          :to="`/brands/${b.slug}`"
          class="orbit-item"
          :style="itemStyle(idx)"
          :aria-label="b.name"
          :title="b.name"
        >
          <div class="orbit-card">
            <img
              v-if="b.logoUrl"
              :src="logoUrl(b)"
              :alt="b.name"
              loading="lazy"
              class="orbit-img"
            />
            <div v-else class="orbit-fallback">Logo</div>
          </div>
        </NuxtLink>
      </div>
    </div>
  </section>
</template>

<style scoped>
.orbit-wrap{
  position: relative;
  /* خليه يطلع شوي لفوك حتى الجزء الخلفي "يدخل" خلف المنتجات */
  margin-top: -24px;
}

.orbit-scene{
  position: relative;
  width: 100%;
  height: 220px;
  display: grid;
  place-items: center;
  perspective: 1100px;
}

.orbit-ring{
  position: relative;
  width: 0;
  height: 0;
  transform-style: preserve-3d;
  /* tilt يخلي الخلفية ترتفع */
  transform: rotateX(var(--orbit-tilt));
  animation: orbitSpin var(--orbit-speed) linear infinite;
}

.orbit-item{
  position: absolute;
  left: 50%;
  top: 50%;
  transform-style: preserve-3d;
  translate: -50% -50%;
  text-decoration: none;
}

.orbit-card{
  width: 76px;
  height: 76px;
  border-radius: 9999px;
  border: 1px solid rgb(var(--border));
  background: rgba(var(--surface-rgb), .96);
  box-shadow: var(--shadow-card);
  display: grid;
  place-items: center;
  overflow: hidden;
  backface-visibility: hidden;
  transition: transform 220ms ease, box-shadow 220ms ease;
}

.orbit-item:hover .orbit-card{
  transform: translateY(-3px) scale(1.03);
  box-shadow: 0 22px 70px rgba(0,0,0,.18);
}

.orbit-img{
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.orbit-fallback{
  font-size: 12px;
  color: rgba(var(--muted), .95);
}

@keyframes orbitSpin{
  from{ transform: rotateX(var(--orbit-tilt)) rotateY(0deg); }
  to{ transform: rotateX(var(--orbit-tilt)) rotateY(360deg); }
}

@media (prefers-reduced-motion: reduce){
  .orbit-ring{ animation: none; }
}

@media (max-width: 640px){
  .orbit-scene{ height: 190px; }
  .orbit-card{ width: 66px; height: 66px; }
}
</style>
