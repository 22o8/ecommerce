function parseJwt(token: string) {
  try {
    const base64Url = token.split(".")[1]
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/")
    const json = decodeURIComponent(
      (typeof atob === "function" ? atob(base64) : Buffer.from(base64, "base64").toString("binary"))
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
  if (!to.path.toLowerCase().startsWith("/admin")) return

  const token = useCookie<string | null>("token").value
  if (!token) return navigateTo("/login")

  const payload = parseJwt(token)
  const rawRole =
    payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
    payload?.role

  // Role can be string or array; also case may differ
  const role = (Array.isArray(rawRole) ? rawRole[0] : rawRole)
  if (String(role ?? "").toLowerCase() !== "admin") return navigateTo("/")
})
