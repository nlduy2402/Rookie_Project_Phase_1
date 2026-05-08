import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ChevronLeft,
  ChevronRight,
  Loader2,
  Pencil,
  Plus,
  Search,
  Trash2,
  Upload,
  X,
} from "lucide-react";
import {
  Button,
  Card,
  Table,
  Modal,
  Form,
  Badge,
  InputGroup,
  Row,
  Col,
  Spinner,
} from "react-bootstrap";
import { toast } from "sonner";
import { categoriesApi, productsApi } from "@/lib/api.js";
import { uploadToCloudinary } from "@/lib/cloudinary.js";

const PAGE_SIZE = 10;
const formatVND = (v) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(v);

export default function ProductsPage() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [searchInput, setSearchInput] = useState("");

  const list = useQuery({
    queryKey: ["products", page, search],
    queryFn: () => productsApi.list(page, PAGE_SIZE, search),
  });

  const products = Array.isArray(list.data) ? list.data : (list.data?.items ?? []);
  const total = Array.isArray(list.data) ? list.data.length : (list.data?.total ?? products.length);
  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  const [editing, setEditing] = useState(null);
  const [creating, setCreating] = useState(false);
  const [deleting, setDeleting] = useState(null);

  const removeMut = useMutation({
    mutationFn: (id) => productsApi.remove(id),
    onSuccess: () => {
      toast.success("Product deleted successfully");
      qc.invalidateQueries({ queryKey: ["products"] });
      setDeleting(null);
    },
    onError: (e) => {
      const backendMessage = e.response?.data?.message;

      toast.error(backendMessage || e.message);
    },
  });

  return (
    <div>
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-3">
        <div>
          <h2 className="h4 fw-bold mb-1">Products</h2>
          <p className="text-muted mb-0">Manage the list of laptops in your store.</p>
        </div>
        <div className="d-flex gap-2">
          <Form
            onSubmit={(e) => {
              e.preventDefault();
              setPage(1);
              setSearch(searchInput);
            }}>
            <InputGroup>
              <InputGroup.Text>
                <Search size={14} />
              </InputGroup.Text>
              <Form.Control
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                placeholder="Search…"
                style={{ width: 220 }}
              />
            </InputGroup>
          </Form>
          <Button onClick={() => setCreating(true)}>
            <Plus size={16} className="me-1" /> Add Product
          </Button>
        </div>
      </div>

      <Card className="shadow-sm border-0">
        <Table hover responsive className="mb-0 align-middle">
          <thead className="table-light">
            <tr>
              <th style={{ width: 80 }}>Image</th>
              <th>Name</th>
              <th>Category</th>
              <th className="text-end">Price</th>
              <th className="text-end">Stock</th>
              <th style={{ width: 120 }} className="text-end">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {list.isLoading && (
              <tr>
                <td colSpan={6} className="text-center text-muted py-4">
                  Loading…
                </td>
              </tr>
            )}
            {list.error && (
              <tr>
                <td colSpan={6} className="text-center text-danger py-4">
                  {list.error.message}
                </td>
              </tr>
            )}
            {products.map((p) => (
              <tr key={p.id}>
                <td>
                  {p.images?.[0]?.url ? (
                    <img src={p.images[0].url} alt={p.name} loading="lazy" className="thumb" />
                  ) : (
                    <div className="thumb bg-light" />
                  )}
                </td>
                <td>
                  <div className="fw-medium">{p.name}</div>
                  <div className="text-muted small text-truncate" style={{ maxWidth: 360 }}>
                    {p.description}
                  </div>
                </td>
                <td>
                  {p.category ? (
                    <Badge bg="secondary">{p.category.name}</Badge>
                  ) : (
                    <span className="text-muted small">#{p.categoryId}</span>
                  )}
                </td>
                <td className="text-end fw-medium">{formatVND(p.price)}</td>
                <td className="text-end">{p.quantity}</td>
                <td className="text-end">
                  <Button size="sm" variant="link" onClick={() => setEditing(p)}>
                    <Pencil size={16} />
                  </Button>
                  <Button
                    size="sm"
                    variant="link"
                    className="text-danger"
                    onClick={() => setDeleting(p)}>
                    <Trash2 size={16} />
                  </Button>
                </td>
              </tr>
            ))}
            {!list.isLoading && products.length === 0 && (
              <tr>
                <td colSpan={6} className="text-center text-muted py-4">
                  No products found.
                </td>
              </tr>
            )}
          </tbody>
        </Table>
      </Card>

      <div className="d-flex justify-content-between align-items-center mt-3">
        <small className="text-muted">
          Page {page} / {totalPages} · {total} products
        </small>
        <div className="d-flex gap-2">
          <Button
            variant="outline-secondary"
            size="sm"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}>
            <ChevronLeft size={16} />
          </Button>
          <Button
            variant="outline-secondary"
            size="sm"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}>
            <ChevronRight size={16} />
          </Button>
        </div>
      </div>

      <ProductFormModal
        show={creating || !!editing}
        product={editing ?? undefined}
        onClose={() => {
          setCreating(false);
          setEditing(null);
        }}
        onSaved={() => {
          qc.invalidateQueries({ queryKey: ["products"] });
          //console.log({ queryKey: ["products"] });
          setCreating(false);
          setEditing(null);
        }}
      />

      <Modal show={!!deleting} onHide={() => setDeleting(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Delete Product?</Modal.Title>
        </Modal.Header>
        <Modal.Body>“{deleting?.name}” will be permanently deleted.</Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setDeleting(null)}>
            Cancel
          </Button>
          <Button
            variant="danger"
            disabled={removeMut.isPending}
            onClick={() => deleting && removeMut.mutate(deleting.id)}>
            {removeMut.isPending ? "Deleting…" : "Delete"}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
}

