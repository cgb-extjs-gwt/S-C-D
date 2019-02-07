import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId, SortableNamedId } from "../../Common/States/CommonStates";

export class ServiceLocationService extends CacheDomainService<SortableNamedId> {
    constructor() {
        super('servicelocation');
    }
}