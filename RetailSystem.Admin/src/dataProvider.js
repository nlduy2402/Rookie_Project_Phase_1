import api from "./api/axiosClient";

const mapId = (item) => {
  if (!item) return null;
  return {
    ...item,
    id: item.id ?? item.Id,
  };
};

const dataProvider = {
  getList: async (resource) => {
    const res = await api.get(`/${resource}`);
    const rawData = res.data?.data || [];

    return {
      data: rawData.map(mapId),
      total: res.data?.total ?? rawData.length,
    };
  },

  getMany: async (resource, params) => {
    const res = await api.get(`/${resource}`);
    const rawData = res.data?.data || [];

    return {
      data: rawData.map(mapId).filter((item) => params.ids.includes(item.id)),
    };
  },

  getOne: async (resource, params) => {
    try {
      const res = await api.get(`/${resource}/${params.id}`);
      return {
        data: mapId(res.data?.data),
      };
    } catch (err) {
      console.error("GET ONE ERROR - TraceId:", err.response?.data?.traceId);

      const listRes = await api.get(`/${resource}`);
      const rawData = listRes.data?.data || [];
      const item = rawData.find((x) => (x.id ?? x.Id) == params.id);

      if (!item) throw new Error("Record not found");

      return {
        data: mapId(item),
      };
    }
  },

  create: async (resource, params) => {
    const res = await api.post(`/${resource}`, params.data);
    return {
      data: mapId(res.data?.data),
    };
  },

  update: async (resource, params) => {
    const res = await api.put(`/${resource}/${params.id}`, params.data);
    return {
      data: mapId(res.data?.data),
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
