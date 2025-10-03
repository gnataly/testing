import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen flex flex-col">
      {/* Fixed Header */}
      <header className="fixed top-0 left-0 right-0 z-50 bg-primary-bg shadow-lg">
        <div className="container mx-auto px-4 py-3 flex items-center">
          {/* Логотип (центрированный) */}
          <div className="flex-1 text-center">
            <Link 
              to="/" 
              className="text-3xl font-bold text-accent hover:text-text-primary transition-colors"
            >
              Театральный центр
            </Link>
          </div>

          {/* Блок авторизации (правый край) */}
          <div className="flex items-center space-x-4">
            {isAuthenticated ? (
              <>
                <span className="text-text-secondary hidden md:inline">Привет, {user?.username}</span>
                <Link
                  to="/profile"
                  className={`px-3 py-2 rounded transition-colors ${
                    location.pathname === '/profile' 
                      ? 'bg-highlight text-text-primary' 
                      : 'text-accent hover:bg-secondary-bg'
                  }`}
                >
                  Мой профиль
                </Link>
                <button
                  onClick={handleLogout}
                  className="px-3 py-2 bg-highlight hover:bg-highlight-light text-text-primary rounded transition-colors"
                >
                  Выйти
                </button>
              </>
            ) : (
              <>
                <Link
                  to="/login"
                  className={`px-3 py-2 rounded transition-colors ${
                    location.pathname === '/login' 
                      ? 'bg-highlight text-text-primary' 
                      : 'text-accent hover:bg-secondary-bg'
                  }`}
                >
                  Войти
                </Link>
                <Link
                  to="/register"
                  className={`px-3 py-2 rounded transition-colors ${
                    location.pathname === '/register' 
                      ? 'bg-highlight text-text-primary' 
                      : 'text-accent hover:bg-secondary-bg'
                  }`}
                >
                  Регистрация
                </Link>
              </>
            )}
          </div>
        </div>
      </header>

      {/* Навигационная панель */}
      <nav className="fixed top-16 left-0 right-0 z-40 bg-secondary-bg shadow-md">
        <div className="container mx-auto px-4 py-2 flex justify-center space-x-6">
          <Link 
            to="/actors" 
            className={`px-4 py-2 rounded transition-colors ${
              location.pathname.startsWith('/actors') 
                ? 'bg-highlight text-text-primary' 
                : 'text-accent hover:bg-primary-bg'
            }`}
          >
            Актеры
          </Link>
          <Link 
            to="/musicals" 
            className={`px-4 py-2 rounded transition-colors ${
              location.pathname.startsWith('/musicals') 
                ? 'bg-highlight text-text-primary' 
                : 'text-accent hover:bg-primary-bg'
            }`}
          >
            Мюзиклы
          </Link>
          <Link 
            to="/theatres" 
            className={`px-4 py-2 rounded transition-colors ${
              location.pathname.startsWith('/theatres') 
                ? 'bg-highlight text-text-primary' 
                : 'text-accent hover:bg-primary-bg'
            }`}
          >
            Театры
          </Link>
          <Link 
            to="/shows" 
            className={`px-4 py-2 rounded transition-colors ${
              location.pathname.startsWith('/shows') 
                ? 'bg-highlight text-text-primary' 
                : 'text-accent hover:bg-primary-bg'
            }`}
          >
            Показы
          </Link>


          {user?.accessLevel === 'Admin' && (
            <Link 
              to="/admin/upgrade-requests" 
              className={`px-4 py-2 rounded transition-colors ${
                location.pathname.startsWith('/admin/upgrade-requests') 
                  ? 'bg-highlight text-text-primary' 
                  : 'text-accent hover:bg-primary-bg'
              }`}
            >
              Заявки на повышение
            </Link>
          )}

        </div>
        
      </nav>
      

      {/* Основное содержимое с центрированными заголовками */}
      <main className="flex-1 pt-32 px-4">
        <div className="max-w-6xl mx-auto text-center">
          {children}
        </div>
      </main>
    </div>
  );
};

export default Layout;