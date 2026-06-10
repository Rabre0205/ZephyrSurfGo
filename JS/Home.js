   const navbar = document.getElementById('navbar');
    window.addEventListener('scroll', () => {
      navbar.classList.toggle('scrolled', window.scrollY > 60);
    });

    const revealEls = document.querySelectorAll('.reveal');
    const observer = new IntersectionObserver((entries) => {
      entries.forEach((entry, i) => {
        if (entry.isIntersecting) {
          setTimeout(() => entry.target.classList.add('visible'), i * 80);
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.12 });
    revealEls.forEach(el => observer.observe(el));


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
 
 const CATALOG_IDX = [
  {
    id: 1,
    name: 'El Cuchillo',
    maker: 'Marcos Villalba',
    country: '🇦🇷 Argentina',
    level: 'Intermedio — Shortboard 6\'2"',
    price: 680
  },

  {
    id: 2,
    name: 'Onda Certa',
    maker: 'Felipe Nunes',
    country: '🇧🇷 Brasil',
    level: 'Principiante — Funboard 7\'4"',
    price: 540
  },

  {
    id: 3,
    name: 'La Garufa',
    maker: 'Diego Rocca',
    country: '🇺🇾 Uruguay',
    level: 'Avanzado — Longboard 9\'0"',
    price: 820
  }
];

function getCart() {
  try {
    return JSON.parse(localStorage.getItem('zsg_cart') || '[]');
  } catch {
    return [];
  }
}

function saveCart(cart) {
  localStorage.setItem('zsg_cart', JSON.stringify(cart));
  updateBadge();
}

function updateBadge() {
  const total = getCart().reduce((s, i) => s + i.qty, 0);

  const badge = document.getElementById('cartBadge');

  if (badge) {
    badge.textContent = total;
    badge.style.display = total > 0 ? 'flex' : 'none';
  }
}

function showToastIdx(msg) {
  let tc = document.getElementById('zsgToasts');

  if (!tc) {
    tc = document.createElement('div');
    tc.id = 'zsgToasts';
    tc.className = 'zsg-toast-container';
    document.body.appendChild(tc);
  }

  const t = document.createElement('div');

  t.className = 'zsg-toast';

  t.innerHTML = `
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#2ec27e" stroke-width="2.5">
      <polyline points="20 6 9 17 4 12"/>
    </svg>
    ${msg}
  `;

  tc.appendChild(t);

  setTimeout(() => t.remove(), 3000);
}

function addToCartFromIndex(id) {
  const prod = CATALOG_IDX.find(p => p.id === id);

  if (!prod) return;

  const cart = getCart();

  const ex = cart.find(i => i.id === id);

  if (ex) {
    ex.qty += 1;
  } else {
    cart.push({ ...prod, qty: 1 });
  }

  saveCart(cart);

  showToastIdx(`"${prod.name}" agregada al carrito`);

  const btns = document.querySelectorAll('.btn-add-cart');

  const idx = [1,2,3].indexOf(id);

  if (btns[idx]) {
    btns[idx].textContent = '✓ Agregado';
    btns[idx].classList.add('added');

    setTimeout(() => {
      btns[idx].textContent = '+ Agregar al carrito';
      btns[idx].classList.remove('added');
    }, 1800);
  }
}

updateBadge();

const images = [
  "../IMG/FondoHome.jpg",
  "../IMG/Imagen5.jpg",
  "../IMG/Imagen4.jpg"
];

const heroBg = document.querySelector(".hero-img");
const btnLeft = document.querySelector(".hero-arrow-left");
const btnRight = document.querySelector(".hero-arrow-right");

let currentImage = 0;

function updateHeroImage() {

    heroBg.style.backgroundImage =
      `url('${images[currentImage]}')`;

}

// Flecha derecha
btnRight.addEventListener("click", () => {

  currentImage++;

  if (currentImage >= images.length) {
    currentImage = 0;
  }

  updateHeroImage();

});

// Flecha izquierda
btnLeft.addEventListener("click", () => {

  currentImage--;

  if (currentImage < 0) {
    currentImage = images.length - 1;
  }

  updateHeroImage();

});

const nav = document.getElementById('navbar');
window.addEventListener('scroll', () => {
  nav.classList.toggle('scrolled', window.scrollY > 60);
});
document.querySelectorAll('.zp-tab').forEach(tab => {
  tab.addEventListener('click', function() {
    this.closest('.zp-tabs').querySelectorAll('.zp-tab').forEach(t => t.classList.remove('active'));
    this.classList.add('active');
  });
});