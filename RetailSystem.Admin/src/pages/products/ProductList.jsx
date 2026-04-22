import {
  List,
  Datagrid,
  TextField,
  NumberField,
  ImageField,
  ArrayField,
  SingleFieldList,
  ChipField,
  EditButton,
} from "react-admin";

const ProductList = () => (
  <List>
    <Datagrid
      sx={{
        borderRadius: 2,

        "& .RaDatagrid-headerCell": {

          fontSize: "14px",
        },



        "& .RaDatagrid-rowCell": {
          borderBottom: "1px solid #333",
          overflow: "hidden",
        },
      }}>
      <TextField
        source="name"
        sx={{
          maxWidth: 200,
          whiteSpace: "nowrap",
          overflow: "hidden",
          textOverflow: "ellipsis",
        }}
      />
      <NumberField source="price" sx={{ textAlign: "center" }} />
      <NumberField source="quantity" sx={{ textAlign: "center" }} />

      {/* 👇 category nested */}
      <TextField source="category.name" label="Category" sx={{ textAlign: "center" }} />
      {/* 👇 hiển thị ảnh đầu tiên */}
      <ArrayField source="images">
        <SingleFieldList >
          <ImageField source="url" />
        </SingleFieldList>
      </ArrayField>

      <EditButton
        sx={{
          color: "#4dabf7",
          fontWeight: "bold",
        }}
      />
    </Datagrid>
  </List>
);

export default ProductList;
