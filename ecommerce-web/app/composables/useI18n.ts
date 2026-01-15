// app/composables/useI18n.ts
import { computed } from 'vue'
import { useCookie } from '#app'

export type Locale = 'ar' | 'en'

const dict: Record<Locale, Record<string, string>> = {
  ar: {
    home: 'الرئيسية',
    products: 'المنتجات',
    services: 'الخدمات',
    myOrders: 'طلباتي',
    serviceRequests: 'طلبات الخدمة',
    admin: 'الادمن',
    dashboard: 'لوحة التحكم',

    login: 'تسجيل الدخول',
    account: 'الحساب',
    logout: 'تسجيل خروج',
    contact: 'تواصل',
    terms: 'الشروط',
    privacy: 'الخصوصية',
    dark: 'داكن',
    light: 'فاتح',

    tagline: 'منتجات رقمية وخدمات',
    'theme.dark': 'داكن',
    'theme.light': 'فاتح',

    heroTitle: 'متجرك للمنتجات الرقمية',
    heroSub: 'اشترِ خدمات وبرامج وملفات رقمية واستلم التحميل فوراً بعد الدفع.',
    browseProducts: 'تصفح المنتجات',
    browseServices: 'تصفح الخدمات',

    whyUs: 'لماذا نحن؟',
    fastDelivery: 'تسليم فوري بعد الدفع',
    securePayments: 'دفع آمن ومنظم',
    support: 'دعم سريع',

    browseFast: 'تصفح بسرعة',
    searchProduct: 'ابحث عن منتج...',
    noProductsYet: 'لا توجد منتجات بعد',

    productDetails: 'تفاصيل المنتج',
    price: 'السعر',
    buy: 'شراء الآن',
    loginToBuy: 'سجّل دخول حتى تشتري',
    backToProducts: 'رجوع للمنتجات',
    noImage: 'لا توجد صورة',
    buyNow: 'شراء الآن',
    whatsappOrder: 'طلب عبر واتساب',

    // Home (landing)
    'home.badge': 'تسليم فوري • دفع آمن',
    'home.subtitle': 'جرّب متجر متكامل لبيع المنتجات والخدمات الرقمية مع معرض صور، بحث، وفرز، وطلبات.',
    // Backward/forward compatible aliases (some pages use different key naming)
    'home.title': 'متجر تجميل وعناية بالبشرة',
    'home.browse': 'تصفح المنتجات',
    'home.goToAccount': 'الذهاب للحساب',
    'home.whyTitle': 'لماذا نحن؟',
    'home.whySubtitle': 'المتجر',
    'home.featured.title': 'منتجات مميزة',
    'home.featured.subtitle': 'مختارات اليوم',
    'home.features.instant.title': 'تسليم فوري',
    'home.features.instant.desc': 'وصول مباشر بعد الدفع',
    'home.features.secure': 'دفع آمن',
    'home.features.support': 'دعم سريع',
    'home.why.title': 'لماذا نحن؟',
    'home.why.subtitle': 'المتجر',
    'home.why.role': 'متجر احترافي',
    'home.why.aTitle': 'جودة العرض',
    'home.why.aDesc': 'تصميم نظيف يناسب متاجر التجميل والعناية.',
    'home.why.bTitle': 'صور متعددة',
    'home.why.bDesc': 'معرض صور لكل منتج مع عرض مصغّرات.',
    'home.why.cTitle': 'تجربة سريعة',
    'home.why.cDesc': 'بحث وفرز وتصفح سلس على كل الأجهزة.',

    // Services
    serviceDetails: 'تفاصيل الخدمة',
    backToServices: 'رجوع للخدمات',
    requestService: 'طلب الخدمة',
    sendRequest: 'إرسال الطلب',

    // Orders / Requests
    orderPage: 'صفحة الطلب',
    loading: 'جارٍ التحميل...',
    download: 'تحميل الملف',
    noDownloadToken: 'لا يوجد توكن تحميل بعد. تأكد أن الدفع مكتمل.',
    needLogin: 'تحتاج تسجيل دخول للمتابعة.',
    myOrdersTitle: 'طلباتي',

    requestFailed: 'فشل الطلب',
    notFound: 'الصفحة غير موجودة',
    backHome: 'رجوع للرئيسية',
  },

  en: {
    home: 'Home',
    products: 'Products',
    services: 'Services',
    myOrders: 'My Orders',
    serviceRequests: 'Service Requests',
    admin: 'Admin',
    dashboard: 'Dashboard',

    login: 'Login',
    account: 'Account',
    logout: 'Logout',
    contact: 'Contact',
    terms: 'Terms',
    privacy: 'Privacy',
    dark: 'Dark',
    light: 'Light',

    tagline: 'Digital products & services',
    'theme.dark': 'Dark',
    'theme.light': 'Light',

    heroTitle: 'Digital Products Store',
    heroSub: 'Buy services, software, and digital files — download instantly after payment.',
    browseProducts: 'Browse products',
    browseServices: 'Browse services',

    whyUs: 'Why us?',
    fastDelivery: 'Instant delivery after payment',
    securePayments: 'Secure checkout',
    support: 'Fast support',

    browseFast: 'Browse fast',
    searchProduct: 'Search product...',
    noProductsYet: 'No products yet',

    productDetails: 'Product details',
    price: 'Price',
    buy: 'Buy now',
    loginToBuy: 'Login to buy',
    backToProducts: 'Back to products',
    noImage: 'No image',
    buyNow: 'Buy now',
    whatsappOrder: 'Order via WhatsApp',

    // Home (landing)
    'home.badge': 'Instant delivery • Secure checkout',
    'home.subtitle': 'A complete store experience for selling digital products and services.',
    // Backward/forward compatible aliases (some pages use different key naming)
    'home.title': 'Beauty Shop – glow delivered',
    'home.browse': 'Browse products',
    'home.goToAccount': 'Go to account',
    'home.whyTitle': 'Why choose us?',
    'home.whySubtitle': 'Trusted experience, clear pricing, and quick support.',
    'home.why.aTitle': 'Curated & authentic',
    'home.why.aDesc': 'Carefully selected items with clear info.',
    'home.why.bTitle': 'Fast WhatsApp support',
    'home.why.bDesc': 'Quick replies for questions and ordering.',
    'home.why.cDesc': 'Transparent prices & smooth checkout.',
    'home.featured.title': 'Featured products',
    'home.featured.subtitle': 'Hand-picked items we recommend.',
    'home.why.cTitle': 'Why choose us',
    'home.why.cSub': 'Fast, secure and easy',
    'home.features.instant.title': 'Instant delivery',
    'home.features.instant.desc': 'Get access right after payment',
    'home.features.secure': 'Secure checkout',
    'home.features.secureDesc': 'Pay with confidence — protected checkout.',
    'home.features.support': 'Fast support',
    'home.features.supportDesc': 'Quick help via WhatsApp and chat.',

    'home.why.title': 'Why us?',
    'home.why.subtitle': 'Developer',
    'home.why.items.1.title': 'Trusted products',
    'home.why.items.1.desc': 'Quality files & clear descriptions',
    'home.why.items.2.title': 'Fast response',
    'home.why.items.2.desc': 'We answer quickly on requests',
    'home.why.items.3.title': 'Easy experience',
    'home.why.items.3.desc': 'Smooth browsing, buying, and delivery',

    // Services
    serviceDetails: 'Service details',
    backToServices: 'Back to services',
    requestService: 'Request service',
    sendRequest: 'Send request',

    // Orders / Requests
    orderPage: 'Order page',
    loading: 'Loading...',
    download: 'Download file',
    noDownloadToken: 'No download token yet. Make sure payment is completed.',
    needLogin: 'You need to login to continue.',
    myOrdersTitle: 'My Orders',

    requestFailed: 'Request failed',
    notFound: 'Page not found',
    backHome: 'Back home',
  },
}

export function useI18n() {
  const localeCookie = useCookie<Locale>('locale', { default: () => 'en' })
  const locale = computed(() => localeCookie.value)

  const t = (key: string) => dict[localeCookie.value]?.[key] ?? key

  const setLocale = (l: Locale) => {
    localeCookie.value = l
  }

  // ✅ توافق مع نسخ سابقة (بعض الملفات القديمة كانت تستدعي init())
  // ما نحتاج أي تهيئة حاليًا، لكن وجودها يمنع خطأ: "init is not a function".
  const init = () => {}

  return { t, locale, setLocale, init }
}
