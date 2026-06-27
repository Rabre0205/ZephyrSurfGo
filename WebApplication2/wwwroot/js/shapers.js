/* ── NAVBAR SCROLL ── */
const navbar = document.getElementById('navbar');
window.addEventListener('scroll', () => navbar.classList.toggle('scrolled', window.scrollY > 60));

/* ── REVEAL ── */
const obs = new IntersectionObserver((entries) => {
    entries.forEach((e, i) => {
        if (e.isIntersecting) {
            setTimeout(() => e.target.classList.add('visible'), i * 90);
            obs.unobserve(e.target);
        }
    });
}, { threshold: 0.1 });
document.querySelectorAll('.reveal').forEach(el => obs.observe(el));


const dropdown = document.querySelector(".util-dropdown");
const toggle = document.querySelector(".dropdown-toggle");

console.log(dropdown);
console.log(toggle);

toggle.addEventListener("click", function (e) {
    e.preventDefault();
    e.stopPropagation();

    console.log("CLICK EN MARCAS");

    dropdown.classList.toggle("active");
});
document.addEventListener("click", function (e) {
    if (!dropdown.contains(e.target)) {
        dropdown.classList.remove("active");
    }
});

/* ════════════════════════════════════
   CARRITO
════════════════════════════════════ */

/* ── CART STATE (localStorage) ── */
function getCart() {
    try { return JSON.parse(localStorage.getItem('master_cart') || '[]'); }
    catch { return []; }
}
function saveCart(cart) {
    localStorage.setItem('master_cart', JSON.stringify(cart));
    updateBadge();
}
function updateBadge() {
    const cart = getCart();
    const total = cart.reduce((s, i) => s + i.qty, 0);
    document.querySelectorAll('.cart-badge, #navBadge').forEach(el => {
        el.textContent = total;
        el.style.display = total > 0 ? 'flex' : 'none';
    });
}


function addBoard(id) {
    const prod = CATALOG_SHP.find(p => p.id === id);

    if (!prod) {
        console.error('Producto no encontrado:', id);
        return;
    }

    const cart = getCart();

    const existing = cart.find(item => item.id === id);

    if (existing) {
        existing.qty += 1;
    } else {
        cart.push({
            ...prod,
            qty: 1
        });
    }

    saveCart(cart);

    console.log('Carrito actualizado:', cart);

    showToast(`"${prod.name}" agregada al carrito`);
}

/* Inicializar badge al cargar */
document.addEventListener('DOMContentLoaded', () => {
    updateBadge();
});

/* ── FILTER CHIPS ── */
const chips = document.querySelectorAll('.chip');
let activeFilter = 'all';
let searchVal = '';

chips.forEach(chip => {
    chip.addEventListener('click', () => {
        chips.forEach(c => c.classList.remove('active'));
        chip.classList.add('active');
        activeFilter = chip.dataset.filter;
        applyFilters();
    });
});

document.getElementById('searchInput').addEventListener('input', e => {
    searchVal = e.target.value.toLowerCase();
    applyFilters();
});

function applyFilters() {
    const cards = document.querySelectorAll('[data-name]');
    let visible = 0;
    cards.forEach(card => {
        const country = card.dataset.country || '';
        const styles = card.dataset.styles || '';
        const name = card.dataset.name || '';

        const matchFilter = activeFilter === 'all'
            || country === activeFilter
            || styles.includes(activeFilter);

        const matchSearch = !searchVal || name.includes(searchVal);

        const show = matchFilter && matchSearch;
        card.dataset.hidden = show ? 'false' : 'true';
        if (show) visible++;
    });
    document.getElementById('emptyState').style.display = visible === 0 ? 'block' : 'none';
}

