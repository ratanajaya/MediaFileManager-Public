import React, { ComponentType, CSSProperties } from 'react';
import { Modal } from 'antd';
import * as _uri from '_utils/UriHelper';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import moment from 'moment';
import { HasLibraryType, MoveFileInfo, MoveFileResponse, PageDeleteModel, PageMoveModel } from '_utils/Types';
import Axios from 'axios';

const { confirm } = Modal;

export interface IWithPageManagerProps {
  deletePage: (
    delObj: PageDeleteModel, 
    directDelete: boolean,
    onSuccess: (param: any) => void, 
    onError: (param: any) => void
  ) => void,
  movePage: (
    movObj: PageMoveModel, 
    onSuccess: (param: any) => void, 
    onError: (param: any) => void
  ) => void
}

export default function withPageManager<T extends HasLibraryType>(OriginalComponent: ComponentType<T & IWithPageManagerProps>) {
  return (hocProps: T) => {
    function deletePage(
      delObj: PageDeleteModel,
      directDelete: boolean,
      onSuccess: (param: any) => void, 
      onError: (param: any) => void
    ){
      const deleteAction = () => {
        Axios.delete(_uri.DeleteFile(hocProps.type, delObj.albumPath, delObj.alRelPath))
            .then(onSuccess)
            .catch(onError);
      }

      if (directDelete) {
        deleteAction();
        return;
      }

      confirm({
        title: `Delete ${delObj.alRelPath}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   YES   ',
        okType: 'danger',
        cancelText: '   NO   ',
        onOk() {
          deleteAction();
        },
        onCancel() {
        },
      });
    }

    function movePage(
      movObj: PageMoveModel, 
      onSuccess: (param: any) => void, 
      onError: (param: any) => void
    ){
      Axios.post<MoveFileResponse>(_uri.MoveFile(hocProps.type), movObj)
        .then((response) =>{
          if (response.data.message === "Destination file already exist") {
            confirm({
              title: `Overwrite?`,
              icon: <ExclamationCircleOutlined />,
              content: <FileComparison 
                file1={response.data.srcInfo ?? {} as MoveFileInfo} file2={response.data.dstInfo ?? {} as MoveFileInfo} 
              />,
              okText: '   YES   ',
              okType: 'danger',
              cancelText: '   NO   ',
              onOk() {
                const newMovObj = {
                  ...movObj,
                  overwrite: true
                }

                movePage(newMovObj, onSuccess, onError);
              },
              onCancel() {
              },
            });
          }
          else{
            onSuccess(response);
          }
        })
        .catch(onError);
    }

    return (
      <OriginalComponent
        {...hocProps}
        deletePage={deletePage}
        movePage={movePage}
      />
    )
  }
}

function FileComparison(props: {
  file1: MoveFileInfo,
  file2: MoveFileInfo
}) {
  const f1 = props.file1;
  const f2 = props.file2;

  return (
    <table className="comparison-table" style={{ width: "100%" }}>
      <tr>
        <th>Source</th>
        <th>Destination</th>
      </tr>
      <tr>
        <td>{f1?.name}</td>
        <td>{f2?.name}</td>
      </tr>
      <tr>
        <td style={getStyle(f1.size, f2.size)}>{(f1.size / 1024).toFixed(2)} KB</td>
        <td style={getStyle(f2.size, f1.size)}>{(f2.size / 1024).toFixed(2)} KB</td>
      </tr>
      <tr>
        <td style={getStyle(f1.createdDate, f2.createdDate)}>{moment(f1.createdDate).format('YYYY-MM-DD hh:mm')}</td>
        <td style={getStyle(f2.createdDate, f1.createdDate)}>{moment(f2.createdDate).format('YYYY-MM-DD hh:mm')}</td>
      </tr>
    </table>
  );

  function getStyle(a: string | number, b: string | number) : CSSProperties {
    return {
      fontWeight: a > b ? "bold" : "initial"
    }
  }
}