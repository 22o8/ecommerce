export type WhatsappCheckoutItem = {
  productId: string
  quantity: number
}

type WhatsappMessageItem = {
  title: string
  quantity: number
  price: number
  originalPrice?: number | null
  discountPercent?: number
}

type WhatsappMessageMeta = {
  orderId?: string
  couponCode?: string | null
  couponTitle?: string | null
  discountAmountIqd?: number
  finalTotalIqd?: number
}

function normalizePhone(v: any) {
  return String(v || '').replace(/\D/g, '')
}

export function useWhatsappCheckout() {
  const api = useApi()
  const config = useRuntimeConfig()
  const cart = useCartStore()
  const appliedCoupon = useState<any | null>('cart_coupon_applied', () => null)

  const getDeviceKey = () => {
    if (!process.client) return ''
    const key = 'coupon_device_key'
    const existing = localStorage.getItem(key)
    if (existing) return existing
    const value = `${Date.now()}-${Math.random().toString(36).slice(2)}-${navigator.userAgent}`
    localStorage.setItem(key, value)
    return value
  }

  const buildCartMessage = (items: WhatsappMessageItem[], meta?: WhatsappMessageMeta) => {
    const when = new Date().toLocaleString('ar-IQ')
    const format = (v: any) => formatIqd(v)
    const subtotal = items.reduce((s, i) => s + ((Number(i.price) || 0) * (Number(i.quantity) || 0)), 0)

    const lines = [
      'طلب جديد من المتجر',
      `الوقت: ${when}`,
      ...(meta?.orderId ? [`رقم الطلب: ${meta.orderId}`] : []),
      '',
      'المنتجات:',
      ...items.map(i =>
        `- ${i.title} × ${i.quantity} = ${format((Number(i.price) || 0) * (Number(i.quantity) || 0))}${i.discountPercent ? ` (خصم ${i.discountPercent}% من ${format(i.originalPrice || i.price)})` : ''}`
      ),
      '',
      `الإجمالي قبل الخصم: ${format(subtotal)}`,
      ...(meta?.couponCode
        ? [
            `الكوبون المستخدم: ${meta.couponCode}${meta.couponTitle ? ` - ${meta.couponTitle}` : ''}`,
            `قيمة الخصم: ${format(Number(meta.discountAmountIqd || 0))}`
          ]
        : []),
      `الإجمالي النهائي: ${format(meta?.finalTotalIqd ?? subtotal)}`
    ]

    return lines.join('\n')
  }

  const openWhatsappForCart = async () => {
    const payload = {
      items: cart.items.map(i => ({
        productId: i.id,
        quantity: Math.max(1, Number(i.quantity) || 1),
      })),
      couponCode: appliedCoupon.value?.code || undefined,
      deviceKey: getDeviceKey() || undefined
    }

    const result: any = await api.post('/Checkout/cart/whatsapp', payload)
    if (!(result as any)?.orderId) {
      throw new Error('تعذر حفظ الطلب في النظام.')
    }

    const number = normalizePhone((config.public as any).whatsappNumber)
    const text = encodeURIComponent(
      buildCartMessage(cart.items as any, {
        orderId: result.orderId,
        couponCode: result.couponCode || appliedCoupon.value?.code || null,
        couponTitle: appliedCoupon.value?.title || null,
        discountAmountIqd: Number(result.discountAmountIqd || appliedCoupon.value?.discountAmountIqd || 0),
        finalTotalIqd: Number(result.amountIqd || 0)
      })
    )
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

    const result: any = await api.post('/Checkout/cart/whatsapp', {
      items: [{ productId: id, quantity: Math.max(1, Number(quantity) || 1) }],
      couponCode: appliedCoupon.value?.code || undefined,
      deviceKey: getDeviceKey() || undefined
    })
    if (!(result as any)?.orderId) {
      throw new Error('تعذر حفظ الطلب في النظام.')
    }

    const number = normalizePhone((config.public as any).whatsappNumber)
    const text = encodeURIComponent(
      buildCartMessage([
        {
          title,
          quantity: Math.max(1, Number(quantity) || 1),
          price: normalizedPrice,
          originalPrice: normalizedOriginal,
          discountPercent: normalizedDiscount,
        }
      ], {
        orderId: result.orderId,
        couponCode: result.couponCode || appliedCoupon.value?.code || null,
        couponTitle: appliedCoupon.value?.title || null,
        discountAmountIqd: Number(result.discountAmountIqd || appliedCoupon.value?.discountAmountIqd || 0),
        finalTotalIqd: Number(result.amountIqd || normalizedPrice * Math.max(1, Number(quantity) || 1))
      })
    )

    const url = number
      ? `https://wa.me/${number}?text=${text}`
      : `https://wa.me/?text=${text}`

    if (import.meta.client) {
      window.open(url, '_blank')
    }
  }

  return { openWhatsappForCart, checkoutSingleProduct }
}
