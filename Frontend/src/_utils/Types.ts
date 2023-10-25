export type AlbumInfoVm = {
  tags: string[],
  characters: string[],
  categories: string[],
  orientations: string[]
  languages: string[],
  suitableImageFormats: string[],
  suitableVideoFormats: string[]
}

export type QueryVm = {
  name: string,
  tier: number,
  query: string
}

export type AlbumCardModel = {
  path: string;
  fullTitle: string;
  languages: string[];
  isRead: boolean;
  isWip: boolean;
  tier: number;
  lastPageIndex: number;
  pageCount: number;
  note: string;
  coverInfo: FileInfoModel;
  entryDate: string;
  correctablePageCount: number;
}

export type AlbumCardGroup = {
  name: string;
  albumCms: AlbumCardModel[];
}

export type AlbumPageInfo = {
    orientation: string;
    fileInfos: FileInfoModel[];
}

export type FileInfoModel = {
  name: string;
  extension: string;
  uncPathEncoded: string;
  size: number;
  createDate: string | null;
  updateDate: string | null;
  orientation: number | null;
  height: number;
  width: number;
}

export interface HasChildren {
  children: JSX.Element
}

export interface HasLibraryType {
  type: number
}

export type PageDeleteModel = {
  albumPath: string,
  alRelPath: string
}

export type PageMoveModel = {
  overwrite: boolean,
  src: {
    albumPath: string,
    alRelPath: string,
  },
  dst: {
    albumPath: string,
    alRelPath: string
  }
};

export interface MoveFileInfo{
  name: string,
  size: number,
  createdDate: string
}

export type MoveFileResponse = {
  message: string,
  srcInfo: MoveFileInfo | null,
  dstInfo: MoveFileInfo | null
}

export type ChapterVM = {
  title: string;
  pageIndex: number;
  pageUncPath: string;
  tier: number;
  pageCount: number;
}

export type QueryPart = {
  query: string,
  page: number,
  row: number,
  path: string
};

export type Album = {
  title: string;
  category: string;
  orientation: string;

  artists: string[];
  tags: string[];
  characters: string[],
  languages: string[];
  note: string;

  tier: number;

  isWip: boolean;
  isRead: boolean;

  entryDate: string | null;
}

export type AlbumVM = {
  path: string;
  pageCount: number;
  lastPageIndex: number;
  coverInfo?: FileInfoModel;
  correctablePageCount: number;
  album: Album;
}

export enum NodeType {
  Folder = 0,
  File = 1
}

export type FsNode = {
  name: string;
  size: number;
  nodeType: NodeType;
  uncPathEncoded: string | null;
  childs: FsNode[];
}

export type LogDashboardModel = {
  id: string;
  albumFullTitle: string;
  operation: string;
  creationTime: string;

  album: Album;
}

export interface ScrapOperation {
  id: number;

  albumPath: string;
  source: string;

  status: 'Pending' | 'Processing' | 'Error' | 'Success';
  message: string | null;

  title: string | null;

  createDate: string;
  operationDate: string | null;
}

export interface Comment {
  id: number;
  scrapOperationId: number;

  author: string;
  content: string;
  score: number | null;
  postedDate: string | null;
}

export interface CompressionCondition {
  width: number;
  height: number;
  quality: number;
}

export interface FileCorrectionModel {
    alRelPath: string;
    extension: string;
    height: number;
    width: number;
    modifiedDate: string;
    byte: number;
    bytesPer100Pixel: number;

    correctionType: number | null;

    upscaleMultiplier: number | null;
    compression: CompressionCondition;
}

export interface FileCorrectionReportModel {
    alRelPath: string;
    success: boolean;
    message: string;

    height: number;
    width: number;

    byte: number;
    bytesPer100Pixel: number;
}

export interface PathCorrectionModel {
  libRelPath: string;
  lastCorrectionDate: string | null;
  correctablePageCount: number;
}

export interface CorrectPageParam {
  type: number;
  libRelPath: string;
  thread: number;
  upscalerType: number;
  fileToCorrectList: FileCorrectionModel[];
  toJpeg: boolean;
}

export interface NewFsNode {
  nodeType: NodeType;
  alRelPath: string;

  fileInfo: FileInfoModel | null;
  dirInfo: DirInfoModel | null;
}

export interface DirInfoModel {
  name: string;
  tier: number;
  childs: NewFsNode[];
}

export interface AlbumFsNodeInfo {
  title: string;
  orientation: string;
  fsNodes: NewFsNode[];
}

export interface KeyValuePair<T1,T2> {
  key: T1;
  value: T2;
}