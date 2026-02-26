// app/composables/useMoney.ts
// توحيد تنسيق الدينار العراقي (أرقام إنكليزية + د.ع)

export function formatIqd(value: any) {
  const n = Number(value ?? 0)
  const safe = Number.isFinite(n) ? Math.round(n) : 0
  // أرقام إنكليزية
  const num = safe.toLocaleString('en-US')
  return `${num} د.ع`
}
