const isDev = process.env.NODE_ENV === "development";
const host = process.env.NEXT_PUBLIC_API_ADDRESS;
const apiBase = isDev ? 'http://localhost/api' : host ? host+'/api' : '/api';

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

export async function apiNoResponse (endpoint, options) {
  const response = await fetch(`${apiBase}${endpoint}`, {
    credentials: 'include',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json'
    },
    ...options
  })

  if (response.status === 204) {
    return false;
  }

  if (response.ok) {
    return true;
  }
  return false;
}