import { setResponseStatus } from 'h3'

// صور المنتج للواجهة العامة. نحاول أكثر من endpoint بالباك.

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const apiBase = (config.public.apiBase || '').replace(/\/$/, '')
  const id = String(event.context.params?.id || '')

  if (!id) {
    setResponseStatus(event, 400)
    return { message: 'Missing id' }
  }

  // 1) endpoint عام (لو موجود)
  try {
    return await $fetch(`${apiBase}/Products/${encodeURIComponent(id)}/images`)
  } catch {}

  // 2) endpoint الادمن (بعض النسخ تكون public للقراءة)
  try {
    return await $fetch(`${apiBase}/admin/products/${encodeURIComponent(id)}/images`)
  } catch {}

  setResponseStatus(event, 404)
  return { message: 'Not Found' }
})
