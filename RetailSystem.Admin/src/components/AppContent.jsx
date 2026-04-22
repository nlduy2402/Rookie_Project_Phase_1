import React from "react";
import routes from "../routes";

const AppContent = () => {
  return (
    <Routes>
      {routes.map(
        (route, idx) =>
          route.element && <Route key={idx} path={route.path} element={<route.element />} />,
      )}
    </Routes>
  );
};

export default AppContent;
