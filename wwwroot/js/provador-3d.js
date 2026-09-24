import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';

const container = document.getElementById('avatar3DContainer');
const canvas = document.getElementById('avatar3DCanvas');
const carregando = document.getElementById('avatar3DCarregando');
const modalProvador = document.getElementById('modalProvadorVirtual');

let cena = null;
let camera = null;
let renderer = null;
let modeloHumano = null;
let inicializado = false;
let carregandoModelo = false;
window.avatar3DPronto = false;

// ======================================================
// APLICA O AVATAR MASCULINO COMO PADRÃO
// ======================================================

function aplicarAvatarMasculino() {

    if (!modeloHumano) {
        return;
    }

    modeloHumano.traverse(function (objeto) {

        if (!objeto.isMesh) {
            return;
        }

        if (!objeto.morphTargetDictionary) {
            return;
        }

        if (!objeto.morphTargetInfluences) {
            return;
        }

        const morphs = objeto.morphTargetDictionary;

        const indiceFeminino =
            morphs['bodyFeminine'];

        const indiceMasculino =
            morphs['bodyMasculine'];

        if (
            indiceFeminino !== undefined &&
            indiceMasculino !== undefined
        ) {

            objeto.morphTargetInfluences[
                indiceFeminino
            ] = 0;

            objeto.morphTargetInfluences[
                indiceMasculino
            ] = 1;
        }

    });

    console.log('Avatar masculino aplicado como padrão.');
}

// ======================================================
// INICIALIZA O THREE.JS
// ======================================================

