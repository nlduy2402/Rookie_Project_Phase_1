import React, { useState } from "react";
import { Container, Row, Col, Card, Form, Button, InputGroup } from "react-bootstrap";
import { Lock, User, ShieldCheck, Eye, EyeOff } from "lucide-react";
import { toast } from "react-hot-toast";
import { useNavigate } from "react-router-dom";
import { setConfig } from "@/lib/config";
export default function LoginPage() {
  const [formData, setFormData] = useState({ username: "", password: "" });
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await fetch("https://localhost:7260/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData),
      });

      const data = await response.json();

      if (response.ok && data.token) {
        setConfig({ jwtToken: data.token });


        toast.success("Login Successful!");
        navigate("/");
      } else {
        toast.error(data.message || "Invalid username or password");
      }
    } catch (error) {
      toast.error("Server error. Please try again later.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      className="min-vh-100 d-flex align-items-center justify-content-center bg-dark"
      style={{
        background: "linear-gradient(135deg, #0f172a 0%, #1e293b 100%)",
      }}>
      <Container>
        <Row className="justify-content-center">
          <Col md={6} lg={4}>
            <Card
              className="border-0 shadow-lg bg-white overflow-hidden"
              style={{ borderRadius: "1rem" }}>
              <Card.Body className="p-4 p-md-5">
                {/* Logo & Header */}
                <div className="text-center mb-4">
                  <div
                    className="bg-primary rounded-circle d-inline-flex align-items-center justify-content-center mb-3 shadow"
                    style={{ width: "60px", height: "60px" }}>
                    <ShieldCheck size={32} className="text-white" />
                  </div>
                  <h3 className="fw-bold text-dark mb-1">RetailSystem</h3>
                  <p className="text-muted small">Enter your credentials to access admin</p>
                </div>

                <Form onSubmit={handleSubmit}>
                  {/* Username */}
                  <Form.Group className="mb-3">
                    <Form.Label className="small fw-semibold text-muted">Username</Form.Label>
                    <InputGroup className="bg-light rounded border">
                      <InputGroup.Text className="bg-transparent border-0 pe-0 text-muted">
                        <User size={18} />
                      </InputGroup.Text>
                      <Form.Control
                        type="text"
                        placeholder="admin_user"
                        className="bg-transparent border-0 py-2 shadow-none"
                        value={formData.username}
                        onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                        required
                      />
                    </InputGroup>
                  </Form.Group>

                  {/* Password */}
                  <Form.Group className="mb-4">
                    <div className="d-flex justify-content-between">
                      <Form.Label className="small fw-semibold text-muted">Password</Form.Label>
                      <a href="#" className="small text-decoration-none fw-medium">
                        Forgot?
                      </a>
                    </div>
                    <InputGroup className="bg-light rounded border">
                      <InputGroup.Text className="bg-transparent border-0 pe-0 text-muted">
                        <Lock size={18} />
                      </InputGroup.Text>
                      <Form.Control
                        type={showPassword ? "text" : "password"}
                        placeholder="••••••••"
                        className="bg-transparent border-0 py-2 shadow-none"
                        value={formData.password}
                        onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                        required
                      />
                      <Button
                        variant="link"
                        className="text-muted border-0 shadow-none"
                        onClick={() => setShowPassword(!showPassword)}>
                        {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                      </Button>
                    </InputGroup>
                  </Form.Group>

                  {/* Submit Button */}
                  <Button
                    variant="primary"
                    type="submit"
                    className="w-100 py-2 fw-bold shadow-sm rounded-3 mb-3"
                    disabled={loading}>
                    {loading ? "Authenticating..." : "Sign In"}
                  </Button>
                </Form>

                <div className="text-center mt-3">
                  <small className="text-muted">Contact system admin if you've lost access.</small>
                </div>
              </Card.Body>
            </Card>

            {/* Footer nhỏ phía dưới Card */}
            <div className="text-center mt-4 text-white-50 small">
              &copy; 2026 RetailSystem Portal. All rights reserved.
            </div>
          </Col>
        </Row>
      </Container>

      <style>{`
        .form-control:focus {
          background-color: transparent;
        }
        .btn-primary {
          background: linear-gradient(to right, #2563eb, #1d4ed8);
          border: none;
        }
        .btn-primary:hover {
          background: linear-gradient(to right, #1d4ed8, #1e40af);
          transform: translateY(-1px);
          transition: all 0.2s;
        }
      `}</style>
    </div>
  );
}
