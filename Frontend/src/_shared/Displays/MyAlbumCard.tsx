import React from 'react';

import { Dropdown } from 'antd';
import { useInView } from 'react-intersection-observer';

import * as Helper from '_utils/Helper';
import * as _uri from '_utils/UriHelper';
import MyFlagIcon from '_shared/Displays/MyFlagIcon';
import { AlbumCardModel } from '_utils/Types';
import _constant from '_utils/_constant';
import * as _helper from '_utils/Helper';

function MyAlbumCard(props : {
  albumCm: AlbumCardModel,
  onView: (albumCm: AlbumCardModel) => void,
  onEdit?: (path: string) => void,
  showContextMenu: boolean,
  showPageCount?: boolean,
  showDate?: boolean,
  type: number
}) {
  const handler = {
    view: function () {
      props.onView(props.albumCm)
    },

    edit: function () {
      console.log('edit modal trigger called');
      if(props.onEdit) props.onEdit(props.albumCm.path);
    },
  }

  const [ref, inView, entry] = useInView({
    threshold: 0,
    rootMargin: "300px 0px 300px 0px"
  });
  
  return (
    <div ref={ref} className='my-album-card-container-1'>
      <div className='my-album-card-container-2'>
        <Dropdown 
          menu={{ items: [{key: '1',label: <></>}], style: {display:'none'} }} 
          onOpenChange={(open) => { if(open) handler.edit(); } } trigger={['contextMenu']} 
          destroyPopupOnHide={true} disabled={!props.showContextMenu}
        >
        <div className='my-album-card-container-3' onClick={handler.view}>
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
          
          {props.showPageCount ?
            <div className="album-pagecount">
              {props.albumCm.pageCount}
            </div> 
            // : props.albumCm.correctablePageCount > 0  ?
            // <div className="album-pagecount">
            //   {props.albumCm.correctablePageCount}
            // </div>
            : null
          }
          

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
        </Dropdown>
      </div>
      {props.showDate ?
        <div style={{border:"1px solid rgba(255,255,255,0.3)", width:"100%"}}>
          <span className='my-album-card-title'>
            {_helper.formatNullableDatetime(props.albumCm.entryDate)}
          </span>
        </div> : <></>
      }
      <span className='my-album-card-title'>
        {props.albumCm.fullTitle}
      </span>
    </div>
  );
}

export default MyAlbumCard;