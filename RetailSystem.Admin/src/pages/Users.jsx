import React, { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Table, Button, Card, Modal, Form } from "react-bootstrap";
import { Trash2, UserPlus, Shield } from "lucide-react";
import { toast } from "react-hot-toast";
import { usersApi } from "../lib/api.js";

export default function UsersPage() {
  const qc = useQueryClient();
  const [deleting, setDeleting] = useState(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ["users"],
    queryFn: () => usersApi.list(),
  });

  const removeMut = useMutation({
    mutationFn: (id) => usersApi.remove(id),
    onSuccess: () => {
      toast.success("User removed successfully");
      qc.invalidateQueries({ queryKey: ["users"] });
      setDeleting(null);
    },
    onError: (e) => toast.error(e.message),
  });

  return (
    <div className="container-fluid py-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="h4 fw-bold mb-1">User Management</h2>
          <p className="text-muted mb-0">Monitor and manage customer accounts.</p>
        </div>
        <Button variant="primary" className="d-flex align-items-center" disabled>
          <UserPlus size={18} className="me-2" /> Add New User
        </Button>
      </div>

      <Card className="shadow-sm border-0">
        <div className="table-responsive">
          <Table hover className="mb-0 align-middle">
            <thead className="bg-light">
              <tr>
                <th className="ps-4">Username</th>
                <th>Full Name</th>
                <th>Email</th>
                <th className="text-end pe-4">Actions</th>
              </tr>
            </thead>
            <tbody>
              {isLoading && (
                <tr>
                  <td colSpan={4} className="text-center py-5 text-muted">
                    Loading users...
                  </td>
                </tr>
              )}

              {error && (
                <tr>
                  <td colSpan={4} className="text-center py-5 text-danger">
                    {error.message}
                  </td>
                </tr>
              )}

              {data?.map((user) => (
                <tr key={user.id}>
                  <td className="ps-4">
                    <div className="d-flex align-items-center">
                      <div className="bg-soft-primary p-2 rounded-circle me-3">
                        <Shield size={16} className="text-primary" />
                      </div>
                      <span className="fw-semibold">{user.userName}</span>
                    </div>
                  </td>
                  <td className="text-muted">
                    {user.fullName || (
                      <span className="fst-italic text-opacity-50">Not updated</span>
                    )}
                  </td>
                  <td>{user.email}</td>
                  <td className="text-end pe-4">
                    <Button size="sm" variant="outline-danger" onClick={() => setDeleting(user)}>
                      <Trash2 size={16} />
                    </Button>
                  </td>
                </tr>
              ))}

              {!isLoading && data.length === 0 && (
                <tr>
                  <td colSpan={4} className="text-center py-5 text-muted">
                    No users found.
                  </td>
                </tr>
              )}
            </tbody>
          </Table>
        </div>
      </Card>

      {/* Modal xác nhận xóa */}
      <Modal show={!!deleting} onHide={() => setDeleting(null)} centered borderless>
        <Modal.Header closeButton className="border-0">
          <Modal.Title className="h5">Confirm Deletion</Modal.Title>
        </Modal.Header>
        <Modal.Body className="py-3">
          Bạn có chắc chắn muốn xóa tài khoản <strong>{deleting?.userName}</strong>?
          <br />
          <small className="text-danger">* Hành động này không thể hoàn tác.</small>
        </Modal.Body>
        <Modal.Footer className="border-0">
          <Button variant="light" onClick={() => setDeleting(null)}>
            Cancel
          </Button>
          <Button
            variant="danger"
            onClick={() => deleting && removeMut.mutate(deleting.id)}
            disabled={removeMut.isPending}>
            {removeMut.isPending ? "Processing..." : "Delete User"}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
}
