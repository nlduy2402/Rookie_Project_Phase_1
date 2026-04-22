import { Edit, SimpleForm, TextInput, NumberInput, ReferenceInput, SelectInput } from "react-admin";

const transform = (data) => ({
  id: data.id,
  name: data.name ?? undefined,
  description: data.description ?? undefined,
  price: data.price ?? undefined,
  quantity: data.quantity ?? undefined,
  chipSet: data.chipSet ?? undefined,
  ram: data.ram ?? undefined,
  ssd: data.ssd ?? undefined,
  categoryId: data.categoryId?.id || data.categoryId,
});

const ProductEdit = () => (
  <Edit transform={transform}>
    <SimpleForm>
      <TextInput source="name" />
      <TextInput source="description" multiline />

      <NumberInput source="price" />
      <NumberInput source="quantity" />

      <TextInput source="chipSet" />
      <TextInput source="ram" />
      <TextInput source="ssd" />

      <ReferenceInput source="category.Id" reference="categories">
        <SelectInput optionText="name" />
      </ReferenceInput>
    </SimpleForm>
  </Edit>
);

export default ProductEdit;
