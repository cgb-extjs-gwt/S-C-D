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

    public findByName(name: string): Promise<NamedId> {
        return this.getAll().then(x => this.find(x, name));
    }

    private find(data: NamedId[], name: string): NamedId {
        let search = data.filter(x => x.name === name);
        return search && search.length > 0 ? search[0] : null;
    }
}