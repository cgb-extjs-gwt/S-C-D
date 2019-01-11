import { get } from "../../Common/Services/Ajax";
import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { Country } from "../Model/Country";

const USR_ACTION: string = 'usr';
const ISCOUNTRYUSER_ACTION: string = 'iscountryuser';

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

    public isCountryUser(): Promise<boolean> {
        return get<boolean>(this.controllerName, ISCOUNTRYUSER_ACTION);
    }
}