import React, { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { getAccountFavorites, submitUpgradeRequest } from '../../api/accounts';
import { AccountFavoritesDTO } from '../../types';
import { Link } from 'react-router-dom';

const AccountProfile: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const [favorites, setFavorites] = useState<AccountFavoritesDTO | null>(null);
  const [loading, setLoading] = useState(true);
  const [requestStatus, setRequestStatus] = useState(user?.upgradeRequest || false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const fetchFavorites = async () => {
      if (!user) return;
      try {
        const data = await getAccountFavorites(user.id);
        setFavorites(data);
      } catch (error) {
        console.error('Failed to fetch favorites:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchFavorites();
  }, [user]);

  const handleUpgradeRequest = async () => {
    if (!user) return;
    try {
      setIsSubmitting(true);
      await submitUpgradeRequest(user.id);
      setRequestStatus(true);
    } catch (error) {
      console.error('Failed to submit upgrade request:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isAuthenticated) {
    return <div className="container mx-auto px-4 py-8">Пожалуйста, войдите в систему</div>;
  }

  if (loading) {
    return <div className="container mx-auto px-4 py-8">Загрузка...</div>;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">Профиль пользователя</h1>
      
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Основная информация</h2>
        <p><strong>Имя пользователя:</strong> {user?.username}</p>
        <p><strong>Статус аккаунта:</strong> {user?.accessLevel === 'Admin' ? 'Администратор' : 'Пользователь'}</p>
        <p><strong>Заявка на повышение:</strong> {requestStatus ? 'Да' : 'Нет'}</p>
        <p>
          <strong>Последний просмотр избранного:</strong>{" "}
          {user?.LastFavoritesViewDate
            ? new Date(user?.LastFavoritesViewDate).toLocaleString()
            : "Еще не просматривалось"}
        </p>
        
        <div className="mt-4 flex flex-wrap gap-4">
          <Link 
            to="/favorites" 
            className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 transition-colors"
          >
            Избранные
          </Link>
          
          {user?.accessLevel === 'User' && !requestStatus && (
            <button
              onClick={handleUpgradeRequest}
              disabled={isSubmitting}
              className="bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600 transition-colors disabled:opacity-50"
            >
              {isSubmitting ? 'Отправка...' : 'Подать заявку на повышение статуса'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default AccountProfile;