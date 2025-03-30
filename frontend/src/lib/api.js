const isDev = process.env.NODE_ENV === "development";
const apiBase = isDev
  ? "http://localhost:3001/api"
  : "https://mutualify.stanr.info/api";

const apiBaseServerside = isDev
  ? "http://localhost:3001/api"
  : "http://backend:3001/api";
  
export default async function api(endpoint, options) {
  const response = await fetch(`${apiBase}${endpoint}`, {
    credentials: "include",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    ...options,
  });

  if (response.status === 204) {
    return null;
  }

  const responseData = await response.json();

  if (response.ok) {
    return responseData;
  }

  throw responseData;
}

export async function apiServerside(endpoint, options) {
  const response = await fetch(`${apiBaseServerside}${endpoint}`, {
    credentials: "include",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    ...options,
  });

  if (response.status === 204) {
    return null;
  }

  const responseData = await response.json();

  if (response.ok) {
    return responseData;
  }

  throw responseData;
}

export async function apiNoResponse(endpoint, options) {
  const response = await fetch(`${apiBase}${endpoint}`, {
    credentials: "include",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
    },
    ...options,
  });

  if (response.status === 204) {
    return false;
  }

  if (response.ok) {
    return true;
  }
  return false;
}
