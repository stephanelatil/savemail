export interface Folder {
  id: number,
  name: string,
  path: string,
  children: Folder[]
}