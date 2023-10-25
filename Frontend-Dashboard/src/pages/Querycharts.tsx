import React, { useState, useEffect } from 'react';
import { TierFractionModel, Environment } from 'common/types';
import Axios from 'axios';
import { Row, Col, Stack, InputGroup, Form, Button } from 'react-bootstrap';

import {
  Chart as ChartJS,
  RadialLinearScale,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';
import { PolarArea, Doughnut } from 'react-chartjs-2';

ChartJS.register(RadialLinearScale, ArcElement, Tooltip, Legend);

export default function Querycharts(props:{
  env: Environment,
  onDispatch: (detail?: any) => void
}) {
  const [tierFractions, setTierFractions] = useState<TierFractionModel[]>([]);

  const [query, setQuery] = useState<string>('');
  const [includeNew, setIncludeNew] = useState<boolean>(true);

  useEffect(() => {
    Axios.get<TierFractionModel[]>(`${props.env.apiBase}Dashboard/GetGenreTierFractions`)
      .then((response) => {
        setTierFractions(response.data)
      })
      .catch((error) => {
        console.log(error);
      });
  },[]);

  useEffect(() => {
    if(tierFractions.length > 0){
      props.onDispatch();
    }
  },[tierFractions]);

  function handleAddQuery(){
    Axios.get<TierFractionModel>(`${props.env.apiBase}Dashboard/GetQueryTierFraction?query=${query}`)
      .then((response) => {
        setQuery('');

        setTierFractions(prev => {
          return [response.data, ...prev];
        });
      })
      .catch((error) => {
        console.log(error);
      });
  }

  return (
    <>
      <Stack gap={1}>
        <InputGroup >
          <Form.Control
            placeholder="Query"
            value={query}
            onChange={event => { setQuery(event.target.value); }}
            onKeyDown={(event) => {
              if(event.key === 'Enter')
                handleAddQuery();
            }}
          />
          <Button 
            onClick={handleAddQuery}
            variant="outline-secondary" 
            style={{width:'100px'}}
          >
            Add
          </Button>
        </InputGroup>
        <div style={{alignSelf:'flex-end'}}>
          <Form.Check 
            type="switch"
            label="Include New"
            checked={includeNew}
            onChange={(event) => setIncludeNew(event.target.checked)}
            style={{ color:'whitesmoke'}}
          />
        </div>
      </Stack>
      <Row>
        { 
          tierFractions.map(tf =>{
            const { t0, t1, t2, t3, ts, tn } = tf;

            const data = {
              labels: [ ...(includeNew ? ['New'] : []), 'Tier 1', 'Tier 2', 'Tier 3', 'Tier S' ],
              datasets: [
                {
                  label: '# of Albums',
                  data: [ ...(includeNew ? [tn] : []), t1, t2, t3, ts],
                  backgroundColor: [
                    ...(includeNew ? ['hsla(60, 70%, 60%, 0.6)'] : []),
                    'hsla(0, 70%, 60%, 0.6)',
                    'hsla(100, 70%, 60%, 0.6)',
                    'hsla(180, 70%, 60%, 0.6)',
                    'hsla(300, 70%, 60%, 0.6)',
                  ],
                  borderWidth: 1,
                }
              ]

            }

            return(
              <Col md={3} sm={4} xs={6} key={tf.name}>
                <Stack gap={2}>
                  <span>{tf.name}</span>
                  <div style={{width:"100%"}}>
                    <PolarArea 
                      data={data} 
                      options={{
                        plugins:{
                          legend:{
                            display: false
                          }
                        }
                      }}
                    />
                  </div>
                </Stack>
              </Col>
            );
          })
        }
      </Row>
    </>
  )
}


