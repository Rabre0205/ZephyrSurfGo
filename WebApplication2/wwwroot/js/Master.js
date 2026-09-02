
/* ════════════════════════════════════
   NAVBAR
════════════════════════════════════ */
const navbar = document.getElementById('navbar');
window.addEventListener('scroll', () => navbar.classList.toggle('scrolled', window.scrollY > 60));

/* ════════════════════════════════════
   REVEAL
════════════════════════════════════ */
const obs = new IntersectionObserver((entries) => {
    entries.forEach((e, i) => {
        if (e.isIntersecting) {
            setTimeout(() => e.target.classList.add('visible'), i * 80);
            obs.unobserve(e.target);
        }
    });
}, { threshold: 0.08 });
document.querySelectorAll('.reveal').forEach(el => obs.observe(el));

/* ════════════════════════════════════
   CART
════════════════════════════════════ */
function getCart() { try { return JSON.parse(localStorage.getItem('master_cart') || '[]'); } catch { return []; } }
function saveCart(c) {
    localStorage.setItem('master_cart', JSON.stringify(c));
    updateBadge();
    renderOrderSummary();
}
function updateBadge() {
    try {
        const cart = getCart();
        const total = cart.reduce((s, i) => s + i.qty, 0);
        const badge = document.getElementById('cartBadge');
        if (badge) { badge.textContent = total; badge.style.display = total > 0 ? 'flex' : 'none'; }
    } catch { }
}
updateBadge();

const DEMO_BOARDS = {
    gaucho: { name: 'Shortboard personalizada', cat: 'Forma · Performance' },
    pampeano: { name: 'Tabla híbrida personalizada', cat: 'Forma · Versátil' },
    charrua: { name: 'Longboard personalizado', cat: 'Forma · Estable' },
    playero: { name: 'Fish personalizada', cat: 'Forma · Rápida y fluida' },
};

const dynamicBoards = Array.isArray(window.shaperPageData?.boards)
    ? window.shaperPageData.boards
    : [];

const BOARDS = DEMO_BOARDS;

async function addToCart(id, name, price) {
    const realProduct = /^producto-(\d+)$/.exec(String(id));
    const cartAddUrl = window.shaperPageData?.cartAddUrl;

    if (realProduct && cartAddUrl) {
        const token = document.querySelector('#realCartToken input[name="__RequestVerificationToken"]')?.value;
        const body = new URLSearchParams({
            productoId: realProduct[1],
            cantidad: '1',
            __RequestVerificationToken: token || ''
        });

        try {
            const response = await fetch(cartAddUrl, {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body
            });

            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const result = await response.json();
            document.querySelectorAll('.cart-badge').forEach(badge => {
                badge.textContent = result.cantidadCarrito;
                badge.style.display = '';
            });
            showToast(result.mensaje || `"${name}" agregado al carrito`);
            return;
        } catch (error) {
            console.error('No se pudo agregar el producto al carrito real.', error);
            showToast('No se pudo agregar el producto. Intentá nuevamente.');
            return;
        }
    }

    const cart = getCart();
    const ex = cart.find(i => i.id === id);
    if (ex) ex.qty += 1;
    else cart.push({ id, name, price, qty: 1, custom: false });
    saveCart(cart);
    showToast(`"${name}" agregada al carrito`);
}


/* ════════════════════════════════════
   VISTA REALISTA POR CAPAS
════════════════════════════════════ */

const BOARD_IMAGE_PATH = '/img/boards/';

const FIN_IMAGES = {
    thruster: 'thruster.png',
    twin: 'twin.png',
    quad: 'quad.png',
    'five-fin': 'five-fin.png',

    // Todavía no tenés imágenes específicas para estas configuraciones
    'two-plus-one': 'thruster.png',
    single: 'thruster.png'
};





function getSelectedValue(id, fallback = '') {
    const element = document.getElementById(id);
    return element ? element.value : fallback;
}

function getSelectedOptionText(id) {
    const select = document.getElementById(id);

    if (!select || select.selectedIndex < 0) {
        return '';
    }

    return select.options[select.selectedIndex].text;
}

function getActiveCustomDetails() {
    return Array.from(
        document.querySelectorAll(
            '#customizador [data-detail].active'
        )
    ).map(button => button.dataset.detail);
}



function updateBoardImages() {
    const deckPaint =
        document.getElementById('deckPaint');

    const bottomPaint =
        document.getElementById('bottomPaint');

    const deckBase = document.getElementById('deckBase');
    const bottomBase = document.getElementById('bottomBase');
    if (deckBase) deckBase.src = '/img/boards/deck-mask.png';
    if (bottomBase) bottomBase.src = '/img/boards/bottom-base.png';

    const design = getSelectedValue('customDesign', 'sin-pintura');
    const designOption = document.getElementById('customDesign')?.selectedOptions?.[0];
    const designZone = designOption?.dataset?.zone || 'ambos';
    const allowsCustomColors = designOption?.dataset?.customColors !== 'false';
    const designImage = designOption?.dataset?.image || '';
    const designPrimary = designOption?.dataset?.primary || '';
    const designSecondary = designOption?.dataset?.secondary || '';
    const colorInputs = [document.getElementById('customColor'), document.getElementById('customSecondaryColor')];
    if (!allowsCustomColors) {
        if (colorInputs[0] && designPrimary) colorInputs[0].value = designPrimary;
        if (colorInputs[1] && designSecondary) colorInputs[1].value = designSecondary;
    }
    colorInputs.forEach(input => { if (input) input.disabled = !allowsCustomColors; });
    const primary = getSelectedValue('customColor', '#c9a84c');
    const secondary = getSelectedValue('customSecondaryColor', '#121212');
    const backgrounds = {
        'sin-pintura': 'transparent',
        'sol-pampeano': `linear-gradient(180deg, ${primary} 0 48%, ${secondary} 48% 54%, ${primary} 54%)`,
        'lineas-clasicas': `repeating-linear-gradient(90deg, ${primary} 0 14px, ${secondary} 14px 19px)`,
        'mitad-y-mitad': `linear-gradient(90deg, ${primary} 0 50%, ${secondary} 50%)`,
        degrade: `linear-gradient(180deg, ${primary}, ${secondary})`,
        'rails-color': `linear-gradient(90deg, ${secondary} 0 12%, ${primary} 18% 82%, ${secondary} 88%)`,
        'nose-color': `linear-gradient(180deg, ${secondary} 0 30%, ${primary} 45%)`,
        'tail-color': `linear-gradient(180deg, ${primary} 0 65%, ${secondary} 82%)`,
        pinline: `linear-gradient(90deg, ${primary} 0 47%, ${secondary} 47% 53%, ${primary} 53%)`,
        abstracto: `linear-gradient(135deg, ${primary} 0 35%, ${secondary} 35% 48%, ${primary} 48% 68%, ${secondary} 68%)`,
        retro: `repeating-linear-gradient(135deg, ${primary} 0 24px, ${secondary} 24px 36px)`,
        minimalista: `linear-gradient(180deg, white 0 72%, ${primary} 72% 78%, white 78%)`,
        'deck-completo': primary,
        'bottom-completo': secondary,
        'doble-color': `linear-gradient(90deg, ${primary} 0 50%, ${secondary} 50%)`
    };
    const background = backgrounds[design] || primary;
    const onlyBottom = design === 'bottom-completo' || designZone === 'bottom';
    const onlyDeck = design === 'deck-completo' || designZone === 'deck';
    if (deckPaint) deckPaint.style.background = onlyBottom ? 'transparent' : background;
    if (bottomPaint) bottomPaint.style.background = onlyDeck ? 'transparent' : background;

    const preview = document.getElementById('customDesignPreview');
    const previewImage = document.getElementById('customDesignPreviewImage');
    if (preview && previewImage) {
        preview.hidden = !designImage;
        if (designImage) previewImage.src = designImage;
    }

    filterCompatibleFins(undefined);

    updateBoardAccessories();
}

