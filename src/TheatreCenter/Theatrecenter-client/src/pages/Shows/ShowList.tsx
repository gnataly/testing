import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getAllShows, deleteShow } from '../../api/shows';
import { formatDateTime } from '../../utils/dateUtils';

const ShowList: React.FC = () => {
  const [shows, setShows] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchShows = async () => {
      try {
        const data = await getAllShows();
        setShows(data);
      } catch (err) {
        setError('Ошибка при загрузке спектаклей');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchShows();
  }, []);

  const handleDelete = async (id: number) => {
    try {
      await deleteShow(id);
      setShows(shows.filter(show => show.id !== id));
    } catch (err) {
      setError('Ошибка при удалении спектакля');
      console.error(err);
    }
  };

  if (loading) return <div className="p-4">Загрузка...</div>;
  if (error) return <div className="p-4 text-red-500">{error}</div>;

  return (
    <div className="w-full">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-xl font-bold">Показы</h1>
        <Link 
          to="/shows/create" 
          className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600 transition-colors"
        >
          Добавить показ
        </Link>
      </div>
      
      {shows.length === 0 ? (
        <div className="p-4 bg-gray-50 rounded-lg border border-gray-200 text-center">
          <p className="mb-4">Спектакли не найдены</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white border">
            <thead>
              <tr className="bg-gray-100">
                <th className="py-2 px-4 border text-left text-sm">ID</th>
                <th className="py-2 px-4 border text-left text-sm">Дата и время</th>
                <th className="py-2 px-4 border text-left text-sm">ID мюзикла</th>
                <th className="py-2 px-4 border text-left text-sm">Действия</th>
                <th className="py-2 px-4 border text-left text-sm">Каст</th>
              </tr>
            </thead>
            <tbody>
              {shows.map(show => (
                <tr key={show.id} className="hover:bg-gray-50">
                  <td className="py-2 px-4 border text-sm">{show.id}</td>
                  <td className="py-2 px-4 border text-sm">
                    <Link to={`/shows/${show.id}`} className="text-blue-500 hover:underline">
                      {formatDateTime(show.date)}
                    </Link>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                    <Link to={`/musicals/${show.musicalId}`} className="text-blue-500 hover:underline">
                      {show.musicalId}
                    </Link>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                    <div className="flex space-x-2">
                      <Link
                        to={`/shows/${show.id}/edit`}
                        className="bg-yellow-500 text-white px-2 py-1 rounded text-xs hover:bg-yellow-600 transition-colors"
                      >
                        Редактировать
                      </Link>
                      <button
                        onClick={() => handleDelete(show.id)}
                        className="bg-red-500 text-white px-2 py-1 rounded text-xs hover:bg-red-600 transition-colors"
                      >
                        Удалить
                      </button>
                    </div>
                  </td>
                  <td className="py-2 px-4 border text-sm">
                    <Link 
                      to={`/shows/${show.id}/cast`}
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

export default ShowList;