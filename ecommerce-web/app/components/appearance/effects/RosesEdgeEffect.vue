<template>
  <div class="roses-edge-layer">
    <span class="rose-bloom rose-top-left"></span>
    <span class="rose-bloom rose-mid-left"></span>
    <span class="rose-bloom rose-bottom-left"></span>
    <span class="rose-bloom rose-top-right"></span>
    <span class="rose-bloom rose-mid-right"></span>
    <span class="rose-bloom rose-bottom-right"></span>
  </div>
</template>

<style scoped>
.roses-edge-layer{
  position:absolute;
  inset:0;
  pointer-events:none;
  z-index:1;
  overflow:hidden;
}
.rose-bloom{
  position:absolute;
  width:170px;
  height:170px;
  opacity:.72;
  filter: drop-shadow(0 16px 26px rgba(218, 79, 136, .15));
}
.rose-bloom::before, .rose-bloom::after{
  content:"";
  position:absolute;
  inset:0;
  border-radius:50%;
}
.rose-bloom::before{
  background:
    radial-gradient(circle at 50% 50%, rgba(255,255,255,.22) 0 8%, rgba(255,255,255,0) 9%),
    radial-gradient(circle at 50% 50%, rgba(255,128,176,.95) 0 24%, rgba(240,79,149,.82) 25% 36%, rgba(224,58,134,.64) 37% 48%, rgba(224,58,134,0) 49%),
    radial-gradient(circle at 30% 35%, rgba(255,170,208,.88) 0 17%, rgba(255,170,208,0) 18%),
    radial-gradient(circle at 70% 35%, rgba(255,170,208,.88) 0 17%, rgba(255,170,208,0) 18%),
    radial-gradient(circle at 32% 72%, rgba(255,170,208,.88) 0 16%, rgba(255,170,208,0) 17%),
    radial-gradient(circle at 68% 72%, rgba(255,170,208,.88) 0 16%, rgba(255,170,208,0) 17%),
    radial-gradient(circle at 18% 54%, rgba(240,79,149,.72) 0 14%, rgba(240,79,149,0) 15%),
    radial-gradient(circle at 82% 54%, rgba(240,79,149,.72) 0 14%, rgba(240,79,149,0) 15%);
}
.rose-bloom::after{
  inset:auto 26px 8px 26px;
  height:54px;
  background: radial-gradient(circle at 50% 50%, rgba(255,192,221,.38), rgba(255,192,221,0) 70%);
  filter: blur(12px);
}
.rose-top-left{ top:18px; left:-58px; transform:rotate(-16deg); }
.rose-mid-left{ top:36%; left:-70px; transform:rotate(10deg) scale(.92); opacity:.56; }
.rose-bottom-left{ bottom:20px; left:-54px; transform:rotate(-8deg) scale(1.04); }
.rose-top-right{ top:18px; right:-58px; transform:rotate(16deg) scaleX(-1); }
.rose-mid-right{ top:36%; right:-70px; transform:rotate(-10deg) scale(.92) scaleX(-1); opacity:.56; }
.rose-bottom-right{ bottom:20px; right:-54px; transform:rotate(8deg) scale(1.04) scaleX(-1); }
@media (max-width: 640px){
  .rose-bloom{ width:120px; height:120px; }
  .rose-top-left,.rose-bottom-left{ left:-48px; }
  .rose-top-right,.rose-bottom-right{ right:-48px; }
  .rose-mid-left,.rose-mid-right{ display:none; }
}
</style>
