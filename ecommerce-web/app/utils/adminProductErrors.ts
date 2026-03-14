export function getAdminProductErrorMessage(
  error: any,
  context?: {
    phase?: 'create' | 'update' | 'upload' | 'delete'
    productCreated?: boolean
  }
) {
  const status =
    error?.statusCode ??
    error?.response?.status ??
    error?.status ??
    error?.data?.statusCode ??
    0

  const data = error?.data ?? error?.response?._data ?? {}
  const rawMessage =
    data?.message ||
    data?.title ||
    extractModelStateMessage(data) ||
    error?.message ||
    'Unknown error'

  const msg = String(rawMessage).toLowerCase()

  if (
    status === 413 ||
    msg.includes('payload too large') ||
    msg.includes('function_payload_too_large') ||
    msg.includes('request entity too large')
  ) {
    return context?.productCreated
      ? 'تم إنشاء المنتج، لكن فشل رفع الصورة لأن حجمها كبير. قلل حجم الصورة ثم ارفعها من صفحة تعديل المنتج.'
      : 'فشل رفع الصورة لأن حجمها كبير جدًا. قلل حجم الصورة ثم أعد المحاولة.'
  }

  if (
    msg.includes('slug already exists') ||
    msg.includes('slug is already exists') ||
    msg.includes('duplicate slug')
  ) {
    return 'هذا الـ slug متكرر. غيّر قيمة الـ slug إلى قيمة أخرى ثم أعد المحاولة.'
  }

  if (
    msg.includes('product already exists') ||
    msg.includes('duplicate product') ||
    (msg.includes('already exists') && msg.includes('product'))
  ) {
    return 'هذا المنتج متكرر أو موجود مسبقًا. راجع صفحة المنتجات قبل إنشاء منتج جديد.'
  }

  if (msg.includes('brand is required')) {
    return 'يرجى اختيار البراند أولًا قبل حفظ المنتج.'
  }

  if (msg.includes('insufficient stock')) {
    return 'الكمية المطلوبة أكبر من المخزون المتوفر لهذا المنتج.'
  }

  if (status === 400) {
    return `تعذر تنفيذ الطلب: ${translateBackendMessage(rawMessage)}`
  }

  if (status === 404) {
    return 'العنصر المطلوب غير موجود أو تم حذفه.'
  }

  if (status === 409) {
    return `يوجد تعارض في البيانات: ${translateBackendMessage(rawMessage)}`
  }

  return `حدث خطأ: ${translateBackendMessage(rawMessage)}`
}

function extractModelStateMessage(data: any) {
  if (!data || typeof data !== 'object') return ''
  const errors = data?.errors
  if (!errors || typeof errors !== 'object') return ''
  for (const key of Object.keys(errors)) {
    const val = errors[key]
    if (Array.isArray(val) && val.length) return String(val[0])
    if (typeof val === 'string' && val.trim()) return val
  }
  return ''
}

function translateBackendMessage(message: string) {
  const m = String(message || '').trim()
  const l = m.toLowerCase()

  if (!m) return 'خطأ غير معروف'

  if (l.includes('slug already exists')) return 'هذا الـ slug متكرر'
  if (l.includes('product already exists')) return 'المنتج موجود مسبقًا'
  if (l.includes('brand is required')) return 'البراند مطلوب'
  if (l.includes('insufficient stock')) return 'المخزون غير كافٍ'
  if (l.includes('coupon already exists')) return 'كود الكوبون موجود مسبقًا'
  if (l.includes('coupon expired')) return 'صلاحية الكوبون منتهية'
  if (l.includes('coupon not found')) return 'الكوبون غير موجود'
  if (l.includes('minimum order not reached')) return 'لم يتم الوصول للحد الأدنى للطلب'
  if (l.includes('coupon usage limit reached')) return 'تم الوصول للحد الأقصى لاستخدام الكوبون'

  return m
}
