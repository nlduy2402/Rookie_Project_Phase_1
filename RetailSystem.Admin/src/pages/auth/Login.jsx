import { useState } from "react";
import { useLogin, useNotify } from "react-admin";
import { Card, CardContent, TextField, Button, Typography } from "@mui/material";

const LoginPage = () => {
  const login = useLogin();
  const notify = useNotify();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      await login({ username, password });
    } catch (error) {
      notify("Login failed", { type: "error" });
    }
  };

  return (
    <div
      style={{
        height: "100vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        background: "#f4f6f8",
      }}>
      <Card style={{ width: 350, padding: 20 }}>
        <CardContent>
          <Typography variant="h5" gutterBottom>
            Admin Login
          </Typography>

          <form onSubmit={handleSubmit}>
            <TextField
              label="Username"
              fullWidth
              margin="normal"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />

            <TextField
              label="Password"
              type="password"
              fullWidth
              margin="normal"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />

            <Button type="submit" variant="contained" fullWidth style={{ marginTop: 16 }}>
              Login
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
};

export default LoginPage;
