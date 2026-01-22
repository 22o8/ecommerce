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
  // ملاحظة: نستخدم take كبير قدر الإمكان حتى نلقط المنتج من أول صفحة.
  try {
    const list: any = await $fetch(`${apiBase}/Products`, {
      query: { page: 1, take: 200 },
    })
    const items = list?.items || list?.data?.items || list || []
    const found = Array.isArray(items) ? items.find((p: any) => String(p.id) === slug || String(p.slug) === slug) : null
    if (found) return found
  } catch (e: any) {
    // ignore
  }

  setResponseStatus(event, 404)
  return { message: 'Not Found' }
})
