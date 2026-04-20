import React, { useState, useEffect } from "react";
import { Layout, Menu, Table, Form, Input, Button, Modal, Space } from "antd";
import { PlusOutlined, EditOutlined, DeleteOutlined } from "@ant-design/icons";
import axios from "axios";

const { Header, Content, Footer } = Layout;

const App = () => {
  const [categories, setCategories] = useState([]); // Đổi users thành categories cho đúng nghĩa
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [form] = Form.useForm();

  useEffect(() => {
    fetchCategories();
  }, []);

  useEffect(() => {
    if (currentUser) {
      form.setFieldsValue(currentUser);
    } else {
      form.resetFields();
    }
  }, [currentUser, form]);

  const fetchCategories = async () => {
    try {
      const response = await axios.get("https://localhost:7260/api/Categories");
      setCategories(response.data);
    } catch (error) {
      console.error("Lỗi gọi API:", error);
    }
  };

  const handleAddOrEditCategory = async (values) => {
    try {
      if (currentUser) {
        await axios.put(`https://localhost:7260/api/Categories/${currentUser.id}`, values);
      } else {
        await axios.post("https://localhost:7260/api/Categories", values);
      }
      setIsModalVisible(false);
      setCurrentUser(null);
      fetchCategories();
    } catch (error) {
      console.error("Lỗi lưu dữ liệu:", error);
    }
  };

  const columns = [
    { title: "Name", dataIndex: "name", key: "name" },
    { title: "Description", dataIndex: "description", key: "description" },
    {
      title: "Action",
      key: "action",
      render: (_, record) => (
        <Space>
          <Button
            icon={<EditOutlined />}
            onClick={() => {
              setCurrentUser(record);
              setIsModalVisible(true);
            }}>
            Edit
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <Layout style={{ minHeight: "100vh", width: "100%" }}>
      <Header style={{ width: "100%" }}>
        <Menu
          theme="dark"
          mode="horizontal"
          items={[
            { key: "home", label: "Home" },
            { key: "contact", label: "Contact" },
          ]}
        />
      </Header>

      <Content style={{ padding: "20px", width: "100%" }}>
        <div style={{ background: "#fff", padding: "24px", minHeight: "280px" }}>
          <Space style={{ marginBottom: "16px", display: "flex", justifyContent: "flex-end" }}>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => {
                setCurrentUser(null);
                setIsModalVisible(true);
              }}>
              Add Category
            </Button>
          </Space>

          <Table
            dataSource={categories}
            columns={columns}
            rowKey="id"
            pagination={{ pageSize: 10 }}
            style={{ width: "100%" }}
          />
        </div>

        {/* Modal và Form giữ nguyên... */}
      </Content>

      <Footer style={{ textAlign: "center" }}>Your Company ©2026</Footer>
    </Layout>
  );
};

export default App;