function iniciar3D() {

    if (!container || !canvas) {

        console.error(
            'Container ou Canvas 3D não encontrado.'
        );

        return;
    }


    // ==================================================
    // DIMENSÕES
    // ==================================================

    // Quando o Passo 2 está oculto, o container pode
    // retornar largura/altura igual a zero.
    //
    // Por isso usamos valores padrão para inicializar
    // o Three.js.

    let largura =
        container.clientWidth || 800;

    let altura =
        container.clientHeight || 580;


    console.log(
        'Dimensões utilizadas no Three.js:',
        largura,
        altura
    );

    // ==================================================
    // CENA ajustarTamanho(
    // ==================================================

    cena = new THREE.Scene();

    cena.background = new THREE.Color(0xffffff);


    // ==================================================
    // ILUMINAÇÃO
    // ==================================================

    const luzAmbiente = new THREE.HemisphereLight(
        0xffffff,
        0x777777,
        2.5
    );

    cena.add(luzAmbiente);


    const luzPrincipal = new THREE.DirectionalLight(
        0xffffff,
        3
    );

    luzPrincipal.position.set(
        2,
        4,
        5
    );

    cena.add(luzPrincipal);


    const luzFrontal = new THREE.DirectionalLight(
        0xffffff,
        1.5
    );

    luzFrontal.position.set(
        -2,
        2,
        5
    );

    cena.add(luzFrontal);


    // ==================================================
    // CÂMERA
    // ==================================================

    camera = new THREE.PerspectiveCamera(
        45,
        largura / altura,
        0.1,
        100
    );

    camera.fov = 35;

    camera.updateProjectionMatrix();

    camera.position.set(
        0,
        1.0,
        4.8
    );

    camera.lookAt(
        0,
        1.0,
        0
    );


    // ==================================================
    // RENDERIZADOR
    // ==================================================

    renderer = new THREE.WebGLRenderer({
        canvas: canvas,
        antialias: true,
        alpha: false
    });

    renderer.setPixelRatio(
        window.devicePixelRatio
    );

    renderer.setSize(
        largura,
        altura,
        false
    );


    // ==================================================
    // CARREGADOR DO MODELO
    // ==================================================

    const loader = new GLTFLoader();

    if (carregando) {
        carregando.style.display = 'block';
        carregando.textContent = ' ';
    }


    loader.load(

        'https://three.ws/avatars/parametric-base.glb',


        // ==================================================
        // MODELO CARREGADO
        // ==================================================

        function (gltf) {

            console.log(
                'Modelo humano carregado!'
            );


            modeloHumano = gltf.scene;

            modeloHumano = gltf.scene;

            console.log('================================');
            console.log('MODELO 3D CARREGADO E DISPONÍVEL');
            console.log('modeloHumano:', modeloHumano);
            console.log('================================');
            window.avatar3DPronto = true;

            console.log('================================');
            console.log('AVATAR 3D PRONTO PARA ALTERAÇÕES');
            console.log('================================');

            // ==============================================
            // COR DA PELE
            // ==============================================

            const corPele = new THREE.Color(
                0xB97852
            );


            modeloHumano.traverse(
                function (objeto) {

                    if (!objeto.isMesh) {
                        return;
                    }


                    objeto.castShadow = true;
                    objeto.receiveShadow = true;


                    if (
                        Array.isArray(
                            objeto.material
                        )
                    ) {

                        objeto.material.forEach(
                            function (material) {

                                material = material.clone();

                                material.color =
                                    corPele;

                                material.roughness =
                                    0.8;

                                material.metalness =
                                    0;

                            }
                        );

                    }
                    else if (objeto.material) {

                        objeto.material =
                            objeto.material.clone();

                        objeto.material.color =
                            corPele;

                        objeto.material.roughness =
                            0.8;

                        objeto.material.metalness =
                            0;
                    }


                    // ==========================================
                    // MORPH TARGETS
                    // ==========================================

                    if (objeto.morphTargetDictionary) {

                        console.log(
                            '========================================'
                        );

                        console.log(
                            'MESH COM MORPHS:',
                            objeto.name
                        );

                        console.log(
                            'Morph targets:',
                            Object.keys(
                                objeto.morphTargetDictionary
                            )
                        );

                        console.log(
                            'Quantidade:',
                            Object.keys(
                                objeto.morphTargetDictionary
                            ).length
                        );

                        console.log(
                            '========================================'
                        );


                        const morphsCorpo = Object.keys(
                            objeto.morphTargetDictionary
                        ).filter(nome => {

                            const n = nome.toLowerCase();

                            return (
                                n.includes('chest') ||
                                n.includes('waist') ||
                                n.includes('hip') ||
                                n.includes('torso') ||
                                n.includes('body')
                            );
                        });


                        console.log(
                            'MORPHS DO CORPO:',
                            morphsCorpo
                        );


                        if (objeto.name === 'Body') {

                            const morphsChest = Object.keys(
                                objeto.morphTargetDictionary
                            ).filter(nome =>
                                nome.toLowerCase().includes('chest')
                            );


                            console.log(
                                '===== MORPHS DO TÓRAX =====',
                                morphsChest
                            );


                            const caixa =
                                new THREE.Box3().setFromObject(objeto);


                            console.log(
                                'BODY LIMITES:',
                                {
                                    minimoY: caixa.min.y,
                                    maximoY: caixa.max.y,
                                    altura: caixa.max.y - caixa.min.y
                                }
                            );
                        }
                    }

                }
            );


            // ==================================================
            // MOSTRA OS BONES DO MODELO
            // ==================================================

            modeloHumano.traverse(function (objeto) {

                if (objeto.isBone) {

                    console.log(
                        'BONE:',
                        objeto.name
                    );

                }

            });


            // ==============================================
            // TAMANHO DO MODELO
            // ==============================================

            modeloHumano.scale.set(
                3.2,
                3.2,
                3.2
            );


            // ==============================================
            // POSIÇÃO
            // ==============================================

            modeloHumano.position.set(
                0,
                -2.8,
                0
            );


            // ==================================================
            // POSE PADRÃO DOS BRAÇOS
            // ==================================================
            //
            // Nesta etapa estamos trabalhando somente
            // com o avatar masculino.
            //
            // O feminino será desenvolvido posteriormente
            // com ajustes próprios de busto, ombros e corpo.
            //
            // ==================================================

            const bracoEsquerdo =
                modeloHumano.getObjectByName(
                    'mixamorigLeftArm'
                );


            const bracoDireito =
                modeloHumano.getObjectByName(
                    'mixamorigRightArm'
                );


            if (bracoEsquerdo) {

                bracoEsquerdo.rotation.z =
                    THREE.MathUtils.degToRad(-35);

                bracoEsquerdo.rotation.x =
                    THREE.MathUtils.degToRad(20);
            }


            if (bracoDireito) {

                bracoDireito.rotation.z =
                    THREE.MathUtils.degToRad(35);

                bracoDireito.rotation.x =
                    THREE.MathUtils.degToRad(20);
            }

            // ==============================================
            // ROTAÇÃO DO AVATAR
            // ==============================================
            modeloHumano.rotation.y = THREE.MathUtils.degToRad(30);

            console.log(
                'Pose masculina dos braços aplicada:',
                !!bracoEsquerdo,
                !!bracoDireito
            );

            // ==============================================
            // APLICA O AVATAR MASCULINO COMO PADRÃO
            // ==============================================

            aplicarAvatarMasculino();
            // ==============================================
            // ADICIONA NA CENA
            // ==============================================

            cena.add(
                modeloHumano
            );
window.modeloHumanoDebug = modeloHumano;

console.log('Modelo adicionado à cena!');

console.log("========================================");
console.log("TESTE DOS MORPHS DO AVATAR");
console.log("========================================");

modeloHumano.traverse(function (objeto) {

    if (!objeto.isMesh) {
        return;
    }

    if (!objeto.morphTargetDictionary) {
        return;
    }

    console.log(
        "MESH:",
        objeto.name
    );

    console.log(
        "MORPHS:",
        Object.keys(
            objeto.morphTargetDictionary
        )
    );

});

console.log("========== TESTE DOS MORPHS ==========");

modeloHumano.traverse(function (objeto) {

    if (!objeto.isMesh) return;
    if (!objeto.morphTargetDictionary) return;

    const morphs = objeto.morphTargetDictionary;

    console.log("MESH:", objeto.name);

    console.log(
        "bodyHeavier:",
        morphs["bodyHeavier"]
    );

    console.log(
        "bodyThinner:",
        morphs["bodyThinner"]
    );

    console.log(
        "bellyBigger:",
        morphs["bellyBigger"]
    );

    console.log(
        "bellySmaller:",
        morphs["bellySmaller"]
    );
});

            // ==========================================================
// APLICA OS DADOS DO PROVADOR AO MODELO 3D
// ==========================================================

if (window.dadosProvador) {

    console.log(
        '========================================'
    );

    console.log(
        'APLICANDO DADOS DO PROVADOR AO AVATAR'
    );

    console.log(
        'Altura:',
        window.dadosProvador.altura
    );

    console.log(
        'Peso:',
        window.dadosProvador.peso
    );

    console.log(
        'Idade:',
        window.dadosProvador.idade
    );

    console.log(
        'Gênero:',
        window.dadosProvador.genero
    );


    // ======================================================
    // GÊNERO
    // ======================================================

    if (
        typeof window.atualizarSexoAvatar3D ===
        'function'
    ) {

        window.atualizarSexoAvatar3D(
            window.dadosProvador.genero
        );

    }


    // ======================================================
    // VOLUME CORPORAL
    // ======================================================

    if (
        typeof window.calcularVolumeCorporal ===
        'function' &&

        typeof window.atualizarPesoAvatar3D ===
        'function'
    ) {

        const intensidadeCorporal =
            window.calcularVolumeCorporal(

                window.dadosProvador.altura,

                window.dadosProvador.peso

            );


        console.log(
            'INTENSIDADE CORPORAL APLICADA:',
            intensidadeCorporal
        );


        window.atualizarPesoAvatar3D(
            intensidadeCorporal
        );

    }


    // ======================================================
    // ABDÔMEN
    // ======================================================

    if (
        typeof window.atualizarBarriga3D ===
        'function'
    ) {

        const peso =
            window.dadosProvador.peso;


        const intensidadeBarriga =
            Math.max(

                0,

                Math.min(

                    0.90,

                    (peso - 70) / 100

                )

            );


        console.log(
            'INTENSIDADE DA BARRIGA APLICADA:',
            intensidadeBarriga
        );


        window.atualizarBarriga3D(
            intensidadeBarriga
        );

    }

}

            // ==============================================
            // ESCONDE CARREGANDO
            // ==============================================

            if (carregando) {

                carregando.style.display = 'none';

                carregando.textContent = '';
            }


            console.log(
                'Modelo adicionado à cena!'
            );

        },


        // ==================================================
        // PROGRESSO
        // ==================================================

        function (progresso) {

            if (
                progresso.total > 0
            ) {

                const percentual =
                    (
                        progresso.loaded /
                        progresso.total
                    ) * 100;


                console.log(
                    'Carregando modelo:',
                    percentual.toFixed(0) + '%'
                );

            }

        },


        // ==================================================
        // ERRO
        // ==================================================

        function (erro) {

            console.error(
                'Erro ao carregar modelo humano:',
                erro
            );


            if (carregando) {

                carregando.style.display =
                    'none';

                carregando.textContent = '';
            }

        }

    );


    // ==================================================
    // FINALIZA
    // ==================================================

    inicializado = true;


    console.log(
        'Three.js inicializado com sucesso!'
    );


    animar();
}

