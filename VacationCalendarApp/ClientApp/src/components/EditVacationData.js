import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'

export class EditVacationData extends Component {
    static displayName = EditVacationData.name;

    constructor(props) {
        super(props);
        this.state = {
            vacationId: this.props.match.params.id,            
            vacation:
            {
                
            }, loading: true,
        };
        this.handleDateFromChange = this.handleDateFromChange.bind(this);
        this.handleDateToChange = this.handleDateToChange.bind(this);
        this.handleVacationTypeChange = this.handleVacationTypeChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleDateFromChange(e) {
        this.setState(prevState => ({
            vacation: {                   // object that we want to update
                ...prevState.vacation,    // keep all other key-value pairs
                dateFrom: e.target.value      // update the value of specific key
            }
        }))
        
    }
    handleDateToChange(e) {
        this.setState({ vacation: e.target.value });
    }
    handleVacationTypeChange(e) {
        this.setState({ vacation: e.target.value });
    }

    handleSubmit(event) {
        alert('A name was submitted: ' + this.state.vacation.dateFrom + ' ' + this.state.vacation.dateTo);
        event.preventDefault();
    }

    componentDidMount() {
        this.populateVacationData();
    }

    static renderVacationForm(vacation) {
        return (
            <form className="form-group" >
                <input type="text" className="form-control" name="employee" placeholder="start date"
                    value={vacation.employeeFullName} />
                <input type="text" className="form-control" name="dateFrom" placeholder="start date"
                    value={vacation.dateFrom}  />
                <input
                    type="text" className="form-control" name="dateTo" placeholder="end date"
                    value={vacation.dateTo}  />
                <input
                    type="text" className="form-control" name="vacationType" placeholder="type"
                    value={vacation.vacationType}  />
                <input type="submit" value="Post" />
            </form>
            //<table className='table table-striped' aria-labelledby="tabelLabel">
            //    <thead>
            //        <tr>
            //            <th>Invisible</th>
            //            <th>Name</th>
            //            <th>Date start</th>
            //            <th>Date end</th>
            //            <th>Type</th>
            //        </tr>
            //    </thead>
            //    <tbody>
                    
            //        <tr key={vacation.id}>
            //            <td>{this.state.username}</td>
            //                <td>{vacation.employeeId}</td>
            //                <td>{vacation.dateFrom}</td>
            //                <td>{vacation.dateTo}</td>
            //                <td>{vacation.vacationType}</td>
            //            </tr>
                    
            //    </tbody>
            //</table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : EditVacationData.renderVacationForm(this.state.vacation);

        return (
            <div>
                <h1 id="tabelLabel" >Edit employees vacations : </h1>

                {contents}
            </div>
        );
    }

    async populateVacationData() {
        const token = await authService.getAccessToken();
        const user = await authService.getUser();
        const response = await fetch('api/vacations/' + this.state.vacationId, {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        const data = await response.json();
        this.setState({ vacation: data, loading: false });
    }
}
