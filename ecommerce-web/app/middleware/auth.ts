export default defineNuxtRouteMiddleware((to) => {
  const route = (to as any) ?? (typeof useRoute === "function" ? useRoute() : null)

  const token = useCookie<string | null>("token").value
  if (!token) {
    const redirect = encodeURIComponent(route?.fullPath || "/")
    return navigateTo(`/login?redirect=${redirect}`)
  }
})
