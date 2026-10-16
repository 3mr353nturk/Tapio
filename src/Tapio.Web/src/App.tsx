import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AdminLayout } from './components/layout/AdminLayout';
import { BusinessLayout } from './components/layout/BusinessLayout';
import { LandingPage } from './pages/LandingPage';
import { LoginPage } from './pages/auth/LoginPage';
import { RegisterPage } from './pages/auth/RegisterPage';
import { ForgotPasswordPage, ResetPasswordPage, VerifyEmailPage } from './pages/auth/AuthUtilityPages';
import { BusinessPage } from './pages/public/BusinessPage';
import { ActionsPage, AnalyticsPage, CampaignsPage, DashboardPage, ProfilePage, SharePage, StandsPage } from './pages/business/BusinessPages';
import { AdminBusinessesPage, AdminDashboardPage, AdminStandsPage, AdminUsersPage } from './pages/admin/AdminPages';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/verify-email" element={<VerifyEmailPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/t/:slug" element={<BusinessPage />} />

        <Route path="/app" element={<ProtectedRoute role="BusinessUser"><BusinessLayout /></ProtectedRoute>}>
          <Route index element={<DashboardPage />} />
          <Route path="share" element={<SharePage />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="actions" element={<ActionsPage />} />
          <Route path="campaigns" element={<CampaignsPage />} />
          <Route path="stands" element={<StandsPage />} />
          <Route path="analytics" element={<AnalyticsPage />} />
        </Route>

        <Route path="/admin" element={<ProtectedRoute role="Admin"><AdminLayout /></ProtectedRoute>}>
          <Route index element={<AdminDashboardPage />} />
          <Route path="businesses" element={<AdminBusinessesPage />} />
          <Route path="users" element={<AdminUsersPage />} />
          <Route path="stands" element={<AdminStandsPage />} />
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
