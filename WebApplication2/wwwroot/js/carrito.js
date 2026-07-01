/* ── BOARD SVG helper ── */
const boardSVG = `
<div style="width:42px;height:90px;background:transparent;display:flex;align-items:center;justify-content:center;overflow:visible;flex-shrink:0;">
    <img src="/IMG/tabla.png?v=2" alt="tabla" style="height:82px;width:auto;object-fit:contain;display:block;" />
</div>`;

/* ── CATALOG (productos de referencia) ── */
const CATALOG = [
    { id: 1, name: 'El Cuchillo', maker: 'Marcos Villalba', country: '🇦🇷 Argentina', level: 'Intermedio — Shortboard 6\'2"', price: 680 },
    { id: 2, name: 'Onda Certa', maker: 'Felipe Nunes', country: '🇧🇷 Brasil', level: 'Principiante — Funboard 7\'4"', price: 540 },
    { id: 3, name: 'La Garufa', maker: 'Diego Rocca', country: '🇺🇾 Uruguay', level: 'Avanzado — Longboard 9\'0"', price: 820 },
    { id: 4, name: 'Pampa Glider', maker: 'Pablo Sosa', country: '🇦🇷 Argentina', level: 'Avanzado — Longboard 9\'6"', price: 950 },
    { id: 5, name: 'Mar Bravo', maker: 'Rui Ferreira', country: '🇧🇷 Brasil', level: 'Experto — Gun 7\'8"', price: 710 },
];

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

/* ── TOAST ── */
function showToast(msg, type = 'success') {
    const tc = document.getElementById('toastContainer');
    const t = document.createElement('div');
    t.className = `toast ${type}`;
    t.innerHTML = `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
        ${type === 'success'
            ? '<polyline points="20 6 9 17 4 12"/>'
            : '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12" y2="16"/>'}
      </svg>${msg}`;
    tc.appendChild(t);
    setTimeout(() => t.remove(), 3200);
}

/* ── ADD TO CART (global, called from index.html) ── */
window.addToCart = function (productId) {
    const prod = CATALOG.find(p => p.id === productId);
    if (!prod) return;
    const cart = getCart();
    const existing = cart.find(i => i.id === productId);
    if (existing) { existing.qty += 1; }
    else { cart.push({ ...prod, qty: 1 }); }
    saveCart(cart);
    showToast(`"${prod.name}" agregada al carrito`);
    renderCart();
};

/* ── REMOVE ── */
function removeItem(id) {
    const cart = getCart().filter(i => i.id !== id);
    saveCart(cart);
    renderCart();
    showToast('Tabla removida del carrito', 'info');
}

/* ── CHANGE QTY ── */
function changeQty(id, delta) {
    const cart = getCart();
    const item = cart.find(i => i.id === id);
    if (!item) return;
    item.qty = Math.max(1, item.qty + delta);
    saveCart(cart);
    renderCart();
}