function filterCompatibleFins(finSystem) {
    document.querySelectorAll('#accessoryFins .accessory-product').forEach(card => {
        const compatible = !finSystem || card.dataset.finSystem === finSystem;
        card.hidden = !compatible;
        if (!compatible && card.classList.contains('selected')) {
            card.classList.remove('selected');
            selectedAccessories.delete(card.dataset.accessoryId);
        }
    });
}

function showCatalogBoardSide(button, side) {
    const card = button.closest('.board-card');
    const image = card?.querySelector('[data-board-gallery] img');
    if (!image) return;
    const nextImage = side === 'back' ? image.dataset.back : image.dataset.front;
    if (!nextImage) return;
    image.src = nextImage;
    image.alt = `${side === 'back' ? 'Dorso' : 'Frente'} de la tabla`;
}

const CUSTOM_MODEL_PRICES = {
    gaucho: 780,
    pampeano: 820,
    charrua: 950,
    playero: 760,
    'grom-plus': 690,
    'grom-two': 700,
    'mini-bird': 710,
    'spawn-mini': 720,
    bv2: 790,
    'fire-chief': 810,
    hkii: 830,
    'miami-spice': 820,
    'pina-colada': 830,
    popper: 800,
    'pretty-sweet': 840,
    'black-vulture': 850,
    churro: 840,
    'churro-2': 860,
    'hot-knife': 870,
    middy: 900,
    'rare-bird': 850,
    'rare-bird-evo': 880,
    'rarest-bird': 890,
    'volume-2': 870,
    shortie: 890,
    a2: 920,
    fad3r: 930,
    faded: 920,
    'faded-2': 950,
    'faded-gun': 990,
    'faded-step-up': 970,
    'peppa-twin': 880,
    sugar: 870,
    'full-strength': 990,
    'mid-strength': 960,
    'twin-strength': 980,
    ...Object.fromEntries(dynamicBoards.map(board => [board.key, board.price]))
};

const CUSTOM_EXTRA_PRICES = {
    construction: {
        'pu-stringer': 0,
        'eps-stringer': 60,
        'eps-future-flex': 130,
        'eps-core-reactor': 160,
        'eps-twin-tech': 150
    },

    glassing: {
        ultralight: 50,
        'regular-innegra': 80,
        heavy: 90,
        'extra-heavy': 120
    },

    carbon: {
        none: 0,
        'progressive-white': 45,
        'progressive-black': 45,
        'carbon-stripes': 60,
        'heel-toe': 55
    },

    design: {
        'sol-pampeano': 70,
        'lineas-clasicas': 50,
        'mitad-y-mitad': 65,
        degrade: 80,
        'rails-color': 55,
        'nose-color': 40,
        'tail-color': 40,
        pinline: 45,
        abstracto: 100,
        retro: 75,
        minimalista: 30,
        'deck-completo': 90,
        'bottom-completo': 90,
        'doble-color': 75,
        'sin-pintura': 0
    }
};

function calculateCustomPrice() {
    const model =
        getSelectedValue('customModel', 'gaucho');

    const construction =
        getSelectedValue(
            'customConstruction',
            'pu-stringer'
        );

    const glassing =
        getSelectedValue(
            'customGlassing',
            'regular-innegra'
        );

    const carbon =
        getSelectedValue(
            'customCarbonPatch',
            'none'
        );

    const design =
        getSelectedValue(
            'customDesign',
            'sin-pintura'
        );

    let total =
        CUSTOM_MODEL_PRICES[model] || 780;

    total +=
        CUSTOM_EXTRA_PRICES.construction[construction] || 0;

    total +=
        CUSTOM_EXTRA_PRICES.glassing[glassing] || 0;

    total +=
        CUSTOM_EXTRA_PRICES.carbon[carbon] || 0;

    total +=
        CUSTOM_EXTRA_PRICES.design[design] || 0;

    const details = getActiveCustomDetails();

    if (details.includes('grip')) {
        total += 45;
    }

    if (details.includes('serial')) {
        total += 15;
    }

    if (details.includes('fcs-box')) {
        total += 50;
    }

    return total;
}

function updateCustomPreview() {

    const setText = function (id, text) {
        const element = document.getElementById(id);

        if (element) {
            element.textContent = text;
        }
    };

    const length =
        getSelectedValue('customLength');

    const width =
        getSelectedValue('customWidth');

    const thickness =
        getSelectedValue('customThickness');

    const volume =
        getSelectedValue('customVolume');

    setText(
        'sidePreviewModel',
        getSelectedOptionText('customModel')
    );

    setText(
        'sidePreviewConstruction',
        getSelectedOptionText('customConstruction')
    );

    setText(
        'sidePreviewSize',
        `${length} × ${width} × ${thickness}`
    );

    setText(
        'sidePreviewVolume',
        volume
    );

    setText(
        'sidePreviewTail',
        getSelectedOptionText('customTail')
    );

    setText(
        'sidePreviewFins',
        getSelectedOptionText('customFinSystem') +
        ' · ' +
        getSelectedOptionText('customFinConfiguration')
    );

    setText(
        'sidePreviewGlassing',
        getSelectedOptionText('customGlassing')
    );

    setText(
        'sidePreviewCarbon',
        getSelectedOptionText('customCarbonPatch')
    );

    setText(
        'sidePreviewDesign',
        getSelectedOptionText('customDesign')
    );

    const price = calculateCustomPrice();

    setText('sidePreviewPrice', 'A confirmar por el shaper');

    const previewPrice =
        document.getElementById('previewPrice');

    if (previewPrice) {
        previewPrice.textContent = 'A confirmar por el shaper';
    }

    updateBoardImages();
    updateAccessoryPreview();
    updateCustomSummary();
}



function toggleCustomDetail(button) {
    button.classList.toggle('active');
    updateCustomPreview();
}

