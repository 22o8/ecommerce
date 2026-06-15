<template>
  <div class="grid gap-6">
    <div class="card-soft p-6 md:p-10">
      <h1 class="text-2xl md:text-3xl font-black rtl-text">محفظة النقاط</h1>
      <p class="mt-1 text-muted rtl-text">تم نقل المحفظة إلى صفحة حسابي.</p>
      <NuxtLink to="/account" class="mt-4 inline-flex">
        <UiButton><Icon name="mdi:wallet-outline" /><span class="rtl-text">فتح محفظتي</span></UiButton>
      </NuxtLink>
    </div>
  </div>
</template>
<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
</script>
