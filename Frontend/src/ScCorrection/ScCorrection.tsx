import { Alert, Button, Checkbox, Select, Space, Spin, Tag, Typography } from 'antd';
import Axios from 'axios';
import React, { useEffect, useMemo, useState } from 'react'
import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';
import { CorrectPageParam, FileCorrectionModel, FileCorrectionReportModel, KeyValuePair, PathCorrectionModel, QueryPart } from '_utils/Types';
import * as _uri from '_utils/UriHelper';
import * as _helper from "_utils/Helper";
import { LoadingOutlined } from '@ant-design/icons';

const { Text } = Typography;

interface IScCorrectionProps{
  queryParts: QueryPart,
}

export default withMyAlert(function ScCorrection(props: IWithMyAlertProps & IScCorrectionProps) {
  const [paths, setPaths] = useState<PathCorrectionModel[]>([]);
  const [selectedPath, setSelectedPath] = useState<string | null>(null);
  const [selectedRes, setSelectedRes] = useState<number>(1280);
  const [selectedThread, setSelectedThread] = useState<number>(2);
  const [upscalerOptions, setUpscalerOptions] = useState<any[]>([]);
  const [selectedUpscaler, setSelectedUpscaler] = useState<number | null>(301);
  const [limitToCorrectablePath, setLimitToCorrectablePath] = useState<boolean>(false);
  const [toJpeg, setToJpeg] = useState<boolean>(false);
  const [clampToTarget, setClampToTarget] = useState<boolean>(false);

  const [fileToCorrect, setFileToCorrect] = useState<FileCorrectionModel[]>([]);
  const [reports, setReports] = useState<FileCorrectionReportModel[]>([]);
  const [loadingSt, setLoadingSt] = useState<{loading: boolean, message: string}>({ loading: false, message: '' });

  const hPath = props.queryParts.path;
  const isSingleAlbumMode = !_helper.isNullOrEmpty(hPath);

  useEffect(() => {
    setLoadingSt({
      loading: true,
      message: 'Loading Albums...'
    });

    Axios.get<KeyValuePair<number,string>[]>(_uri.GetUpscalers())
      .then((response) => {
        setUpscalerOptions(response.data.map(a => ({
          value: a.key,
          label: a.value
        })));
      })
      .catch((error) => {
        props.popApiError(error);
      });

    if(isSingleAlbumMode){
      //reloadFileToCorrect(hPath, selectedThread, selectedRes);
    }
    else{
      Axios.get<PathCorrectionModel[]>(_uri.ScGetCorrectablePaths())
        .then((response) => {
          setPaths(response.data);
        })
        .catch((error) => {
          props.popApiError(error);
        })
        .finally(() => {
          setLoadingSt({
            loading: false,
            message: 'Albums loaded'
          });
        });
    }
  },[]);

  function fullScan(){
    setLoadingSt({
      loading: true,
      message: 'Scanning the library for correctable pages...'
    });

    Axios.post<PathCorrectionModel[]>(_uri.ScFullScanCorrectiblePaths(), { }, { params:{thread:selectedThread, upscaleTarget:selectedRes } })
      .then((response) => {
        setPaths(response.data);
      })
      .catch((error) => {
        props.popApiError(error);
      })
      .finally(() => {
        setLoadingSt({
          loading: false,
          message: 'Library scan finished'
        });
      });
  }

  const pathToDisplays = paths.filter(a => !limitToCorrectablePath || a.correctablePageCount > 0);

  const upscaleOptionDisplay = pathToDisplays.map(a => ({
    value: a.libRelPath,
    label: (
      <div style={{display:'flex'}}>
        <div style={{flex:'1'}}>{a.libRelPath}</div>
        <div style={{width:'50px', textAlign:'right'}}>{`[${a.correctablePageCount}]`}</div>
        <div style={{width:'160px', paddingRight:'4px', textAlign:'right'}}>{_helper.formatNullableDatetime(a.lastCorrectionDate)}</div>
      </div>)
  }));

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

  function reloadFileToCorrect(path: string | null, thread: number, res: number, clampToTarget: boolean) {
    if(!path) return;

    setLoadingSt({
      loading: true,
      message: 'Loading files...'
    });

    const type = isSingleAlbumMode ? 0 : 1;

    Axios.get<FileCorrectionModel[]>(_uri.GetCorrectablePages(type, path, thread, res, clampToTarget))
      .then((response) => {
        setFileToCorrect(response.data);

        if(!isSingleAlbumMode){
          setPaths(prev => {
            const foundIdx = prev.findIndex(a => a.libRelPath === path);
            prev[foundIdx].correctablePageCount = response.data.length;

            return [...prev];
          });
        }

        setReports([]);
      })
      .catch((error) => {
        props.popApiError(error);
      })
      .finally(() => {
        setLoadingSt({
          loading: false,
          message: 'Files loaded'
        });
      });
  }

  useEffect(() => {
    if(isSingleAlbumMode){
      reloadFileToCorrect(hPath, selectedThread, selectedRes, clampToTarget);
    }
    else{
      reloadFileToCorrect(selectedPath, selectedThread, selectedRes, clampToTarget);
    }
  }, [selectedPath, selectedRes, clampToTarget]);

  function handleSubmit(){
    const {path, type} = isSingleAlbumMode ? { path: hPath, type: 0 } : { path: selectedPath!, type: 1 };

    if(path == null) 
      return;

    if(selectedUpscaler == null) {
      props.popError('Please select an upscaler', 'Upscaler not selected');
      return;
    }

    setLoadingSt({
      loading: true,
      message: 'Performing correction on files...'
    });

    const param: CorrectPageParam = {
      type: type,
      libRelPath: path,
      thread: selectedThread,
      upscalerType: selectedUpscaler,
      fileToCorrectList: fileToCorrect,
      toJpeg: toJpeg,
    };

    Axios.post<FileCorrectionReportModel[]>(_uri.CorrectPages(), param)
      .then((response) => {
        setReports(response.data);
      })
      .catch((error) => {
        props.popApiError(error);
      })
      .finally(() => {
        setLoadingSt({
          loading: false,
          message: 'Correction finished'
        });
      });
  }

  const contentPad = {
    paddingLeft:'12px', paddingRight:'12px'
  }

  const fileToCorrectSorted = fileToCorrect.sort((a, b) => (a.correctionType ?? 0) - (b.correctionType ?? 0) || a.alRelPath.localeCompare(b.alRelPath));
  const uLength = fileToCorrectSorted.filter(a => a.correctionType === 1).length;
  const cLength = fileToCorrectSorted.length - uLength;

  const ftcByteSum = fileToCorrect.map(a => a.byte).reduce((prev, next) => prev + next, 0);
  const ftcByteAvg = fileToCorrect.length > 0 ? ftcByteSum / fileToCorrect.length : 0;

  const rptByteSum = reports.map(a => a.byte).reduce((prev, next) => prev + next, 0);
  const rptByteAvg = reports.length > 0 ? rptByteSum / reports.length : 0;

  const memoizedFileList = useMemo(() => { 
    return (
      <div>
        {fileToCorrectSorted.map(a =>{ 
          const report = reports.find(c => c.alRelPath === a.alRelPath);

          return (
            <div style={{width:'100%'}} key={a.alRelPath}>
              <div className='divider-4'></div>
              <div style={{width:'100%', display: 'flex', ...contentPad}}>
                <div style={{flex:'2'}}><Text>{a.alRelPath}</Text></div>
                <div style={{width:'220px', display:'flex'}}>
                  <div style={{width:'30px'}}>
                    {a.correctionType === 1 
                      ? <Tag color="green">U</Tag> 
                      : <Tag color="orange">C</Tag>
                    }
                  </div>
                  <div style={{flex:'1', textAlign:'end'}}>
                    <div><Text>{_helper.formatBytes2(a.byte)}</Text>  <Text strong>{a.bytesPer100Pixel}</Text></div>
                    <div><Text>{a.width} x {a.height}</Text></div>
                  </div>
                  <div style={{flex:'1', textAlign:'end'}}>
                    <div><Text>{_helper.formatBytes2(report?.byte)}</Text>  <Text strong>{report?.bytesPer100Pixel}</Text></div>
                    <div><Text>{a.compression.width} x {a.compression.height}</Text></div>
                  </div>
                </div>
              </div>
              {report != null && !report.success ? 
                <Alert message={report.message} type="error" />
                : null
              }
            </div>
          );
        })}
      </div>
    );
  }, [fileToCorrectSorted, reports]);

  return (
    <Space direction='vertical' style={{width:'100%'}}>
      {isSingleAlbumMode ? null : 
        <div style={{display:'flex'}}>
          <div style={{flex:'1px'}}>
            <Select 
              style={{width:'100%'}}
              options={upscaleOptionDisplay}
              value={selectedPath}
              onSelect={value => { setSelectedPath(value); }}
              disabled={loadingSt.loading || isSingleAlbumMode}
            />
          </div>
          <div style={{width:'8px'}}></div>
          <div style={{flex:'1', maxWidth:'120px', display: 'flex', alignItems: 'center', paddingLeft:'12px'}}>
            <Checkbox 
              checked={limitToCorrectablePath} 
              onChange={(e) => setLimitToCorrectablePath(e.target.checked)}
              disabled={loadingSt.loading || isSingleAlbumMode}
            >
              Limit
            </Checkbox>
          </div>
          <div style={{width:'8px'}}></div>
          <div style={{flex:'1', maxWidth:'120px'}}>
            <Button 
              style={{width:'100%'}} onClick={fullScan}
              disabled={loadingSt.loading || isSingleAlbumMode}
            >
              Full Scan
            </Button>
          </div>
        </div>
      }
      <div style={{display:'flex'}}>
        <div style={{flex:'1px'}}>
          <Select 
            style={{width:'100%'}}
            options={upscalerOptions}
            value={selectedUpscaler} onChange={val => setSelectedUpscaler(val)}
            disabled={loadingSt.loading}
          />
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px'}}>
          <Select 
            style={{width:'100%'}}
            options={threadOptions}
            value={selectedThread} onChange={val => setSelectedThread(val)}
            disabled={loadingSt.loading}
          />
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px'}}>
          <Select 
            style={{width:'100%'}}
            options={resOptions}
            value={selectedRes} onChange={val => setSelectedRes(val)}
            disabled={loadingSt.loading}
          />
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px', display: 'flex', alignItems: 'center', paddingLeft:'12px'}}>
          <Checkbox 
            checked={clampToTarget} 
            onChange={(e) => setClampToTarget(e.target.checked)}
            disabled={loadingSt.loading}
          >
            Clamp
          </Checkbox>
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px', display: 'flex', alignItems: 'center', paddingLeft:'12px'}}>
          <Checkbox 
            checked={toJpeg} 
            onChange={(e) => setToJpeg(e.target.checked)}
            disabled={loadingSt.loading}
          >
            To Jpeg
          </Checkbox>
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px'}}>
          <Button 
            style={{width:'100%'}} onClick={handleSubmit}
            disabled={loadingSt.loading}>
            Submit
          </Button>
        </div>
      </div>
      {!_helper.isNullOrEmpty(loadingSt.message) 
        ? <Alert message={<div>{loadingSt.loading? <LoadingOutlined style={{marginRight:'8px'}} /> : null}{loadingSt.message}</div>} type={loadingSt.loading ? 'warning' : 'info'} />
        : null
      }
      <div style={{width:'100%', display:'flex', ...contentPad}}>
        <div style={{flex:'2'}}><Text>U: {uLength}    C: {cLength}</Text></div>
        <div style={{width:'220px', display:'flex'}}>
          <div style={{width:'30px'}}></div>
          <div style={{flex:'1', textAlign:'end'}}>
            <div><Text>Sum: {_helper.formatBytes2(ftcByteSum)}</Text></div>
            <div><Text>Avg: {_helper.formatBytes2(ftcByteAvg)}</Text></div>
          </div>
          <div style={{flex:'1', textAlign:'end'}}>
            <div><Text>Sum: {_helper.formatBytes2(rptByteSum)}</Text></div>
            <div><Text>Avg: {_helper.formatBytes2(rptByteAvg)}</Text></div>
          </div>
        </div>
      </div>
      {memoizedFileList}
      <div className='divider-8'></div>
    </Space>
  );
})
