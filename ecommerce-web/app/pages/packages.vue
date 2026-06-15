<template>
  <div class="grid gap-6">
    <section class="card-soft p-6 md:p-10 text-center">
      <div class="mx-auto h-14 w-14 rounded-3xl bg-[rgb(var(--primary))]/15 grid place-items-center">
        <Icon name="mdi:package-variant-closed" class="text-3xl text-[rgb(var(--primary))]" />
      </div>
      <h1 class="mt-4 text-3xl md:text-5xl font-black rtl-text">بكچات العناية الكورية</h1>
      <p class="mt-3 text-muted rtl-text max-w-2xl mx-auto">قريباً ستجد هنا روتينات جاهزة للعناية بالبشرة: تفتيح، ترطيب، حب الشباب، المسامات، والكولاجين.</p>
    </section>
    <section class="grid gap-4 md:grid-cols-3">
      <div v-for="item in ideas" :key="item.title" class="card-soft p-5">
        <h2 class="font-black rtl-text">{{ item.title }}</h2>
        <p class="mt-2 text-sm text-muted rtl-text leading-7">{{ item.desc }}</p>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
const ideas = [
  { title: 'بكچ التفتيح', desc: 'منتجات تساعد على الإشراقة وتوحيد لون البشرة.' },
  { title: 'بكچ حب الشباب', desc: 'روتين مخصص للبشرة الدهنية والمعرضة للحبوب.' },
  { title: 'بكچ الترطيب', desc: 'اختيارات مناسبة لدعم حاجز البشرة وتقليل الجفاف.' },
]
</script>
