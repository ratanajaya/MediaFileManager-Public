import React, { useState, useEffect, useMemo } from 'react';

import { Row, Col, Dropdown } from 'antd';
import { SwipeEventData, useSwipeable } from 'react-swipeable';
import * as CSS from 'csstype';

import ReaderModalContextMenu from '_shared/ReaderModal/ReaderModalContextMenu';
import _constant from '_utils/_constant';
import loadingImg from '_assets/resources/loading.gif';
import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';
import withPageManager, { IWithPageManagerProps } from '_shared/HOCs/withPageManager';
import * as _helper from '_utils/Helper';
import * as _uri from '_utils/UriHelper';
import Axios from 'axios';
import { AlbumCardModel, AlbumFsNodeInfo, FileInfoModel, NewFsNode, NodeType } from '_utils/Types';
import { IsPortrait } from '_utils/Display';

import cssVariables from '_assets/styles/cssVariables';
import ReaderModalChapterDrawer from '_shared/ReaderModal/ReaderModalChapterDrawer';
import Spinner from '_shared/Spinner/Spinner';

interface PagingIndex{
  cPageI: number,
  pPageI: number,
  triad: string,
  zenpenIndex: number,
  chuhenIndex: number,
  kouhenIndex: number,
}

const defaultIndex: PagingIndex = {
  cPageI: 0,
  pPageI: 0,
  triad: "zenpen",
  zenpenIndex: 0,
  chuhenIndex: 0,
  kouhenIndex: 0,
}

interface AlbumPaginationModel{
  path: string,
  lpi: number,
  fsNodes: NewFsNode[],
  indexes: PagingIndex,
  orientation: string,
  includeDetail: boolean,
  restartPageTrigger?: boolean
}

