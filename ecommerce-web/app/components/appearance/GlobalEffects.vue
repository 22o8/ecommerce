<template>
  <div v-if="enabled" class="pointer-events-none absolute inset-x-0 top-0 z-[5] h-[340px] overflow-hidden sm:h-[420px] lg:h-[500px]">
    <div class="absolute inset-0 bg-gradient-to-b from-black/6 via-transparent to-transparent dark:from-white/4"></div>

    <SnowEffect v-if="resolvedEffects.snow || resolvedEffects.christmas" />

    <div v-if="resolvedEffects.ramadan" class="absolute inset-0">
      <div class="absolute top-8 right-5 hidden dark:block sm:right-8">
        <MoonIcon class="h-16 w-16 opacity-65 animate-float sm:h-20 sm:w-20" />
      </div>
      <div class="absolute top-8 right-5 block dark:hidden sm:right-8">
        <LanternIcon class="h-16 w-16 opacity-75 animate-float sm:h-20 sm:w-20" />
      </div>
      <Sparkles v-if="!liteMode" class="absolute inset-0" />
    </div>

    <div v-if="resolvedEffects.eid" class="absolute inset-x-0 top-16 flex justify-center px-4 sm:top-20">
      <div class="eid-banner rounded-[28px] border border-app px-5 py-4 backdrop-blur-sm shadow-xl sm:px-7">
        <div class="flex items-center gap-4">
          <div class="eid-sheep" aria-hidden="true">
            <span class="ear ear-left"></span>
            <span class="ear ear-right"></span>
            <span class="body"></span>
            <span class="face"></span>
            <span class="horn horn-left"></span>
            <span class="horn horn-right"></span>
            <span class="eye eye-left"></span>
            <span class="eye eye-right"></span>
            <span class="leg leg-1"></span>
            <span class="leg leg-2"></span>
            <span class="leg leg-3"></span>
            <span class="leg leg-4"></span>
          </div>
          <div class="min-w-0 text-center sm:text-start">
            <div class="text-xs font-semibold tracking-[0.2em] text-[rgb(var(--muted))] uppercase">Eid</div>
            <div class="text-base font-black text-[rgb(var(--text))] sm:text-lg">{{ t('seasonal.eidAdhaTitle') }}</div>
            <div class="text-xs text-[rgb(var(--muted))] sm:text-sm">{{ t('seasonal.eidAdhaSubtitle') }}</div>
          </div>
        </div>
      </div>
    </div>

    <HeartsEffect v-if="resolvedEffects.valentines" />
    <BlackFridayEffect v-if="resolvedEffects.blackFriday" />
  </div>
</template>

<script setup lang="ts">
import SnowEffect from '~/components/appearance/effects/SnowEffect.vue'
import Sparkles from '~/components/appearance/effects/Sparkles.vue'
import HeartsEffect from '~/components/appearance/effects/HeartsEffect.vue'
import BlackFridayEffect from '~/components/appearance/effects/BlackFridayEffect.vue'
import MoonIcon from '~/components/appearance/icons/MoonIcon.vue'
import LanternIcon from '~/components/appearance/icons/LanternIcon.vue'

const route = useRoute()
const store = useAppearanceStore()
const { t } = useI18n()
const { liteMode } = useMobilePerf()

const enabled = computed(() => route.path === '/')
const effects = computed(() => store.data.effects || {})
const resolvedEffects = computed(() => effects.value)
</script>

<style scoped>
.animate-float { animation: floaty 5.5s ease-in-out infinite; }
@keyframes floaty { 0%, 100% { transform: translateY(0px); } 50% { transform: translateY(-10px); } }
.eid-banner{ background: radial-gradient(120% 140% at 0% 0%, rgba(var(--primary), .12), transparent 55%), linear-gradient(180deg, rgba(var(--surface), .88), rgba(var(--surface), .78)); }
.eid-sheep{ position:relative; width:86px; height:68px; flex:0 0 auto; }
.eid-sheep .body{ position:absolute; left:16px; top:18px; width:46px; height:30px; border-radius:999px; background:#fff8ef; box-shadow:0 0 0 7px #fffdf8 inset, 0 10px 18px rgba(0,0,0,.08); }
.eid-sheep .face{ position:absolute; right:10px; top:24px; width:24px; height:22px; border-radius:999px; background:#8b6a58; }
.eid-sheep .ear{ position:absolute; top:27px; width:10px; height:12px; border-radius:999px; background:#b68b71; }
.eid-sheep .ear-left{ right:27px; transform:rotate(18deg); }
.eid-sheep .ear-right{ right:7px; transform:rotate(-18deg); }
.eid-sheep .horn{ position:absolute; top:18px; width:14px; height:14px; border:3px solid #caa56e; border-bottom-color:transparent; border-left-color:transparent; border-radius:999px; }
.eid-sheep .horn-left{ right:24px; transform:rotate(130deg); }
.eid-sheep .horn-right{ right:2px; transform:scaleX(-1) rotate(130deg); }
.eid-sheep .eye{ position:absolute; top:32px; width:3px; height:3px; border-radius:999px; background:#1f2937; }
.eid-sheep .eye-left{ right:24px; }
.eid-sheep .eye-right{ right:16px; }
.eid-sheep .leg{ position:absolute; bottom:6px; width:4px; height:16px; border-radius:999px; background:#8b6a58; }
.eid-sheep .leg-1{ left:24px; } .eid-sheep .leg-2{ left:36px; } .eid-sheep .leg-3{ left:50px; } .eid-sheep .leg-4{ left:60px; }
</style>
