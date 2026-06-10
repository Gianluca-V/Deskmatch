import { createContext, useContext, useState } from 'react';
import api, { STORAGE_KEY } from '../lib/api';

const AuthContext = createContext();

const loadSession = () => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
};

export const AuthProvider = ({ children }) => {
  const [session, setSession] = useState(() => loadSession());

  const login = (loginResponse) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(loginResponse));
    setSession(loginResponse);
  };

  const logout = async () => {
    try {
      await api.post('/api/auth/logout');
    } catch {
      // limpiar sesión local aunque el endpoint falle
    }
    localStorage.removeItem(STORAGE_KEY);
    setSession(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user: session?.user ?? null,
        session,
        login,
        logout,
        isAuthenticated: !!session?.accessToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return context;
};

export default AuthContext;