window.iniciarAvatar3D = iniciar3D;

// ======================================================
// ANIMAÇÃO
// ======================================================

function animar() {

    if (!inicializado) {
        return;
    }


    requestAnimationFrame(
        animar
    );


    renderer.render(
        cena,
        camera
    );
}


// ======================================================
// QUANDO O MODAL FOR ABERTO
// ======================================================

if (modalProvador) {

    modalProvador.addEventListener(
        'shown.bs.modal',
        function () {

            console.log(
                'Provador aberto.'
            );


            if (carregando) {

                carregando.style.display =
                    'block';

            }

            if (!inicializado) {

                iniciar3D();

            }
            else {

                ajustarTamanho();

            }

        }
    );

}


// ======================================================
// REDIMENSIONAMENTO
// ======================================================

function ajustarTamanho() {

    if (
        !container ||
        !renderer ||
        !camera
    ) {

        return;
    }


    const largura =
        container.clientWidth;

    const altura =
        container.clientHeight;


    if (
        largura === 0 ||
        altura === 0
    ) {

        return;
    }


    renderer.setSize(
        largura,
        altura,
        false
    );


    camera.aspect =
        largura / altura;


    camera.updateProjectionMatrix();

}

window.ajustarTamanhoAvatar3D =
    ajustarTamanho;
