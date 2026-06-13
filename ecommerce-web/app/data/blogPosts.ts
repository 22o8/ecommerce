export type BlogFaq = { question: string; answer: string }
export type BlogPost = {
  title: string
  slug: string
  content: string
  coverImage: string
  metaTitle: string
  metaDescription: string
  keywords: string[]
  faq: BlogFaq[]
  relatedProductKeywords?: string[]
}

export const blogPosts: BlogPost[] = [
  {
    title: 'أفضل تونر كوري في العراق للبشرة الحساسة والجافة',
    slug: 'best-korean-toner-iraq',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'أفضل تونر كوري في العراق | DR SEOUL BEAUTY',
    metaDescription: 'دليل اختيار أفضل تونر كوري في العراق للبشرة الحساسة والجافة والدهنية مع كلمات مهمة مثل ANUA Iraq وKorean skincare Iraq.',
    keywords: ['أفضل تونر كوري في العراق', 'تونر كوري العراق', 'تونر كوري بغداد', 'ANUA Iraq', 'Korean skincare Iraq'],
    content: `اختيار التونر الكوري المناسب في العراق يعتمد على نوع البشرة والجو وطريقة الاستخدام. للبشرة الحساسة ابحث عن Heartleaf أو Centella، وللبشرة الجافة ركز على Hyaluronic Acid وCeramides، أما البشرة الدهنية فتحتاج تونر خفيف غير دهني. DR SEOUL BEAUTY يستهدف الباحثين عن تونر كوري أصلي في العراق وبغداد والبصرة وأربيل مع شرح واضح لكل منتج وروابط داخلية للمنتجات المناسبة.`,
    relatedProductKeywords: ['toner', 'heartleaf', 'anua'],
    faq: [
      { question: 'ما أفضل تونر كوري للبشرة الحساسة في العراق؟', answer: 'التونر الذي يحتوي Heartleaf أو Centella أو Hyaluronic Acid يكون مناسباً غالباً للبشرة الحساسة، مع ضرورة اختبار المنتج تدريجياً.' },
      { question: 'هل التونر الكوري ضروري؟', answer: 'ليس إجبارياً، لكنه يساعد على ترطيب البشرة وتجهيزها للسيروم والمرطب.' },
      { question: 'أين أجد تونر كوري أصلي في بغداد؟', answer: 'يمكنك استهداف المنتجات الأصلية عبر صفحات DR SEOUL BEAUTY وصفحات المدن مثل بغداد داخل الموقع.' },
    ],
  },
  {
    title: 'أفضل سيروم لحب الشباب وآثار الحبوب في العراق',
    slug: 'best-acne-serum-iraq',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'أفضل سيروم لحب الشباب في العراق | Korean Skincare Iraq',
    metaDescription: 'تعرف على أفضل أنواع السيروم الكوري للحبوب وآثار الحبوب في العراق مع نصائح للبشرة الدهنية والحساسة.',
    keywords: ['أفضل سيروم حب الشباب العراق', 'سيروم كوري العراق', 'سيروم للحبوب بغداد', 'K beauty Iraq', 'COSRX Iraq'],
    content: `السيروم المناسب لحب الشباب يجب أن يكون خفيفاً وغير كوميدوجينيك. من أشهر المكونات التي يبحث عنها المستخدم العراقي Niacinamide وCentella وTea Tree وSnail Mucin. لا تخلط أكثر من مقشر أو مادة فعالة قوية في نفس الروتين، وابدأ تدريجياً مع واقي الشمس صباحاً.`,
    relatedProductKeywords: ['serum', 'acne', 'cosrx', 'centella'],
    faq: [
      { question: 'هل السيروم يعالج حب الشباب وحده؟', answer: 'قد يساعد في الحالات الخفيفة، أما الحالات الشديدة فتحتاج مراجعة مختص جلدية.' },
      { question: 'هل أستخدم السيروم مع واقي الشمس؟', answer: 'نعم، واقي الشمس مهم جداً خصوصاً مع منتجات التفتيح أو المقشرات.' },
      { question: 'ما أفضل كلمات البحث لهذا النوع؟', answer: 'سيروم حب الشباب العراق، serum acne Iraq، Korean skincare Baghdad.' },
    ],
  },
  {
    title: 'روتين العناية الكوري اليومي في العراق خطوة بخطوة',
    slug: 'korean-skincare-routine-iraq',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'روتين العناية الكوري في العراق | DR SEOUL BEAUTY',
    metaDescription: 'روتين عناية كوري مبسط مناسب للعراق: غسول، تونر، سيروم، مرطب، وواقي شمس مع ترتيب الاستخدام الصحيح.',
    keywords: ['روتين عناية كوري العراق', 'Korean skincare routine Iraq', 'واقي شمس كوري العراق', 'سيروم كوري بغداد'],
    content: `لا تحتاج إلى عشر خطوات يومياً حتى تستفيد من العناية الكورية. الروتين الأقوى والأكثر واقعية في العراق هو: غسول لطيف، تونر مرطب، سيروم حسب المشكلة، مرطب مناسب، وواقي شمس صباحاً. الاستمرار أهم من كثرة المنتجات، واختيار المنتج حسب نوع البشرة أهم من اسم البراند فقط.`,
    relatedProductKeywords: ['cleanser', 'toner', 'serum', 'sunscreen'],
    faq: [
      { question: 'ما أهم خطوة صباحاً؟', answer: 'واقي الشمس هو أهم خطوة لحماية البشرة من التصبغات والشيخوخة المبكرة.' },
      { question: 'هل أحتاج روتين طويل؟', answer: 'لا، روتين بسيط وثابت أفضل من خلط منتجات كثيرة.' },
      { question: 'هل الروتين الكوري مناسب للعراق؟', answer: 'نعم بشرط اختيار قوام خفيف ومنتجات تناسب الجو ونوع البشرة.' },
    ],
  },
  {
    title: 'ANUA أم COSRX أيهما أفضل لبشرتك؟',
    slug: 'anua-vs-cosrx-iraq',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'ANUA vs COSRX Iraq | مقارنة براندات العناية الكورية',
    metaDescription: 'مقارنة بين ANUA و COSRX للمستخدم العراقي: البشرة الحساسة، الحبوب، الترطيب، والاحمرار.',
    keywords: ['ANUA Iraq', 'COSRX Iraq', 'Anua vs COSRX', 'منتجات كورية العراق', 'Korean skincare comparison Iraq'],
    content: `ANUA تشتهر بالتهدئة والمكونات اللطيفة مثل Heartleaf، لذلك تناسب كثيراً من أصحاب البشرة الحساسة والاحمرار. COSRX مشهور بمنتجات عملية للحبوب والمسام والملمس مثل Snail Mucin وBHA. في العراق، الاختيار يعتمد على المشكلة: إذا كان الهدف تهدئة واحمرار ابدأ مع ANUA، وإذا كانت المشكلة حبوب ومسام وآثار فابحث في خيارات COSRX المناسبة.`,
    relatedProductKeywords: ['anua', 'cosrx', 'heartleaf', 'snail'],
    faq: [
      { question: 'هل ANUA مناسب للبشرة الحساسة؟', answer: 'كثير من منتجات ANUA موجهة للتهدئة، لكن الأفضل اختبار المنتج على جزء صغير أولاً.' },
      { question: 'هل COSRX قوي على البشرة؟', answer: 'بعض منتجاته فعالة وقد تكون قوية، لذلك يفضل الاستخدام التدريجي.' },
      { question: 'ما الأفضل في العراق؟', answer: 'الأفضل هو المنتج المناسب لنوع بشرتك وليس البراند وحده.' },
    ],
  },
  {
    title: 'أفضل سيروم كولاجين وببتيدات في العراق',
    slug: 'best-collagen-serum-iraq',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'أفضل سيروم كولاجين في العراق | Medipeel Iraq',
    metaDescription: 'دليل اختيار سيروم كولاجين وببتيدات كوري في العراق للترطيب وتحسين مظهر الخطوط والمرونة.',
    keywords: ['سيروم كولاجين العراق', 'Medipeel Iraq', 'سيروم ببتيدات بغداد', 'Korean collagen serum Iraq'],
    content: `سيرومات الكولاجين والببتيدات مفيدة لمن يبحث عن ترطيب وامتلاء وتحسين مظهر الخطوط الدقيقة. ابحث عن منتجات تحتوي Peptides وHyaluronic Acid وCollagen مع مرطب مناسب. لا تنس أن النتائج تحتاج استمراراً، وأن واقي الشمس جزء مهم لحماية البشرة.`,
    relatedProductKeywords: ['collagen', 'peptide', 'medipeel'],
    faq: [
      { question: 'هل سيروم الكولاجين يشد البشرة فوراً؟', answer: 'قد يعطي ترطيباً وامتلاءً مؤقتاً، أما التحسن الحقيقي يحتاج استمراراً وروتيناً كاملاً.' },
      { question: 'هل يناسب البشرة الجافة؟', answer: 'غالباً نعم إذا كانت تركيبته مرطبة ولطيفة.' },
      { question: 'ما الكلمات المناسبة لهذا المقال؟', answer: 'سيروم كولاجين العراق، Medipeel Iraq، Korean collagen serum Iraq.' },
    ],
  },
]

export function findBlogPost(slug: string) {
  return blogPosts.find(p => p.slug === slug)
}
