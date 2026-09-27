// ==========================================
// PROVADOR VIRTUAL - ETAPA 1
// ==========================================

document.addEventListener("DOMContentLoaded", function () {

    const btnAbrirProvador = document.getElementById("btnAbrirProvador");
    const modalCadastro = document.getElementById("modalCadastro");

    const imagemProvadorVirtual = document.getElementById('imagemProvadorVirtual');

    const btnFecharCadastro = document.getElementById("btnFecharCadastro");
    
 // ---------------------------------------------------------
// FUNÇÃO: obter imagem da cor atualmente selecionada
// ---------------------------------------------------------
function obterImagemAtualProvador() {

    // Imagem principal do produto como fallback
    let imagem = '@Model.Imagem';

    // Verifica se existe uma cor selecionada
    if (corSelecionada &&
        typeof imagensPorCor !== 'undefined') {

        const dadosCor = imagensPorCor[corSelecionada];

        // Se a cor possui uma imagem principal
        if (dadosCor && dadosCor.Principal) {

            imagem = dadosCor.Principal;
        }

        // Se não houver principal, tenta a primeira imagem da galeria
        else if (
            dadosCor &&
            dadosCor.Imagens &&
            dadosCor.Imagens.length > 0
        ) {

            imagem = dadosCor.Imagens[0];
        }
    }

    return imagem;
}

    console.log("Botão:", btnAbrirProvador);
    console.log("Modal:", modalCadastro);

 // ==========================================
// Abrir o modal do provador virtual
// ==========================================
    btnAbrirProvador?.addEventListener("click", function () {

        console.log("Abrindo modal...");
        // Descobre a imagem correspondente à cor selecionada
        const imagemAtual = obterImagemAtualProvador();

        // Atualiza a imagem do Provador
        if (imagemProvadorVirtual) {
            imagemProvadorVirtual.src = imagemAtual;
        }

        modalCadastro.classList.add("ativo");

    });

    btnFecharCadastro?.addEventListener("click", function () {

        modalCadastro.classList.remove("ativo");

    });

    modalCadastro?.addEventListener("click", function (e) {

        if (e.target === modalCadastro) {

            modalCadastro.classList.remove("ativo");

        }

    });

});

// ==========================================
// Seleção Masculino/Feminino
// ==========================================
const btnMasc =
document.getElementById("btnGeneroMasculino");

const btnFem =
document.getElementById("btnGeneroFeminino");

btnMasc?.addEventListener("click",()=>{

btnMasc.classList.add("ativo");

btnFem.classList.remove("ativo");

});

btnFem?.addEventListener("click",()=>{

btnFem.classList.add("ativo");

btnMasc.classList.remove("ativo");

});

// ==========================================
// Tooltip
// ==========================================
const infoIdade =
document.getElementById("infoIdade");

const tooltip =
document.getElementById("tooltipIdade");

infoIdade?.addEventListener("mouseenter",()=>{

tooltip.classList.add("ativo");

});

infoIdade?.addEventListener("mouseleave",()=>{

tooltip.classList.remove("ativo");

});

// ==========================================
// Habilitar Próximo
// ==========================================
const altura =
document.getElementById("alturaProvador");

const peso =
document.getElementById("pesoProvador");

const idade =
document.getElementById("idadeProvador");

const btnProximo =
document.getElementById("btnProximoProvador");

function validarCadastro(){

const ok =

altura.value>=100 &&
altura.value<=250 &&

peso.value>=30 &&
peso.value<=300 &&

idade.value>=10 &&
idade.value<=120;

btnProximo.disabled=!ok;

}

altura?.addEventListener("input",validarCadastro);
peso?.addEventListener("input",validarCadastro);
idade?.addEventListener("input",validarCadastro);

// ==========================================
// PROVADOR VIRTUAL - ETAPA 2
// Abrir o Modal Avatar
// ==========================================
const modalAvatar =
document.getElementById("modalAvatar");

const btnVoltarCadastro =
document.getElementById("btnVoltarCadastro");

btnProximo?.addEventListener("click",()=>{

    modalCadastro.classList.remove("ativo");

    modalAvatar.classList.add("ativo");

     window.inicializarAvatar?.();
});

// ==========================================
// Voltar
// ==========================================
btnVoltarCadastro?.addEventListener("click",()=>{

    modalAvatar.classList.remove("ativo");

    modalCadastro.classList.add("ativo");

});

// ==========================================
// Fechar
// ==========================================
document
.getElementById("btnFecharAvatar")
?.addEventListener("click",()=>{

    modalAvatar.classList.remove("ativo");

});

