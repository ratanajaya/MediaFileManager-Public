import React from 'react';
import { Button, Typography } from 'antd';
import { useInView } from 'react-intersection-observer';
import * as Helper from '_utils/Helper';
import * as _uri from '_utils/UriHelper';
import MyFlagIcon from '_shared/Displays/MyFlagIcon';
import { AlbumCardModel } from '_utils/Types';

export default function MyAlbumWideCard(props: {
  type: number,
  albumCm: AlbumCardModel,
  onView: (albumCm: AlbumCardModel) => void,
  onEdit: (path: string) => void,
  onDelete: (path: string) => void,
}) {
  const [ref, inView, entry] = useInView({
    threshold: 0,
    rootMargin: "300px 0px 300px 0px"
  });

  return(
    <div ref={ref} style={{display:"flex", flexDirection:"row", justifyContent:"space-between", alignItems:"stretch", columnGap:"10px", width:"100%", marginBottom: '8px' }}>
      <div className='my-album-card-container-2' style={{width:"150px"}}>
        <div className='my-album-card-container-3' onClick={() => { props.onView(props.albumCm); }}>
          {inView ? 
            <img className='my-album-card-img' alt="img"
              src={_uri.StreamResizedImage(props.albumCm.coverInfo.uncPathEncoded, 150, props.type)}
            /> : null
          }

          <div className='my-album-card-flag'>
            <div style={{flex:"4", display:"flex"}}>
              {!props.albumCm.isRead ?
                <MyFlagIcon flagType={"New"} /> : ""
              }
              {props.albumCm.isWip ?
                <MyFlagIcon flagType={"Wip"} /> : ""
              }
            </div>
            <div style={{flex:"6", textAlign:"right"}}>
              {props.albumCm.languages.map((item, i) => <MyFlagIcon flagType={item} key={i} />)}
            </div>
          </div>

          <div className='my-album-card-tierbar'>
            {[3, 2, 1].map((e, i) =>
              <div key={`tierBar-${i}`}
                style={{
                  width: "100%",
                  height: "33%",
                  backgroundColor: Helper.ColorFromIndex(props.albumCm.tier, e),
                  borderTop: Helper.BorderFromIndex(props.albumCm.tier, e),
                  borderRight: Helper.BorderRightFromIndex(props.albumCm.tier, e)
                }} />
            )}
          </div>

          <div className='my-album-card-progressbar' style={{ display: props.albumCm.lastPageIndex > 0 ? "block" : "none" }}>
            <div style={{
              backgroundColor: "DodgerBlue",
              height: "100%",
              width: `${Helper.getPercent100(props.albumCm.lastPageIndex + 1, props.albumCm.pageCount)}%`
            }} />
          </div>

          {props.albumCm.note !== null && props.albumCm.note !== "" ?
            <div className="album-note">
              {props.albumCm.note}
            </div> : ""
          }

          <div>
          </div>
        </div>
      </div>
      <div style={{flex:"1", paddingTop:"12px"}}>
        <Typography.Text>
          {props.albumCm.fullTitle}
        </Typography.Text>
      </div>
      <div style={{width:"50px", paddingTop:"8px"}}>
        <Button 
          ghost
          type='primary'
          onClick={() => props.onEdit(props.albumCm.path) }
        >
          Edit
        </Button>
      </div>
      <div style={{width:"50px", paddingTop:"8px"}}>
        <Button 
          danger
          onClick={() => props.onDelete(props.albumCm.path) } 
        >
          Delete
        </Button>
      </div>
    </div>
  );
}
