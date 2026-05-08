import { useLocation } from "react-router-dom";
import { useEffect } from "react";

export default function NotFound() {
  const location = useLocation();
  useEffect(() => {
    console.error("404:", location.pathname);
  }, [location.pathname]);

  return (
    <div className="d-flex min-vh-100 align-items-center justify-content-center bg-light">
      <div className="text-center">
        <h1 className="display-3 fw-bold">404</h1>
        <p className="text-muted">Page Not Found</p>
        <a href="/" className="text-primary">
          Back to Dashboard
        </a>
      </div>
    </div>
  );
}
