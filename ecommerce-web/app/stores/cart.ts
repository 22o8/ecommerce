import { defineStore } from 'pinia'

export type CartItem = {
  id: string
  title: string
  price: number
  originalPrice?: number | null
  discountPercent?: number
  imageUrl?: string | null
  stockQuantity?: number
  quantity: number
}

const normalizeProduct = (product: any) => {
  const id = String(product?.id ?? product?.productId ?? product?.slug ?? '')
  const title = String(product?.title ?? product?.name ?? product?.Title ?? '')
  const discountPercent = Number(product?.discountPercent ?? 0)
  const finalPrice = Number(product?.finalPriceIqd ?? product?.finalPrice ?? product?.priceIqd ?? product?.price ?? product?.priceUsd ?? 0)
  const originalPrice = Number(product?.priceIqd ?? product?.price ?? product?.priceUsd ?? finalPrice)
  const rawImage = product?.images?.[0]?.url || product?.images?.[0] || product?.coverImage || product?.imageUrl || product?.ImageUrl || product?.image || ''
  const stockQuantity = Number(product?.stockQuantity ?? product?.StockQuantity ?? 0)
  return {
    id,
    title,
    price: Number.isFinite(finalPrice) ? finalPrice : 0,
    originalPrice: Number.isFinite(originalPrice) ? originalPrice : null,
    discountPercent: Number.isFinite(discountPercent) ? discountPercent : 0,
    imageUrl: rawImage ? String(rawImage) : null,
    stockQuantity: Number.isFinite(stockQuantity) ? stockQuantity : 0,
  }
}

export const useCartStore = defineStore('cart', () => {
  const items = useState<CartItem[]>('cart_items', () => [])

  const count = computed(() => items.value.reduce((sum, it) => sum + (it.quantity || 0), 0))
  const total = computed(() => items.value.reduce((sum, it) => sum + (Number(it.price) || 0) * (it.quantity || 0), 0))

  function add(product: any, qty = 1) {
    const normalized = normalizeProduct(product)
    if (!normalized.id || Number(normalized.stockQuantity ?? 0) <= 0) return
    const existing = items.value.find(i => i.id === normalized.id)
    if (existing) {
      existing.quantity += qty
      existing.price = normalized.price || existing.price
      existing.originalPrice = normalized.originalPrice ?? existing.originalPrice ?? null
      existing.discountPercent = normalized.discountPercent ?? existing.discountPercent ?? 0
      existing.imageUrl = normalized.imageUrl || existing.imageUrl || null
      existing.title = normalized.title || existing.title
      existing.stockQuantity = normalized.stockQuantity ?? existing.stockQuantity ?? 0
    } else {
      items.value.push({ ...normalized, quantity: Math.max(1, qty) })
    }
  }

  function remove(id: string) { items.value = items.value.filter(i => i.id !== id) }
  function setQty(id: string, qty: number) {
    const it = items.value.find(i => i.id === id)
    if (!it) return
    const safe = Number.isFinite(qty) ? Math.trunc(qty) : 1
    const maxQty = Math.max(1, Math.min(99, Number(it.stockQuantity ?? 99) || 99))
    it.quantity = Math.min(maxQty, Math.max(1, safe))
  }
  function increase(id: string) { const it = items.value.find(i => i.id === id); if (it) setQty(id, (it.quantity || 1) + 1) }
  function decrease(id: string) { const it = items.value.find(i => i.id === id); if (it) setQty(id, (it.quantity || 1) - 1) }
  function clear() { items.value = [] }

  return { items, count, total, add, remove, setQty, increase, decrease, clear }
})
