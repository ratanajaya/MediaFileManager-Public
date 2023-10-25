import React, { useState, useMemo } from 'react';

import { Row, Col, Drawer, Modal, Menu, Dropdown, Typography } from 'antd';
import {
  EditOutlined,
  DeleteOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';

import MyInputStar from '_shared/Editors/MyInputStar';
import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';
import withPageManager, { IWithPageManagerProps } from '_shared/HOCs/withPageManager';
import TextDialog from '_shared/Modals/TextDialog';
import * as _uri from '_utils/UriHelper';
import Axios from 'axios';
import { ChapterVM, NewFsNode, NodeType } from '_utils/Types';
import cssVariables from '_assets/styles/cssVariables';
import * as _helper from '_utils/Helper';

const { confirm } = Modal;

interface IReaderModalChapterDrawerProps{
  albumPath: string,
  fsNodes: NewFsNode[],
  type: 0 | 1,
  visible: boolean,
  currentPageIndex: number,
  onClose: () => void,
  onChapterRenameSuccess: (chapterName: string, newChapterName: string) => void,
  onChapterDeleteSuccess: (chapterName: string) => void,
  onJumpToPage: (pageIndex: number) => void,
  onChapterTierChangeSuccess: (chapterName: string, tier: number) => void
}

function ReaderModalChapterDrawer(props: IReaderModalChapterDrawerProps 
  & IWithPageManagerProps 
  & IWithMyAlertProps) {

  const chapters = useMemo(() => {
    let newChapters: ChapterVM[] = [];
    let i = 0;

    props.fsNodes
      .forEach(fs => {
        if(fs.nodeType !== NodeType.Folder){
          i++;
          return;
        }

        const childLen = fs.dirInfo!.childs.length;
        if(childLen === 0)
          return;

        newChapters.push({
          title: fs.dirInfo!.name,
          tier: fs.dirInfo!.tier,
          pageIndex: i,
          pageCount: childLen,
          pageUncPath: `${props.albumPath}\\${fs.dirInfo!.childs[0].alRelPath}`
        });

        i += childLen;
      });

      return newChapters;
  }, [props.fsNodes]);

  const [renameModal, setRenameModal] = useState<{
    visible: boolean,
    initialValue: string,
    onOk: (value: string, initialValue: string) => void
  }>({
    visible: false,
    initialValue: '',
    onOk: (value: string, initialValue: string) => {
      const movObj = {
        overwrite: false,
        src: {
          albumPath: props.albumPath,
          alRelPath: initialValue
        },
        dst: {
          albumPath: props.albumPath,
          alRelPath: value
        }
      };

      props.movePage(movObj, (response: any) => {
          props.onChapterRenameSuccess(initialValue, value);
        },(error: any) => {
          props.popApiError(error);
        });
    }
  });

  const updateChapterTier = (param: { 
    albumPath: string,
    chapterName: string,
    tier: number
  }) => {
    Axios.post(_uri.UpdateAlbumChapter(props.type), param)
        .then(function (response) {
          props.onChapterTierChangeSuccess(param.chapterName, param.tier);
        })
        .catch(function (error) {
          props.popApiError(error);
        });
  };

  const handler = {
    rename: (chapterTitle: string) => {
      setRenameModal(prev => {
        prev.visible = true;
        prev.initialValue = chapterTitle;
        return{ ...prev };
      });
    },
    delete: (chapterTitle: string) => {
      confirm({
        title: `Delete chapter ${chapterTitle}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   Yes   ',
        okType: 'danger',
        cancelText: '   No   ',
        onOk() {
          Axios.delete<number>(_uri.DeleteAlbumChapter(props.type, props.albumPath, chapterTitle))
            .then(function (response) {
              props.onChapterDeleteSuccess(chapterTitle);
            })
            .catch(function (error) {
              props.popApiError(error);
            });
        },
        onCancel() {
        },
      });
    },
    tierChange:(chapterTitle: string, value: number) => {
      updateChapterTier({ 
        albumPath:props.albumPath, 
        chapterName:chapterTitle, 
        tier:value 
      });
    }
  };

  return (
    <Drawer
      placement="left"
      closable={false}
      onClose={props.onClose}
      open={props.visible}
      width={300}
    >
      <div style={{ paddingTop: "10px", paddingBottom:"10px" }}>
        {chapters.map((chapter, index) =>{ 
          const isCurrentlyViewed = props.currentPageIndex >= chapter.pageIndex
            && (index === chapters.length - 1
              || props.currentPageIndex < chapters[index + 1].pageIndex);

          const chapterProgress = props.currentPageIndex - chapter.pageIndex  + 1;

          const rowStyle = isCurrentlyViewed ? {
            backgroundColor: cssVariables.highlightMid
          } : {};

          return (
            <Row style={{ ...rowStyle, padding:'2px 24px 2px 24px' }} key={chapter.title}>
              <Dropdown overlay={
                <Menu>
                  <Menu.Item key="1" onClick={() => handler.rename(chapter.title)}><EditOutlined />Rename</Menu.Item>
                  <Menu.Item key="2" onClick={() => handler.delete(chapter.title)}><DeleteOutlined />Delete</Menu.Item>
                </Menu>}
                trigger={['contextMenu']}
              >
                <Col span={8} onClick={() => props.onJumpToPage(chapter.pageIndex)}>
                  <img
                    style={{ objectFit: "contain", maxWidth: "100%", maxHeight: "100px", border: "1px solid white" }}
                    src={_uri.StreamResizedImage(chapter.pageUncPath, 150, props.type)}
                    alt="img"
                  >
                  </img>
                </Col>
              </Dropdown>
              <Col span={1}></Col>
              <Col span={15}>
                <Row>
                  <Col span={24}>
                    <Typography.Text>{chapter.title}</Typography.Text>
                  </Col>
                  <Col span={24} style={{ paddingLeft:'10px' }}>
                    <MyInputStar value={chapter.tier} colSpan={{ label:0, control:24 }} onChange={(label, value) => { handler.tierChange(chapter.title, value); }}  />
                  </Col>
                  <Col span={24}>
                    <Typography.Text>{isCurrentlyViewed ? `${chapterProgress}/` : ''}{chapter.pageCount}</Typography.Text>
                  </Col>
                </Row>
              </Col>
            </Row>
          )}
        )};
      </div>
      <TextDialog
        visible={renameModal.visible} 
        onClose={() => setRenameModal(prev => { prev.visible = false; return {...prev}; })}
        initialValue={renameModal.initialValue}
        onOk={renameModal.onOk}
      />
    </Drawer>
  );
}

export default withPageManager(withMyAlert(ReaderModalChapterDrawer));