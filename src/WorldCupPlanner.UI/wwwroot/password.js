// Campos de contraseña (PasswordField.razor): lo que solo se puede hacer en el navegador. Escucha en
// document porque Blazor dibuja los campos después.
(() => {
    // Un clic en el botón del ojo no le saca el foco al campo: se sigue escribiendo donde se estaba.
    // Con teclado el foco queda en el botón, como en cualquier botón de dos estados.
    document.addEventListener("mousedown", event => {
        if (event.target instanceof Element && event.target.closest(".password-toggle")) {
            event.preventDefault();
        }
    });

    // Aviso de Bloq Mayús: el estado de la tecla sale de un evento de teclado o de mouse, y el texto
    // del aviso viene en data-text del párrafo.
    function hintFor(target) {
        return target instanceof HTMLInputElement && target.dataset.capsHint
            ? document.getElementById(target.dataset.capsHint)
            : null;
    }

    function update(event) {
        const hint = hintFor(event.target);
        // Algunos eventos de teclado sintéticos (autocompletar) no traen getModifierState.
        if (hint && event.getModifierState) {
            hint.textContent = event.getModifierState("CapsLock") ? hint.dataset.text : "";
        }
    }

    document.addEventListener("keydown", update);
    document.addEventListener("keyup", update);
    document.addEventListener("mousedown", update);
    // Fuera del campo no se sabe si la tecla sigue activada: el aviso se retira.
    document.addEventListener("focusout", event => {
        const hint = hintFor(event.target);
        if (hint) {
            hint.textContent = "";
        }
    });
})();
