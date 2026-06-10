   /* ════════════════════════════════
       DATOS DE DEALERS
    ════════════════════════════════ */
      const dealers = [
        // ── ARGENTINA ──
        {
          id: 1,
          country: "ar",
          flag: "🇦🇷",
          countryName: "Argentina",
          name: "Surf Store Mar del Plata",
          city: "Mar del Plata, Buenos Aires",
          address: "Av. Constitución 4521, Mar del Plata",
          tags: ["stock", "custom"],
          lat: -38.0023,
          lng: -57.5575,
        },
        {
          id: 2,
          country: "ar",
          flag: "🇦🇷",
          countryName: "Argentina",
          name: "Olas & Mareas",
          city: "Miramar, Buenos Aires",
          address: "Ruta 11, km 404, Miramar",
          tags: ["stock"],
          lat: -38.2704,
          lng: -57.8368,
        },
        {
          id: 3,
          country: "ar",
          flag: "🇦🇷",
          countryName: "Argentina",
          name: "Quiksurf Pinamar",
          city: "Pinamar, Buenos Aires",
          address: "Av. Shaw 1200, Pinamar",
          tags: ["stock", "custom", "repair"],
          lat: -37.1045,
          lng: -56.8673,
        },
        {
          id: 4,
          country: "ar",
          flag: "🇦🇷",
          countryName: "Argentina",
          name: "La Tabla Perfecta",
          city: "Villa Gesell, Buenos Aires",
          address: "Av. 3 Nº 668, Villa Gesell",
          tags: ["stock", "repair"],
          lat: -37.2578,
          lng: -56.9738,
        },
        {
          id: 5,
          country: "ar",
          flag: "🇦🇷",
          countryName: "Argentina",
          name: "Necochea Surf Club",
          city: "Necochea, Buenos Aires",
          address: "Calle 83 Nº 421, Necochea",
          tags: ["stock"],
          lat: -38.5537,
          lng: -58.7401,
        },
        {
          id: 6,
          country: "ar",
          flag: "🇦🇷",
          countryName: "Argentina",
          name: "Sur Boards Buenos Aires",
          city: "Buenos Aires, CABA",
          address: "Thames 1789, Palermo",
          tags: ["custom", "repair"],
          lat: -34.5887,
          lng: -58.4246,
        },
        // ── BRASIL ──
        {
          id: 7,
          country: "br",
          flag: "🇧🇷",
          countryName: "Brasil",
          name: "Floripa Surf Shop",
          city: "Florianópolis, Santa Catarina",
          address: "Rua das Capivaras 88, Florianópolis",
          tags: ["stock", "custom"],
          lat: -27.5949,
          lng: -48.5482,
        },
        {
          id: 8,
          country: "br",
          flag: "🇧🇷",
          countryName: "Brasil",
          name: "Rio Surf Culture",
          city: "Ipanema, Rio de Janeiro",
          address: "Av. Vieira Souto 320, Ipanema",
          tags: ["stock", "repair"],
          lat: -22.9872,
          lng: -43.2044,
        },
        {
          id: 9,
          country: "br",
          flag: "🇧🇷",
          countryName: "Brasil",
          name: "Ubatuba Board House",
          city: "Ubatuba, São Paulo",
          address: "Rua Guarani 54, Ubatuba",
          tags: ["stock", "custom", "repair"],
          lat: -23.4337,
          lng: -45.0838,
        },
        {
          id: 10,
          country: "br",
          flag: "🇧🇷",
          countryName: "Brasil",
          name: "Balneário Surf Co.",
          city: "Balneário Camboriú, SC",
          address: "Av. Atlântica 3200, Balneário Camboriú",
          tags: ["stock"],
          lat: -26.993,
          lng: -48.6348,
        },
        {
          id: 11,
          country: "br",
          flag: "🇧🇷",
          countryName: "Brasil",
          name: "Búzios Wave Shop",
          city: "Búzios, Rio de Janeiro",
          address: "Rua das Pedras 88, Búzios",
          tags: ["stock", "custom"],
          lat: -22.7468,
          lng: -41.8817,
        },
        {
          id: 12,
          country: "br",
          flag: "🇧🇷",
          countryName: "Brasil",
          name: "Itacaré Surf Center",
          city: "Itacaré, Bahia",
          address: "Rua Lodônio Almeida 12, Itacaré",
          tags: ["stock", "repair"],
          lat: -14.2781,
          lng: -38.9969,
        },
        // ── URUGUAY ──
        {
          id: 13,
          country: "uy",
          flag: "🇺🇾",
          countryName: "Uruguay",
          name: "Punta del Este Surf",
          city: "Punta del Este, Maldonado",
          address: "Parada 4, Playa Mansa",
          tags: ["stock", "custom"],
          lat: -34.9632,
          lng: -54.9405,
        },
        {
          id: 14,
          country: "uy",
          flag: "🇺🇾",
          countryName: "Uruguay",
          name: "La Barra Tabla Shop",
          city: "La Barra, Maldonado",
          address: "Ruta 10 km 161, La Barra",
          tags: ["stock", "repair"],
          lat: -34.9086,
          lng: -54.8672,
        },
        {
          id: 15,
          country: "uy",
          flag: "🇺🇾",
          countryName: "Uruguay",
          name: "Pocitos Surf Co.",
          city: "Pocitos, Montevideo",
          address: "Rambla Gandhi 601, Pocitos",
          tags: ["stock", "custom", "repair"],
          lat: -34.9125,
          lng: -56.1622,
        },
        {
          id: 16,
          country: "uy",
          flag: "🇺🇾",
          countryName: "Uruguay",
          name: "Cabo Polonio Sessions",
          city: "Cabo Polonio, Rocha",
          address: "Ruta 10 km 264, Rocha",
          tags: ["stock"],
          lat: -34.4012,
          lng: -53.7841,
        },
      ];

      /* ════════════════════════════════
       MAPA LEAFLET
    ════════════════════════════════ */
      const map = L.map("map", {
        center: [-33.5, -55.5],
        zoom: 5,
        zoomControl: true,
      });

      L.tileLayer(
        "https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png",
        {
          attribution: "© OpenStreetMap © CARTO",
          subdomains: "abcd",
          maxZoom: 19,
        },
      ).addTo(map);

      // Custom marker icon
      function makeIcon(country) {
        const colors = { ar: "#74b9ff", br: "#55efc4", uy: "#fdcb6e" };
        const color = colors[country] || "#4394e0";
        const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="28" height="36" viewBox="0 0 28 36">
        <path d="M14 0C6.27 0 0 6.27 0 14c0 9.63 12.13 20.93 13.31 21.97a1 1 0 001.38 0C15.87 34.93 28 23.63 28 14 28 6.27 21.73 0 14 0z" fill="${color}"/>
        <circle cx="14" cy="14" r="6" fill="#0D1B3E"/>
      </svg>`;
        return L.divIcon({
          html: svg,
          className: "",
          iconSize: [28, 36],
          iconAnchor: [14, 36],
          popupAnchor: [0, -36],
        });
      }

      const tagLabels = {
        stock: "En stock",
        custom: "Custom",
        repair: "Reparaciones",
      };
      const tagClasses = {
        stock: "tag-stock",
        custom: "tag-custom",
        repair: "tag-repair",
      };

      const markers = {};
      dealers.forEach((d) => {
        const marker = L.marker([d.lat, d.lng], {
          icon: makeIcon(d.country),
        }).addTo(map);
        marker.bindPopup(`
        <div class="popup-name">${d.name}</div>
        <div class="popup-addr">${d.address}<br><span style="color:#4394e0;font-size:.75rem">${d.city}</span></div>
        <a href="#" class="popup-link" onclick="highlightDealer(${d.id}); return false;">Ver en lista →</a>
      `);
        markers[d.id] = marker;
      });

      /* ════════════════════════════════
       RENDER LISTA
    ════════════════════════════════ */
      function renderList(filter = "all", search = "") {
        const list = document.getElementById("dealersList");
        list.innerHTML = "";

        const countries = [
          { code: "ar", flag: "🇦🇷", name: "Argentina" },
          { code: "br", flag: "🇧🇷", name: "Brasil" },
          { code: "uy", flag: "🇺🇾", name: "Uruguay" },
        ];

        countries.forEach((c) => {
          if (filter !== "all" && filter !== c.code) return;

          const filtered = dealers.filter((d) => {
            if (d.country !== c.code) return false;
            if (
              search &&
              !d.name.toLowerCase().includes(search) &&
              !d.city.toLowerCase().includes(search)
            )
              return false;
            return true;
          });
          if (!filtered.length) return;

          const group = document.createElement("div");
          group.className = "country-group";
          group.setAttribute("data-country", c.code);

          const header = document.createElement("div");
          header.className = "country-header";
          header.innerHTML = `<span class="flag">${c.flag}</span> ${c.name} <span class="count">${filtered.length} tienda${filtered.length !== 1 ? "s" : ""}</span>`;
          group.appendChild(header);

          filtered.forEach((d) => {
            const item = document.createElement("div");
            item.className = "dealer-item";
            item.setAttribute("data-id", d.id);
            const tagsHtml = d.tags
              .map(
                (t) =>
                  `<span class="tag ${tagClasses[t]}">${tagLabels[t]}</span>`,
              )
              .join("");
            item.innerHTML = `
            <div class="dealer-item-name">${d.name}</div>
            <div class="dealer-item-city">${d.city}</div>
            <div class="dealer-item-tags">${tagsHtml}</div>
            <span class="dealer-item-arrow">›</span>
          `;
            item.addEventListener("click", () => focusDealer(d.id));
            group.appendChild(item);
          });

          list.appendChild(group);
        });

        if (!list.children.length) {
          list.innerHTML = `<div style="padding:2rem 1.4rem;color:var(--muted);font-size:.85rem">No se encontraron dealers para tu búsqueda.</div>`;
        }
      }


      function focusDealer(id) {
        const d = dealers.find((x) => x.id === id);
        if (!d) return;
        // highlight en lista
        document
          .querySelectorAll(".dealer-item")
          .forEach((el) => el.classList.remove("highlighted"));
        const el = document.querySelector(`.dealer-item[data-id="${id}"]`);
        if (el) {
          el.classList.add("highlighted");
          el.scrollIntoView({ behavior: "smooth", block: "nearest" });
        }
        // mapa
        map.setView([d.lat, d.lng], 13, { animate: true });
        markers[id].openPopup();
      }

      function highlightDealer(id) {
        const el = document.querySelector(`.dealer-item[data-id="${id}"]`);
        if (el) {
          el.classList.add("highlighted");
          el.scrollIntoView({ behavior: "smooth", block: "nearest" });
        }
      }

      /* ════════════════════════════════
       FILTROS
    ════════════════════════════════ */
      let activeFilter = "all";
      let activeSearch = "";

      document.querySelectorAll(".filter-btn").forEach((btn) => {
        btn.addEventListener("click", () => {
          document
            .querySelectorAll(".filter-btn")
            .forEach((b) => b.classList.remove("active"));
          btn.classList.add("active");
          activeFilter = btn.dataset.filter;
          renderList(activeFilter, activeSearch);
          // sync util-regions
          document
            .querySelectorAll(".util-regions a")
            .forEach((a) => a.classList.remove("active"));
          const ua = document.querySelector(
            `.util-regions a[data-region="${activeFilter}"]`,
          );
          if (ua) ua.classList.add("active");
          // fit map
          fitMapToFilter(activeFilter);
        });
      });

      document.querySelectorAll(".util-regions a").forEach((a) => {
        a.addEventListener("click", (e) => {
          e.preventDefault();
          const region = a.dataset.region;
          document
            .querySelectorAll(".util-regions a")
            .forEach((x) => x.classList.remove("active"));
          a.classList.add("active");
          // sync filter btns
          document.querySelectorAll(".filter-btn").forEach((b) => {
            b.classList.toggle("active", b.dataset.filter === region);
          });
          activeFilter = region;
          renderList(activeFilter, activeSearch);
          fitMapToFilter(activeFilter);
        });
      });

      document.getElementById("searchInput").addEventListener("input", (e) => {
        activeSearch = e.target.value.toLowerCase().trim();
        renderList(activeFilter, activeSearch);
      });

      function fitMapToFilter(filter) {
  const views = {
    all: { center: [-30.5, -55.5], zoom: 4 },
    ar: { center: [-38.4, -63.6], zoom: 5 },
    br: { center: [-22.9, -43.2], zoom: 5 },
    uy: { center: [-34.8, -56.1], zoom: 7 },
  };

  const view = views[filter] || views.all;

  map.setView(view.center, view.zoom, {
    animate: true,
  });
}
      // Init
      renderList();


setTimeout(() => {
  map.invalidateSize();
}, 300);


       const navbar = document.getElementById('navbar');

  window.addEventListener('scroll', () => {
    navbar.classList.toggle('scrolled', window.scrollY > 60);
  });

  const dropdown = document.querySelector(".util-dropdown");
const toggle = document.querySelector(".dropdown-toggle");

toggle.addEventListener("click", (e) => {
  e.stopPropagation();
  dropdown.classList.toggle("active");
});

document.addEventListener("click", () => {
  dropdown.classList.remove("active");
});

