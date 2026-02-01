// يزامن حالة Pinia مع الـ Cookies بعد الريفرش حتى يبقى المستخدم مسجل دخول
export default defineNuxtPlugin(() => {
  const auth = useAuthStore()
  auth.initFromCookies()
})
