<template>
  <div class="mx-auto max-w-7xl px-3 sm:px-4 py-6 space-y-4">
    <div class="card-soft p-4 flex items-center justify-between gap-3">
      <div>
        <div class="text-xl font-extrabold rtl-text">مفضلاتي</div>
        <div class="text-sm text-muted rtl-text">المنتجات التي أضفتها إلى المفضلة (محفوظة على حسابك)</div>
      </div>
      <div class="keep-ltr text-sm text-muted">{{ fav.count }} items</div>
    </div>

    <div v-if="!auth.isAuthed" class="card-soft p-6 text-center">
      <div class="text-lg font-extrabold rtl-text">سجّل الدخول لعرض المفضلة</div>
      <div class="text-sm text-muted rtl-text mt-1">المفضلة مرتبطة بالحساب وتظهر على أي جهاز بعد تسجيل الدخول.</div>
      <NuxtLink to="/login" class="btn-primary inline-flex mt-4 px-5 py-2 rounded-2xl">تسجيل الدخول</NuxtLink>
    </div>

    <div v-else-if="fav.loading" class="card-soft p-4 text-muted rtl-text">جاري التحميل...</div>

    <div v-else-if="fav.items.length === 0" class="card-soft p-8 text-center">
      <div class="text-lg font-extrabold rtl-text">ما عندك مفضلات بعد</div>
      <div class="text-sm text-muted rtl-text mt-1">افتح أي منتج واضغط ❤️ لإضافته.</div>
      <NuxtLink to="/products" class="btn-soft inline-flex mt-4 px-5 py-2 rounded-2xl">تصفح المنتجات</NuxtLink>
    </div>

    <div v-else class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <ProductCard v-for="p in fav.items" :key="p.id" :p="p" />
    </div>
  </div>
</template>

<script setup lang="ts">
import ProductCard from '~/components/ProductCard.vue'
import { useFavoritesStore } from '~/stores/favorites'

const auth = useAuthStore()
const fav = useFavoritesStore()

watch(
  () => auth.isAuthed,
  (v) => { if (v) fav.refresh() },
  { immediate: true }
)
</script>
