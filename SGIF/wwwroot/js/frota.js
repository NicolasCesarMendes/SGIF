// Frota: modais de cadastro/edição e de exclusão.
// Os formulários são enviados por fetch, então os erros aparecem dentro do próprio modal.
(function () {
    const modalAtivoEl = document.getElementById('modalAtivo');
    const modalExcluirEl = document.getElementById('modalExcluir');

    // Quem só consulta (Funcionário) não tem os modais na página
    if (!modalAtivoEl || !modalExcluirEl) {
        return;
    }

    const modalAtivo = new bootstrap.Modal(modalAtivoEl);
    const modalExcluir = new bootstrap.Modal(modalExcluirEl);
    const corpoAtivo = document.getElementById('modalAtivoCorpo');

    const formExcluir = document.getElementById('formExcluir');
    const excluirTipo = document.getElementById('excluirTipo');
    const excluirId = document.getElementById('excluirId');
    const excluirTexto = document.getElementById('excluirTexto');
    const excluirErro = document.getElementById('excluirErro');

    // Se a sessão expirou, o servidor redireciona para o Login: acompanha o redirecionamento
    function sessaoExpirada(resposta) {
        if (resposta.redirected) {
            window.location.href = resposta.url;
            return true;
        }
        return false;
    }

    async function abrirFormulario(tipo, id) {
        corpoAtivo.innerHTML = '<div class="modal-body text-center py-5">Carregando...</div>';
        modalAtivo.show();

        let url = modalAtivoEl.dataset.urlForm + '?tipo=' + encodeURIComponent(tipo);
        if (id) {
            url += '&id=' + encodeURIComponent(id);
        }

        try {
            const resposta = await fetch(url);
            if (sessaoExpirada(resposta)) {
                return;
            }
            if (!resposta.ok) {
                corpoAtivo.innerHTML = '<div class="modal-body text-danger">Não foi possível carregar o formulário.</div>';
                return;
            }
            corpoAtivo.innerHTML = await resposta.text();
        } catch (erro) {
            corpoAtivo.innerHTML = '<div class="modal-body text-danger">Falha de conexão. Tente novamente.</div>';
        }
    }

    // Botões da página (delegação de eventos)
    document.addEventListener('click', function (e) {
        const novo = e.target.closest('[data-novo]');
        if (novo) {
            abrirFormulario(novo.dataset.novo, null);
            return;
        }

        const editar = e.target.closest('[data-editar]');
        if (editar) {
            abrirFormulario(editar.dataset.tipo, editar.dataset.id);
            return;
        }

        const excluir = e.target.closest('[data-excluir]');
        if (excluir) {
            excluirTipo.value = excluir.dataset.tipo;
            excluirId.value = excluir.dataset.id;
            excluirTexto.textContent = 'Deseja realmente excluir ' + excluir.dataset.nome + '? Esta ação não pode ser desfeita.';
            excluirErro.classList.add('d-none');
            modalExcluir.show();
        }
    });

    // Envio do formulário de cadastro/edição
    corpoAtivo.addEventListener('submit', async function (e) {
        e.preventDefault();

        const form = e.target;
        const botao = form.querySelector('button[type="submit"]');
        if (botao) {
            botao.disabled = true;
        }

        try {
            const resposta = await fetch(form.action, { method: 'POST', body: new FormData(form) });
            if (sessaoExpirada(resposta)) {
                return;
            }

            if (resposta.ok) {
                window.location.reload();   // mantém a busca e os filtros atuais
                return;
            }

            // Erro de validação: o servidor devolve o formulário com as mensagens
            corpoAtivo.innerHTML = await resposta.text();
        } catch (erro) {
            if (botao) {
                botao.disabled = false;
            }
            alert('Falha de conexão. Tente novamente.');
        }
    });

    // Confirmação de exclusão
    formExcluir.addEventListener('submit', async function (e) {
        e.preventDefault();

        const botao = formExcluir.querySelector('button[type="submit"]');
        botao.disabled = true;
        excluirErro.classList.add('d-none');

        try {
            const resposta = await fetch(formExcluir.action, { method: 'POST', body: new FormData(formExcluir) });
            if (sessaoExpirada(resposta)) {
                return;
            }

            if (resposta.ok) {
                window.location.reload();
                return;
            }

            excluirErro.textContent = await resposta.text();
            excluirErro.classList.remove('d-none');
        } catch (erro) {
            excluirErro.textContent = 'Falha de conexão. Tente novamente.';
            excluirErro.classList.remove('d-none');
        } finally {
            botao.disabled = false;
        }
    });
})();
