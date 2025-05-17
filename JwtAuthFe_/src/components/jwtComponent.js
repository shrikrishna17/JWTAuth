// src/App.js
import React, { useState } from 'react';
import { login, register, getProtectedData, getAdminOnly } from '../services/authService';

function JwtComponent() {
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');

  const handleLogin = async () => {
    try {
      await login(userName, password);
      setMessage("Login successful");
    } catch (err) {
      setMessage("Login failed: " + err.message);
    }
  };

  const handleRegister = async () => {
    try {
      await register(userName, password);
      setMessage("Registration successful");
    } catch (err) {
      setMessage("Registration failed: " + err.message);
    }
  };

  const handleAuthenticatedCall = async () => {
    try {
      const result = await getProtectedData();
      setMessage(result);
    } catch (err) {
      setMessage("Error: " + err.message);
    }
  };

  const handleAdminCall = async () => {
    try {
      const result = await getAdminOnly();
      setMessage(result);
    } catch (err) {
      setMessage("Error: " + err.message);
    }
  };

  return (
    <div style={{ padding: 40 }}>
      <h2>JWT Auth Test (with Refresh)</h2>
      <input
        placeholder="Username"
        value={userName}
        onChange={(e) => setUserName(e.target.value)}
      /><br />
      <input
        placeholder="Password"
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      /><br />
      <button onClick={handleLogin}>Login</button>
      <button onClick={handleRegister}>Register</button>
      <br /><br />
      <button onClick={handleAuthenticatedCall}>Authenticated Endpoint</button>
      <button onClick={handleAdminCall}>Admin-Only Endpoint</button>
      <p>{message}</p>
    </div>
  );
}

export default JwtComponent;
