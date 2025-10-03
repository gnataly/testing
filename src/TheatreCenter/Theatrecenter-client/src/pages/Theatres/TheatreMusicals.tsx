import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getMusicalsByTheatre } from '../../api/musicals';

const TheatreMusicals: React.FC = () => {
  const { id } = useParams();
  const [musicals, setMusicals] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchMusicals = async () => {
      try {
        const data = await getMusicalsByTheatre(Number(id));
        setMusicals(data);
      } catch (error) {
        console.error('Error fetching musicals:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMusicals();
  }, [id]);

  if (loading) return <div>Loading...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">Мюзиклы театра</h1>
      
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100">
              <th className="py-2 px-4 border text-left">ID</th>
              <th className="py-2 px-4 border text-left">Название</th>
              <th className="py-2 px-4 border text-left">Описание</th>
              <th className="py-2 px-4 border text-left">Действия</th>
            </tr>
          </thead>
          <tbody>
            {musicals.map(musical => (
              <tr key={musical.id} className="hover:bg-gray-50">
                <td className="py-2 px-4 border">{musical.id}</td>
                <td className="py-2 px-4 border">
                  <Link 
                    to={`/musicals/${musical.id}`}
                    className="text-blue-500 hover:underline"
                  >
                    {musical.title}
                  </Link>
                </td>
                <td className="py-2 px-4 border">{musical.description}</td>
                <td className="py-2 px-4 border">
                  <Link 
                    to={`/musicals/${musical.id}/roles`}
                    className="text-blue-500 hover:underline"
                  >
                    Роли
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

export default TheatreMusicals;