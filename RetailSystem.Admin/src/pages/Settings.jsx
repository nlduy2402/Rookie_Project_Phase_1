import { useEffect, useState } from "react";
import { Button, Card, Form } from "react-bootstrap";
import { toast } from "sonner";
import { setConfig, getConfig } from "@/lib/config.js";

export default function SettingsPage() {
  const [form, setForm] = useState(getConfig());

  useEffect(() => {
    setForm(getConfig());
  }, []);

  const save = (e) => {
    e.preventDefault();
    setConfig(form);
    toast.success("Đã lưu cấu hình");
  };

  return (
    <div className="mx-auto" style={{ maxWidth: 720 }}>
      <h2 className="h4 fw-bold mb-1">Settings</h2>
      <p className="text-muted">All configuration is saved in the browser (localStorage).</p>

      <Form onSubmit={save}>
        <Card className="shadow-sm border-0 mb-3">
          <Card.Header className="bg-white">
            <strong>API Backend</strong>
          </Card.Header>
          <Card.Body>
            <Form.Group className="mb-3">
              <Form.Label>Base URL</Form.Label>
              <Form.Control
                value={form.apiBaseUrl}
                onChange={(e) => setForm({ ...form, apiBaseUrl: e.target.value })}
                placeholder="https://api.example.com"
              />
              <Form.Text>Vd: https://api.mysite.com</Form.Text>
            </Form.Group>
          </Card.Body>
        </Card>

        <Card className="shadow-sm border-0 mb-3">
          <Card.Header className="bg-white">
            <strong>Cloudinary (upload images)</strong>
          </Card.Header>
          <Card.Body>
            <Form.Group className="mb-3">
              <Form.Label>Cloud name</Form.Label>
              <Form.Control
                value={form.cloudinaryCloudName}
                onChange={(e) => setForm({ ...form, cloudinaryCloudName: e.target.value })}
                placeholder="dsqqhlj3e"
              />
            </Form.Group>
            <Form.Group>
              <Form.Label>Upload preset</Form.Label>
              <Form.Control
                value={form.cloudinaryUploadPreset}
                onChange={(e) => setForm({ ...form, cloudinaryUploadPreset: e.target.value })}
                placeholder="ml_default"
              />
              <Form.Text>The upload preset must be an unsigned preset in Cloudinary.</Form.Text>
            </Form.Group>
          </Card.Body>
        </Card>

        <div className="d-flex justify-content-end">
          <Button type="submit">Save Configuration</Button>
        </div>
      </Form>
    </div>
  );
}
