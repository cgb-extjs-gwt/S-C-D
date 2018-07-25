import * as React from 'react';
import * as CountryManagementService from '../../../Dict/Services/CountryManagementService';
import * as CountryManagementState from '../../../Dict/States/CountryStates';

class CountryManagement extends React.Component<any>{
    state: {
        countries : []
    }

    componentDidMount(){
        let data = CountryManagementService.getCountrySettings().then(countryData => 
        {
            this.setState({ countries: countryData})
        });
    }

    render(){
        return (
            <p>Hello from country component...</p>
        );
    }
    
}

export default CountryManagement;