// ecommerce-web/app/middleware/auth.ts
export default defineNuxtRouteMiddleware((to) => {
  const route = (to as any) ?? (typeof useRoute === "function" ? useRoute() : null)

  // SSR يگدر يقرا token (حتى لو HttpOnly)
  const token = useCookie<string | null>("token").value

  // بالـ client ما يگدر يقراه إذا HttpOnly -> نعتمد على auth/role
  const auth = useCookie<string | null>("auth").value
  const role = useCookie<string | null>("role").value

  const isLoggedIn = !!token || auth === "1" || !!role

  if (!isLoggedIn) {
    const redirect = encodeURIComponent(route?.fullPath || "/")
    return navigateTo(`/login?redirect=${redirect}`)
  }
})
