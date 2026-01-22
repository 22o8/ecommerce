<template>
  <span class="inline-flex items-center justify-center" :class="wrapperClass" aria-hidden="true">
    <svg v-if="svg" :viewBox="svg.viewBox" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
         class="block" :class="svgClass">
      <path v-for="(d,i) in svg.paths" :key="i" :d="d" />
      <circle v-for="(c,i) in (svg.circles||[])" :key="'c'+i" :cx="c[0]" :cy="c[1]" :r="c[2]" />
      <polyline v-for="(p,i) in (svg.polylines||[])" :key="'p'+i" :points="p" />
      <line v-for="(l,i) in (svg.lines||[])" :key="'l'+i" :x1="l[0]" :y1="l[1]" :x2="l[2]" :y2="l[3]" />
    </svg>
    <span v-else class="select-none opacity-70">●</span>
  </span>
</template>

<script setup lang="ts">
const props = defineProps<{
  name: string
  class?: string
}>()

const svgClass = computed(() => props.class || '')
const wrapperClass = computed(() => '')

type SvgDef = { viewBox: string; paths: string[]; circles?: [number,number,number][]; polylines?: string[]; lines?: [number,number,number,number][] }

const icons: Record<string, SvgDef> = {
  // Basic set (lightweight, no external deps)
  'mdi:arrow-right': { viewBox:'0 0 24 24', paths:['M5 12h12','M13 6l6 6-6 6'] },
  'mdi:flash-outline': { viewBox:'0 0 24 24', paths:['M13 2L3 14h7l-1 8 10-12h-7l1-8z'] },
  'mdi:image-outline': { viewBox:'0 0 24 24', paths:['M4 6h16v12H4z','M8 10a2 2 0 1 0 0.01 0','M4 16l5-5 4 4 3-3 4 4'] },
  'mdi:star-outline': { viewBox:'0 0 24 24', paths:['M12 2l3.1 6.3L22 9.2l-5 4.8L18.2 21 12 17.8 5.8 21 7 14 2 9.2l6.9-0.9L12 2z'] },
  'mdi:account-lock-outline': { viewBox:'0 0 24 24', paths:['M12 12a4 4 0 1 0-4-4 4 4 0 0 0 4 4z','M4 20a8 8 0 0 1 16 0','M17 12v-1a2 2 0 0 1 4 0v1','M15 12h6v5a2 2 0 0 1-2 2h-2a2 2 0 0 1-2-2z'] },
  'mdi:login-variant': { viewBox:'0 0 24 24', paths:['M10 17l1.5-1.5L9 13h10v-2H9l2.5-2.5L10 7l-5 5z','M20 3h-8v2h8v14h-8v2h8a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2z'] },
  'mdi:receipt-text-outline': { viewBox:'0 0 24 24', paths:['M6 2l2 2 2-2 2 2 2-2 2 2 2-2v20l-2-2-2 2-2-2-2 2-2-2-2 2-2-2-2 2V2z','M8 8h8','M8 12h8','M8 16h5'] },
  'mdi:shield-check-outline': { viewBox:'0 0 24 24', paths:['M12 2l8 4v6c0 5-3.4 9.7-8 10-4.6-.3-8-5-8-10V6l8-4z','M8 12l2.5 2.5L16 9'] },
  'mdi:whatsapp': { viewBox:'0 0 24 24', paths:['M20 11.5A8.5 8.5 0 1 1 11.5 3 8.5 8.5 0 0 1 20 11.5z','M7 20l1-3','M9.3 9.3c.5-1 1-1 1.4-.2l.8 1.6c.2.4.1.7-.1 1l-.4.5c-.2.2-.1.5.1.7 1 1.1 2.2 2 3.6 2.5.3.1.6 0 .8-.2l.5-.6c.2-.3.6-.4 1-.3l1.7.6c.8.3.9.9.5 1.5-.7 1-1.8 1.5-3.1 1.3-3.4-.6-6.7-3.9-7.4-7.3-.2-1.2.2-2.3 1-3.1z'] },
  'mdi:shopping-search-outline': { viewBox:'0 0 24 24', paths:['M6 7h15l-1.5 9h-12z','M6 7l-1-3H2','M9 20a1 1 0 1 0 0.01 0','M17 20a1 1 0 1 0 0.01 0','M19 11a3 3 0 1 0 0.01 0','M22 14l-1.5-1.5'] },
  'mdi:message-text-outline': { viewBox:'0 0 24 24', paths:['M4 4h16v12H7l-3 3V4z','M7 8h10','M7 12h7'] },
  'mdi:truck-fast-outline': { viewBox:'0 0 24 24', paths:['M3 7h11v10H3z','M14 10h4l3 3v4h-7z','M7 17a2 2 0 1 0 0.01 0','M18 17a2 2 0 1 0 0.01 0','M2 10h3','M2 12h2'] },
  'mdi:headset': { viewBox:'0 0 24 24', paths:['M4 12a8 8 0 0 1 16 0','M4 12v6a2 2 0 0 0 2 2h2v-6H6a2 2 0 0 1-2-2z','M20 12v6a2 2 0 0 1-2 2h-2v-6h2a2 2 0 0 0 2-2z'] },
  'mdi:package-variant-closed': { viewBox:'0 0 24 24', paths:['M21 8l-9 5-9-5 9-5 9 5z','M3 8v10l9 5 9-5V8','M12 13v10'] },
}

const svg = computed(() => icons[props.name])
</script>
