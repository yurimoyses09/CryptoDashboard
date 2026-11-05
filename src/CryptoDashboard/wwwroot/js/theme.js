// Tema claro/escuro do Tabler (atributo data-bs-theme no <html>).
// Carregado no <head> para aplicar o tema antes da primeira pintura e evitar piscar.
(function () {
    var KEY = "cd-theme";

    function stored() {
        try {
            var value = localStorage.getItem(KEY);
            return value === "light" || value === "dark" ? value : null;
        } catch (e) {
            return null;
        }
    }

    function systemTheme() {
        return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    function apply(theme) {
        document.documentElement.setAttribute("data-bs-theme", theme);
    }

    apply(stored() || systemTheme());

    window.cdTheme = {
        current: function () {
            return document.documentElement.getAttribute("data-bs-theme") || "light";
        },
        toggle: function () {
            var next = window.cdTheme.current() === "dark" ? "light" : "dark";
            apply(next);
            try { localStorage.setItem(KEY, next); } catch (e) { /* modo privado: o tema vale só nesta página */ }
            return next;
        }
    };
})();
