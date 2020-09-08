import React, { Component } from 'react';

export class VacationType extends React.Component {
    constructor(props) {
        super(props);
        this.state = {

        };

        this.handleChange = this.handleChange.bind(this);        
    }

    handleChange(event) {
        this.props.onOptionChange(event);
    }

    render() {
        return (  
                <label> Pick vacation type: 
                <select name={this.props.name} value={this.props.selected} onChange={this.handleChange}> 
                    {
                        this.props.vacationTypeChoices.map((v) =>
                            <option key={v.value} value={v.value} >{v.text}</option>)
                    }
                </select>
                </label>                
            
        );
    }
}

