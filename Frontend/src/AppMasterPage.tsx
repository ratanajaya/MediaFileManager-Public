import React, { useState, useEffect, useMemo } from 'react';
import { withRouter, Link, RouteComponentProps } from "react-router-dom";

import { Layout, Menu, Button, Row, Col, Modal, Typography } from 'antd';
import {
  DesktopOutlined, TeamOutlined, PartitionOutlined, TagsOutlined,
  ConsoleSqlOutlined, SearchOutlined,  
  LaptopOutlined, SettingOutlined, IdcardOutlined,
  PieChartOutlined, ProfileOutlined, BarChartOutlined, AppstoreOutlined, ControlOutlined
} from '@ant-design/icons';
import TextArea from 'antd/lib/input/TextArea';
import queryString from 'query-string';
import useSWR, { SWRConfig } from 'swr';

import SelfComps from 'SelfComps/SelfComps';
import Albums from 'Albums/Albums';
import Genres from 'Genres/Genres';
//import FileManagement from 'FileManagement/FileManagement';
import Setting from 'Setting/Setting';
import withMyAlert, { IWithMyAlertProps } from "_shared/HOCs/withMyAlert";
import * as _uri from "_utils/UriHelper";
import * as _helper from "_utils/Helper";
import _constant from "_utils/_constant";
import { QueryPart, QueryVm } from '_utils/Types';
import Dashboard from 'Dashboard/Dashboard';
import FileManagement from 'FileManagement/FileManagement';
import ScCorrection from 'ScCorrection/ScCorrection';

const { Content, Footer, Sider } = Layout;
const { SubMenu } = Menu;

interface IAppMasterPageProps extends RouteComponentProps<any>{
  page: string
}

function AppMasterPage(props: IAppMasterPageProps & IWithMyAlertProps) {
  const querStr = _helper.nz(queryString.parse(props.location.search).query as string, "");
  const pageStr = parseInt(_helper.nz(queryString.parse(props.location.search).page as string, "0"));
  const rowStr = parseInt(_helper.nz(queryString.parse(props.location.search).row as string, "0")) ;
  const pathStr = _helper.nz(queryString.parse(props.location.search).path as string, "");

  const queryParts: QueryPart = {
    query: querStr,
    page: pageStr,
    row: rowStr,
    path: pathStr
  };

  function getPageAndMenu(){
    if(props.page === "Sc"){
      return {
        page: <SelfComps queryParts={queryParts} history={props.history} />,
        selectedMenu: 'Sc'
      };
    }
    else if (props.page === "ScCorrection") {
      return {
        page: <ScCorrection queryParts={queryParts} />,
        selectedMenu: 'ScCorrection'
      };
    }
    else if (props.page === "Albums") {
      return {
        page: <Albums queryParts={queryParts} history={props.history} />,
        selectedMenu: '2'
      };
    }
    else if (props.page === "Artists") {
      return {
        page: <Genres page={props.page} history={props.history} />,
        selectedMenu: '3'
      };
    }
    else if (props.page === "Characters") {
      return {
        page: <Genres page={props.page} history={props.history} />,
        selectedMenu: '4'
      };
    }
    else if (props.page === "Genres") {
      return {
        page: <Genres page={props.page} history={props.history} />,
        selectedMenu: '5'
      };
    }
    else if (props.page === "FileManagement") {
      return {
        page: <FileManagement path={queryParts.path} type={0} />,
        selectedMenu: ''
      };
    }
    else if (props.page === "Setting") {
      return {
        page: <Setting />,
        selectedMenu: '6'
      };
    }
    else if (props.page === "QueryChart") {
      return {
        page: <Dashboard dashboard={props.page} />,
        selectedMenu: '7'
      };
    }
    else if (props.page === "Logs") {
      return {
        page: <Dashboard dashboard={props.page} />,
        selectedMenu: '8'
      };
    }
    return {
      page: <div></div>,
      selectedMenu: '9'
    }
  }

  const { page, selectedMenu } = useMemo(() => getPageAndMenu(), [props.page, props.location.search, props.history]);
  
  const styleMenuItem = { fontWeight:600 };

  const menuContent = (
    <>
      <div className="logo">
        {/* <span>{props.page === "Albums" ? albumCount : ''}</span> */}
      </div>
      <QueryEditor query={querStr} history={props.history} />
      <Menu theme='dark' defaultSelectedKeys={[selectedMenu]} defaultOpenKeys={["sub0"]} mode="inline">
        <Menu.Item key="2">
          <Link to="/albums">
            <DesktopOutlined />
            <span style={styleMenuItem}>Albums</span>
          </Link>
        </Menu.Item>
        <SubMenu key="sub0" title={
            <>
              <TagsOutlined />
              <span style={styleMenuItem}>Queries</span>
            </>
          }
        >
          <Menu.Item key="3">
            <Link to="/artists">
              <TeamOutlined />
              <span style={styleMenuItem}>Artists</span>
            </Link>
          </Menu.Item>
          <Menu.Item key="4">
            <Link to="/characters">
              <IdcardOutlined />
              <span style={styleMenuItem}>Characters</span>
            </Link>
          </Menu.Item>
          <Menu.Item key="5">
            <Link to="/genres">
              <PartitionOutlined />
              <span style={styleMenuItem}>Genres</span>
            </Link>
          </Menu.Item>
        </SubMenu>
        <SubMenu key="sub1" title={
            <>
              <BarChartOutlined />
              <span style={styleMenuItem}>Dashboad</span>
            </>
          }
        >
          <Menu.Item key="7">
            <Link to="/querychart">
              <PieChartOutlined />
              <span style={styleMenuItem}>Query Chart</span>
            </Link>
          </Menu.Item>
          <Menu.Item key="8">
            <Link to="/logs">
              <ProfileOutlined />
              <span style={styleMenuItem}>Logs</span>
            </Link>
          </Menu.Item>
        </SubMenu>
        {!_constant.isPublic ? 
          <SubMenu key="sub2" title={
              <>
                <LaptopOutlined />
                <span style={styleMenuItem}>Self Comp</span>
              </>
            }
          >
            <Menu.Item key="Sc">
              <Link to="/sc">
                <AppstoreOutlined />
                <span style={styleMenuItem}>Views</span>
              </Link>
            </Menu.Item>
            <Menu.Item key="ScCorrection">
              <Link to="/sccorrection">
                <ControlOutlined />
                <span style={styleMenuItem}>Correction</span>
              </Link>
            </Menu.Item>
          </SubMenu>
        : ''}
        <Menu.Item key="6">
          <Link to="/setting">
            <SettingOutlined />
            <span style={styleMenuItem}>Setting</span>
          </Link>
        </Menu.Item>
      </Menu>
      <div style={{ borderBottom:"1px solid grey", width:"100%", marginBottom:"10px" }}></div>
    </>
  );

  return (
    <SWRConfig value={{ revalidateOnFocus: false, fetcher: (uri) => fetch(uri).then(res => res.json()) }}>
      <Layout style={{ minHeight: '100vh' }}>
        <Sider breakpoint="lg" collapsedWidth="0">
          {menuContent}
        </Sider>
        <Layout className="site-layout">
          <Content>
            <div className="site-layout-background" style={{ paddingTop: 25, paddingLeft: 10, paddingRight: 10, minHeight: 360 }}>
              {page}
            </div>
          </Content>
          <Footer style={{ textAlign: 'center' }}></Footer>
        </Layout>
      </Layout>
    </SWRConfig>
  );
}

