import React, { useState, useEffect, ReactNode } from 'react';

import { Button, Divider, Progress, Input, Switch, message, Typography, Space } from 'antd';

import * as CSS from 'csstype';
import Axios from 'axios';
import _constant from "_utils/_constant";
import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';
import * as _uri from "_utils/UriHelper";
import * as _helper from "_utils/Helper";
import { CheckCircleOutlined, ClockCircleOutlined, PoweroffOutlined, ReloadOutlined, SyncOutlined } from '@ant-design/icons';

type EventStreamData = {
  isError: boolean,
  maxStep: number,
  currentStep: number,
  message: string
}

type ActionButtonType = {
  icon: JSX.Element,
  isLoading: boolean,
  text: string,
  eventMsgs: EventStreamData[],
  percent: number,
  progress?: number,
  execute: ((event: any) => void) | null
}

export default withMyAlert(function Setting(props: {} & IWithMyAlertProps){
  function setLoading(btnName: string, isLoading: boolean){
    setButton((prev) => {
      prev[btnName].isLoading = isLoading;
      prev[btnName].progress = isLoading ? 0 : 100;
      return {
        ...prev
      }
    });
  }

  function setDefaultButton(text: string, icon: JSX.Element, name: string, uri: string):ActionButtonType {
    return {
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: text,
      icon: icon,
      execute: () => {
        setLoading(name, true);

        const sse = new EventSource(uri, {});
        sse.onmessage = (res) => {
          const resData = JSON.parse(res.data);
          const isLoading = resData.currentStep !== resData.maxStep;
          if(!isLoading) sse.close();
          if(resData.isError) props.popError("API Error", resData.message);

          const percent = _helper.getPercent100(resData.currentStep, resData.maxStep);
          setButton(prev => { 
            prev[name].percent = percent;
            prev[name].isLoading = isLoading;
            prev[name].eventMsgs.push(resData);
            return {
              ...prev
            };
          });
        }
      }
    }
  }
  
  const [button, setButton] = useState<{[key: string]: ActionButtonType}>({
    reload: setDefaultButton('Reload', <ReloadOutlined />,'reload', _uri.ReloadDatabase(0)),
    fullScan: setDefaultButton('Full Scan', <SyncOutlined />, 'fullScan', _uri.RescanDatabase(0)),
    checkApiUrl:{
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: 'Check Url',
      icon: <CheckCircleOutlined />,
      execute: null
    },
    checkMediaUrl:{
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: 'Check Url',
      icon: <CheckCircleOutlined />,
      execute: null
    },
    checkResMediaUrl:{
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: 'Check Url',
      icon: <CheckCircleOutlined />,
      execute: null
    },
  });

  return (
    <>
      <SettingForm 
        popInfo={props.popInfo}
        popApiError={props.popApiError}
        popError={props.popError}
      />

      <Divider orientation="left">
        Actions
      </Divider>
      <ActionButton {...button.reload} />
      <ActionButton {...button.fullScan} />

      <Divider orientation="left">
        Event Stream
      </Divider>
    </>
  );
});