// ======================================================
// CONTROLE DO TÓRAX PELO PROVADOR
// ======================================================

window.atualizarToraxAvatar3D = function (posicao) {

    if (!modeloHumano) {

        console.warn(
            'Modelo humano ainda não carregado.'
        );

        return;
    }


    posicao =
        Math.max(
            0,
            Math.min(
                4,
                posicao
            )
        );


    let meshTorax = null;


    // Procura automaticamente o mesh
    // que possui os morphs do tórax

    modeloHumano.traverse(function (objeto) {

        if (!objeto.isMesh) {
            return;
        }


        if (!objeto.morphTargetDictionary) {
            return;
        }


        const morphs =
            objeto.morphTargetDictionary;


        if (
            morphs['chestWider'] !== undefined &&
            morphs['chestNarrower'] !== undefined
        ) {

            meshTorax = objeto;
        }

    });


    if (!meshTorax) {

        console.error(
            'NÃO ENCONTREI um mesh com chestWider/chestNarrower.'
        );

        return;
    }


    console.log(
        'MESH DO TÓRAX ENCONTRADO:',
        meshTorax.name
    );


    const indiceMaior =
        meshTorax.morphTargetDictionary[
            'chestWider'
        ];


    const indiceMenor =
        meshTorax.morphTargetDictionary[
            'chestNarrower'
        ];


    // Intensidade máxima do ajuste

    const intensidadeMaxima = 0.25;


    // Posição 2 = neutro

    const ajuste =
        ((posicao - 2) / 2) *
        intensidadeMaxima;


    // Zera os dois morphs

    meshTorax.morphTargetInfluences[
        indiceMaior
    ] = 0;


    meshTorax.morphTargetInfluences[
        indiceMenor
    ] = 0;


    // Tórax maior

    if (ajuste > 0) {

        meshTorax.morphTargetInfluences[
            indiceMaior
        ] = ajuste;
    }


    // Tórax menor

    else if (ajuste < 0) {

        meshTorax.morphTargetInfluences[
            indiceMenor
        ] = Math.abs(ajuste);
    }


    console.log(
        'TÓRAX 3D:',
        'posição =',
        posicao,
        'influência =',
        ajuste
    );
};


