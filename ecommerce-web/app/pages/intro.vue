<template>
  <div class="intro-share-page min-h-[80vh] grid place-items-center px-4 py-16 text-center rtl-text">
    <div class="max-w-xl rounded-[2rem] border border-app bg-surface p-8 shadow-soft">
      <div class="mx-auto mb-4 grid h-16 w-16 place-items-center rounded-3xl bg-[rgba(var(--primary),0.12)] text-[rgb(var(--primary))]">
        <Icon name="mdi:movie-open-play-outline" class="text-3xl" />
      </div>
      <h1 class="text-3xl font-black text-[rgb(var(--text))]">استفتاحية المتجر</h1>
      <p class="mt-3 leading-8 text-[rgb(var(--muted))]">إذا كانت الاستفتاحية مفعلة أو محفوظة، ستظهر كواجهة كاملة فوق هذه الصفحة. يمكنك مشاركة هذا الرابط مع الزوار أو العملاء.</p>
      <NuxtLink to="/" class="admin-primary mt-6 inline-flex px-5 py-3">العودة للمتجر</NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })
</script>