/* ── RENDER ── */
function renderCart() {
    const cart = getCart();
    const wrapper = document.getElementById('cartWrapper');
    updateBadge();

    if (cart.length === 0) {
        wrapper.innerHTML = `
          <div class="cart-empty">
            <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="#0D1B3E" stroke-width="1.2">
              <circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/>
              <path d="M1 1h4l2.68 13.39a2 2 0 001.99 1.61H19a2 2 0 001.99-1.79L22 7H6"/>
            </svg>
            <h2>Tu carrito está vacío</h2>
            <p>Explorá nuestra selección de tablas de autor y encontrá la ideal para tu ola.</p>
            <a href="master.html#tablas" class="btn-primary-custom" style="display:inline-block;background:var(--accent);color:#fff;padding:.85rem 2rem;border-radius:2px;text-decoration:none;font-size:.88rem;letter-spacing:.06em;text-transform:uppercase;font-weight:500;">Ver tablas</a>
          </div>
          ${renderRecommended()}
        `;
        return;
    }

    const subtotal = cart.reduce((s, i) => s + i.price * i.qty, 0);
    const shipping = subtotal >= 800 ? 0 : 45;
    const total = subtotal + shipping;

    const itemsHTML = `
        <table style="width:100%;border-collapse:collapse;font-family:'DM Sans',sans-serif;font-size:.84rem;color:#111;">
          <thead>
            <tr>
              <th style="padding:.7rem 1rem;text-align:left;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:#555;border-bottom:1px solid rgba(0,0,0,.12);"></th>
              <th style="padding:.7rem 1rem;text-align:left;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:#555;border-bottom:1px solid rgba(0,0,0,.12);">Tabla</th>
              <th style="padding:.7rem 1rem;text-align:left;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:#555;border-bottom:1px solid rgba(0,0,0,.12);">Shaper / Detalle</th>
              <th style="padding:.7rem 1rem;text-align:center;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:#555;border-bottom:1px solid rgba(0,0,0,.12);">Cantidad</th>
              <th style="padding:.7rem 1rem;text-align:right;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:#555;border-bottom:1px solid rgba(0,0,0,.12);">Precio unit.</th>
              <th style="padding:.7rem 1rem;text-align:right;font-size:.62rem;letter-spacing:.14em;text-transform:uppercase;color:#555;border-bottom:1px solid rgba(0,0,0,.12);">Total</th>
              <th style="padding:.7rem 1rem;border-bottom:1px solid rgba(0,0,0,.12);"></th>
            </tr>
          </thead>
          <tbody>
            ${cart.map((item, idx) => {
        const subtitle = item.details || item.level || (item.custom ? 'Tabla customizada' : 'Tabla estándar');
        const shaperInfo = item.maker ? `${item.maker}${item.country ? ' · ' + item.country : ''}` : (item.country || subtitle);
        const idStr = JSON.stringify(item.id);
        return `
              <tr style="border-bottom:1px solid rgba(0,0,0,.07);${idx % 2 === 1 ? 'background:rgba(0,0,0,.02);' : ''}animation:fadeIn .3s ${idx * 60}ms both;">
                <td style="padding:.85rem 1rem;">${boardSVG}</td>
                <td style="padding:.85rem 1rem;">
                  <p style="font-weight:500;margin:0 0 .2rem;">${item.name}</p>
                  <p style="font-size:.72rem;color:#777;margin:0;">${subtitle}</p>
                </td>
                <td style="padding:.85rem 1rem;color:#666;font-size:.78rem;">${shaperInfo}</td>
                <td style="padding:.85rem 1rem;text-align:center;">
                  <div style="display:inline-flex;align-items:center;gap:.4rem;background:rgba(0,0,0,.05);border-radius:3px;padding:.15rem .3rem;">
                    <button class="qty-btn" onclick="changeQty(${idStr}, -1)" style="background:none;border:none;color:#333;font-size:1rem;cursor:pointer;padding:0 .2rem;line-height:1;">−</button>
                    <span style="font-weight:500;min-width:1.2rem;text-align:center;">${item.qty}</span>
                    <button class="qty-btn" onclick="changeQty(${idStr}, 1)" style="background:none;border:none;color:#333;font-size:1rem;cursor:pointer;padding:0 .2rem;line-height:1;">+</button>
                  </div>
                </td>
                <td style="padding:.85rem 1rem;text-align:right;color:#666;font-size:.78rem;">USD ${item.price.toLocaleString()}</td>
                <td style="padding:.85rem 1rem;text-align:right;font-family:'Bebas Neue',sans-serif;font-size:1.1rem;letter-spacing:.04em;color:#111;">USD ${(item.price * item.qty).toLocaleString()}</td>
                <td style="padding:.85rem 1rem;text-align:center;">
                  <button class="btn-remove" onclick="removeItem(${idStr})" style="background:none;border:1px solid rgba(0,0,0,.18);color:#888;font-size:.68rem;letter-spacing:.06em;text-transform:uppercase;padding:.3rem .55rem;border-radius:2px;cursor:pointer;white-space:nowrap;transition:all .2s;" onmouseover="this.style.borderColor='rgba(255,80,80,.5)';this.style.color='rgba(255,100,100,.8)'" onmouseout="this.style.borderColor='rgba(0,0,0,.18)';this.style.color='#888'">
                    <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="vertical-align:middle;margin-right:.2rem;"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/></svg>
                    Quitar
                  </button>
                </td>
              </tr>`;
    }).join('')}
          </tbody>
        </table>
      `;

    const shippingLabel = shipping === 0
        ? '<span style="color:#2ec27e;font-weight:500">Envío gratuito 🎉</span>'
        : `USD ${shipping}`;

    wrapper.innerHTML = `
        <!-- LEFT: Items -->
        <div>
          <p class="cart-section-title">${cart.length} tabla${cart.length > 1 ? 's' : ''} en tu carrito</p>
          <div style="overflow-x:auto;-webkit-overflow-scrolling:touch;">${itemsHTML}</div>
          ${renderRecommended()}
        </div>

        <!-- RIGHT: Summary -->
        <div class="cart-summary">
          <p class="summary-title">Resumen</p>

          ${cart.map(i => `
            <div class="summary-line">
              <span>${i.name} × ${i.qty}</span>
              <span>USD ${(i.price * i.qty).toLocaleString()}</span>
            </div>
          `).join('')}

          <div class="summary-line">
            <span>Envío estimado</span>
            <span>${shippingLabel}</span>
          </div>

          ${shipping > 0 ? `
            <div class="summary-shipping">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="1" y="3" width="15" height="13"/><polygon points="16 8 20 8 23 11 23 16 16 16 16 8"/>
                <circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/>
              </svg>
              Superá USD 800 para envío gratis
            </div>
          ` : ''}

          <div class="summary-line total">
            <span>Total</span>
            <span>USD ${total.toLocaleString()}</span>
          </div>

          <div class="coupon-row">
            <input class="coupon-input" type="text" placeholder="Código de descuento" id="couponInput" />
            <button class="coupon-btn" onclick="applyCoupon()">Aplicar</button>
          </div>

          <button class="btn-checkout" onclick="openOrderForm()" id="btnProceed">
            Finalizar pedido →
          </button>
          <a href="master.html#tablas" class="btn-continue">Seguir eligiendo</a>

          <div class="trust-badges">
            <div class="badge-item">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
              </svg>
              Pago seguro
            </div>
            <div class="badge-item">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path d="M21 16V8a2 2 0 00-1-1.73l-7-4a2 2 0 00-2 0l-7 4A2 2 0 003 8v8a2 2 0 001 1.73l7 4a2 2 0 002 0l7-4A2 2 0 0021 16z"/>
              </svg>
              100% artesanal
            </div>
            <div class="badge-item">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/>
                <polyline points="17 6 23 6 23 12"/>
              </svg>
              Garantía shaper
            </div>
          </div>
        </div>
      `;
}

