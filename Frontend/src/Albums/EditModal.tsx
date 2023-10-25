import React, { useState, useEffect } from 'react';

import { Button, Checkbox, Modal, Form, Input, Radio, Rate, Collapse, Space, Typography, Spin, Carousel, Tooltip, Tag } from 'antd';
import useSWR from 'swr';
import Axios from 'axios';

import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';

import * as _helper from '_utils/Helper';
import * as _uri from '_utils/UriHelper';
import { Album, AlbumInfoVm, AlbumVM, ScrapOperation, Comment } from '_utils/Types';
import Multicheck from '_shared/Editors/Multicheck';
import InputButton from '_shared/Editors/InputButton';
import Multitag from '_shared/Editors/Multitag';
import { CaretRightFilled, CheckCircleOutlined, ClockCircleOutlined, CloseCircleOutlined, EditFilled, SyncOutlined } from '@ant-design/icons';
import moment from 'moment';
import cssVariables from '_assets/styles/cssVariables';

const { Item, useForm } = Form;

export default withMyAlert(function EditModal(props: {
  type: 0 | 1,
  albumPath: string | null,
  onOk: (albumVm: AlbumVM) => void,
  onEditFiles: (albumVm: AlbumVM) => void,
  onCorrectFiles: (albumVm: AlbumVM) => void,
  onRefresh: (albumVm: AlbumVM) => void,
  onDelete: (albumVm: AlbumVM) => void,
  onCancel: () => void,
} & IWithMyAlertProps) {
  const { data: albumInfo, error: aiError } = useSWR<AlbumInfoVm>(_uri.GetAlbumInfo());

  const [albumVm, setAlbumVm] = useState<AlbumVM>({
    path: "", //not updated
    pageCount: 0, //not updated
    lastPageIndex: 0, //not updated
    correctablePageCount: 0, //not updated
    album: {
      title: "",
      category: "Manga",
      orientation: "Portrait",
      artists: [],
      tags: [],
      characters: [],
      languages: [],
      note: "",
      tier: 0,
      isWip: false,
      isRead: false,
      entryDate: null
    }
  });

  const [form] = useForm();
  const album = albumVm.album;
  useEffect(() => {
    form.resetFields();
  },[albumVm]);

  useEffect(() => {
    if(props.albumPath === null){
      form.resetFields();

      return;
    }
    Axios.get<AlbumVM>(_uri.GetAlbumVm(props.type, props.albumPath))
      .then(function (response) {
        setAlbumVm(response.data);
      })
      .catch(function (error: any) {
        props.popApiError(error);
      });

  }, [props.albumPath]);

  const handlers = {
    albumVmChange: function (label: string, value: any) {
      let newAlbumVm = { ...albumVm };
      let cleanedValue: any = value;
      if (label === "Artists") {
        cleanedValue = value.split(",");
      }
      else if (label === "Tags" || label === "Languages" || label === "Characters") {
        cleanedValue = albumVm.album[_helper.firstLetterLowerCase(label) as keyof Album];
        if (cleanedValue.includes(value)) {
          cleanedValue = cleanedValue.filter((a: string) => a !== value);
        }
        else {
          cleanedValue.push(value);
        }
      }
      //@ts-ignore
      newAlbumVm.album[_helper.firstLetterLowerCase(label) as keyof Album] = cleanedValue;
      setAlbumVm(newAlbumVm);
    },
    ok: function () {
      const formVal = form.getFieldsValue();
      const newAlbumVm: AlbumVM = {
        ...albumVm,
        album: {
          ...albumVm.album,
          ...formVal,
          artists: formVal.artists.split(','),
          isWip: formVal.flags.includes('isWip'),
          isRead: formVal.flags.includes('isRead'),
        }
      }

      props.onOk(newAlbumVm);
    },
  }

  const [activeKeys, setActiveKeys] = useState<string | string[]>(['panelMetadata']);

  if(props.albumPath == null) return <></>;

  return(
    <Modal 
      open={true}
      onCancel={props.onCancel}
      closable={false}
      width={580}
      //title={props.albumPath.split('\\').slice(-1)[0]}
      footer={
        <div style={{width:'100%', display:'flex', gap:'10px'}}>
          <div style={{flex:'1'}}>
            <Button 
              danger
              style={{width:'100%'}}
              onClick={() => props.onDelete(albumVm)} 
            >
              Delete
            </Button>
          </div>
          <div style={{flex:'1'}}>
            <Button 
              style={{width:'100%'}}
              onClick={() => props.onRefresh(albumVm)} 
            >
              Refresh
            </Button>
          </div>
          <div style={{flex:'1'}}>
            <a 
              href={`/#/ScCorrection?path=${encodeURIComponent(props.albumPath)}`}
              style={{
                width: '100%', 
                display: 'flex', 
                justifyContent: 'center', // This is for horizontal centering
                alignItems: 'center',     // This is for vertical centering
                height: '100%'            // Take the full height of the parent
              }}
            >
              Correction
            </a>
          </div>
          <div style={{flex:'1'}}>
            <Button 
              type='primary' ghost
              style={{width:'100%'}}
              onClick={handlers.ok} 
            >
              Save
            </Button>
          </div>
        </div>
      }
    >
      {!albumInfo || albumVm.path === '' ? "loading..." : (
        <Collapse 
          bordered={false}
          expandIcon={({ isActive }) => <CaretRightFilled rotate={isActive ? 90 : 0} />}
          style={{marginBottom:'10px'}}
          accordion
          activeKey={activeKeys}
          onChange={setActiveKeys}
        >
          <Collapse.Panel className='editPanel' header='Comments' key='panelComment'>
            <CommentPanel albumPath={albumVm.path} open={activeKeys === 'panelComment'} popApiError={props.popApiError} />
          </Collapse.Panel>
          <Collapse.Panel className='editPanel' header='Metadata' key='panelMetadata'>
            <Form
              form={form}
              layout='horizontal'
              size='middle'
              colon={false}
              labelCol={{
                span: 4,
              }}
              wrapperCol={{
                span: 20,
              }}
              initialValues={{
                'title': album.title,
                'artists': album.artists.join(','),
                'category': album.category,
                'orientation': album.orientation,
                'tags': album.tags,
                'languages': album.languages,
                'characters': album.characters,
                'note': album.note,
                'tier': album.tier,
                'flags': [
                  ...(album.isWip ? ['isWip'] : []), 
                  ...(album.isRead ? ['isRead'] : [])
                ]
              }}
            >
              <Item label="Titlex" name="title">
                <Input.TextArea rows={3} />
              </Item>
              <Item label="Artists" name="artists">
                <Input />
              </Item>
              <Item label="Category" name="category">
                <Radio.Group>
                  {albumInfo.categories.map((a, i) => (
                    <Radio key={a} value={a} style={{width:'100px'}}>{a}</Radio>
                  ))}
                </Radio.Group>
              </Item>
              <Item label="Orientation" name="orientation">
                <Radio.Group>
                  {albumInfo.orientations.map((a, i) => (
                    <Radio key={a} value={a} style={{width:'100px'}}>{a}</Radio>
                  ))}
                </Radio.Group>
              </Item>
              <Item label="Tags" name="tags">
                <Multicheck items={albumInfo.tags} />
              </Item>
              <Item label="Languages" name="languages">
                <Multicheck items={albumInfo.languages} />
              </Item>
              <Item label="Characters" name="characters">
                <Multitag items={albumInfo.characters} />
              </Item>
              <Item label="Note" name="note">
                <InputButton onClick={() => { form.setFieldValue('note', ''); }} />
              </Item>
              <Item label="Tier" name="tier">
                <Rate count={3} />
              </Item>
              <Item label="Flags" name="flags">
                <Checkbox.Group>
                  <Checkbox value="isWip" style={{width:'100px'}}>
                      WIP
                    </Checkbox>
                  <Checkbox value="isRead" style={{width:'100px'}}>
                    Read
                  </Checkbox>
                </Checkbox.Group>
              </Item>
            </Form>
          </Collapse.Panel>
        </Collapse>
      )}
    </Modal>
  );
})

