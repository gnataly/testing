import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import TheaterCurtain from './components/TheaterCurtain';

// Actors
import ActorList from './pages/Actors/ActorList';
import ActorForm from './pages/Actors/ActorForm';
import ActorDetails from './pages/Actors/ActorDetails';

// Musicals
import MusicalList from './pages/Musicals/MusicalList';
import MusicalForm from './pages/Musicals/MusicalForm';
import MusicalDetails from './pages/Musicals/MusicalDetails';

// Theatres
import TheatreList from './pages/Theatres/TheatreList';
import TheatreForm from './pages/Theatres/TheatreForm';
import TheatreDetails from './pages/Theatres/TheatreDetails';

// Roles
import RoleList from './pages/Roles/RoleList';
import RoleForm from './pages/Roles/RoleForm';
import RoleDetails from './pages/Roles/RoleDetails';

// Shows
import ShowList from './pages/Shows/ShowList';
import ShowForm from './pages/Shows/ShowForm';
import ShowDetails from './pages/Shows/ShowDetails';

import MusicalShows from './pages/Musicals/MusicalShows';
import MusicalRoles from './pages/Musicals/MusicalRoles';
import ShowCast from './pages/Shows/ShowCast';
import TheatreMusicals from './pages/Theatres/TheatreMusicals';

import Login from './pages/Auth/Login';
import Register from './pages/Auth/Register';
import AccountProfile from './pages/Accounts/AccountProfile';

import FavoritesPage from './pages/Favorites/FavoritesPage';
import UpgradeRequestsPage from './pages/Accounts/UpgradeRequestsPage';

import HomePage from './pages/HomePage';

const App: React.FC = () => {
  return (
    
    <Router>
      <AuthProvider>
        <TheaterCurtain>
      <Layout>
        <Routes>
          {/* Actors */}
          <Route path="/actors" element={<ActorList />} />
          <Route path="/actors/create" element={<ActorForm />} />
          <Route path="/actors/:id" element={<ActorDetails />} />
          <Route path="/actors/:id/edit" element={<ActorForm />} />
          
          {/* Musicals */}
          <Route path="/musicals" element={<MusicalList />} />
          <Route path="/musicals/create" element={<MusicalForm />} />
          <Route path="/musicals/:id" element={<MusicalDetails />} />
          <Route path="/musicals/:id/edit" element={<MusicalForm />} />

          {/* Theatres */}
          <Route path="/theatres" element={<TheatreList />} />
          <Route path="/theatres/create" element={<TheatreForm />} />
          <Route path="/theatres/:id" element={<TheatreDetails />} />
          <Route path="/theatres/:id/edit" element={<TheatreForm />} />

          {/* Roles */}
          <Route path="/roles" element={<RoleList />} />
          <Route path="/roles/create" element={<RoleForm />} />
          <Route path="/roles/:id" element={<RoleDetails />} />
          <Route path="/roles/:id/edit" element={<RoleForm />} />

          {/* Shows */}
          <Route path="/shows" element={<ShowList />} />
          <Route path="/shows/create" element={<ShowForm />} />
          <Route path="/shows/:id" element={<ShowDetails />} />
          <Route path="/shows/:id/edit" element={<ShowForm />} />

          <Route path="/musicals/:id/shows" element={<MusicalShows />} />
          <Route path="/musicals/:id/roles" element={<MusicalRoles />} />
          <Route path="/shows/:id/cast" element={<ShowCast />} />
          <Route path="/theatres/:id/musicals" element={<TheatreMusicals />} />

          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/profile" element={<AccountProfile />} />
          <Route path="/admin/upgrade-requests" element={<UpgradeRequestsPage />} />

          <Route path="/favorites" element={<FavoritesPage />} />

          <Route path="/" element={<HomePage />} />
        </Routes>
      </Layout>
      </TheaterCurtain>
      </AuthProvider>
    </Router>
    
  );
};

export default App;