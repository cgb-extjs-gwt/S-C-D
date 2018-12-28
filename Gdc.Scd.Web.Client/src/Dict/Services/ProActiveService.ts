import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class ProActiveService extends CacheDomainService<NamedId> {
    constructor() {
        super('proactive');
    }
}
