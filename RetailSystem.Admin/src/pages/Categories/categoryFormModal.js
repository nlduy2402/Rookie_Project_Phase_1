import React, { useState, useEffect } from "react";
import {
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CButton,
  CForm,
  CFormInput,
} from "@coreui/react";

const CategoryFormModal = ({ visible, onClose, onSubmit, selected }) => {
  const [name, setName] = useState("");

  useEffect(() => {
    setName(selected?.name || "");
  }, [selected]);

  const handleSubmit = () => {
    onSubmit({ name });
  };

  return (
    <CModal visible={visible} onClose={onClose}>
      <CModalHeader>
        <CModalTitle>{selected ? "Edit" : "Create"} Category</CModalTitle>
      </CModalHeader>

      <CModalBody>
        <CForm>
          <CFormInput
            label="Category Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </CForm>
      </CModalBody>

      <CModalFooter>
        <CButton color="secondary" onClick={onClose}>
          Cancel
        </CButton>
        <CButton color="primary" onClick={handleSubmit}>
          Save
        </CButton>
      </CModalFooter>
    </CModal>
  );
};

export default CategoryFormModal;
