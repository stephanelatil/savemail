import { useEffect } from "react";

export const useMountEffect = (fun:() =>void) => useEffect(fun, [])