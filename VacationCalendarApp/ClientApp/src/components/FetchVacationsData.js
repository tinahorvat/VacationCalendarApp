import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService';
import {  NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

export class FetchVacationsData extends Component {
    static displayName = FetchVacationsData.name;

    constructor(props) {
        super(props);
        this.state = { vacations: [], loading: true, };
    }

    componentDidMount() {
        this.populateVacationsData();
    }

    static renderVacationsTable(vacations) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Employee</th>
                        <th>Vacation id</th>
                        <th>Date start</th>
                        <th>Date end</th>
                        <th>Type</th>
                    </tr>
                </thead>
                <tbody>
                    {vacations.map(vacation =>
                        <tr key={vacation.id}>
                            <td>{vacation.employeeFullName}</td>
                            <td>
                                <NavLink tag={Link} className="text-dark" to={`edit-vacation-data/${vacation.id}`}>{vacation.id}</NavLink></td>
                            <td>{vacation.dateFrom}</td>
                            <td>{vacation.dateTo}</td>                            
                            <td>{vacation.vacationType}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : FetchVacationsData.renderVacationsTable(this.state.vacations);

        return (
            <div>
                <h1 id="tabelLabel" >Vacations for month: </h1>
                
                {contents}
            </div>
        );
    }

    async populateVacationsData() {
        const token = await authService.getAccessToken();
        const response = await fetch('api/vacations', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        const data = await response.json();
        this.setState({ vacations: data, loading: false });
    }
}
