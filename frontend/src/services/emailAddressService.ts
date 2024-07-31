import { apiFetch, apiFetchWithBody } from './fetchService';
import { EmailAddress } from '@/models/emailAddress';

const EMAILADDRESS_ENDPOINT = '/api/EmailAddress/';

export const getFullAddress = async (address: string) : Promise<EmailAddress> => {
    const response = await apiFetch(`${EMAILADDRESS_ENDPOINT}${address}`);

    if (!response.ok) {
        throw new Error(`${response.status} ${response.statusText}: Failed to load emailAddress`);
    }

    return response.json();
}