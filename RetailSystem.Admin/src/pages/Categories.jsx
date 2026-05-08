import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, Trash2 } from "lucide-react";
import { Button, Card, Table, Modal, Form } from "react-bootstrap";
import { toast } from "sonner";
import { categoriesApi } from "@/lib/api.js";

export default function CategoriesPage() {
  const qc = useQueryClient();
  const { data, isLoading, error } = useQuery({
    queryKey: ["categories"],
    queryFn: () => categoriesApi.list(),
  });

  const [editing, setEditing] = useState(null);
  const [creating, setCreating] = useState(false);
  const [deleting, setDeleting] = useState(null);

  const onSaved = () => {
    qc.invalidateQueries({ queryKey: ["categories"] });
    setEditing(null);
    setCreating(false);
  };

  const removeMut = useMutation({
    mutationFn: (id) => categoriesApi.remove(id),
    onSuccess: () => {
      toast.success("Category deleted successfully");
      qc.invalidateQueries({ queryKey: ["categories"] }); 
      setDeleting(null);
    },
    onError: (e) => toast.error(e.message),
  });

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <div>
          <h2 className="h4 fw-bold mb-1">Categories</h2>
          <p className="text-muted mb-0">Manage your product categories.</p>
        </div>
        <Button onClick={() => setCreating(true)}>
          <Plus size={16} className="me-1" /> Add Category
        </Button>
      </div>

      <Card className="shadow-sm border-0">
        <Table hover responsive className="mb-0 align-middle">
          <thead className="table-light">
            <tr>
              {/* <th style={{ width: 60 }}>ID</th> */}
              <th>Name</th>
              <th>Description</th>
              <th style={{ width: 120 }} className="text-end">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={4} className="text-center text-muted py-4">
                  Loading…
                </td>
              </tr>
            )}
            {error && (
              <tr>
                <td colSpan={4} className="text-center text-danger py-4">
                  {error.message}
                </td>
              </tr>
            )}
            {data?.map((c) => (
              <tr key={c.id}>
                {/* <td className="text-muted">{c.id}</td> */}
                <td className="fw-medium">{c.name}</td>
                <td className="text-muted text-truncate" style={{ maxWidth: 400 }}>
                  {c.description}
                </td>
                <td className="text-end">
                  <Button size="sm" variant="link" onClick={() => setEditing(c)}>
                    <Pencil size={16} />
                  </Button>
                  <Button
                    size="sm"
                    variant="link"
                    className="text-danger"
                    onClick={() => setDeleting(c)}>
                    <Trash2 size={16} />
                  </Button>
                </td>
              </tr>
            ))}
            {data && data.length === 0 && (
              <tr>
                <td colSpan={4} className="text-center text-muted py-4">
                  No categories found.
                </td>
              </tr>
            )}
          </tbody>
        </Table>
      </Card>

      <CategoryFormModal
        show={creating || !!editing}
        category={editing ?? undefined}
        onClose={() => {
          setCreating(false);
          setEditing(null);
        }}
        onSaved={onSaved}
      />

      <Modal show={!!deleting} onHide={() => setDeleting(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Delete Category?</Modal.Title>
        </Modal.Header>
        <Modal.Body>“{deleting?.name}” will be deleted permanently.</Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setDeleting(null)}>
            Cancel
          </Button>
          <Button
            variant="danger"
            onClick={() => deleting && removeMut.mutate(deleting.id)}
            disabled={removeMut.isPending}>
            {removeMut.isPending ? "Deleting…" : "Delete"}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
}

function CategoryFormModal({ show, onClose, category, onSaved }) {
  const isEdit = !!category;
  const [form, setForm] = useState({ name: "", description: "" });

  useEffect(() => {
    if (show) setForm({ name: category?.name ?? "", description: category?.description ?? "" });
  }, [show, category]);

  const mut = useMutation({
    mutationFn: async () =>
      isEdit && category ? categoriesApi.update(category.id, form) : categoriesApi.create(form),
    onSuccess: () => {
      toast.success(isEdit ? "Category updated" : "Category created");
      onSaved();
    },
    onError: (e) => toast.error(e.message),
  });

  return (
    <Modal show={show} onHide={onClose} centered>
      <Form
        onSubmit={(e) => {
          e.preventDefault();
          mut.mutate();
        }}>
        <Modal.Header closeButton>
          <Modal.Title>{isEdit ? "Edit Category" : "Add Category"}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form.Group className="mb-3">
            <Form.Label>Name</Form.Label>
            <Form.Control
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              required
            />
          </Form.Group>
          <Form.Group>
            <Form.Label>Description</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" disabled={mut.isPending}>
            {mut.isPending ? "Saving…" : "Save"}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
}
