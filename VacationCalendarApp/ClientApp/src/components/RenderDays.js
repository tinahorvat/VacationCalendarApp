import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService';
import AccessAllowed from "./_helpers/AccessAllowed";
import { NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

export class RenderDays extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            user: {role : null, id: null},
            month: new Date(2024, 9, 0),
            employees: [], loading: true
        };

        this.handleChange = this.handleChange.bind(this);
    }

    componentDidMount() {
        this.populateEmployeesData();
    }

    handleChange(event) {
        this.props.onOptionChange(event);
    }

    renderTableHeader() {       
        const n = this.state.month.getDate()
        let cells = [];
        cells.push(<th>Employee</th>);
        for (let i = 1; i <= n; i++) {
            cells.push(<th>{i}</th>);
                       
        }
        let row = <tr>{cells}</tr>
        return row;        
    }

    range(first, last) { //get range of numbers in current month
        
        if (last < first || (first < 1) || (last > 31))
        {
            return;
        }
        if (first === last) return [first];
        return [first, ...this.range(first + 1, last)];
    }

    compareDates(date1, date2) //date format
    {
        var zeroTimeDate1 = new Date(date1.getFullYear(), date1.getMonth(), date1.getDate());
        var zeroTimeDate2 = new Date(date2.getFullYear(), date2.getMonth(), date2.getDate());
        if (zeroTimeDate1 < zeroTimeDate2) return -1;
        if (zeroTimeDate1 == zeroTimeDate2) return 0;
        if (zeroTimeDate1 > zeroTimeDate2) return 1;
    }

    renderTableRow(employee) {
        const n = this.state.month.getDate() //number of days in month (day value of date must be zero)
        const currentChosenDate = this.state.month;

        const firstDay = new Date(currentChosenDate.getFullYear(), currentChosenDate.getMonth(), 1);
        const lastDay = new Date(currentChosenDate.getFullYear(), currentChosenDate.getMonth(), n);

        let cells = [];
        cells.push(<td>{employee.employeeFullName}
            <AccessAllowed
                role={this.state.user.role}
                perform="vacations:create"
                data={{
                    userId: this.state.user.id,
                    vacationOwnerId: employee.userName
                }}
                yes={() => (
                    <div>
                        <NavLink tag={Link} className="text-dark" to={`create-vacation-data/${employee.employeeId}`}>Add new</NavLink>
                    </div>
                )}
            />
        </td>);        

        let vacationDaysInMonth = [];
        employee.vacations.forEach((item) =>
        {            
            let startDate = new Date(item.dateFrom); //mind the time for comparison
            let endDate = new Date(item.dateTo);
            if (this.compareDates(lastDay, startDate) <0) return;
            if (this.compareDates(firstDay, endDate) > 0) return;

            if (this.compareDates(firstDay, startDate) > 0) {
                if (this.compareDates(lastDay, endDate) < 0) {
                    vacationDaysInMonth.push(this.range(1, n));
                }
                else {
                    vacationDaysInMonth.push(this.range(1, endDate.getDate()));
                }
            }
            else {
                if (this.compareDates(lastDay, endDate) < 0) {
                    vacationDaysInMonth.push(this.range(startDate.getDate(), n));
                }
                else {
                    vacationDaysInMonth.push(this.range(startDate.getDate(), endDate.getDate()));
                }
            }
            
        });       

        vacationDaysInMonth = vacationDaysInMonth.flat();
        for (let i = 1; i <= n; i++) {
            (vacationDaysInMonth.some((e) => (e == i)))
                ? cells.push(<td>V</td>)
                : cells.push(<td></td>)
        }
        let row = <tr key={employee.EmployeeId}>{cells}</tr>
        return row;
    }

    renderTableData(employees)
    {
        let tableData= []; //rows for employees
        employees.map(e => tableData.push(this.renderTableRow(e)))
        return tableData;
    }

    renderTable()
    {
        return (
            <table>
                <thead>
                    {this.renderTableHeader()}
                </thead>
                <tbody>
                    {this.renderTableData(this.state.employees)}
                </tbody>
            </table>
            )
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderTable();
        return (  
            <div>
            {contents} 
            </div>
        );
    }

    async populateEmployeesData() {
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
        const response = await fetch('api/vacations/GetEmployeesVacations', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        const data = await response.json();
        this.setState({ employees: data, loading: false });
    }
}
