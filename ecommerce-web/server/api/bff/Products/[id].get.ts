import { setResponseStatus } from 'h3'

// Endpoint بديل للـ API في حال ما موجود GET /Products/{id} بالباك.
// يرجع المنتج عن طريق قائمة المنتجات.

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const apiBase = (config.public.apiBase || '').replace(/\/$/, '')
  const id = String(event.context.params?.id || '')

  if (!id) {
    setResponseStatus(event, 400)
    return { message: 'Missing id' }
  }

  // جرّب مباشرة لو موجود بالباك
  try {
    return await $fetch(`${apiBase}/Products/${encodeURIComponent(id)}`)
  } catch {}

  // fallback: من اللست
  try {
    const list: any = await $fetch(`${apiBase}/Products`, {
      query: { page: 1, take: 200 },
    })
    const items = list?.items || list?.data?.items || list || []
    const found = Array.isArray(items) ? items.find((p: any) => String(p.id) === id) : null
    if (found) return found
  } catch {}

  setResponseStatus(event, 404)
  return { message: 'Not Found' }
})
