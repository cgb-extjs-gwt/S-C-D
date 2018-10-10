import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class DurationService extends CacheDomainService<NamedId> {
    constructor() {
        super('duration');
    }
}