function ReaderModal(props: {
  albumCm: AlbumCardModel | null,
  type: 0 | 1,
  onOpenEditModal: () => void,
  onChapterDeleteSuccess: (path: string, val: number) => void,
  onClose: (path: string, lpi: number) => void
} & IWithMyAlertProps & IWithPageManagerProps) {
  const [apm, setApm] = useState<AlbumPaginationModel>({
    path: "",
    lpi: 0,
    fsNodes: [],
    indexes: defaultIndex,
    orientation: 'Portrait',
    includeDetail: props.type === 1 || false,
    restartPageTrigger: false,
  });
  const [forceRotation, setForceRotation] = useState<boolean>(false);
  const alwaysPortrait = _uri.GetAlwaysPortrait();

  const [loading, setLoading] = useState<boolean>(false);
  
  const pages = useMemo(() => {
    return _helper.getFlatFileInfosFromFsNodes(apm.fsNodes);
  }, [apm.fsNodes]);

  useEffect(() => {
    if (props.albumCm == null){
      setApm(prev => {
        prev.path = "";
        prev.lpi = 0;
        prev.fsNodes = [];
        return {...prev};
      });

      return;
    }
    refreshPages(false);
    
  }, [props.albumCm]);

  const refreshPages = (restartPage: boolean) => {
    if(!props.albumCm) return;
    console.log("refreshPages");

    setLoading(true);

    Axios.get<AlbumFsNodeInfo>(_uri.GetAlbumFsNodeInfo(props.type, props.albumCm.path, apm.includeDetail, apm.includeDetail))
      .then(function (response) {
        setApm({
          path: props.albumCm!.path,
          lpi: restartPage ? 0 : props.albumCm!.lastPageIndex,
          orientation: response.data.orientation,
          fsNodes: response.data.fsNodes,
          indexes: defaultIndex,
          includeDetail: apm.includeDetail,
          restartPageTrigger: restartPage ? !apm.restartPageTrigger : apm.restartPageTrigger
        });

        if (props.albumCm!.pageCount !== _helper.countFileNodes(response.data.fsNodes) && props.type !== 1) {
          Axios.get<string>(_uri.RefreshAlbum(props.type, props.albumCm!.path))
            .then(function (response) {
            })
            .catch(function (error) {
              props.popApiError(error);
            });
        }
      })
      .catch(function (error) {
        props.popApiError(error);
      })
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    const fileCount = _helper.countFileNodes(apm.fsNodes);
    const targetPage = apm.lpi < fileCount ? apm.lpi : fileCount - 1;

    pageHandler.jumpTo(targetPage);
  }, [apm.path, apm.restartPageTrigger]);

  useEffect(() => {
    document.addEventListener("keydown", handleKeyDown2, false);

    return () => { //component will unmount
      document.removeEventListener("keydown", handleKeyDown2, false);
    };
  }, []);

  const handleKeyDown2 = (event: any) => {
    if (event.keyCode === 27 && props.albumCm != null) {
      pageHandler.close();
    }
  };

  //#region Event handlers
  const pageHandler = {
    jumpTo: function (page: number) {
      const fileCount = _helper.countFileNodes(apm.fsNodes); 

      const maxIndex = fileCount - 1;
      const newPPageI = apm.indexes?.cPageI ?? 0;
      const newCPageI = _helper.clamp(page, 0, maxIndex);

      const { newTriad, zenpenIndex, chuhenIndex, kouhenIndex } = ((): {
        newTriad: string, zenpenIndex: number, chuhenIndex: number, kouhenIndex: number
      } => {
        function pageCeil(modifier: number) {
          const pagesLen = fileCount;
          return (((newCPageI + modifier) % pagesLen) + pagesLen) % pagesLen;
        }

        if (newCPageI % 3 === 0) {
          return {
            newTriad: "zenpen",
            zenpenIndex: newCPageI,
            chuhenIndex: pageCeil(1),
            kouhenIndex: pageCeil(-1)
          }
        }
        if (newCPageI % 3 === 1) {
          return {
            newTriad: "chuhen",
            zenpenIndex: pageCeil(-1),
            chuhenIndex: newCPageI,
            kouhenIndex: pageCeil(1)
          }
        }
        
        return {
          newTriad: "kouhen",
          zenpenIndex: pageCeil(1),
          chuhenIndex: pageCeil(-1),
          kouhenIndex: newCPageI
        }
      })();

      setApm({
        indexes: {
          cPageI: newCPageI,
          pPageI: newPPageI,
          triad: newTriad,
          zenpenIndex: zenpenIndex,
          chuhenIndex: chuhenIndex,
          kouhenIndex: kouhenIndex
        },
        path: apm.path,
        lpi: apm.lpi,
        orientation: apm.orientation,
        includeDetail: apm.includeDetail,
        fsNodes: apm.fsNodes
      });

      if (apm.path !== "" && page === fileCount - 1 && props.type !== 1) {
        Axios.post(_uri.UpdateAlbumOuterValue(props.type), {
          albumPath: apm.path,
          lastPageIndex: page
        })
          .then((response) => {
          })
          .catch((error) => {
            props.popApiError(error);
          });
      }
    },
    rename: (newVal: string, oldVal: string) => {
      const srcAlRelPath = currentPageInfo.uncPathEncoded;
      const dstAlRelPath = (() => {
        var arr = currentPageInfo.uncPathEncoded.split('\\');
        arr.pop();
        arr.push(newVal);

        return _helper.pathJoin(arr, '\\');
      })();

      const movObj = {
        overwrite: false,
        src: {
          albumPath: apm.path,
          alRelPath: srcAlRelPath,
        },
        dst: {
          albumPath: apm.path,
          alRelPath: dstAlRelPath
        }
      };

      props.movePage(movObj, (response) => {
          setApm((prev) => {
            const isInRoot = prev.fsNodes.findIndex(a => a.alRelPath === srcAlRelPath) !== -1;
            if(isInRoot){
              prev.fsNodes = prev.fsNodes.filter(a => a.alRelPath !== dstAlRelPath); //Remove overwritten node

              const foundIdx = prev.fsNodes.findIndex(a => a.alRelPath === srcAlRelPath);
              if(foundIdx !== -1){
                prev.fsNodes[foundIdx].alRelPath = dstAlRelPath;
                prev.fsNodes[foundIdx].fileInfo!.name = newVal;
                prev.fsNodes[foundIdx].fileInfo!.uncPathEncoded = dstAlRelPath;
              }
            }
            else{
              prev.fsNodes.forEach(a => {
                if(a.nodeType !== NodeType.Folder) return;

                const isInCurrentChapter = a.dirInfo!.childs.findIndex(b => b.alRelPath === srcAlRelPath) !== -1;
                if(isInCurrentChapter){
                  a.dirInfo!.childs = a.dirInfo!.childs.filter(b => b.alRelPath !== dstAlRelPath); //Remove overwritten node
                  
                  const foundIdx = a.dirInfo!.childs.findIndex(b => b.alRelPath === srcAlRelPath);
                  if(foundIdx !== -1){
                    a.dirInfo!.childs[foundIdx].alRelPath = dstAlRelPath;
                    a.dirInfo!.childs[foundIdx].fileInfo!.name = newVal;
                    a.dirInfo!.childs[foundIdx].fileInfo!.uncPathEncoded = dstAlRelPath;
                  }
                }
              });
            }

            return ({
              ...prev,
              fsNodes: [...prev.fsNodes]
            });
          });
        },
        (error) => {
          props.popApiError(error);
        }
      )
    },
    move: (newDir: string) => {
      //Currently at 2023-05-21
      //This function only applies for SC
      const srcAlRelPath = currentPageInfo.uncPathEncoded;

      const movObj = {
        overwrite: false,
        src: {
          albumPath: apm.path,
          alRelPath: srcAlRelPath,
        },
        dst: {
          albumPath: newDir,
          alRelPath: currentPageInfo.name
        }
      };

      props.movePage(movObj, 
        (response) => {
          removePageFromApm(currentPageInfo.uncPathEncoded);
        },
        (error) => {
          props.popApiError(error);
        }
      )
    },
    delete: (pageInfo: FileInfoModel, directDelete: boolean) => {
      const delObj = {
        albumPath: apm.path,
        alRelPath: pageInfo.uncPathEncoded
      };

      props.deletePage(delObj, directDelete, (response) => {
          removePageFromApm(currentPageInfo.uncPathEncoded);
        },
        (error: any) => {
          props.popApiError(error);
        });
    },
    close: () => {
      props.onClose(apm.path, apm.indexes.cPageI);
    },
    chapterDeleteSuccess: (chapterName: string) => {
      const newFsNodes = apm.fsNodes.filter(a => a.dirInfo?.name !== chapterName);
      setApm(prev => {
        prev.fsNodes = newFsNodes;
        prev.indexes = defaultIndex;
        return {...prev};
      });

      props.onChapterDeleteSuccess(apm.path, _helper.countFileNodes(newFsNodes));
    },
    chapterRenameSuccess: (chapterName: string, newChapterName: string) => {
      let newFsNodes = apm.fsNodes;
      const fsNodeIdx = newFsNodes.findIndex(a => a.dirInfo?.name === chapterName);
      newFsNodes[fsNodeIdx].alRelPath = `${newChapterName}`;
      newFsNodes[fsNodeIdx].dirInfo!.name = newChapterName;
      newFsNodes[fsNodeIdx].dirInfo!.childs = newFsNodes[fsNodeIdx].dirInfo!.childs.map(a => {
        return {
          ...a,
          alRelPath: `${newChapterName}\\${a.fileInfo!.name}`,
          fileInfo: {
            ...a.fileInfo!,
            uncPathEncoded: `${newChapterName}\\${a.fileInfo!.name}`
          }
        }
      });

      setApm(prev => {
        return {
          ...prev,
          fsNodes: [...newFsNodes]
        };
      });
    },
    chapterTierChangeSuccess: (chapterName: string, tier: number) => {
      let newFsNodes = apm.fsNodes;
      const fsNodeIdx = newFsNodes.findIndex(a => a.dirInfo?.name === chapterName);
      newFsNodes[fsNodeIdx].dirInfo!.tier = tier;

      setApm(prev => {
        return {
          ...prev,
          indexes: clampIndexes(prev.indexes, _helper.countFileNodes(newFsNodes) - 1),
          fsNodes: [...newFsNodes]
        };
      });
    }
  }

  function removePageFromApm(alRelPath: string){
    setApm(prev => {
      prev.fsNodes = prev.fsNodes.filter(a => a.fileInfo?.uncPathEncoded !== alRelPath);
      prev.fsNodes.forEach(a => {
        if(a.nodeType !== NodeType.Folder) return;

        a.dirInfo!.childs = a.dirInfo!.childs.filter(b => b.fileInfo?.uncPathEncoded !== alRelPath);
      });

      return{
        ...prev,
        indexes: clampIndexes(prev.indexes, _helper.countFileNodes(prev.fsNodes) - 1),
        fsNodes: [...prev.fsNodes]
      }
    });
  }

  function clampIndexes(indexes: PagingIndex, max: number): PagingIndex{
    return{
      cPageI:_helper.clamp(indexes.cPageI, 0, max),
      pPageI:_helper.clamp(indexes.pPageI, 0, max),
      zenpenIndex:_helper.clamp(indexes.zenpenIndex, 0, max),
      chuhenIndex:_helper.clamp(indexes.chuhenIndex, 0, max),
      kouhenIndex:_helper.clamp(indexes.kouhenIndex, 0, max),
      triad: indexes.triad
    }
  }

  function changeDetailVisibility(){
    const includeDetail = !apm.includeDetail;

    if(includeDetail && pages[0].createDate == null){
      Axios.get<AlbumFsNodeInfo>(_uri.GetAlbumFsNodeInfo(props.type, props.albumCm!.path, includeDetail, includeDetail))
      .then(function (response) {
        setApm(prev => {
          prev.includeDetail = includeDetail;
          prev.fsNodes = response.data.fsNodes;
          return { ...prev };
        });
      })
      .catch(function (error) {
        props.popApiError(error);
      });
    }
    else{
      setApm(prev => {
        prev.includeDetail = includeDetail;
        return { ...prev };
      });
    }
  }

  const albumHandler = {
    tierChange: (value: number) => {
      Axios.put(_uri.UpdateAlbumTier(props.type), {
        albumPath: apm.path,
        tier: value
      })
        .then(function (response) {
        })
        .catch(function (error) {
          props.popApiError(error);
        });
    },
    recount: () => {
      Axios.get<number>(_uri.RecountAlbumPages(props.type, apm.path))
        .then(function (response) {
          props.onChapterDeleteSuccess(apm.path, response.data);
        })
        .catch(function (error) {
          props.popApiError(error);
        });
    }
  }

  const [showContextMenu, setShowContextMenu] = useState(false);
  const [showDrawer, setShowDrawer] = useState(false);

  const isCurrentScreenPortrait = IsPortrait();
  const isReaderRotated = isCurrentScreenPortrait 
    && (!alwaysPortrait && apm.orientation === _constant.orientation.landscape);

  const buttonHandler = {
    rightTop: () => {
      pageHandler.close();
    },
    rightMid: () => {
      pageHandler.jumpTo(apm.indexes.cPageI + 1);
    },
    rightBot: () => {
      setForceRotation(!forceRotation);
      //pageHandler.jumpTo(pages.length - 1);
    },
    midTop: () => {
      changeDetailVisibility();
    },
    midMid: () => {
      setShowContextMenu(true);
    },
    midBot: () => {
    },
    leftTop: () => {
      setShowDrawer(true);
    },
    leftMid: () => {
      pageHandler.jumpTo(apm.indexes.cPageI - 1);
    },
    leftBot: () => {
      pageHandler.jumpTo(0);
    }
  }

  const swipeHandlerEvents ={
    onSwipedUp: (eventData: SwipeEventData) => { 
      if(!isReaderRotated) return;
      pageHandler.jumpTo(apm.indexes.cPageI - 1);
    },
    onSwipedDown: (eventData: SwipeEventData) => { 
      if(!isReaderRotated) return;
      pageHandler.jumpTo(apm.indexes.cPageI + 1);
    },
    onSwipedLeft: (eventData: SwipeEventData) => { 
      if(isReaderRotated) return; 
      pageHandler.jumpTo(apm.indexes.cPageI + 1); 
    },
    onSwipedRight: (eventData: SwipeEventData) => { 
      if(isReaderRotated) return; 
      pageHandler.jumpTo(apm.indexes.cPageI - 1); 
    }
  };

  const swipeHandler1 = useSwipeable(swipeHandlerEvents);
  const swipeHandler2 = useSwipeable(swipeHandlerEvents);
  const swipeHandler3 = useSwipeable(swipeHandlerEvents);

  const bottomSwipeHandler = useSwipeable({
    onSwipedRight: (eventData) => {
      const swipeLen = Math.max(Math.abs(eventData.deltaX), Math.abs(eventData.deltaY));
      const shouldDelete = swipeLen > 100 && eventData.velocity > 0.5;

      if(shouldDelete)
        pageHandler.delete(currentPageInfo, true);
    },
    onSwipedUp: (eventData) => { 
      const swipeLen = Math.max(Math.abs(eventData.deltaX), Math.abs(eventData.deltaY));
      const shouldDelete = swipeLen > 100 && eventData.velocity > 0.5;

      if(shouldDelete)
        pageHandler.delete(currentPageInfo, true);
    },
  });
  //#endregion

  const showGuide = _constant.isPublic && apm.indexes !== null && apm.indexes.cPageI === 0;

  const styleByRotation = getStyleByRotation(isReaderRotated);

  if (loading){ return (<Spinner loading={true} />); }
  if (apm.indexes === null || pages.length === 0) { return (<></>); }

  const currentPageInfo = apm.indexes.triad === "zenpen" ? pages[apm.indexes.zenpenIndex]
    : apm.indexes.triad === "chuhen" ? pages[apm.indexes.chuhenIndex]
      : pages[apm.indexes.kouhenIndex]; //"kouhen"

  const shouldPageRotate = (pageOrientation: number | null) => {
    return forceRotation || (isCurrentScreenPortrait && (
      (!alwaysPortrait && apm.orientation === _constant.orientation.landscape) 
      || ((!alwaysPortrait && apm.orientation === _constant.orientation.auto) && pageOrientation === 2)));
  }

  const zenpenPage = <PageDisplay
    id="zenpen-page"
    albumPath={apm.path}
    pageInfo={pages[apm.indexes.zenpenIndex]}
    queue={apm.indexes.triad === "zenpen" ? "q0" : apm.indexes.triad === "kouhen" ? "q1" : "q2"}
    isRotated={shouldPageRotate(pages[apm.indexes.zenpenIndex].orientation)}
    type={props.type}
  />;

  const chuhenPage = <PageDisplay
    id="chuhen-page"
    albumPath={apm.path}
    pageInfo={pages[apm.indexes.chuhenIndex]}
    queue={apm.indexes.triad === "chuhen" ? "q0" : apm.indexes.triad === "zenpen" ? "q1" : "q2"}
    isRotated={shouldPageRotate(pages[apm.indexes.chuhenIndex].orientation)}
    type={props.type}
  />;

  const kouhenPage = <PageDisplay
    id="kouhen-page"
    albumPath={apm.path}
    pageInfo={pages[apm.indexes.kouhenIndex]}
    queue={apm.indexes.triad === "kouhen" ? "q0" : apm.indexes.triad === "chuhen" ? "q1" : "q2"}
    isRotated={shouldPageRotate(pages[apm.indexes.kouhenIndex].orientation)}
    type={props.type}
  />;

  return (
    <>
      <div className='background-blackout'></div>
      <div style={{ height: "100vh", position: "fixed", zIndex: 2, left: 0, top: 0 }}>
        {zenpenPage}
        {chuhenPage}
        {kouhenPage}
      </div>
      <div className="overlay" style={{ zIndex: 3, ...styleByRotation }}>
        <div className="overlay-content">
          <span className="with-shadow">{currentPageInfo.name}</span>
          <div style={{ position: "fixed", bottom: "0px", width: "100%", textAlign: "center" }}>
            <span className="with-shadow">{apm.indexes.cPageI + 1}/{pages.length}</span>
          </div>
        </div>
      </div>
      <div className="overlay" style={{ zIndex: 4, ...styleByRotation }}>
        <div style={{position:'relative', height:'100%', width:'100%'}}>
          <div className="checker-row" style={{height:'33%'}} {...swipeHandler1}>
            <div className="checker" onClick={buttonHandler.leftTop} >
                {getGuideLabel(showGuide, "Browse Chapters")}
            </div>
            <div className="checker" onClick={buttonHandler.midTop}>
                {getGuideLabel(showGuide, "Show File Info")}
            </div>
            <div className="checker" onClick={buttonHandler.rightTop}>
                {getGuideLabel(showGuide, "Close Album")}
                {!apm.includeDetail ? <></> : 
                  <div className="with-shadow" style={{ padding:'4px', border: `1px solid ${cssVariables.highlightMid}` }}>
                    <div>{_helper.formatBytes(currentPageInfo.size, 0)}</div>
                    <div>{currentPageInfo.width} x {currentPageInfo.height}</div>
                  </div>
                }
            </div>
          </div>
          <div className="checker-row" style={{height:'33%'}} {...swipeHandler2}>
            <div className="checker" onClick={buttonHandler.leftMid}>
                {getGuideLabel(showGuide, "Previous Page")}
            </div>
            <div className="checker" onClick={buttonHandler.midMid}>
              <Dropdown 
                menu={{ items: [{key: '1',label: <></>}], style: {display:'none'} }} 
                onOpenChange={(open) => { if(open) props.onOpenEditModal(); } } trigger={['contextMenu']} 
                destroyPopupOnHide={true}
              >
                <div style={{height:'100%', width:'100%'}}>
                  {getGuideLabel(showGuide, "Context Menu")}
                </div>
              </Dropdown>
            </div>
            <div className="checker" onClick={buttonHandler.rightMid}>
                {getGuideLabel(showGuide, "Next Page")}
            </div>
          </div>
          <div className="checker-row" style={{height:'28%'}} {...swipeHandler3}>
            <div className="checker" onClick={buttonHandler.leftBot}>
                {getGuideLabel(showGuide, "First Page")}
            </div>
            <div className="checker" onClick={buttonHandler.midBot}>
            </div>
            <div className="checker" onClick={buttonHandler.rightBot} >
                {getGuideLabel(showGuide, "Last Page")}
            </div>
          </div>
          <div className="checker-row" style={{height:'6%'}} {...bottomSwipeHandler}>
              {getGuideLabel(showGuide, "Danger Zone")}
          </div>
        </div>
      </div>
      
      {props.albumCm ? 
        <>
          <ReaderModalContextMenu
            visible={showContextMenu}
            albumCm={props.albumCm}
            initialValue={apm.indexes.cPageI}
            pageName={currentPageInfo.name}
            type={props.type}
    
            onTierChange={albumHandler.tierChange}
            onRecount={() => { albumHandler.recount(); setShowContextMenu(false); }}
            onJump={(page) => { pageHandler.jumpTo(page); setShowContextMenu(false); }}
            onJumpToLast={() => { pageHandler.jumpTo(pages.length - 1); setShowContextMenu(false); }}
            onUndoJump={() => { pageHandler.jumpTo(apm.indexes?.pPageI ?? 0); setShowContextMenu(false); }}
            onRename={(newVal, oldVal) => { pageHandler.rename(newVal, oldVal); setShowContextMenu(false); }}
            onMove={(newDir) => {
              if(!newDir) return;
              pageHandler.move(newDir);
              setShowContextMenu(false); 
            }}
            onDelete={() => { pageHandler.delete(currentPageInfo, false); setShowContextMenu(false); }}
            onCommitDelete={() => { pageHandler.delete(currentPageInfo, true); setShowContextMenu(false); }}
            onCancel={() => setShowContextMenu(false)}
          />
          <ReaderModalChapterDrawer
            visible={showDrawer}
            albumPath={apm.path}
            fsNodes={apm.fsNodes}
            currentPageIndex={apm.indexes.cPageI}
            type={props.type}
    
            onClose={() => setShowDrawer(false)}
            onJumpToPage={pageHandler.jumpTo}
            onChapterDeleteSuccess={pageHandler.chapterDeleteSuccess}
            onChapterRenameSuccess={pageHandler.chapterRenameSuccess}
            onChapterTierChangeSuccess={pageHandler.chapterTierChangeSuccess}
          />
        </> : <></>
      }
    </>
  );
}

