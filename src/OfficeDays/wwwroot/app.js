const state = { user: null, csrf: null, holidays: [] };
const months = Array.from({ length: 12 }, (_, index) =>
  new Intl.DateTimeFormat(undefined, { month: "long" }).format(new Date(2024, index, 1)));

async function api(path, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (options.body) headers["Content-Type"] = "application/json";
  if (options.method && options.method !== "GET" && state.csrf) headers["X-CSRF-TOKEN"] = state.csrf;
  const response = await fetch(path, { credentials: "same-origin", ...options, headers });
  if (response.status === 401) {
    showLogin();
    throw new Error("Your session has ended. Please sign in again.");
  }
  if (!response.ok) {
    const problem = await response.json().catch(() => ({}));
    const validation = problem.errors && Object.values(problem.errors).flat()[0];
    throw new Error(validation || problem.detail || problem.title || `Request failed (${response.status})`);
  }
  return response.status === 204 ? null : response.json();
}

async function refreshCsrf() {
  state.csrf = (await (await fetch("/api/auth/csrf", { credentials: "same-origin" })).json()).token;
}

function showLogin() {
  document.getElementById("dashboard").hidden = true;
  document.getElementById("login-view").hidden = false;
}

function localYearMonth(timeZone) {
  const parts = new Intl.DateTimeFormat("en", { timeZone, year: "numeric", month: "numeric" }).formatToParts();
  return {
    year: Number(parts.find(x => x.type === "year").value),
    month: Number(parts.find(x => x.type === "month").value)
  };
}

function selectedPeriod() { return { year: Number(document.getElementById("year").value), month: Number(document.getElementById("month").value) }; }
function query() { const p = selectedPeriod(); return `year=${p.year}&month=${p.month}`; }
function displayDate(value) { return new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeZone: "UTC" }).format(new Date(`${value}T00:00:00Z`)); }
function displayTime(value) { return value ? new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value)) : "Never"; }
function message(text, error = false) { const el = document.getElementById("page-message"); el.textContent = text; el.style.color = error ? "var(--danger)" : "var(--green-dark)"; }

function emptyItem(text) {
  const li = document.createElement("li"); li.className = "empty"; li.textContent = text; return li;
}

function actionItem(title, detail, actionText, action) {
  const li = document.createElement("li");
  const main = document.createElement("div"); main.className = "item-main";
  const strong = document.createElement("strong"); strong.textContent = title; main.append(strong);
  if (detail) { const small = document.createElement("small"); small.textContent = detail; main.append(small); }
  const button = document.createElement("button"); button.type = "button"; button.className = "danger small"; button.textContent = actionText;
  button.addEventListener("click", action); li.append(main, button); return li;
}

async function loadDashboard() {
  const [status, attendance, vacations, tokens] = await Promise.all([
    api(`/api/status?${query()}`), api(`/api/attendance?${query()}`),
    api(`/api/vacations?${query()}`), api("/api/tokens")
  ]);
  const { year, month } = selectedPeriod();
  document.getElementById("period-title").textContent = `${months[month - 1]} ${year}`;
  document.getElementById("required-days").textContent = status.requiredOfficeDays;
  document.getElementById("office-days").textContent = status.officeDays;
  document.getElementById("remaining-days").textContent = status.remainingOfficeDays;
  document.getElementById("progress-label").textContent = `${status.progressPercentage}%`;
  document.getElementById("progress-bar").style.width = `${status.progressPercentage}%`;
  document.getElementById("eligibility-note").textContent = `${status.eligibleWorkingDays} eligible working days · maximum ${status.maximumWfhDays} WFH days`;

  const attendanceList = document.getElementById("attendance-list"); attendanceList.replaceChildren();
  attendance.forEach(item => attendanceList.append(actionItem(`✓ ${displayDate(item.date)}`, "", "Remove", async () => {
    await api(`/api/attendance/${item.date}`, { method: "DELETE" }); message("Office day removed."); await loadDashboard();
  })));
  if (!attendance.length) attendanceList.append(emptyItem("No office days recorded for this month."));

  const vacationList = document.getElementById("vacation-list"); vacationList.replaceChildren();
  vacations.forEach(item => vacationList.append(actionItem(
    item.from === item.to ? displayDate(item.from) : `${displayDate(item.from)} – ${displayDate(item.to)}`,
    "", "Remove", async () => { await api(`/api/vacations/${item.id}`, { method: "DELETE" }); message("Vacation removed."); await loadDashboard(); }
  )));
  if (!vacations.length) vacationList.append(emptyItem("No vacation overlapping this month."));

  renderTokens(tokens);
  if (state.user.isAdmin) await loadHolidays(year);
}

function renderTokens(tokens) {
  const list = document.getElementById("token-list"); list.replaceChildren();
  tokens.forEach(token => {
    const detail = `Created ${displayTime(token.createdAt)} · Last used ${displayTime(token.lastUsedAt)}`;
    const li = actionItem(token.name, detail, "Revoke", async () => {
      if (!confirm(`Revoke “${token.name}”?`)) return;
      await api(`/api/tokens/${token.id}`, { method: "DELETE" }); message("Token revoked."); renderTokens(await api("/api/tokens"));
    });
    const pill = document.createElement("span"); pill.className = `status-pill ${token.status.toLowerCase()}`; pill.textContent = token.status;
    li.querySelector(".item-main").append(pill);
    if (token.status === "Revoked") li.querySelector("button").remove();
    list.append(li);
  });
  if (!tokens.length) list.append(emptyItem("No API tokens yet."));
}

