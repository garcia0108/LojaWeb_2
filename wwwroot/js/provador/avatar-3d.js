import * as THREE from "three";
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js";

let renderer;
let scene;
let camera;
let avatar;
let coresLigadas = false;

const ossos = {};

//Guardar as malhas da pele
const malhasPele = [];

//Criar a paleta
const tonsPele = {
    1: "#F4E8D5",
    2: "#E7C8A0",
    3: "#D6A070",
    4: "#B97A56",
    5: "#8A5A3C",
    6: "#5E3B2A",
    7: "#2B1A14"
};

function alterarTomPele(indice) {

    const cor = tonsPele[indice];

    avatar.traverse((obj) => {

        if (!obj.isMesh) return;

        obj.material = obj.material.clone();
        obj.material.color.set(cor);

    });

}

export function inicializarAvatar() {

    const container = document.getElementById("avatar3DContainer");

    if (!container) return;

    // Evita criar dois renderizadores
    if (renderer) return;

    scene = new THREE.Scene();

    camera = new THREE.PerspectiveCamera(
        35,
        container.clientWidth / container.clientHeight,
        0.1,
        100
    );

    camera.position.set(0, 1.15, 3);

    renderer = new THREE.WebGLRenderer({
        antialias: true,
        alpha: true
    });

    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(container.clientWidth, container.clientHeight);

    container.appendChild(renderer.domElement);

    // Luz ambiente
    scene.add(new THREE.AmbientLight(0xffffff, 1.4));

    // Luz principal
    const luz = new THREE.DirectionalLight(0xffffff, 2);
    luz.position.set(2, 4, 3);
    scene.add(luz);

    const loader = new GLTFLoader();

    loader.load(
        "/assets/avatar/avatar-masculino.glb",

        function (gltf) {

            avatar = gltf.scene;


             // Câmera um pouco mais afastada
            camera.position.set(0, 1.1, 3.8);

            avatar.scale.set(1.55,1.55,1.55);

            // sobe bastante
            avatar.position.set(0,-0.25,0);

            // leve inclinação igual à referência
            avatar.rotation.y=0.30;

            scene.add(avatar);

            // ==========================================
            // GUARDAR OSSOS
            // ==========================================
            avatar.traverse((obj) => {
                if (obj.isBone) {
                    ossos[obj.name] = obj;
                }
            });

            avatar.traverse((obj) => {

                if (!obj.isMesh) return;

                console.log({
                    mesh: obj.name,
                    material: obj.material?.name
                });

            });

            // ==========================================
            // APLICAR CONFIGURAÇÕES DO AVATAR
            // =========================================
            aplicarPoseProvador(avatar);
            ligarCoresPele();
            alterarTomPele(3);

            // ==========================================
            // APLICAR DADOS DO PROVADOR AO AVATAR
            // ==========================================
            if (window.dadosProvador) {

    console.log("DADOS RECEBIDOS PELO AVATAR:", {
        altura: window.dadosProvador.altura,
        peso: window.dadosProvador.peso,
        imc: window.dadosProvador.imc
    });

    window.aplicarPesoAvatar?.(
        window.dadosProvador.altura,
        window.dadosProvador.peso,
        window.dadosProvador.imc
    );

}

            console.log("Avatar carregado!");

            // Guarda para usarmos depois nos morphs
            window.avatarMesh = avatar;

        },

        undefined,

        function (erro) {

            console.error("Erro carregando avatar:", erro);

        }
    );

    window.addEventListener("resize", () => {

        camera.aspect =
            container.clientWidth / container.clientHeight;

        camera.updateProjectionMatrix();

        renderer.setSize(
            container.clientWidth,
            container.clientHeight
        );

    });

    function animar() {

        requestAnimationFrame(animar);

        renderer.render(scene, camera);

    }

    animar();

}

// ==========================================
// T-POSE → A-POSE
// ==========================================
function aplicarPoseProvador(avatar) {

    avatar.traverse((obj) => {

        if (!obj.isBone) return;

        switch (obj.name) {

            case "LeftArm":
                obj.rotation.x -= 0.95;
                break;

            case "RightArm":
                obj.rotation.x -= 0.95;
                break;

        }

    });

}

