const isDev = process.env.NODE_ENV === "development";
const apiBase = isDev ? 'http://localhost/api' : 'https://mutualify.stanr.info/api';

export default async function api (endpoint, options) {
  const response = await fetch(`${apiBase}${endpoint}`, {
    credentials: 'include',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json'
    },
    ...options
  })

  if (response.status === 204) {
    return null
  }

  const responseData = await response.json()

  if (response.ok) {
    return responseData
  }

  throw responseData
}