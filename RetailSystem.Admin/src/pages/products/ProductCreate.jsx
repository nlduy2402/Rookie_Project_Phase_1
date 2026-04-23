import {
  Create,
  SimpleForm,
  TextInput,
  NumberInput,
  ReferenceInput,
  SelectInput,
  ArrayInput,
  SimpleFormIterator,
} from "react-admin";

const ProductCreate = () => {
  return (
    <Create redirect="list">
      <SimpleForm>
        {/* BASIC */}
        <TextInput source="name" fullWidth required />
        <TextInput source="description" fullWidth multiline />

        {/* PRICE */}
        <NumberInput source="price" />
        <NumberInput source="quantity" />

        {/* SPEC */}
        <TextInput source="chipSet" fullWidth />
        <TextInput source="ram" fullWidth />
        <TextInput source="ssd" fullWidth />

        {/* CATEGORY */}
        <ReferenceInput source="categoryId" reference="categories">
          <SelectInput optionText="name" />
        </ReferenceInput>

        {/* 🔥 IMAGES (QUAN TRỌNG) */}
        <ArrayInput source="imageUrls">
          <SimpleFormIterator>
            <TextInput label="Image URL" />
          </SimpleFormIterator>
        </ArrayInput>
      </SimpleForm>
    </Create>
  );
};

export default ProductCreate;