// ==========================================
// APLICAR PESO CORPORAL AO AVATAR
// ==========================================
function aplicarPesoAvatar(altura, peso, imc) {

    console.log("APLICANDO PESO NO AVATAR:", {
        altura,
        peso,
        imc
    });

    if (!avatar) {
        console.error("Avatar ainda não existe!");
        return;
    }


    if (!avatar) return;

    // ------------------------------------------
    // Normalização do IMC
    // ------------------------------------------
    // IMC abaixo de 18.5  -> corpo mais fino
    // 18.5 - 24.9         -> normal
    // 25 - 29.9           -> aumento moderado
    // 30+                 -> aumento maior

    let fatorPeso = THREE.MathUtils.clamp(
        (imc - 18) / 17,
        0,
        1
    );

    avatar.traverse((obj) => {

        if (!obj.isMesh) return;

        if (!obj.morphTargetDictionary) return;

        if (!obj.morphTargetInfluences) return;

        const morphs = obj.morphTargetDictionary;
        const influences = obj.morphTargetInfluences;

        // ------------------------------------------
        // FUNÇÃO AUXILIAR
        // ------------------------------------------
        function aplicarMorph(nome, valor) {

            const indice = morphs[nome];

            if (indice === undefined) return;

            influences[indice] = THREE.MathUtils.clamp(
                valor,
                0,
                1
            );
        }

        // ------------------------------------------
        // LIMPAR MORPHS RELACIONADOS AO PESO
        // ------------------------------------------

        aplicarMorph("bellyBigger", 0);
        aplicarMorph("bellySmaller", 0);
        aplicarMorph("bellySoft", 0);

        aplicarMorph("waistWider", 0);
        aplicarMorph("waistNarrower", 0);

        aplicarMorph("torsoLatsWider", 0);
        aplicarMorph("torsoLatsNarrower", 0);

        aplicarMorph("torsoUnderbustWider", 0);
        aplicarMorph("torsoUnderbustNarrower", 0);

        // ------------------------------------------
        // BARRIGA
        // ------------------------------------------

        const barriga = fatorPeso * 1.00;

        aplicarMorph(
            "bellyBigger",
            barriga
        );

        // ------------------------------------------
        // MACIEZ DA BARRIGA
        // ------------------------------------------

        const maciez = fatorPeso * 0.80;

        aplicarMorph(
            "bellySoft",
            maciez
        );

        // ------------------------------------------
        // CINTURA
        // ------------------------------------------

        const cintura = fatorPeso * 0.70;

        aplicarMorph(
            "waistWider",
            cintura
        );

        // ------------------------------------------
        // LATERAIS DO TRONCO
        // ------------------------------------------

        const laterais = fatorPeso * 0.55;

        aplicarMorph(
            "torsoLatsWider",
            laterais
        );

        // ------------------------------------------
        // PARTE INFERIOR DO TÓRAX
        // ------------------------------------------

        const toraxInferior = fatorPeso * 0.45;

        aplicarMorph(
            "torsoUnderbustWider",
            toraxInferior
        );

    });

    console.log(
        "Peso aplicado ao avatar:",
        {
            altura,
            peso,
            imc: imc.toFixed(2),
            fatorPeso: fatorPeso.toFixed(2)
        }
    );
}

window.aplicarPesoAvatar = aplicarPesoAvatar;

// ==========================================
// Ligar às bolinhas
// ==========================================
function ligarCoresPele() {

    document
        .querySelectorAll(".cor-avatar")
        .forEach(botao => {

            botao.addEventListener("click", () => {

                document
                    .querySelectorAll(".cor-avatar")
                    .forEach(b => b.classList.remove("ativo"));

                botao.classList.add("ativo");

                alterarTomPele(botao.dataset.cor);

            });

        });

}

window.inicializarAvatar = inicializarAvatar;