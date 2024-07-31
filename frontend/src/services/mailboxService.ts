import { EditMailBox, MailBox } from "@/models/mailBox"
import { apiFetch, apiFetchWithBody } from "./fetchService"
import { Folder } from "@/models/folder";

const MAILBOX_ENDPOINT:string = "/api/MailBox/"

export const getAllMailboxes = async () : Promise<MailBox[]> =>{
    const response = await apiFetch(MAILBOX_ENDPOINT);

    if (response.status == 401 || response.status == 403)
        throw new Error("User is not logged in");
    return response.json();
}

export const getMailbox = async (id:number):Promise<MailBox> =>{
    const response = await apiFetch(`${MAILBOX_ENDPOINT}${id}`);
    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Mailbox not found");

    return response.json();
}

export const editMailBox = async (editMailBox:EditMailBox) : Promise<null> => {
    const id = editMailBox.id;
    const response = await apiFetchWithBody(`${MAILBOX_ENDPOINT}${id}`, 'PATCH', editMailBox);

    if (response.status == 400)
        throw new Error("Invalid or missing values: "+await response.text());
    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Mailbox not found");
    
    return null;
}

export const createMailBox = async (editMailBox:EditMailBox) : Promise<MailBox> => {
    const response = await apiFetchWithBody(MAILBOX_ENDPOINT, "POST", editMailBox);

    if (response.status == 400)
        throw new Error("Invalid or missing values: "+await response.text());
    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status >= 500)
        throw new Error("Database Error please try again later")

    return response.json();
}

export const getMailboxFolders = async (id:number) : Promise<Folder[]> => {
    const response = await apiFetch(`${MAILBOX_ENDPOINT}Folders`);

    if (response.status == 400)
        throw new Error("Invalid or missing values: "+await response.text());
    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Mailbox not found");
    if (response.status >= 500)
        throw new Error("Database Error please try again later")

    return response.json();
}

export const deleteMailBox = async (id:number) : Promise<null> => {
    const response = await apiFetchWithBody(`${MAILBOX_ENDPOINT}${id}`, 'DELETE');

    if (response.status == 401 || response.status == 403)
        throw new Error("Forbidden");
    if (response.status == 404)
        throw new Error("Mailbox not found");
    if (response.status >= 500)
        throw new Error("Database Error please try again later")

    return null;
}