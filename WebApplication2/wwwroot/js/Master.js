
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

function addCustomToCart() {
    const model = document.querySelector('#modelSelector .active')?.dataset.model || 'gaucho';
    const board = BOARDS[model];
    const design = document.querySelector('#designGrid .design-swatch.active')?.dataset.name || 'Clásico';
    const fin = document.querySelector('#finGrid .fin-option.active .fin-label')?.textContent || 'Thruster';
    const notes = document.getElementById('shaperNotes')?.value || '';
    const cart = getCart();
    const customId = model + '_custom';
    const existing = cart.find(i => i.id === customId);
    if (existing) existing.qty += 1;
    else cart.push({
        id: customId,
        name: board.name + ' (Custom)',
        price: board.price + 80,
        qty: 1,
        custom: true,
        details: `${board.spec} · ${fin} · ${design}`,
        notes
    });
    saveCart(cart);
    showToast(`"${board.name} Custom" agregada al carrito`);
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
    if (!recommendedBoard) return;
    const btn = document.querySelector(`#modelSelector [data-model="${recommendedBoard}"]`);
    if (btn) {
        document.querySelectorAll('#modelSelector .toggle-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        updatePreviewFromModel(recommendedBoard);
    }
}

renderQuestion();

/* ════════════════════════════════════
   CUSTOMIZADOR
════════════════════════════════════ */
const DESIGNS = [
    { name: 'Sol Pampeano', colors: ['#c9a84c', '#f5e090', '#1a1a1a'] },
    { name: 'Océano Profundo', colors: ['#1a3a6a', '#4394e0', '#071626'] },
    { name: 'Naturaleza', colors: ['#1a4a25', '#2ec27e', '#0a1a0d'] },
    { name: 'Fuego Lento', colors: ['#6a1a1a', '#e05050', '#1a0a0a'] },
    { name: 'Neblina', colors: ['#2a2a3a', '#8090b8', '#161620'] },
    { name: 'Tierra', colors: ['#3a2a1a', '#8a6a40', '#1a1208'] },
    { name: 'Aurora', colors: ['#1a1a3a', '#5040c8', '#c060e0'] },
    { name: 'Surf Blanco', colors: ['#e0e0e0', '#ffffff', '#c8c8c8'] },
    { name: 'Noche', colors: ['#0a0a0a', '#1a1a1a', '#2a2a2a'] },
    { name: 'Mar de Noche', colors: ['#0a1a2a', '#0d3a5a', '#1a5a8a'] },
    { name: 'Dorado Viejo', colors: ['#2a1a0a', '#8a6030', '#e0a050'] },
    { name: 'Coral', colors: ['#2a0a10', '#c03040', '#f06070'] },
    { name: 'Pradera', colors: ['#1a2a0a', '#4a8a20', '#80c040'] },
    { name: 'Gris Urbano', colors: ['#1a1a1a', '#3a3a3a', '#6a6a6a'] },
    { name: 'Cobalto', colors: ['#0a1828', '#1a3a6a', '#4060c0'] },
];

const COLOR_PALETTE = [
    '#c9a84c', '#4394e0', '#2ec27e', '#e63946', '#9060c8',
    '#ff8c00', '#00b4d8', '#f8f9fa', '#1a1a1a', '#e07060',
    '#60c0a0', '#a0a0a0',
];

let currentState = {
    model: 'gaucho',
    design: 0,
    color: '#c9a84c',
    tail: 'squash',
    fin: 'thruster',
};

// Build design grid
const designGrid = document.getElementById('designGrid');
DESIGNS.forEach((d, i) => {
    const sw = document.createElement('div');
    sw.className = 'design-swatch' + (i === 0 ? ' active' : '');
    sw.dataset.name = d.name;
    sw.dataset.index = i;
    sw.title = d.name;
    sw.style.background = `linear-gradient(135deg, ${d.colors[2]}, ${d.colors[0]}, ${d.colors[1]})`;
    sw.onclick = function () {
        document.querySelectorAll('.design-swatch').forEach(s => s.classList.remove('active'));
        this.classList.add('active');
        currentState.design = i;
        document.getElementById('previewDesign').textContent = d.name;
        renderBoard();
    };
    designGrid.appendChild(sw);
});

// Build color row
const colorRow = document.getElementById('colorRow');
COLOR_PALETTE.forEach(col => {
    const dot = document.createElement('div');
    dot.className = 'color-dot' + (col === '#c9a84c' ? ' active' : '');
    dot.style.background = col;
    dot.title = col;
    dot.onclick = function () {
        document.querySelectorAll('.color-dot').forEach(d => d.classList.remove('active'));
        this.classList.add('active');
        currentState.color = col;
        renderBoard();
    };
    colorRow.appendChild(dot);
});

function selectModel(btn) {
    document.querySelectorAll('#modelSelector .toggle-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    currentState.model = btn.dataset.model;
    updatePreviewFromModel(btn.dataset.model);
}

function updatePreviewFromModel(model) {
    const b = BOARDS[model];
    document.getElementById('previewModel').textContent = b.name;
    document.getElementById('previewSize').textContent = b.spec;
    document.getElementById('previewVol').textContent = b.vol;
    document.getElementById('previewPrice').textContent = `USD ${b.price + 80}`;
    currentState.model = model;
    renderBoard();
}

function selectTail(btn) {
    document.querySelectorAll('#tailSelector .toggle-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    currentState.tail = btn.dataset.tail;
    renderBoard();
}

function selectFin(el, fin) {
    document.querySelectorAll('#finGrid .fin-option').forEach(f => f.classList.remove('active'));
    el.classList.add('active');
    currentState.fin = fin;
    document.getElementById('previewFin').textContent = fin.charAt(0).toUpperCase() + fin.slice(1);
    renderBoard();
}

function selectBoard(id) {
    const btn = document.querySelector(`#modelSelector [data-model="${id}"]`);
    if (btn) {
        document.querySelectorAll('#modelSelector .toggle-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        updatePreviewFromModel(id);
    }
    const customizer = document.getElementById('customizador');
    if (customizer) customizer.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

/* ── CANVAS BOARD RENDERER ── */
function renderBoard() {
    const canvas = document.getElementById('boardCanvas');
    const ctx = canvas.getContext('2d');
    const w = canvas.width, h = canvas.height;
    ctx.clearRect(0, 0, w, h);

    const model = currentState.model;
    const design = DESIGNS[currentState.design];
    const color = currentState.color;
    const tail = currentState.tail;

    // Board shape dimensions
    const cx = w / 2;
    const topY = 30, botY = h - 30;
    const midH = (topY + botY) / 2;

    // Widths per model
    const widths = { gaucho: 48, pampeano: 58, charrua: 65, playero: 70 };
    const maxW = widths[model] || 50;

    // Nose width
    const noseW = model === 'playero' ? maxW * 0.55 : model === 'charrua' ? maxW * 0.6 : maxW * 0.35;

    // Tail path per tail type
    function tailPoints() {
        switch (tail) {
            case 'squash': return [[cx - maxW * 0.42, botY - 8], [cx + maxW * 0.42, botY - 8], [cx + maxW * 0.3, botY], [cx - maxW * 0.3, botY]];
            case 'round': return [[cx - maxW * 0.42, botY - 12], [cx + maxW * 0.42, botY - 12]]; // will use arc
            case 'swallow': return [[cx - maxW * 0.4, botY - 8], [cx + maxW * 0.4, botY - 8], [cx + maxW * 0.25, botY], [cx, botY - 14], [cx - maxW * 0.25, botY]];
            case 'pin': return [[cx - maxW * 0.3, botY - 16], [cx + maxW * 0.3, botY - 16], [cx, botY]];
            case 'bat': return [[cx - maxW * 0.45, botY - 6], [cx + maxW * 0.45, botY - 6], [cx + maxW * 0.35, botY + 2], [cx, botY - 8], [cx - maxW * 0.35, botY + 2]];
            default: return [[cx - maxW * 0.42, botY - 8], [cx + maxW * 0.42, botY - 8], [cx, botY]];
        }
    }

    // Draw board outline
    ctx.save();
    ctx.beginPath();

    // Nose
    ctx.moveTo(cx, topY);

    // Right rail
    ctx.bezierCurveTo(
        cx + noseW, topY + (botY - topY) * 0.25,
        cx + maxW, midH - (botY - topY) * 0.05,
        cx + maxW * 0.45, midH + (botY - topY) * 0.25
    );

    // Tail right side
    const tp = tailPoints();
    if (tail === 'round') {
        ctx.lineTo(cx + maxW * 0.42, botY - 12);
        ctx.arcTo(cx, botY + 5, cx - maxW * 0.42, botY - 12, maxW * 0.45);
        ctx.lineTo(cx - maxW * 0.42, botY - 12);
    } else if (tail === 'swallow') {
        ctx.lineTo(cx + maxW * 0.4, botY - 8);
        ctx.lineTo(cx + maxW * 0.25, botY);
        ctx.lineTo(cx, botY - 14);
        ctx.lineTo(cx - maxW * 0.25, botY);
    } else if (tail === 'bat') {
        ctx.lineTo(cx + maxW * 0.45, botY - 6);
        ctx.lineTo(cx + maxW * 0.35, botY + 2);
        ctx.lineTo(cx, botY - 8);
        ctx.lineTo(cx - maxW * 0.35, botY + 2);
    } else if (tail === 'pin') {
        ctx.lineTo(cx + maxW * 0.3, botY - 16);
        ctx.lineTo(cx, botY);
    } else {
        // squash
        ctx.lineTo(cx + maxW * 0.42, botY - 8);
        ctx.lineTo(cx + maxW * 0.3, botY);
        ctx.lineTo(cx - maxW * 0.3, botY);
    }

    // Left rail
    if (tail !== 'swallow' && tail !== 'bat' && tail !== 'pin' && tail !== 'round') {
        ctx.lineTo(cx - maxW * 0.42, botY - 8);
    }
    ctx.bezierCurveTo(
        cx - maxW * 0.45, midH + (botY - topY) * 0.25,
        cx - maxW, midH - (botY - topY) * 0.05,
        cx - noseW, topY + (botY - topY) * 0.25
    );
    ctx.closePath();

    // Fill with design gradient
    const grd = ctx.createLinearGradient(0, topY, w, botY);
    grd.addColorStop(0, design.colors[2]);
    grd.addColorStop(0.5, color);
    grd.addColorStop(1, design.colors[2]);
    ctx.fillStyle = grd;
    ctx.fill();

    // Outline
    ctx.strokeStyle = 'rgba(255,255,255,0.15)';
    ctx.lineWidth = 1;
    ctx.stroke();
    ctx.restore();

    // Stringer
    ctx.save();
    ctx.beginPath();
    ctx.moveTo(cx, topY + 4);
    ctx.lineTo(cx, botY - 10);
    ctx.strokeStyle = 'rgba(255,255,255,0.18)';
    ctx.lineWidth = 1;
    ctx.stroke();
    ctx.restore();

    // Concave reflection
    ctx.save();
    ctx.beginPath();
    ctx.ellipse(cx, midH, maxW * 0.25, (botY - topY) * 0.2, 0, 0, Math.PI * 2);
    const grd2 = ctx.createRadialGradient(cx, midH, 0, cx, midH, maxW * 0.25);
    grd2.addColorStop(0, 'rgba(255,255,255,0.09)');
    grd2.addColorStop(1, 'transparent');
    ctx.fillStyle = grd2;
    ctx.fill();
    ctx.restore();

    // Logo text
    ctx.save();
    ctx.font = 'bold 10px "Bebas Neue", sans-serif';
    ctx.letterSpacing = '3px';
    ctx.textAlign = 'center';
    ctx.fillStyle = 'rgba(255,255,255,0.5)';
    ctx.fillText('MASTER', cx, topY + (botY - topY) * 0.3);
    ctx.restore();

    // Fins
    drawFins(ctx, cx, botY, maxW, currentState.fin, color);
}

function drawFins(ctx, cx, botY, maxW, setup, color) {
    ctx.save();
    ctx.fillStyle = 'rgba(255,255,255,0.3)';
    const fh = 22, fw = 10;
    function fin(x, y, flip) {
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x + (flip ? -fw : fw) * 0.6, y - fh);
        ctx.lineTo(x + (flip ? fw : -fw) * 0.3, y);
        ctx.closePath();
        ctx.fill();
    }
    switch (setup) {
        case 'thruster':
            fin(cx - maxW * 0.28, botY - 6, false);
            fin(cx + maxW * 0.28, botY - 6, true);
            fin(cx, botY - 8, false);
            break;
        case 'twin':
            fin(cx - maxW * 0.32, botY - 8, false);
            fin(cx + maxW * 0.32, botY - 8, true);
            break;
        case '2+1':
            fin(cx - maxW * 0.3, botY - 8, false);
            fin(cx + maxW * 0.3, botY - 8, true);
            ctx.beginPath();
            ctx.moveTo(cx, botY - 12);
            ctx.lineTo(cx + 6, botY - 12 - fh * 1.2);
            ctx.lineTo(cx - 6, botY - 12);
            ctx.closePath();
            ctx.fill();
            break;
        case 'quad':
            fin(cx - maxW * 0.3, botY - 8, false);
            fin(cx + maxW * 0.3, botY - 8, true);
            fin(cx - maxW * 0.2, botY - 4, false);
            fin(cx + maxW * 0.2, botY - 4, true);
            break;
        case 'single':
            ctx.beginPath();
            ctx.moveTo(cx, botY - 8);
            ctx.lineTo(cx + 8, botY - 8 - fh * 1.4);
            ctx.lineTo(cx - 8, botY - 8);
            ctx.closePath();
            ctx.fill();
            break;
        case 'five':
            fin(cx - maxW * 0.28, botY - 6, false);
            fin(cx + maxW * 0.28, botY - 6, true);
            fin(cx, botY - 8, false);
            fin(cx - maxW * 0.18, botY - 3, false);
            fin(cx + maxW * 0.18, botY - 3, true);
            break;
    }
    ctx.restore();
}

// Initial render
renderBoard();

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
