/* ============================================================================
   MarketPlace — application logic (vanilla JS)
   Talks to the ASP.NET Core MarketPlace API. Base URL lives in config.js.
============================================================================ */

const API = window.APP_CONFIG.API_BASE;

/* ------------------------------------------------------------------ state */
const state = {
  products: [],
  categories: [],
  total: 0,
  page: 1,
  pageSize: 12,
  query: { search: "", categoryId: null, minPrice: null, maxPrice: null, sortBy: "" },
  cart: loadCart(),
  user: loadUser(),
  detailQty: 1,
  currentProduct: null,
};

/* Role enum from the API serializes as an integer (0 Buyer, 1 Seller, 2 Admin). */
const ROLE_LABELS = ["Buyer", "Seller", "Admin"];
/* OrderStatus enum: 0 Pending, 1 Paid, 2 Shipped, 3 Delivered, 4 Cancelled. */
const ORDER_STATUS = ["Pending", "Paid", "Shipped", "Delivered", "Cancelled"];

const SORTS = [
  { v: "",           label: "Newest" },
  { v: "price_asc",  label: "Price: Low → High" },
  { v: "price_desc", label: "Price: High → Low" },
  { v: "title",      label: "Name: A → Z" },
];

/* ============================================================ API helpers */
async function api(path, { method = "GET", body, auth = false } = {}) {
  const headers = { "Content-Type": "application/json" };
  if (auth && state.user?.token) headers["Authorization"] = `Bearer ${state.user.token}`;

  let res;
  try {
    res = await fetch(`${API}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch (e) {
    // Network / CORS / server-down all land here.
    throw new ApiError(0, "Cannot reach the API. Is it running, and is CORS enabled?");
  }

  if (res.status === 204) return null;

  const text = await res.text();
  const data = text ? safeJson(text) : null;

  if (!res.ok) {
    throw new ApiError(res.status, extractMessage(data, res.status));
  }
  return data;
}

class ApiError extends Error {
  constructor(status, message) { super(message); this.status = status; }
}
function safeJson(t) { try { return JSON.parse(t); } catch { return t; } }
function extractMessage(data, status) {
  if (data && typeof data === "object") {
    if (data.title) return data.title;
    if (data.message) return data.message;
    if (data.errors) return Object.values(data.errors).flat().join(" ");
  }
  if (typeof data === "string" && data.trim()) return data;
  if (status === 401) return "You need to sign in for this.";
  if (status === 403) return "You don't have permission for this.";
  return `Request failed (HTTP ${status}).`;
}

/* ============================================================ persistence */
function loadUser() { try { return JSON.parse(localStorage.getItem("mp_user")); } catch { return null; } }
function saveUser(u) { state.user = u; localStorage.setItem("mp_user", JSON.stringify(u)); }
function clearUser() { state.user = null; localStorage.removeItem("mp_user"); }
function loadCart() { try { return JSON.parse(localStorage.getItem("mp_cart")) || []; } catch { return []; } }
function saveCart() { localStorage.setItem("mp_cart", JSON.stringify(state.cart)); }

/* ============================================================ utilities */
const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];
const esc = (s) => String(s ?? "").replace(/[&<>"']/g, (c) =>
  ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
const money = (n) => "$" + Number(n).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/* Deterministic gradient per product so the store looks vivid without images
   (the API has no image field — we generate a stable color from the id). */
const PALETTES = [
  ["#6d5efc", "#8b5cf6"], ["#10b981", "#059669"], ["#f59e0b", "#f97316"],
  ["#ef4444", "#ec4899"], ["#0ea5e9", "#6366f1"], ["#14b8a6", "#06b6d4"],
  ["#a855f7", "#d946ef"], ["#f43f5e", "#fb7185"], ["#22c55e", "#84cc16"],
];
function gradientFor(id) {
  const [a, b] = PALETTES[Math.abs(id) % PALETTES.length];
  return `linear-gradient(135deg, ${a}, ${b})`;
}
function initialOf(title) { return (title || "?").trim().charAt(0).toUpperCase(); }
function categoryName(id) { return state.categories.find((c) => c.id === id)?.name || "Product"; }
function stockTag(stock) {
  if (stock <= 0) return { cls: "stock-out", text: "Sold out" };
  if (stock <= 5) return { cls: "stock-low", text: `${stock} left` };
  return { cls: "stock-in", text: "In stock" };
}

function toast(message, type = "ok") {
  const host = $("#toastHost");
  const el = document.createElement("div");
  el.className = `toast ${type}`;
  el.innerHTML = `<span>${type === "ok" ? "✅" : "⚠️"}</span><span>${esc(message)}</span>`;
  host.appendChild(el);
  setTimeout(() => el.remove(), 3600);
}

/* ============================================================ rendering */
function renderChips() {
  const host = $("#chips");
  const all = `<button class="chip ${state.query.categoryId === null ? "active" : ""}" data-cat="all">All</button>`;
  const cats = state.categories.map((c) =>
    `<button class="chip ${state.query.categoryId === c.id ? "active" : ""}" data-cat="${c.id}">${esc(c.name)}</button>`
  ).join("");
  host.innerHTML = all + cats;
  $$(".chip", host).forEach((btn) => btn.addEventListener("click", () => {
    const v = btn.dataset.cat;
    state.query.categoryId = v === "all" ? null : Number(v);
    state.page = 1;
    fetchProducts();
  }));
}

function renderSorts() {
  const sel = $("#sortSelect");
  sel.innerHTML = SORTS.map((s) => `<option value="${s.v}">${s.label}</option>`).join("");
  sel.value = state.query.sortBy;
}

function skeleton() {
  $("#grid").innerHTML = `<div class="skeleton-grid">${
    Array.from({ length: 8 }).map(() => `<div class="skeleton"></div>`).join("")
  }</div>`;
  $("#pager").innerHTML = "";
}

function renderProducts() {
  const grid = $("#grid");
  $("#resultCount").textContent = state.total
    ? `${state.total} product${state.total === 1 ? "" : "s"}`
    : "";

  if (!state.products.length) {
    grid.innerHTML = `<div class="state" style="grid-column:1/-1">
      <div class="emoji">🔍</div><h3>No products found</h3>
      <p>Try adjusting your search or filters.</p></div>`;
    $("#pager").innerHTML = "";
    return;
  }

  grid.innerHTML = state.products.map((p) => {
    const st = stockTag(p.stock);
    return `
    <article class="card" data-id="${p.id}">
      <div class="card-media" style="background:${gradientFor(p.id)}">
        <span class="cat-tag">${esc(categoryName(p.categoryId))}</span>
        <span class="stock-tag ${st.cls}">${st.text}</span>
        ${initialOf(p.title)}
      </div>
      <div class="card-body">
        <div class="card-title">${esc(p.title)}</div>
        <div class="card-desc">${esc(p.description) || "No description provided."}</div>
        <div class="card-foot">
          <div class="price"><small>$</small>${Number(p.price).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
          <button class="btn btn-primary btn-sm add-btn" data-id="${p.id}" ${p.stock <= 0 ? "disabled" : ""}>
            ${p.stock <= 0 ? "Sold out" : "Add"}
          </button>
        </div>
      </div>
    </article>`;
  }).join("");

  $$(".card", grid).forEach((card) => card.addEventListener("click", (e) => {
    if (e.target.closest(".add-btn")) return;
    openDetail(Number(card.dataset.id));
  }));
  $$(".add-btn", grid).forEach((btn) => btn.addEventListener("click", (e) => {
    e.stopPropagation();
    const p = state.products.find((x) => x.id === Number(btn.dataset.id));
    addToCart(p, 1);
  }));

  renderPager();
}

function renderPager() {
  const pages = Math.max(1, Math.ceil(state.total / state.pageSize));
  if (pages <= 1) { $("#pager").innerHTML = ""; return; }
  const cur = state.page;
  const nums = [];
  for (let i = 1; i <= pages; i++) {
    if (i === 1 || i === pages || Math.abs(i - cur) <= 1) nums.push(i);
    else if (nums[nums.length - 1] !== "…") nums.push("…");
  }
  $("#pager").innerHTML =
    `<button ${cur === 1 ? "disabled" : ""} data-go="${cur - 1}">‹ Prev</button>` +
    nums.map((n) => n === "…"
      ? `<button disabled>…</button>`
      : `<button class="${n === cur ? "active" : ""}" data-go="${n}">${n}</button>`).join("") +
    `<button ${cur === pages ? "disabled" : ""} data-go="${cur + 1}">Next ›</button>`;
  $$("#pager button[data-go]").forEach((b) => b.addEventListener("click", () => {
    const go = Number(b.dataset.go);
    if (go >= 1 && go <= pages && go !== cur) { state.page = go; fetchProducts(); scrollToGrid(); }
  }));
}

function scrollToGrid() {
  $("#catalog").scrollIntoView({ behavior: "smooth", block: "start" });
}

/* ============================================================ data loads */
async function fetchCategories() {
  try {
    state.categories = await api("/api/categories") || [];
    renderChips();
  } catch (e) {
    state.categories = [];
    renderChips();
  }
}

async function fetchProducts() {
  skeleton();
  const q = state.query;
  const params = new URLSearchParams();
  if (q.search) params.set("search", q.search);
  if (q.categoryId != null) params.set("categoryId", q.categoryId);
  if (q.minPrice != null && q.minPrice !== "") params.set("minPrice", q.minPrice);
  if (q.maxPrice != null && q.maxPrice !== "") params.set("maxPrice", q.maxPrice);
  if (q.sortBy) params.set("sortBy", q.sortBy);
  params.set("page", state.page);
  params.set("pageSize", state.pageSize);

  try {
    const data = await api(`/api/products?${params.toString()}`);
    state.products = data.items || [];
    state.total = data.totalCount || 0;
    state.page = data.page || state.page;
    renderProducts();
  } catch (e) {
    $("#grid").innerHTML = `<div class="state" style="grid-column:1/-1">
      <div class="emoji">🔌</div><h3>Couldn't load products</h3>
      <p>${esc(e.message)}</p></div>`;
    $("#pager").innerHTML = "";
  }
}

/* ============================================================ product detail */
async function openDetail(id) {
  state.detailQty = 1;
  const overlay = $("#detailOverlay");
  const body = $("#detailBody");
  body.innerHTML = `<div class="state"><div class="emoji">⏳</div><p>Loading…</p></div>`;
  overlay.classList.add("open");

  try {
    const p = await api(`/api/products/${id}`);
    state.currentProduct = p;
    renderDetail(p);
  } catch (e) {
    body.innerHTML = `<div class="state"><div class="emoji">😕</div><h3>Not found</h3><p>${esc(e.message)}</p></div>`;
  }
}

function renderDetail(p) {
  const st = stockTag(p.stock);
  $("#detailBody").innerHTML = `
    <div class="detail">
      <div class="detail-media" style="background:${gradientFor(p.id)}">${initialOf(p.title)}</div>
      <div class="detail-info">
        <span class="cat">${esc(categoryName(p.categoryId))}</span>
        <h2>${esc(p.title)}</h2>
        <p class="desc">${esc(p.description) || "No description provided."}</p>
        <div class="detail-meta">
          <div class="m"><span>Availability</span><b class="${p.stock > 0 ? "" : ""}">${st.text}</b></div>
          <div class="m"><span>Product ID</span><b>#${p.id}</b></div>
          <div class="m"><span>Listed</span><b>${new Date(p.createdAt).toLocaleDateString()}</b></div>
        </div>
        <div class="price">${money(p.price)}</div>
        ${p.stock > 0 ? `
          <div class="qty" id="detailQty">
            <button data-d="-1">−</button><span id="qtyVal">1</span><button data-d="1">+</button>
          </div>
          <div class="detail-actions">
            <button class="btn btn-primary btn-block" id="detailAdd">Add to cart</button>
          </div>` : `<div class="detail-actions"><button class="btn btn-soft btn-block" disabled>Sold out</button></div>`}
      </div>
    </div>`;

  if (p.stock > 0) {
    $$("#detailQty button").forEach((b) => b.addEventListener("click", () => {
      const next = state.detailQty + Number(b.dataset.d);
      state.detailQty = Math.max(1, Math.min(p.stock, next));
      $("#qtyVal").textContent = state.detailQty;
    }));
    $("#detailAdd").addEventListener("click", () => {
      addToCart(p, state.detailQty);
      $("#detailOverlay").classList.remove("open");
    });
  }
}

/* ============================================================ cart */
function addToCart(p, qty) {
  if (!p || p.stock <= 0) return;
  const line = state.cart.find((l) => l.id === p.id);
  const have = line ? line.qty : 0;
  const want = Math.min(p.stock, have + qty);
  if (line) line.qty = want;
  else state.cart.push({ id: p.id, title: p.title, price: p.price, stock: p.stock, qty: want });
  saveCart();
  renderCart();
  updateCartCount();
  toast(`Added “${p.title}” to cart`);
}
function setQty(id, qty) {
  const line = state.cart.find((l) => l.id === id);
  if (!line) return;
  line.qty = Math.max(1, Math.min(line.stock, qty));
  saveCart(); renderCart(); updateCartCount();
}
function removeFromCart(id) {
  state.cart = state.cart.filter((l) => l.id !== id);
  saveCart(); renderCart(); updateCartCount();
}
function cartTotal() { return state.cart.reduce((s, l) => s + l.price * l.qty, 0); }
function updateCartCount() {
  const n = state.cart.reduce((s, l) => s + l.qty, 0);
  const el = $("#cartCount");
  el.textContent = n; el.dataset.count = n;
}

function renderCart() {
  const body = $("#cartBody");
  if (!state.cart.length) {
    body.innerHTML = `<div class="state"><div class="emoji">🛒</div><h3>Your cart is empty</h3><p>Browse the store and add something you like.</p></div>`;
    $("#cartFoot").classList.add("hidden");
    return;
  }
  $("#cartFoot").classList.remove("hidden");
  body.innerHTML = state.cart.map((l) => `
    <div class="cart-line">
      <div class="cart-thumb" style="background:${gradientFor(l.id)}">${initialOf(l.title)}</div>
      <div class="info">
        <b>${esc(l.title)}</b>
        <span>${money(l.price)}</span>
        <div class="mini-qty">
          <button data-dec="${l.id}">−</button>
          <b>${l.qty}</b>
          <button data-inc="${l.id}">+</button>
        </div>
      </div>
      <div style="text-align:right">
        <div style="font-weight:700">${money(l.price * l.qty)}</div>
        <button class="rm" data-rm="${l.id}">Remove</button>
      </div>
    </div>`).join("");

  const total = cartTotal();
  $("#cartSubtotal").textContent = money(total);
  $("#cartTotal").textContent = money(total);
  $$("[data-inc]", body).forEach((b) => b.onclick = () => { const l = state.cart.find(x => x.id === +b.dataset.inc); setQty(l.id, l.qty + 1); });
  $$("[data-dec]", body).forEach((b) => b.onclick = () => { const l = state.cart.find(x => x.id === +b.dataset.dec); setQty(l.id, l.qty - 1); });
  $$("[data-rm]", body).forEach((b) => b.onclick = () => removeFromCart(+b.dataset.rm));
}

async function checkout() {
  if (!state.user) { openAuth("login"); toast("Please sign in to place an order", "err"); return; }
  if (!state.cart.length) return;

  const btn = $("#checkoutBtn");
  btn.disabled = true; btn.textContent = "Placing order…";
  try {
    const dto = { items: state.cart.map((l) => ({ productId: l.id, quantity: l.qty })) };
    const order = await api("/api/orders", { method: "POST", body: dto, auth: true });
    state.cart = []; saveCart(); renderCart(); updateCartCount();
    toggleCart(false);
    toast(`Order #${order.id} placed — ${money(order.totalPrice)} 🎉`);
    fetchProducts(); // stock changed
  } catch (e) {
    if (e.status === 401) { clearUser(); refreshUserUI(); openAuth("login"); }
    toast(e.message, "err");
  } finally {
    btn.disabled = false; btn.textContent = "Place order";
  }
}

/* ============================================================ orders view */
async function openOrders() {
  const overlay = $("#ordersOverlay");
  const body = $("#ordersBody");
  overlay.classList.add("open");
  body.innerHTML = `<div class="state"><div class="emoji">⏳</div><p>Loading your orders…</p></div>`;
  try {
    const orders = await api("/api/orders/my", { auth: true });
    if (!orders.length) {
      body.innerHTML = `<div class="state"><div class="emoji">📦</div><h3>No orders yet</h3><p>When you place an order it'll show up here.</p></div>`;
      return;
    }
    body.innerHTML = orders.sort((a, b) => b.id - a.id).map(renderOrderCard).join("");
    bindOrderActions(body);
  } catch (e) {
    body.innerHTML = `<div class="state"><div class="emoji">😕</div><h3>Couldn't load orders</h3><p>${esc(e.message)}</p></div>`;
  }
}

function renderOrderCard(o) {
  const canCancel = o.status === 0 || o.status === 1; // Pending or Paid
  return `
    <div class="order-card">
      <div class="order-top">
        <b>Order #${o.id}</b>
        <span class="date">${new Date(o.createdAt).toLocaleString()}</span>
      </div>
      <div class="order-items">
        ${o.items.map((it) => `<div><span>${it.quantity} × Product #${it.productId}</span><span>${money(it.unitPrice * it.quantity)}</span></div>`).join("")}
      </div>
      <div class="order-foot">
        <span class="badge st-${o.status}">${ORDER_STATUS[o.status] ?? o.status}</span>
        <div style="display:flex;align-items:center;gap:12px">
          <b>${money(o.totalPrice)}</b>
          ${canCancel ? `<button class="btn btn-danger btn-sm" data-cancel="${o.id}">Cancel</button>` : ""}
        </div>
      </div>
    </div>`;
}

function bindOrderActions(root) {
  $$("[data-cancel]", root).forEach((b) => b.addEventListener("click", async () => {
    const id = Number(b.dataset.cancel);
    b.disabled = true; b.textContent = "Cancelling…";
    try {
      await api(`/api/orders/${id}/cancel`, { method: "POST", auth: true });
      toast(`Order #${id} cancelled`);
      openOrders();
    } catch (e) { toast(e.message, "err"); b.disabled = false; b.textContent = "Cancel"; }
  }));
}

