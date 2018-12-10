import { get, post } from "../../Common/Services/Ajax";
import * as CountryManagementState from "../States/CountryStates";
import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

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

export class CountryManagementService extends CacheDomainService<NamedId> {
    constructor() {
        super('countrymanagement');
    }

    public getIsoCode(): Promise<NamedId[]> {
        return this.getAll().then(x => this.distinct(x, 'isO3Code'));
    }

    private distinct(data: any, prop: string): NamedId[] {
        let result = [];
        if (data) {

            let obj = {};

            for (let i = 0, len = data.items.length; i < len; i++) {
                let key = data.items[i][prop];
                if (key) {
                    obj[key] = '';
                }
            }

            let uniqueKeys = Object.keys(obj).sort();

            for (let i = 0, len = uniqueKeys.length; i < len; i++) {
                let key = uniqueKeys[i];
                result[i] = <NamedId>{ id: key, name: key };
            }
        }
        return result;
    }
}

