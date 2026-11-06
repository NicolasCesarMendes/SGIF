// Modais do sistema (Minha conta, Usuários e Sensores).
//
// Abrir um formulário:  <button data-modal-url="/Usuarios/FormUsuario?id=3" data-modal-grande>
//   O HTML do formulário é carregado por fetch. Sucesso (200) = recarrega a página;
//   erro de validação (422) = o servidor devolve o formulário já com as mensagens.
//
// Pedir confirmação de exclusão:
//   <button data-excluir-url="/Usuarios/Excluir" data-excluir-id="3" data-excluir-texto="Deseja excluir...?">
(function () {
    const elForm = document.getElementById('modalForm');
    const elConfirmar = document.getElementById('modalConfirmar');

    if (!elForm || !elConfirmar) {
        return;
    }

    const modalForm = new bootstrap.Modal(elForm);
    const modalConfirmar = new bootstrap.Modal(elConfirmar);
    const dialogForm = document.getElementById('modalFormDialog');
    const corpoForm = document.getElementById('modalFormCorpo');

    const formConfirmar = document.getElementById('formConfirmar');
    const confirmarId = document.getElementById('confirmarId');
    const confirmarTexto = document.getElementById('confirmarTexto');
    const confirmarErro = document.getElementById('confirmarErro');

    // Se a sessão expirou, o servidor redireciona para o Login: acompanha o redirecionamento
    function sessaoExpirada(resposta) {
        if (resposta.redirected) {
            window.location.href = resposta.url;
            return true;
        }
        return false;
    }

    // Texto de erro devolvido pelo servidor (ignora páginas HTML de erro)
    async function mensagemDe(resposta, padrao) {
        const texto = (await resposta.text()).trim();
        return (texto === '' || texto.startsWith('<')) ? padrao : texto;
    }

    // Mostra uma mensagem de erro (texto puro) no lugar do conteúdo do modal
    function mostrarMensagem(texto) {
        corpoForm.innerHTML = '';

        const corpo = document.createElement('div');
        corpo.className = 'modal-body';
        const alerta = document.createElement('div');
        alerta.className = 'alert alert-danger mb-0';
        alerta.textContent = texto;
        corpo.appendChild(alerta);

        const rodape = document.createElement('div');
        rodape.className = 'modal-footer';
        const fechar = document.createElement('button');
        fechar.type = 'button';
        fechar.className = 'btn btn-outline-secondary';
        fechar.setAttribute('data-bs-dismiss', 'modal');
        fechar.textContent = 'Fechar';
        rodape.appendChild(fechar);

        corpoForm.appendChild(corpo);
        corpoForm.appendChild(rodape);
    }

    async function abrirFormulario(url, grande) {
        dialogForm.classList.toggle('modal-lg', grande);
        corpoForm.innerHTML = '<div class="modal-body text-center py-5">Carregando...</div>';
        modalForm.show();

        try {
            const resposta = await fetch(url);
            if (sessaoExpirada(resposta)) {
                return;
            }

            if (!resposta.ok) {
                mostrarMensagem(await mensagemDe(resposta, 'Não foi possível carregar o formulário.'));
                return;
            }

            corpoForm.innerHTML = await resposta.text();
        } catch (erro) {
            mostrarMensagem('Falha de conexão. Tente novamente.');
        }
    }

    // Botões da página (delegação de eventos)
    document.addEventListener('click', function (e) {
        const abrir = e.target.closest('[data-modal-url]');
        if (abrir) {
            e.preventDefault();
            abrirFormulario(abrir.dataset.modalUrl, 'modalGrande' in abrir.dataset);
            return;
        }

        const excluir = e.target.closest('[data-excluir-url]');
        if (excluir) {
            formConfirmar.action = excluir.dataset.excluirUrl;
            confirmarId.value = excluir.dataset.excluirId;
            confirmarTexto.textContent = excluir.dataset.excluirTexto || 'Deseja realmente excluir? Esta ação não pode ser desfeita.';
            confirmarErro.classList.add('d-none');
            modalConfirmar.show();
        }
    });

    // Envio de qualquer formulário que esteja dentro do modal
    corpoForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const form = e.target;
        const botao = e.submitter || form.querySelector('button[type="submit"]');
        if (botao) {
            botao.disabled = true;
        }

        try {
            const resposta = await fetch(form.action, { method: 'POST', body: new FormData(form) });
            if (sessaoExpirada(resposta)) {
                return;
            }

            if (resposta.ok) {
                window.location.reload();   // mantém a página, a busca e os filtros atuais
                return;
            }

            if (resposta.status === 422) {
                // Erro de validação: o servidor devolve o formulário com as mensagens
                corpoForm.innerHTML = await resposta.text();
                return;
            }

            mostrarMensagem(await mensagemDe(resposta, 'Não foi possível concluir a operação.'));
        } catch (erro) {
            if (botao) {
                botao.disabled = false;
            }
            alert('Falha de conexão. Tente novamente.');
        }
    });

    // Confirmação de exclusão
    formConfirmar.addEventListener('submit', async function (e) {
        e.preventDefault();

        const botao = formConfirmar.querySelector('button[type="submit"]');
        botao.disabled = true;
        confirmarErro.classList.add('d-none');

        try {
            const resposta = await fetch(formConfirmar.action, { method: 'POST', body: new FormData(formConfirmar) });
            if (sessaoExpirada(resposta)) {
                return;
            }

            if (resposta.ok) {
                window.location.reload();
                return;
            }

            confirmarErro.textContent = await mensagemDe(resposta, 'Não foi possível excluir. Tente novamente.');
            confirmarErro.classList.remove('d-none');
        } catch (erro) {
            confirmarErro.textContent = 'Falha de conexão. Tente novamente.';
            confirmarErro.classList.remove('d-none');
        } finally {
            botao.disabled = false;
        }
    });
})();
