import React from 'react';
import { RouteComponentProps } from 'react-router-dom';

import { Row, Col, Typography } from 'antd';
import useSWR from 'swr';

import MyAlbumCard from '_shared/Displays/MyAlbumCard';
import * as _uri from "_utils/UriHelper";
import { AlbumCardGroup, AlbumCardModel } from '_utils/Types';

interface IGenreProps{
  page: string,
  history: RouteComponentProps['history']
}

export default function Genres(props: IGenreProps) {
  const uri = props.page === "Genres" ? _uri.GetGenreCardModels() :
    props.page === "Artists" ? _uri.GetFeaturedArtistCardModels() :
      props.page === "Characters" ? _uri.GetFeaturedCharacterCardModels() :
        null;

  const { data: albumCGroups, error } = useSWR<AlbumCardGroup[], any>(uri);

  if (error) { return <Typography.Text>Error!</Typography.Text>; }
  if (!albumCGroups) { return <Typography.Text>Loading {props.page.toLowerCase()}...</Typography.Text>; }

  const readerHandler = {
    view: function (albumCm: AlbumCardModel) {
      props.history.push('/albums?query=' + albumCm.path)
    },
  }

  return (
    <>
      {albumCGroups.map((acg, index) =>{
        return (
          <Row key={"acgRow" + acg.name} gutter={0}>
            {acg.albumCms.map((a) => (
              <Col key={"albumCol" + a.path} style={{ textAlign: 'center' }} lg={4} md={6} sm={6} xs={12}>
                <MyAlbumCard
                  albumCm={a}
                  onView={readerHandler.view}
                  onEdit={() => { }}
                  showContextMenu={false}
                  showPageCount={true}
                  type={0}
                />
              </Col>
            ))}
          </Row>
        );
      })}
    </>
  );
}