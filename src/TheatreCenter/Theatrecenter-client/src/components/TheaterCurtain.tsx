import { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import './TheaterCurtain.css';

const TheaterCurtain = ({ children }: { children: React.ReactNode }) => {
  const [isClosed, setIsClosed] = useState(false);
  const [contentVisible, setContentVisible] = useState(true);
  const [currentChildren, setCurrentChildren] = useState(children);
  const [prevChildren, setPrevChildren] = useState(children);
  const location = useLocation();

  useEffect(() => {
    // Пропускаем первый рендер
    // if (!isClosed && prevChildren === children) return;

    // Начинаем анимацию закрытия
    setIsClosed(true);
    setContentVisible(false);
    setPrevChildren(currentChildren);

    const sequence = [
      // Полное закрытие занавеса (400ms)
      { delay: 400, action: () => setCurrentChildren(children) },
      
      // Задержка в закрытом состоянии (300ms)
      { delay: 700, action: () => {
        setContentVisible(true);
        setIsClosed(false);
      }}
    ];

    const timers = sequence.map(({delay, action}) => 
      setTimeout(action, delay)
    );

    return () => timers.forEach(timer => clearTimeout(timer));
  }, [location, children]);

  return (
    <>
      <div className={`content-wrapper ${contentVisible ? '' : 'hidden'}`}>
        {isClosed ? prevChildren : currentChildren}
      </div>
      <div className="curtain-container">
        <div className={`curtain-panel curtain-left ${isClosed ? 'closed' : ''}`} />
        <div className={`curtain-panel curtain-right ${isClosed ? 'closed' : ''}`} />
      </div>
    </>
  );
};

export default TheaterCurtain;