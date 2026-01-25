import { defineStore } from 'pinia'
import { computed } from 'vue'

export type CartItem = {
  id: string
  slug?: string
  title: string
  priceUsd: number
  image?: string
  qty: number
}

export const useCartStore = defineStore('cart', () => {
  // ✅ نخزن السلة بكوكي (JSON) حتى تبقى بعد التحديث/إغلاق المتصفح
  const itemsCookie = useCookie<CartItem[]>('cart', {
    default: () => [],
    path: '/',
    sameSite: 'lax',
    secure: process.env.NODE_ENV === 'production',
  })

  const items = computed(() => itemsCookie.value ?? [])

  const count = computed(() => items.value.reduce((sum, it) => sum + (it.qty || 0), 0))
  const totalUsd = computed(() => items.value.reduce((sum, it) => sum + (it.priceUsd || 0) * (it.qty || 0), 0))

  function add(payload: Omit<CartItem, 'qty'>, qty = 1) {
    const list = [...items.value]
    const idx = list.findIndex(x => x.id === payload.id)
    if (idx >= 0) {
      list[idx] = { ...list[idx], qty: (list[idx].qty || 0) + qty }
    } else {
      list.push({ ...payload, qty })
    }
    itemsCookie.value = list
  }

  function setQty(id: string, qty: number) {
    const q = Math.max(1, Math.floor(qty || 1))
    const list = items.value.map(it => (it.id === id ? { ...it, qty: q } : it))
    itemsCookie.value = list
  }

  function remove(id: string) {
    itemsCookie.value = items.value.filter(it => it.id !== id)
  }

  function clear() {
    itemsCookie.value = []
  }

  return { items, count, totalUsd, add, setQty, remove, clear }
})