function updateBoardAccessories() {
    const details = getActiveCustomDetails();

    const grip =
        document.getElementById('deckGrip');

    const deckCarbon =
        document.getElementById('deckCarbon');

    const bottomCarbon =
        document.getElementById('bottomCarbon');

    const logo =
        document.getElementById('deckLogo');

    const finImage =
        document.getElementById('bottomFins');

    const stringerName =
        document.getElementById('deckStringerName');

    /* Grip */
    if (grip) {
        const hasGrip =
            details.includes('grip');

        grip.hidden = !hasGrip;

        /*
           Podés agregar un select customGripColor.
           Si no existe, usa negro.
        */
        const gripColor =
            getSelectedValue(
                'customGripColor',
                'black'
            );

        grip.src =
            BOARD_IMAGE_PATH +
            (
                gripColor === 'white'
                    ? 'grip-white.png'
                    : 'grip-black.png'
            );
    }

    /* Carbon patch */
    const carbonValue =
        getSelectedValue(
            'customCarbonPatch',
            'none'
        );

    const showCarbon =
        carbonValue !== 'none';

    if (deckCarbon) {
        deckCarbon.hidden = !showCarbon;
    }

    if (bottomCarbon) {
        bottomCarbon.hidden = !showCarbon;
    }

    /* Logo */
    if (logo) {
        const showLogo =
            details.includes('logo');

        logo.hidden = !showLogo;
    }

    /* Quillas */
    if (finImage) {

        let finSetup =
            getSelectedValue(
                'customFinConfiguration',
                'thruster'
            );

        const selectedFin =
            Array.from(selectedAccessories.values())
                .find(item => item.type === 'fin' || item.id.startsWith('fins-'));

        if (selectedFin) {

            if (selectedFin.image) {
                finImage.src = selectedFin.image;
            } else {

            switch (selectedFin.id) {

                case 'fins-reactor':
                    finSetup = 'thruster';
                    break;

                case 'fins-twin':
                    finSetup = 'twin';
                    break;

                case 'fins-quad':
                    finSetup = 'quad';
                    break;

                case 'fins-five':
                    finSetup = 'five-fin';
                    break;
            }

            }

        }

        if (!selectedFin || !selectedFin.image) {
            finImage.src =
                BOARD_IMAGE_PATH +
                FIN_IMAGES[finSetup];
        }

    }

    /* Nombre en el stringer */
    if (stringerName) {
        stringerName.textContent =
            document
                .getElementById(
                    'customStringerName'
                )
                ?.value
                .trim() || '';
    }
}

async function addCustomToCart() {
    const modelSelect = document.getElementById('customModel');
    const submitMessage = document.getElementById('customSubmitMessage');
    const submitButton = document.getElementById('customSubmitButton');

    const setSubmitMessage = function (message, isError = false) {
        if (!submitMessage) return;
        submitMessage.textContent = message;
        submitMessage.classList.toggle('error', isError);
        submitMessage.classList.toggle('success', !isError && Boolean(message));
    };

    if (!modelSelect) {
        showToast('No se encontró el personalizador');
        return;
    }

    const requestUrl = window.shaperPageData?.customRequestUrl;
    if (!requestUrl) {
        showToast('No se pudo iniciar el pedido personalizado');
        return;
    }

    const selectedText = function (id) {
        const select = document.getElementById(id);

        if (!select || select.selectedIndex < 0) {
            return '';
        }

        return select.options[select.selectedIndex].text;
    };

    const modelName =
        modelSelect.options[modelSelect.selectedIndex].text;

    const length =
        document.getElementById('customLength')?.value || '';

    const width =
        document.getElementById('customWidth')?.value || '';

    const thickness =
        document.getElementById('customThickness')?.value || '';

    const volume =
        document.getElementById('customVolume')?.value || '';

    const boardPrice = calculateCustomPrice();

    let accessoriesPrice = 0;

    selectedAccessories.forEach(function (accessory) {
        accessoriesPrice += Number(accessory.price) || 0;
    });

    const price = 0;
    const activeDetails = Array.from(
        document.querySelectorAll(
            '#customizador [data-detail].active'
        )
    ).map(function (button) {
        return button.textContent.trim();
    });

    const primaryColor =
        getSelectedValue('customColor', '#ffffff');

    const secondaryColor =
        getSelectedValue('customSecondaryColor', '#ffffff');

    const designNotes = document.getElementById('customDesignNotes')?.value.trim() || '';
    const questionnaireProfile = buildQuestionnaireProfile();
    const userNotes = document.getElementById('shaperNotes')?.value.trim() || '';
    const combinedNotes = [
        questionnaireProfile ? `Datos originales del cuestionario:\n${questionnaireProfile}` : '',
        designNotes ? `Idea de diseño:\n${designNotes}` : '',
        userNotes ? `Información adicional:\n${userNotes}` : ''
    ].filter(Boolean).join('\n\n');

    const token = document.querySelector('#realCartToken input[name="__RequestVerificationToken"]')?.value || '';
    const body = new URLSearchParams({
        ShaperId: String(window.shaperPageData?.shaperId || ''),
        ProductoBaseId: '', Modelo: modelName, PrecioEstimado: String(price),
        Largo: length, Ancho: width, Grosor: thickness, Volumen: volume,
        Construccion: selectedText('customConstruction'), Tail: selectedText('customTail'),
        SistemaQuillas: selectedText('customFinSystem'),
        ConfiguracionQuillas: selectedText('customFinConfiguration'),
        Laminado: selectedText('customGlassing'), ParcheCarbono: selectedText('customCarbonPatch'),
        Diseno: selectedText('customDesign'), ColorPrimario: primaryColor,
        ColorSecundario: secondaryColor, DetallesAdicionales: activeDetails.join(', '),
        AccesoriosJson: JSON.stringify(Array.from(selectedAccessories.values())),
        Notas: combinedNotes,
        __RequestVerificationToken: token
    });

    try {
        setSubmitMessage('Enviando pedido personalizado…');
        if (submitButton) submitButton.disabled = true;
        const response = await fetch(requestUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
            body
        });
        if (response.redirected) {
            window.location.href = response.url;
            return;
        }
        if (!response.ok) {
            throw new Error(response.status === 400
                ? 'La sesión o el formulario vencieron. Recargá la página e intentá nuevamente.'
                : 'El servidor no pudo guardar el pedido personalizado.');
        }
        const result = await response.json();
        showToast(result.mensaje || (result.creado ? 'Pedido personalizado enviado' : 'No se pudo enviar'));
        if (result.creado && window.shaperPageData?.customRequestsUrl) {
            setSubmitMessage('Pedido personalizado enviado. Abriendo Mis pedidos…');
            setTimeout(() => window.location.href = window.shaperPageData.customRequestsUrl, 900);
        } else {
            setSubmitMessage(result.mensaje || 'No se pudo enviar el pedido personalizado.', true);
        }
    } catch (error) {
        const message = error?.message || 'No se pudo enviar el pedido personalizado. Intentá nuevamente.';
        setSubmitMessage(message, true);
        showToast(message);
    } finally {
        if (submitButton) submitButton.disabled = false;
    }
}

function renderOrderSummary() {
    // orderItems / subtotalVal / totalVal live in carrito.html; skip gracefully if absent
    const container = document.getElementById('orderItems');
    const subtotalEl = document.getElementById('subtotalVal');
    const totalEl = document.getElementById('totalVal');
    if (!container) return;
    const cart = getCart();
    if (!cart.length) {
        container.innerHTML = '<p style="font-size:.82rem;color:rgba(255,255,255,.3);text-align:center;padding:1.5rem 0;">Tu carrito está vacío.<br><span style="font-size:.75rem;">Elegí una tabla arriba.</span></p>';
        if (subtotalEl) subtotalEl.textContent = 'USD 0';
        if (totalEl) totalEl.textContent = 'USD 0';
        return;
    }
    let subtotal = 0;
    container.innerHTML = cart.map(item => {
        subtotal += item.price * item.qty;
        return `
        <div class="order-item">
          <div class="order-item-img">
            <svg width="24" height="42" viewBox="0 0 24 42" fill="none"><path d="M12 1C17 8,20 18,20 26C20 33,17 39,12 42C7 39,4 33,4 26C4 18,7 8,12 1Z" fill="rgba(201,168,76,.5)"/></svg>
          </div>
          <div class="order-item-info">
            <p class="order-item-name">${item.name} × ${item.qty}</p>
            <p class="order-item-detail">${item.details || (item.custom ? 'Tabla customizada' : 'Tabla estándar')}</p>
          </div>
          <span class="order-item-price">USD ${item.price * item.qty}</span>
        </div>
      `;
    }).join('');
    if (subtotalEl) subtotalEl.textContent = `USD ${subtotal}`;
    if (totalEl) totalEl.textContent = `USD ${subtotal}`;
}
renderOrderSummary();