/* ============================================================ auth UI */
function openAuth(tab = "login") {
  $("#authOverlay").classList.add("open");
  switchAuthTab(tab);
  $("#authError").textContent = "";
}
function switchAuthTab(tab) {
  $$("#authTabs button").forEach((b) => b.classList.toggle("active", b.dataset.tab === tab));
  $("#loginForm").classList.toggle("hidden", tab !== "login");
  $("#registerForm").classList.toggle("hidden", tab !== "register");
  $("#authTitle").textContent = tab === "login" ? "Welcome back" : "Create your account";
  $("#authError").textContent = "";
}

async function doLogin(e) {
  e.preventDefault();
  const btn = $("#loginBtn"); const err = $("#authError");
  err.textContent = "";
  btn.disabled = true; btn.textContent = "Signing in…";
  try {
    const dto = { email: $("#loginEmail").value.trim(), password: $("#loginPassword").value };
    const res = await api("/api/auth/login", { method: "POST", body: dto });
    onAuthenticated(res, "Signed in");
  } catch (ex) {
    // No global exception handler on the API → bad credentials surface as 500.
    err.textContent = ex.status === 401 || ex.status === 500
      ? "Invalid email or password."
      : ex.message;
  } finally { btn.disabled = false; btn.textContent = "Sign in"; }
}

