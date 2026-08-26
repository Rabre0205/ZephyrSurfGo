const shapers = Array.isArray(window.zephyrShapers) ? window.zephyrShapers : [];
const countries = {
    ar: { name: 'Argentina', flag: '🇦🇷', center: [-38.4, -63.6], color: '#74b9ff' },
    br: { name: 'Brasil', flag: '🇧🇷', center: [-14.2, -51.9], color: '#55c995' },
    uy: { name: 'Uruguay', flag: '🇺🇾', center: [-32.8, -56.0], color: '#f0c65a' }
};

const map = L.map('map', { center: [-27, -56], zoom: 4, zoomControl: true });
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '© OpenStreetMap', maxZoom: 18 }).addTo(map);

function markerIcon(country) {
    const color = countries[country]?.color || '#4394e0';
    return L.divIcon({ className: '', iconSize: [32, 42], iconAnchor: [16, 42], popupAnchor: [0, -40], html:
        `<svg width="32" height="42" viewBox="0 0 32 42"><path d="M16 0C7.2 0 0 7.2 0 16c0 11 14.6 25 16 26 1.4-1 16-15 16-26C32 7.2 24.8 0 16 0z" fill="${color}"/><circle cx="16" cy="16" r="7" fill="#0d204b"/></svg>` });
}

const markers = new Map();
const countryOffsets = { ar: 0, br: 0, uy: 0 };
shapers.forEach(shaper => {
    const country = countries[shaper.country] || countries.uy;
    const index = countryOffsets[shaper.country]++;
    const angle = index * 2.4;
    const radius = index === 0 ? 0 : .45 + Math.floor(index / 6) * .25;
    const position = [country.center[0] + Math.sin(angle) * radius, country.center[1] + Math.cos(angle) * radius];
    const marker = L.marker(position, { icon: markerIcon(shaper.country) }).addTo(map);
    marker.bindPopup(`<div class="popup-name">${escapeHtml(shaper.name)}</div><div class="popup-addr">${country.flag} ${country.name}<br>${shaper.products} producto${shaper.products === 1 ? '' : 's'} publicado${shaper.products === 1 ? '' : 's'}</div><a class="popup-link" href="${shaper.pageUrl}">Ver página del shaper →</a>`);
    markers.set(shaper.id, marker);
});

let activeCountry = 'all';
let activeSearch = '';
function escapeHtml(value) { const div = document.createElement('div'); div.textContent = value || ''; return div.innerHTML; }
function visibleShapers() {
    return shapers.filter(s => (activeCountry === 'all' || s.country === activeCountry) && (!activeSearch || `${s.name} ${s.owner} ${s.countryName}`.toLowerCase().includes(activeSearch)));
}
function renderList() {
    const list = document.getElementById('dealersList');
    const visible = visibleShapers();
    document.getElementById('resultCount').textContent = `${visible.length} shaper${visible.length === 1 ? '' : 's'}`;
    if (!visible.length) { list.innerHTML = '<div class="map-empty"><strong>No encontramos shapers</strong><span>Probá con otro país o término de búsqueda.</span></div>'; return; }
    list.innerHTML = visible.map(s => {
        const country = countries[s.country];
        const initial = escapeHtml((s.name || 'S').charAt(0).toUpperCase());
        return `<article class="shaper-map-card"><div class="shaper-map-logo">${s.logo ? `<img src="${escapeHtml(s.logo)}" alt="">` : initial}</div><div class="shaper-map-info"><span>${country.flag} ${country.name}</span><h2>${escapeHtml(s.name)}</h2><p>${escapeHtml(s.owner)} · ${s.products} producto${s.products === 1 ? '' : 's'}</p><div><button type="button" data-focus="${s.id}">Ver en el mapa</button><a href="${s.pageUrl}">Ver página →</a></div></div></article>`;
    }).join('');
    list.querySelectorAll('[data-focus]').forEach(button => button.addEventListener('click', () => focusShaper(Number(button.dataset.focus))));
}
function focusShaper(id) { const marker = markers.get(id); if (!marker) return; map.setView(marker.getLatLng(), 6, { animate: true }); marker.openPopup(); }
function updateMap() {
    const visibleIds = new Set(visibleShapers().map(s => s.id));
    markers.forEach((marker, id) => visibleIds.has(id) ? marker.addTo(map) : marker.removeFrom(map));
    if (activeCountry === 'all') map.setView([-27, -56], 4); else map.setView(countries[activeCountry].center, activeCountry === 'uy' ? 6 : 4);
}
document.querySelectorAll('.filter-btn').forEach(button => button.addEventListener('click', () => {
    document.querySelectorAll('.filter-btn').forEach(item => item.classList.remove('active')); button.classList.add('active'); activeCountry = button.dataset.filter; renderList(); updateMap();
}));
document.getElementById('searchInput').addEventListener('input', event => { activeSearch = event.target.value.toLowerCase().trim(); renderList(); updateMap(); });
renderList(); setTimeout(() => map.invalidateSize(), 250);