export default withRouter(withMyAlert(AppMasterPage));

function QueryEditor(props:{
  query: string,
  history: RouteComponentProps['history']
}) {
  const [visible, setVisible] = useState(false);
  const [query, setQuery] = useState("");

  useEffect(() => {
    setQuery(props.query);
  }, [props.query]);

  const queryHandler = {
    change: function(tagName: string) {
      const targetQueryPlus = "tag:" + tagName;
      const targetQueryMinus = "tag!" + tagName;

      const currentQueries = query !== "" ? query.split(',') : [];
      const cleanedQueries = currentQueries.filter((item, index) => !(item === targetQueryPlus || item === targetQueryMinus));

      const itemToAdd = !currentQueries.includes(targetQueryPlus) && !currentQueries.includes(targetQueryMinus) ? [targetQueryPlus] 
        : !currentQueries.includes(targetQueryMinus) ? [targetQueryMinus] : [];

      const newQueries = [...cleanedQueries, ...itemToAdd];

      setQuery(newQueries.join(','));
    },
    goToAlbumList: function () {
      let currentUrlParams = new URLSearchParams(window.location.search);
      currentUrlParams.set('query', query);
      setVisible(false);
      props.history.push("Albums?" + currentUrlParams.toString());
    }
  }
  
  function handleKeyDown(e: any){
    if(e.keyCode === 13 && e.shiftKey === false) {
      e.preventDefault();
      queryHandler.goToAlbumList();
    }
  }

  function getSelectStatus(tag: string) {
    const queryArr = query.split(',');

    return queryArr.some((str) => str.toLowerCase().includes("tag:" + tag.toLowerCase())) ? 'plus' :
      queryArr.some((str) => str.toLowerCase().includes("tag!" + tag.toLowerCase())) ? 'minus' :
        null;
  }

  const { data: tags, error } = useSWR<QueryVm[]>(_uri.GetTagVMs());

  return (
    <div style={{ paddingLeft: "26px" }}>
      <Button type="link" onClick={() => setVisible(true)} style={{ width: "90%", textAlign: "left", padding: "0px" }}>
        <ConsoleSqlOutlined /> 
        <span style={{ fontWeight:600 }}> Query Editor </span>
      </Button>
      <Modal
        open={visible}
        onCancel={() => setVisible(false)}
        closable={false}
        footer={
          <Button type="primary" onClick={queryHandler.goToAlbumList}>
            <SearchOutlined />Search
          </Button>
        }
      >
        <TextArea
          onChange={(event) => setQuery(event.target.value)}
          value={query}
          onKeyDown={handleKeyDown}
        />
        {error ? <Typography.Text>tags loading error...</Typography.Text> : !tags ? <Typography.Text>loading tags...</Typography.Text> : 
          <Row style={{marginTop:'10px'}}>
            {tags.map((tag, index) => {
              const selectStatus = getSelectStatus(tag.name);

              return (
                <Col span={6} key={tag.name}>
                  <Button
                    ghost={selectStatus !== null}
                    danger={selectStatus === 'minus'}
                    type={selectStatus ? "primary" : "link"}
                    onClick={() => { queryHandler.change(tag.name); }}
                    style={{ width: "100%", textAlign: "left", marginBottom: "2px" }}
                  >
                    {tag.name}
                  </Button>
                </Col>
              );
            })
            }
          </Row>
        }
      </Modal>
    </div>
  );
}