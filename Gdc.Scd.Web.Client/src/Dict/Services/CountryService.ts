import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryService extends CacheDomainService<NamedId> {

    private static isolist: Promise<string[]>

    constructor() {
        super('country');
    }

    public iso(): Promise<string[]> {
        return this.getFromUrl<string>('iso');
    }
}