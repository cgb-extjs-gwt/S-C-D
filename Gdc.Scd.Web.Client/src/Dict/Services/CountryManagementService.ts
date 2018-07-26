import {get, post} from "../../Common/Services/Ajax";
import * as CountryManagementState from "../States/CountryStates";

const CONTROLLER_NAME = 'CountryManagement'

let countries = null;

export const getCountrySettings = () => {
        
    if (countries)
            return Promise.resolve(countries);
        
    return get<CountryManagementState.CountryManagementState[]>(CONTROLLER_NAME, 'GetAll').then(
            data => {
                countries = data;
                return countries;
            });
}