async function loadHolidays(year) {
  state.holidays = await api(`/api/bank-holidays/${year}`);
  renderHolidays();
}

function renderHolidays() {
  const list = document.getElementById("holiday-list"); list.replaceChildren();
  state.holidays.sort((a, b) => a.date.localeCompare(b.date)).forEach((holiday, index) => {
    const item = actionItem(displayDate(holiday.date), holiday.name, "Remove", () => {
      state.holidays.splice(index, 1); renderHolidays(); message("Holiday removed locally. Save the year to apply.");
    });
    const edit = document.createElement("button"); edit.type = "button"; edit.className = "secondary small"; edit.textContent = "Edit";
    edit.addEventListener("click", () => {
      document.getElementById("holiday-date").value = holiday.date; document.getElementById("holiday-name").value = holiday.name; document.getElementById("holiday-name").focus();
    });
    item.insertBefore(edit, item.lastElementChild); list.append(item);
  });
  if (!state.holidays.length) list.append(emptyItem("No configured holidays for this year."));
}

async function boot() {
  months.forEach((name, index) => document.getElementById("month").add(new Option(name, index + 1)));
  await refreshCsrf();
  try {
    state.user = await api("/api/auth/me");
    const now = localYearMonth(state.user.timeZoneId);
    document.getElementById("year").value = now.year; document.getElementById("month").value = now.month;
    document.getElementById("account-name").textContent = `${state.user.username} · ${state.user.timeZoneId}`;
    document.getElementById("login-view").hidden = true; document.getElementById("dashboard").hidden = false;
    document.getElementById("admin-holidays").hidden = !state.user.isAdmin;
    await loadDashboard();
  } catch { showLogin(); }
}

document.getElementById("login-form").addEventListener("submit", async event => {
  event.preventDefault(); document.getElementById("login-error").textContent = "";
  try {
    await api("/api/auth/login", { method: "POST", body: JSON.stringify({ username: document.getElementById("login-username").value, password: document.getElementById("login-password").value }) });
    await refreshCsrf(); location.reload();
  } catch (error) { document.getElementById("login-error").textContent = error.message; }
});

document.getElementById("logout").addEventListener("click", async () => { await api("/api/auth/logout", { method: "POST" }); location.reload(); });
document.getElementById("month").addEventListener("change", () => loadDashboard().catch(error => message(error.message, true)));
document.getElementById("year").addEventListener("change", () => loadDashboard().catch(error => message(error.message, true)));
function shiftMonth(delta) {
  let { year, month } = selectedPeriod(); month += delta;
  if (month === 0) { month = 12; year--; } if (month === 13) { month = 1; year++; }
  document.getElementById("year").value = year; document.getElementById("month").value = month; loadDashboard().catch(error => message(error.message, true));
}
document.getElementById("previous-month").addEventListener("click", () => shiftMonth(-1));
document.getElementById("next-month").addEventListener("click", () => shiftMonth(1));

document.getElementById("attendance-form").addEventListener("submit", async event => {
  event.preventDefault();
  try { await api(`/api/attendance/${document.getElementById("attendance-date").value}`, { method: "PUT" }); message("Office day recorded."); await loadDashboard(); }
  catch (error) { message(error.message, true); }
});

document.getElementById("vacation-form").addEventListener("submit", async event => {
  event.preventDefault();
  try {
    await api("/api/vacations", { method: "POST", body: JSON.stringify({ from: document.getElementById("vacation-from").value, to: document.getElementById("vacation-to").value }) });
    message("Vacation added."); await loadDashboard();
  } catch (error) { message(error.message, true); }
});

document.getElementById("token-form").addEventListener("submit", async event => {
  event.preventDefault();
  try {
    const created = await api("/api/tokens", { method: "POST", body: JSON.stringify({ name: document.getElementById("token-name").value }) });
    document.getElementById("raw-token").textContent = created.token; document.getElementById("new-token").hidden = false; document.getElementById("token-name").value = "";
    renderTokens(await api("/api/tokens")); message("Token created. Copy it before leaving this page.");
  } catch (error) { message(error.message, true); }
});
document.getElementById("copy-token").addEventListener("click", async () => { await navigator.clipboard.writeText(document.getElementById("raw-token").textContent); document.getElementById("copy-token").textContent = "Copied"; });

document.getElementById("holiday-form").addEventListener("submit", event => {
  event.preventDefault(); const date = document.getElementById("holiday-date").value; const name = document.getElementById("holiday-name").value.trim();
  if (Number(date.slice(0, 4)) !== Number(document.getElementById("year").value)) { message("Holiday must be in the selected year.", true); return; }
  const existing = state.holidays.find(item => item.date === date);
  if (existing) existing.name = name; else state.holidays.push({ date, name });
  document.getElementById("holiday-name").value = ""; renderHolidays(); message("Holiday added locally. Save the year to apply.");
});
document.getElementById("save-holidays").addEventListener("click", async () => {
  try {
    await api(`/api/bank-holidays/${document.getElementById("year").value}`, { method: "PUT", body: JSON.stringify(state.holidays) });
    message("Bank holidays saved."); await loadDashboard();
  } catch (error) { message(error.message, true); }
});

boot();
