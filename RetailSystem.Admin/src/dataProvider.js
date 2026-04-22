import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7260/api",
});

const mapId = (item) => ({
  ...item,
  id: item.id ?? item.Id,
});

const dataProvider = {
  getList: async (resource) => {
    const res = await api.get(`/${resource}`);

    return {
      data: res.data.map(mapId),
      total: res.data.length,
    };
  },

  getMany: async (resource, params) => {
    const res = await api.get(`/${resource}`);

    return {
      data: res.data.map(mapId).filter((item) => params.ids.includes(item.id)),
    };
  },

  getOne: async (resource, params) => {
    console.log("GET ONE ID:", params.id);

    try {
      const res = await api.get(`/${resource}/${params.id}`);

      return {
        data: mapId(res.data),
      };
    } catch (err) {
      console.error("GET ONE ERROR:", err);

      // 🔥 fallback nếu backend lỗi (optional)
      const listRes = await api.get(`/${resource}`);
      const item = listRes.data.find((x) => x.id === params.id);

      if (!item) throw new Error("Record not found");

      return {
        data: mapId(item),
      };
    }
  },

  create: async (resource, params) => {
    const res = await api.post(`/${resource}`, params.data);

    return {
      data: mapId(res.data),
    };
  },

  update: async (resource, params) => {
    console.log("DATA SENT:", params.data);
    const res = await api.put(`/${resource}/${params.id}`, params.data);

    return {
      data: mapId(res.data),
    };
  },

  delete: async (resource, params) => {
    await api.delete(`/${resource}/${params.id}`);

    return {
      data: { id: params.id },
    };
  },
};

export default dataProvider;
