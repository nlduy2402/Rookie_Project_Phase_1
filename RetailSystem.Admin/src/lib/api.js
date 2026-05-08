import { getConfig } from "./config.js";

async function request(path, init = {}) {
  const cfg = getConfig();
  const url = `${cfg.apiBaseUrl.replace(/\/$/, "")}${path}`;

  const headers = new Headers(init.headers);

  const isFormData = init.body instanceof FormData;

  if (!headers.has("Content-Type") && init.body && !isFormData) {
    headers.set("Content-Type", "application/json");
  }

  if (cfg.jwtToken) {
    headers.set("Authorization", `Bearer ${cfg.jwtToken}`);
  }

  const res = await fetch(url, {
    ...init,
    headers,
  });

  const text = await res.text();

  let body;

  try {
    body = text ? JSON.parse(text) : undefined;
  } catch {
    body = text;
  }

  if (!res.ok) {
    const msg = body?.message || body?.title || `HTTP ${res.status}`;
    throw new Error(msg);
  }

  if (body && typeof body === "object" && "data" in body) {
    return body.data;
  }

  return body;
}

export const categoriesApi = {
  list: () => request("/api/categories"),
  get: (id) => request(`/api/categories/${id}`),
  create: (input) => request("/api/categories", { method: "POST", body: JSON.stringify(input) }),
  update: (id, input) =>
    request(`/api/categories/${id}`, { method: "PUT", body: JSON.stringify(input) }),
  remove: (id) => request(`/api/categories/${id}`, { method: "DELETE" }),
};

export const productsApi = {
  list: (page = 1, pageSize = 10, search = "") => {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (search) params.set("search", search);
    return request(`/api/products?${params}`);
  },
  get: (id) => request(`/api/products/${id}`),
  create: (input) =>
    request("/api/products", {
      method: "POST",

      body: input,
    }),
  update: (id, input) =>
    request(`/api/products/${id}`, {
      method: "PUT", 
      body: input,
    }),
  remove: (id) => request(`/api/products/${id}`, { method: "DELETE" }),
};

export const ordersApi = {
  list: (page = 1, pageSize = 4) => {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    return request(`/api/orders?${params}`);
  },
  ship: (id) => {
    return request(`/api/orders/Ship/${id}`, {
      method: "POST",
    });
  },
};

export const usersApi = {
  list: () => request("/api/users"),
  get: (id) => request(`/api/users/${id}`),
  create: (input) => request("/api/users", { method: "POST", body: JSON.stringify(input) }),
  update: (id, input) =>
    request(`/api/users/${id}`, { method: "PUT", body: JSON.stringify(input) }),
  remove: (id) => request(`/api/users/${id}`, { method: "DELETE" }),
};

export const loginApi = {
  login: (username, password) =>
    request("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password }),
    }),
};