// ======================================================
// CONTROLE DA CINTURA PELO PROVADOR
// ======================================================

window.atualizarCinturaAvatar3D = function (posicao) {

    if (!modeloHumano) {

        console.warn(
            'Modelo humano ainda não carregado.'
        );

        return;
    }


    posicao =
        Math.max(
            0,
            Math.min(
                4,
                posicao
            )
        );


    let meshCintura = null;


    modeloHumano.traverse(function (objeto) {

        if (!objeto.isMesh) {
            return;
        }


        if (!objeto.morphTargetDictionary) {
            return;
        }


        const morphs =
            objeto.morphTargetDictionary;


        if (
            morphs['waistWider'] !== undefined &&
            morphs['waistNarrower'] !== undefined
        ) {

            meshCintura = objeto;
        }

    });


    if (!meshCintura) {

        console.error(
            'NÃO ENCONTREI um mesh com waistWider/waistNarrower.'
        );

        return;
    }


    console.log(
        'MESH DA CINTURA ENCONTRADO:',
        meshCintura.name
    );


    const indiceMaior =
        meshCintura.morphTargetDictionary[
            'waistWider'
        ];


    const indiceMenor =
        meshCintura.morphTargetDictionary[
            'waistNarrower'
        ];


    // Intensidade máxima

    const intensidadeMaxima = 0.25;


    // Posição 2 = neutro

    const ajuste =
        ((posicao - 2) / 2) *
        intensidadeMaxima;


    // Zera os morphs anteriores

    meshCintura.morphTargetInfluences[
        indiceMaior
    ] = 0;


    meshCintura.morphTargetInfluences[
        indiceMenor
    ] = 0;


    // Cintura maior

    if (ajuste > 0) {

        meshCintura.morphTargetInfluences[
            indiceMaior
        ] = ajuste;
    }


    // Cintura menor

    else if (ajuste < 0) {

        meshCintura.morphTargetInfluences[
            indiceMenor
        ] = Math.abs(ajuste);
    }


    console.log(
        'CINTURA 3D:',
        'posição =',
        posicao,
        'influência =',
        ajuste
    );
};


// ======================================================
// CONTROLE DO VOLUME CORPORAL PELO PESO
// ======================================================

window.atualizarPesoAvatar3D = function (intensidade) {

    if (!modeloHumano) {

        console.warn(
            'Modelo humano ainda não carregado.'
        );

        return;
    }

    // Mantém a intensidade dentro do intervalo permitido
    intensidade = Math.max(
        -1,
        Math.min(1, intensidade)
    );

    let meshCorpo = null;

    // Procura o mesh que possui os morphs
    // bodyHeavier / bodyThinner
    modeloHumano.traverse(function (objeto) {

        if (!objeto.isMesh) {
            return;
        }

        if (!objeto.morphTargetDictionary) {
            return;
        }

        const morphs =
            objeto.morphTargetDictionary;

        if (
            morphs['bodyHeavier'] !== undefined &&
            morphs['bodyThinner'] !== undefined
        ) {
            meshCorpo = objeto;
        }

    });

    if (!meshCorpo) {

        console.error(
            'NÃO ENCONTREI um mesh com bodyHeavier/bodyThinner.'
        );

        return;
    }

    const indiceMaisCheio =
        meshCorpo.morphTargetDictionary[
            'bodyHeavier'
        ];

    const indiceMaisMagro =
        meshCorpo.morphTargetDictionary[
            'bodyThinner'
        ];

    // --------------------------------------------------
    // LIMPA OS MORPHS ANTERIORES
    // --------------------------------------------------

    meshCorpo.morphTargetInfluences[
        indiceMaisCheio
    ] = 0;

    meshCorpo.morphTargetInfluences[
        indiceMaisMagro
    ] = 0;

    // --------------------------------------------------
    // CORPO MAIS VOLUMOSO
    // --------------------------------------------------

    if (intensidade > 0) {

        // Reduzimos a influência do morph global.
        // A ideia é evitar que o peso aumente
        // excessivamente os ombros.
        const volumeGlobal =
            intensidade * 0.35;

        meshCorpo.morphTargetInfluences[
            indiceMaisCheio
        ] = volumeGlobal;

    }

    // --------------------------------------------------
    // CORPO MAIS MAGRO
    // --------------------------------------------------

    else if (intensidade < 0) {

        meshCorpo.morphTargetInfluences[
            indiceMaisMagro
        ] = Math.abs(intensidade) * 0.35;

    }

    console.log(
        'PESO AVATAR 3D:',
        'intensidade =',
        intensidade,
        'volume global =',
        intensidade * 0.35,
        'barriga =',
        intensidade * 0.90
    );
};

