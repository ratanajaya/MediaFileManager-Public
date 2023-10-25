import moment from "moment";
import { FileInfoModel, NewFsNode, NodeType } from "./Types";

export function firstLetterLowerCase(src: string) {
  return src.charAt(0).toLowerCase() + src.slice(1);
}

export function getPercent100(value: number, maxValue: number) {
  if(maxValue === 0) return 0;

  let fraction = (value / maxValue) * 100;
  let result = fraction <= 100 ? fraction : 100;
  return result;
}

export function clamp(src: number, min: number, max: number) {
  return Math.max(min, Math.min(src, max));
}

export function pathJoin(parts: string[], sep: string){
  var validParts = parts.filter(e => { return e != null && e !== "" });
  var separator = sep || '/';
  var replace   = new RegExp(separator+'{1,}', 'g');
  
  return validParts.join(separator).replace(replace, separator);
}

export function formatBytes(bytes: number, decimals: number = 2){
  if (bytes === 0) return '0 Bytes';

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
}

export function formatBytes2(bytes: number | null | undefined) {
  if(!bytes)
    return null;

  const dp = 1;
  const si = true;
  const thresh = si ? 1000 : 1024;

  if (Math.abs(bytes) < thresh) {
    return bytes + ' B';
  }

  const units = si 
    ? ['KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'] 
    : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];
  let u = -1;
  const r = 10**dp;

  do {
    bytes /= thresh;
    ++u;
  } while (Math.round(Math.abs(bytes) * r) / r >= thresh && u < units.length - 1);

  return bytes.toFixed(dp) + ' ' + units[u];
}

export function formatDate(isoDateStr: string){
  return new Date(isoDateStr).toLocaleString();
}

export function formatNullableDatetime(dt: string | null){
  if(!dt || !moment.utc(dt).isValid()) return '---';

  return moment.utc(dt).format('YYYY-MM-DD HH:mm:ss');
}

export function nz(src: string | null | undefined, alt: string){
  return (src && src.length > 0) ? src : alt;
}

export function ColorFromIndex(tier: number, e: number) {
  const top = { h: 180, s: 70, l: 60 };
  const mid = { h: 100, s: 70, l: 60 };
  const bot = { h: 0, s: 70, l: 60 };

  const hsl = e === 3 ? top : e === 2 ? mid : bot;
  const alpha = tier >= e ? 1 : 0;

  return `hsla(${hsl.h},${hsl.s}%,${hsl.l}%,${alpha})`;
}

export function BorderFromIndex(tier: number, e: number) {
  return tier > e && (e === 1 || e === 2) ? "2px solid black" : "0px";
}

export function BorderRightFromIndex(tier: number, e: number){
  return tier >= e ? "1px solid black" : "0px";
}

export function getRandomInt(min: number, max: number) {
  return Math.floor(Math.random() * (max - min) + min); //The maximum is exclusive and the minimum is inclusive
}

export function isNullOrEmpty(src: string | null | undefined){
  return !src ? true : src === '';
}

export function countFileNodes(nodes: NewFsNode[]){
  let count = 0;
  nodes.forEach(n => {
    if(n.nodeType === NodeType.File){
      count++;
    }
    else{
      count += countFileNodes(n.dirInfo!.childs);
    }
  });

  return count;
}

export function getFlatFileInfosFromFsNodes(nodes: NewFsNode[]){
  let result: FileInfoModel[] = [];
  nodes.forEach(n => {
    if(n.nodeType === NodeType.File){
      result.push(n.fileInfo!);
    }
    else{
      result = result.concat(getFlatFileInfosFromFsNodes(n.dirInfo!.childs));
    }
  });

  return result;
}

export function findFileNodeIndex(nodes: NewFsNode[], alRelPath: string){
  let i : number = 0;
  nodes.forEach(n => {
    if(n.nodeType === NodeType.File){
      if(n.alRelPath === alRelPath)
        return i;
      else
        i++;
    }
    else{
      let j = 0;
      n.dirInfo!.childs.forEach(nc => {
        if(nc.nodeType === NodeType.File){
          if(nc.alRelPath === alRelPath)
            return i + j;
          else
            j++;
        }
      });
    }
  });

  return 0;
}