/* ── MODAL DATA ── */
const SHAPERS = {
    rocca: {
        name: 'Diego Rocca',
        origin: '🇺🇾 Punta del Este, Uruguay',
        years: '28 años shapando',
        specialty: 'Longboard / Mid-length',
        country: 'UY',
        photo: '../IMG/tabla.png',
        bio: `Diego Rocca comenzó a shapear en 1996 luego de pasar una temporada en Byron Bay, Australia, donde se empapó de la cultura del longboard clásico. De regreso en Uruguay, fundó su taller en Punta del Este y nunca miró atrás. Sus tablas son reconocidas en toda la región por el equilibrio perfecto entre peso, rocker y concavidad. Trabaja con foam importado y resina epoxi de primera calidad, pero su mayor secreto es el tiempo: nunca apura un trabajo.`,
        stats: [
            { num: '340+', label: 'Tablas crafteadas' },
            { num: '28', label: 'Años de experiencia' },
            { num: '4', label: 'Modelos de catálogo' },
            { num: '6', label: 'Países destino' },
        ],
        styles: ['Longboard', 'Mid-length', 'Shortboard', 'Custom'],
        boards: [
            { name: 'La Garufa', spec: 'Longboard 9\'0" · Avanzado', price: 'USD 820', id: 3 },
            { name: 'El Pastel', spec: 'Mid-length 7\'4" · Intermedio', price: 'USD 690', id: null },
            { name: 'Punta Sur', spec: 'Noserider 9\'6" · Avanzado', price: 'USD 900', id: null },
            { name: 'La Rambla', spec: 'Classic 8\'2" · Intermedio', price: 'USD 760', id: null },
        ],
        color: '#063f4a',
        accent: '#0a7a8a',
    },
    villalba: {
        name: 'Marcos Villalba',
        origin: '🇦🇷 Mar del Plata, Argentina',
        years: '17 años shapando',
        specialty: 'Shortboard / Performance',
        country: 'AR',
        photo: '../IMG/tabla.png',
        bio: `Marcos nació y creció en Mar del Plata, rodeado de las olas frías y potentes del Atlántico Sur. Aprendió a shapear de manera autodidacta a los 22 años, y desde entonces ha perfeccionado su técnica estudiando los diseños de los grandes shapers australianos. Sus shortboards son notorios por sus raíles afilados y el rocker pronunciado que permite maniobras de alta performance en olas huecas.`,
        stats: [
            { num: '180+', label: 'Tablas crafteadas' },
            { num: '17', label: 'Años de experiencia' },
            { num: '3', label: 'Modelos de catálogo' },
        ],
        styles: ['Shortboard', 'Fish', 'Step-up', 'Thruster'],
        boards: [
            { name: 'El Cuchillo', spec: 'Shortboard 6\'2" · Intermedio', price: 'USD 680', id: 1 },
            { name: 'Pampero', spec: 'Step-up 6\'8" · Avanzado', price: 'USD 750', id: null },
            { name: 'La Navaja', spec: 'Fish 5\'10" · Intermedio', price: 'USD 620', id: null },
        ],
        color: '#0d2d45',
        accent: '#1a4060',
    },
    nunes: {
        name: 'Felipe Nunes',
        origin: '🇧🇷 Florianópolis, Brasil',
        years: '12 años shapando',
        specialty: 'Funboard / Mini-mal',
        country: 'BR',
        photo: '../IMG/tabla.png',
        bio: `Felipe creció surfiendo las aguas turquesa de Florianópolis, isla conocida como "la isla de la magia". Su pasión por el surf lo llevó a estudiar diseño de tablas en Sídney a los 21 años. De regreso en Brasil, abrió su taller en Praia Mole y empezó a shapear tablas accesibles sin comprometer la calidad. Hoy sus funboards son el primer paso de cientos de surfers brasileños.`,
        stats: [
            { num: '110+', label: 'Tablas crafteadas' },
            { num: '12', label: 'Años de experiencia' },
            { num: '2', label: 'Modelos de catálogo' },
        ],
        styles: ['Funboard', 'Mini-mal', 'Longboard', 'Egg'],
        boards: [
            { name: 'Onda Certa', spec: 'Funboard 7\'4" · Principiante', price: 'USD 540', id: 2 },
            { name: 'Ilha Bela', spec: 'Mini-mal 8\'0" · Principiante', price: 'USD 580', id: null },
        ],
        color: '#1a3a20',
        accent: '#2a5030',
    },
    sosa: {
        name: 'Pablo Sosa',
        origin: '🇦🇷 Miramar, Argentina',
        years: '22 años shapando',
        specialty: 'Longboard Clásico',
        country: 'AR',
        photo: '../IMG/tabla.png',
        bio: `Pablo es el guardián del longboard clásico en Argentina. Su taller en Miramar, a pocos metros del mar, es un museo viviente donde conviven plantillas del siglo pasado con herramientas modernas. Aprendió el oficio de su padre, quien importó la cultura del surf desde California en los años 80. Sus noseriders, hechos con maderas nativas como el cedro misionero, son piezas únicas de colección.`,
        stats: [
            { num: '250+', label: 'Tablas crafteadas' },
            { num: '22', label: 'Años de experiencia' },
            { num: '5', label: 'Modelos de catálogo' },
        ],
        styles: ['Longboard', 'Noserider', 'Classic', 'Single-fin'],
        boards: [
            { name: 'Pampa Glider', spec: 'Longboard 9\'6" · Avanzado', price: 'USD 950', id: 4 },
            { name: 'El Cedro', spec: 'Noserider 9\'0" · Avanzado', price: 'USD 1100', id: null },
            { name: 'La Atlántica', spec: 'Classic 8\'6" · Intermedio', price: 'USD 820', id: null },
        ],
        color: '#2a1a38',
        accent: '#3d2652',
    },
    ferreira: {
        name: 'Rui Ferreira',
        origin: '🇧🇷 Río de Janeiro, Brasil',
        years: '19 años shapando',
        specialty: 'Gun / Big Wave',
        country: 'BR',
        photo: '../IMG/tabla.png',
        bio: `Ex competidor del circuito sudamericano, Rui se retiró de la competencia para dedicarse por completo a shapear las tablas que alguna vez soñó tener. Su experiencia en olas grandes en Itacoatiara y Saquarema lo convierte en el referente indiscutido del big wave en Brasil. Sus guns son herramientas serias diseñadas para dominar el océano en su estado más poderoso.`,
        stats: [
            { num: '130+', label: 'Tablas crafteadas' },
            { num: '19', label: 'Años de experiencia' },
            { num: '2', label: 'Modelos de catálogo' },
        ],
        styles: ['Gun', 'Big Wave', 'Shortboard', 'Semi-gun'],
        boards: [
            { name: 'Mar Bravo', spec: 'Gun 7\'8" · Experto', price: 'USD 710', id: 5 },
            { name: 'O Touro', spec: 'Big Wave 8\'6" · Experto', price: 'USD 890', id: null },
        ],
        color: '#1a2a3a',
        accent: '#253d55',
    },
    suarez: {
        name: 'Valentina Suárez',
        origin: '🇺🇾 Montevideo, Uruguay',
        years: '9 años shapando',
        specialty: 'Fish / Twin-fin',
        country: 'UY',
        photo: '../IMG/tabla.png',
        bio: `Valentina rompió todos los moldes siendo la primera mujer shaper reconocida de Uruguay. Formada en la escuela de shaping de San Sebastián, España, trajo de vuelta una visión fresca que fusiona la estética del surf vasco con las necesidades de las olas rioplatenses. Sus fish twins con canales laterales son el secreto mejor guardado entre los surfers avanzados de Punta del Este.`,
        stats: [
            { num: '80+', label: 'Tablas crafteadas' },
            { num: '9', label: 'Años de experiencia' },
            { num: '3', label: 'Modelos de catálogo' },
        ],
        styles: ['Fish', 'Twin-fin', 'Egg', 'Retro'],
        boards: [
            { name: 'La Pesca', spec: 'Fish 5\'8" · Intermedio', price: 'USD 590', id: null },
            { name: 'Twin Soul', spec: 'Twin-fin 6\'0" · Avanzado', price: 'USD 640', id: null },
            { name: 'El Huevo', spec: 'Egg 7\'0" · Intermedio', price: 'USD 610', id: null },
        ],
        color: '#1a2030',
        accent: '#283050',
    },
};

