import { Navigate, Outlet } from "react-router-dom";
import { getConfig } from "@/lib/config";
const ProtectedRoute = () => {
  const { jwtToken } = getConfig();

  if (!jwtToken) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
