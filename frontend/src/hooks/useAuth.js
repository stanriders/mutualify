import localforage from 'localforage'
import { useEffect, useState } from 'react'
import api from '../lib/api'

// user cache
const FIVE_MINUTES = 1000 * 60 * 5

export default function useAuth() {
  const [user, setUser] = useState(null)

  // Get user at initial loading
  useEffect(() => {
    getInitialData()
  }, [])

  async function getInitialData() {
    // Get user data from localstorage if it's not expired
    const [user, userUpdatedAt] = await Promise.all([
      localforage.getItem('user'),
      localforage.getItem('user_updated_at'),
    ])

    const difference = Date.now() - userUpdatedAt

    if (user && difference <= FIVE_MINUTES) {
      return setUser(user)
    }

    // Get user data from the API
    try {
      const user = await api('/me')
      setUser(user)
      localforage.setItem('user', user)
      localforage.setItem('user_updated_at', Date.now())
    } catch (e) {
      console.error(e)
      setUser(null)
    }
  }

 async function invalidateUserCache() {
    await Promise.all([
      localforage.removeItem('user'),
      localforage.removeItem('user_updated_at')
    ])
    getInitialData()
  }

  async function logout() {
    await Promise.all([
      localforage.removeItem('user'),
      localforage.removeItem('user_updated_at')
    ])

    window.location = '/api/OAuth/signout'
  }

  return {
    user,
    logout,
    invalidateUserCache
  }
}