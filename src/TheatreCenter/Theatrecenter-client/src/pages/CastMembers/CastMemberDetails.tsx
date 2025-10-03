import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getCastMemberById } from '../../api/castMembers';

const CastMemberDetails: React.FC = () => {
  const { id } = useParams();
  const [castMember, setCastMember] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchCastMember = async () => {
      try {
        const data = await getCastMemberById(Number(id));
        setCastMember(data);
      } catch (error) {
        console.error('Error fetching cast member:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchCastMember();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (!castMember) return <div>Cast member not found</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-4">Участник труппы</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <p><strong>ID спектакля:</strong> {castMember.showId}</p>
          <p><strong>ID роли:</strong> {castMember.roleId}</p>
        </div>
        <div>
          <p><strong>ID актера:</strong> {castMember.actorId}</p>
          {castMember.comment && (
            <p><strong>Комментарий:</strong> {castMember.comment}</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default CastMemberDetails;