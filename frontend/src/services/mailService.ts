import { Mail } from "@/models/mail"
import { apiFetch, apiFetchWithBody } from "./fetchService"
import { Folder } from "@/models/folder";

const MAIL_ENDPOINT:string = "/api/Mail/"

export const getMail = async (id:number):Promise<Mail> =>{
    const response = await apiFetch(`${MAIL_ENDPOINT}${id}`);
    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Mail not found");

    return response.json();
}

export const deleteMail = async (id:number) : Promise<null> => {
    const response = await apiFetchWithBody(`${MAIL_ENDPOINT}${id}`, 'DELETE');

    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Mail not found");
    if (response.status >= 500)
        throw new Error("Database Error please try again later")

    return null;
}