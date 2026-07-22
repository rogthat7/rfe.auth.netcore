/* ─── App Root ────────────────────────────────────────────────────────────── */
import { RouterProvider } from 'react-router-dom'
import { Toaster }        from 'sonner'
import { router }         from './router'

export default function App() {
  return (
    <>
      <RouterProvider router={router} />
      <Toaster
        position="bottom-right"
        toastOptions={{
          style: {
            background: '#1E293B',
            border: '1px solid rgba(255,255,255,0.08)',
            color: '#F1F5F9',
            fontFamily: "'Inter', sans-serif",
            fontSize: '14px',
          },
        }}
      />
    </>
  )
}
