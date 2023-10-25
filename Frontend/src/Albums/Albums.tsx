import React, { useState, useEffect, useMemo } from 'react';

import { Row, Col, Modal, Collapse, Switch, Button, Checkbox, Radio, Grid, Select, Spin } from 'antd';
import { CaretRightFilled, ExclamationCircleOutlined } from '@ant-design/icons';

import { InView } from 'react-intersection-observer';
import Axios from 'axios';

import MyAlbumCard from '_shared/Displays/MyAlbumCard';
import ReaderModal from '_shared/ReaderModal/ReaderModal';
import EditModal from 'Albums/EditModal';
import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';
import PullToRefresh from 'react-simple-pull-to-refresh';
import * as _uri from '_utils/UriHelper';
import * as _helper from '_utils/Helper';
import _constant from '_utils/_constant';
import MyAlbumWideCard from '_shared/Displays/MyAlbumWideCard';
import { AlbumCardModel, AlbumVM, QueryPart, LogDashboardModel, PathCorrectionModel } from '_utils/Types';
import * as CSS from 'csstype';

import QmarkCyan from '_assets/resources/qmark-cyan.png';
import QmarkGreen from '_assets/resources/qmark-green.png';
import QmarkYellow from '_assets/resources/qmark-yellow.png';
import { RouteComponentProps } from 'react-router-dom';

import cssVariables from '_assets/styles/cssVariables';

const { confirm } = Modal;
const { useBreakpoint } = Grid;

interface IAlbumProps {
  queryParts: QueryPart,
  history: RouteComponentProps['history']
}

interface IOrderModel {
  newOnTop: boolean,
  sort: 'name' | 'dtAsc' | 'dtDesc'
}

