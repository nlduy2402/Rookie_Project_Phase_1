import React, { useState } from "react";
import { Outlet, NavLink, useLocation } from "react-router-dom";
import { Offcanvas, Button, Dropdown, Badge, Container } from "react-bootstrap";
import {
  LayoutDashboard,
  Package,
  Tags,
  Settings,
  Menu,
  ChartNoAxesGantt,
  UserCircle2Icon,
  Bell,
  User,
  LogOut,
  ShieldCheck,
  ChevronLeft,
  ChevronRight,
} from "lucide-react";
import { toast } from "react-hot-toast";

const items = [
  { title: "Dashboard", url: "/", icon: LayoutDashboard, end: true },
  { title: "Products", url: "/products", icon: Package },
  { title: "Categories", url: "/categories", icon: Tags },
  { title: "Orders", url: "/orders", icon: ChartNoAxesGantt },
  { title: "Users", url: "/users", icon: UserCircle2Icon },
  { title: "Settings", url: "/settings", icon: Settings },
];

const titles = {
  "/": "Dashboard",
  "/products": "Products",
  "/categories": "Categories",
  "/orders": "Orders",
  "/users": "Users",
  "/settings": "Settings",
};

export default function AppLayout() {
  const { pathname } = useLocation();
  const [showMobile, setShowMobile] = useState(false);
  const [isCollapsed, setIsCollapsed] = useState(false); // Toggle cho Desktop

  const currentTitle = titles[pathname] ?? "Admin Panel";
  const sidebarWidth = isCollapsed ? "80px" : "260px";

  const handleLogout = () => {
    // 1. Xóa token
    localStorage.removeItem("token");

    // 2. Thông báo
    toast.success("Logged out successfully");

    // 3. Chuyển hướng (Dùng window.location để reset toàn bộ state của app cho an toàn)
    window.location.href = "/login";
  };

  // Component Sidebar dùng chung
  const SidebarContent = ({ mobile = false }) => (
    <div
      className={`d-flex flex-column h-100 bg-dark text-white ${!mobile && isCollapsed ? "align-items-center" : ""}`}>
      <div
        className="brand mb-4 px-3 pt-4 d-flex align-items-center gap-2"
        style={{ height: "64px" }}>
        <div className="bg-primary rounded p-1 shadow-sm">
          <ShieldCheck size={24} className="text-white" />
        </div>
        {(!isCollapsed || mobile) && (
          <span className="fw-bold fs-5 overflow-hidden text-nowrap">RetailSystem</span>
        )}
      </div>

      <nav className="flex-grow-1 px-2">
        {items.map((it) => (
          <NavLink
            key={it.url}
            to={it.url}
            end={it.end}
            className={({ isActive }) =>
              `nav-link d-flex align-items-center gap-3 px-3 py-2 rounded mb-1 transition-all ${
                isActive ? "bg-primary text-white shadow-sm" : "text-gray-400 hover-bg-dark"
              } ${!mobile && isCollapsed ? "justify-content-center px-0" : ""}`
            }
            title={isCollapsed ? it.title : ""}>
            <it.icon size={20} />
            {(!isCollapsed || mobile) && <span>{it.title}</span>}
          </NavLink>
        ))}
      </nav>

      <div className="p-3 border-top border-secondary mt-auto text-center">
        {!isCollapsed || mobile ? (
          <small className="text-muted">v1.0.2</small>
        ) : (
          <small className="text-muted">v1</small>
        )}
      </div>
    </div>
  );

  return (
    <div className="min-vh-100 bg-light">
      {/* SIDEBAR - DESKTOP */}
      <aside
        className="app-sidebar d-none d-md-block shadow-lg bg-dark transition-all"
        style={{
          width: sidebarWidth,
          position: "fixed",
          height: "100vh",
          zIndex: 1050,
          overflowX: "hidden",
        }}>
        <SidebarContent />
        <button
          onClick={() => setIsCollapsed(!isCollapsed)}
          className="btn btn-primary btn-sm rounded-circle position-absolute d-flex align-items-center justify-content-center shadow"
          style={{ right: "-12px", top: "70px", width: "24px", height: "24px", zIndex: 1100 }}>
          {isCollapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
        </button>
      </aside>

      {/* SIDEBAR - MOBILE */}
      <Offcanvas
        show={showMobile}
        onHide={() => setShowMobile(false)}
        className="bg-dark text-white"
        style={{ width: "280px" }}>
        <SidebarContent mobile />
      </Offcanvas>

      {/* MAIN CONTENT AREA */}
      <div
        className="app-main d-flex flex-column transition-all"
        style={{
          marginLeft: sidebarWidth, 
          minHeight: "100vh",
        }}>
        {/* HEADER */}
        <header
          className="app-header d-flex align-items-center justify-content-between px-4 bg-white border-bottom sticky-top shadow-sm"
          style={{ height: "64px" }}>
          <div className="d-flex align-items-center gap-3">
            <Button
              variant="light"
              size="sm"
              className="d-md-none"
              onClick={() => setShowMobile(true)}>
              <Menu size={20} />
            </Button>
            <h6 className="mb-0 fw-bold text-dark">{currentTitle}</h6>
          </div>

          <div className="d-flex align-items-center gap-3">
            {/* Notification */}
            <Dropdown align="end">
              <Dropdown.Toggle
                variant="light"
                className="p-2 rounded-circle border-0 bg-transparent no-caret shadow-none">
                <div className="position-relative">
                  <Bell size={20} className="text-muted" />
                  <Badge
                    bg="danger"
                    pill
                    className="position-absolute top-0 start-100 translate-middle border border-white"
                    style={{ fontSize: "10px" }}>
                    2
                  </Badge>
                </div>
              </Dropdown.Toggle>
              <Dropdown.Menu className="shadow border-0 mt-2 py-2" style={{ width: "280px" }}>
                <Dropdown.Header className="fw-bold">Notifications</Dropdown.Header>
                <Dropdown.Item className="py-2 border-bottom small">
                  New Order #1234 from Duy
                </Dropdown.Item>
                <Dropdown.Item className="text-center small text-primary py-2">
                  View All
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>

            <div className="vr d-none d-sm-block mx-2" style={{ height: "20px" }}></div>

            {/* Account */}
            <Dropdown align="end">
              <Dropdown.Toggle
                variant="light"
                id="dropdown-user"
                className="d-flex align-items-center gap-2 border-0 bg-transparent py-1 px-2 rounded-pill shadow-none">
                <div
                  className="bg-primary text-white rounded-circle d-flex align-items-center justify-content-center shadow-sm"
                  style={{ width: 34, height: 34 }}>
                  <User size={18} />
                </div>
                <div className="d-none d-lg-block text-start">
                  <div className="fw-bold small mb-0" style={{ lineHeight: 1.2 }}>
                    Administrator
                  </div>
                  <small className="text-muted" style={{ fontSize: "11px" }}>
                    admin@retail.com
                  </small>
                </div>
              </Dropdown.Toggle>
              <Dropdown.Menu className="shadow border-0 mt-2 p-2" style={{ minWidth: "200px" }}>
                <Dropdown.Item className="rounded py-2 d-flex align-items-center gap-3">
                  <UserCircle2Icon size={16} /> Profile
                </Dropdown.Item>
                <Dropdown.Divider />
                <Dropdown.Item
                  onClick={handleLogout}
                  className="rounded py-2 d-flex align-items-center gap-3 text-danger">
                  <LogOut size={16} /> Logout
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
          </div>
        </header>

        {/* MAIN PAGE */}
        <main className="p-4 flex-grow-1">
          <Container fluid className="p-0">
            <Outlet />
          </Container>
        </main>

        <footer className="px-4 py-3 bg-white border-top text-muted small d-flex justify-content-between mt-auto">
          <span>&copy; 2026 RetailSystem</span>
          <span className="d-none d-sm-block">v1.0.2 Beta</span>
        </footer>
      </div>

      <style>{`
        .transition-all { transition: all 0.3s ease-in-out; }
        .nav-link { color: #adb5bd; text-decoration: none; }
        .nav-link:hover { color: #fff; background: rgba(255,255,255,0.05); }
        .no-caret::after { display: none !important; }
        .hover-bg-dark:hover { background: rgba(255,255,255,0.1); }
        
        /* Fix tràn layout cho Mobile */
        @media (max-width: 768px) {
          .app-main { margin-left: 0 !important; }
        }
      `}</style>
    </div>
  );
}
