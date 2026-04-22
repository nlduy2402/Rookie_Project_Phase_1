import axios from "axios";

const instance = axios.create({
  baseURL: "https://localhost:7250/api", // đổi theo API bạn
  headers: {
    "Content-Type": "application/json",
  },
});

export default instance;
