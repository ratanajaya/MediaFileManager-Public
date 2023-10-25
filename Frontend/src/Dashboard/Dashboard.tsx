import React, { useState, useRef } from 'react';
import * as _uri from '_utils/UriHelper';
import cssVariables from '_assets/styles/cssVariables';

export default function Dashboard(props:{
  dashboard: string
}) {
  const iframeRef = useRef(null);

  const [iframeHeight, setIframeHeight] = useState('200px');

  window.document.addEventListener("dashboardEvent", (event => {
    //setTimeout(() => {
      //@ts-ignore
      const sh2 = iframeRef.current.contentWindow.document.body.scrollHeight;
      console.log('from parent sh2', sh2);
      setIframeHeight(`${sh2}px`);
    //}, 300);
  }));

  const param={
    apiBase: _uri.GetApiBaseUrl(),
    dashboard: props.dashboard,
    cssVariables: cssVariables
  }

  //const [url, setUrl] = useState('');
  
  const paramBase64 = window.btoa(JSON.stringify(param));
  console.log(paramBase64);

  const protocol = window.location.protocol;
  const url = ['http', 'https'].includes(protocol) ?
    `/Dashboard?param=${paramBase64}` :
    `Dashboard/index.html?param=${paramBase64}`;

  return (
    <div>
      <iframe
        id="DashboardIframe"
        ref={iframeRef}
        title='Dashboard'
        src={url}
        style={{ width: '100%', height:iframeHeight, border:0, overflow:'hidden' }}
      />
    </div>
  )
}
