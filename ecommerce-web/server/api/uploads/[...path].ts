import { defineEventHandler, getRequestHeader, sendProxy, createError } from "h3"

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()

  const apiOriginRaw =
    (config as any).apiOrigin ||
    (config.public as any).apiOrigin ||
    process.env.NUXT_PUBLIC_API_ORIGIN ||
    process.env.NUXT_API_ORIGIN

  const apiOrigin = String(apiOriginRaw || "").replace(/\/+$/, "")

  if (!apiOrigin) {
    throw createError({
      statusCode: 500,
      statusMessage:
        "Missing API origin. Set runtimeConfig.public.apiOrigin (NUXT_PUBLIC_API_ORIGIN).",
    })
  }

  const p = (event.context.params as any)?.path
  const rest = Array.isArray(p) ? p.join("/") : String(p || "")
  const target = `${apiOrigin}/uploads/${rest}`

  // مرّر الكوكيز لو موجودة (مو ضرورية للصور غالباً، بس مفيدة)
  const headers: Record<string, string> = {}
  const cookie = getRequestHeader(event, "cookie")
  if (cookie) headers.cookie = cookie

  return sendProxy(event, target, { headers })
})
