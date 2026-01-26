function parseJwt(token: string) {
  try {
    const base64Url = token.split(".")[1]
    if (!base64Url) return null

    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/")

    // SSR/Client safe decode
    const bin =
      typeof atob === "function"
        ? atob(base64)
        : (globalThis as any)?.Buffer
          ? (globalThis as any).Buffer.from(base64, "base64").toString("binary")
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
  // ✅ Guard: لا تستخدم path إذا مو موجود
  const path = (to?.path || "").toLowerCase()
  if (!path.startsWith("/admin")) return

  // ✅ إذا ماكو توكن -> لوجن + redirect
  const token = useCookie<string | null>("token").value
  if (!token) {
    return navigateTo(
      `/login?redirect=${encodeURIComponent(to.fullPath || "/admin")}`
    )
  }

  // ✅ تحقق من صلاحية الادمن
  const payload = parseJwt(token)
  const rawRole =
    payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
    payload?.role

  const role = Array.isArray(rawRole) ? rawRole[0] : rawRole
  if (String(role ?? "").toLowerCase() !== "admin") {
    return navigateTo("/")
  }
})
