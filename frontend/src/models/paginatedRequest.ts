import { URL } from "./helpers";

export interface PaginatedRequest<T>{
    items:T[],
    pageIndex:number,
    prev:URL,
    next:URL
}