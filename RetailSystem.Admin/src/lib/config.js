const STORAGE_KEY = "app_config_v1";

const defaults = {
  apiBaseUrl: "https://api.example.com",
  jwtToken: "",
  cloudinaryCloudName: "",
  cloudinaryUploadPreset: "",
};

export function getConfig() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return defaults;
    return { ...defaults, ...JSON.parse(raw) };
  } catch {
    return defaults;
  }
}

export function setConfig(cfg) {
  const merged = { ...getConfig(), ...cfg };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(merged));
  window.dispatchEvent(new Event("app-config-changed"));
}
