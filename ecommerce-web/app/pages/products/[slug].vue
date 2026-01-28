<script setup lang="ts">
// تم إلغاء صفحة تفاصيل المنتج واستبدالها بـ "Quick Preview" داخل صفحة /products
// أي رابط قديم مثل /products/:id سيتم تحويله إلى /products?p=:id

const route = useRoute()

// نحافظ على نفس المعامل المستخدم في صفحة المنتجات
const id = String(route.params.slug || '')

// تحويل مباشر (SSR-friendly)
await navigateTo(
  {
    path: '/products',
    query: id ? { p: id } : undefined
  },
  { replace: true }
)
</script>

<template>
  <!-- لا شيء هنا لأننا نعمل Redirect -->
  <div />
</template>
