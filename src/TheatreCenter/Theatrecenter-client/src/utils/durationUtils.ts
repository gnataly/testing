// export const formatDuration = (duration: string): string => {
//   if (!duration) return '';
  
//   try {
//     const hoursMatch = duration.match(/(\d+)H/);
//     const minutesMatch = duration.match(/(\d+)M/);
    
//     const hours = hoursMatch ? parseInt(hoursMatch[1]) : 0;
//     const minutes = minutesMatch ? parseInt(minutesMatch[1]) : 0;
    
//     if (hours > 0 && minutes > 0) {
//       return `${hours} ч ${minutes} мин`;
//     } else if (hours > 0) {
//       return `${hours} ч`;
//     } else if (minutes > 0) {
//       return `${minutes} мин`;
//     } else {
//       return duration;
//     }
//   } catch {
//     return duration; 
//   }
// };

export const formatDuration = (duration: string): string => {
  try {
    const date = new Date(duration);
    if (isNaN(date.getTime())) {
      return duration;
    }
    // const hoursMatch = duration.match(/(\d+)H/);
    // const minutesMatch = duration.match(/(\d+)M/);
    const hoursMatch = date.getHours().toString().padStart(2, '0');
    const minutesMatch = date.getMinutes().toString().padStart(2, '0');
    const hours = hoursMatch ? parseInt(hoursMatch[1]) : 0;
    const minutes = minutesMatch ? parseInt(minutesMatch[1]) : 0;
    
    if (hours > 0 && minutes > 0) {
      return `${hours} ч ${minutes} мин`;
    } else if (hours > 0) {
      return `${hours} ч`;
    } else if (minutes > 0) {
      return `${minutes} мин`;
    } else {
      return `${hours}:${minutes}`;
    }
    
    // return `${hours}:${minutes}`;
  } catch {
    return duration;
  }
};