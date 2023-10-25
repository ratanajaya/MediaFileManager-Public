import React, { ComponentType } from 'react';
import { Modal } from 'antd';
import { ModalFuncProps } from 'antd/lib/modal';

export interface IWithMyAlertProps {
  popInfo: (title: string, stringContent: string, objContent: any) => void,
  popError: (title: string, stringContent: string, objContent?: any) => void,
  popApiError: (error: any) => void
}

export default function withMyAlert<T>(OriginalComponent: ComponentType<T & IWithMyAlertProps>){
  return(hocProps: T) => {
    const width = 500;

    function popInfo(title: string, stringContent: string, objContent: any) {
      Modal.info({
        title: (title),
        content: (
          <>
            <span>{stringContent}</span>
            <pre>{objContent ? JSON.stringify(objContent, null, 2) : ""}</pre>
          </>
        ),
        onOk() { },
        width: width
      });
    }

    function popError(title: string, stringContent: string, objContent?: any) {
      Modal.error({
        title: (title),
        content: (
          <>
            <span>{stringContent}</span>
            <pre>{objContent ? JSON.stringify(objContent, null, 2) : ""}</pre>
          </>
        ),
        onOk() { },
        width: width
      });
    }

    function popApiError(error: any) {
      if (error.response === null || error.response === undefined) {
        Modal.error({
          title: ('No response'),
          content: (error.toString()),
          onOk() { },
          width: width
        });
        return;
      }
      const { status, statusText, data } = error.response;

      const content: ModalFuncProps = {
        title: `${status} - ${statusText}`,
        content: (
          <span>
            {JSON.stringify(data, null, 2)}
          </span>
        ),
        onOk() { },
        width: width
      };

      if (status >= 400 && status < 500) {
        Modal.warning(content);
      }
      else {
        Modal.error(content);
      }
    }

    return (
      <OriginalComponent
        {...hocProps}
        popInfo={popInfo}
        popError={popError}
        popApiError={popApiError}
      />
    )
  }
}