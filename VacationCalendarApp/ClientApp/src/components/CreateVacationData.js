import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'

export class CreateVacationData extends Component {
    static displayName = CreateVacationData.name;

    constructor(props) {
        super(props);
        this.state = {
            employeeId: this.props.match.params.id, 
            vacation:
            {
                employeeId: this.props.employeeId,
                employeeName: this.props.employeeFullName,
                dateFrom: null,
                dateTo: null,
                vacationType : null
            },
            errorMessage: null
        };
        this.handleDateFromChange = this.handleDateFromChange.bind(this);
        this.handleDateToChange = this.handleDateToChange.bind(this);
        this.handleVacationTypeChange = this.handleVacationTypeChange.bind(this);
        this.handleFormSubmit = this.handleFormSubmit.bind(this);
    }

    handleDateFromChange(e) {
        this.setState({
            ...this.state,
            vacation: { ...this.state.vacation, dateFrom: e.target.value },
        })
    };

    handleDateToChange(e) {
        this.setState({
            ...this.state,
            vacation: { ...this.state.vacation, dateTo: e.target.value },
        })
    };

    handleVacationTypeChange(e) {
        this.setState({
            ...this.state,
            vacation: { ...this.state.vacation, vacationType: e.target.value },
        })
    };

    async handleFormSubmit(e) {
        e.preventDefault();
        const token = await authService.getAccessToken();

        await fetch("/api/vacations/", {
            method: "POST",
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
                        <input type="text" name="dateFrom" value={vacation.dateFrom} onChange={(e) => this.handleDateFromChange(e)} />
                    </div>
                    <div className="row">
                        <label className="col-50" htmlFor="dateTo">Date to</label>
                        <input type="text" name="dateTo" value={vacation.dateTo} onChange={(e) => this.handleDateToChange(e)} />
                    </div>
                    <div className="row">
                        <label className="col-50" htmlFor="vacationType">Vacation type</label>
                        <input type="text" name="vacationType" value={vacation.vacationType} onChange={(e) => this.handleVacationTypeChange(e)} />
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
                <h1 id="tabelLabel" >Add vacation : </h1>

                {contents}
            </div>
        );
    }

    async populateVacationData() {
        const token = await authService.getAccessToken();
        await fetch('api/vacations/', {
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
