import { defineStore } from 'pinia'

export type CartItem = {
  id: string
  title: string
  price: number
  originalPrice?: number
  discountPercent?: number
  imageUrl?: string | null
  quantity: number
}

function normalizeCartProduct(product: any) {
  const originalPrice = Number(product?.priceIqd ?? product?.price ?? 0)
  const discountPercent = Number(product?.discountPercent ?? 0)
  const finalPrice = Number(product?.finalPriceIqd ?? (discountPercent > 0 ? (originalPrice * (100 - discountPercent) / 100) : originalPrice))
  return {
    id: String(product?.id ?? ''),
    title: String(product?.title ?? product?.name ?? ''),
    imageUrl: product?.imageUrl ?? product?.coverImage ?? product?.images?.[0]?.url ?? product?.images?.[0] ?? null,
    price: finalPrice,
    originalPrice,
    discountPercent,
  }
}

export const useCartStore = defineStore('cart', () => {
  const items = useState<CartItem[]>('cart_items', () => [])

  const count = computed(() => items.value.reduce((sum, it) => sum + it.quantity, 0))
  const total = computed(() => items.value.reduce((sum, it) => sum + (it.price || 0) * it.quantity, 0))

  function add(product: any, qty = 1) {
    const normalized = normalizeCartProduct(product)
    const existing = items.value.find(i => i.id === normalized.id)
    if (existing) {
      existing.quantity += qty
      existing.price = normalized.price
      existing.originalPrice = normalized.originalPrice
      existing.discountPercent = normalized.discountPercent
      existing.imageUrl = normalized.imageUrl
      existing.title = normalized.title
    }
    else items.value.push({ ...normalized, quantity: qty })
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
