import { Spin } from 'antd'
import React from 'react'
import cssVariables from '_assets/styles/cssVariables';
import { LoadingOutlined } from '@ant-design/icons';

export default function Spinner(props: {
  loading: boolean
}) {
  return true ? (
    <div style={{
      position:'fixed',
      height: '100%',
      width: '100%',
      top: 0,
      right: 0,
      zIndex: 999,
      background: cssVariables.overlayMain,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
    }}>
      <Spin size='large' spinning={true} indicator={<LoadingOutlined style={{fontSize: 128}} />} />
    </div>
  ) : <></>
}