/* ════════════════════════════════════
   RECOMENDADOR — PREGUNTAS
════════════════════════════════════ */
const QUESTIONS = [
    {
        num: 'Pregunta 1 de 8',
        text: '¿Cuál es tu altura?',
        hint: 'Ingresala en centímetros. La altura ayuda a estimar el largo y la distribución de volumen adecuados.',
        type: 'slider', key: 'height', min: 140, max: 210, step: 1, default: 175, unit: 'cm'
    },
    {
        num: 'Pregunta 2 de 8',
        text: '¿Cuánto pesás?',
        hint: 'Ingresalo en kilogramos. Se utiliza junto con tu nivel para estimar el volumen necesario.',
        type: 'slider', key: 'weight', min: 40, max: 140, step: 1, default: 72, unit: 'kg'
    },
    {
        num: 'Pregunta 3 de 8',
        text: '¿Cuál es tu nivel de surf?',
        hint: 'Elegí el nivel que mejor represente lo que podés hacer hoy, no el que esperás alcanzar.',
        type: 'options',
        key: 'experience',
        options: [
            { label: 'Principiante', desc: 'Estoy aprendiendo a remar, pararme y correr la pared de la ola.', value: 'beginner' },
            { label: 'Intermedio', desc: 'Agarro olas sin ayuda y realizo maniobras básicas.', value: 'intermediate' },
            { label: 'Avanzado', desc: 'Tengo control, lectura de ola y maniobras consistentes.', value: 'advanced' },
            { label: 'Profesional / competitivo', desc: 'Busco precisión y rendimiento para surf de alta exigencia.', value: 'expert' },
        ]
    },
    {
        num: 'Pregunta 4 de 8',
        text: '¿Cuánto tiempo llevás surfeando y con qué frecuencia lo hacés?',
        hint: 'Esto permite diferenciar el nivel declarado de la experiencia y práctica reales.',
        type: 'compound-options',
        fields: [
            { key: 'surfTime', label: 'Tiempo surfeando', options: [
                { value: 'less_one', label: 'Menos de 1 año' }, { value: 'one_three', label: '1 a 3 años' },
                { value: 'three_seven', label: '3 a 7 años' }, { value: 'more_seven', label: 'Más de 7 años' }
            ]},
            { key: 'frequency', label: 'Frecuencia habitual', options: [
                { value: 'occasional', label: 'Algunas veces al mes' }, { value: 'weekly', label: '1 vez por semana' },
                { value: 'regular', label: '2 a 3 veces por semana' }, { value: 'high', label: '4 o más veces por semana' }
            ]}
        ]
    },
    {
        num: 'Pregunta 5 de 8',
        text: '¿Qué tipo de olas surfeás generalmente?',
        hint: 'El tipo de ola determina el rocker, el volumen y la forma del tail que necesitás.',
        type: 'options',
        key: 'waves',
        options: [
            { icon: '〰️', label: 'Olas chicas y planas', desc: 'Río de la Plata, mushy beachbreak, 1-3 pies.', value: 'small' },
            { icon: '🌊', label: 'Olas medianas', desc: 'Beachbreak sólido, puntillas moderadas, 3-6 pies.', value: 'medium' },
            { icon: '💪', label: 'Olas con power', desc: 'Reef, puntos con forma, olas huecas.', value: 'powerful' },
            { icon: '🌀', label: 'De todo un poco', desc: 'Surfeo lo que hay según el día.', value: 'varies' },
        ]
    },
    {
        num: 'Pregunta 6 de 8',
        text: '¿Qué buscás principalmente en tu próxima tabla?',
        hint: 'La geometría de una tabla implica compromisos. Elegí el objetivo más importante para vos.',
        type: 'options',
        key: 'goal',
        options: [
            { label: 'Estabilidad y facilidad', desc: 'Más confianza, remada y facilidad para agarrar olas.', value: 'stability' },
            { label: 'Velocidad y fluidez', desc: 'Generar velocidad y conectar las secciones con continuidad.', value: 'speed' },
            { label: 'Maniobrabilidad', desc: 'Una tabla ágil que responda y permita girar con facilidad.', value: 'maneuver' },
            { label: 'Performance', desc: 'Mayor respuesta para maniobras exigentes y surf vertical.', value: 'performance' },
            { label: 'Versatilidad', desc: 'Un equilibrio que funcione en diferentes condiciones.', value: 'versatility' },
        ]
    },
    {
        num: 'Pregunta 7 de 8',
        text: '¿Qué tabla usás actualmente y qué te gusta o cambiarías de ella?',
        hint: 'Si conocés las medidas, incluilas. Contanos si te falta estabilidad, velocidad, remada o maniobrabilidad.',
        type: 'textarea', key: 'currentBoard', placeholder: 'Ej.: Shortboard 6\'0, 30 L. Me gusta cómo gira, pero me cuesta remar y entrar temprano...'
    },
    {
        num: 'Pregunta 8 de 8',
        text: '¿Qué tipo de surfer sentís que sos?',
        hint: 'Esto describe tu forma de surfear. Es independiente de tu nivel técnico.',
        type: 'options',
        key: 'style',
        options: [
            { label: 'Performance / agresivo', desc: 'Busco maniobras verticales, respuesta e intensidad.', value: 'performance' },
            { label: 'Fluido', desc: 'Prefiero líneas largas, velocidad natural y continuidad.', value: 'cruisy' },
            { label: 'Versátil', desc: 'Me adapto a distintas tablas, olas y formas de surfear.', value: 'versatile' },
            { label: 'Recreativo', desc: 'Priorizo disfrutar, agarrar olas y surfear sin complicaciones.', value: 'fun' },
        ]
    }
];

let currentQ = 0;
const answers = {};

