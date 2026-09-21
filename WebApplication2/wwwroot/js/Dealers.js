const pickups = Array.isArray(window.zephyrPickups) ? window.zephyrPickups : [];
const countries = {
    ar: { name: 'Argentina', flag: '🇦🇷', center: [-38.4, -63.6] },
    br: { name: 'Brasil', flag: '🇧🇷', center: [-14.2, -51.9] },
    uy: { name: 'Uruguay', flag: '🇺🇾', center: [-32.8, -56] }
};
const list = document.getElementById('dealersList');
const resultCount = document.getElementById('resultCount');
const status = document.getElementById('locatorStatus');
const searchInput = document.getElementById('searchInput');
const clearButton = document.getElementById('clearFiltersButton');
const nearMeButton = document.getElementById('nearMeButton');
const mapError = document.getElementById('mapError');
let activeCountry = 'all';
let search = '';
let selectedId = null;
let userLocation = null;

function escapeHtml(value) {
    const element = document.createElement('div');
    element.textContent = value || '';
    return element.innerHTML;
}

function normalize(value) {
    return (value || '').toLocaleLowerCase('es').normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

function distanceKm(origin, pickup) {
    const radians = degrees => degrees * Math.PI / 180;
    const earthRadius = 6371;
    const deltaLat = radians(pickup.lat - origin.lat);
    const deltaLng = radians(pickup.lng - origin.lng);
    const value = Math.sin(deltaLat / 2) ** 2 + Math.cos(radians(origin.lat)) * Math.cos(radians(pickup.lat)) * Math.sin(deltaLng / 2) ** 2;
    return earthRadius * 2 * Math.atan2(Math.sqrt(value), Math.sqrt(1 - value));
}

function visiblePickups() {
    const query = normalize(search);
    const filtered = pickups.filter(pickup => {
        const searchable = normalize(`${pickup.name} ${pickup.shaper} ${pickup.address} ${pickup.city} ${pickup.countryName} ${countries[pickup.country]?.name || ''}`);
        return (activeCountry === 'all' || pickup.country === activeCountry) && (!query || searchable.includes(query));
    });
    if (userLocation) filtered.sort((a, b) => distanceKm(userLocation, a) - distanceKm(userLocation, b));
    return filtered;
}

let map = null;
const markers = new Map();
try {
    map = L.map('map', { center: [-27, -56], zoom: 4 });
    const tiles = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '© OpenStreetMap', maxZoom: 18 }).addTo(map);
    tiles.on('tileerror', () => { mapError.hidden = false; });
} catch {
    document.getElementById('map').hidden = true;
    mapError.hidden = false;
}

function markerIcon() {
    return L.divIcon({ className: '', iconSize: [32, 42], iconAnchor: [16, 42], popupAnchor: [0, -40], html: '<svg aria-hidden="true" width="32" height="42" viewBox="0 0 32 42"><path d="M16 0C7.2 0 0 7.2 0 16c0 11 14.6 25 16 26 1.4-1 16-15 16-26C32 7.2 24.8 0 16 0z" fill="#b35a32"/><circle cx="16" cy="16" r="7" fill="#24211b"/></svg>' });
}

if (map) {
    pickups.forEach(pickup => {
        const directions = `https://www.google.com/maps/dir/?api=1&destination=${pickup.lat},${pickup.lng}`;
        const marker = L.marker([pickup.lat, pickup.lng], { icon: markerIcon(), title: pickup.name }).addTo(map);
        marker.bindPopup(`<div class="popup-name">${escapeHtml(pickup.name)}</div><div class="popup-addr">${escapeHtml(pickup.address)} · ${escapeHtml(pickup.city)}<br>${escapeHtml(pickup.schedule || 'Horario a coordinar')}<br>${escapeHtml(pickup.notes || '')}</div><a class="popup-link" target="_blank" rel="noopener" href="${directions}">Cómo llegar →</a>`);
        marker.on('click', () => selectPickup(pickup.id, false));
        markers.set(pickup.id, marker);
    });
}

function selectPickup(id, moveMap = true) {
    selectedId = Number(id);
    document.querySelectorAll('.shaper-map-card').forEach(card => {
        const selected = Number(card.dataset.id) === selectedId;
        card.classList.toggle('selected', selected);
        card.setAttribute('aria-current', selected ? 'true' : 'false');
        if (selected) card.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    });
    const pickup = pickups.find(item => item.id === selectedId);
    if (pickup) status.textContent = `${pickup.name}: ${pickup.address}, ${pickup.city}. ${pickup.schedule || 'Horario a coordinar.'}`;
    const marker = markers.get(selectedId);
    if (moveMap && map && marker) map.setView(marker.getLatLng(), 15);
    if (marker) marker.openPopup();
}

