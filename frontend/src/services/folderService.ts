import { Mail } from "@/models/mail";
import { apiFetch, apiFetchWithBody, FetchError as Error } from "./fetchService"
import { Folder } from "@/models/folder";
import { PaginatedRequest } from "@/models/paginatedRequest";

const FOLDER_ENDPOINT:string = "/api/Folder/"

export const getFolder = async (id:number):Promise<Folder> =>{
    const response = await apiFetch(`${FOLDER_ENDPOINT}${id}`);
    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden", response.status);
    if (response.status == 404)
        throw new Error("Folder not found", response.status);

    return response.json();
}

export const getFolderMails = async (folderId:number, pageNumber:number=1) : Promise<PaginatedRequest<Mail>> => {
    const url = `${FOLDER_ENDPOINT}${folderId}/Mails?pageNumber=${pageNumber}&pageSize=25`;
    const response = await apiFetch(url);

    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden", response.status);
    if (response.status == 404)
        throw new Error("Folder not found", response.status);
    if (response.status >= 500)
        throw new Error("Database Error please try again later", response.status);

    return response.json();
}

export const deleteFolder = async (id:number) : Promise<null> => {
    const response = await apiFetchWithBody(`${FOLDER_ENDPOINT}${id}`, 'DELETE');

    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden", response.status);
    if (response.status == 404)
        throw new Error("Folder not found", response.status);
    if (response.status >= 500)
        throw new Error("Database Error please try again later", response.status);

    return null;
}