/* ── CART logic ── */
const CATALOG_SHP = [
    { id: 1, name: 'El Cuchillo', maker: 'Marcos Villalba', country: '🇦🇷 Argentina', level: 'Intermedio — Shortboard 6\'2"', price: 680 },
    { id: 2, name: 'Onda Certa', maker: 'Felipe Nunes', country: '🇧🇷 Brasil', level: 'Principiante — Funboard 7\'4"', price: 540 },
    { id: 3, name: 'La Garufa', maker: 'Diego Rocca', country: '🇺🇾 Uruguay', level: 'Avanzado — Longboard 9\'0"', price: 820 },
    { id: 4, name: 'Pampa Glider', maker: 'Pablo Sosa', country: '🇦🇷 Argentina', level: 'Avanzado — Longboard 9\'6"', price: 950 },
    { id: 5, name: 'Mar Bravo', maker: 'Rui Ferreira', country: '🇧🇷 Brasil', level: 'Experto — Gun 7\'8"', price: 710 },
];


function addBoard(id) {
    if (!id) return;
    const prod = CATALOG_SHP.find(p => p.id === id);
    if (!prod) return;
    const cart = getCart();
    const ex = cart.find(i => i.id === id);
    if (ex) ex.qty += 1; else cart.push({ ...prod, qty: 1 });
    saveCart(cart);
    showToast(`"${prod.name}" agregada al carrito`);
}