function render() {
    const data = visiblePickups();
    resultCount.textContent = `${data.length} punto${data.length === 1 ? '' : 's'} de retiro`;
    clearButton.hidden = activeCountry === 'all' && !search && !userLocation;
    if (!data.length) {
        list.innerHTML = `<div class="map-empty"><strong>${pickups.length ? 'No encontramos puntos de retiro' : 'Todavía no hay puntos publicados'}</strong><span>${pickups.length ? 'Probá otro país o término de búsqueda.' : 'Los puntos aparecerán cuando un shaper habilite un lugar de entrega.'}</span></div>`;
        status.textContent = pickups.length ? 'No hay resultados para los filtros seleccionados.' : 'Aún no hay ubicaciones disponibles.';
        return;
    }
    list.innerHTML = data.map(pickup => {
        const distance = userLocation ? `<span class="pickup-distance">A ${distanceKm(userLocation, pickup).toFixed(1)} km</span>` : '';
        return `<article class="shaper-map-card${pickup.id === selectedId ? ' selected' : ''}" data-id="${pickup.id}" tabindex="0" aria-current="${pickup.id === selectedId}"><div class="shaper-map-logo">${pickup.logo ? `<img src="${escapeHtml(pickup.logo)}" alt="Logo de ${escapeHtml(pickup.shaper)}">` : '⌖'}</div><div class="shaper-map-info"><span>${countries[pickup.country].flag} ${escapeHtml(pickup.city)}</span>${distance}<h2>${escapeHtml(pickup.name)}</h2><p><strong>${escapeHtml(pickup.shaper)}</strong><br>${escapeHtml(pickup.address)}</p><dl><dt>Horario</dt><dd>${escapeHtml(pickup.schedule || 'A coordinar')}</dd>${pickup.notes ? `<dt>Indicaciones</dt><dd>${escapeHtml(pickup.notes)}</dd>` : ''}</dl><div><button type="button" data-focus="${pickup.id}" aria-label="Ver ${escapeHtml(pickup.name)} en el mapa">Ver en el mapa</button><a href="${pickup.pageUrl}">Ver shaper →</a></div></div></article>`;
    }).join('');
    list.querySelectorAll('[data-focus]').forEach(button => button.addEventListener('click', () => selectPickup(button.dataset.focus)));
    list.querySelectorAll('.shaper-map-card').forEach(card => card.addEventListener('keydown', event => {
        if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); selectPickup(card.dataset.id); }
    }));
}

function updateMap() {
    if (!map) return;
    const data = visiblePickups();
    const ids = new Set(data.map(pickup => pickup.id));
    markers.forEach((marker, id) => ids.has(id) ? marker.addTo(map) : marker.remove());
    if (!data.length) return;
    const bounds = L.latLngBounds(data.map(pickup => [pickup.lat, pickup.lng]));
    if (data.length === 1) map.setView(bounds.getCenter(), 14);
    else map.fitBounds(bounds, { padding: [35, 35], maxZoom: 12 });
}

function refresh() { render(); updateMap(); }

document.querySelectorAll('.filter-btn').forEach(button => button.addEventListener('click', () => {
    document.querySelectorAll('.filter-btn').forEach(item => {
        const active = item === button;
        item.classList.toggle('active', active);
        item.setAttribute('aria-pressed', active.toString());
    });
    activeCountry = button.dataset.filter;
    selectedId = null;
    refresh();
}));

searchInput.addEventListener('input', event => { search = event.target.value.trim(); selectedId = null; refresh(); });
clearButton.addEventListener('click', () => {
    activeCountry = 'all'; search = ''; selectedId = null; userLocation = null; searchInput.value = '';
    document.querySelectorAll('.filter-btn').forEach(button => { const active = button.dataset.filter === 'all'; button.classList.toggle('active', active); button.setAttribute('aria-pressed', active.toString()); });
    nearMeButton.textContent = '⌖ Cerca mío'; status.textContent = 'Seleccioná un punto para ver sus datos.'; refresh();
});

nearMeButton.addEventListener('click', () => {
    if (!navigator.geolocation) { status.textContent = 'Tu navegador no permite obtener la ubicación.'; return; }
    nearMeButton.disabled = true; nearMeButton.textContent = 'Buscando…';
    navigator.geolocation.getCurrentPosition(position => {
        userLocation = { lat: position.coords.latitude, lng: position.coords.longitude };
        nearMeButton.disabled = false; nearMeButton.textContent = '✓ Por cercanía'; status.textContent = 'Los puntos están ordenados desde el más cercano.'; refresh();
    }, () => {
        nearMeButton.disabled = false; nearMeButton.textContent = '⌖ Cerca mío'; status.textContent = 'No pudimos acceder a tu ubicación. Podés seguir usando el mapa y los filtros.';
    }, { enableHighAccuracy: false, timeout: 8000, maximumAge: 300000 });
});

refresh();
window.setTimeout(() => { if (map) map.invalidateSize(); updateMap(); }, 250);
