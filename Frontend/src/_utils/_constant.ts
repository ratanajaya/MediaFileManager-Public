const apiUrl = "https://wkr.azurewebsites.net/";
const storageUrl = "https://wkrstorage.file.core.windows.net/library/";
const isPublic = false;

const albumCardHeight = "150px";

const colProps = {
  lg: 3,
  md: 6,
  sm: 6,
  xs: 12
};

const orientation = {
  portrait: "Portrait",
  landscape: "Landscape",
  auto: "Auto"
}

export default {
  apiUrl, storageUrl, isPublic, albumCardHeight, colProps, orientation
}