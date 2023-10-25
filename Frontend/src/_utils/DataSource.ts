import { AlbumInfoVm, QueryVm } from '_utils/Types';
import _constant from '_utils/_constant';

var albumInfo : AlbumInfoVm | null = null;

//#region AlbumInfo
export const GetAlbumInfo = async () => {
  if (albumInfo === null) {
    albumInfo = await FetchAlbumInfo();
  }
  return albumInfo;
}

async function FetchAlbumInfo() {
  const res = await fetch(_constant.apiUrl + "Crud/GetAlbumInfo", {
    method: 'GET'
  });

  return res.json();
}
//#endregion