const emptyForm = {
  name: "",
  description: "",
  price: 0,
  quantity: 0,
  chipSet: "",
  ram: "",
  ssd: "",
  images: [],
  categoryId: 0,
};

function ProductFormModal({ show, onClose, product, onSaved }) {
  const isEdit = !!product;
  const [form, setForm] = useState(emptyForm);
  const [uploading, setUploading] = useState(false);

  const cats = useQuery({
    queryKey: ["categories"],
    queryFn: () => categoriesApi.list(),
    enabled: show,
  });

  useEffect(() => {
    if (show) {
      setForm(
        product
          ? {
              name: product.name,
              description: product.description,
              price: product.price,
              quantity: product.quantity,
              chipSet: product.chipSet,
              ram: product.ram,
              ssd: product.ssd,
              images: product.images ?? [],
              categoryId: product.categoryId,
            }
          : emptyForm,
      );
    }
  }, [show, product]);

  const mut = useMutation({
    mutationFn: async () => {
      const formData = new FormData();

      formData.append("Name", form.name);
      formData.append("Description", form.description || "");
      formData.append("Price", form.price);
      formData.append("Quantity", form.quantity);
      formData.append("ChipSet", form.chipSet || "");
      formData.append("RAM", form.ram || "");
      formData.append("SSD", form.ssd || "");
      formData.append("CategoryId", form.categoryId);

      if (isEdit && product) {
        formData.append("Id", product.id);
      }
      // add images
      form.images.forEach((img) => {
        if (img.file) {
          formData.append("Images", img.file);
        }
      });
      // add exist images when edit product
      const existingImageIds = form.images.filter((img) => !img.file).map((img) => img.id);
      existingImageIds.forEach((id) => {
        formData.append("ExistImages", id);
      });

      if (isEdit && product) {
        return productsApi.update(product.id, formData);
      } else {
        return productsApi.create(formData);
      }
    },
    onSuccess: () => {
      toast.success(isEdit ? "Product updated" : "Product created");
      onSaved();
      onClose(); // Đóng modal sau khi xong
    },
    onError: (e) => toast.error(e.message),
  });
  // const handleFiles = async (files) => {
  //   if (!files || !files.length) return;
  //   setUploading(true);
  //   try {
  //     const urls = await Promise.all(Array.from(files).map((f) => uploadToCloudinary(f)));
  //     setForm((f) => ({ ...f, images: [...f.images, ...urls.map((url) => ({ url, name: "" }))] }));
  //     toast.success(`Uploaded ${urls.length} images successfully`);
  //   } catch (e) {
  //     toast.error(e.message);
  //   } finally {
  //     setUploading(false);
  //   }
  // };

  const handleFiles = (files) => {
    if (!files || !files.length) return;

    const newImages = Array.from(files).map((file) => ({
      file: file,
      url: URL.createObjectURL(file),
    }));

    setForm((prev) => ({
      ...prev,
      images: [...prev.images, ...newImages],
    }));
  };

  return (
    <Modal show={show} onHide={onClose} centered size="lg" scrollable>
      <Form
        onSubmit={(e) => {
          e.preventDefault();
          mut.mutate();
        }}>
        <Modal.Header closeButton>
          <Modal.Title>{isEdit ? "Edit Product" : "Add Product"}</Modal.Title>
        </Modal.Header>
        <Modal.Body style={{ maxHeight: "calc(100vh - 210px)", overflowY: "auto" }}>
          <Row className="g-3">
            <Col xs={12}>
              <Form.Label>Product Name</Form.Label>
              <Form.Control
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                required
              />
            </Col>
            <Col xs={12}>
              <Form.Label>Description</Form.Label>
              <Form.Control
                as="textarea"
                rows={2}
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
              />
            </Col>
            <Col md={6}>
              <Form.Label>Price (VND)</Form.Label>
              <Form.Control
                type="number"
                value={form.price}
                onChange={(e) => setForm({ ...form, price: Number(e.target.value) })}
                required
              />
            </Col>
            <Col md={6}>
              <Form.Label>Quantity</Form.Label>
              <Form.Control
                type="number"
                value={form.quantity}
                onChange={(e) => setForm({ ...form, quantity: Number(e.target.value) })}
                required
              />
            </Col>
            <Col xs={12}>
              <Form.Label>Category</Form.Label>
              <Form.Select
                value={form.categoryId ? String(form.categoryId) : ""}
                onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })}
                required>
                <option value="">Select Category</option>
                {cats.data?.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </Form.Select>
            </Col>
            <Col xs={12}>
              <Form.Label>ChipSet</Form.Label>
              <Form.Control
                value={form.chipSet}
                onChange={(e) => setForm({ ...form, chipSet: e.target.value })}
              />
            </Col>
            <Col md={6}>
              <Form.Label>RAM</Form.Label>
              <Form.Control
                value={form.ram}
                onChange={(e) => setForm({ ...form, ram: e.target.value })}
              />
            </Col>
            <Col md={6}>
              <Form.Label>SSD</Form.Label>
              <Form.Control
                value={form.ssd}
                onChange={(e) => setForm({ ...form, ssd: e.target.value })}
              />
            </Col>
            <Col xs={12}>
              <Form.Label>Product Images</Form.Label>
              <div className="d-flex flex-wrap gap-2">
                {/* {form.images.map((img, idx) => (
                  <div key={idx} className="image-wrap">
                    <img src={img.url} alt="" className="thumb-lg" />
                    <Button
                      variant="danger"
                      className="btn-remove"
                      onClick={() =>
                        setForm({ ...form, images: form.images.filter((_, i) => i !== idx) })
                      }
                      type="button">
                      <X size={12} />
                    </Button>
                  </div>
                ))} */}
                {form.images.map((img, idx) => (
                  <div key={idx} className="image-wrap">
                    <img src={img.url} alt="" className="thumb-lg" />
                    <Button
                      variant="danger"
                      className="btn-remove"
                      onClick={() => {
                        // Giải phóng bộ nhớ link tạm
                        URL.revokeObjectURL(img.url);
                        setForm({
                          ...form,
                          images: form.images.filter((_, i) => i !== idx),
                        });
                      }}
                      type="button">
                      <X size={12} />
                    </Button>
                  </div>
                ))}
                <label className="upload-box">
                  {uploading ? (
                    <Spinner size="sm" />
                  ) : (
                    <>
                      <Upload size={16} />
                      <span>Upload</span>
                    </>
                  )}
                  <input
                    type="file"
                    accept="image/*"
                    multiple
                    hidden
                    onChange={(e) => handleFiles(e.target.files)}
                    disabled={uploading}
                  />
                </label>
              </div>
              <Form.Text>
                Images are uploaded directly to Cloudinary (configuration in Settings).
              </Form.Text>
            </Col>
          </Row>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" disabled={mut.isPending || uploading}>
            {mut.isPending ? (
              <>
                <Loader2 size={14} className="me-1" /> Saving…
              </>
            ) : (
              "Save"
            )}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
}
