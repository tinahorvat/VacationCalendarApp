﻿import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'
import { VacationType } from './small/VacationType'

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
            userRole : null
        };
        this.handleInputChange = this.handleInputChange.bind(this);        
        this.handleFormSubmit = this.handleFormSubmit.bind(this);
    }

    handleInputChange(e) {
        this.setState({
            ...this.state,
            vacation: { ...this.state.vacation, [e.target.name]: e.target.value },
        })
    };

    async handleFormSubmit(e) {
        e.preventDefault();
        const token = await authService.getAccessToken();

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
                    this.props.history.push("/"); //TODO: navigate to calendar
                }                
                if (response.status === 403 || response.status === 400) { alert(response.statusText) }
                return response;
            })
            .catch(error => alert("Something went wrong"))
    }      

    componentDidMount() {
        this.populateVacationData();
    }

    renderVacationForm(vacation) {
        return (
            <div className="formContainer">
                <form onSubmit={(e) => this.handleFormSubmit(e)}>
                    <div className="row">
                        <label className="col-50" htmlFor="employee">Employee</label>
                        <input type="text" name="employee" defaultValue={vacation.employeeFullName} />
                    </div>
                    <div className="row">
                        <label className="col-50" htmlFor="dateFrom">Date from</label>
                        <input type="text" name="dateFrom" value={vacation.dateFrom} onChange={(e) => this.handleInputChange(e)} />
                    </div>
                    <div className="row">
                        <label className="col-50" htmlFor="dateTo">Date to</label>
                        <input type="text" name="dateTo" value={vacation.dateTo} onChange={(e) => this.handleInputChange(e)} />
                    </div>                    
                    <div className="row">
                        <VacationType name="vacationType" selected={vacation.vacationType} vacationTypeChoices={vacation.vacationTypeChoices} onOptionChange={this.handleInputChange} />
                    </div>
                    <div className="row">
                        <input type="submit" value="Submit vacation" />
                    </div>
                </form>
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderVacationForm(this.state.vacation);

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
        var role = user.role;
        this.setState({ userRole : role });
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
            .catch(error => this.setState({ errorMessage: error, loading: false }));        
    }


}
