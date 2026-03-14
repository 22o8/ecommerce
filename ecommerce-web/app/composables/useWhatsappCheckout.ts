export type WhatsappCheckoutItem = {
  productId: string
  quantity: number
}

function normalizePhone(v: any) {
  return String(v || '').replace(/\D/g, '')
}

export function useWhatsappCheckout() {
  const api = useApi()
  const config = useRuntimeConfig()
  const cart = useCartStore()
  const appliedCoupon = useState<any | null>('cart_coupon_applied', () => null)

  const buildCartMessage = (items: Array<{ title: string; quantity: number; price: number; originalPrice?: number | null; discountPercent?: number }>) => {
    const when = new Date().toLocaleString('ar-IQ')
    const format = (v: any) => formatIqd(v)

    const lines = [
      'طلب جديد من المتجر',
      `الوقت: ${when}`,
      '',
      'المنتجات:',
      ...items.map(i =>
        `- ${i.title} × ${i.quantity} = ${format((Number(i.price) || 0) * (Number(i.quantity) || 0))}${i.discountPercent ? ` (خصم ${i.discountPercent}% من ${format(i.originalPrice || i.price)})` : ''}`
      ),
      '',
      `الإجمالي: ${format(items.reduce((s, i) => s + ((Number(i.price) || 0) * (Number(i.quantity) || 0)), 0))}`
    ]

    return lines.join('\n')
  }

  const openWhatsappForCart = async () => {
    const payload = {
      items: cart.items.map(i => ({
        productId: i.id,
        quantity: Math.max(1, Number(i.quantity) || 1),
      })),
      couponCode: appliedCoupon.value?.code || undefined
    }

    const result = await api.post('/Checkout/cart/whatsapp', payload)
    if (!(result as any)?.orderId) {
      throw new Error('تعذر حفظ الطلب في النظام.')
    }

    const number = normalizePhone((config.public as any).whatsappNumber)
    const text = encodeURIComponent(buildCartMessage(cart.items as any))
    const url = number
      ? `https://wa.me/${number}?text=${text}`
      : `https://wa.me/?text=${text}`

    if (import.meta.client) {
      window.open(url, '_blank')
    }
  }

  const checkoutSingleProduct = async (product: any, quantity = 1) => {
    const id = String(product?.id ?? product?.productId ?? '')
    if (!id) throw new Error('Missing product id')

    const normalizedPrice = Number(product?.finalPriceIqd ?? product?.finalPrice ?? product?.priceIqd ?? product?.price ?? product?.priceUsd ?? 0)
    const normalizedOriginal = Number(product?.priceIqd ?? product?.price ?? product?.priceUsd ?? normalizedPrice)
    const normalizedDiscount = Number(product?.discountPercent ?? 0)
    const title = String(product?.title ?? product?.name ?? 'منتج')

    const result = await api.post('/Checkout/cart/whatsapp', {
      items: [{ productId: id, quantity: Math.max(1, Number(quantity) || 1) }],
      couponCode: appliedCoupon.value?.code || undefined
    })
    if (!(result as any)?.orderId) {
      throw new Error('تعذر حفظ الطلب في النظام.')
    }

    const number = normalizePhone((config.public as any).whatsappNumber)
    const text = encodeURIComponent(buildCartMessage([{
      title,
      quantity: Math.max(1, Number(quantity) || 1),
      price: normalizedPrice,
      originalPrice: normalizedOriginal,
      discountPercent: normalizedDiscount,
    }]))

    const url = number
      ? `https://wa.me/${number}?text=${text}`
      : `https://wa.me/?text=${text}`

    if (import.meta.client) {
      window.open(url, '_blank')
    }
  }

  return { openWhatsappForCart, checkoutSingleProduct }
}
