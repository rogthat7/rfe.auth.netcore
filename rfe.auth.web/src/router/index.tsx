/* ─── Router ──────────────────────────────────────────────────────────────── */
import { lazy, Suspense } from 'react'
import { createBrowserRouter, Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '../store/auth.store'
import { Sidebar }  from '../components/layout/Sidebar/Sidebar'
import { TopBar }   from '../components/layout/TopBar/TopBar'
import { Spinner }  from '../components/ui/Spinner/Spinner'
import styles from './router.module.css'

const Login        = lazy(() => import('../pages/auth/Login/Login'))
const Register     = lazy(() => import('../pages/auth/Register/Register'))
const VerifyPhone  = lazy(() => import('../pages/auth/VerifyPhone/VerifyPhone'))
const VerifyEmail  = lazy(() => import('../pages/auth/VerifyEmail/VerifyEmail'))
const Unverified   = lazy(() => import('../pages/auth/Unverified/Unverified'))
const Dashboard    = lazy(() => import('../pages/dashboard/Dashboard'))
const Applications = lazy(() => import('../pages/applications/Applications'))
const Users        = lazy(() => import('../pages/users/Users'))
const Roles        = lazy(() => import('../pages/roles/Roles'))
const NotFound     = lazy(() => import('../pages/NotFound/NotFound'))

function PageLoader() {
  return (
    <div style={{ display:'flex', alignItems:'center', justifyContent:'center', minHeight:'200px' }}>
      <Spinner size={32} />
    </div>
  )
}

function ProtectedLayout() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return (
    <div className={styles.shell}>
      <Sidebar />
      <div className={styles.main}>
        <TopBar />
        <main className={styles.content}>
          <Suspense fallback={<PageLoader />}>
            <Outlet />
          </Suspense>
        </main>
      </div>
    </div>
  )
}

function PublicRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  return isAuthenticated ? <Navigate to="/" replace /> : <>{children}</>
}

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <PublicRoute><Suspense fallback={<PageLoader />}><Login /></Suspense></PublicRoute>,
  },
  {
    path: '/register',
    element: <PublicRoute><Suspense fallback={<PageLoader />}><Register /></Suspense></PublicRoute>,
  },
  {
    path: '/verify-phone',
    element: <Suspense fallback={<PageLoader />}><VerifyPhone /></Suspense>,
  },
  {
    path: '/verify-email',
    element: <Suspense fallback={<PageLoader />}><VerifyEmail /></Suspense>,
  },
  {
    path: '/unverified',
    element: <Suspense fallback={<PageLoader />}><Unverified /></Suspense>,
  },
  {
    element: <ProtectedLayout />,
    children: [
      { index: true,          element: <Dashboard /> },
      { path: 'applications', element: <Applications /> },
      { path: 'users',        element: <Users /> },
      { path: 'roles',        element: <Roles /> },
    ],
  },
  { path: '*', element: <Suspense fallback={<PageLoader />}><NotFound /></Suspense> },
])
