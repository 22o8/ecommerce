<!-- app/components/ProductCard.vue -->
<script setup lang="ts">
import { computed } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'

const props = defineProps<{ item: any; compact?: boolean }>()
const api = useApi()
const { t } = useI18n()

// ProductsController يرجّع coverImage جاهز. نخليه المصدر الوحيد للـ thumbnail
const thumb = computed(() => {
  const raw = props.item?.coverImage || props.item?.image || props.item?.thumbnail
  return api.buildAssetUrl(String(raw || ''))
})

const price = computed(() => {
  const n = Number(props.item?.priceUsd || 0)
  return Number.isFinite(n) ? n : 0
})
</script>

<template>
  <article class="cardx group overflow-hidden" :class="props.compact ? 'compact' : ''">
    <!-- Thumbnail -->
    <NuxtLink :to="`/products/${props.item.slug}`" class="block" :class="props.compact ? 'compact-thumb' : ''">
      <div class="thumb">
        <template v-if="thumb">
          <img
            :src="thumb"
            :alt="props.item.title"
            class="thumb-img"
            loading="lazy"
          />
        </template>
        <template v-else>
          <div class="thumb-fallback">
            <div class="text-xs opacity-70">{{ t('noImage') }}</div>
          </div>
        </template>

        <div class="thumb-overlay"></div>

        <div class="price-badge">
          ${{ price }}
        </div>
      </div>
    </NuxtLink>

    <!-- Content -->
    <div class="p-4" :class="props.compact ? 'compact-body' : ''">
      <h3 class="title line-clamp-1">{{ props.item.title }}</h3>
      <p class="desc line-clamp-2">{{ props.item.description || t('dash') }}</p>

      <div class="mt-4 flex items-center justify-between gap-2">
        <NuxtLink
          class="text-sm font-extrabold"
          :style="{ color: 'rgb(var(--primary))' }"
          :to="`/products/${props.item.slug}`"
        >
          {{ t('productDetails') }}
        </NuxtLink>

        <NuxtLink :to="`/products/${props.item.slug}`">
          <AppButton variant="soft">{{ t('buy') }}</AppButton>
        </NuxtLink>
      </div>
    </div>
  </article>
</template>

<style scoped>
.cardx{
  border-radius: 24px;
  border: 1px solid rgb(var(--border));
  background: rgb(var(--panel));
  box-shadow: var(--shadow2);
  transition: transform .18s ease, border-color .18s ease, box-shadow .18s ease;
}
.cardx:hover{
  transform: translateY(-4px) scale(1.01);
  border-color: rgba(var(--primary), .35);
  box-shadow: 0 18px 50px rgba(0,0,0,.12);
}

.thumb{
  position: relative;
  aspect-ratio: 1 / 1;
  overflow: hidden;
  background: rgb(var(--bg2));
}
.thumb-img{
  width: 100%;
  height: 100%;
  object-fit: cover;
  transform: scale(1);
  transition: transform .25s ease;
}
.cardx:hover .thumb-img{ transform: scale(1.08); }

.thumb-fallback{
  width: 100%;
  height: 100%;
  display: grid;
  place-items: center;
  color: rgb(var(--muted));
}

.thumb-overlay{
  position:absolute;
  inset:0;
  background: linear-gradient(180deg, rgba(0,0,0,0) 50%, rgba(0,0,0,.14) 100%);
  opacity: .9;
}

.price-badge{
  position:absolute;
  top: 12px;
  right: 12px;
  padding: 8px 10px;
  border-radius: 999px;
  border: 1px solid rgba(0,0,0,.08);
  background: rgba(255,255,255,.85);
  backdrop-filter: blur(10px);
  font-weight: 900;
  font-size: 12px;
  color: rgb(var(--text-strong));
}

.title{
  font-weight: 900;
  font-size: 16px;
}
.desc{
  margin-top: 6px;
  font-size: 13px;
  color: rgb(var(--muted));
}

/* List / compact view */
.compact{
  display: flex;
  align-items: stretch;
}
.compact-thumb{
  width: 180px;
  flex: 0 0 180px;
}
.compact .thumb{ aspect-ratio: 1 / 1; height: 100%; }
.compact-body{ flex: 1; }

@media (max-width: 640px){
  .compact{ display: block; }
  .compact-thumb{ width: auto; }
}
</style>