const commentDummy : Comment[] = [
  {
    id: 1,
    scrapOperationId: 1,
    author: 'John',
    content: 'Lorem ipsum',
    score: 12,
    postedDate: '2022-09-27 18:00:00.000'
  },
  {
    id: 2,
    scrapOperationId: 1,
    author: 'Ratanajaya Dr',
    content: 'When dates are represented with numbers they can be interpreted in different ways. For example, 01\/05\/22 could mean January 5, 2022, or May 1, 2022. On an individual level this uncertainty can be very frustrating, in a business context it can be very expensive. Organizing meetings and deliveries, writing contracts and buying airplane tickets can be very difficult when the date is unclear.\r\n\r\nISO 8601 tackles this uncertainty by setting out an internationally agreed way to represent dates:\r\n\r\nYYYY-MM-DD\r\n\r\nTherefore, the order of the elements used to express date and time in ISO 8601 is as follows: year, month, day, hour, minutes, seconds, and milliseconds.',
    score: null,
    postedDate: '2023-01-27 12:00:00.000'
  },
  {
    id: 3,
    scrapOperationId: 1,
    author: 'Commenter 1',
    content: 'If you\'re in at-will employment then of course almost anything is \"reasonable\" so far as the law is concerned. They can require you to take a 50% pay cut and wear a clown suit if they like, and you can either find a smaller house and suit up, or you can quit and bad-mouth them on Glassdoor.',
    score: -3,
    postedDate: '2023-01-27 13:12:11.000'
  },
  {
    id: 4,
    scrapOperationId: 1,
    author: 'Commenter 1',
    content: 'If you\'re in at-will employment then of course almost anything is \"reasonable\" so far as the law is concerned. They can require you to take a 50% pay cut and wear a clown suit if they like, and you can either find a smaller house and suit up, or you can quit and bad-mouth them on Glassdoor.',
    score: -3,
    postedDate: '2023-01-27 13:12:12.000'
  },
];

