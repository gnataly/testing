import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getShowById } from '../../api/shows';
import { formatDateTime } from '../../utils/dateUtils';

const ShowDetails: React.FC = () => {
  const { id } = useParams();
  const [show, setShow] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchShow = async () => {
      try {
        const data = await getShowById(Number(id));
        setShow(data);
      } catch (error) {
        console.error('Error fetching show:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchShow();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (!show) return <div>Show not found</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-4">Спектакль</h1>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <p><strong>Дата и время:</strong> {formatDateTime(show.date)}</p>
        </div>
        <div>
          <p><strong>ID мюзикла:</strong> {show.musicalId}</p>
        </div>
        <div className="mt-6">
        <Link 
            to={`/shows/${id}/cast`}
            className="text-blue-500 hover:underline"
        >
            Посмотреть каст этого показа
        </Link>
        </div>
      </div>
    </div>
  );
};

export default ShowDetails;