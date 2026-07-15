
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

const BOARDS = {
    gaucho: { name: 'El Gaucho', cat: 'Shortboard · Performance', spec: '6\'2" × 18¾" × 2⅜"', vol: '28.5L', price: 780 },
    pampeano: { name: 'El Pampeano', cat: 'Mid-Length · Versátil', spec: '7\'0" × 20½" × 2¾"', vol: '42L', price: 660 },
    charrua: { name: 'El Charrúa', cat: 'Longboard · Clásico', spec: '9\'2" × 22½" × 3"', vol: '75L', price: 950 },
    playero: { name: 'El Playero', cat: 'Fish · Retro', spec: '5\'10" × 20¾" × 2½"', vol: '35L', price: 590 },
};

function addToCart(id, name, price) {
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

function createBoardBackground(design, primary, secondary) {
    switch (design) {
        case 'sin-pintura':
            return 'transparent';

        case 'mitad-y-mitad':
            return `linear-gradient(
                90deg,
                ${primary} 0 50%,
                ${secondary} 50% 100%
            )`;

        case 'degrade':
            return `linear-gradient(
                180deg,
                ${primary},
                ${secondary}
            )`;

        case 'rails-color':
            return `linear-gradient(
                90deg,
                ${primary} 0 14%,
                transparent 14% 86%,
                ${primary} 86% 100%
            )`;

        case 'nose-color':
            return `linear-gradient(
                180deg,
                ${primary} 0 28%,
                transparent 28% 100%
            )`;

        case 'tail-color':
            return `linear-gradient(
                180deg,
                transparent 0 72%,
                ${primary} 72% 100%
            )`;

        case 'lineas-clasicas':
            return `linear-gradient(
                180deg,
                transparent 0 30%,
                ${primary} 30% 36%,
                ${secondary} 36% 39%,
                transparent 39% 100%
            )`;

        case 'pinline':
            return `linear-gradient(
                90deg,
                transparent 0 7%,
                ${primary} 7% 9%,
                transparent 9% 91%,
                ${primary} 91% 93%,
                transparent 93% 100%
            )`;

        case 'retro':
            return `linear-gradient(
                90deg,
                transparent 0 28%,
                ${primary} 28% 43%,
                ${secondary} 43% 50%,
                transparent 50% 100%
            )`;

        case 'doble-color':
            return `linear-gradient(
                180deg,
                ${primary} 0 50%,
                ${secondary} 50% 100%
            )`;

        case 'abstracto':
            return `
                radial-gradient(
                    ellipse at 25% 25%,
                    ${primary} 0 18%,
                    transparent 19%
                ),
                radial-gradient(
                    ellipse at 75% 58%,
                    ${secondary} 0 24%,
                    transparent 25%
                ),
                linear-gradient(
                    145deg,
                    transparent 0 35%,
                    ${primary} 36% 52%,
                    transparent 53%
                )
            `;

        case 'minimalista':
            return `radial-gradient(
                circle at 50% 32%,
                ${primary} 0 11%,
                transparent 12%
            )`;

        case 'deck-completo':
        case 'bottom-completo':
            return primary;

        case 'sol-pampeano':
        default:
            return `repeating-conic-gradient(
                from 0deg at 50% 28%,
                ${primary} 0deg 10deg,
                ${secondary} 10deg 20deg
            )`;
    }
}

function updateBoardImages() {
    const deckPaint =
        document.getElementById('deckPaint');

    const bottomPaint =
        document.getElementById('bottomPaint');

    if (!deckPaint || !bottomPaint) {
        return;
    }

    const design =
        getSelectedValue(
            'customDesign',
            'sol-pampeano'
        );

    const primary =
        getSelectedValue(
            'customColor',
            '#c9a84c'
        );

    const secondary =
        getSelectedValue(
            'customSecondaryColor',
            '#121212'
        );

    const background =
        createBoardBackground(
            design,
            primary,
            secondary
        );

    /*
       Podés decidir que ciertos diseños aparezcan solo
       de un lado de la tabla.
    */
    if (design === 'bottom-completo') {
        deckPaint.style.background = 'transparent';
        bottomPaint.style.background = primary;
    } else if (design === 'deck-completo') {
        deckPaint.style.background = primary;
        bottomPaint.style.background = 'transparent';
    } else {
        deckPaint.style.background = background;
        bottomPaint.style.background = background;
    }

    updateBoardAccessories();
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
    'twin-strength': 980
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

    setText(
        'sidePreviewPrice',
        'USD ' + price
    );

    const previewPrice =
        document.getElementById('previewPrice');

    if (previewPrice) {
        previewPrice.textContent =
            'USD ' + price;
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
                .find(item => item.id.startsWith('fins-'));

        if (selectedFin) {

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

        finImage.src =
            BOARD_IMAGE_PATH +
            FIN_IMAGES[finSetup];

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

function addCustomToCart() {
    const modelSelect = document.getElementById('customModel');

    if (!modelSelect) {
        showToast('No se encontró el personalizador');
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

    const priceText =
        document.getElementById('previewPrice')?.textContent || 'USD 0';

    const price =
        Number(priceText.replace(/[^0-9.]/g, '')) || 0;

    const activeDetails = Array.from(
        document.querySelectorAll(
            '#customizador [data-detail].active'
        )
    ).map(function (button) {
        return button.textContent.trim();
    });

    const details = [
        `${length} × ${width} × ${thickness}`,
        volume,
        selectedText('customConstruction'),
        selectedText('customTail'),
        selectedText('customFinSystem') +
        ' · ' +
        selectedText('customFinConfiguration'),
        selectedText('customGlassing'),
        selectedText('customCarbonPatch'),
        selectedText('customDesign'),
        activeDetails.join(', ')
    ]
        .filter(function (value) {
            return value !== '';
        })
        .join(' · ');

    const cart = getCart();

    cart.push({
        id: 'custom_' + modelSelect.value + '_' + Date.now(),
        name: modelName + ' (Custom)',
        price: price,
        qty: 1,
        custom: true,
        details: details,
        notes:
            document.getElementById('shaperNotes')
                ?.value.trim() || ''
    });

    saveCart(cart);

    showToast(
        '"' + modelName + ' Custom" agregada al carrito'
    );
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
        num: 'Pregunta 1 de 6',
        text: '¿Cuánto tiempo llevás surfeando?',
        hint: 'Esto nos ayuda a entender qué tan familiarizado estás con el surf y qué tipo de tabla te va a funcionar mejor.',
        type: 'options',
        key: 'experience',
        options: [
            { icon: '🌊', label: 'Estoy empezando', desc: 'Menos de 1 año o todavía aprendiendo a pararme.', value: 'beginner' },
            { icon: '🏄', label: '1 a 3 años', desc: 'Ya me paro y surfeo olas chicas con algo de control.', value: 'intermediate' },
            { icon: '🏄‍♂️', label: '3 a 7 años', desc: 'Surfeo seguido, hago maniobras básicas y me defiendo.', value: 'advanced' },
            { icon: '🔥', label: 'Más de 7 años', desc: 'Surfeo hace rato, soy consistente y quiero performance.', value: 'expert' },
        ]
    },
    {
        num: 'Pregunta 2 de 6',
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
        num: 'Pregunta 3 de 6',
        text: '¿Cuál es tu peso?',
        hint: 'El volumen ideal de la tabla se calcula en base a tu peso y nivel. Es la variable más importante.',
        type: 'slider',
        key: 'weight',
        min: 45, max: 120, step: 1, default: 72,
        unit: 'kg'
    },
    {
        num: 'Pregunta 4 de 6',
        text: '¿Qué estilo de surf te identifica?',
        hint: 'Cada tabla está diseñada para un estilo particular. Elegí el que más te representa o al que aspirás.',
        type: 'options',
        key: 'style',
        options: [
            { icon: '⚡', label: 'Agresivo / performance', desc: 'Maniobras verticales, aéreos, surf de alta intensidad.', value: 'performance' },
            { icon: '🌀', label: 'Fluido / maniobras largas', desc: 'Turns suaves, longboard vibes, nose time.', value: 'cruisy' },
            { icon: '🎯', label: 'Versátil / intermedio', desc: 'Un poco de todo. Quiero divertirme en todas las olas.', value: 'versatile' },
            { icon: '🏖️', label: 'Recreativo', desc: 'Me divierto sin complicarme. No compito.', value: 'fun' },
        ]
    },
    {
        num: 'Pregunta 5 de 6',
        text: '¿Qué querés mejorar de tu surf?',
        hint: 'La tabla correcta puede acelerar enormemente tu progresión en el área que más te importa.',
        type: 'options',
        key: 'goal',
        options: [
            { icon: '⬆️', label: 'Surf más arriba en la ola', desc: 'Quiero maniobras más en el lip y verticales.', value: 'attack' },
            { icon: '🔄', label: 'Más control y consistencia', desc: 'Caigo mucho, quiero ser más consistente.', value: 'control' },
            { icon: '🕊️', label: 'Más deslizamiento y flow', desc: 'Quiero que el surf se sienta más fluido.', value: 'flow' },
            { icon: '📈', label: 'Subir de nivel en general', desc: 'Estoy estancado y necesito un cambio.', value: 'level_up' },
        ]
    },
    {
        num: 'Pregunta 6 de 6',
        text: '¿Qué priorizás en una tabla?',
        hint: 'Todos queremos todo, pero la geometría de la tabla implica compromisos. ¿Qué va primero?',
        type: 'options',
        key: 'priority',
        options: [
            { icon: '⚡', label: 'Velocidad', desc: 'Que vuele y entre rápido en la ola.', value: 'speed' },
            { icon: '🎛️', label: 'Maniobrabilidad', desc: 'Que gire fácil y responda en el lip.', value: 'maneuver' },
            { icon: '⚖️', label: 'Estabilidad', desc: 'Que sea predecible y me dé confianza.', value: 'stability' },
            { icon: '🌊', label: 'Paddle power', desc: 'Que entre en todas las olas sin esfuerzo.', value: 'paddle' },
        ]
    }
];

let currentQ = 0;
const answers = {};

function renderQuestion() {
    const q = QUESTIONS[currentQ];
    const card = document.getElementById('recCard');

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
    }

    card.innerHTML = `
      <p class="rec-question-num">${q.num}</p>
      <p class="rec-question-text">${q.text}</p>
      <p class="rec-question-hint">${q.hint}</p>
      ${bodyHTML}
      <div class="rec-nav">
        <button class="btn-rec-back" onclick="goBack()" ${currentQ === 0 ? 'style="opacity:0;pointer-events:none"' : ''}>← Atrás</button>
        <button class="btn-rec-next" id="recNext" onclick="goNext()"
                ${(q.type !== 'options' || answers[q.key]) ? '' : 'disabled'}>
          ${currentQ < QUESTIONS.length - 1 ? 'Siguiente →' : 'Ver mi tabla ideal →'}
        </button>
      </div>
    `;
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
    const { experience, waves, weight, style, goal, priority } = answers;
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
    if (goal === 'attack') { scores.gaucho += 2; }
    if (goal === 'control') { scores.pampeano += 2; scores.charrua += 1; }
    if (goal === 'flow') { scores.charrua += 2; scores.pampeano += 1; }
    if (goal === 'level_up') { scores.pampeano += 2; }

    // Priority
    if (priority === 'speed') { scores.gaucho += 2; scores.playero += 1; }
    if (priority === 'maneuver') { scores.gaucho += 2; scores.playero += 2; }
    if (priority === 'stability') { scores.charrua += 2; scores.pampeano += 2; }
    if (priority === 'paddle') { scores.charrua += 3; scores.pampeano += 2; }

    const best = Object.entries(scores).sort((a, b) => b[1] - a[1])[0][0];
    return best;
}

const RESULT_REASONS = {
    gaucho: 'Con tu nivel y el tipo de olas que surfeás, el Gaucho es la herramienta perfecta: rocker medio, raíles medios-bajos y el volumen justo para que surfées agresivo sin perder flujo. Es la tabla con la que más vas a mejorar este año.',
    pampeano: 'El Pampeano es exactamente lo que necesitás ahora: volumen suficiente para entrar cómodo en cualquier ola, pero con una plantilla que ya te permite explorar maniobras. Es la tabla que más surfers en tu nivel disfrutan.',
    charrua: 'El Charrúa es tu tabla. Para las olas que surfeás y tu estilo, el volumen y el deslizamiento de un longboard clásico te van a dar una experiencia completamente diferente y mucho más placer en el agua.',
    playero: 'El Playero fue hecho para vos. Con las olas más chatas, el twin-fin fish te da una velocidad y soltura que ninguna tabla de rocker convencional puede igualar. Una vez que la probás, es difícil volver.',
};

let recommendedBoard = null;

function showResult() {
    const best = computeRecommendation();
    recommendedBoard = best;
    const b = BOARDS[best];

    document.getElementById('rec-form').style.display = 'none';
    document.getElementById('rec-result').style.display = 'block';
    document.getElementById('recProgress').style.display = 'none';

    document.getElementById('resultTitle').textContent = b.name;
    document.getElementById('resultCat').textContent = b.cat;
    document.getElementById('resultName').textContent = b.name;
    document.getElementById('resultSpec').textContent = b.spec;
    document.getElementById('resultWhy').textContent = RESULT_REASONS[best];
    document.getElementById('resultPrice').textContent = `USD ${b.price}`;
    document.getElementById('resultSpecStats').innerHTML = `
      <div><div class="rspec-num">${b.vol}</div><div class="rspec-label">Volumen</div></div>
      <div><div class="rspec-num">${b.spec.split('×')[0].trim()}</div><div class="rspec-label">Largo</div></div>
    `;

    // Board SVG
    const colors = { gaucho: '#c9a84c', pampeano: '#4394e0', charrua: '#2ec27e', playero: '#9060c8' };
    const col = colors[best];
    document.getElementById('resultBoardSvg').innerHTML = `
      <svg width="90" height="280" viewBox="0 0 90 280" xmlns="http://www.w3.org/2000/svg" style="filter:drop-shadow(0 12px 28px rgba(0,0,0,.7))">
        <path d="M45 5 C62 28, 70 75, 70 140 C70 195 62 248 45 274 C28 248 20 195 20 140 C20 75 28 28 45 5Z" fill="${col}" opacity=".85"/>
        <line x1="45" y1="7" x2="45" y2="272" stroke="rgba(255,255,255,.2)" stroke-width="1"/>
        <text x="45" y="100" text-anchor="middle" font-family="'Bebas Neue',sans-serif" font-size="8" fill="rgba(255,255,255,.6)" letter-spacing="1">MASTER</text>
      </svg>
    `;
}

function addResultToCart() {
    if (!recommendedBoard) return;
    const b = BOARDS[recommendedBoard];
    addToCart(recommendedBoard + '_rec', b.name, b.price);
}

function applyRecommendedBoard() {
    if (!recommendedBoard) {
        return;
    }

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

            modelSelect.dispatchEvent(
                new Event('change')
            );
        }
    }

    document
        .getElementById('customizador')
        ?.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
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
        summaryBoardPrice.textContent =
            'USD ' + boardPrice;
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
        summaryTotal.textContent =
            'USD ' +
            (boardPrice + accessoriesTotal);
    }

    /*
       Actualizar también la información debajo
       de la tabla.
    */
    const sidePreviewPrice =
        document.getElementById('sidePreviewPrice');

    if (sidePreviewPrice) {
        sidePreviewPrice.textContent =
            'USD ' +
            (boardPrice + accessoriesTotal);
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


