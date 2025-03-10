import { apiFetch, FetchError as Error } from './fetchService';
import { EmailAddress } from '@/models/emailAddress';

const EMAILADDRESS_ENDPOINT = '/api/EmailAddress/';

export const getFullAddress = async (address: string) : Promise<EmailAddress> => {
    const response = await apiFetch(`${EMAILADDRESS_ENDPOINT}${address}`);

    if (response.status == 500)
    {
        const err = await response.json();
        console.error(err.message);
        throw new Error(err.message, 500)
    }
    else if (!response.ok) {
        throw new Error(`${response.status} ${response.statusText}: Failed to load emailAddress`, response.status);
    }

    return response.json();
}