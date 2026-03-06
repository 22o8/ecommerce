import { defineStore } from 'pinia'

export type CartItem = {
  id: string
  title: string
  price: number
  imageUrl?: string | null
  quantity: number
}

export const useCartStore = defineStore('cart', () => {
  const items = useState<CartItem[]>('cart_items', () => [])

  const count = computed(() => items.value.reduce((sum, it) => sum + it.quantity, 0))
  const total = computed(() => items.value.reduce((sum, it) => sum + (it.price || 0) * it.quantity, 0))

  function add(product: { id: string; title: string; price: number; imageUrl?: string | null }, qty = 1) {
    const existing = items.value.find(i => i.id === product.id)
    if (existing) existing.quantity += qty
    else items.value.push({ ...product, quantity: qty })
  }

  function remove(id: string) {
    items.value = items.value.filter(i => i.id !== id)
  }

  function setQty(id: string, qty: number) {
    const it = items.value.find(i => i.id === id)
    if (!it) return
    const safe = Number.isFinite(qty) ? Math.trunc(qty) : 1
    it.quantity = Math.min(99, Math.max(1, safe))
  }

  function increase(id: string) {
    const it = items.value.find(i => i.id === id)
    if (!it) return
    setQty(id, (it.quantity || 1) + 1)
  }

  function decrease(id: string) {
    const it = items.value.find(i => i.id === id)
    if (!it) return
    setQty(id, (it.quantity || 1) - 1)
  }

  function clear() {
    items.value = []
  }

  return { items, count, total, add, remove, setQty, increase, decrease, clear }
})
