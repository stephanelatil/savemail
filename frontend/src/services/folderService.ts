import { Mail } from "@/models/mail";
import { apiFetch, apiFetchWithBody } from "./fetchService"
import { Folder } from "@/models/folder";
import { PaginatedRequest } from "@/models/paginatedRequest";

const FOLDER_ENDPOINT:string = "/api/Folder/"

export const getFolder = async (id:number):Promise<Folder> =>{
    const response = await apiFetch(`${FOLDER_ENDPOINT}${id}`);
    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Folder not found");

    return response.json();
}

export const getFolderMails = async (id:number) : Promise<PaginatedRequest<Mail>> => {
    const response = await apiFetch(`${FOLDER_ENDPOINT}Mails`);

    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Folder not found");
    if (response.status >= 500)
        throw new Error("Database Error please try again later")

    return response.json();
}

export const deleteFolder = async (id:number) : Promise<null> => {
    const response = await apiFetchWithBody(`${FOLDER_ENDPOINT}${id}`, 'DELETE');

    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Folder not found");
    if (response.status >= 500)
        throw new Error("Database Error please try again later")

    return null;
}