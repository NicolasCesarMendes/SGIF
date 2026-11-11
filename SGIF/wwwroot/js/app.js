// SGIF — único JavaScript do site (Vue 3, sem build).
//
// Como funciona: todo bloco da página com  data-vue="nome"  vira um mini-app Vue. O HTML que o servidor
// gerou dentro do bloco é o template, e os data-* do bloco viram props (data-url-form -> props.urlForm).
// Em .cshtml use  v-on:click  e  v-bind:x  (o "@" e o ":" curtos conflitam com o Razor / confundem).
//
// SEGURANÇA: o Vue interpreta {{ }} do HTML do bloco. Por isso NUNCA ponha texto vindo do banco ou digitado
// por usuários dentro de um bloco data-vue (ou use v-pre no elemento que o contém).
import { createApp, ref, computed, watch, onMounted } from '../lib/vue/vue.esm-browser.prod.js';

const componentes = {
  // Mensagem do servidor (TempData) que o usuário pode fechar
  fechavel: { setup: () => ({ aberto: ref(true) }) },

  // Atualização automática ("tempo real"): busca a página de novo e troca só a região marcada.
  // Pausa com a aba escondida, com modal aberto e após 15 min sem uso; segue o redirecionamento ao Login.
  // Dispara "sgif:atualizado" no document a cada ciclo.
  atualizacao: {
    props: ['regiao', 'segundos', 'inatividade'],
    setup(props) {
      const alvo = document.getElementById(props.regiao);
      const intervalo = Math.max(2, +props.segundos || 10) * 1000;
      const limite = (+props.inatividade || 900) * 1000;
      const pausado = ref(false);
      const status = ref(`Atualização automática a cada ${intervalo / 1000} s`);
      let inativo = false, ocupado = false, ultimaAtividade = Date.now();

      // o token anti-forgery muda a cada resposta: ignorá-lo evita trocar o conteúdo sem necessidade
      const normalizar = (html) => html.replace(/(name="__RequestVerificationToken"[^>]*?value=")[^"]*/g, '$1');
      const pode = () => !pausado.value && !inativo && !document.hidden && !document.querySelector('dialog[open]');

      async function atualizar() {
        if (ocupado || !alvo) return;
        ocupado = true;
        try {
          const r = await fetch(location.href, { cache: 'no-store', credentials: 'same-origin', headers: { 'X-Requested-With': 'atualizacao-automatica' } });
          if (r.redirected) return (location.href = r.url);          // sessão expirou ou conta desativada
          if (!r.ok) return (status.value = 'Sem resposta do servidor. Tentando de novo...');

          const nova = new DOMParser().parseFromString(await r.text(), 'text/html').getElementById(props.regiao);
          const mudou = !!nova && normalizar(nova.innerHTML) !== normalizar(alvo.innerHTML);
          if (mudou) alvo.innerHTML = nova.innerHTML;
          status.value = `Atualizado às ${new Date().toLocaleTimeString('pt-BR')} (a cada ${intervalo / 1000} s)`;
          document.dispatchEvent(new CustomEvent('sgif:atualizado', { detail: { mudou } }));
        } catch {
          status.value = 'Sem conexão. Tentando de novo...';
        } finally {
          ocupado = false;
        }
      }

      function alternar() {
        pausado.value = !pausado.value;
        if (pausado.value) status.value = 'Atualização automática pausada';
        else atualizar();
      }

      ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'].forEach((e) => document.addEventListener(e, () => {
        ultimaAtividade = Date.now();
        if (inativo) { inativo = false; atualizar(); }
      }, { passive: true }));
      document.addEventListener('visibilitychange', () => { if (pode()) atualizar(); });
      setInterval(() => {
        if (!inativo && Date.now() - ultimaAtividade > limite) {
          inativo = true;
          status.value = 'Atualização pausada por inatividade. Mexa na página para retomar.';
        } else if (pode()) atualizar();
      }, intervalo);

      return { pausado, status, alternar };
    },
  },

  // Gráficos de linha (SVG) da média e do máximo da frota, a partir do JSON de /Home/Serie
  graficos: {
    props: ['url'],
    setup(props) {
      const periodos = [['1h', 'Última hora'], ['24h', '24 horas'], ['7d', '7 dias'], ['30d', '30 dias']];
      const periodo = ref('24h');
      const series = ref([]);
      const erro = ref('');
      const carregando = ref(true);

      async function carregar() {
        try {
          const r = await fetch(`${props.url}?periodo=${periodo.value}`, { credentials: 'same-origin' });
          if (r.redirected) return (location.href = r.url);
          if (!r.ok) throw new Error();
          series.value = (await r.json()).series;
          erro.value = '';
        } catch {
          erro.value = 'Não foi possível carregar os gráficos agora.';
        } finally {
          carregando.value = false;
        }
      }
      onMounted(carregar);
      watch(periodo, carregar);
      document.addEventListener('sgif:atualizado', carregar);

      const W = 640, H = 240, PX = 44, PY = 20;
      const numero = (n) => n.toLocaleString('pt-BR', { maximumFractionDigits: 1 });
      const FORMATO = { '1h': { hour: '2-digit', minute: '2-digit' }, '24h': { hour: '2-digit', minute: '2-digit' }, '7d': { day: '2-digit', month: '2-digit', hour: '2-digit' }, '30d': { day: '2-digit', month: '2-digit' } };
      const hora = (t) => new Date(t).toLocaleString('pt-BR', FORMATO[periodo.value]).replace(',', '') + (periodo.value === '7d' ? 'h' : '');

      const graficos = computed(() => series.value.map((s) => {
        const pts = s.pontos.map((p) => ({ t: new Date(p.inicio).getTime(), media: p.media, max: p.maximo }));
        const base = { tipo: s.tipo, nome: s.nome, unidade: s.unidade };
        if (!pts.length) return base;

        const t0 = pts[0].t, t1 = pts[pts.length - 1].t;
        let lo = Math.min(...pts.map((p) => p.media)), hi = Math.max(...pts.map((p) => p.max));
        const folga = (hi - lo || 1) * 0.1;
        lo -= folga; hi += folga;

        const x = (t) => PX + ((t - t0) / (t1 - t0 || 1)) * (W - PX - 16);
        const y = (v) => H - PY - ((v - lo) / (hi - lo)) * (H - PY * 2 - 18);
        const linha = (k) => pts.map((p) => `${x(p.t).toFixed(1)},${y(p[k]).toFixed(1)}`).join(' ');

        return {
          ...base,
          linhaMedia: linha('media'),
          linhaMax: linha('max'),
          marcas: [lo, (lo + hi) / 2, hi].map((v) => ({ y: +y(v).toFixed(1), rotulo: numero(v) })),
          eixoX: [0, 0.25, 0.5, 0.75, 1].map((f) => ({ x: +x(t0 + f * (t1 - t0)).toFixed(1), rotulo: hora(t0 + f * (t1 - t0)), ancora: f === 0 ? 'start' : f === 1 ? 'end' : 'middle' })),
          ultima: numero(pts[pts.length - 1].media),
          maxima: numero(Math.max(...pts.map((p) => p.max))),
          tabela: pts.map((p) => ({ t: p.t, rotulo: hora(p.t), media: numero(p.media), max: numero(p.max) })),
        };
      }));

      return { periodos, periodo, graficos, erro, carregando };
    },
  },
};

document.querySelectorAll('[data-vue]').forEach((el) => {
  const { vue, ...props } = el.dataset;
  createApp(componentes[vue], props).mount(el);
});
