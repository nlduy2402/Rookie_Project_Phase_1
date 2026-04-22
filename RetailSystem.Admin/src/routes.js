import React from "react";

const CategoryList = React.lazy(() => import("./pages/categories/CategoryList"));

const routes = [{ path: "/category", name: "Category", element: CategoryList }];

export default routes;