function renderQuestion() {
    const q = QUESTIONS[currentQ];
    const card = document.getElementById('recCard');

    ensureProgressSteps();

    // Progress dots
    document.querySelectorAll('.rec-step-dot').forEach((dot, i) => {
        dot.classList.remove('active', 'done');
        if (i < currentQ) dot.classList.add('done');
        else if (i === currentQ) dot.classList.add('active');
    });
    document.querySelectorAll('.rec-step-line').forEach((line, i) => {
        line.classList.toggle('done', i < currentQ);
    });

    let bodyHTML = '';

    if (q.type === 'options') {
        bodyHTML = `<div class="rec-options" id="recOptions">
        ${q.options.map(opt => `
          <div class="rec-option ${answers[q.key] === opt.value ? 'selected' : ''}"
               onclick="selectAnswer('${q.key}', '${opt.value}', this)">
            <span class="rec-option-label">${opt.label}</span>
            <span class="rec-option-desc">${opt.desc}</span>
          </div>
        `).join('')}
      </div>`;
    } else if (q.type === 'slider') {
        const val = answers[q.key] !== undefined ? answers[q.key] : q.default;
        bodyHTML = `
        <div class="rec-range-value" id="sliderVal">${val} ${q.unit}</div>
        <div class="rec-slider-wrap">
          <input type="range" class="rec-range" id="recSlider"
                 min="${q.min}" max="${q.max}" step="${q.step}" value="${val}"
                 oninput="updateSlider(this.value, '${q.unit}', '${q.key}')">
          <div class="rec-range-labels">
            <span>${q.min} ${q.unit}</span>
            <span>${q.max} ${q.unit}</span>
          </div>
        </div>
      `;
        answers[q.key] = val;
    } else if (q.type === 'compound-options') {
        bodyHTML = `<div class="rec-compound">
          ${q.fields.map(field => `
            <label class="rec-compound-field">
              <span>${field.label}</span>
              <select onchange="updateCompound('${field.key}', this.value)">
                <option value="">Seleccioná una opción</option>
                ${field.options.map(opt => `<option value="${opt.value}" ${answers[field.key] === opt.value ? 'selected' : ''}>${opt.label}</option>`).join('')}
              </select>
            </label>`).join('')}
        </div>`;
    } else if (q.type === 'textarea') {
        bodyHTML = `<textarea class="rec-textarea" maxlength="500" rows="5"
          placeholder="${q.placeholder}" oninput="updateTextAnswer('${q.key}', this.value)">${answers[q.key] || ''}</textarea>`;
    }

    card.innerHTML = `
      <p class="rec-question-num">${q.num}</p>
      <p class="rec-question-text">${q.text}</p>
      <p class="rec-question-hint">${q.hint}</p>
      ${bodyHTML}
      <div class="rec-nav">
        <button class="btn-rec-back" onclick="goBack()" ${currentQ === 0 ? 'style="opacity:0;pointer-events:none"' : ''}>← Atrás</button>
        <button class="btn-rec-next" id="recNext" onclick="goNext()"
                ${isQuestionAnswered(q) ? '' : 'disabled'}>
          ${currentQ < QUESTIONS.length - 1 ? 'Siguiente →' : 'Ver mi tabla ideal →'}
        </button>
      </div>
    `;
}

function ensureProgressSteps() {
    const progress = document.getElementById('recProgress');
    if (!progress || progress.querySelectorAll('.rec-step-dot').length === QUESTIONS.length) return;

    progress.innerHTML = QUESTIONS.map((_, index) =>
        `${index > 0 ? '<div class="rec-step-line"></div>' : ''}<div class="rec-step-dot ${index === 0 ? 'active' : ''}" data-step="${index}">${index + 1}</div>`
    ).join('');
}

function selectAnswer(key, value, el) {
    answers[key] = value;
    el.closest('.rec-options').querySelectorAll('.rec-option').forEach(o => o.classList.remove('selected'));
    el.classList.add('selected');
    document.getElementById('recNext').disabled = false;
}

function updateSlider(val, unit, key) {
    document.getElementById('sliderVal').textContent = `${val} ${unit}`;
    answers[key] = parseInt(val);
    document.getElementById('recNext').disabled = false;
}

function isQuestionAnswered(question) {
    if (question.type === 'compound-options') return question.fields.every(field => Boolean(answers[field.key]));
    if (question.type === 'textarea') return Boolean((answers[question.key] || '').trim());
    if (question.type === 'slider') return answers[question.key] !== undefined;
    return Boolean(answers[question.key]);
}

function updateCompound(key, value) {
    answers[key] = value;
    document.getElementById('recNext').disabled = !isQuestionAnswered(QUESTIONS[currentQ]);
}

function updateTextAnswer(key, value) {
    answers[key] = value;
    document.getElementById('recNext').disabled = value.trim().length < 3;
}

function goNext() {
    if (currentQ < QUESTIONS.length - 1) {
        currentQ++;
        renderQuestion();
    } else {
        showResult();
    }
}

function goBack() {
    if (currentQ > 0) { currentQ--; renderQuestion(); }
}

function restartRec() {
    currentQ = 0;
    Object.keys(answers).forEach(k => delete answers[k]);
    document.getElementById('rec-form').style.display = '';
    document.getElementById('rec-result').style.display = 'none';
    document.getElementById('recProgress').style.display = 'flex';
    renderQuestion();
}

/* ── RECOMMENDATION ENGINE ── */
function computeRecommendation() {
    const { experience, waves, weight, style, goal } = answers;
    const wt = weight || 72;

    // Scoring
    const scores = { gaucho: 0, pampeano: 0, charrua: 0, playero: 0 };

    // Experience
    if (experience === 'beginner') { scores.charrua += 3; scores.pampeano += 2; }
    if (experience === 'intermediate') { scores.pampeano += 3; scores.playero += 1; }
    if (experience === 'advanced') { scores.gaucho += 3; scores.playero += 2; }
    if (experience === 'expert') { scores.gaucho += 3; scores.playero += 3; }

    // Waves
    if (waves === 'small') { scores.playero += 3; scores.charrua += 2; }
    if (waves === 'medium') { scores.gaucho += 2; scores.pampeano += 2; }
    if (waves === 'powerful') { scores.gaucho += 3; }
    if (waves === 'varies') { scores.pampeano += 2; }

    // Weight → volume
    const targetVol = wt * (experience === 'beginner' ? 0.65 : experience === 'intermediate' ? 0.55 : experience === 'advanced' ? 0.42 : 0.36);
    const vols = { gaucho: 28.5, pampeano: 42, charrua: 75, playero: 35 };
    Object.keys(vols).forEach(k => {
        const diff = Math.abs(vols[k] - targetVol);
        scores[k] += Math.max(0, 3 - diff / 8);
    });

    // Style
    if (style === 'performance') { scores.gaucho += 3; }
    if (style === 'cruisy') { scores.charrua += 3; scores.pampeano += 1; }
    if (style === 'versatile') { scores.pampeano += 2; scores.gaucho += 1; }
    if (style === 'fun') { scores.playero += 2; scores.pampeano += 2; }

    // Goal
    if (goal === 'stability') { scores.charrua += 3; scores.pampeano += 2; }
    if (goal === 'speed') { scores.playero += 3; scores.gaucho += 1; }
    if (goal === 'maneuver') { scores.gaucho += 2; scores.playero += 2; }
    if (goal === 'performance') { scores.gaucho += 3; }
    if (goal === 'versatility') { scores.pampeano += 3; }

    const best = Object.entries(scores).sort((a, b) => b[1] - a[1])[0][0];
    return best;
}

function buildRecommendedSpecs(shape) {
    const experienceIndex = { beginner: 0, intermediate: 1, advanced: 2, expert: 3 }[answers.experience] ?? 1;
    const lengths = {
        gaucho: ['6\'2"', '6\'0"', '5\'10"', '5\'8"'],
        pampeano: ['7\'0"', '6\'10"', '6\'8"', '6\'6"'],
        charrua: ['9\'0"', '9\'0"', '8\'0"', '8\'0"'],
        playero: ['6\'2"', '6\'0"', '5\'10"', '5\'8"']
    };
    const widths = {
        gaucho: answers.weight >= 85 ? '19 1/2"' : answers.weight >= 70 ? '19"' : '18 3/4"',
        pampeano: answers.weight >= 85 ? '21"' : '20 1/2"',
        charrua: '22"',
        playero: answers.weight >= 85 ? '21"' : '20 1/2"'
    };
    const thicknesses = {
        gaucho: answers.weight >= 85 ? '2 1/2"' : '2 3/8"',
        pampeano: answers.weight >= 85 ? '2 3/4"' : '2 5/8"',
        charrua: '3"',
        playero: answers.weight >= 85 ? '2 3/4"' : '2 1/2"'
    };
    const factors = { beginner: .62, intermediate: .50, advanced: .40, expert: .34 };
    let volume = Number(answers.weight || 72) * factors[answers.experience];
    if (answers.goal === 'stability') volume += 4;
    if (answers.goal === 'performance') volume -= 2;
    if (shape === 'charrua') volume = Math.max(volume, 55);
    if (shape === 'playero') volume += 2;

    return {
        length: lengths[shape][experienceIndex],
        width: widths[shape],
        thickness: thicknesses[shape],
        volume: `${Math.max(22, Math.round(volume * 10) / 10)}L`
    };
}

