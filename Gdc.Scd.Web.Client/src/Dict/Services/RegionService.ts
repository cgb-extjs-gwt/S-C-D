import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class RegionService extends CacheDomainService<NamedId> {
    constructor() {
        super('region');
    }
}