function CommentPanel(props: {
  albumPath: string,
  open: boolean,
  popApiError: (error: any) => void
}){
  const [loading, setLoading] = useState<boolean>(false);
  const [scrapOperations, setScrapOperations] = useState<ScrapOperation[] | null>(null);
  const [scrapEditing, setScrapEditing] = useState<boolean>(false);
  const [newSource, setNewSource] = useState<string>('');
  const [scrapComments, setScrapComments] = useState<{
    scrapOperationId: number,
    comments: Comment[]
  }[]>([]);
  const [openScrapId, setOpenScrapId] = useState<number | null>(null);

  useEffect(() => {
    if(!props.open || scrapOperations != null)
      return;
      setLoading(true);

      Axios.get<ScrapOperation[]>(_uri.GetScrapOperations(props.albumPath))
        .then((response) => {
          setScrapOperations(response.data);
          if(response.data.length > 0)
            setOpenScrapId(0);
        })
        .catch((error: any) => props.popApiError(error))
        .finally(() => setLoading(false));
  },[props.open]);

  const handlers = {
    addSource: () => {
      setLoading(true);

      Axios.post<ScrapOperation>(_uri.InsertScrapOperation(), {albumPath: props.albumPath, source: newSource})
        .then((response) => {
          setScrapOperations(prev => {
            if(prev == null)
              return [response.data];

            return [...prev, response.data];
          });

          setNewSource('');
        })
        .catch((error: any) => props.popApiError(error))
        .finally(() => setLoading(false));
    },
    updateSource: (id: number) => {
      setLoading(true);
      const idx = scrapOperations!.findIndex(a => a.id === id);
      const param = scrapOperations![idx];

      Axios.post<ScrapOperation>(_uri.UpdateScrapOperation(), param)
        .then((response) => {
          setNewSource('');
          setScrapEditing(false);
        })
        .catch((error: any) => props.popApiError(error))
        .finally(() => setLoading(false));
    },
    setOpenIdx: (slideIdx: number) => {
      console.log('setOpenIdx ', slideIdx);
      if(!scrapOperations) return;

      const scrapOp = scrapOperations[slideIdx];
      
      setOpenScrapId(scrapOp.id);

      if(scrapComments.some(a => a.scrapOperationId === scrapOp.id))
        return;

      setLoading(true);

      Axios.get<Comment[]>(_uri.GetComments(scrapOp.id))
        .then(res => {
          setScrapComments(prev => {
            return[...prev, {
              scrapOperationId: scrapOp.id,
              comments: res.data
            }];
          })
        })
        .catch((error: any) => props.popApiError(error))
        .finally(() => setLoading(false));
    }
  }

  const comments = scrapComments.find(a => a.scrapOperationId === openScrapId)?.comments ?? [];

  return(
    <Spin spinning={loading}>
      <div style={{width:'100%'}}>
        <div>
          <Input.Group compact>
            <Input
              style={{
                width: 'calc(100% - 60px)',
              }}
              value={newSource}
              onChange={(event) => setNewSource(event.target.value)}
            />
            <Button onClick={handlers.addSource} style={{width:'60px'}}>Add</Button>
          </ Input.Group>
        </div>
        <div>
          <Carousel dotPosition='top' style={{color:'white'}} beforeChange={(curr, next) => { handlers.setOpenIdx(next); setScrapEditing(false); } }>
            {scrapOperations?.map((a, i) => (
              <div key={a.id}>
                <div className='divider-18'></div>
                <div style={{display:'flex', width:'100%'}}>
                  {scrapEditing ? 
                    <div style={{flex:'1'}}>
                      <Input.Group compact>
                        <Input
                          style={{
                            width: 'calc(100% - 60px)',
                          }}
                          value={a.source}
                          onChange={(event) => {
                            setScrapOperations(prev => {
                              if(!prev) return null;
                              
                              prev[i].source = event.target.value;
                              
                              return[...prev];
                            })
                          }}
                        />
                        <Button onClick={() => handlers.updateSource(a.id)} style={{width:'60px'}}>Save</Button>
                      </ Input.Group>
                    </div> :
                    <>
                      <div style={{flex:'1'}}>
                        <Typography.Text style={{marginLeft:'12px'}}>{a.source}</Typography.Text>
                      </div>
                      <div style={{width:'30px', fontSize:'18px', textAlign:'right'}} onClick={() => setScrapEditing(true)}>
                        <EditFilled />
                      </div>
                    </>
                  }
                </div>
                <div style={{marginLeft:'10px'}}>
                  {
                    a.status === 'Pending' ? <Tag icon={<ClockCircleOutlined />} color="default"> {a.status} </Tag> :
                    a.status === 'Processing' ? <Tag icon={<SyncOutlined spin />} color="processing"> {a.status} </Tag> :
                    a.status === 'Error' ? <Tag icon={<CloseCircleOutlined />} color="error"> {a.status} </Tag> :
                    a.status === 'Success' ? <Tag icon={<CheckCircleOutlined />} color="success"> {a.status} </Tag> :
                    null
                  }
                </div>                
                <div className='divider-4'></div>
                <div style={{ width:'100%', maxHeight:'580px', overflow:'auto' }}>
                  {comments.map(a => (
                    <div key={a.id} className='comment-box'>
                      <div className='comment-author'>
                        {a.author}
                      </div>
                      <div className='divider-2' />
                      <div style={{display:'flex', justifyContent:'space-between'}}>
                        <Tooltip title={moment(a.postedDate).format('hh:mm')}>
                          <span className='comment-timestamp'>
                            {moment(a.postedDate).format('yyyy-MMM-DD')}
                          </span>
                        </Tooltip>
                        {a.score != null ?
                          <span className='comment-score' style={{color: (a.score < 0 ? 'red' : 'lime')}}>
                            {a.score < 0 ? '' : '+'}{a.score}
                          </span>: null
                        }
                      </div>
                      <div className='divider-6' />
                      <div className='comment-content'>
                        {a.content}
                      </div>
                    </div>
                  ))}
                </div>
                <div className='divider-8'></div>
              </div>
            ))}
          </Carousel>
        </div>
      </div>
    </Spin>
  );
}