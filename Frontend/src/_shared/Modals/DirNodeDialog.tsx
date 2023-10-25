import React, { useState, useEffect } from 'react';
import { Modal, Button, Tree, Space } from 'antd';
import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';
import * as _uri from '_utils/UriHelper';
import Axios from 'axios';
import { FsNode } from '_utils/Types';
import { DataNode } from 'antd/lib/tree';

const { DirectoryTree } = Tree;

interface DirNodeDialogProps{
  open: boolean,
  onClose: () => void,
  onOk: (selected?: string) => void
}

function DirNodeDialog(props: DirNodeDialogProps & IWithMyAlertProps) {
  const [nodes, setNodes] = useState<FsNode[]>([]);
  const [selected, setSelected] = useState<string>();

  useEffect(() => {
    Axios.get<FsNode[]>(_uri.GetLibraryDirNodes(1, '', true))
      .then((response) => {
        setNodes(response.data);
      })
      .catch((error) => {
        props.popApiError(error);
      });
  },[]);

  return (
    <Modal
      open={props.open}
      footer={null}
      centered={true}
      closable={false}
      onCancel={() => props.onClose()}
      style={{ width: 400 }}
    >
      <Space direction='vertical' style={{width:'100%'}}>
        <div style={{overflow:'auto', maxHeight:'90vh'}}>
          <MyDirTree 
            nodes={nodes}
            onSelect={setSelected}
          />
        </div>
        <Button
          style={{ width: "100%" }} type="primary"
          onClick={() => { props.onOk(selected); props.onClose(); }}
        >
          Move Here
        </Button>
      </Space>
    </Modal>
  );
}

export default withMyAlert(DirNodeDialog);

function MyDirTree(props: {
  nodes: FsNode[],
  onSelect: (key: string) => void,
}){
  function toDataNode(fsNode: FsNode):DataNode {
    return {
      title: fsNode.name,
      key: fsNode.name,
      isLeaf: !fsNode.childs || fsNode.childs.length === 0,
      children: !fsNode.childs ? [] : fsNode.childs.map(child => toDataNode(child)),
      
    };
  }

  const dataNodes = props.nodes.map(fsNode => toDataNode(fsNode));

  return (
    <DirectoryTree 
      treeData={dataNodes}
      onSelect={(keys, info) => { if(keys.length > 0) props.onSelect(keys[0].toString()) }}
    />
  );
}