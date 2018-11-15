import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryGroupService extends CacheDomainService<NamedId> {
    constructor() {
        super('countrygroup');
    }

    public getDigit(): Promise<NamedId[]> {
        return this.getAll().then(x => this.distinct(x, 'countryDigit'));
    }

    public getLut(): Promise<NamedId[]> {
        return this.getAll().then(x => this.distinct(x, 'lutCode'));
    }

    private distinct(data: NamedId[], prop: string): NamedId[] {
        throw new Error('not implemented');
    }
}