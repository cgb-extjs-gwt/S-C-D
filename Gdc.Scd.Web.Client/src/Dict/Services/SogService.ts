import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class SogService extends CacheDomainService<NamedId> {
    constructor() {
        super('sog');
    }
}
