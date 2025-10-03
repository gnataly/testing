import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getCastMembersByShow } from '../../api/castMembers';

const ShowCast: React.FC = () => {
  const { id } = useParams();
  const [castMembers, setCastMembers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchCast = async () => {
      try {
        const data = await getCastMembersByShow(Number(id));
        setCastMembers(data);
      } catch (error) {
        console.error('Error fetching cast:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchCast();
  }, [id]);

  if (loading) return <div>Loading...</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">Каст показа</h1>
      
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border">
          <thead>
            <tr className="bg-gray-100">
              <th className="py-2 px-4 border text-left">ID</th>
              <th className="py-2 px-4 border text-left">ID роли</th>
              <th className="py-2 px-4 border text-left">ID актера</th>
              <th className="py-2 px-4 border text-left">Комментарий</th>
              <th className="py-2 px-4 border text-left">Действия</th>
            </tr>
          </thead>
          <tbody>
            {castMembers.map(cm => (
              <tr key={cm.id} className="hover:bg-gray-50">
                <td className="py-2 px-4 border">{cm.id}</td>
                <td className="py-2 px-4 border">{cm.roleId}</td>
                <td className="py-2 px-4 border">
                  <Link 
                    to={`/actors/${cm.actorId}`}
                    className="text-blue-500 hover:underline"
                  >
                    {cm.actorId}
                  </Link>
                </td>
                <td className="py-2 px-4 border">{cm.comment || '-'}</td>
                <td className="py-2 px-4 border">
                  <Link 
                    to={`/cast-members/${cm.id}`}
                    className="text-blue-500 hover:underline"
                  >
                    Подробнее
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

export default ShowCast;