// ======================================================
// CALCULA O VOLUME CORPORAL A PARTIR DE ALTURA + PESO
// ======================================================

window.calcularVolumeCorporal = function (
    alturaCm,
    pesoKg
) {

    if (!alturaCm || !pesoKg) {
        return 0;
    }


    const alturaMetros =
        alturaCm / 100;


    const imc =
        pesoKg /
        (
            alturaMetros *
            alturaMetros
        );


    console.log(
        'Altura:',
        alturaCm,
        'cm'
    );


    console.log(
        'Peso:',
        pesoKg,
        'kg'
    );


    console.log(
        'IMC calculado:',
        imc.toFixed(2)
    );


    // Faixa central usada pelo avatar

    const imcNeutro = 23;


    // Sensibilidade visual

    const sensibilidade = 0.035;


    let intensidade =
        (
            imc -
            imcNeutro
        ) *
        sensibilidade;


    // Limita o resultado

    intensidade =
        Math.max(
            -0.30,
            Math.min(
                0.40,
                intensidade
            )
        );


    console.log(
        'Intensidade corporal:',
        intensidade.toFixed(3)
    );


    return intensidade;
};


// ======================================================
// CONTROLE DO VOLUME ABDOMINAL
// ======================================================

window.atualizarBarriga3D = function (
    intensidade
) {

    if (!modeloHumano) {

        console.warn(
            'Modelo humano ainda não carregado.'
        );

        return;
    }


    intensidade =
        Math.max(
            -1,
            Math.min(
                1,
                intensidade
            )
        );


    let meshBarriga = null;


    modeloHumano.traverse(function (objeto) {

        if (!objeto.isMesh) {
            return;
        }


        if (!objeto.morphTargetDictionary) {
            return;
        }


        const morphs =
            objeto.morphTargetDictionary;


        if (
            morphs['bellyBigger'] !== undefined &&
            morphs['bellySmaller'] !== undefined
        ) {

            meshBarriga = objeto;
        }

    });


    if (!meshBarriga) {

        console.error(
            'NÃO ENCONTREI bellyBigger/bellySmaller.'
        );

        return;
    }


    console.log(
        'MESH DA BARRIGA:',
        meshBarriga.name
    );


    const indiceMaior =
        meshBarriga.morphTargetDictionary[
            'bellyBigger'
        ];


    const indiceMenor =
        meshBarriga.morphTargetDictionary[
            'bellySmaller'
        ];


    // Limpa os morphs

    meshBarriga.morphTargetInfluences[
        indiceMaior
    ] = 0;


    meshBarriga.morphTargetInfluences[
        indiceMenor
    ] = 0;


    if (intensidade > 0) {

        meshBarriga.morphTargetInfluences[
            indiceMaior
        ] = intensidade;

    }
    else if (intensidade < 0) {

        meshBarriga.morphTargetInfluences[
            indiceMenor
        ] = Math.abs(intensidade);
    }


    console.log(
        'BARRIGA 3D:',
        intensidade
    );
};

// ======================================================
// CONTROLE DO QUADRIL PELO PROVADOR
// ======================================================

