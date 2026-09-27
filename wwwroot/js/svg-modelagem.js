class SvgModelagemRenderer {

    constructor(categoria) {

        this.svg = document.getElementById("camisetaSvg");

        if (!this.svg) return;

        this.render(categoria);

    }

    mostrar(id, visivel) {

        const el = this.svg.querySelector(`#${id}`);

        if (el) {

            el.setAttribute(
                "display",
                visivel ? "inline" : "none"
            );

        }

    }

    render(categoria) {

        this.mostrar("golaRedonda", false);
        this.mostrar("golaPolo", false);
        this.mostrar("golaSocial", false);
        this.mostrar("mangaLonga", false);
        this.mostrar("punhos", false);
        this.mostrar("recorteDryFit", false);

        switch (categoria) {

            case "Polo":

                this.mostrar("golaPolo", true);
                break;

            case "Camisa Social":

                this.mostrar("golaSocial", true);
                this.mostrar("mangaLonga", true);
                this.mostrar("punhos", true);
                break;

            case "Moletom":

                this.mostrar("golaRedonda", true);
                this.mostrar("mangaLonga", true);
                this.mostrar("punhos", true);
                break;

            case "Dry Fit":

                this.mostrar("golaRedonda", true);
                this.mostrar("recorteDryFit", true);
                break;

            case "Regata":

                this.mostrar("golaRedonda", true);
                break;

            default:

                this.mostrar("golaRedonda", true);

        }

    }

}

document.addEventListener("DOMContentLoaded", () => {

    const container = document.querySelector(".container-fluid");
    const categoria = container?.dataset.categoria || "Camisa Malha";

    window.renderer = new SvgModelagemRenderer(categoria);

});