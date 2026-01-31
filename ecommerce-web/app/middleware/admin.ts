// ecommerce-web/app/middleware/admin.ts
function parseJwt(token: string) {
  try {
    const part = token.split(".")[1]
    if (!part) return null

    const base64 = part.replace(/-/g, "+").replace(/_/g, "/")

    const bin =
      typeof atob === "function"
        ? atob(base64)
        : typeof Buffer !== "undefined"
          ? Buffer.from(base64, "base64").toString("binary")
          : ""

    if (!bin) return null

    const json = decodeURIComponent(
      bin
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    )

    return JSON.parse(json)
  } catch {
    return null
  }
}

export default defineNuxtRouteMiddleware((to) => {
  const route = (to as any) ?? (typeof useRoute === "function" ? useRoute() : null)
  const path = String(route?.path ?? "").toLowerCase()
  if (path && !path.startsWith("/admin")) return

  const token = useCookie<string | null>("token").value
  const roleCookie = (useCookie<string | null>("role").value || "").toLowerCase()
  const auth = useCookie<string | null>("auth").value

  // إذا ماكو لا token (SSR) ولا role/auth (client) -> روح login
  if (!token && auth !== "1" && !roleCookie) {
    const redirect = encodeURIComponent(route?.fullPath || "/admin")
    return navigateTo(`/login?redirect=${redirect}`)
  }

  // SSR: تحقق من JWT
  if (token) {
    const payload = parseJwt(token)
    const rawRole =
      payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
      payload?.role

    const role = (Array.isArray(rawRole) ? rawRole[0] : rawRole || "").toString().toLowerCase()
    if (role !== "admin") return navigateTo("/")
    return
  }

  // Client: تحقق من role cookie (غير httpOnly)
  if (roleCookie !== "admin") return navigateTo("/")
})
