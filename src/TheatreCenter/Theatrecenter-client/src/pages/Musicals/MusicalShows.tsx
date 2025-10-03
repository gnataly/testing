import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getShowsByMusical } from '../../api/shows';

const MusicalShows: React.FC = () => {
  const { id } = useParams();
  const [shows, setShows] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchShows = async () => {
      try {
        const data = await getShowsByMusical(Number(id));
        setShows(data);
      } catch (error) {
        console.error('Error fetching shows:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchShows();
  }, [id]);

  if (loading) return <div>Loading...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">Показы мюзикла</h1>
      
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100">
              <th className="py-2 px-4 border text-left">ID</th>
              <th className="py-2 px-4 border text-left">Дата и время</th>
              <th className="py-2 px-4 border text-left">Действия</th>
            </tr>
          </thead>
          <tbody>
            {shows.map(show => (
              <tr key={show.id} className="hover:bg-gray-50">
                <td className="py-2 px-4 border">{show.id}</td>
                <td className="py-2 px-4 border">
                  {new Date(show.date).toLocaleString()}
                </td>
                <td className="py-2 px-4 border">
                  <Link 
                    to={`/shows/${show.id}/cast`}
                    className="text-blue-500 hover:underline"
                  >
                    Посмотреть каст
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default MusicalShows;