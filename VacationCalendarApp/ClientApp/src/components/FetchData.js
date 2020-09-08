import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";

import { RenderDays } from './RenderDays'


export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
      super(props);     
      this.state = {
          startDate : new Date(),
      }

      this.setStartDate = this.setStartDate.bind(this);
  }

  componentDidMount() {
  }

    setStartDate = date => {
        this.setState({
            ...this.state,
            startDate: date
        })
    };

  render() {
   
    return (
      <div>
            <h1 id="tabelLabel" >Vacations</h1>
            <label>
                <DatePicker
                    selected={this.state.startDate}
                    onChange={date => this.setStartDate(date)}
                    dateFormat="MM/yyyy"
                    showMonthYearPicker
                    shouldCloseOnSelect={true}
                />
            </label>
            <RenderDays date={this.state.startDate} />
      </div>
    );
  }


}
