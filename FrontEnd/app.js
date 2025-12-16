// Hämtar paket från backend och ritar dem på sidan
async function loadPackages() {
    const status = document.getElementById("status");
    const grid = document.getElementById("packages");

    try {
        // Träffar vårt API: GET /packages
        const res = await fetch("/packages");

        if (!res.ok) {
            // Denna är för om vi får ett error så får man feedback
            status.textContent = "Kunde inte ladda paket (" + res.status + ")";
            return;
        }

        const data = await res.json();

        if (!data || data.length === 0) {
            status.textContent = "Inga paket hittades.";
            return;
        }

        // Allt är okej, ta bort ladd-texten. EGENTLIGEN är (in hindsight) har jag satt för mycket fokus på frontend under laddning men mycket av detta kommer från experience från andra projekt, mall(ish). Bra habits vill jag helst inte ändra
        status.textContent = "";
        grid.innerHTML = "";

        // Loopar igenom alla paket
        for (const p of data) {
            const card = document.createElement("div");
            card.className = "card";

            // Fält kan heta lite olika beroende på backend
            const name = p.name ?? p.Name ?? "Unnamed package";
            const price = Number(p.totalPrice ?? p.TotalPrice ?? 0);
            const days = Number(p.durationDays ?? p.DurationDays ?? 0);
            const desc = p.description ?? p.Description ?? "";

            card.innerHTML = `
        <h3>${escapeHtml(name)}</h3>
        <div class="meta">${days} dagar • ${price.toFixed(0)} SEK</div>
        <p class="small">${escapeHtml(desc)}</p>
      `;

            grid.appendChild(card);
        }
    } catch (err) {
        // Om något gick ååt skoggen
        status.textContent = "Något gick fel. Kolla konsolen.";
        console.error(err);
    }
}

// Minimal XSS-skydd
function escapeHtml(str) {
    return String(str)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

// Kör direkt när sidan laddas
loadPackages();