export default withPageManager(withMyAlert(ReaderModal));

const getStyleByRotation = (isRotated: boolean) => {
  return isRotated ? 
  {
    transform: "translatex(calc(50vw - 50%)) translatey(calc(50vh - 50%)) rotate(270deg)",
    width: "100vh",
    height: "100vw",
  } :
  {
    transform: "translatex(calc(50vw - 50%)) translatey(calc(50vh - 50%))",
    width: "100vw",
    height: "100vh"
  };
}

const getGuideLabel = (isGrey: boolean, text: string) => {
  return isGrey ?
    <span style={{ textAlign: "center", margin: "auto", textShadow: "-1px 0 black, 0 1px black, 1px 0 black, 0 -1px black", color: cssVariables.textMain }}>
      {text}
    </span> :
    "";
}

function PageDisplay(props: {
  id: string,
  albumPath: string,
  pageInfo: FileInfoModel,
  isRotated: boolean,
  queue: string,
  type: 0 | 1
}) {
  const muteVideo = _uri.GetMuteVideo();
  const styleByRotation = getStyleByRotation(props.isRotated);

  const styleShow: CSS.Properties = {
    objectFit: "contain",
    ...styleByRotation
  };
  const styleHide: CSS.Properties = { 
    objectFit: "contain", 
    position: "absolute",
    height: 0, width: 0,
  };
  //const styleShow = { height: 300, width: 300 };
  //const styleHide = { height: 150, width: 150 };

  const styleVidContainer1: CSS.Properties = {
    ...styleByRotation,
    display: 'grid',
    alignItems:'center',
  }

  if (IsVideo(props.pageInfo.extension)) {
    return props.queue === "q0" ? (
      <div style={styleVidContainer1}>
        <video
          autoPlay loop
          muted={muteVideo}
          id={props.id}
          src={_uri.StreamPage(`${props.albumPath}\\${props.pageInfo.uncPathEncoded}`, props.type)}
          style={{objectFit:'contain', width:'100%', height:'100%'}}
        >
          video not showing
        </video>
      </div>
    ) : <></>
  }

  return (
    <img
      id={props.id}
      style={props.queue === "q0" ? styleShow : styleHide}
      src={props.queue === "q0" || props.queue === "q1" ? _uri.StreamPage(`${props.albumPath}\\${props.pageInfo.uncPathEncoded}`, props.type) : loadingImg}
      alt="Not loaded"
    />
  );
}

function IsVideo(extension: string) {
  if (extension === ".webm" || extension === ".mp4") {
    return true;
  }
  return false;
}