<template>
  <div v-if="enabled" class="pointer-events-none fixed inset-0 z-[5]">
    <SnowEffect v-if="resolvedEffects.snow || resolvedEffects.christmas" />

    <!-- Ramadan: moon for dark, lantern for light -->
    <div v-if="resolvedEffects.ramadan" class="absolute inset-0">
      <div class="absolute top-10 right-6 hidden dark:block">
        <MoonIcon class="w-24 h-24 opacity-70 animate-float" />
      </div>
      <div class="absolute top-10 right-6 block dark:hidden">
        <LanternIcon class="w-24 h-24 opacity-80 animate-float" />
      </div>
      <Sparkles v-if="!liteMode" class="absolute inset-0" />
    </div>

    <!-- Eid: subtle sparkles -->
    <Sparkles v-if="resolvedEffects.eid" class="absolute inset-0" />

    <!-- Valentines: floating hearts -->
    <HeartsEffect v-if="resolvedEffects.valentines" />

    <!-- Black Friday: dark glow + neon / light confetti -->
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

const enabled = computed(() => {
  // don't distract inside admin
  return !route.path.startsWith('/admin')
})

const { liteMode, isMobile } = useMobilePerf()

const effects = computed(() => store.data.effects || {})
const mobileEffects = computed(() => ({
  snow: false,
  christmas: false,
  ramadan: !!effects.value?.ramadan,
  eid: false,
  valentines: false,
  blackFriday: !isMobile.value && !!effects.value?.blackFriday,
}))
const resolvedEffects = computed(() => (liteMode.value ? mobileEffects.value : effects.value))
</script>

<style scoped>
.animate-float {
  animation: floaty 5.5s ease-in-out infinite;
}
@keyframes floaty {
  0%, 100% { transform: translateY(0px); }
  50% { transform: translateY(-10px); }
}
</style>
