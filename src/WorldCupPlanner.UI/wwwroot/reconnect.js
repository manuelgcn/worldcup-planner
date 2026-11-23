// Aviso de conexión cortada (#components-reconnect-modal en App.razor). Blazor solo le cambia la
// clase; el botón "Recargar" y el foco se atienden acá porque el circuito ya no responde.
(() => {
    const modal = document.getElementById("components-reconnect-modal");
    const reload = modal.querySelector("[data-reload]");

    reload.addEventListener("click", () => location.reload());

    // Cuando ya no se reintenta, el foco va al único control que todavía sirve.
    new MutationObserver(() => {
        if (modal.matches(".components-reconnect-failed, .components-reconnect-rejected")) {
            reload.focus();
        }
    }).observe(modal, { attributes: true, attributeFilter: ["class"] });
})();
