import { List, Datagrid, TextField } from "react-admin";

export const UserList = () => (
  <List>
    <Datagrid>
      <TextField source="id" />
      <TextField source="name" />
      <TextField source="email" />
      <TextField source="address.city" />
      <TextField source="company.name" />
    </Datagrid>
  </List>
);
