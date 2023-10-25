import React, { useState, useEffect, useRef } from 'react';
import 'App.css';
import Querycharts from 'pages/Querycharts';
import Logs from 'pages/Logs';
import { Environment } from 'common/types';

function App() {
  const defaultParam: Environment = {
    apiBase: "http://localhost:51474/",
    dashboard: 'Logs',
    cssVariables: {
      textMain: 'whitesmoke',
      bgL1: 'black',
      bgL2: 'black',
      bgL3: 'black'
    }
  };

  const paramFromUri = (() => {
    const paramBase64 = (new URLSearchParams(window.location.search)).get('param');
    if(paramBase64 == null)
      return null;

    return JSON.parse(window.atob(paramBase64)) as (Environment | null);
  })();

  console.log('paramFromUri', paramFromUri);

  const param = paramFromUri ?? defaultParam;

  function handleDispatch(detail?: any){
    const event = new CustomEvent('dashboardEvent', { 
      detail: {}
    });
    window.parent.document.dispatchEvent(event);
  }
  
  return (
    <div className="App" style={{
      backgroundColor: param.cssVariables.bgL3,
      color: param.cssVariables.textMain
    }}>
      {param.dashboard === 'QueryChart' 
        ? <Querycharts env={param} onDispatch={handleDispatch} />
        : param.dashboard === 'Logs' 
        ? <Logs env={param} onDispatch={handleDispatch} />
        : <div>Not Found</div>
      }
      
    </div>
  );
}

export default App;
