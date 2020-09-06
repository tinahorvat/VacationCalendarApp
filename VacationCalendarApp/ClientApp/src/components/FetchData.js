import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'

import { RenderDays } from './RenderDays'

//import Timeline from 'react-calendar-timeline'
//import moment from 'moment'

//import FullCalendar from '@fullcalendar/react'
//import dayGridPlugin from '@fullcalendar/daygrid'
//import listPlugin from '@fullcalendar/list'


export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);     
  }

  componentDidMount() {
  }

  static renderForecastsTable(forecasts) {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Date</th>
            <th>Temp. (C)</th>
            <th>Temp. (F)</th>
            <th>Summary</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map(forecast =>
            <tr key={forecast.date}>
              <td>{forecast.date}</td>
              <td>{forecast.temperatureC}</td>
              <td>{forecast.temperatureF}</td>
              <td>{forecast.summary}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render() {
    //let contents = this.state.loading
    //  ? <p><em>Loading...</em></p>
    //  : FetchData.renderForecastsTable(this.state.forecasts);
      
    return (
      <div>
            <h1 id="tabelLabel" >Vacations</h1>
            <RenderDays />
      </div>
    );
  }


}
