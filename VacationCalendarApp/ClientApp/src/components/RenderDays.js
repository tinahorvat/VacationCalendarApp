import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService';
import AccessAllowed from "./_helpers/AccessAllowed";
import { NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

export class RenderDays extends React.Component {
    constructor(props) {
        super(props);
        this.state = {     
            //month: new Date(this.props.date.getFullYear(), this.props.date.getMonth(), 0),
            user: { role: null, id: null },            
            employees: [], loading: true
        };

        this.handleChange = this.handleChange.bind(this);
    }

    GetCurrentMonth(date)
    {
        let d = new Date(date.getFullYear(), date.getMonth(), 0)
        return d;
    }

    GetDaysOfTheMonth(date) {
        let d = new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();
        return d;
    }
    componentDidMount() {
        this.populateEmployeesData();
    }

    handleChange(event) {
        this.props.onOptionChange(event);
    }

    renderTableHeader() {       
        const n = this.GetDaysOfTheMonth(this.props.date);
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

    static compareDates(date1, date2) //date format ignore Time
    {
        var zeroTimeDate1 = new Date(date1.getFullYear(), date1.getMonth(), date1.getDate());
        var zeroTimeDate2 = new Date(date2.getFullYear(), date2.getMonth(), date2.getDate());
        if (zeroTimeDate1 < zeroTimeDate2) return -1;
        if (zeroTimeDate1 == zeroTimeDate2) return 0;
        if (zeroTimeDate1 > zeroTimeDate2) return 1;
    }

    renderTableRow(employee) {
        const n = this.GetDaysOfTheMonth(this.props.date) //number of days in month (day value of date must be zero)
        const currentChosenDate = this.props.date;

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
                        <NavLink tag={Link} className="text-dark" to={`create-vacation-data/${employee.employeeId}`}>Add</NavLink>
                    </div>
                )}
            />
        </td>);

        let index = 1;
        
        //be sure to sort vacations by dateFrom, overlaping of vacations is not allowed by model design
        employee.vacations.sort((a, b) => RenderDays.compareDates(new Date(a.dateFrom), new Date(b.dateFrom)));

        employee.vacations.forEach((item) => { //project to new list where only this months vacations, with a list of vacation dates for each object
            let startDate = new Date(item.dateFrom); //mind the time for comparison, must be ignored (compare function handles that)
            let endDate = new Date(item.dateTo);
            let currentRange;
            
            if (!(RenderDays.compareDates(lastDay, startDate) < 0) && !(RenderDays.compareDates(firstDay, endDate) > 0)) { //vacation must have days in current month
                if (RenderDays.compareDates(firstDay, startDate) > 0) {
                    if (RenderDays.compareDates(lastDay, endDate) < 0) {
                        currentRange = this.range(1, n);
                        }
                    else {
                        currentRange = this.range(1, endDate.getDate());
                       }
                }
                else {
                    if (RenderDays.compareDates(lastDay, endDate) < 0) {
                        currentRange = this.range(startDate.getDate(), n);
                        
                    }
                    else {
                        currentRange = this.range(startDate.getDate(), endDate.getDate());
                        
                    }
                }
                let lastElIndex = currentRange[currentRange.length - 1]; //(arr[arr.length - 1])
                for (let i = index; i < currentRange[0]; i++)
                {
                    //fill blanks
                    cells.push(<td ></td>)
                }
                for (let i = currentRange[0]; i <= lastElIndex; i++) {
                    //fill vacations
                    cells.push(<td style={{ background: "red" }}>
                        <AccessAllowed
                            role={this.state.user.role}
                            perform="vacations:edit"
                            data={{
                                userId: this.state.user.id,
                                vacationOwnerId: employee.userName
                            }}
                            yes={() => (
                                <div>
                                    <Link tag={Link} className="text-dark" to={`edit-vacation-data/${item.id}`}>{item.vacationType[0]}</Link>
                                </div>
                            )}
                        />
                    </td>)
                }
                index = lastElIndex+1;
            }            
        });        
        //from index to N - if no vacations, and for the rest of the month after last vacation date
        for (let i = index; i <= n; i++) {
            //fill blanks
            cells.push(<td ></td>)
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
            <table className='table table-striped'>
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
        if (user != null) {
            const role = user.role;
            const userId = user.name;
            if (role == null) { this.setState({ user: { id: null, role: 'Anonymous' } }) }
            else {
                if (role.includes("Admin", 0)) {
                    this.setState({ user: { id: userId, role: 'Admin' } })
                }
                else { this.setState({ user: { id: userId, role: 'Employee' } }) }
            }
        }
        else { this.setState({ user: { id: null, role: 'Anonymous' } }) }
        
        const response = await fetch('api/vacations/GetEmployeesVacations', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        const data = await response.json();
        this.setState({ employees: data, loading: false });
    }
}
