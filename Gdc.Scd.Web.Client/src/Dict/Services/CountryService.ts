import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { DomainService } from "../../Common/Services/DomainService";
import { Country } from "../Model/Country";

export class CountryService extends CacheDomainService<Country> {
    constructor() {
        super('country');
    }

    public loadAll(): Promise<Country[]> {
        return new DomainService<Country>(this.controllerName).getAll();
    }

    public iso(): Promise<string[]> {
        return this.getFromUrlAll<string>('iso');
    }
}