function showToast(msg) {
    const tc = document.getElementById('toastContainer');
    const t = document.createElement('div');
    t.className = 'zsg-toast';
    t.innerHTML = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#2ec27e" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>${msg}`;
    tc.appendChild(t);
    setTimeout(() => t.remove(), 3200);
}

/* ── MODAL ── */
let currentModal = null;

function openModal(key) {
    const s = SHAPERS[key];
    if (!s) return;
    currentModal = key;

    const boardsHTML = `
        <table style="width:100%;border-collapse:collapse;font-family:'DM Sans',sans-serif;font-size:.82rem;color:rgba(255,255,255,.75);">
          <thead>
            <tr>
              <th style="padding:.6rem .9rem;text-align:left;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:var(--ocean,#4394e0);border-bottom:1px solid rgba(255,255,255,.1);white-space:nowrap;">Modelo</th>
              <th style="padding:.6rem .9rem;text-align:left;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:var(--ocean,#4394e0);border-bottom:1px solid rgba(255,255,255,.1);white-space:nowrap;">Especificación</th>
              <th style="padding:.6rem .9rem;text-align:center;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:var(--ocean,#4394e0);border-bottom:1px solid rgba(255,255,255,.1);white-space:nowrap;">Precio</th>
              <th style="padding:.6rem .9rem;text-align:center;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:var(--ocean,#4394e0);border-bottom:1px solid rgba(255,255,255,.1);"></th>
            </tr>
          </thead>
          <tbody>
            ${s.boards.map((b, idx) => `
              <tr style="border-bottom:1px solid rgba(255,255,255,.06);${idx % 2 === 1 ? 'background:rgba(255,255,255,.02);' : ''}">
                <td style="padding:.7rem .9rem;font-weight:500;color:#fff;white-space:nowrap;">${b.name}</td>
                <td style="padding:.7rem .9rem;color:rgba(255,255,255,.5);white-space:nowrap;">${b.spec}</td>
                <td style="padding:.7rem .9rem;text-align:center;font-family:'Bebas Neue',sans-serif;font-size:1rem;letter-spacing:.05em;color:#fff;white-space:nowrap;">${b.price}</td>
                <td style="padding:.7rem .9rem;text-align:center;">
                  ${b.id ? `<button onclick="addBoard(${b.id})" style="background:var(--ocean,#4394e0);color:#fff;border:none;padding:.35rem .75rem;font-size:.68rem;letter-spacing:.07em;text-transform:uppercase;border-radius:2px;cursor:pointer;transition:background .2s;white-space:nowrap;" onmouseover="this.style.background='var(--wave,#0d7ab5)'" onmouseout="this.style.background='var(--ocean,#4394e0)'">+ Agregar</button>` : '<span style="font-size:.7rem;color:rgba(255,255,255,.25);">Consultar</span>'}
                </td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      `;

    const statsHTML = s.stats.map(st => `
        <div><div class="modal-stat-num">${st.num}</div><div class="modal-stat-label">${st.label}</div></div>
      `).join('');

    const tagsHTML = s.styles.map(t => `<span class="modal-tag">${t}</span>`).join('');

    document.getElementById('modalPanel').innerHTML = `
        <div class="modal-hero" style="background:${s.color}; position:relative; overflow:hidden; display:flex; align-items:center; justify-content:flex-end; padding-right:3rem;">
          ${s.photo ? `
          <div style="position:absolute;right:0;top:0;bottom:0;width:220px;background:#f0f0ee;display:flex;align-items:center;justify-content:center;overflow:hidden;">
            <img src="${s.photo}" alt="tabla de surf" style="height:220px;width:auto;object-fit:contain;filter:drop-shadow(0 8px 20px rgba(0,0,0,.18));transition:transform .4s;" />
          </div>` : ''}
          <svg viewBox="0 0 820 260" xmlns="http://www.w3.org/2000/svg" style="position:relative;z-index:1;">
            <rect width="820" height="260" fill="transparent"/>
            <ellipse cx="660" cy="130" rx="40" ry="140" fill="rgba(67,148,224,.06)" transform="rotate(-15 660 130)"/>
            <path d="M0 230 Q205 210 410 230 Q615 250 820 230" stroke="rgba(255,255,255,.06)" stroke-width="2" fill="none"/>
          </svg>
          <span class="modal-hero-name">${s.name}</span>
          <button class="modal-close" onclick="closeModal()">✕</button>
        </div>
        <div class="modal-body">
          <div class="modal-meta">
            <span class="modal-tag accent">${s.specialty}</span>
            <span class="modal-tag">${s.origin}</span>
            <span class="modal-tag">${s.years}</span>
            ${tagsHTML}
          </div>
          <p class="modal-bio">${s.bio}</p>
          <div class="modal-stats">${statsHTML}</div>
          <p class="modal-boards-title">Tablas disponibles</p>
          <div style="overflow-x:auto;-webkit-overflow-scrolling:touch;margin-bottom:.5rem;">${boardsHTML}</div>
          <div class="modal-cta">
            <a href="index.html#contacto" class="btn-primary-custom" style="font-size:.82rem;padding:.75rem 1.5rem;">Contactar shaper</a>
            <button class="btn-outline-white" onclick="closeModal()" style="border-color:rgba(0,0,0,.15);color:var(--muted);font-size:.82rem;padding:.75rem 1.5rem;background:none;">Cerrar</button>
          </div>
        </div>
      `;

    document.getElementById('modalOverlay').classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeModal(e) {
    if (e && e.target !== document.getElementById('modalOverlay')) return;
    document.getElementById('modalOverlay').classList.remove('open');
    document.body.style.overflow = '';
    currentModal = null;
}

document.addEventListener('keydown', e => { if (e.key === 'Escape') { document.getElementById('modalOverlay').classList.remove('open'); document.body.style.overflow = ''; } });

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

