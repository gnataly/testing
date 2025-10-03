import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getAllMusicals, deleteMusical } from '../../api/musicals';
import { AgeRestriction, ageRestrictionTranslations } from '../../types';
import { formatDuration } from '../../utils/durationUtils';

const MusicalList: React.FC = () => {
  const [musicals, setMusicals] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchMusicals = async () => {
      try {
        const data = await getAllMusicals();
        setMusicals(data);
      } catch (err) {
        setError('Ошибка при загрузке мюзиклов');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchMusicals();
  }, []);

  const handleDelete = async (id: number) => {
    try {
      await deleteMusical(id);
      setMusicals(musicals.filter(musical => musical.id !== id));
    } catch (err) {
      setError('Ошибка при удалении мюзикла');
      console.error(err);
    }
  };

  if (loading) return <div className="p-4">Загрузка...</div>;
  if (error) return <div className="p-4 text-red-500">{error}</div>;

  return (
    <div className="w-full">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-xl font-bold">Мюзиклы</h1>
        <Link 
          to="/musicals/create" 
          className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600 transition-colors"
        >
          Добавить мюзикл
        </Link>
      </div>
      
      {musicals.length === 0 ? (
        <div className="p-4 bg-gray-50 rounded-lg border border-gray-200 text-center">
          <p className="mb-4">Мюзиклы не найдены</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white border">
            <thead>
              <tr className="bg-gray-100">
                <th className="py-2 px-4 border text-left text-sm">ID</th>
                <th className="py-2 px-4 border text-left text-sm">Название</th>
                <th className="py-2 px-4 border text-left text-sm">Длительность</th>
                <th className="py-2 px-4 border text-left text-sm">Возрастное ограничение</th>
                <th className="py-2 px-4 border text-left text-sm">ID театра</th>
                <th className="py-2 px-4 border text-left text-sm">Действия</th>
                <th className="py-2 px-4 border text-left text-sm">Показы</th>
                <th className="py-2 px-4 border text-left text-sm">Роли</th>
              </tr>
            </thead>
            <tbody>
              {musicals.map(musical => (
                <tr key={musical.id} className="hover:bg-gray-50">
                  <td className="py-2 px-4 border text-sm">{musical.id}</td>
                  <td className="py-2 px-4 border text-sm">
                    <Link to={`/musicals/${musical.id}`} className="text-blue-500 hover:underline">
                      {musical.title}
                    </Link>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                    {formatDuration(musical.duration)}
                  </td>
                  <td className="py-2 px-4 border text-sm">{ageRestrictionTranslations[musical.ageRestriction as AgeRestriction]}</td>
                  <td className="py-2 px-4 border text-sm">
                    <Link to={`/theatres/${musical.theatreId}`} className="text-blue-500 hover:underline">
                      {musical.theatreId}
                    </Link>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                    <div className="flex space-x-2">
                      <Link
                        to={`/musicals/${musical.id}/edit`}
                        className="bg-yellow-500 text-white px-2 py-1 rounded text-xs hover:bg-yellow-600 transition-colors"
                      >
                        Редактировать
                      </Link>
                      <button
                        onClick={() => handleDelete(musical.id)}
                        className="bg-red-500 text-white px-2 py-1 rounded text-xs hover:bg-red-600 transition-colors"
                      >
                        Удалить
                      </button>
                    </div>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                      <Link 
                          to={`/musicals/${musical.id}/shows`}
                          className="text-blue-500 hover:underline"
                      >
                          Показать
                      </Link>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                      <Link 
                          to={`/musicals/${musical.id}/roles`}
                          className="text-blue-500 hover:underline"
                      >
                          Показать
                      </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default MusicalList;