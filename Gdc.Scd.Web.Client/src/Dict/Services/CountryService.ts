import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { DomainService } from "../../Common/Services/DomainService";
import { Country } from "../Model/Country";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryService extends CacheDomainService<Country> {
    constructor() {
        super('country');
    }

    public loadAll(): Promise<Country[]> {
        return new DomainService<Country>(this.controllerName).getAll();
    }

    public getAllNames(): Promise<NamedId[]> {
        return this.getFromUrl('GetAll');
    }

    public iso(): Promise<string[]> {
        return this.getFromUrlAll<string>('iso');
    }
}