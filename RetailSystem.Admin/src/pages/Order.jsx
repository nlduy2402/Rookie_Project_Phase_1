import React, { useEffect, useState } from "react";
import {
  Card,
  Table,
  Badge,
  Row,
  Col,
  Container,
  Pagination,
  Spinner,
  Alert,
  Image,
} from "react-bootstrap";
import { ordersApi } from "../lib/api.js";
import dayjs from "dayjs";
import utc from "dayjs/plugin/utc";
dayjs.extend(utc);

const OrderPage = () => {
  const [data, setData] = useState(null); // Lưu PageResult<Order>
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 4;

  useEffect(() => {
    fetchOrders(currentPage);
  }, [currentPage]);

  const fetchOrders = async (page) => {
    setLoading(true);
    try {
      const response = await ordersApi.list(page, pageSize);
      // Giả sử request() trả về trực tiếp Data (PageResult)
      setData(response);
      setLoading(false);
    } catch (err) {
      setError("Cannot fetch orders.");
      setLoading(false);
    }
  };

  const handleShipOrder = async (orderId) => {
    if (!window.confirm(`Comfirm delivery order #${orderId} to customer?`)) {
      return;
    }

    try {
      const response = await ordersApi.ship(orderId);
      alert("Update order successfully!");

      fetchOrders(currentPage);
    } catch (err) {
      alert("Error: " + (err.message || "Cannot update order"));
    }
  };

  const getStatusBadge = (status) => {
    const statusMap = {
      Completed: { bg: "success", text: "Completed" },
      Cancelled: { bg: "danger", text: "Cancelled" },
      Processing: { bg: "info", text: "Processing" },
      Pending: { bg: "warning", text: "Pending" },
      Failed: { bg: "dark", text: "Failed" },
    };
    const config = statusMap[status] || { bg: "secondary", text: status };
    return <Badge bg={config.bg}>{config.text}</Badge>;
  };

  if (loading)
    return (
      <Container className="text-center py-5">
        <Spinner animation="border" variant="primary" />
      </Container>
    );

  if (error)
    return (
      <Container className="py-5">
        <Alert variant="danger">{error}</Alert>
      </Container>
    );

  return (
    <Container className="py-4">
      <h4 className="mb-4 fw-bold">Orders Management</h4>

      {data?.items?.map((order) => (
        <Card key={order.id} className="shadow-sm border-0 mb-4 overflow-hidden">
          <Card.Header className="bg-white border-bottom py-3">
            <Row className="align-items-center">
              <Col>
                <div className="d-flex align-items-center gap-2">
                  <span className="fw-bold fs-5 text-dark">Order #{order.id}</span>
                  {getStatusBadge(order.status)}
                </div>
                <small className="text-muted">
                  Time: {dayjs.utc(order.orderDate).local().format("DD/MM/YYYY HH:mm")}
                </small>
              </Col>
              <Col className="text-end">
                <span className="text-muted small">Customer:</span>
                <div className="fw-bold text-primary">{order.user?.userName || "Annonymus"}</div>
              </Col>
            </Row>
          </Card.Header>

          <Card.Body className="p-0">
            <Table responsive hover className="mb-0 align-middle">
              <thead className="bg-light">
                <tr>
                  <th className="ps-4" style={{ width: "100px" }}>
                    Image
                  </th>
                  <th className="ps-4">Product</th>
                  <th className="text-center">Quantity</th>
                  <th className="text-end pe-4">Price</th>
                </tr>
              </thead>
              <tbody>
                {order.orderDetails.map((item) => (
                  <tr key={item.id}>
                    <td className="ps-4">
                      {/* Hiển thị ảnh sản phẩm */}
                      <Image
                        src={item.product.images[0].url || "https://via.placeholder.com/60"}
                        rounded
                        style={{ width: "60px", height: "60px", objectFit: "cover" }}
                        alt="product"
                      />
                    </td>
                    <td className="ps-4 py-3">
                      <div className="fw-medium"> #{item.product.name}</div>
                    </td>
                    <td className="text-center">{item.quantity}</td>
                    <td className="text-end pe-4">{item.price.toLocaleString("vi-VN")} đ</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </Card.Body>

          <Card.Footer className="bg-white border-top py-3">
            <Row className="align-items-center">
              <Col xs={6}>
                <div className="small text-muted">Method: {order.paymentMethod}</div>
                <div
                  className={`small ${order.paymentStatus === "Failed" ? "text-danger" : "text-success"}`}>
                  Payment: {order.paymentStatus}
                </div>
              </Col>
              <Col xs={6} className="text-end">
                {order.status === "Processing" && (
                  <button
                    className="btn btn-outline-success"
                    onClick={() => handleShipOrder(order.id)}>
                    Shipped
                  </button>
                )}
                <div className="text-muted small">Total:</div>
                <div className="fs-4 fw-bold text-danger">
                  {order.totalAmount.toLocaleString("vi-VN")} đ
                </div>
              </Col>
            </Row>
          </Card.Footer>
        </Card>
      ))}

      {data && data.totalPages > 1 && (
        <div className="d-flex justify-content-center mt-4">
          <Pagination>
            <Pagination.First onClick={() => setCurrentPage(1)} disabled={currentPage === 1} />
            <Pagination.Prev
              onClick={() => setCurrentPage((prev) => prev - 1)}
              disabled={currentPage === 1}
            />

            {[...Array(data.totalPages).keys()].map((num) => (
              <Pagination.Item
                key={num + 1}
                active={num + 1 === currentPage}
                onClick={() => setCurrentPage(num + 1)}>
                {num + 1}
              </Pagination.Item>
            ))}

            <Pagination.Next
              onClick={() => setCurrentPage((prev) => prev + 1)}
              disabled={currentPage === data.totalPages}
            />
            <Pagination.Last
              onClick={() => setCurrentPage(data.totalPages)}
              disabled={currentPage === data.totalPages}
            />
          </Pagination>
        </div>
      )}
    </Container>
  );
};

export default OrderPage;
