<template>
  <div class="absolute inset-0">
    <!-- Dark mode: neon + red glow -->
    <div class="hidden dark:block absolute inset-0">
      <div class="bf-neon absolute inset-0"></div>
      <div class="bf-glow absolute inset-0"></div>

      <!-- % particles -->
      <div class="absolute inset-0 overflow-hidden">
        <span v-for="n in 18" :key="n" class="bf-percent">%</span>
      </div>
    </div>

    <!-- Light mode: confetti + soft gradients -->
    <div class="block dark:hidden absolute inset-0">
      <div class="bf-light-grad absolute inset-0"></div>
      <div class="absolute inset-0 overflow-hidden">
        <span v-for="n in 28" :key="n" class="bf-confetti"></span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// purely decorative
</script>

<style scoped>
.bf-neon{
  background:
    repeating-linear-gradient(135deg, rgba(255,0,70,.10) 0 2px, transparent 2px 18px),
    radial-gradient(900px 500px at 10% 20%, rgba(255,0,70,.16), transparent 60%),
    radial-gradient(700px 420px at 80% 70%, rgba(255,0,70,.12), transparent 60%);
  opacity: .9;
}
.bf-glow{
  box-shadow: inset 0 0 120px rgba(255,0,70,.16);
}

.bf-percent{
  position:absolute;
  top: -20px;
  left: calc(var(--x) * 1%);
  font-weight: 900;
  font-size: calc(14px + var(--s) * 1px);
  color: rgba(255,0,70,.55);
  text-shadow: 0 0 12px rgba(255,0,70,.45);
  animation: bf-fall calc(7s + var(--d) * 0.2s) linear infinite;
  transform: translateY(-40px);
}

/* توزيع ثابت عبر nth-child */
.bf-percent:nth-child(1){--x:7;--s:8;--d:1}
.bf-percent:nth-child(2){--x:15;--s:2;--d:4}
.bf-percent:nth-child(3){--x:22;--s:6;--d:2}
.bf-percent:nth-child(4){--x:31;--s:4;--d:6}
.bf-percent:nth-child(5){--x:38;--s:10;--d:3}
.bf-percent:nth-child(6){--x:45;--s:1;--d:7}
.bf-percent:nth-child(7){--x:52;--s:7;--d:5}
.bf-percent:nth-child(8){--x:59;--s:3;--d:9}
.bf-percent:nth-child(9){--x:66;--s:9;--d:8}
.bf-percent:nth-child(10){--x:73;--s:5;--d:11}
.bf-percent:nth-child(11){--x:79;--s:2;--d:10}
.bf-percent:nth-child(12){--x:84;--s:6;--d:12}
.bf-percent:nth-child(13){--x:89;--s:3;--d:13}
.bf-percent:nth-child(14){--x:93;--s:8;--d:15}
.bf-percent:nth-child(15){--x:12;--s:4;--d:16}
.bf-percent:nth-child(16){--x:27;--s:7;--d:18}
.bf-percent:nth-child(17){--x:61;--s:5;--d:17}
.bf-percent:nth-child(18){--x:96;--s:1;--d:19}

@keyframes bf-fall{
  0%{ transform: translateY(-60px) rotate(0deg); opacity: .1; }
  10%{ opacity: .8; }
  100%{ transform: translateY(120vh) rotate(90deg); opacity: .0; }
}

.bf-light-grad{
  background:
    radial-gradient(800px 450px at 20% 10%, rgba(255,100,120,.18), transparent 60%),
    radial-gradient(700px 420px at 80% 70%, rgba(255,200,80,.14), transparent 60%),
    linear-gradient(180deg, rgba(255,255,255,.0), rgba(255,255,255,.0));
}

.bf-confetti{
  position:absolute;
  top:-10px;
  left: calc(var(--x) * 1%);
  width: calc(4px + var(--w) * 1px);
  height: calc(8px + var(--h) * 1px);
  background: rgba(255,0,70,.35);
  border-radius: 2px;
  transform: rotate(calc(var(--r) * 1deg));
  animation: confetti-fall calc(6s + var(--d) * .25s) linear infinite;
  opacity:.85;
}

.bf-confetti:nth-child(1){--x:4;--w:2;--h:6;--r:20;--d:1}
.bf-confetti:nth-child(2){--x:9;--w:4;--h:5;--r:60;--d:3}
.bf-confetti:nth-child(3){--x:13;--w:1;--h:7;--r:100;--d:2}
.bf-confetti:nth-child(4){--x:19;--w:3;--h:6;--r:140;--d:6}
.bf-confetti:nth-child(5){--x:24;--w:2;--h:4;--r:10;--d:7}
.bf-confetti:nth-child(6){--x:28;--w:4;--h:7;--r:80;--d:5}
.bf-confetti:nth-child(7){--x:33;--w:1;--h:5;--r:120;--d:8}
.bf-confetti:nth-child(8){--x:37;--w:3;--h:6;--r:30;--d:10}
.bf-confetti:nth-child(9){--x:42;--w:2;--h:7;--r:70;--d:9}
.bf-confetti:nth-child(10){--x:46;--w:4;--h:4;--r:150;--d:12}
.bf-confetti:nth-child(11){--x:51;--w:2;--h:6;--r:110;--d:11}
.bf-confetti:nth-child(12){--x:55;--w:3;--h:5;--r:40;--d:13}
.bf-confetti:nth-child(13){--x:60;--w:1;--h:7;--r:90;--d:14}
.bf-confetti:nth-child(14){--x:64;--w:4;--h:6;--r:160;--d:15}
.bf-confetti:nth-child(15){--x:69;--w:2;--h:5;--r:130;--d:16}
.bf-confetti:nth-child(16){--x:73;--w:3;--h:7;--r:50;--d:17}
.bf-confetti:nth-child(17){--x:78;--w:1;--h:4;--r:20;--d:18}
.bf-confetti:nth-child(18){--x:82;--w:4;--h:6;--r:100;--d:19}
.bf-confetti:nth-child(19){--x:87;--w:2;--h:7;--r:140;--d:20}
.bf-confetti:nth-child(20){--x:91;--w:3;--h:5;--r:70;--d:21}
.bf-confetti:nth-child(21){--x:96;--w:1;--h:6;--r:120;--d:22}
.bf-confetti:nth-child(22){--x:6;--w:3;--h:5;--r:30;--d:23}
.bf-confetti:nth-child(23){--x:17;--w:2;--h:6;--r:60;--d:24}
.bf-confetti:nth-child(24){--x:35;--w:4;--h:7;--r:110;--d:25}
.bf-confetti:nth-child(25){--x:58;--w:1;--h:4;--r:150;--d:26}
.bf-confetti:nth-child(26){--x:76;--w:3;--h:6;--r:80;--d:27}
.bf-confetti:nth-child(27){--x:88;--w:2;--h:5;--r:100;--d:28}
.bf-confetti:nth-child(28){--x:98;--w:4;--h:7;--r:140;--d:29}

@keyframes confetti-fall{
  0%{ transform: translateY(-20px) rotate(0deg); opacity: .0; }
  10%{ opacity: .9; }
  100%{ transform: translateY(120vh) rotate(180deg); opacity: .0; }
}
</style>
