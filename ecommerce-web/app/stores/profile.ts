import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import { useAuthStore } from './auth'

export const useProfileStore = defineStore('profile', () => {
  const auth = useAuthStore()

  const profile = ref<{ name?: string; email?: string; phone?: string }>({})

  const isLoggedIn = computed(() => !!auth.token)

  // Hydrate from token payload (best-effort) to keep UI stable even without a /me endpoint.
  function refreshFromToken() {
    const payload = auth.payload
    if (!payload) {
      profile.value = {}
      return
    }

    profile.value = {
      name: (payload as any)?.name ?? (payload as any)?.fullName,
      email: (payload as any)?.email,
      phone: (payload as any)?.phone,
    }
  }

  // Auto refresh whenever token changes
  watch(
    () => auth.token,
    () => refreshFromToken(),
    { immediate: true }
  )

  function setManual(data: Partial<{ name: string; email: string; phone: string }>) {
    profile.value = { ...profile.value, ...data }
  }

  // Backward-compatible: بعض الصفحات كانت تستدعيها.
  // الـ watcher أعلاه كافي، فهاي مجرد دالة فارغة حتى ما يصير كراش.
  function hydrateFromAuth() {
    return
  }

  return { profile, isLoggedIn, setManual, hydrateFromAuth }
})
