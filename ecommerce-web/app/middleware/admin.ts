// ecommerce-web/app/middleware/admin.ts
export default defineNuxtRouteMiddleware((to) => {
  const path = String(to?.path || '').toLowerCase()
  if (!path.startsWith('/admin')) return

  const authStore = useAuthStore()
  authStore.initFromCookies()

  // ✅ نعتمد على role/auth/user (غير HttpOnly)
  const roleCookie = String(useCookie<string | null>('role').value || '').trim().toLowerCase()
  const authFlag = String(useCookie<string | null>('auth').value || '').trim()

  const userCookie = useCookie<any>('user').value
  let userRole = ''
  if (userCookie) {
    if (typeof userCookie === 'string') {
      try { userRole = (JSON.parse(userCookie)?.role || '').toString().trim().toLowerCase() } catch {}
    } else {
      userRole = (userCookie?.role || '').toString().trim().toLowerCase()
    }
  }

  const finalRole = (authStore.userData?.role || userRole || roleCookie || '').toString().trim().toLowerCase()

  // إذا ما مسجل دخول أصلاً
  if (authFlag !== '1' && !finalRole) {
    const redirect = encodeURIComponent(to.fullPath || '/admin')
    return navigateTo(`/login?redirect=${redirect}`)
  }

  // إذا مو أدمن
  if (finalRole !== 'admin') {
    return navigateTo('/')
  }
})
