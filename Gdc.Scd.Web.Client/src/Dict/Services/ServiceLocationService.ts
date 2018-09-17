import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class ServiceLocationService extends CacheDomainService<NamedId> {
    constructor() {
        super('servicelocation');
    }
}