import { get } from "../../Common/Services/Ajax";
import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { Country } from "../Model/Country";
import { NamedId } from "../../Common/States/CommonStates";

const USR_ACTION: string = 'usr';
const ISCOUNTRYUSER_ACTION: string = 'iscountryuser';
const ISADMINUSER_ACTION: string = 'isadminuser';


export class UserCountryService extends CacheDomainService<Country> {
    constructor() {
        super('country');
    }

    public getAll(): Promise<Country[]> {
        return this.getFromUrl(USR_ACTION);
    }

    public loadAll(): Promise<Country[]> {
        return get<Country[]>(this.controllerName, USR_ACTION);
    }

    public getAllNames(): Promise<NamedId[]> {
        return this.getFromUrl(USR_ACTION);
    }

    public isCountryUser(cntId = 0): Promise<boolean> {
        return get<boolean>(this.controllerName, ISCOUNTRYUSER_ACTION, { cntId: cntId });
    }

    public isAdminUser(): Promise<boolean> {
        return get<boolean>(this.controllerName, ISADMINUSER_ACTION);
    }

    private distinct(data: Country[], prop: string): string[] {
        let result = [];
        if (data) {

            let obj = {};

            for (let i = 0, len = data.length; i < len; i++) {
                let key = data[i][prop];
                if (key) {
                    obj[key] = '';
                }
            }

            let uniqueKeys = Object.keys(obj).sort();

            for (let i = 0, len = uniqueKeys.length; i < len; i++) {
                let key = uniqueKeys[i];
                result[i] = <Country>{ id: key, name: key };
            }
        }
        return result;
    }


}