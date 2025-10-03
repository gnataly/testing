import React, { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { getAccountFavorites, updateLastFavoritesView } from '../../api/accounts';
import { AccountFavoritesDTO, FavoriteItemDTO } from '../../types';
import { formatDateTime } from '../../utils/dateUtils';
import { useNavigate } from 'react-router-dom';

const FavoritesPage: React.FC = () => {
  const { user } = useAuth();
  const [favorites, setFavorites] = useState<AccountFavoritesDTO | null>(null);
  const [loading, setLoading] = useState(true);
  const [lastViewDate, setLastViewDate] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (!user) {
      navigate('/login');
      return;
    }

    const fetchData = async () => {
      try {
        // Сохраняем текущую дату просмотра перед обновлением
        const currentLastView = user.LastFavoritesViewDate;
        setLastViewDate(currentLastView);
        
        // Обновляем дату просмотра на сервере
        await updateLastFavoritesView(user.id);
        
        // Загружаем избранное
        const data = await getAccountFavorites(user.id);
        setFavorites(data);
      } catch (error) {
        console.error('Failed to fetch favorites:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [user, navigate]);

  if (loading) {
    return <div className="container mx-auto px-4 py-8">Загрузка...</div>;
  }

  const renderFavoriteItem = (item: FavoriteItemDTO, type: string) => {
    const isUpdated = lastViewDate && new Date(item.lastModified) > new Date(lastViewDate);
    
    return (
      <tr key={`${type}-${item.id}`} className="hover:bg-gray-50">
        <td className="py-2 px-4 border">{item.id}</td>
        <td className="py-2 px-4 border">{item.name}</td>
        <td className="py-2 px-4 border capitalize">{type}</td>
        <td className={`py-2 px-4 border ${isUpdated ? 'text-green-600 font-bold' : ''}`}>
          {type === 'theatre' ? '-' : formatDateTime(item.lastModified)}
        </td>
      </tr>
    );
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">Избранное</h1>
      
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100">
              <th className="py-2 px-4 border text-left">ID</th>
              <th className="py-2 px-4 border text-left">Название</th>
              <th className="py-2 px-4 border text-left">Тип</th>
              <th className="py-2 px-4 border text-left">Дата изменения</th>
            </tr>
          </thead>
          <tbody>
            {favorites?.favoriteActors.map(item => renderFavoriteItem(item, 'actor'))}
            {favorites?.favoriteMusicals.map(item => renderFavoriteItem(item, 'musical'))}
            {favorites?.favoriteTheatres.map(item => renderFavoriteItem(item, 'theatre'))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default FavoritesPage;