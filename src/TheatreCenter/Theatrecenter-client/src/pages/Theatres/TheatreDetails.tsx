import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getTheatreById } from '../../api/theatres';
import { addFavoriteTheatre, removeFavoriteTheatre } from '../../api/accounts';
import { useAuth } from '../../context/AuthContext';

const TheatreDetails: React.FC = () => {
  const { id } = useParams();
  const [theatre, setTheatre] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [isFavorite, setIsFavorite] = useState(false);
  const { user, isAuthenticated } = useAuth();

  useEffect(() => {
    const fetchTheatre = async () => {
      try {
        const data = await getTheatreById(Number(id));
        setTheatre(data);
        setIsFavorite(false);
      } catch (error) {
        console.error('Error fetching theatre:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchTheatre();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (!theatre) return <div>Theatre not found</div>;

  const handleFavoriteToggle = async () => {
    if (!user || !theatre) return;
    
    try {
      if (isFavorite) {
        await removeFavoriteTheatre({ accountId: user.id, targetId: theatre.id });
      } else {
        await addFavoriteTheatre({ accountId: user.id, targetId: theatre.id });
      }
      setIsFavorite(!isFavorite);
    } catch (error) {
      console.error('Error updating favorite:', error);
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-4">{theatre.name}</h1>
      {isAuthenticated && (
          <button 
            onClick={handleFavoriteToggle}
            className="text-2xl focus:outline-none"
            aria-label={isFavorite ? "Удалить из избранного" : "Добавить в избранное"}
          >
            {isFavorite ? '❤️' : '🤍'}
          </button>
        )}
      <div className="grid grid-cols-1 gap-4">
        {theatre.addInfo && (
          <div>
            <h2 className="text-lg font-semibold mb-2">Дополнительная информация</h2>
            <p>{theatre.addInfo}</p>
          </div>
        )}
      </div>
      <div className="mt-6">
        <Link 
            to={`/theatres/${id}/musicals`}
            className="text-blue-500 hover:underline"
        >
            Посмотреть все мюзиклы этого театра
        </Link>
        </div>
    </div>
  );
};

export default TheatreDetails;