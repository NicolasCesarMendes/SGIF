// Atualização automática das telas ("tempo real").
//
// Como usar: marque a região da página que deve se atualizar sozinha (o id é obrigatório):
//   <div id="regiao-painel" data-atualizar="5"> ... </div>
//     data-atualizar = intervalo em segundos (mínimo 2). Com várias regiões, vale o menor intervalo.
//   <span data-atualizacao-status></span>          mostra o horário da última atualização
//   <button data-atualizacao-alternar></button>    pausa e retoma
//
// A cada ciclo a página atual é buscada de novo no servidor e SÓ as regiões marcadas são trocadas.
// Menu, modais e o resto da página não são tocados. A atualização:
//   - não roda com a aba escondida nem com um modal aberto;
//   - pausa depois de 15 minutos sem o usuário mexer na página (para a sessão poder expirar);
//   - segue o redirecionamento para o Login se a sessão acabou ou a conta foi desativada.
//
// Depois de cada atualização dispara o evento "sgif:atualizado" no document
// (event.detail.mudou diz se algo mudou). Os gráficos podem escutá-lo para buscar os novos pontos.
(function () {
    'use strict';

    const regioes = Array.from(document.querySelectorAll('[data-atualizar]')).filter(function (r) {
        return r.id;
    });

    if (regioes.length === 0) {
        return;
    }

    const segundos = Math.max(2, Math.min.apply(null, regioes.map(function (r) {
        return parseInt(r.dataset.atualizar, 10) || 10;
    })));

    // Tempo sem atividade do usuário para pausar (em segundos). Padrão: 15 minutos.
    const limiteInatividade = (parseInt(regioes[0].dataset.atualizarInatividade, 10) || 900) * 1000;

    const indicadores = Array.from(document.querySelectorAll('[data-atualizacao-status]'));
    const botoes = Array.from(document.querySelectorAll('[data-atualizacao-alternar]'));

    let pausadoPeloUsuario = false;
    let emAndamento = false;
    let ultimaAtividade = Date.now();
    let inativo = false;

    function mostrar(texto) {
        indicadores.forEach(function (i) { i.textContent = texto; });
    }

    function mostrarHora() {
        const h = new Date();
        const dois = function (n) { return (n < 10 ? '0' : '') + n; };
        mostrar('Atualizado às ' + dois(h.getHours()) + ':' + dois(h.getMinutes()) + ':' + dois(h.getSeconds())
            + ' (a cada ' + segundos + ' s)');
    }

    // Os formulários trazem um token anti-forgery diferente a cada resposta: ignorá-lo na comparação
    // evita trocar o conteúdo (e piscar a tela) quando nada mudou de verdade.
    function normalizar(html) {
        return html.replace(/(name="__RequestVerificationToken"[^>]*?value=")[^"]*/g, '$1');
    }

    function podeAtualizar() {
        return !pausadoPeloUsuario
            && !inativo
            && !document.hidden
            && !document.querySelector('.modal.show');
    }

    async function atualizar() {
        if (emAndamento) {
            return;
        }

        emAndamento = true;

        try {
            const resposta = await fetch(window.location.href, {
                cache: 'no-store',
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'atualizacao-automatica' }
            });

            // Sessão expirada, conta desativada ou sem permissão: acompanha o redirecionamento
            if (resposta.redirected) {
                window.location.href = resposta.url;
                return;
            }

            if (!resposta.ok) {
                mostrar('Sem resposta do servidor. Tentando de novo...');
                return;
            }

            const documentoNovo = new DOMParser().parseFromString(await resposta.text(), 'text/html');
            let mudou = false;

            regioes.forEach(function (atual) {
                const nova = documentoNovo.getElementById(atual.id);

                if (nova && normalizar(nova.innerHTML) !== normalizar(atual.innerHTML)) {
                    atual.innerHTML = nova.innerHTML;
                    mudou = true;
                }
            });

            mostrarHora();
            document.dispatchEvent(new CustomEvent('sgif:atualizado', { detail: { mudou: mudou } }));
        } catch (erro) {
            mostrar('Sem conexão. Tentando de novo...');
        } finally {
            emAndamento = false;
        }
    }

    function atualizarBotoes() {
        botoes.forEach(function (b) { b.textContent = pausadoPeloUsuario ? 'Retomar' : 'Pausar'; });
    }

    botoes.forEach(function (b) {
        b.addEventListener('click', function () {
            pausadoPeloUsuario = !pausadoPeloUsuario;
            atualizarBotoes();

            if (pausadoPeloUsuario) {
                mostrar('Atualização automática pausada');
            } else {
                atualizar();
            }
        });
    });

    // Atividade do usuário: serve para pausar quando a página fica parada por muito tempo
    ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'].forEach(function (evento) {
        document.addEventListener(evento, function () {
            ultimaAtividade = Date.now();

            if (inativo) {
                inativo = false;
                atualizar();
            }
        }, { passive: true });
    });

    // Voltou para a aba: atualiza na hora
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden && podeAtualizar()) {
            atualizar();
        }
    });

    setInterval(function () {
        if (!inativo && Date.now() - ultimaAtividade > limiteInatividade) {
            inativo = true;
            mostrar('Atualização pausada por inatividade. Mexa na página para retomar.');
            return;
        }

        if (podeAtualizar()) {
            atualizar();
        }
    }, segundos * 1000);

    atualizarBotoes();
    mostrar('Atualização automática a cada ' + segundos + ' s');
})();
