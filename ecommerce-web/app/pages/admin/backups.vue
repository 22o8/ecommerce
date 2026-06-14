<template>
  <div class="w-full">
    <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold">النسخ الاحتياطي</h1>
        <p class="text-sm text-white/70">
          نزّل نسخة كاملة من بيانات المتجر مع الصور والملفات المرفوعة حتى تستطيع استرجاعها مستقبلاً.
        </p>
      </div>
      <UiButton variant="ghost" @click="navigateTo('/admin/products')">رجوع للمنتجات</UiButton>
    </div>

    <div class="mt-6 grid gap-4 lg:grid-cols-2">
      <UiCard>
        <UiCardHeader>
          <UiCardTitle>نسخة كاملة</UiCardTitle>
          <UiCardDescription>
            تحتوي على المنتجات، الصور، البراندات، التصنيفات، الطلبات، الفواتير، الإعلانات، التقييمات، والكوبونات.
          </UiCardDescription>
        </UiCardHeader>
        <UiCardContent>
          <div class="grid gap-3 text-sm text-white/75">
            <p>
              الملف يكون بصيغة ZIP، وبداخله ملفات JSON للبيانات ومجلد media للصور والملفات التي يمكن تنزيلها من روابطها العامة.
            </p>
            <p class="rounded-2xl border border-amber-300/20 bg-amber-300/10 p-3 text-amber-100">
              إذا كان حجم الصور كبيراً جداً قد يستغرق التحميل وقتاً. لا تغلق الصفحة حتى يبدأ التحميل.
            </p>

            <div class="flex flex-wrap gap-2 pt-2">
              <UiButton :disabled="downloading" @click="downloadBackup(true)">
                {{ downloading ? 'جاري تجهيز النسخة...' : 'تنزيل نسخة كاملة مع الصور' }}
              </UiButton>
              <UiButton variant="secondary" :disabled="downloading" @click="downloadBackup(false)">
                تنزيل بيانات فقط
              </UiButton>
            </div>
          </div>
        </UiCardContent>
      </UiCard>

      <UiCard>
        <UiCardHeader>
          <UiCardTitle>ملاحظات الاسترجاع</UiCardTitle>
        </UiCardHeader>
        <UiCardContent>
          <ul class="list-disc space-y-2 ps-5 text-sm text-white/75">
            <li>احتفظ بالنسخة على جهازك وعلى Google Drive أو هارد خارجي.</li>
            <li>النسخة لا تغيّر أي شيء في الموقع؛ هي تنزيل فقط.</li>
            <li>يفضل تنزيل نسخة قبل أي تعديل كبير على المنتجات أو التصنيفات.</li>
            <li>إذا احتجت استرجاع النسخة لاحقاً، استخدم ملفات JSON مع قاعدة البيانات أو أرسلها للمطور.</li>
          </ul>
        </UiCardContent>
      </UiCard>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

const downloading = ref(false)

function downloadBackup(includeMedia: boolean) {
  downloading.value = true
  const url = `/api/bff/admin/backups/full?includeMedia=${includeMedia ? 'true' : 'false'}`
  const a = document.createElement('a')
  a.href = url
  a.download = ''
  document.body.appendChild(a)
  a.click()
  a.remove()
  window.setTimeout(() => {
    downloading.value = false
  }, 5000)
}
</script>
