function $(id) {
    return document.getElementById(id);
}

async function doLogin() {
    const status = $("status");
    const btn = $("loginBtn");

    const email = $("email").value.trim();
    const password = $("password").value;

    status.textContent = "";

    if (!email || !password) {
        status.textContent = "Skriv in email och lösenord.";
        return;
    }

    btn.disabled = true;
    status.textContent = "Loggar in...";

    try {
        const res = await fetch("/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            credentials: "include",
            body: JSON.stringify({ email, password })
        });

        if (!res.ok) {
            const txt = await res.text();
            status.textContent = "Login fail (" + res.status + "): " + txt;
            btn.disabled = false;
            return;
        }

        // backend returnerar bool (true/false)
        const ok = await res.json();

        if (!ok) {
            status.textContent = "Fel email eller lösenord.";
            btn.disabled = false;
            return;
        }

        // Success: tillbaka till startsidan
        window.location.href = "/index.html";
    } catch (e) {
        console.error(e);
        status.textContent = "Något gick fel. Kolla konsolen.";
        btn.disabled = false;
    }
}

$("loginBtn").addEventListener("click", doLogin);

$("backBtn").addEventListener("click", () => {
    window.location.href = "/index.html";
});

// Enter = login (för att det känns rätt)
$("password").addEventListener("keydown", (e) => {
    if (e.key === "Enter") doLogin();
});
