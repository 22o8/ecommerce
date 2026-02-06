<script setup lang="ts">
import { computed } from 'vue'
import { useApi } from '~/composables/useApi'

type Brand = {
  id: string
  name: string
  slug: string
  logoUrl?: string | null
}

const props = defineProps<{ brands: Brand[] }>()

const api = useApi()

// نكرر القائمة حتى يصير تمرير مستمر
const loop = computed(() => [...props.brands, ...props.brands])
</script>

<template>
  <section v-if="brands?.length" class="mt-16">
    <div class="mx-auto max-w-6xl px-4">
      <div class="text-center">
        <h2 class="text-2xl md:text-3xl font-extrabold">Our Brands</h2>
        <p class="mt-2 text-muted">Explore premium beauty brands</p>
      </div>
    </div>

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
                  :src="api.buildAssetUrl(b.logoUrl)"
                  :alt="b.name"
                  class="h-10 w-10 rounded-xl object-cover"
                  loading="lazy"
                />
                <div v-else class="h-10 w-10 rounded-xl bg-black/5" />
              </div>
              <span class="marquee__name">{{ b.name }}</span>
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
  display:flex;
  align-items:center;
  gap: 10px;
  padding: 12px 14px;
  border-radius: 999px;
  border: 1px solid rgba(var(--border), .9);
  background: rgb(var(--surface));
  box-shadow: 0 12px 40px rgba(0,0,0,.06);
  transition: transform .18s ease, box-shadow .18s ease;
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
  from{ transform: translateX(0); }
  to{ transform: translateX(-50%); }
}

/* احترام تفضيل تقليل الحركة */
@media (prefers-reduced-motion: reduce){
  .marquee__track{ animation: none; }
}
</style>
