import { HasChildren } from './Types';
import { useMediaQuery } from 'react-responsive';

const DesktopOnly = ({ children } : HasChildren) => {
  const isDesktop = useMediaQuery({ minWidth: 768 });
  return isDesktop ? children : null;
}
const MobileOnly = ({ children } : HasChildren) => {
  const isMobile = useMediaQuery({ maxWidth: 767 });
  return isMobile ? children : null;
}

const IsMobile = () => {
  var width = window.innerWidth
  || document.documentElement.clientWidth
  || document.body.clientWidth;

  return width < 768;
}

const IsPhone = () => {
  var width = window.innerWidth
  || document.documentElement.clientWidth
  || document.body.clientWidth;

  return width < 480;
}

const IsPortrait = () => {
  var height = window.innerHeight
  || document.documentElement.clientHeight
  || document.body.clientHeight;

  return height > window.innerWidth;
}

export { DesktopOnly, MobileOnly, IsMobile, IsPhone, IsPortrait };