const indicadorQuadril =
    document.getElementById('indicadorQuadril');

const btnQuadrilMenos =
    document.getElementById('btnQuadrilMenos');

const btnQuadrilMais =
    document.getElementById('btnQuadrilMais');

const reguaQuadril =
    indicadorQuadril?.closest('.medida-regua');

const quadrilTotalPosicoes = 5;

let quadrilPosicao = 2;

const quadrilIncremento = 0.5;

let quadrilMedida =
    quadrilPosicao * quadrilIncremento;


// ======================================================
// ATUALIZA O QUADRIL
// ======================================================

function atualizarQuadril() {

    if (!indicadorQuadril) {
        return;
    }

    const percentual =
        (quadrilPosicao /
        (quadrilTotalPosicoes - 1)) * 100;

    indicadorQuadril.style.left =
        percentual + '%';


    // Desabilita o botão -
    if (btnQuadrilMenos) {

        btnQuadrilMenos.disabled =
            quadrilPosicao === 0;

    }


    // Desabilita o botão +
    if (btnQuadrilMais) {

        btnQuadrilMais.disabled =
            quadrilPosicao ===
            quadrilTotalPosicoes - 1;

    }


    // Cada posição representa 0,5 cm internamente
    quadrilMedida =
        quadrilPosicao * quadrilIncremento;


    // Atualiza o avatar 3D
    if (
        typeof window.atualizarQuadrilAvatar3D ===
        'function'
    ) {

        window.atualizarQuadrilAvatar3D(
            quadrilPosicao
        );

    }


    console.log(
        'Quadril:',
        quadrilMedida,
        'cm de ajuste interno'
    );
}


// ======================================================
// BOTÃO MENOS
// ======================================================

if (btnQuadrilMenos) {

    btnQuadrilMenos.addEventListener(
        'click',
        function () {

            if (quadrilPosicao > 0) {

                quadrilPosicao--;

                atualizarQuadril();

            }

        }
    );

}


// ======================================================
// BOTÃO MAIS
// ======================================================

if (btnQuadrilMais) {

    btnQuadrilMais.addEventListener(
        'click',
        function () {

            if (
                quadrilPosicao <
                quadrilTotalPosicoes - 1
            ) {

                quadrilPosicao++;

                atualizarQuadril();

            }

        }
    );

}


// ======================================================
// CLIQUE DIRETO NA RÉGUA
// ======================================================

if (reguaQuadril) {

    reguaQuadril.addEventListener(
        'click',
        function (evento) {

            const rect =
                reguaQuadril.getBoundingClientRect();

            const percentual =
                (evento.clientX - rect.left) /
                rect.width;

            let novaPosicao =
                Math.round(
                    percentual *
                    (quadrilTotalPosicoes - 1)
                );

            novaPosicao =
                Math.max(
                    0,
                    Math.min(
                        quadrilTotalPosicoes - 1,
                        novaPosicao
                    )
                );

            quadrilPosicao =
                novaPosicao;

            atualizarQuadril();

        }
    );

}

// ======================================================
// POSIÇÃO INICIAL
// ======================================================

atualizarQuadril();

// ======================================================
// CONTROLE 3D DO QUADRIL
// ======================================================