const RESULT_REASONS = {
    gaucho: 'Una forma shortboard ofrece la respuesta y maniobrabilidad que mejor coinciden con tu perfil. Las medidas son iniciales y el shaper deberá validarlas.',
    pampeano: 'Una forma híbrida equilibra remada, estabilidad y maniobrabilidad para distintas condiciones. Las medidas son iniciales y el shaper deberá validarlas.',
    charrua: 'Una forma longboard prioriza estabilidad, remada y facilidad para entrar en la ola. Las medidas son iniciales y el shaper deberá validarlas.',
    playero: 'Una forma fish aporta velocidad y fluidez, especialmente en olas chicas o con poca fuerza. Las medidas son iniciales y el shaper deberá validarlas.',
};

let recommendedBoard = null;
let recommendedSpecs = null;

function showResult() {
    const best = computeRecommendation();
    recommendedBoard = best;
    const b = BOARDS[best];
    recommendedSpecs = buildRecommendedSpecs(best);

    document.getElementById('rec-form').style.display = 'none';
    document.getElementById('rec-result').style.display = 'block';
    document.getElementById('recProgress').style.display = 'none';

    document.getElementById('resultTitle').textContent = b.name;
    const surferTypes = { performance: 'performance', cruisy: 'fluido', versatile: 'versátil', fun: 'recreativo' };
    document.getElementById('resultSub').textContent = `Perfil ${surferTypes[answers.style] || 'personalizado'} · recomendación inicial para revisar con el shaper`;
    document.getElementById('resultCat').textContent = b.cat;
    document.getElementById('resultName').textContent = b.name;
    document.getElementById('resultSpec').textContent = `${recommendedSpecs.length} × ${recommendedSpecs.width} × ${recommendedSpecs.thickness}`;
    document.getElementById('resultWhy').textContent = RESULT_REASONS[best];
    document.getElementById('resultPrice').textContent = 'Precio a confirmar por el shaper';
    document.getElementById('resultSpecStats').innerHTML = `
      <div><div class="rspec-num">${recommendedSpecs.volume}</div><div class="rspec-label">Volumen inicial</div></div>
      <div><div class="rspec-num">${recommendedSpecs.length}</div><div class="rspec-label">Largo inicial</div></div>
      <div><div class="rspec-num">${answers.height} cm</div><div class="rspec-label">Tu altura</div></div>
      <div><div class="rspec-num">${answers.weight} kg</div><div class="rspec-label">Tu peso</div></div>
    `;

    const visual = document.getElementById('resultBoardSvg');
    const boardImage = document.createElement('img');
    boardImage.className = 'result-board-image';
    boardImage.src = '/img/boards/deck-mask.png';
    boardImage.alt = `Vista frontal de ${b.name}`;
    boardImage.addEventListener('error', function () {
        if (!this.src.endsWith('/img/boards/deck-mask.png')) this.src = '/img/boards/deck-mask.png';
    });
    visual.replaceChildren(boardImage);
}

function addResultToCart() {
    applyRecommendedBoard();
}

function applyRecommendedBoard() {
    if (!recommendedBoard) {
        showToast('Primero completá el cuestionario para obtener una recomendación');
        return;
    }

    const customizer = document.getElementById('customizador');
    customizer?.scrollIntoView({ behavior: 'smooth', block: 'start' });

    const modelSelect =
        document.getElementById('customModel');

    if (modelSelect) {
        const optionExists = Array.from(
            modelSelect.options
        ).some(function (option) {
            return option.value === recommendedBoard;
        });

        if (optionExists) {
            modelSelect.value = recommendedBoard;

            try {
                modelSelect.dispatchEvent(new Event('change'));
            } catch (error) {
                console.error('No se pudieron aplicar automáticamente todos los datos recomendados.', error);
            }
        }
    }

    if (recommendedSpecs) {
        setSelectRecommendation('customLength', recommendedSpecs.length);
        setSelectRecommendation('customWidth', recommendedSpecs.width);
        setSelectRecommendation('customThickness', recommendedSpecs.thickness);
        const volumeInput = document.getElementById('customVolume');
        if (volumeInput) volumeInput.value = recommendedSpecs.volume;
        updateCustomPreview();
    }

    const paintOptions = document.getElementById('paintOptions');
    if (paintOptions) paintOptions.hidden = false;

    const transfer = document.getElementById('questionnaireTransfer');
    const transferText = document.getElementById('questionnaireTransferText');
    if (transfer && transferText) {
        transferText.textContent = buildQuestionnaireProfile(true);
        transfer.hidden = false;
    }

    window.history.replaceState(null, '', '#customizador');
}

function setSelectRecommendation(id, value) {
    const select = document.getElementById(id);
    if (!select) return;
    let option = Array.from(select.options).find(item => item.value === value || item.text === value);
    if (!option) {
        option = new Option(value, value);
        select.add(option);
    }
    select.value = option.value;
}

function buildQuestionnaireProfile(compact = false) {
    if (!answers.height || !answers.weight || !answers.experience) return '';

    const labels = {
        experience: { beginner: 'Principiante', intermediate: 'Intermedio', advanced: 'Avanzado', expert: 'Profesional/competitivo' },
        surfTime: { less_one: 'Menos de 1 año', one_three: '1 a 3 años', three_seven: '3 a 7 años', more_seven: 'Más de 7 años' },
        frequency: { occasional: 'Algunas veces al mes', weekly: '1 vez por semana', regular: '2 a 3 veces por semana', high: '4 o más veces por semana' },
        waves: { small: 'Olas chicas y planas', medium: 'Olas medianas', powerful: 'Olas con power', varies: 'Condiciones variadas' },
        goal: { stability: 'Estabilidad y facilidad', speed: 'Velocidad y fluidez', maneuver: 'Maniobrabilidad', performance: 'Performance', versatility: 'Versatilidad' },
        style: { performance: 'Performance/agresivo', cruisy: 'Fluido', versatile: 'Versátil', fun: 'Recreativo' }
    };
    const value = (group, key) => labels[group][answers[key]] || answers[key] || 'Sin indicar';

    if (compact) {
        return `${answers.height} cm · ${answers.weight} kg · ${value('experience', 'experience')} · ${value('style', 'style')}. Objetivo: ${value('goal', 'goal')}.`;
    }

    return [
        `Altura: ${answers.height} cm`,
        `Peso: ${answers.weight} kg`,
        `Nivel: ${value('experience', 'experience')}`,
        `Experiencia: ${value('surfTime', 'surfTime')}`,
        `Frecuencia: ${value('frequency', 'frequency')}`,
        `Olas habituales: ${value('waves', 'waves')}`,
        `Objetivo principal: ${value('goal', 'goal')}`,
        `Tipo de surfer: ${value('style', 'style')}`,
        `Tabla actual y cambios buscados: ${answers.currentBoard || 'Sin indicar'}`
    ].join('\n');
}

