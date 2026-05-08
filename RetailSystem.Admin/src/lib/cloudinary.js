import { getConfig } from "./config.js";

export async function uploadToCloudinary(file) {
  const { cloudinaryCloudName, cloudinaryUploadPreset } = getConfig();
  if (!cloudinaryCloudName || !cloudinaryUploadPreset) {
    throw new Error(
      "Cloudinary chưa được cấu hình. Vào Settings để thêm cloud name và upload preset.",
    );
  }
  const url = `https://api.cloudinary.com/v1_1/${cloudinaryCloudName}/image/upload`;
  const fd = new FormData();
  fd.append("file", file);
  fd.append("upload_preset", cloudinaryUploadPreset);
  const res = await fetch(url, { method: "POST", body: fd });
  if (!res.ok) throw new Error("Upload Cloudinary failed");
  const json = await res.json();
  return json.secure_url;
}
