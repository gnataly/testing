import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getAllTheatres, deleteTheatre } from '../../api/theatres';

const TheatreList: React.FC = () => {
  const [theatres, setTheatres] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTheatres = async () => {
      try {
        const data = await getAllTheatres();
        setTheatres(data);
      } catch (err) {
        setError('Ошибка при загрузке театров');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchTheatres();
  }, []);

  const handleDelete = async (id: number) => {
    try {
      await deleteTheatre(id);
      setTheatres(theatres.filter(theatre => theatre.id !== id));
    } catch (err) {
      setError('Ошибка при удалении театра');
      console.error(err);
    }
  };

  if (loading) return <div className="p-4">Загрузка...</div>;
  if (error) return <div className="p-4 text-red-500">{error}</div>;

  return (
    <div className="w-full">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-xl font-bold">Театры</h1>
        <Link 
          to="/theatres/create" 
          className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600 transition-colors"
        >
          Добавить театр
        </Link>
      </div>
      
      {theatres.length === 0 ? (
        <div className="p-4 bg-gray-50 rounded-lg border border-gray-200 text-center">
          <p className="mb-4">Театры не найдены</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white border">
            <thead>
              <tr className="bg-gray-100">
                <th className="py-2 px-4 border text-left text-sm">ID</th>
                <th className="py-2 px-4 border text-left text-sm">Название</th>
                <th className="py-2 px-4 border text-left text-sm">Доп. информация</th>
                <th className="py-2 px-4 border text-left text-sm">Действия</th>
                <th className="py-2 px-4 border text-left text-sm">Мюзиклы</th>
              </tr>
            </thead>
            <tbody>
              {theatres.map(theatre => (
                <tr key={theatre.id} className="hover:bg-gray-50">
                  <td className="py-2 px-4 border text-sm">{theatre.id}</td>
                  <td className="py-2 px-4 border text-sm">
                    <Link to={`/theatres/${theatre.id}`} className="text-blue-500 hover:underline">
                      {theatre.name}
                    </Link>
                  </td>
                  <td className="py-2 px-4 border text-sm">{theatre.addInfo || '-'}</td>
                  <td className="py-2 px-4 border text-sm">
                    <div className="flex space-x-2">
                      <Link
                        to={`/theatres/${theatre.id}/edit`}
                        className="bg-yellow-500 text-white px-2 py-1 rounded text-xs hover:bg-yellow-600 transition-colors"
                      >
                        Редактировать
                      </Link>
                      <button
                        onClick={() => handleDelete(theatre.id)}
                        className="bg-red-500 text-white px-2 py-1 rounded text-xs hover:bg-red-600 transition-colors"
                      >
                        Удалить
                      </button>
                    </div>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                      <Link 
                          to={`/theatres/${theatre.id}/musicals`}
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

export default TheatreList;