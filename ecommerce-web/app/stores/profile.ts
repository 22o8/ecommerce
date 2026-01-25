import { defineStore } from 'pinia'
import { computed } from 'vue'

export const useProfileStore = defineStore('profile', () => {
  const phoneCookie = useCookie<string>('customer_phone', {
    default: () => '',
    path: '/',
    sameSite: 'lax',
    secure: process.env.NODE_ENV === 'production',
  })

  const phone = computed(() => phoneCookie.value || '')

  const setPhone = (p: string) => {
    phoneCookie.value = (p || '').trim()
  }

  return { phone, setPhone }
})
