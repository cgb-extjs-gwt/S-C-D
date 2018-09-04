import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class AvailabilityService extends CacheDomainService<NamedId> {
    constructor() {
        super('availability');
    }
}