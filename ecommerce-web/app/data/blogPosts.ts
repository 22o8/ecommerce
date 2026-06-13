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
}

export const blogPosts: BlogPost[] = [
  {
    title: 'أفضل تونر كوري للبشرة الحساسة والجافة',
    slug: 'best-korean-toner',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'أفضل تونر كوري للبشرة | DR SEOUL BEAUTY',
    metaDescription: 'دليل سريع لاختيار أفضل تونر كوري للبشرة الحساسة والجافة والدهنية مع نصائح استخدام قبل السيروم والمرطب.',
    keywords: ['best korean toner', 'تونر كوري', 'تونر للبشرة الحساسة', 'korean skincare iraq'],
    content: `التونر الكوري يساعد على تهيئة البشرة بعد الغسول، ويفضل اختيار تركيبة خفيفة تحتوي مكونات مهدئة ومرطبة. للبشرة الحساسة ابحث عن Heartleaf أو Centella أو Hyaluronic Acid، وللبشرة الدهنية اختر تونر خفيف غير دهني. استخدمه صباحاً ومساءً قبل السيروم والمرطب.`,
    faq: [
      { question: 'هل التونر ضروري في الروتين الكوري؟', answer: 'ليس إجبارياً، لكنه يساعد على الترطيب وتجهيز البشرة لاستقبال السيروم والمرطب.' },
      { question: 'كم مرة أستخدم التونر؟', answer: 'غالباً مرة إلى مرتين يومياً حسب تحمل البشرة وتعليمات المنتج.' },
    ],
  },
  {
    title: 'أفضل سيروم لحب الشباب وآثار الحبوب',
    slug: 'best-acne-serum',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'أفضل سيروم لحب الشباب | Korean Skincare Iraq',
    metaDescription: 'تعرف على طريقة اختيار سيروم مناسب لحب الشباب وآثار الحبوب ضمن روتين عناية كوري بسيط وآمن.',
    keywords: ['best acne serum', 'سيروم حب الشباب', 'آثار الحبوب', 'k beauty iraq'],
    content: `السيروم المناسب لحب الشباب يجب أن يكون خفيفاً وغير كوميدوجينيك. ابحث عن مكونات مثل Niacinamide وCentella وTea Tree، وتجنب خلط الكثير من المقشرات في نفس الروتين. ابدأ تدريجياً وراقب استجابة البشرة.`,
    faq: [
      { question: 'هل السيروم يعالج حب الشباب وحده؟', answer: 'قد يساعد، لكن الحالات الشديدة تحتاج مراجعة مختص جلدية.' },
      { question: 'هل أستخدم السيروم مع واقي الشمس؟', answer: 'نعم، واقي الشمس مهم خصوصاً عند استخدام مكونات فعالة أو مقشرة.' },
    ],
  },
  {
    title: 'روتين العناية الكوري اليومي خطوة بخطوة',
    slug: 'korean-skincare-routine',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'روتين العناية الكوري اليومي | DR SEOUL BEAUTY',
    metaDescription: 'روتين كوري مبسط: غسول، تونر، سيروم، مرطب، وواقي شمس مع ترتيب الاستخدام الصحيح.',
    keywords: ['korean skincare routine', 'روتين عناية كوري', 'واقي شمس كوري', 'سيروم كوري'],
    content: `ابدأ بغسول لطيف، ثم تونر مرطب، ثم سيروم حسب المشكلة، وبعده مرطب مناسب. في الصباح لا تنس واقي الشمس. لا تحتاج إلى عشر خطوات دائماً؛ الأهم اختيار المنتجات المناسبة لبشرتك والاستمرار.`,
    faq: [
      { question: 'ما أهم خطوة صباحاً؟', answer: 'واقي الشمس هو أهم خطوة لحماية البشرة وتقليل التصبغات.' },
      { question: 'هل أحتاج روتين طويل؟', answer: 'لا، روتين بسيط وثابت أفضل من خلط منتجات كثيرة.' },
    ],
  },
  {
    title: 'Anua vs COSRX: أيهما أفضل لبشرتك؟',
    slug: 'anua-vs-cosrx',
    coverImage: '/hero-placeholder.svg',
    metaTitle: 'Anua vs COSRX مقارنة براندات العناية الكورية',
    metaDescription: 'مقارنة مبسطة بين Anua و COSRX لاختيار المنتج الأنسب للبشرة الحساسة، الحبوب، والترطيب.',
    keywords: ['Anua vs COSRX', 'Anua Iraq', 'COSRX Iraq', 'korean skincare comparison'],
    content: `Anua تشتهر بمنتجات مهدئة وخفيفة مناسبة للبشرة الحساسة، بينما COSRX معروف بمنتجات عملية لعلاج الحبوب والملمس والترطيب. الاختيار يعتمد على المشكلة الأساسية: تهدئة واحمرار، أو حبوب ومسام وملمس.`,
    faq: [
      { question: 'هل Anua مناسب للبشرة الحساسة؟', answer: 'كثير من منتجات Anua موجهة للتهدئة، لكن يجب اختبار المنتج على جزء صغير أولاً.' },
      { question: 'هل COSRX قوي على البشرة؟', answer: 'بعض منتجاته فعالة وقد تكون قوية، لذلك يفضل الاستخدام التدريجي.' },
    ],
  },
]

export function findBlogPost(slug: string) {
  return blogPosts.find(p => p.slug === slug)
}
