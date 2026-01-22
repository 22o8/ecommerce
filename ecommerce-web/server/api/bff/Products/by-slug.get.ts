import { getQuery, setResponseStatus } from 'h3'

// بعض نسخ الـ API ما توفر endpoint ثابت لـ (get product by id/slug).
// هذا الـ BFF يحاول يرجع المنتج بطريقة مرنة حتى ما يطيح الفرونت بـ 404.

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const apiBase = (config.public.apiBase || '').replace(/\/$/, '')

  const q = getQuery(event)
  const slug = String(q.slug || '')

  if (!slug) {
    setResponseStatus(event, 400)
    return { message: 'Missing slug' }
  }

  // 1) جرّب الـ API كما هو
  try {
    return await $fetch(`${apiBase}/Products/by-slug`, {
      query: { slug },
    })
  } catch (e: any) {
    // ignore and fallback
  }

  // 2) جرّب صيغة path param: /Products/by-slug/{slug}
  try {
    return await $fetch(`${apiBase}/Products/by-slug/${encodeURIComponent(slug)}`)
  } catch (e: any) {
    // ignore and fallback
  }

  // 3) fallback: جب قائمة منتجات وفلتر بالـ id/slug محلياً
  // ملاحظة: بعض الـ APIs تستخدم أسماء باراميترات مختلفة (page/take أو Page/Take أو pageNumber/pageSize)
  // فنجرّب أكثر من شكل ونكبر حجم الصفحة حتى نضمن نلقط المنتج.
  const candidates = [
    { page: 1, take: 500 },
    { Page: 1, Take: 500 },
    { pageNumber: 1, pageSize: 500 },
  ]

  const slugify = (input: string) =>
    String(input || '')
      .trim()
      .toLowerCase()
      .replace(/\s+/g, '-')
      .replace(/[^a-z0-9\-\u0600-\u06FF]/g, '')
      .replace(/-+/g, '-')
      .replace(/^-|-$/g, '')

  for (const q of candidates) {
    try {
      const list: any = await $fetch(`${apiBase}/Products`, { query: q })
      const items = list?.items || list?.data?.items || list || []
      if (!Array.isArray(items)) continue

      const found = items.find((p: any) => {
        const pid = String(p.id || '')
        const pslug = String(p.slug || '')
        const ptitle = String(p.title || p.name || '')
        return (
          pid === slug ||
          pslug === slug ||
          (ptitle && slugify(ptitle) === slug)
        )
      })

      if (found) return found
    } catch {
      // جرّب الشكل التالي
    }
  }

  setResponseStatus(event, 404)
  return { message: 'Not Found' }
})