async function doRegister(e) {
  e.preventDefault();
  const btn = $("#registerBtn"); const err = $("#authError");
  err.textContent = "";
  btn.disabled = true; btn.textContent = "Creating account…";
  try {
    const dto = {
      username: $("#regUsername").value.trim(),
      email: $("#regEmail").value.trim(),
      password: $("#regPassword").value,
    };
    const res = await api("/api/auth/register", { method: "POST", body: dto });
    onAuthenticated(res, "Account created — welcome!");
  } catch (ex) {
    err.textContent = ex.status === 500 && /email/i.test(ex.message)
      ? "That email is already registered."
      : ex.message || "Could not create account.";
  } finally { btn.disabled = false; btn.textContent = "Create account"; }
}

function onAuthenticated(res, msg) {
  saveUser({
    id: res.id, username: res.username, email: res.email,
    role: res.role, token: res.token,
  });
  $("#authOverlay").classList.remove("open");
  refreshUserUI();
  toast(msg);
}

function logout() {
  clearUser();
  refreshUserUI();
  $("#userMenu").classList.add("hidden");
  toast("Signed out");
}

function refreshUserUI() {
  const guest = $("#guestActions");
  const menuWrap = $("#userMenuWrap");
  if (state.user) {
    guest.classList.add("hidden");
    menuWrap.classList.remove("hidden");
    $("#avatarBtn").textContent = initialOf(state.user.username || state.user.email);
    $("#menuName").textContent = state.user.username || "—";
    $("#menuEmail").textContent = state.user.email || "";
    $("#menuRole").textContent = ROLE_LABELS[state.user.role] ?? "Buyer";
  } else {
    guest.classList.remove("hidden");
    menuWrap.classList.add("hidden");
  }
}

