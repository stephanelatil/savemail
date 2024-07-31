import { AppUser } from '@/models/appUser';
import {EditAppUser } from '@/models/appUser';
import { apiFetch, apiFetchWithBody } from './fetchService';

const USER_ENDPOINT = '/api/AppUser/';

export const getUser = async (id: string) : Promise<AppUser> => {
    const response = await apiFetch(USER_ENDPOINT);

    if (!response.ok) {
        throw new Error(`${response.status} ${response.statusText}: Failed to load user`);
    }

    return response.json();
}

/**
 * Retrieves the currently logged-in user.
 * @returns A promise that resolves to the `User` object for the logged-in user.
 * @throws An error if the fetch operation fails.
 */
export const getLoggedInUser = async () : Promise<AppUser> => {
    const response = await apiFetch(`${USER_ENDPOINT}me`);

    if (!response.ok) {
        throw new Error(`${response.status} ${response.statusText}: Failed to load loggedin user`);
    }

    return response.json();
}


/**
 * Edits the details of a user.
 * @param user The user object containing updates. Will only update names and two factor
 * @returns A promise that resolves to the updated `User` object.
 * @throws An error if the edit operation fails or if the user is not authorized to make the edit.
 */
export const editUser = async (user: EditAppUser): Promise<boolean> => {
    const id = user.id

    const response = await apiFetchWithBody(`${USER_ENDPOINT}${id}`, 'PATCH', user)

    if (!response.ok) {
        if (response.status === 401 || response.status === 403)
            throw new Error(`You are not authorized to edit this user`)
        if (response.status == 404)
            throw new Error("User not found error")
        if (response.status == 400)
            throw new Error("Invalid or missing values")
        throw new Error(`Failed to edit the user`)
    }
    return response.json()
}


/**
 * Edits the details of a user.
 * @param user The user object containing updates. Will only update names and two factor
 * @returns A promise that resolves to the updated `User` object.
 * @throws An error if the edit operation fails or if the user is not authorized to make the edit.
 */
export const deleteUser = async (id: string): Promise<null> => {
    const response = await apiFetchWithBody(`${USER_ENDPOINT}${id}`, 'DELETE')

    if (!response.ok) {
        if (response.status === 401 || response.status === 403)
            throw new Error(`You are not authorized to edit this user`)
        if (response.status == 404)
            throw new Error("User not found error")
        throw new Error(`Failed to edit the user`)
    }
    return null;
}