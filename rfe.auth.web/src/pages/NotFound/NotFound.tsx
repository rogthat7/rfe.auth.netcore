import { Link } from 'react-router-dom'
import { Shield } from 'lucide-react'
import styles from './NotFound.module.css'

export default function NotFound() {
  return (
    <div className={styles.page}>
      <div className={styles.icon}><Shield size={48} /></div>
      <h1 className={styles.code}>404</h1>
      <h2 className={styles.title}>Page Not Found</h2>
      <p className={styles.desc}>The page you're looking for doesn't exist or you don't have access.</p>
      <Link to="/" className={styles.link}>← Back to Dashboard</Link>
    </div>
  )
}