/* ============================================================ drawers/modals */
function toggleCart(open) {
  $("#cartDrawer").classList.toggle("open", open);
  $("#cartBackdrop").classList.toggle("open", open);
  if (open) renderCart();
}

/* ============================================================ events */
function bindEvents() {
  // search (debounced)
  let t;
  $("#searchInput").addEventListener("input", (e) => {
    clearTimeout(t);
    t = setTimeout(() => { state.query.search = e.target.value.trim(); state.page = 1; fetchProducts(); }, 350);
  });

  // sort
  $("#sortSelect").addEventListener("change", (e) => { state.query.sortBy = e.target.value; state.page = 1; fetchProducts(); });

  // price filters
  $("#applyPrice").addEventListener("click", () => {
    state.query.minPrice = $("#minPrice").value || null;
    state.query.maxPrice = $("#maxPrice").value || null;
    state.page = 1; fetchProducts();
  });

  // auth
  $("#loginOpenBtn").addEventListener("click", () => openAuth("login"));
  $("#registerOpenBtn").addEventListener("click", () => openAuth("register"));
  $$("#authTabs button").forEach((b) => b.addEventListener("click", () => switchAuthTab(b.dataset.tab)));
  $("#loginForm").addEventListener("submit", doLogin);
  $("#registerForm").addEventListener("submit", doRegister);

  // user menu
  $("#avatarBtn").addEventListener("click", (e) => { e.stopPropagation(); $("#userMenu").classList.toggle("hidden"); });
  $("#ordersMenuBtn").addEventListener("click", () => { $("#userMenu").classList.add("hidden"); openOrders(); });
  $("#logoutBtn").addEventListener("click", logout);
  document.addEventListener("click", () => $("#userMenu").classList.add("hidden"));

  // cart
  $("#cartBtn").addEventListener("click", () => toggleCart(true));
  $("#cartClose").addEventListener("click", () => toggleCart(false));
  $("#cartBackdrop").addEventListener("click", () => toggleCart(false));
  $("#checkoutBtn").addEventListener("click", checkout);

  // overlays: close on backdrop / x
  $$("[data-close]").forEach((el) => el.addEventListener("click", (e) => {
    if (e.target === el || e.target.closest(".close-x")) el.classList.remove("open");
  }));
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
      $$(".overlay.open").forEach((o) => o.classList.remove("open"));
      toggleCart(false);
    }
  });

  // hero CTA
  $("#browseCta").addEventListener("click", scrollToGrid);
}

/* ============================================================ boot */
async function init() {
  bindEvents();
  renderSorts();
  refreshUserUI();
  updateCartCount();
  renderCart();
  await fetchCategories();
  await fetchProducts();
}

document.addEventListener("DOMContentLoaded", init);
