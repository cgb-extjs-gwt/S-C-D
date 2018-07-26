import { get, post } from "../../Common/Services/Ajax";
import { CountryManagementState } from "../States/CountryStates";

const CONTROLLER_NAME = 'CountryController'

let countries: CountryManagementState[] = null;

export function getCountries(): Promise<CountryManagementState[]> {
    return getCountrySettings();
}

export function getCountrySettings(): Promise<CountryManagementState[]> {

    if (countries)
        return Promise.resolve(countries);

    return get<CountryManagementState[]>(CONTROLLER_NAME, 'GetAll').then(
        data => {
            countries = data;
            return countries;
        });
}

