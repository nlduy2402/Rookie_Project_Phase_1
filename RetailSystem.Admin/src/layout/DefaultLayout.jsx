import React from "react";
import AppSidebar from "../components/AppSidebar";
import AppHeader from "../components/AppHeader";
import AppContent from "../components/AppContent";

const DefaultLayout = () => {
  return (
    <div className="d-flex">
      <AppSidebar />
      <div className="flex-grow-1">
        <AppHeader />
        <AppContent />
      </div>
    </div>
  );
};

export default DefaultLayout;
