<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })
const competitors = ref<any[]>([
  { name: 'YesStyle', url: 'https://www.yesstyle.com' },
  { name: 'StyleKorean', url: 'https://www.stylekorean.com' },
  { name: 'Stylevana', url: 'https://www.stylevana.com' },
  { name: 'Olive Young', url: 'https://global.oliveyoung.com' },
])
const newItem = reactive({ name: '', url: '' })
const report = computed(() => competitors.value.map(c => ({
  ...c,
  opportunities: ['اكتب مقالات مقارنة للبراندات الأكثر بحثاً', 'أضف FAQ لكل منتج حسب المشكلة الجلدية', 'قوّي الربط الداخلي بين التصنيفات والمنتجات'],
  missingKeywords: ['korean skincare iraq', 'best korean toner', 'acne serum korea', 'sunscreen korean'],
  blogIdeas: [`أفضل منتجات من ${c.name}`, `${c.name} بدائل متوفرة في العراق`, `مقارنة ${c.name} مع DR SEOUL BEAUTY`],
})))
function addCompetitor() {
  if (!newItem.name || !newItem.url) return
  competitors.value.push({ name: newItem.name, url: newItem.url })
  newItem.name = ''
  newItem.url = ''
}
</script>

<template>
  <div class="grid gap-6">
    <section class="admin-card p-6">
      <h1 class="text-2xl font-extrabold">Competitor Analyzer</h1>
      <p class="mt-1 text-sm text-muted">تحليل فرص SEO مقابل المنافسين. التحليل الحالي آمن وداخلي، ويمكن ربطه لاحقاً بخدمة Crawling خارجية.</p>
      <div class="mt-5 grid gap-3 md:grid-cols-[1fr_1fr_auto]">
        <input v-model="newItem.name" class="admin-input" placeholder="اسم المنافس" />
        <input v-model="newItem.url" class="admin-input" placeholder="رابط المنافس" />
        <button class="admin-primary" @click="addCompetitor">إضافة</button>
      </div>
    </section>

    <section class="grid gap-4">
      <div v-for="item in report" :key="item.url" class="admin-card p-5">
        <div class="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 class="text-lg font-extrabold">{{ item.name }}</h2>
            <p class="text-xs text-muted keep-ltr">{{ item.url }}</p>
          </div>
          <span class="rounded-full bg-primary/10 px-3 py-1 text-xs text-primary">SEO Opportunities</span>
        </div>
        <div class="mt-4 grid gap-4 md:grid-cols-3">
          <div><b>Missing Keywords</b><ul class="mt-2 list-disc ps-5 text-sm text-muted"><li v-for="x in item.missingKeywords" :key="x">{{ x }}</li></ul></div>
          <div><b>Content Gaps</b><ul class="mt-2 list-disc ps-5 text-sm text-muted"><li v-for="x in item.opportunities" :key="x">{{ x }}</li></ul></div>
          <div><b>Blog Ideas</b><ul class="mt-2 list-disc ps-5 text-sm text-muted"><li v-for="x in item.blogIdeas" :key="x">{{ x }}</li></ul></div>
        </div>
      </div>
    </section>
  </div>
</template>
