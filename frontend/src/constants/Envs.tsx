import { env } from "next-runtime-env";


export const get_frontend_url = ():string => env('NEXT_PUBLIC_FRONTEND_URL') ?? "http://localhost:3000";
export const get_backend_url = ():string => env('NEXT_PUBLIC_BACKEND_URL') ?? "http://localhost:5000";