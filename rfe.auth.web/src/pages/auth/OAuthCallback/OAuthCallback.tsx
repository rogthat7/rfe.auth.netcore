import { useEffect, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useAuthStore } from '../../../store/auth.store'
import { Spinner } from '../../../components/ui/Spinner/Spinner'
import { toast } from 'sonner'
import axios from 'axios'

function decodeJwt(token: string) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    let appsArray: string[] = []
    if (payload.apps) {
      if (Array.isArray(payload.apps)) {
        appsArray = payload.apps
      } else {
        try {
          const parsed = JSON.parse(payload.apps)
          if (Array.isArray(parsed)) {
            appsArray = parsed
          }
        } catch {
          appsArray = [payload.apps]
        }
      }
    }
    return {
      id:    payload.userId || payload.sub || '',
      name:  payload.userName || payload.name || 'System Admin',
      phone: payload.userName || payload.phone || '',
      role:  payload.role || 'appUser',
      apps:  appsArray,
    }
  } catch { return null }
}

export default function OAuthCallback() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)
  const [error, setError] = useState<string | null>(null)
  const executedRef = useRef(false)

  useEffect(() => {
    if (executedRef.current) return
    executedRef.current = true

    const code = searchParams.get('code')
    const codeVerifier = sessionStorage.getItem('pkce_code_verifier')

    if (!code) {
      setError('Authorization code missing.')
      toast.error('Authorization code missing.')
      return
    }

    if (!codeVerifier) {
      setError('PKCE code verifier missing.')
      toast.error('Session expired. Please try logging in again.')
      return
    }

    const exchangeToken = async () => {
      try {
        const body = new URLSearchParams()
        body.append('grant_type', 'authorization_code')
        body.append('client_id', 'mock-external-app')
        body.append('code', code)
        body.append('redirect_uri', window.location.origin + '/oauth-callback')
        body.append('code_verifier', codeVerifier)

        // Make direct post request using standard Form Url Encoded
        const res = await axios.post('/connect/token', body, {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        })

        const data = res.data
        const token = data.access_token

        if (!token) {
          throw new Error('Access token not returned from authorization server.')
        }

        const decoded = decodeJwt(token)
        if (!decoded) {
          throw new Error('Invalid access token format.')
        }

        setAuth(token, decoded)
        sessionStorage.removeItem('pkce_code_verifier')
        toast.success(`Welcome, ${decoded.name}!`)
        navigate('/')
      } catch (err: any) {
        console.error(err)
        const msg = err.response?.data?.error_description || err.message || 'Token exchange failed.'
        setError(msg)
        toast.error(msg)
      }
    }

    exchangeToken()
  }, [searchParams, setAuth, navigate])

  return (
    <div style={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '400px',
      gap: '16px',
      color: '#f1f5f9'
    }}>
      {error ? (
        <div style={{ textAlign: 'center' }}>
          <h2 style={{ color: '#ef4444', marginBottom: '8px' }}>Authentication Error</h2>
          <p style={{ color: '#94a3b8' }}>{error}</p>
          <button
            onClick={() => navigate('/login')}
            style={{
              marginTop: '16px',
              padding: '8px 16px',
              backgroundColor: '#0284c7',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Go to Login
          </button>
        </div>
      ) : (
        <>
          <Spinner size={48} />
          <p style={{ color: '#94a3b8' }}>Finalizing sign-in...</p>
        </>
      )}
    </div>
  )
}