/* ── RECOMMENDED ── */
function renderRecommended() {
    const cart = getCart();
    const cartIds = cart.map(i => i.id);
    const recs = CATALOG.filter(p => !cartIds.includes(p.id)).slice(0, 3);
    if (recs.length === 0) return '';

    return `
        <div class="cart-recommended" style="margin-top:2.5rem;">
          <p class="cart-section-title">También te puede interesar</p>
          <div class="rec-grid">
            ${recs.map(p => `
              <div class="rec-card">
                <div class="rec-img">${boardSVG}</div>
                <div class="rec-info">
                  <p class="rec-maker">Shaper: ${p.maker}</p>
                  <p class="rec-name">${p.name}</p>
                  <p class="rec-price">USD ${p.price.toLocaleString()}</p>
                  <button class="btn-add-rec" onclick="addToCart(${p.id})">+ Agregar al carrito</button>
                </div>
              </div>
            `).join('')}
          </div>
        </div>
      `;
}

/* ── COUPON ── */
function applyCoupon() {
    const val = document.getElementById('couponInput')?.value.trim().toUpperCase();
    if (!val) return;
    showToast(val === 'SURF10' ? '¡Cupón aplicado! 10% off' : 'Código inválido o expirado', val === 'SURF10' ? 'success' : 'info');
}

/* ── ORDER FORM MODAL ── */
function openOrderForm() {
    const cart = getCart();
    if (!cart.length) { showToast('Agregá al menos una tabla para continuar', 'info'); return; }
    document.getElementById('orderModal').classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeOrderForm() {
    document.getElementById('orderModal').classList.remove('open');
    document.body.style.overflow = '';
}

// Close modal on overlay click
document.getElementById('orderModal').addEventListener('click', function (e) {
    if (e.target === this) closeOrderForm();
});

function submitOrder() {
    const fname = document.getElementById('fname')?.value.trim();
    const lname = document.getElementById('lname')?.value.trim();
    const femail = document.getElementById('femail')?.value.trim();
    const fphone = document.getElementById('fphone')?.value.trim();
    if (!fname || !lname || !femail || !fphone) {
        showToast('Por favor completá tus datos de contacto', 'info');
        return;
    }
    closeOrderForm();
    showToast('Pedido enviado — Francisco te contacta en 24hs', 'success');
    // Could clear cart here: saveCart([]); renderCart();
}

/* ── INIT ── */
updateBadge();
renderCart();

/* Expose for use on index pages */
window.ZSG = { addToCart, getCart, saveCart, updateBadge, showToast };

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