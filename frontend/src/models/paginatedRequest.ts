import { URL } from "./helpers";

export interface PaginatedRequest<T>{
    items:T[],
    pageIndex:number,
    previousPage:URL,
    nextPage:URL
}