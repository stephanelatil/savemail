import {HttpMethod} from '@/models/helpers'

/**
 * Performs a GET request to the specified endpoint.
 *
 * @remarks
 * Acts as a wrapper around the `fetch` function to simplify the process of making API requests.
 * Automatically prepends the backend URL to the endpoint if not already included.
 * Sets request to include credentials and specifies no caching.
 *
 * @param endpoint The API endpoint to fetch data from.
 * @returns A promise that resolves to the `Response` object from the fetch operation.
 */
export const apiFetch = async (endpoint: string): Promise<Response> => {
  let url = ''
  if (!endpoint.includes(process.env.NEXT_PUBLIC_BACKEND_URL!)) {
    url = `${process.env.NEXT_PUBLIC_BACKEND_URL}`
  }
  url = url.concat(endpoint)
  const options: RequestInit = {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    cache: 'no-store',
  }

  return await fetch(url, options)
}

/**
 * Performs an API request with CSRF token validation.
 * This function is specifically designed for operations that require CSRF protection,
 * such as POST, PATCH, DELETE, etc. The CSRF token is retrieved from cookies.
 *
 * @param endpoint The API endpoint to fetch data from or send data to.
 * @param method The HTTP method to use for the request.
 * @param body Optional body for the request, applicable for methods like POST.
 * @returns A promise that resolves to the `Response` object from the fetch operation.
 */
export const apiFetchWithBody = async (
  endpoint: string,
  method: HttpMethod,
  body?: any,
): Promise<Response> => {
  const url = `${process.env.NEXT_PUBLIC_BACKEND_URL}${endpoint}`
  const options: RequestInit = {
    method,
    headers: {
      'Content-Type': 'application/json'
    },
    credentials: 'include',
    cache: 'no-store',
  }

  if (body) {
    options.body = JSON.stringify(body)
  }

  return await fetch(url, options)
}

/**
 * Performs a POST request with `FormData` to the specified endpoint, including CSRF token.
 * This function is suitable for uploading files and other data that can't be JSON-encoded.
 *
 * @param endpoint The API endpoint to send the form data to.
 * @param body The `FormData` object containing data to be sent.
 * @returns A promise that resolves to the `Response` object from the fetch operation.
 */
export const apiFetchWithFormData = async (endpoint: string, body: FormData) => {
  const url = `${process.env.NEXT_PUBLIC_BACKEND_URL}${endpoint}`
  const options: RequestInit = {
    method: 'POST',
    body,
    credentials: 'include'
  }

  return await fetch(url, options)
}
