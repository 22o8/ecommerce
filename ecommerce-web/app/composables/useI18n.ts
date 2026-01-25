// app/composables/useI18n.ts
import { computed } from 'vue'

type Locale = 'ar' | 'en'

const DICTS: Record<Locale, Record<string, string>> = {
  ar: {
    // Navbar
    'nav.home': 'الرئيسية',
    'nav.cart': 'السلة',
    'nav.login': 'تسجيل الدخول',
    'nav.logout': 'تسجيل خروج',
    'nav.admin': 'لوحة التحكم',
    'nav.language': 'اللغة',

    // Home
    'home.heroTitle': 'متجرك للمنتجات الرقمية',
    'home.heroSubtitle': 'اختر ما تحتاجه وأرسل الطلب مباشرة عبر واتساب أو من خلال إنشاء طلب داخل الموقع.',
    'browseProducts': 'تصفح المنتجات',
    'contact': 'تواصل معنا',
    'home.feature1Title': 'طلب سريع',
    'home.feature1Desc': 'اجمع أكثر من منتج في سلة واحدة وأنشئ طلبًا واحدًا.',
    'home.feature2Title': 'تواصل مباشر',
    'home.feature2Desc': 'إرسال تفاصيل السلة كاملة عبر واتساب بضغطة زر.',
    'home.feature3Title': 'حسابات مستخدمين',
    'home.feature3Desc': 'أنشئ حسابك واحفظ بياناتك لتسهيل الشراء لاحقًا.',

    // Products
    'products.title': 'المنتجات',
    'products.search': 'بحث',
    'products.addToCart': 'أضف للسلة',
    'products.details': 'التفاصيل',
    'products.price': 'السعر',

    // Cart
    'cart.title': 'السلة',
    'cart.empty': 'السلة فارغة',
    'cart.itemsCount': 'عدد العناصر',
    'cart.total': 'المجموع',
    'cart.checkout': 'إتمام الطلب',
    'cart.createOrder': 'إنشاء طلب',
    'cart.sendWhatsapp': 'إرسال عبر واتساب',
    'cart.remove': 'حذف',
    'cart.qty': 'الكمية',

    // Auth
    'login.title': 'تسجيل الدخول',
    'login.subtitle': 'أدخل بياناتك للمتابعة',
    'register.title': 'إنشاء حساب',
    'register.subtitle': 'اكتب معلوماتك بدقة حتى تتم عملية الشراء بسلاسة.',
    'auth.fullName': 'الاسم الكامل',
    'auth.phone': 'رقم الهاتف',
    'auth.email': 'البريد الإلكتروني',
    'auth.password': 'كلمة المرور',
    'auth.noAccount': 'ما عندك حساب؟ أنشئ واحد',
    'auth.haveAccount': 'عندك حساب؟ سجل دخول',
    'auth.warning': 'تحذير: يجب أن تكون المعلومات دقيقة حتى تتم عملية الشراء بسلاسة.',
    'auth.submitLogin': 'تسجيل الدخول',
    'auth.submitRegister': 'إنشاء حساب',

    // Admin
    'admin.title': 'لوحة التحكم',
    'admin.overview': 'نظرة عامة',
    'admin.products': 'المنتجات',
    'admin.orders': 'الطلبات',
    'admin.totalProducts': 'إجمالي المنتجات',
    'admin.totalOrders': 'إجمالي الطلبات',
    'admin.newProduct': 'منتج جديد',
    'admin.delete': 'حذف',
    'admin.refresh': 'تحديث',
    'admin.deleteAllOrders': 'حذف كل الطلبات',
    'admin.confirmDelete': 'تأكيد الحذف؟',
  },
  en: {
    'nav.home': 'Home',
    'nav.cart': 'Cart',
    'nav.login': 'Login',
    'nav.logout': 'Logout',
    'nav.admin': 'Admin',
    'nav.language': 'Language',

    'home.heroTitle': 'Your Digital Store',
    'home.heroSubtitle': 'Add items to your cart and send the full order via WhatsApp or create an order inside the site.',
    'browseProducts': 'Browse products',
    'contact': 'Contact us',
    'home.feature1Title': 'Fast ordering',
    'home.feature1Desc': 'Collect multiple items in one cart and create a single order.',
    'home.feature2Title': 'Direct contact',
    'home.feature2Desc': 'Send your full cart details via WhatsApp with one click.',
    'home.feature3Title': 'User accounts',
    'home.feature3Desc': 'Create an account and keep your details for quicker checkout.',

    'products.title': 'Products',
    'products.search': 'Search',
    'products.addToCart': 'Add to cart',
    'products.details': 'Details',
    'products.price': 'Price',

    'cart.title': 'Cart',
    'cart.empty': 'Your cart is empty',
    'cart.itemsCount': 'Items',
    'cart.total': 'Total',
    'cart.checkout': 'Checkout',
    'cart.createOrder': 'Create order',
    'cart.sendWhatsapp': 'Send via WhatsApp',
    'cart.remove': 'Remove',
    'cart.qty': 'Qty',

    'login.title': 'Login',
    'login.subtitle': 'Enter your details to continue',
    'register.title': 'Create account',
    'register.subtitle': 'Please provide accurate details for a smooth purchase.',
    'auth.fullName': 'Full name',
    'auth.phone': 'Phone',
    'auth.email': 'Email',
    'auth.password': 'Password',
    'auth.noAccount': "Don't have an account? Create one",
    'auth.haveAccount': 'Already have an account? Login',
    'auth.warning': 'Warning: your details must be accurate for a smooth purchase.',
    'auth.submitLogin': 'Login',
    'auth.submitRegister': 'Create account',

    'admin.title': 'Admin Panel',
    'admin.overview': 'Overview',
    'admin.products': 'Products',
    'admin.orders': 'Orders',
    'admin.totalProducts': 'Total Products',
    'admin.totalOrders': 'Total Orders',
    'admin.newProduct': 'New Product',
    'admin.delete': 'Delete',
    'admin.refresh': 'Refresh',
    'admin.deleteAllOrders': 'Delete all orders',
    'admin.confirmDelete': 'Confirm delete?',
  }
}

export function useI18n() {
  const localeCookie = useCookie<Locale>('locale', { default: () => 'ar' })
  const locale = computed<Locale>(() => localeCookie.value || 'ar')

  const isRtl = computed(() => locale.value === 'ar')

  const t = (key: string) => {
    const dict = DICTS[locale.value] || DICTS.ar
    return dict[key] ?? key
  }

  const setLocale = (l: Locale) => {
    localeCookie.value = l
    if (process.client) {
      document.documentElement.setAttribute('dir', l === 'ar' ? 'rtl' : 'ltr')
      document.documentElement.setAttribute('lang', l)
    }
  }

  return { t, locale, setLocale, isRtl }
}
