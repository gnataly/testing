import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { AccountDTO } from '../types';
import { getAccountById } from '../api/accounts';

interface AuthContextType {
  user: AccountDTO | null;
  isAuthenticated: boolean;
  setUser: (user: AccountDTO | null) => void;
  logout: () => void;
  loading: boolean;
  error: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AccountDTO | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

useEffect(() => {
  const checkAuth = async () => {
    try {
      const token = localStorage.getItem('token');
      const userId = localStorage.getItem('userId'); 
      if (token && userId) {
        const currentUser = await getAccountById(Number(userId)); 
        setUser(currentUser);
      }
    } catch (err) {
      console.error('Auth check failed:', err);
      logout();
    } finally {
      setLoading(false);
    }
  };

  checkAuth();
}, []);

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId'); // Удаляем ID при выходе
    setUser(null);
  };

  const value = {
    user,
    isAuthenticated: !!user,
    setUser,
    logout,
    loading,
    error,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};