<template>
  <main class="mx-auto max-w-4xl px-4 py-10">
    <div class="mb-6">
      <NuxtLink to="/" class="inline-flex items-center gap-2 text-sm font-bold text-muted hover:text-[rgb(var(--primary))]">
        <Icon name="mdi:arrow-right" />
        <span>رجوع للرئيسية</span>
      </NuxtLink>
      <h1 class="mt-5 text-3xl font-black rtl-text sm:text-4xl">شارك لصديقك واربح نقاط</h1>
      <p class="mt-2 text-sm text-muted rtl-text">هذه صفحتك الخاصة بالمشاركة. انسخ الرابط أو الكود وأرسله لصديقك.</p>
    </div>

    <ReferralShareCard />
  </main>
</template>

<script setup lang="ts">
useHead({
  title: 'شارك لصديقك واربح نقاط | DR SEOUL BEAUTY',
  meta: [
    { name: 'robots', content: 'noindex,follow' },
    { name: 'description', content: 'رابط مشاركة خاص بزبائن DR SEOUL BEAUTY.' },
  ],
})
</script>
