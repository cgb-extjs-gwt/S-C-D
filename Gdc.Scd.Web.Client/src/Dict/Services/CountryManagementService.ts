import { get, post } from "../../Common/Services/Ajax";
import * as CountryManagementState from "../States/CountryStates";

const CONTROLLER_NAME = 'CountryManagement'

let countries: CountryManagementState.CountryManagementState[] = null;

export const getCountrySettings = () => {
    return get<CountryManagementState.CountryManagementState[]>(CONTROLLER_NAME, 'GetAll');
}

export const getCountries = () => {
    if (countries)
        return Promise.resolve(countries);

    return getCountrySettings().then(
        data => {
            countries = data;
            return countries;
        });
}

export const saveCountries = (postCountries: CountryManagementState.CountryManagementState[]) => {
    if (postCountries && postCountries.length > 0) {
        return post<CountryManagementState.CountryManagementState[]>(CONTROLLER_NAME, 'SaveAll', postCountries);
    }
}
