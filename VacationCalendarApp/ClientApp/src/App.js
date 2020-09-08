import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { FetchVacationsData } from './components/FetchVacationsData';
import { EditVacationData } from './components/EditVacationData';
import { CreateVacationData } from './components/CreateVacationData';
import AuthorizeRoute from './components/api-authorization/AuthorizeRoute';
import ApiAuthorizationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import { ApplicationPaths } from './components/api-authorization/ApiAuthorizationConstants';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />        
        <Route path='/fetch-data' component={FetchData} />
        <AuthorizeRoute path='/fetch-vacations-data' component={FetchVacationsData} />
        <AuthorizeRoute path='/edit-vacation-data/:id' component={EditVacationData} />
        <AuthorizeRoute path='/create-vacation-data/:id' component={CreateVacationData} />
        <Route path={ApplicationPaths.ApiAuthorizationPrefix} component={ApiAuthorizationRoutes} />
      </Layout>
    );
  }
}