window.atualizarQuadrilAvatar3D = function (posicao) {

    if (!modeloHumano) {

        console.warn(
            'Modelo humano ainda não carregado.'
        );

        return;
    }


    // Garante que a posição fique entre 0 e 4

    posicao = Math.max(
        0,
        Math.min(4, posicao)
    );


    // ==================================================
    // PROCURA O MESH DO QUADRIL
    // ==================================================

    let meshQuadril = null;


    modeloHumano.traverse(function (objeto) {

        if (!objeto.isMesh) {
            return;
        }


        if (!objeto.morphTargetDictionary) {
            return;
        }


        const morphs =
            objeto.morphTargetDictionary;


        if (
            morphs['hipsWider'] !== undefined &&
            morphs['hipsNarrower'] !== undefined
        ) {

            meshQuadril = objeto;

        }

    });


    // ==================================================
    // VERIFICA SE ENCONTROU
    // ==================================================

    if (!meshQuadril) {

        console.error(
            'NÃO ENCONTREI um mesh com hipsWider/hipsNarrower.'
        );

        return;
    }


    console.log(
        'MESH DO QUADRIL ENCONTRADO:',
        meshQuadril.name
    );


    // ==================================================
    // PEGA OS ÍNDICES DOS MORPHS
    // ==================================================

    const indiceMaior =
        meshQuadril.morphTargetDictionary[
            'hipsWider'
        ];


    const indiceMenor =
        meshQuadril.morphTargetDictionary[
            'hipsNarrower'
        ];


    // ==================================================
    // INTENSIDADE DO AJUSTE
    // ==================================================

    const intensidadeMaxima = 0.25;


    // Posição 2 = neutro

    const ajuste =
        ((posicao - 2) / 2) *
        intensidadeMaxima;


    // ==================================================
    // ZERA OS MORPHS ANTERIORES
    // ==================================================

    meshQuadril.morphTargetInfluences[
        indiceMaior
    ] = 0;


    meshQuadril.morphTargetInfluences[
        indiceMenor
    ] = 0;


    // ==================================================
    // QUADRIL MAIOR
    // ==================================================

    if (ajuste > 0) {

        meshQuadril.morphTargetInfluences[
            indiceMaior
        ] = ajuste;

    }


    // ==================================================
    // QUADRIL MENOR
    // ==================================================

    else if (ajuste < 0) {

        meshQuadril.morphTargetInfluences[
            indiceMenor
        ] = Math.abs(ajuste);

    }


    console.log(
        'QUADRIL 3D:',
        'posição =',
        posicao,
        'influência =',
        ajuste
    );

};

// ======================================================
// CONTROLE DO SEXO DO AVATAR
// ======================================================
//
// Nesta etapa:
//
// - Masculino = avatar desenvolvido
// - Feminino = permanece disponível na interface,
//   mas será desenvolvido posteriormente.
//
// ======================================================

window.atualizarSexoAvatar3D = function (sexo) {

    if (!modeloHumano) {

        console.warn(
            'Modelo humano ainda não carregado.'
        );

        return;
    }


    const feminino =
        sexo &&
        sexo.toLowerCase().includes('fem');


    const masculino =
        sexo &&
        sexo.toLowerCase().includes('masc');


    if (!feminino && !masculino) {

        console.warn(
            'Sexo do avatar não reconhecido:',
            sexo
        );

        return;
    }


    let quantidadeMeshes = 0;


    modeloHumano.traverse(function (objeto) {

        if (!objeto.isMesh) {
            return;
        }


        if (!objeto.morphTargetDictionary) {
            return;
        }


        if (!objeto.morphTargetInfluences) {
            return;
        }


        const morphs =
            objeto.morphTargetDictionary;


        const indiceFeminino =
            morphs['bodyFeminine'];


        const indiceMasculino =
            morphs['bodyMasculine'];


        // Mesh participa do morph corporal

        if (
            indiceFeminino !== undefined &&
            indiceMasculino !== undefined
        ) {

            // Primeiro zera os dois

            objeto.morphTargetInfluences[
                indiceFeminino
            ] = 0;


            objeto.morphTargetInfluences[
                indiceMasculino
            ] = 0;


            // Nesta etapa somente o masculino
            // será utilizado como avatar desenvolvido.

            if (masculino) {

                objeto.morphTargetInfluences[
                    indiceMasculino
                ] = 1;
            }


            quantidadeMeshes++;


            console.log(
                'Sexo aplicado no mesh:',
                objeto.name
            );
        }

    });


    console.log(
        '--------------------------------'
    );


    console.log(
        'Avatar:',
        masculino
            ? 'MASCULINO'
            : 'FEMININO - aguardando implementação'
    );


    console.log(
        'Meshes atualizados:',
        quantidadeMeshes
    );


    console.log(
        '--------------------------------'
    );
};


// ======================================================
// REDIMENSIONAMENTO DA JANELA
// ======================================================

window.addEventListener(
    'resize',
    ajustarTamanho
);