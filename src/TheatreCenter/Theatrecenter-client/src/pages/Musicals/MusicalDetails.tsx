import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getMusicalById } from '../../api/musicals';
import { AgeRestriction, ageRestrictionTranslations } from '../../types';
import { formatDuration } from '../../utils/durationUtils';
import { addFavoriteMusical, removeFavoriteMusical } from '../../api/accounts';
import { useAuth } from '../../context/AuthContext';

const MusicalDetails: React.FC = () => {
  const { id } = useParams();
  const [musical, setMusical] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [isFavorite, setIsFavorite] = useState(false);
  const { user, isAuthenticated } = useAuth();

  useEffect(() => {
    const fetchMusical = async () => {
      try {
        const data = await getMusicalById(Number(id));
        setMusical(data);
      } catch (error) {
        console.error('Error fetching musical:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMusical();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (!musical) return <div>Musical not found</div>;

      const handleFavoriteToggle = async () => {
      if (!user || !musical) return;
      
      try {
        if (isFavorite) {
          await removeFavoriteMusical({ accountId: user.id, targetId: musical.id });
        } else {
          await addFavoriteMusical({ accountId: user.id, targetId: musical.id });
        }
        setIsFavorite(!isFavorite);
      } catch (error) {
        console.error('Error updating favorite:', error);
      }
    };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-4">{musical.title}</h1>
      {isAuthenticated && (
          <button 
            onClick={handleFavoriteToggle}
            className="text-2xl focus:outline-none"
            aria-label={isFavorite ? "Удалить из избранного" : "Добавить в избранное"}
          >
            {isFavorite ? '❤️' : '🤍'}
          </button>
        )}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <p><strong>Описание:</strong> {musical.description}</p>
          <p><strong>Длительность:</strong> {formatDuration(musical.duration)}</p>
        </div>
        <div>
          <p><strong>Возрастное ограничение:</strong> {ageRestrictionTranslations[musical.ageRestriction as AgeRestriction]}</p>
          <p><strong>ID театра:</strong> {musical.theatreId}</p>
        </div>
        <div className="mt-6">
            <Link 
                to={`/musicals/${id}/shows`}
                className="text-blue-500 hover:underline"
            >
                Посмотреть все показы этого мюзикла
            </Link>
        </div>
      </div>
    </div>
  );
};

export default MusicalDetails;