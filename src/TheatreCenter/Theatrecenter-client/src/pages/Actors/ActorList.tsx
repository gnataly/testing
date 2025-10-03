import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getAllActors, deleteActor } from '../../api/actors';
import { VoiceType, Gender, voiceTypeTranslations, genderTranslations } from '../../types';
import { formatDate } from '../../utils/dateUtils';

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

const ActorList: React.FC = () => {
  const [actors, setActors] = useState<Actor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchActors = async () => {
      try {
        const data = await getAllActors();
        setActors(data);
      } catch (err) {
        setError('Ошибка при загрузке актеров');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchActors();
  }, []);

  const handleDelete = async (id: number) => {
    try {
      await deleteActor(id);
      setActors(actors.filter(actor => actor.id !== id));
    } catch (err) {
      setError('Ошибка при удалении актера');
      console.error(err);
    }
  };

  if (loading) return <div className="p-4">Загрузка...</div>;
  if (error) return <div className="p-4 text-red-500">{error}</div>;
  if (actors.length === 0) return <div className="p-4">Актеры не найдены</div>;


  return (
    <div className="w-full">
      {/* Заголовок страницы вверху */}
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-xl font-bold">Актеры</h1>
        <Link 
          to="/actors/create" 
          className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600 transition-colors"
        >
          Добавить актера
        </Link>
      </div>
      
      {/* Таблица актеров */}
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100">
              <th className="py-2 px-4 border text-left text-sm">ID</th>
              <th className="py-2 px-4 border text-left text-sm">Имя</th>
              <th className="py-2 px-4 border text-left text-sm">Тип голоса</th>
              <th className="py-2 px-4 border text-left text-sm">Пол</th>
              <th className="py-2 px-4 border text-left text-sm">Дата рождения</th>
              <th className="py-2 px-4 border text-left text-sm">Действия</th>
              
            </tr>
          </thead>
          <tbody>
            {actors.map(actor => (
              <tr key={actor.id} className="hover:bg-gray-50">
                <td className="py-2 px-4 border text-sm">{actor.id}</td>
                <td className="py-2 px-4 border text-sm">
                  <Link to={`/actors/${actor.id}`} className="text-blue-500 hover:underline">
                    {actor.name}
                  </Link>
                </td>
                <td className="py-2 px-4 border text-sm">{voiceTypeTranslations[actor.voiceType]}</td>
                <td className="py-2 px-4 border text-sm">{genderTranslations[actor.gender]}</td>
                <td className="py-2 px-4 border text-sm">
                  {formatDate(actor.birthDate)}
                </td>
                <td className="py-2 px-4 border text-sm">
                  <div className="flex space-x-2">
                    <Link
                      to={`/actors/${actor.id}/edit`}
                      className="bg-yellow-500 text-white px-2 py-1 rounded text-xs hover:bg-yellow-600 transition-colors"
                    >
                      Редактировать
                    </Link>
                    <button
                      onClick={() => handleDelete(actor.id)}
                      className="bg-red-500 text-white px-2 py-1 rounded text-xs hover:bg-red-600 transition-colors"
                    >
                      Удалить
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default ActorList;