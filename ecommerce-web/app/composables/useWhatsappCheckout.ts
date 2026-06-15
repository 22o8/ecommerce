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

function openWhatsappUrl(url: string) {
  if (!import.meta.client) return

  const ua = String(navigator.userAgent || '').toLowerCase()
  const isMobile = /android|iphone|ipad|ipod|mobile|iemobile|opera mini/.test(ua) || window.innerWidth < 768

  if (isMobile) {
    window.location.href = url
    return
  }

  const newTab = window.open(url, '_blank', 'noopener,noreferrer')
  if (!newTab) {
    window.location.href = url
  }
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

    const value = (globalThis.crypto?.randomUUID?.() || `${Date.now()}-${Math.random().toString(36).slice(2)}`)
      .replace(/[^a-zA-Z0-9_-]/g, '')
      .slice(0, 64)

    localStorage.setItem(key, value)
    return value
  }


  const collectInvoiceExtras = () => {
    if (!import.meta.client) return { deliveryFeeIqd: 0, customerNote: '' }
    const feeRaw = prompt('مبلغ التوصيل بالدينار (اختياري)', '0') || '0'
    const note = prompt('ملاحظة على الفاتورة أو العنوان (اختياري)', '') || ''
    return { deliveryFeeIqd: Math.max(0, Number(feeRaw) || 0), customerNote: note }
  }

  const buildCartMessage = (items: WhatsappMessageItem[], meta?: WhatsappMessageMeta) => {
    const when = new Date().toLocaleString('ar-IQ', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
    const format = (v: any) => formatIqd(v)
    const subtotal = items.reduce((s, i) => s + ((Number(i.price) || 0) * (Number(i.quantity) || 0)), 0)
    const finalTotal = Number(meta?.finalTotalIqd ?? subtotal)
    const shortOrderCode = meta?.orderId
      ? String(meta.orderId).replace(/-/g, '').slice(-8).toUpperCase()
      : ''

    const productLines = items.map((i, idx) => {
      const quantity = Math.max(1, Number(i.quantity) || 1)
      const lineTotal = (Number(i.price) || 0) * quantity
      const discountNote = i.discountPercent
        ? `\n   خصم المنتج: ${i.discountPercent}% من ${format(i.originalPrice || i.price)}`
        : ''

      return [
        `${idx + 1}) ${i.title}`,
        `   الكمية: ${quantity}`,
        `   المجموع: ${format(lineTotal)}${discountNote}`
      ].join('\n')
    })

    const lines = [
      '🛍️ طلب جديد من المتجر',
      '━━━━━━━━━━━━━━',
      `🕒 وقت الطلب: ${when}`,
      ...(shortOrderCode ? [`🔖 كود الطلب: #${shortOrderCode}`] : []),
      '',
      '📦 المنتجات المطلوبة:',
      ...productLines,
      '',
      '💰 ملخص الطلب:',
      `الإجمالي قبل الخصم: ${format(subtotal)}`,
      ...(meta?.couponCode
        ? [
            `الكوبون المستخدم: ${meta.couponCode}${meta.couponTitle ? ` - ${meta.couponTitle}` : ''}`,
            `قيمة الخصم: ${format(Number(meta.discountAmountIqd || 0))}`
          ]
        : []),
      `الإجمالي النهائي: ${format(finalTotal)}`,
      '━━━━━━━━━━━━━━',
      'يرجى تأكيد توفر المنتجات وطريقة الاستلام مع الزبون.'
    ]

    return lines.join('\n')
  }

  const openWhatsappForCart = async () => {
    const extras = collectInvoiceExtras()
    const payload = {
      ...extras,
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

    openWhatsappUrl(url)
  }

  const checkoutSingleProduct = async (product: any, quantity = 1) => {
    const id = String(product?.id ?? product?.productId ?? '')
    if (!id) throw new Error('Missing product id')

    const normalizedPrice = Number(product?.finalPriceIqd ?? product?.finalPrice ?? product?.priceIqd ?? product?.price ?? product?.priceUsd ?? 0)
    const normalizedOriginal = Number(product?.priceIqd ?? product?.price ?? product?.priceUsd ?? normalizedPrice)
    const normalizedDiscount = Number(product?.discountPercent ?? 0)
    const title = String(product?.title ?? product?.name ?? 'منتج')

    const extras = collectInvoiceExtras()
    const result: any = await api.post('/Checkout/cart/whatsapp', {
      ...extras,
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

    openWhatsappUrl(url)
  }

  return { openWhatsappForCart, checkoutSingleProduct }
}
