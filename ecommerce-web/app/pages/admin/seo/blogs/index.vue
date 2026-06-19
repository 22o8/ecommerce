<script setup lang="ts">
import { blogPosts } from '~/data/blogPosts'
import { iraqSeoKeywords } from '~/data/iraqSeo'

definePageMeta({ layout: 'admin', middleware: 'admin' })

const draft = reactive({
  title: '',
  slug: '',
  metaTitle: '',
  metaDescription: '',
  keywords: '',
  content: '',
  faqQuestion: '',
  faqAnswer: '',
})
const localDrafts = ref<any[]>([])

function slugify(value: string) {
  return String(value || '').trim().toLowerCase().replace(/['’]/g, '').replace(/[^a-z0-9\u0600-\u06FF]+/g, '-').replace(/^-+|-+$/g, '')
}
function generateBlogDraft(topic?: string) {
  const title = topic || draft.title || 'أفضل منتجات كورية أصلية في العراق'
  draft.title = title
  draft.slug = slugify(title)
  draft.metaTitle = `${title} | DR SEOUL BEAUTY`
  draft.metaDescription = `دليل SEO مخصص عن ${title} مع استهداف السوق العراقي وكلمات Korean skincare Iraq ومنتجات كورية أصلية العراق.`.slice(0, 155)
  draft.keywords = [title, ...iraqSeoKeywords, 'بغداد', 'البصرة', 'أربيل'].join(', ')
  draft.content = `اكتب هنا مقالاً لا يقل عن 800 كلمة حول ${title}. ابدأ بمقدمة عن المشكلة، ثم اشرح أفضل المكونات، طريقة الاختيار، أخطاء شائعة، ثم اربط المنتجات المناسبة داخل DR SEOUL BEAUTY.`
  draft.faqQuestion = `ما أهمية ${title}؟`
  draft.faqAnswer = `يساعد هذا الموضوع المستخدم العراقي على اختيار منتجات كورية أصلية مناسبة حسب نوع البشرة والمدينة.`
}
function saveDraft() {
  if (!import.meta.client) return
  const item = { ...draft, createdAt: new Date().toISOString() }
  localDrafts.value = [item, ...localDrafts.value].slice(0, 20)
  localStorage.setItem('drseo_blog_drafts', JSON.stringify(localDrafts.value))
}
function loadDrafts() {
  if (!import.meta.client) return
  try { localDrafts.value = JSON.parse(localStorage.getItem('drseo_blog_drafts') || '[]') } catch { localDrafts.value = [] }
}
onMounted(loadDrafts)
</script>

<template>
  <div class="grid gap-6">
    <section class="admin-card p-6">
      <h1 class="text-2xl font-extrabold">Blog SEO Manager</h1>
      <p class="mt-1 text-sm text-muted">إدارة أفكار المقالات المستهدفة للعراق. النسخة الحالية تحفظ المسودات داخل المتصفح، والمقالات الثابتة موجودة داخل <code>app/data/blogPosts.ts</code>.</p>
    </section>

    <section class="admin-card p-6">
      <h2 class="text-xl font-extrabold">مولد مسودة مقال</h2>
      <div class="mt-4 flex flex-wrap gap-2">
        <button class="admin-secondary" @click="generateBlogDraft('أفضل تونر كوري في العراق')">أفضل تونر كوري</button>
        <button class="admin-secondary" @click="generateBlogDraft('أفضل سيروم كولاجين في بغداد')">سيروم كولاجين بغداد</button>
        <button class="admin-secondary" @click="generateBlogDraft('ANUA أم COSRX في العراق')">ANUA vs COSRX</button>
        <button class="admin-secondary" @click="generateBlogDraft('روتين العناية الكوري للبشرة الدهنية في العراق')">روتين البشرة الدهنية</button>
      </div>
      <div class="mt-5 grid gap-3 md:grid-cols-2">
        <input v-model="draft.title" class="admin-input" placeholder="عنوان المقال" @change="draft.slug = slugify(draft.title)" />
        <input v-model="draft.slug" class="admin-input" placeholder="slug" />
        <input v-model="draft.metaTitle" class="admin-input" placeholder="Meta Title" />
        <input v-model="draft.metaDescription" class="admin-input" placeholder="Meta Description" />
        <textarea v-model="draft.keywords" class="admin-input md:col-span-2" placeholder="Keywords"></textarea>
        <textarea v-model="draft.content" class="admin-input min-h-40 md:col-span-2" placeholder="Content outline"></textarea>
        <input v-model="draft.faqQuestion" class="admin-input" placeholder="FAQ Question" />
        <input v-model="draft.faqAnswer" class="admin-input" placeholder="FAQ Answer" />
      </div>
      <div class="mt-4 flex gap-2">
        <button class="admin-primary" @click="generateBlogDraft()">Generate Blog SEO</button>
        <button class="admin-secondary" @click="saveDraft">حفظ المسودة</button>
      </div>
    </section>

    <section class="grid gap-4 xl:grid-cols-2">
      <div class="admin-card p-6">
        <h2 class="text-xl font-extrabold">المقالات المنشورة حالياً</h2>
        <div class="mt-4 grid gap-3">
          <NuxtLink v-for="post in blogPosts" :key="post.slug" :to="`/blog/${post.slug}`" target="_blank" class="rounded-2xl border border-app p-4 hover:bg-surface-2">
            <b>{{ post.title }}</b>
            <p class="mt-1 text-xs text-muted">/blog/{{ post.slug }}</p>
            <p class="mt-2 text-sm text-muted">{{ post.metaDescription }}</p>
          </NuxtLink>
        </div>
      </div>
      <div class="admin-card p-6">
        <h2 class="text-xl font-extrabold">المسودات المحفوظة</h2>
        <div v-if="!localDrafts.length" class="mt-4 text-sm text-muted">لا توجد مسودات بعد.</div>
        <div v-for="item in localDrafts" :key="item.createdAt" class="mt-3 rounded-2xl border border-app p-4">
          <b>{{ item.title }}</b>
          <p class="mt-1 text-xs text-muted">/blog/{{ item.slug }}</p>
          <p class="mt-2 text-sm text-muted">{{ item.metaDescription }}</p>
        </div>
      </div>
    </section>
  </div>
</template>
