// Tema "cancha de noche". Va en <head> sin defer ni async: pone data-theme en <html> antes del
// primer pintado, así la página nunca parpadea en el tema equivocado.
(() => {
    const STORAGE_KEY = "golazo.theme";
    const NIGHT = "night";
    const DAY = "day";
    // Barra del navegador en el celular: verde cancha de día, verde noche (--m26-chalk) de noche.
    const THEME_COLOR = { day: "#0E4D2C", night: "#0A2014" };
    const systemDark = window.matchMedia("(prefers-color-scheme: dark)");

    // Sin elección guardada manda el sistema. localStorage puede estar bloqueado: se sigue sin él.
    let chosen = null;
    try {
        const stored = localStorage.getItem(STORAGE_KEY);
        chosen = stored === NIGHT || stored === DAY ? stored : null;
    } catch {
        // Bloqueado: queda la preferencia del sistema.
    }

    function currentTheme() {
        return chosen ?? (systemDark.matches ? NIGHT : DAY);
    }

    function apply() {
        const theme = currentTheme();
        const night = theme === NIGHT;
        document.documentElement.dataset.theme = theme;
        document.querySelector('meta[name="color-scheme"]')?.setAttribute("content", night ? "dark" : "light");
        document.querySelector('meta[name="theme-color"]')?.setAttribute("content", THEME_COLOR[theme]);
        document.querySelectorAll("[data-theme-toggle]").forEach(button => button.setAttribute("aria-pressed", String(night)));
    }

    apply();
    systemDark.addEventListener("change", apply);

    document.addEventListener("click", event => {
        if (!event.target.closest("[data-theme-toggle]")) {
            return;
        }

        chosen = currentTheme() === NIGHT ? DAY : NIGHT;
        try {
            localStorage.setItem(STORAGE_KEY, chosen);
        } catch {
            // Bloqueado: la elección vale hasta recargar.
        }
        apply();
    });

    // MainLayout lo llama al dibujar el botón: el servidor no sabe qué tema tiene el navegador.
    window.golazoTheme = { sync: apply };
})();
