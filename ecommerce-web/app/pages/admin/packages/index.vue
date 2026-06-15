<template>
  <div class="grid gap-6">
    <div>
      <h1 class="text-2xl font-black rtl-text">البكچات - خطة العمل المقترحة</h1>
      <p class="text-muted rtl-text mt-1">هذه صفحة تخطيط جاهزة داخل لوحة التحكم حتى نحدد نظام البكچات قبل بناء قاعدة البيانات الخاصة به.</p>
    </div>

    <div class="grid gap-4 md:grid-cols-2">
      <div class="card-soft p-6">
        <h2 class="font-black text-xl rtl-text">شنو هي البكچات؟</h2>
        <p class="mt-3 text-muted rtl-text leading-8">البكچ هو مجموعة منتجات تُباع كسلة واحدة بسعر خاص، مثل بكچ تفتيح، بكچ حب الشباب، أو روتين عناية كامل. الهدف منه زيادة قيمة الطلب وتسهيل الاختيار على الزبون.</p>
      </div>
      <div class="card-soft p-6">
        <h2 class="font-black text-xl rtl-text">ماذا نحتاج حتى نبنيه؟</h2>
        <ul class="mt-3 grid gap-2 text-muted rtl-text leading-8 list-disc pe-5">
          <li>اسم البكچ بالعربي والإنكليزي.</li>
          <li>صورة أو فيديو للبكچ.</li>
          <li>اختيار المنتجات الداخلة بالبكچ مع الكمية لكل منتج.</li>
          <li>سعر البكچ النهائي أو خصم تلقائي.</li>
          <li>مخزون البكچ يُحسب من أقل منتج متوفر داخل المجموعة.</li>
          <li>ربط البكچ بتصنيف أو مشكلة مثل ترطيب، مسامات، حب شباب.</li>
        </ul>
      </div>
    </div>

    <div class="card-soft p-6">
      <h2 class="font-black text-xl rtl-text">الحقول المقترحة عند إنشاء بكچ</h2>
      <div class="mt-4 grid gap-3 md:grid-cols-3">
        <div v-for="field in fields" :key="field" class="rounded-3xl border border-app bg-surface p-4 rtl-text">{{ field }}</div>
      </div>
    </div>

    <div class="card-soft p-6">
      <h2 class="font-black text-xl rtl-text">منطق البيع والأرباح</h2>
      <ol class="mt-3 grid gap-2 text-muted rtl-text leading-8 list-decimal pe-5">
        <li>عند طلب البكچ تبقى الفاتورة قيد التنفيذ ولا تنقص الكمية مباشرة.</li>
        <li>عند ضغط تم البيع تنقص كميات كل المنتجات داخل البكچ.</li>
        <li>الأرباح تُحسب من سعر البكچ ناقص تكلفة المنتجات + التوصيل.</li>
        <li>النقاط تُمنح للعميل حسب قيمة البكچ بعد تأكيد البيع.</li>
      </ol>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin' })
const fields = [
  'اسم البكچ', 'Slug للرابط', 'وصف قصير', 'وصف طويل SEO', 'صورة الغلاف', 'منتجات البكچ', 'كمية كل منتج', 'السعر قبل الخصم', 'السعر بعد الخصم', 'التصنيف', 'تصنيف حل المشكلة', 'مفعل/غير مفعل', 'مميز', 'ترتيب العرض', 'FAQ للبكچ'
]
</script>
