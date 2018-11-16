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
                result[i] = <NamedId>{ id: key, name: key };
            }
        }
        return result;
    }
}