// ==========================================
// RÉGUAS PERSONALIZADAS
// ==========================================

const valoresCorpo = {

    torax:0,
    cintura:0,
    quadril:0

};

function atualizarRegua(nome){

    const ponteiro =
    document.getElementById(

        "ponteiro"+nome.charAt(0).toUpperCase()+nome.slice(1)

    );

    if(!ponteiro)return;

    const percentual =
    ((valoresCorpo[nome]+100)/200)*100;

    ponteiro.style.left=percentual+"%";

}

["torax","cintura","quadril"]
.forEach(atualizarRegua);

// ==========================================
// Botões + e −
// ==========================================
document
.querySelectorAll(".btn-regua")
.forEach(btn=>{

    btn.addEventListener("click",()=>{

        const alvo =
        btn.dataset.target;

        const passo =
        btn.classList.contains("mais")?5:-5;

        valoresCorpo[alvo]=

        Math.max(-100,

        Math.min(100,

        valoresCorpo[alvo]+passo));

        atualizarRegua(alvo);

        // depois ligaremos ao avatar Meshy

    });

});

// ==========================================
// Arrastar a bolinha
// ==========================================
document
.querySelectorAll(".trilho")
.forEach(trilho=>{

    const ponteiro =
    trilho.querySelector(".ponteiro");

    const nome =
    trilho.parentElement.dataset.slider;

    let arrastando=false;

    ponteiro.addEventListener("pointerdown",()=>{

        arrastando=true;

    });

    window.addEventListener("pointerup",()=>{

        arrastando=false;

    });

    window.addEventListener("pointermove",(e)=>{

        if(!arrastando)return;

        const r =
        trilho.getBoundingClientRect();

        let x =
        e.clientX-r.left;

        x=Math.max(0,Math.min(r.width,x));

        valoresCorpo[nome]=

        Math.round((x/r.width)*200-100);

        atualizarRegua(nome);

    });

});

// ==========================================
// Régua / sliders
// ==========================================
function alterarSlider(sliderId, valor){

    const slider =
    document.getElementById(sliderId);

    slider.value =
    Math.max(-100,
    Math.min(100,
    Number(slider.value)+valor));

}

document
.getElementById("maisTorax")
?.addEventListener("click",()=>alterarSlider("toraxSlider",5));

document
.getElementById("menosTorax")
?.addEventListener("click",()=>alterarSlider("toraxSlider",-5));

document
.getElementById("maisCintura")
?.addEventListener("click",()=>alterarSlider("cinturaSlider",5));

document
.getElementById("menosCintura")
?.addEventListener("click",()=>alterarSlider("cinturaSlider",-5));

document
.getElementById("maisQuadril")
?.addEventListener("click",()=>alterarSlider("quadrilSlider",5));

document
.getElementById("menosQuadril")
?.addEventListener("click",()=>alterarSlider("quadrilSlider",-5));

// ==========================================
// Criar o visualizador Three.js
// AVATAR MESHY
// ==========================================


/*let cenaAvatar;
let cameraAvatar;
let rendererAvatar;
let avatarMesh;

function inicializarAvatar(){

    const container =
    document.getElementById("avatar3DContainer");

    if(!container || rendererAvatar)return;

    cenaAvatar =
    new THREE.Scene();

    cameraAvatar =
    new THREE.PerspectiveCamera(

        35,

        container.clientWidth/container.clientHeight,

        0.1,

        100

    );

    cameraAvatar.position.set(0,1.2,3);

    rendererAvatar =
    new THREE.WebGLRenderer({

        antialias:true,
        alpha:true

    });

    rendererAvatar.setSize(

        container.clientWidth,

        container.clientHeight

    );

    rendererAvatar.setPixelRatio(window.devicePixelRatio);

    container.appendChild(rendererAvatar.domElement);

    const luzPrincipal =
    new THREE.DirectionalLight(0xffffff,2);

    luzPrincipal.position.set(2,4,3);

    cenaAvatar.add(luzPrincipal);

    cenaAvatar.add(

        new THREE.AmbientLight(0xffffff,1.5)

    );

    const loader =
    new GLTFLoader();

    loader.load(

        "/assets/avatar/avatar-masculino.glb",

        function(gltf){

            avatarMesh =
            gltf.scene;

            avatarMesh.scale.set(1.25,1.25,1.25);

            avatarMesh.position.set(0,-1.1,0);

            avatarMesh.rotation.y=-0.28;

            cenaAvatar.add(avatarMesh);

        }

    );

    function animar(){

        requestAnimationFrame(animar);

        rendererAvatar.render(

            cenaAvatar,

            cameraAvatar

        );

    }

    animar();

}*/