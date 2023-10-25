import { Tooltip, Typography } from 'antd';
import moment from 'moment';
import React from 'react';
import cssVariables from '_assets/styles/cssVariables';
import { Comment } from '_utils/Types';

export default function Sandbox() {
  const comments : Comment[] = [
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
  ];

  return (
    <div style={{backgroundColor:'wheat'}}>
      <div style={{backgroundColor: cssVariables.bgL1, width:'400px', height:'600px', overflow:'auto'}}>
        {comments.map(a => (
          <div className='comment-box'>
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
    </div>
  )
}
