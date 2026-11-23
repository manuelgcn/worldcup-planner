// Cuadro eliminatorio: al pasar el mouse o el foco por un equipo se marca su fila en todas las
// rondas y el resto se atenúa (los estilos están en Fixture.razor.css). Escucha en document porque
// Blazor redibuja el cuadro, y marca con data-* porque Blazor reescribe el atributo class entero.
(() => {
    function follow(team) {
        const bracket = document.querySelector(".bracket-compact");
        if (!bracket) {
            return;
        }

        bracket.querySelectorAll("[data-followed]").forEach(row => row.removeAttribute("data-followed"));
        bracket.toggleAttribute("data-following", team !== null);
        if (team !== null) {
            bracket.querySelectorAll(`[data-team="${team}"]`).forEach(row => row.setAttribute("data-followed", ""));
        }
    }

    function teamAt(target) {
        return target instanceof Element
            ? target.closest(".bracket-compact [data-team]")?.dataset.team ?? null
            : null;
    }

    // Cuando el mouse sale de un equipo vuelve a mandar el que tenga el foco, si hay uno.
    document.addEventListener("mouseover", event => follow(teamAt(event.target) ?? teamAt(document.activeElement)));
    document.addEventListener("focusin", event => follow(teamAt(event.target)));
    document.addEventListener("focusout", () => follow(null));
})();
