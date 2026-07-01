const unitId = "unit-4b-riverside";

const statusBadge = document.getElementById("status-badge");
const unitTitle = document.getElementById("unit-title");
const tenantName = document.getElementById("tenant-name");
const rentLabel = document.getElementById("rent-label");
const progressBar = document.getElementById("progress-bar");
const progressLabel = document.getElementById("progress-label");
const advanceBtn = document.getElementById("advance-btn");
const completeNote = document.getElementById("complete-note");
const timeline = document.getElementById("timeline");

const statusCopy = {
  Vacant: { emoji: "🏢", label: "Vacant", progress: 10 },
  Listed: { emoji: "📋", label: "Listed for lease", progress: 30 },
  UnderOffer: { emoji: "📝", label: "Offer under review", progress: 55 },
  LeaseSigned: { emoji: "✍️", label: "Lease signed", progress: 80 },
  Occupied: { emoji: "🔑", label: "Occupied", progress: 100 },
};

function formatTime(value) {
  if (!value) return "";
  return new Date(value).toLocaleString(undefined, {
    weekday: "short",
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
}

function renderUnit(unit) {
  const copy = statusCopy[unit.status] ?? { emoji: "📦", label: unit.status, progress: 0 };
  statusBadge.textContent = `${copy.emoji} ${copy.label}`;
  unitTitle.textContent = unit.displayTitle;
  tenantName.textContent = `Prospect tenant: ${unit.prospectTenant}`;
  rentLabel.textContent = `Rent: ${unit.rentLabel}`;
  progressBar.style.width = `${copy.progress}%`;
  progressLabel.textContent = `Lease pipeline ${copy.progress}% complete`;

  if (unit.canAdvance && unit.nextActionLabel) {
    advanceBtn.hidden = false;
    advanceBtn.textContent = unit.nextActionLabel;
    advanceBtn.disabled = false;
    completeNote.hidden = true;
  } else {
    advanceBtn.hidden = true;
    completeNote.hidden = false;
  }
}

function renderTimeline(entries) {
  timeline.innerHTML = "";
  if (!entries.length) {
    timeline.innerHTML = "<li class='timeline__item'><span class='timeline__message'>No audit entries yet.</span></li>";
    return;
  }

  for (const entry of entries) {
    const item = document.createElement("li");
    item.className = "timeline__item";
    const message = entry.message?.text ?? entry.message ?? "Update recorded";
    const detail = entry.message?.shortDescription ?? entry.revisionType ?? "";
    item.innerHTML = `
      <span class="timeline__time">${formatTime(entry.occurredAt)}</span>
      <span class="timeline__message">${message}</span>
      <div class="timeline__detail">${detail}</div>`;
    timeline.appendChild(item);
  }
}

async function loadUnit() {
  const response = await fetch(`/api/demo/units/${unitId}`);
  if (!response.ok) throw new Error("Could not load the demo property unit.");
  return response.json();
}

async function loadHistory() {
  const response = await fetch(`/api/chronicle/entities/PropertyUnit/${unitId}/history?maxResults=20`);
  if (!response.ok) return [];
  return response.json();
}

async function refresh() {
  const [unit, history] = await Promise.all([loadUnit(), loadHistory()]);
  renderUnit(unit);
  renderTimeline(history);
}

advanceBtn.addEventListener("click", async () => {
  advanceBtn.disabled = true;
  try {
    const response = await fetch(`/api/demo/units/${unitId}/advance`, { method: "POST" });
    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      alert(body.error ?? "Could not advance the workflow.");
    }
    await refresh();
  } finally {
    advanceBtn.disabled = false;
  }
});

refresh().catch((error) => {
  statusBadge.textContent = error.message;
});
