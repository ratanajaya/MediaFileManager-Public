import React, { useState, useEffect } from 'react';

import { Row, Col, Typography } from 'antd';

import MyAlbumCard from '_shared/Displays/MyAlbumCard';
import withMyAlert, { IWithMyAlertProps } from '_shared/HOCs/withMyAlert';
import PullToRefresh from 'react-simple-pull-to-refresh';
import * as _uri from '_utils/UriHelper';
import _constant from '_utils/_constant';
import Axios from 'axios';
import { AlbumCardModel, QueryPart } from '_utils/Types';
import useSWR from 'swr';
import * as _helper from '_utils/Helper';
import { RouteComponentProps } from 'react-router-dom';
import ReaderModal from '_shared/ReaderModal/ReaderModal';

interface ISelfCompProps{
  queryParts: QueryPart,
  history: RouteComponentProps['history']
}

const type = 1;

export default withMyAlert(function SelfComps(props: ISelfCompProps & IWithMyAlertProps) {
  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  const [selectedAlbumCm, setSelectedAlbumCm] = useState<AlbumCardModel | null>(null);

  const uri = _uri.GetAlbumCardModels(type, 0, 0, props.queryParts.path ?? "")
  const { data: albumCms, error, mutate } = useSWR<AlbumCardModel[], any>(uri);

  if (error) { return <pre>{JSON.stringify(error, undefined, 2)}</pre>; }
  if (!albumCms) { return <Typography.Text>Loading {_helper.nz(props.queryParts.path, "SelfComp")}...</Typography.Text>; }

  const readerHandler = {
    view: (pageCount: number, albumCm: AlbumCardModel) => {
      if(pageCount === 0){
        props.history.push("Sc?path=" + albumCm.path);
      }
      else{
        setSelectedAlbumCm(albumCm);
      }
    },

    close: (path: string, lastPageIndex: number) => {
      setSelectedAlbumCm(null);

      Axios.post(_uri.UpdateAlbumOuterValue(type), {
        albumPath: path,
        lastPageIndex: lastPageIndex
      })
        .then((response) => {
        })
        .catch((error) => {
          props.popApiError(error);
        });
        
        mutate(() => albumCms.map((albumCm, aIndex) => {
            if (albumCm.path !== path) {
              return albumCm;
            }
            return {
              ...albumCm,
              lastPageIndex: lastPageIndex
            };
          }), false);
    }
  }

  const crudHandler = {
    // pageDeleteSuccess: (path: string) => {
    //   mutate(() => albumCms.map((albumCm, aIndex) => {
    //       if (albumCm.path !== path) {
    //         return albumCm;
    //       }
    //       return {
    //         ...albumCm,
    //         pageCount: albumCm.pageCount - 1
    //       };
    //     }), false);
    // },
    chapterDeleteSuccess: (path: string, newPageCount: number) => {
      mutate(() => albumCms.map((albumCm, aIndex) => {
          if (albumCm.path !== path) {
            return albumCm;
          }
          return {
            ...albumCm,
            pageCount: newPageCount,
            lastPageIndex: 0
          };
        }), false);
    }
  }

  return (
    <>
      <PullToRefresh onRefresh={mutate}>
        <Row gutter={0}>
          {albumCms.map((a, index) => (
            <Col style={{ textAlign: 'center' }} {..._constant.colProps} key={"albumCol" + a.path}>
              <MyAlbumCard
                albumCm={a}
                onView={(albumCm) => readerHandler.view(a.pageCount, albumCm)}
                showContextMenu={true}
                showPageCount={a.pageCount > 0}
                type={type}
              />
            </Col>
          ))}
        </Row>
      </PullToRefresh>
      {selectedAlbumCm !== null ? 
        <ReaderModal
          onClose={readerHandler.close}
          //onPageDeleteSuccess={crudHandler.pageDeleteSuccess}
          onChapterDeleteSuccess={crudHandler.chapterDeleteSuccess}
          onOpenEditModal={() => {}}
          albumCm={selectedAlbumCm}
          type={type}
        />: <></>
      }
      
    </>
  );
})