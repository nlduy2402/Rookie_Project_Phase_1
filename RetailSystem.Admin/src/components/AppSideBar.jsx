import React from "react";
import navigation from "../_nav";

const AppSidebar = () => {
  return (
    <div style={{ width: "200px", background: "#eee", minHeight: "100vh" }}>
      {navigation.map((item, index) => (
        <div key={index} style={{ padding: "10px" }}>
          <Link to={item.to}>{item.name}</Link>
        </div>
      ))}
    </div>
  );
};

export default AppSidebar;
