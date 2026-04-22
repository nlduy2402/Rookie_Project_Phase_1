import { List, Datagrid, TextField, EditButton, DeleteButton, ShowButton } from "react-admin";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

const ActionButtons = () => (
  <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
    <EditButton label="" icon={<EditIcon />} />
    <DeleteButton label="" icon={<DeleteIcon />} />
  </div>
);

const CategoryList = () => (
  <List>
    <Datagrid>
      <TextField source="name" />
      <TextField source="description" />
      <TextField
        style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}
        source="action"
      />
      <ActionButtons />
    </Datagrid>
  </List>
);

export default CategoryList;
