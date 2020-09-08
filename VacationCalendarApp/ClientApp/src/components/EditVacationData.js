import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'
import { VacationType } from './small/VacationType'
import AccessAllowed from "./_helpers/AccessAllowed";

import DatePicker from "react-datepicker";

import "react-datepicker/dist/react-datepicker.css";

export class EditVacationData extends Component {
    static displayName = EditVacationData.name;

    constructor(props) {
        super(props);
        this.state = {
            vacationId: this.props.match.params.id,
            vacation:
            {
               
            }, loading: true,
            errorMessage: null,
            user: { role: null, id: null },  
            parsedFrom: null,
            parsedTo: null
        };
        this.handleInputChange = this.handleInputChange.bind(this);   
        this.handleDateFromChange = this.handleDateFromChange.bind(this);
        this.handleDateToChange = this.handleDateToChange.bind(this);
        this.HandleDeleteAction = this.HandleDeleteAction.bind(this);   
        this.handleFormSubmit = this.handleFormSubmit.bind(this);
    }
    
    handleInputChange(e) {
        this.setState({
            ...this.state,
            vacation: { ...this.state.vacation, [e.target.name]: e.target.value },
        })
    };

    handleDateFromChange = dateFrom => {
        this.setState({
            ...this.state,
            parsedFrom : dateFrom
        })
    };
    handleDateToChange = date => {
        this.setState({
            ...this.state,
            parsedTo: date
        })
    };

    async handleFormSubmit(e) {
        e.preventDefault();
        const token = await authService.getAccessToken();
        const stringDateFrom = this.state.parsedFrom.toISOString();
        const stringDateTo = this.state.parsedTo.toISOString();
        this.setState(prevState => {
            return { ...prevState, vacation: { ...prevState.vacation, dateFrom: stringDateFrom } }
        })
        this.setState(prevState => {
            return { ...prevState, vacation: { ...prevState.vacation, dateTo: stringDateTo } }
        })
        
        await fetch("/api/vacations/" + this.state.vacationId, {
            method: "PUT",
            body: JSON.stringify(this.state.vacation),
            headers: new Headers({
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            })
        })            
            .then((response) => {
                if (response.ok) {
                    this.props.history.push("/fetch-data"); 
                }                
                if (response.status === 403 || response.status === 400) { alert("Validation" + response.details) }
                return response;
            })
            .catch(error => alert("Something went wrong"))
    }  

    async HandleDeleteAction(e)
    {     
        e.preventDefault();
        const token = await authService.getAccessToken();
        await fetch("/api/vacations/" + this.state.vacationId, {
            method: "DELETE",            
            headers: new Headers({
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            })
        })
            .then((response) => {
                if (response.ok) {
                    this.props.history.push("/fetch-data"); //TODO: navigate to calendar
                }
                if (response.status === 403 || response.status === 400) { alert(response.statusText) }
                return response;
            })
            .catch(error => alert("Something went wrong"))
    }

    componentDidMount() {
        this.populateVacationData();
        
    }

    renderVacationForm(vacation, parsedFrom, parsedTo) {
        return (
            <div>
                <form onSubmit={(e) => this.handleFormSubmit(e)}>
                    <div className="form-group">                    
                        <label  htmlFor="employee">Employee</label>
                        <input  type="text" name="employee" defaultValue={vacation.employeeFullName} />
                    </div>
                    <div className="form-group">
                        <label htmlFor="dateFrom">Date from</label>                        
                        <DatePicker  name="dateFrom" dateFormat="yyyy/MM/dd" selected={parsedFrom} onChange={this.handleDateFromChange} />
                    </div>
                    <div className="form-group">
                        <label  htmlFor="dateTo">Date to</label>
                        <DatePicker dateFormat="yyyy/MM/dd" selected={parsedTo} onChange={this.handleDateToChange} />
                    </div>                    
                    <div className="form-group">
                        <VacationType name="vacationType" selected={vacation.vacationType} vacationTypeChoices={vacation.vacationTypeChoices} onOptionChange={this.handleInputChange} />
                    </div>
                    <AccessAllowed
                        role={this.state.user.role}
                        perform="vacations:edit"
                        data={{
                            userId: this.state.user.id,
                            vacationOwnerId: this.state.vacation.userName
                        }}
                        yes={() => (
                            <div className="form-group">
                                <input type="submit" value="Submit vacation" />
                            </div> 
                        )}
                    /> 
                                       
                </form>
                <div className="form-group">
                    <AccessAllowed
                        role={this.state.user.role}
                        perform="vacations:delete"
                        data={{
                            userId: this.state.user.id,
                            vacationOwnerId: this.state.vacation.userName
                        }}
                        yes={() => (
                            <div>
                                <button type="button" onClick={this.HandleDeleteAction}>
                                    Delete
                            </button>
                            </div>
                        )}
                    />                
                </div>
                
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderVacationForm(this.state.vacation, this.state.parsedFrom, this.state.parsedTo);

        return (
            <div>
                <h1 id="tabelLabel" >Edit vacation : </h1>
                {contents}
            </div>
        );
    }


    async populateVacationData() {
        const token = await authService.getAccessToken();  
        const user = await authService.getUser();
        const role = user.role;
        const userId = user.name;
        if (role == null) { this.setState({ user: { id: null, role: 'Anonymous' } }) }
        else {
            if (role.includes("Admin", 0)) {
                this.setState({ user: { id: userId, role: 'Admin' } })
            }
            else { this.setState({ user: { id: userId, role: 'Employee' } }) }
        }
        await fetch('api/vacations/' + this.state.vacationId, {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        })
            .then(response => {
            if (response.ok) {
                return response.json()
            }
            if (response.status === 403) {
                this.setState({ errorMessage: "You are not authorized!", loading: false })
            }
            })
            .then(data => this.setState({ vacation: data || [], loading: false }))
            .catch(error => {
                this.setState({ errorMessage: error, loading: false })

                }
            );   

        const parsedDateFrom = new Date(this.state.vacation.dateFrom);
        const parsedDateTo = new Date(this.state.vacation.dateTo);
        this.setState({ ...this.state, parsedFrom: parsedDateFrom });
        this.setState({ ...this.state, parsedTo: parsedDateTo })                
    }


}
