import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService';
import AccessAllowed from "./_helpers/AccessAllowed";
import {  NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

export class FetchVacationsData extends Component {
    static displayName = FetchVacationsData.name;

    constructor(props) {
        super(props);
        this.state = {
            vacations: [], loading: true,
            user: { id : null, role : null }
            
        };
    }

    componentDidMount() {
        this.populateVacationsData();
    }

    static renderVacationsTable(vacations, user) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Employee</th>
                        <th>Vacation</th>
                        <th>Date start</th>
                        <th>Date end</th>
                        <th>Type</th>
                    </tr>
                </thead>
                <tbody>
                    {vacations.map(vacation =>
                        <tr key={vacation.id}>
                            <td>{vacation.employeeFullName}
                                <AccessAllowed
                                    role={user.role}
                                    perform="vacations:create"
                                    yes={() => (
                                        <div>
                                            <NavLink tag={Link} className="text-dark" to={`create-vacation-data/${vacation.employeeId}`}>Create vacation for this employee</NavLink>
                                        </div>
                                    )}
                                    
                                />
                                
                            </td>
                            <td>
                                <AccessAllowed
                                    role={user.role}
                                    perform="vacations:edit"
                                    data={{
                                        userId: user.id,
                                        vacationOwnerId: user.id
                                    }}
                                    yes={() => (
                                        <div>
                                            <NavLink tag={Link} className="text-dark" to={`edit-vacation-data/${vacation.id}`}>Edit/delete this vacation{vacation.id}</NavLink>
                                            </div>
                                    )}
                                />
                                    
                                
                                </td>
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
            : FetchVacationsData.renderVacationsTable(this.state.vacations, this.state.user);

        return (
            <div>
                <h1 id="tabelLabel" >Vacations for month: </h1>
                
                {contents}
            </div>
        );
    }

    async populateVacationsData() {
        const token = await authService.getAccessToken();
        const user = await authService.getUser();
        const role = user.role;
        const userId = user.name;
        if (role == null) { this.setState({ user: { id: null, role: 'Anonymous' } }) }
        else {
            if (role.includes("Admin", 0)) {
                this.setState({ user: { id: userId, role: 'Admin' } })
            }
            else { this.setState({ user: { id: userId, role: 'Employee' }  }) }
        }
        const response = await fetch('api/vacations', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        const data = await response.json();
        this.setState({ vacations: data, loading: false });
    }
}