function SettingForm(props: Required<IWithMyAlertProps>){
  const [apiBaseUrl, setApiBaseUrl] = useState<string>("");
  const [mediaBaseUrl, setMediaBaseUrl] = useState<string>("");
  const [resMediaBaseUrl, setResMediaBaseUrl] = useState<string>("");
  const [alwaysPortrait, setAlwaysPortrait] = useState<boolean>(false);
  const [muteVideo, setMuteVideo] = useState<boolean>(false);

  useEffect(() =>{
    setApiBaseUrl(_uri.GetApiBaseUrl());
    setMediaBaseUrl(_uri.GetMediaBaseUrl());
    setResMediaBaseUrl(_uri.GetResMediaBaseUrl());
    setAlwaysPortrait(_uri.GetAlwaysPortrait());
    setMuteVideo(_uri.GetMuteVideo());
  },[]);

  function checkUrl(url: string){
    Axios.get<any>(url)
      .then((response) => {
        props.popInfo(`${response.status} ${response.statusText}`, '', response.data);
      })
      .catch((error) => {
        props.popApiError(error);
      });
  }

  const [useCensorship, setUseCenshorship] = useState<boolean | null>(null);
  useEffect(() => {
    Axios.get<boolean>(_uri.Censorship())
      .then((response) => {
        setUseCenshorship(response.data)
      })
      .catch((error) => {
        props.popApiError(error);
      });
  },[]);

  function censorshipChanged(checked: boolean){
    Axios.post(_uri.Censorship(), { }, { params:{ status:checked } })
      .then((response) => {
        setUseCenshorship(checked)
      })
      .catch((error) => {
        props.popApiError(error);
      });
  }

  function pcAction(action: string){
    const url = action === 'Sleep' ? _uri.Sleep() : _uri.Hibernate();

    Axios.post(url)
      .then(res => {
        message.success(`${action} command sent`);
      })
      .catch(err => {
        message.error(JSON.stringify(err));
      });
  }

  return(
    <>
      <div style={{display:'flex', gap:'12px', marginBottom:'12px', marginTop:'4px'}}>
        <Button
          danger
          style={{height:'70px', width:'70px'}}
          onClick={() => { pcAction('Sleep'); }}
        >
          <ClockCircleOutlined style={{fontSize:36}} />
        </Button>
        <Button
          danger
          style={{height:'70px', width:'70px'}}
          onClick={() => { pcAction('Hibernate'); }}
        >
          <PoweroffOutlined style={{fontSize:36}} />
        </Button>
      </div>
      <RowBase
        col0={
          <Typography.Text strong>API</Typography.Text>
        }
        col1={
          <Input.Search 
            value={apiBaseUrl} 
            onChange={(e) => { if(!_constant.isPublic) setApiBaseUrl(e.target.value); }} 
            onSearch={() => { if(!_constant.isPublic) checkUrl(apiBaseUrl); }}
          />
        }
      />
      <RowBase
        col0={
          <Typography.Text strong>Media</Typography.Text>
        }
        col1={
          <Input 
            value={mediaBaseUrl} 
            onChange={(e) => { if(!_constant.isPublic) setMediaBaseUrl(e.target.value); }}
          />
        }
      />
      <RowBase
        col0={
          <Typography.Text strong>Res Media</Typography.Text>
        }
        col1={
          <Input 
            value={resMediaBaseUrl} 
            onChange={(e) => { if(!_constant.isPublic) setResMediaBaseUrl(e.target.value); }}
          />
        }
      />
      <RowBase
        col0={
          <Typography.Text strong>Always Portrait</Typography.Text>
        }
        col1={
          <Switch
            checked={alwaysPortrait}
            onChange={(val) => setAlwaysPortrait(val)}
            style={{width:"40px"}}
          />
        }
      />
      <RowBase
        col0={
          <Typography.Text strong>Mute Video</Typography.Text>
        }
        col1={
          <Switch
            checked={muteVideo}
            onChange={(val) => setMuteVideo(val)}
            style={{width:"40px"}}
          />
        }
      />
      <RowBase
        col0={
          <Typography.Text strong>Censorship</Typography.Text>
        }
        col1={
          <Switch
            checked={useCensorship ?? false}
            onChange={censorshipChanged}
            style={{width:"40px"}}
          />
        }
      />
      <RowBase
        col1={ 
          <Button
            type='primary' ghost
            onClick={() => {
              _uri.SaveApiBaseUrl(apiBaseUrl);
              _uri.SaveMediaBaseUrl(mediaBaseUrl);
              _uri.SaveResMediaBaseUrl(resMediaBaseUrl);
              _uri.SaveAlwaysPortrait(alwaysPortrait);
              _uri.SaveMuteVideo(muteVideo);
            }}
            style={{ width:"100%", maxWidth:"100px"}}
            disabled={_constant.isPublic}
          >
            Save
          </Button>
        }
      />
    </>
  );
}

function ActionButton(props: ActionButtonType){
  return(
    <>
    <div style={{display:"flex", flexDirection:"row", marginBottom:"5px"}}>
      <div style={{width:"130px"}}>
      <Button
        type='primary' ghost
        icon={props.icon}
        loading={props.isLoading}
        onClick={props.execute ?? (() =>{}) }
        block={true}
        style={{ width:"100%", maxWidth:"140px"}}
        disabled={_constant.isPublic}
      >
        {props.text}
      </Button>
      </div>
      <div style={{
        height:"32px", 
        flex:"auto", 
        marginLeft:"10px",
        display:"flex",
        justifyContent: "flex-end",
        flexDirection: "column"
      }}>
        <Progress strokeLinecap="square" percent={props.percent} />
      </div>
    </div>
    <div hidden={props.eventMsgs.length === 0}
      style={{display:"flex", flexDirection:"row", marginBottom:"5px"}}>
      <div style={{width:"130px"}}></div>
      <div style={{
        //height:"32px", 
        flex:"auto",
        marginLeft:"10px",
        paddingRight:"32px"
      }}>
        <Space direction="vertical">
          {props.eventMsgs.map((e, i) => <Typography.Text key={i}>{e.message}</Typography.Text>)}
        </Space>
      </div>
    </div>
    </>
  );
}

function RowBase(props: {
  col0?: ReactNode,
  col1?: ReactNode,
}){
  const style: CSS.Properties = {
    display:"flex", 
    flexDirection:"row", 
    marginBottom:"5px",
    textAlign: "right"
  };

  return(
    <>
    <div style={style}>
      <div style={{width:"130px"}}>
        {props.col0}
      </div>
      <div style={{
        height:"32px",
        width:"100%",
        textAlign:"left",
        marginLeft:"10px",
      }}>
        {props.col1}
      </div>
    </div>
    </>
  );
}