function restartQuestionnaireFromOrder() {
    restartRec();
    document.getElementById('questionnaireTransfer')?.setAttribute('hidden', 'hidden');
    document.getElementById('recomendador')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

renderQuestion();



document.addEventListener(
    'DOMContentLoaded',
    function () {
        const stockMeasurements =
            document.getElementById(
                'customStockMeasurements'
            );

        if (stockMeasurements) {
            stockMeasurements.addEventListener(
                'change',
                function () {
                    const values =
                        this.value.split('|');

                    const length = values[0];
                    const width = values[1];
                    const thickness = values[2];
                    const volume = values[3];

                    const lengthInput =
                        document.getElementById(
                            'customLength'
                        );

                    const widthInput =
                        document.getElementById(
                            'customWidth'
                        );

                    const thicknessInput =
                        document.getElementById(
                            'customThickness'
                        );

                    const volumeInput =
                        document.getElementById(
                            'customVolume'
                        );

                    if (lengthInput) {
                        lengthInput.value =
                            length + '"';
                    }

                    if (widthInput) {
                        widthInput.value =
                            width + '"';
                    }

                    if (thicknessInput) {
                        thicknessInput.value =
                            thickness + '"';
                    }

                    if (volumeInput) {
                        volumeInput.value =
                            volume + 'L';
                    }

                    updateCustomPreview();
                }
            );
        }

        const controlIds = [
            'customModel',
            'customConstruction',
            'customStockMeasurements',
            'customLength',
            'customWidth',
            'customThickness',
            'customVolume',
            'customTail',
            'customFinSystem',
            'customFinConfiguration',
            'customFinOption',
            'customGlassing',
            'customCarbonPatch',
            'customDesign',
            'customColor',
            'customSecondaryColor',
            'customGripColor',
            'customDecal',
            'customStringerName',
            'surferHeight',
            'surferWeight',
            'surferLevel',
            'surferFitness'
        ];

        controlIds.forEach(function (id) {
            const control =
                document.getElementById(id);

            if (!control) {
                return;
            }

            control.addEventListener(
                'change',
                updateCustomPreview
            );

            control.addEventListener(
                'input',
                updateCustomPreview
            );
        });

        updateCustomPreview();
    }
);

function changeBoardView(view, button) {
    const deckStage = document.getElementById('deckStage');
    const bottomStage = document.getElementById('bottomStage');

    document.querySelectorAll('.board-view-tab').forEach(function (tab) {
        tab.classList.remove('active');
    });

    if (button) {
        button.classList.add('active');
    }

    if (view === 'bottom') {
        if (deckStage) {
            deckStage.classList.remove('active');
            deckStage.hidden = true;
        }

        if (bottomStage) {
            bottomStage.hidden = false;
            bottomStage.classList.add('active');
        }
    } else {
        if (bottomStage) {
            bottomStage.classList.remove('active');
            bottomStage.hidden = true;
        }

        if (deckStage) {
            deckStage.hidden = false;
            deckStage.classList.add('active');
        }
    }
}

/* ════════════════════════════════════
   ACCESORIOS DEL CUSTOM ORDER
════════════════════════════════════ */

const selectedAccessories = new Map();

const ACCESSORY_FIN_IMAGES = {
    'fins-reactor': 'thruster.png',
    'fins-twin': 'twin.png',
    'fins-quad': 'quad.png',
    'fins-five': 'five-fin.png'
};

/* Seleccionar o quitar un accesorio */
function selectAccessory(card) {
    if (!card) {
        return;
    }

    const accessoryId = card.dataset.accessoryId;

    if (!accessoryId) {
        console.warn(
            'La tarjeta no tiene data-accessory-id',
            card
        );

        return;
    }

    const accessoryName =
        card.dataset.accessoryName || 'Accesorio';

    const accessoryPrice =
        Number(card.dataset.accessoryPrice || 0);

    const wasSelected =
        card.classList.contains('selected');

    const panel =
        card.closest('.accessory-panel');

    /*
       Permite una selección por categoría:
       una quilla, un leash y un tail pad.
    */
    if (panel) {
        panel
            .querySelectorAll('.accessory-product')
            .forEach(function (product) {
                product.classList.remove('selected');

                const productId =
                    product.dataset.accessoryId;

                if (productId) {
                    selectedAccessories.delete(productId);
                }
            });
    }

    /*
       Si tocamos una tarjeta que ya estaba seleccionada,
       queda deseleccionada.
    */
    if (wasSelected) {
        updateAccessoryPreview();
        updateCustomSummary();
        return;
    }

    card.classList.add('selected');

    const accessorySelect =
        card.querySelector('.accessory-select');

    selectedAccessories.set(accessoryId, {
        id: accessoryId,
        name: accessoryName,
        price: accessoryPrice,
        type: card.dataset.accessoryType || '',
        image: card.dataset.accessoryImage || '',
        option: accessorySelect
            ? accessorySelect.value
            : ''
    });

    updateAccessoryPreview();
    updateCustomSummary();
}

/* Cambiar entre Fins, Leash y Tail Pads */
function showAccessoryTab(tabName, button) {
    document
        .querySelectorAll('.accessory-tab')
        .forEach(function (tab) {
            tab.classList.remove('active');
        });

    document
        .querySelectorAll('.accessory-panel')
        .forEach(function (panel) {
            panel.classList.remove('active');
            panel.hidden = true;
        });

    if (button) {
        button.classList.add('active');
    }

    const panels = {
        fins: document.getElementById('accessoryFins'),
        leash: document.getElementById('accessoryLeash'),
        pads: document.getElementById('accessoryPads')
    };

    const selectedPanel = panels[tabName];

    if (selectedPanel) {
        selectedPanel.hidden = false;
        selectedPanel.classList.add('active');
    }
}

/* Cambio de tamaño/color dentro del accesorio */
document.addEventListener('change', function (event) {
    const select = event.target.closest('.accessory-select');

    if (!select) {
        return;
    }

    const card =
        select.closest('.accessory-product');

    if (!card) {
        return;
    }

    const accessoryId =
        card.dataset.accessoryId;

    /*
       Si todavía no estaba seleccionado,
       seleccionarlo al cambiar su opción.
    */
    if (!card.classList.contains('selected')) {
        selectAccessory(card);
        return;
    }

    const accessory =
        selectedAccessories.get(accessoryId);

    if (accessory) {
        accessory.option = select.value;
    }

    updateCustomSummary();
});

/* Mostrar los accesorios en la tabla */
function updateAccessoryPreview() {
    const deckGrip =
        document.getElementById('deckGrip');

    const bottomFins =
        document.getElementById('bottomFins');

    /* ── Tail pad ── */

    const blackGrip =
        selectedAccessories.has('grip-black');

    const whiteGrip =
        selectedAccessories.has('grip-white');

    if (deckGrip) {
        if (blackGrip) {
            deckGrip.src =
                BOARD_IMAGE_PATH + 'grip-black.png';

            deckGrip.hidden = false;
        } else if (whiteGrip) {
            deckGrip.src =
                BOARD_IMAGE_PATH + 'grip-white.png';

            deckGrip.hidden = false;
        } else {
            /*
               Si no hay grip seleccionado como accesorio,
               se respeta el selector general del customizador.
            */
            const customGripColor =
                getSelectedValue(
                    'customGripColor',
                    ''
                );

            if (customGripColor === 'black') {
                deckGrip.src =
                    BOARD_IMAGE_PATH + 'grip-black.png';

                deckGrip.hidden = false;
            } else if (customGripColor === 'white') {
                deckGrip.src =
                    BOARD_IMAGE_PATH + 'grip-white.png';

                deckGrip.hidden = false;
            } else {
                deckGrip.hidden = true;
            }
        }
    }

    /* ── Quillas ── */

    const selectedFin =
        Array
            .from(selectedAccessories.values())
            .find(function (item) {
                return item.id.startsWith('fins-');
            });

    if (bottomFins) {
        let imageName;

        if (selectedFin) {
            imageName =
                ACCESSORY_FIN_IMAGES[selectedFin.id];
        }

        /*
           Si no se eligió una quilla como accesorio,
           usa la configuración elegida en Board Details.
        */
        if (!imageName) {
            const finConfiguration =
                getSelectedValue(
                    'customFinConfiguration',
                    'thruster'
                );

            imageName =
                FIN_IMAGES[finConfiguration] ||
                'thruster.png';
        }

        bottomFins.src =
            BOARD_IMAGE_PATH + imageName;

        bottomFins.hidden = false;
    }
}

/* Actualizar el resumen y el precio */
function updateCustomSummary() {
    const modelSelect =
        document.getElementById('customModel');

    const modelName =
        modelSelect &&
            modelSelect.selectedIndex >= 0
            ? modelSelect.options[
                modelSelect.selectedIndex
            ].text
            : 'Tabla Master';

    const boardPrice =
        calculateCustomPrice();

    let accessoriesTotal = 0;
    let accessoryHtml = '';

    selectedAccessories.forEach(function (item) {
        accessoriesTotal += item.price;

        const optionText =
            item.option
                ? ` · ${escapeAccessoryHtml(item.option)}`
                : '';

        accessoryHtml += `
            <div class="custom-summary-row">
                <span>
                    ${escapeAccessoryHtml(item.name)}
                    ${optionText}
                </span>

                <span>
                    USD ${item.price}
                </span>
            </div>
        `;
    });

    const summaryBoardName =
        document.getElementById('summaryBoardName');

    const summaryBoardPrice =
        document.getElementById('summaryBoardPrice');

    const summaryFinSetup =
        document.getElementById('summaryFinSetup');

    const summaryFinPrice =
        document.getElementById('summaryFinPrice');

    const summaryAccessories =
        document.getElementById('summaryAccessories');

    const summaryTotal =
        document.getElementById('summaryTotal');

    if (summaryBoardName) {
        summaryBoardName.textContent =
            'Master ' + modelName;
    }

    if (summaryBoardPrice) {
        summaryBoardPrice.textContent = 'A confirmar por el shaper';
    }

    const selectedFin =
        Array
            .from(selectedAccessories.values())
            .find(function (item) {
                return item.id.startsWith('fins-');
            });

    if (summaryFinSetup) {
        if (selectedFin) {
            summaryFinSetup.textContent =
                selectedFin.name +
                (
                    selectedFin.option
                        ? ` · ${selectedFin.option}`
                        : ''
                );
        } else {
            summaryFinSetup.textContent =
                'Fin Setup (' +
                getSelectedOptionText(
                    'customFinConfiguration'
                ) +
                ')';
        }
    }

    if (summaryFinPrice) {
        summaryFinPrice.textContent =
            selectedFin
                ? 'USD ' + selectedFin.price
                : 'Incluido';
    }

    if (summaryAccessories) {
        /*
           Evita repetir la quilla porque ya se muestra
           en summaryFinSetup.
        */
        let otherAccessoriesHtml = '';

        selectedAccessories.forEach(function (item) {
            if (item.id.startsWith('fins-')) {
                return;
            }

            otherAccessoriesHtml += `
                <div class="custom-summary-row">
                    <span>
                        ${escapeAccessoryHtml(item.name)}
                        ${item.option
                    ? ` · ${escapeAccessoryHtml(item.option)}`
                    : ''
                }
                    </span>

                    <span>
                        USD ${item.price}
                    </span>
                </div>
            `;
        });

        summaryAccessories.innerHTML =
            otherAccessoriesHtml;
    }

    if (summaryTotal) {
        summaryTotal.textContent = accessoriesTotal > 0
            ? `Tabla a confirmar · Accesorios USD ${accessoriesTotal}`
            : 'A confirmar por el shaper';
    }

    /*
       Actualizar también la información debajo
       de la tabla.
    */
    const sidePreviewPrice =
        document.getElementById('sidePreviewPrice');

    if (sidePreviewPrice) {
        sidePreviewPrice.textContent = 'A confirmar por el shaper';
    }
}

/* Evitar insertar HTML desde los datos */
function escapeAccessoryHtml(value) {
    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}

/* ════════════════════════════════════
   ORDER SUBMIT
════════════════════════════════════ */
function submitOrder() {
    const cart = getCart();
    if (!cart.length) { showToast('Agregá al menos una tabla al carrito'); return; }
    const fname = document.getElementById('fname')?.value;
    const femail = document.getElementById('femail')?.value;
    if (!fname || !femail) { showToast('Completá al menos tu nombre y email'); return; }
    showToast(`¡Gracias ${fname}! Francisco te escribe en 24hs.`);
    // Could also redirect to carrito.html
    setTimeout(() => {
        localStorage.removeItem('master_cart');
        updateBadge();
        renderOrderSummary();
    }, 1500);
}

/* ════════════════════════════════════
   TOAST
════════════════════════════════════ */
function showToast(msg) {
    const tc = document.getElementById('toastContainer');
    const t = document.createElement('div');
    t.className = 'zsg-toast';
    t.innerHTML = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="${msg.includes('!') ? '#2ec27e' : 'var(--gold)'}" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>${msg}`;
    tc.appendChild(t);
    setTimeout(() => t.remove(), 3400);
}


/* ════════════════════════════════════
   LANG SWITCHER
════════════════════════════════════ */
const LANG_FLAGS = { es: '🇺🇾', en: '🇬🇧', pt: '🇧🇷' };
const LANG_CODES = { es: 'ES', en: 'EN', pt: 'PT' };

let currentLang = localStorage.getItem('zsg_lang') || 'es';

function toggleLangDropdown() {
    document.getElementById('langSwitcher').classList.toggle('open');
}
document.addEventListener('click', e => {
    if (!document.getElementById('langSwitcher')?.contains(e.target)) {
        document.getElementById('langSwitcher')?.classList.remove('open');
    }
});

function setLang(lang) {
    currentLang = lang;
    localStorage.setItem('zsg_lang', lang);
    document.getElementById('langFlag').textContent = LANG_FLAGS[lang];
    document.getElementById('langCode').textContent = LANG_CODES[lang];
    document.querySelectorAll('.lang-option').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.lang === lang);
    });
    document.getElementById('langSwitcher').classList.remove('open');
    document.documentElement.lang = lang;
    if (typeof applyTranslations === 'function') applyTranslations();
}

// Init switcher display
(function initLang() {
    const flag = document.getElementById('langFlag');
    const code = document.getElementById('langCode');
    if (flag) flag.textContent = LANG_FLAGS[currentLang];
    if (code) code.textContent = LANG_CODES[currentLang];
    document.querySelectorAll('.lang-option').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.lang === currentLang);
    });
    document.documentElement.lang = currentLang;
})();


