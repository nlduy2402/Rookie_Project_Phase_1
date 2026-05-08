import { useQuery } from "@tanstack/react-query";
import { Package, Tags, AlertCircle } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, Row, Col, Alert, Button } from "react-bootstrap";
import { categoriesApi, productsApi } from "@/lib/api.js";
import { getConfig } from "@/lib/config.js";

export default function Index() {
  const cfg = getConfig();
  const configured = !!cfg.apiBaseUrl && cfg.apiBaseUrl !== "https://api.example.com";

  const products = useQuery({
    queryKey: ["products", "stat"],
    queryFn: () => productsApi.list(1, 1),
    enabled: configured,
    retry: false,
  });
  const categories = useQuery({
    queryKey: ["categories", "stat"],
    queryFn: () => categoriesApi.list(),
    enabled: configured,
    retry: false,
  });

  const productCount = (() => {
    const d = products.data;
    if (!d) return "—";
    if (Array.isArray(d)) return d.length;
    return d.total ?? "—";
  })();

  return (
    <div>
      <h2 className="h4 fw-bold mb-1">Overview</h2>
      <p className="text-muted">Manage products and categories of your laptop store.</p>

      {!configured && (
        <Alert variant="warning" className="d-flex align-items-start gap-2">
          <AlertCircle size={20} className="mt-1" />
          <div className="flex-grow-1">
            <div className="fw-semibold">API Not Configured</div>
            <div className="small">Go to Settings page to enter Base URL and JWT token.</div>
          </div>
          <Link to="/settings">
            <Button size="sm" variant="warning">
              Settings
            </Button>
          </Link>
        </Alert>
      )}

      <Row className="g-3">
        <Col md={6} lg={4}>
          <StatCard
            icon={Package}
            label="Product"
            value={productCount}
            loading={products.isLoading}
            to="/products"
          />
        </Col>
        <Col md={6} lg={4}>
          <StatCard
            icon={Tags}
            label="Category"
            value={categories.data?.length ?? "—"}
            loading={categories.isLoading}
            to="/categories"
          />
        </Col>
      </Row>
    </div>
  );
}

function StatCard({ icon: Icon, label, value, loading, to }) {
  return (
    <Link to={to} className="text-decoration-none">
      <Card className="shadow-sm border-0 h-100">
        <Card.Body>
          <div className="d-flex justify-content-between align-items-center mb-2">
            <span className="text-muted small">{label}</span>
            <span className="badge bg-light text-primary p-2">
              <Icon size={16} />
            </span>
          </div>
          <div className="h3 fw-bold mb-0 text-dark">{loading ? "…" : value}</div>
        </Card.Body>
      </Card>
    </Link>
  );
}
