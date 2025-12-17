function $(id) { return document.getElementById(id); }

// vill inte att någon skriver <script> i title och blir hackerman
function escapeHtml(str) {
    return String(str ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

async function fetchJson(url) {
    const res = await fetch(url, { credentials: "include" });
    if (!res.ok) {
        const txt = await res.text();
        throw new Error(url + " -> " + res.status + " " + txt);
    }
    return await res.json();
}

async function postJson(url, bodyObj) {
    const res = await fetch(url, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(bodyObj)
    });

    if (!res.ok) {
        const txt = await res.text();
        throw new Error(url + " -> " + res.status + " " + txt);
    }

    const ct = res.headers.get("content-type") ?? "";
    if (ct.includes("application/json")) return await res.json();
    return await res.text();
}

function setStatus(id, text) {
    const el = $(id);
    if (el) el.textContent = text;
}

function setDisabled(id, disabled) {
    const el = $(id);
    if (el) el.disabled = disabled;
}

function goHome() {
    window.location.href = "/index.html";
}

// State (keep it simple)
let cardId = null;
let selectedDestinationId = null;

const selectedActivityIds = new Set();
const selectedAmenityIds = new Set();

function renderSingleSelect(listEl, items, getId, getTitle, getSub, onPick, selectedId) {
    listEl.innerHTML = "";

    for (const item of items) {
        const id = getId(item);

        const div = document.createElement("div");
        div.className = "pick" + (selectedId === id ? " selected" : "");
        div.innerHTML = `
            <p class="name">${escapeHtml(getTitle(item))}</p>
            <p class="sub">${escapeHtml(getSub(item))}</p>
        `;

        div.addEventListener("click", () => onPick(id));
        listEl.appendChild(div);
    }
}

function renderMultiSelect(listEl, items, getId, getTitle, getSub, selectedSet) {
    listEl.innerHTML = "";

    for (const item of items) {
        const id = getId(item);
        const selected = selectedSet.has(id);

        const div = document.createElement("div");
        div.className = "pick" + (selected ? " selected" : "");
        div.innerHTML = `
            <p class="name">${escapeHtml(getTitle(item))}</p>
            <p class="sub">${escapeHtml(getSub(item))}</p>
        `;

        div.addEventListener("click", () => {
            if (selectedSet.has(id)) selectedSet.delete(id);
            else selectedSet.add(id);

            // re-render för att visa selected-class
            renderMultiSelect(listEl, items, getId, getTitle, getSub, selectedSet);
        });

        listEl.appendChild(div);
    }
}

async function checkLoginOrBounce() {
    try {
        const me = await fetchJson("/me");
        const email = me.email ?? me.Email ?? "?";
        const role = me.role ?? me.Role ?? "?";

        $("whoami").textContent = "Welcome " + email + " (" + role + ")";
        setStatus("pageStatus", "");
        return true;
    } catch (e) {
        window.location.href = "/login.html";
        return false;
    }
}

async function loadBaseLists() {
    // DESTINATIONS
    try {
        const destinations = await fetchJson("/destinations");
        setStatus("destStatus", "");

        const pick = (id) => {
            selectedDestinationId = id;

            // re-render för selected UI
            renderSingleSelect(
                $("destList"),
                destinations,
                d => d.id ?? d.Id,
                d => d.name ?? d.Name ?? ("Destination " + (d.id ?? d.Id)),
                d => "id: " + (d.id ?? d.Id),
                pick,
                selectedDestinationId
            );
        };

        renderSingleSelect(
            $("destList"),
            destinations,
            d => d.id ?? d.Id,
            d => d.name ?? d.Name ?? ("Destination " + (d.id ?? d.Id)),
            d => "id: " + (d.id ?? d.Id),
            pick,
            selectedDestinationId
        );
    } catch (e) {
        console.error(e);
        setStatus("destStatus", "Error: " + e.message);
    }

    // ACTIVITIES
    try {
        const activities = await fetchJson("/activities");
        setStatus("actStatus", "");
        renderMultiSelect(
            $("actList"),
            activities,
            a => a.id ?? a.Id,
            a => a.name ?? a.Name ?? ("Activity " + (a.id ?? a.Id)),
            a => a.description ?? a.Description ?? ("id: " + (a.id ?? a.Id)),
            selectedActivityIds
        );
    } catch (e) {
        console.error(e);
        setStatus("actStatus", "Error: " + e.message);
    }

    // AMENITIES
    try {
        const amenities = await fetchJson("/amenities");
        setStatus("amenStatus", "");
        renderMultiSelect(
            $("amenList"),
            amenities,
            a => a.id ?? a.Id,
            a => a.name ?? a.Name ?? ("Amenity " + (a.id ?? a.Id)),
            a => a.description ?? a.Description ?? ("id: " + (a.id ?? a.Id)),
            selectedAmenityIds
        );
    } catch (e) {
        console.error(e);
        setStatus("amenStatus", "Error: " + e.message);
    }
}

async function createCard() {
    setStatus("createStatus", "");
    setStatus("linkStatus", "");

    const title = $("titleInput").value.trim();
    const description = $("descInput").value.trim();

    const budgetRaw = $("budgetInput").value;
    const budget = budgetRaw === "" ? null : Number(budgetRaw);

    const startDate = $("startInput").value || null;
    const endDate = $("endInput").value || null;

    if (!title) {
        setStatus("createStatus", "Title behövs (annars blir det bara 'Untitled').");
        return;
    }

    const body = { title, description, budget, startDate, endDate };

    try {
        $("createBtn").disabled = true;
        setStatus("createStatus", "Creating...");

        const result = await postJson("/custom-cards", body);

        const id =
            result?.id ?? result?.Id ??
            result?.cardId ?? result?.CardId ??
            null;

        if (!id) {
            setStatus("createStatus", "Created, men fick inget cardId tillbaka från servern. Fråga INTE Oskar att debugga");
            $("createBtn").disabled = false;
            return;
        }

        cardId = Number(id);
        $("cardIdText").textContent = String(cardId);

        setStatus("createStatus", "Created, Nu kan du börja länka mer stuff.");

        // Aktivera link-knappar
        setDisabled("linkDestinationBtn", false);
        setDisabled("linkActivitiesBtn", false);
        setDisabled("linkAmenitiesBtn", false);

        // Done-knapp
        setDisabled("doneBtn", false);

    } catch (e) {
        console.error(e);
        setStatus("createStatus", "Error: " + e.message);
        $("createBtn").disabled = false;
    }
}

async function linkDestination() {
    if (!cardId) return setStatus("linkStatus", "Skapa card först.");
    if (!selectedDestinationId) return setStatus("linkStatus", "Välj en destination först.");

    try {
        setStatus("linkStatus", "Linking destination...");
        await postJson("/custom-cards/destinations", {
            cardId: cardId,
            destinationId: selectedDestinationId
        });
        setStatus("linkStatus", "Destination linked");
    } catch (e) {
        console.error(e);
        setStatus("linkStatus", "Error: " + e.message);
    }
}

async function linkActivities() {
    if (!cardId) return setStatus("linkStatus", "Skapa card först.");
    if (selectedActivityIds.size === 0) return setStatus("linkStatus", "Välj minst en activity.");

    try {
        setStatus("linkStatus", "Linking activities...");

        // Länkar en och en. Vi har lite data, så det är lugnt.
        for (const activityId of selectedActivityIds) {
            await postJson("/custom-cards-activities", {
                cardId: cardId,
                activityId: activityId
            });
        }

        setStatus("linkStatus", "Activities linked");
    } catch (e) {
        console.error(e);
        setStatus("linkStatus", "Error: " + e.message);
    }
}

async function linkAmenities() {
    if (!cardId) return setStatus("linkStatus", "Skapa card först.");
    if (selectedAmenityIds.size === 0) return setStatus("linkStatus", "Välj minst en amenity.");

    try {
        setStatus("linkStatus", "Linking amenities...");

        for (const amenityId of selectedAmenityIds) {
            await postJson("/custom-cards-amenities", {
                cardId: cardId,
                amenityId: amenityId
            });
        }

        setStatus("linkStatus", "Amenities linked");
    } catch (e) {
        console.error(e);
        setStatus("linkStatus", "Error: " + e.message);
    }
}

async function init() {
    $("backBtn").addEventListener("click", goHome);

    const ok = await checkLoginOrBounce();
    if (!ok) return;

    setStatus("pageStatus", "Loading options...");
    await loadBaseLists();
    setStatus("pageStatus", "");

    $("createBtn").addEventListener("click", createCard);

    $("linkDestinationBtn").addEventListener("click", linkDestination);
    $("linkActivitiesBtn").addEventListener("click", linkActivities);
    $("linkAmenitiesBtn").addEventListener("click", linkAmenities);

    $("doneBtn").addEventListener("click", goHome);
}

init();