export default withMyAlert(function Albums(props: IAlbumProps & IWithMyAlertProps) {
  const type = 0;

  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  //#region Display Album List
  const pageSize = 100;
  const [albumCms, setAlbumCms] = useState<AlbumCardModel[]>([]);
  const [maxPage, setMaxPage] = useState(0);
  const screens = useBreakpoint();
  const itemPerRow = screens.xs ? 2 : screens.sm && !screens.md ? 4 : 8;
  const [loading, setLoading] = useState(false);
  const [useListView, setUseListView] = useState(false);
  const [showDate, setShowDate] = useState(false);
  const [selectedTiers, setSelectedTiers] = useState<string[]>([]);
  const [order, setOrder] = useState<IOrderModel>({
    newOnTop: true,
    sort:'name'
  });

  useEffect(() =>{
    const storedSelectedTiers = window.localStorage.getItem('selectedTiers');
    if(storedSelectedTiers != null)
      setSelectedTiers(JSON.parse(storedSelectedTiers));
    else
      setSelectedTiers(['N','0','1','2','3']);
  },[]);

  const handleTierChange = (tier: string) => {
    const newTier = selectedTiers.includes(tier)
      ? selectedTiers.filter(e => e !== tier)
      : [...selectedTiers, tier];

    setSelectedTiers(newTier);

    window.localStorage.setItem('selectedTiers', JSON.stringify(newTier));
  }

  const handleRefresh = () => {
    return new Promise((resolve, reject) => {
      Axios.get<AlbumCardModel[]>(_uri.GetAlbumCardModels(type, 0, 0, props.queryParts.query))
        .then((response) => {
          setAlbumCms(response.data);
          resolve("param from resolve");
        })
        .catch((error) => {
          props.popApiError(error);
          reject("param from reject");
        })
        .finally(() => {
          //setLoading(false);
        })
    });
  }

  useEffect(() => {
    handleRefresh();
  }, [props.queryParts.query]);
  
  function inViewChange(inView: any, entry: any){
    if(entry.isIntersecting) setMaxPage(maxPage + 1);
  }
  //#endregion

  //#region Display Album Pages
  const [selectedAlbumCm, setSelectedAlbumCm] = useState<AlbumCardModel | null>(null);
  useEffect(() => {
    var body = document.body;
    if(selectedAlbumCm != null){
      body.style.overflow = "hidden";
    }
    else{
      body.style.overflow = "visible";
    }
  },[selectedAlbumCm])

  const readerHandler = {
    view: (albumCm: AlbumCardModel) => {
      setSelectedAlbumCm(albumCm);
    },
    randomView: (selector: (acm: AlbumCardModel) => boolean ) => {
      const possibleAlbums = albumCms.filter(selector);
      const acm = possibleAlbums[_helper.getRandomInt(0, possibleAlbums.length)];
      readerHandler.view(acm);
    },
    close: (path: string, lastPageIndex: number) => {
      setSelectedAlbumCm(null);

      Axios.post(_uri.UpdateAlbumOuterValue(type), {
        albumPath: path,
        lastPageIndex: lastPageIndex
      })
        .then((response) => {
        })
        .catch((error) => {
          props.popApiError(error);
        });

      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.path !== path) {
          return albumCm;
        }
        return {
          ...albumCm,
          lastPageIndex: lastPageIndex,
          isRead: lastPageIndex === albumCm.pageCount - 1 ? true : albumCm.isRead
        };
      }));
    }
  }
  //#endregion

  //#region Edit Delete
  const [editedAlbumPath, setEditedAlbumPath] = useState<string | null>(null);

  const crudHandler = {
    edit: (path: string) => {
      setEditedAlbumPath(path);
    },
    pageDeleteSuccess: (path: string) => {
      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.path !== path) {
          return albumCm;
        }
        return {
          ...albumCm,
          pageCount: albumCm.pageCount - 1
        };
      }));
    },
    chapterDeleteSuccess: (path: string, newPageCount: number) => {
      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.path !== path) {
          return albumCm;
        }
        return {
          ...albumCm,
          pageCount: newPageCount,
          lastPageIndex: 0
        };
      }));
    }
  }

  const editHandler = {
    ok: (editedAlbumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      Axios.post(_uri.UpdateAlbum(type), editedAlbumVm)
        .then((response) => {
          let newAlbumCms = [...albumCms];
          let editedAlbumVmIndex = newAlbumCms.findIndex(a => a.path === editedAlbumVm.path);
          newAlbumCms[editedAlbumVmIndex].fullTitle = `[${editedAlbumVm.album.artists.join(', ')}] ${editedAlbumVm.album.title}`;
          newAlbumCms[editedAlbumVmIndex].languages = editedAlbumVm.album.languages;
          newAlbumCms[editedAlbumVmIndex].note = editedAlbumVm.album.note;
          newAlbumCms[editedAlbumVmIndex].tier = editedAlbumVm.album.tier;
          newAlbumCms[editedAlbumVmIndex].isWip = editedAlbumVm.album.isWip;

          setAlbumCms(newAlbumCms);
        })
        .catch((error) => {
          props.popApiError(error);
        });
    },
    cancel: () => {
      setEditedAlbumPath(null);
    },
    delete: (albumVm: AlbumVM) => {
      const album = albumVm.album;
      confirm({
        title: `Delete album [${album.artists.join(', ')}] ${album.title}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   YES   ',
        okType: 'danger',
        cancelText: '   NO   ',
        onOk() {
          editHandler.directDelete(albumVm.path);
        },
        onCancel() {
        },
      });
    },
    directDelete: (path: string) => {
      Axios.delete(_uri.DeleteAlbum(type, path))
        .then(function (response) {
          setAlbumCms(albumCms.filter(albumCm => {
            return albumCm.path !== path;
          }));
        })
        .catch(function (error) {
          props.popApiError(error);
        })
        .finally(() => {
          setEditedAlbumPath(null);
        })
    },
    refresh: (albumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      Axios.get<string>(_uri.RefreshAlbum(0, albumVm.path))
        .then((response) => {
          props.popInfo("Refresh Album", response.data, null);
        })
        .catch((error) => {
          props.popApiError(error);
        });
    },
    editFiles: (albumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      props.history.push("FileManagement?path=" + albumVm.path);
    },
    correctFiles: (albumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      props.history.push("ScCorrection?path=" + encodeURIComponent(albumVm.path));
    },
  }
  //#endregion

  function handleScanCorrectablePages(thread: number, res: number){
    setLoading(true);

    const paths = filteredAlbumCms.map(a => a.path);

    Axios.post<PathCorrectionModel[]>(_uri.HScanCorrectiblePaths(), { paths: paths, thread:thread, upscaleTarget:res })
      .then((response) => {
        setAlbumCms(prev => {
          const newAcms = prev.map(acm => {
            const pcm = response.data.find(a => a.libRelPath === acm.path);
            return {
              ...acm,
              correctablePageCount: pcm?.correctablePageCount ?? acm.correctablePageCount,
            }
          });

          return [...newAcms];
        });
      })
      .catch((error) => {
        props.popApiError(error);
      })
      .finally(() => {
        setLoading(false);
      });
  }

  const randomAlbums = [
    {
      name: "RANDOM_NEW",
      img: QmarkYellow,
      selector: (acm: AlbumCardModel) => { return !acm.isRead }
    },
    {
      name: "RANDOM_T2",
      img: QmarkGreen,
      selector: (acm: AlbumCardModel) => { return acm.tier === 2 }
    },
    {
      name: "RANDOM_T3",
      img: QmarkCyan,
      selector: (acm: AlbumCardModel) => { return acm.tier === 3 }
    }
  ]

  const addRandomAlbum = albumCms.length >= 100;
  const offset = addRandomAlbum ? 3 : 0;

  const filteredAlbumCms = albumCms.filter(a => { 
    return !a.isRead && a.tier === 0 
      ? selectedTiers.includes('N') 
      : selectedTiers.includes(`${a.tier}`);
  });

  const memoizedAlbumList = useMemo(() => {
    function albumCmSorter(a: AlbumCardModel, b: AlbumCardModel){
      if(order.sort === 'name'){
        return a.fullTitle.localeCompare(b.fullTitle);
      }
      else if(order.sort === 'dtAsc'){
        return a.entryDate.localeCompare(b.entryDate);
      }
      else if(order.sort === 'dtDesc'){
        return b.entryDate.localeCompare(a.entryDate);
      }
  
      return 1;
    }
  
    const pagedAlbumCms = (() => {
      let unreads: AlbumCardModel[] = [];
      let reads: AlbumCardModel[] = [];
  
      if(order.newOnTop)
        filteredAlbumCms.forEach((a) => (a.isRead ? reads : unreads).push(a));
      else
        reads = filteredAlbumCms;
  
      const orderedUnreads = unreads.sort(albumCmSorter);
      const orderedReads = reads.sort(albumCmSorter);
  
      const itemCount = !useListView ? maxPage * pageSize : filteredAlbumCms.length;
      return (orderedUnreads.concat(orderedReads)).slice(0, itemCount);
    })();

    console.log(`memoizedAlbumList rendering with pagedAlbumCms: ${pagedAlbumCms.length}, maxPage: ${maxPage}`);

    return(
      <PullToRefresh onRefresh={handleRefresh}>
        {!useListView ? 
          <>
            {/* <Row gutter={0} type="flex"> */}
            <Row gutter={0}>
              {addRandomAlbum ? 
                randomAlbums.map((a, index) => 
                  {
                    const border = {};

                    const rowIndex = Math.floor(index / itemPerRow);
                    const bc = (index + rowIndex) % 2 === 0 ? cssVariables.highlightLight : 'inherit';
                    return (
                      <Col style={{ textAlign: 'center', backgroundColor: bc }} {..._constant.colProps} key={a.name}>
                        <div className='my-album-card-container-1'>
                          <div className='my-album-card-container-2' style={{ ...border }}>
                            <div className='my-album-card-container-3' onClick={() => readerHandler.randomView(a.selector)}>
                              <img className='my-album-card-img' alt="img" src={a.img}/>
                            </div>
                          </div>
                          <span className='my-album-card-title'>
                            {a.name}
                          </span>
                        </div>
                      </Col>
                    );
                  }
                )
                : <></>}
              {pagedAlbumCms.map((a, index) => {
                const ofsetedIndex = index + offset;
                const rowIndex = Math.floor(ofsetedIndex / itemPerRow);
                const bc = (ofsetedIndex + rowIndex) % 2 === 0 ? cssVariables.highlightLight : 'inherit';
                return (
                  <Col style={{ textAlign: 'center', backgroundColor:bc }} {..._constant.colProps} key={"albumCol" + a.path}>
                    <MyAlbumCard
                      albumCm={a}
                      onView={readerHandler.view}
                      onEdit={crudHandler.edit}
                      showContextMenu={true}
                      type={0}
                      showDate={showDate}
                      showPageCount={true}
                    />
                  </Col>
                );
              })}
            </Row>
            <InView as="div" onChange={inViewChange}>
              <div style={{width:"100%", height:"5px", backgroundColor:"transparent"}}></div>
            </InView>
          </> :
          <div style={{paddingRight:"10px"}}>
            {pagedAlbumCms.map((a, index) => {
              return(
                <MyAlbumWideCard albumCm={a}
                  onView={readerHandler.view}
                  onEdit={crudHandler.edit}
                  onDelete={editHandler.directDelete}
                  type={0}
                />
              );
            })}
          </div>
        }
      </PullToRefresh>
    )
  },[filteredAlbumCms, itemPerRow, maxPage, useListView, showDate, order]);

  return (
    <>
      <Extras 
        query={props.queryParts.query}
        showDate={showDate} onShowDateChange={setShowDate}
        useListView={useListView} onListViewChange={setUseListView}
        selectedTiers={selectedTiers} onTierChange={handleTierChange}
        onScanCorrectablePages={handleScanCorrectablePages}
        filterInfo={`Total: ${albumCms.length} Filtered: ${filteredAlbumCms.length}`}
        order={order}
        setOrder={setOrder}
        loading={loading}
      />
      {memoizedAlbumList}
      <ReaderModal
        onClose={readerHandler.close}
        //onPageDeleteSuccess={crudHandler.pageDeleteSuccess}
        onChapterDeleteSuccess={crudHandler.chapterDeleteSuccess}
        onOpenEditModal={()=>{
          setEditedAlbumPath(selectedAlbumCm?.path ?? null)
        }}
        albumCm={selectedAlbumCm}
        type={0}
      />
      { editedAlbumPath != null ?
        <EditModal
          albumPath={editedAlbumPath}
          onOk={editHandler.ok}
          onCancel={editHandler.cancel}
          onDelete={editHandler.delete}
          onRefresh={editHandler.refresh}
          onEditFiles={editHandler.editFiles}
          onCorrectFiles={editHandler.correctFiles}
          type={0}
        /> : null
      }
    </>
  );
});

const threadOptions = [
  {value: 1, label: '1'},
  {value: 2, label: '2'},
  {value: 3, label: '3'},
  {value: 4, label: '4'},
  {value: 5, label: '5'},
]

const resOptions = [
  {value: 1280, label: '1280px'},
  {value: 1600, label: '1600px'},
  {value: 1920, label: '1920px'},
]

function Extras(props: { 
  query: string,
  showDate: boolean,
  onShowDateChange: (val: boolean) => void,
  useListView: boolean,
  onListViewChange: (val: boolean) => void,
  selectedTiers: string[],
  onTierChange: (tier: string) => void,
  onScanCorrectablePages: (thread: number, res: number) => void,
  order: IOrderModel,
  setOrder: React.Dispatch<React.SetStateAction<IOrderModel>>,
  filterInfo: string,
  loading: boolean
}){
  const [activeKeys, setActiveKeys] = useState<string | string[]>([]);
  const [deleteLogs, setDeleteLogs] = useState<LogDashboardModel[]>([]);
  const [selectedThread, setSelectedThread] = useState<number>(3);
  const [selectedRes, setSelectedRes] = useState<number>(1280);

  useEffect(() => {
    if(activeKeys.indexOf('deletedAlbums') !== -1){
      Axios.get<LogDashboardModel[]>(_uri.GetDeleteLogs(props.query))
        .then(res => {
          setDeleteLogs(res.data.sort((a, b) => a.albumFullTitle.localeCompare(b.albumFullTitle)));
        })
        .catch(error => {
          console.log(error);
        });
    }
  }, [props.query, activeKeys]);

  function handleOrderChange(name: string, val: any){
    props.setOrder(prev => {
      //@ts-ignore
      prev[name] = val;
      return {...prev}
    })
  }

  return (
    <Spin spinning={props.loading}>
      <Collapse 
        bordered={false}
        expandIcon={({ isActive }) => <CaretRightFilled rotate={isActive ? 90 : 0} />}
        style={{marginBottom:'10px'}}
        activeKey={activeKeys}
        onChange={setActiveKeys}
      >
        <Collapse.Panel header={
          <div style={{display:'flex'}}>
            <div style={{flex:'1'}}>Filters</div>
            <div style={{flex:'1', textAlign:'right'}}>{props.filterInfo}</div>
          </div>
        } key='filters'>
          <Row gutter={4}>
            <Col sm={8}>
              <Row>
                <Col span={12}>
                  <div>Show Date</div>
                  <Switch checked={props.showDate} onChange={props.onShowDateChange} />
                </Col>
                <Col span={12}>
                  <div>List View</div>
                  <Switch checked={props.useListView} onChange={props.onListViewChange}/>
                </Col>
              </Row>
            </Col>
            <Col sm={8}>
              <Checkbox checked={props.order.newOnTop} onChange={(e) => handleOrderChange('newOnTop', e.target.checked)}>New on Top</Checkbox>
              <Radio.Group value={props.order.sort} onChange={(e) => handleOrderChange('sort', e.target.value)} >
                <Radio value={'name'}>Name</Radio>
                <Radio value={'dtAsc'}>Dt Asc</Radio>
                <Radio value={'dtDesc'}>Dt Desc</Radio>
              </Radio.Group>
            </Col>
            <Col sm={8}>
              <TierChecklist onTierChange={props.onTierChange} selectedTiers={props.selectedTiers} />
            </Col>
          </Row>
          <Row gutter={4}>
            <Col sm={8}>
              <Button onClick={() => props.onScanCorrectablePages(selectedThread, selectedRes)} 
                type='primary' ghost={true} style={{width:'100%'}}
              >
                Scan Correctable Pages
              </Button>
            </Col>
            <Col sm={3}>
              <Select 
                style={{width:'100%'}}
                options={threadOptions}
                value={selectedThread} onChange={val => setSelectedThread(val)}
              />
            </Col>
            <Col sm={3}>
              <Select 
                style={{width:'100%'}}
                options={resOptions}
                value={selectedRes} onChange={val => setSelectedRes(val)}
              />
            </Col>
          </Row>
        </Collapse.Panel>
        <Collapse.Panel header={
          <div style={{display:'flex'}}>
            <div style={{flex:'1'}}>Deleted Albums</div>
          </div>
        } key='deletedAlbums'>
          {deleteLogs.map(a => (
            <div key={a.id} style={{display:'flex'}}>
              <div style={{flex:'1'}}>{a.albumFullTitle}</div>
              <div>
                {_helper.formatNullableDatetime(a.creationTime)}
              </div>
            </div>
          ))}
        </Collapse.Panel>
      </Collapse>
    </Spin>
  );
}

function TierChecklist(props:{
  selectedTiers: string[],
  onTierChange: (tier: string) => void
}){
  const getButtonDisplay = (tier: string):{
    type: "primary" | "link",
    ghost: boolean,
    style: CSS.Properties
  } => {
    const isTierMatch = props.selectedTiers.includes(tier);

    return {
      type: isTierMatch ? "primary" : "link",
      ghost: isTierMatch,
      style: {
        width: "100%"
      }
    }
  };

  return(
    <Row>
      <Col span={4}></Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('0')} {...getButtonDisplay('0')}>0</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('1')} {...getButtonDisplay('1')}>1</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('2')} {...getButtonDisplay('2')}>2</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('3')} {...getButtonDisplay('3')}>3</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('N')} {...getButtonDisplay('N')}>N</Button>
      </Col>
    </Row>
  );
}