import React from 'react';
import { HashRouter, Route, Switch, Redirect } from 'react-router-dom';

// import 'antd/dist/antd.css';
import '_assets/styles/App.scss';
import '_assets/styles/AntdOverride.scss'

import AppMasterPage from 'AppMasterPage';
import { ConfigProvider } from 'antd';
import { theme } from 'antd';
import Sandbox from 'Sandbox/Sandbox';

function App() {
  return (
    <ConfigProvider theme={{
      algorithm: theme.darkAlgorithm,
      // token: {
      //   colorBgLayout:'rgb(30,30,30)'
      // }
    }}>
      <HashRouter>
        <Switch>
          <Route exact path="/" ><Redirect to="/genres" /></Route>
          <Route exact path="/sc" ><AppMasterPage page="Sc" /></Route>
          <Route exact path="/albums" ><AppMasterPage page="Albums" /></Route>
          <Route exact path="/artists" ><AppMasterPage page="Artists" /></Route>
          <Route exact path="/characters" ><AppMasterPage page="Characters" /></Route>
          <Route exact path="/genres" ><AppMasterPage page="Genres" /></Route>
          <Route exact path="/querychart" ><AppMasterPage page="QueryChart" /></Route>
          <Route exact path="/logs" ><AppMasterPage page="Logs" /></Route>
          <Route exact path="/setting" ><AppMasterPage page="Setting" /></Route>
          <Route exact path="/filemanagement" ><AppMasterPage page="FileManagement" /></Route>
          <Route exact path="/sccorrection" ><AppMasterPage page="ScCorrection" /></Route>
          <Route exact path="/sandbox" ><Sandbox /></Route>
        </Switch>
      </HashRouter>
    </ConfigProvider>
  );
}

export default App;