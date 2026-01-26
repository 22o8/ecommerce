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
  // ✅ SSR-safe route
  const route = to ?? useRoute?.()
  const path = String(route?.path ?? "").toLowerCase()

  if (!path.startsWith("/admin")) return

  const token = useCookie<string | null>("token").value
  if (!token) {
    const redirect = encodeURIComponent(route?.fullPath || "/admin")
    return navigateTo(`/login?redirect=${redirect}`)
  }

  const payload = parseJwt(token)
  const rawRole =
    payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
    payload?.role

  const role = Array.isArray(rawRole) ? rawRole[0] : rawRole
  if (String(role).toLowerCase() !== "admin") {
    return navigateTo("/")
  }
})
