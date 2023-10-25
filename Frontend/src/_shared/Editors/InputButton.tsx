import React from 'react';
import { Button, Input } from 'antd';
import { CloseOutlined } from '@ant-design/icons';

export default function InputButton(props : {
  value?: string[],
  onChange?: (value: string) => void,
  onClick?: () => void
}) {  
  return (
    <Input.Group compact style={{display:'flex'}}>
      <Input value={props.value} onChange={(val) => { props.onChange?.(val.target.value) }} style={{flex:'1'}} />
      <Button  onClick={props.onClick}><CloseOutlined /></Button>
    </Input.Group>
  );
}