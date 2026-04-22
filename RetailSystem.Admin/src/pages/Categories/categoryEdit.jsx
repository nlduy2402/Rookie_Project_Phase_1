import { Edit, SimpleForm, TextInput, required } from "react-admin";

const CategoryEdit = () => (
  <Edit>
    <SimpleForm>
      {/* Không cho sửa id */}
      <TextInput source="id" disabled />

      {/* Các field cần sửa */}
      <TextInput source="name" label="Name" validate={required()} />
      <TextInput source="description" label="Description" multiline />
    </SimpleForm>
  </Edit>
);

export default CategoryEdit;
