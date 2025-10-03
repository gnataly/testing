import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getActorById } from '../../api/actors';
import { VoiceType, Gender, voiceTypeTranslations, genderTranslations } from '../../types';
import { formatDate } from '../../utils/dateUtils';
import { addFavoriteActor, removeFavoriteActor, checkIsFavorite } from '../../api/accounts';
import { useAuth } from '../../context/AuthContext';

interface Actor {
  id: number;
  name: string;
  voiceType: VoiceType;
  gender: Gender;
  birthDate: string;
  height: number;
  weight: number;
  addInfo?: string;
}


const ActorDetails: React.FC = () => {
  const { id } = useParams();
  const [actor, setActor] = useState<Actor | null>(null);
  const [loading, setLoading] = useState(true);
  const [isFavorite, setIsFavorite] = useState(false);
  const { user, isAuthenticated } = useAuth();

  useEffect(() => {
    const fetchActor = async () => {
      try {
        const data = await getActorById(Number(id));
        setActor(data);
        if (user && isAuthenticated) {
          const favoriteStatus = await checkIsFavorite({
            accountId: user.id,
            targetId: Number(id),
            type: 'actor'
          });
          setIsFavorite(favoriteStatus);
        }
      } catch (error) {
        console.error('Error fetching actor:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchActor();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (!actor) return <div>Actor not found</div>;

    const handleFavoriteToggle = async () => {
    if (!user || !actor) return;
    
    try {
      if (isFavorite) {
        await removeFavoriteActor({ accountId: user.id, targetId: actor.id });
      } else {
        await addFavoriteActor({ accountId: user.id, targetId: actor.id });
      }
      setIsFavorite(!isFavorite);
    } catch (error) {
      console.error('Error updating favorite:', error);
    }
  };

  const getVoiceTypeTranslation = (type: VoiceType) => {
    return voiceTypeTranslations[type] || type;
  };

  const getGenderTranslation = (gender: Gender) => {
    return genderTranslations[gender] || gender;
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-4">{actor.name}</h1>
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
          <p><strong>Тип голоса:</strong> {getVoiceTypeTranslation(actor.voiceType)}</p>
          <p><strong>Пол:</strong> {getGenderTranslation(actor.gender)}</p>
        </div>
        <div>
          <p><strong>Дата рождения:</strong> {formatDate(actor.birthDate)}</p>
          <p><strong>Рост:</strong> {actor.height} см</p>
          <p><strong>Вес:</strong> {actor.weight} кг</p>
        </div>
      </div>
      {actor.addInfo && (
        <div className="mt-4">
          <h2 className="text-xl font-semibold mb-2">Дополнительная информация</h2>
          <p>{actor.addInfo}</p>
        </div>
      )}
    </div>
  );
};

export default ActorDetails;