import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class YearService extends CacheDomainService<NamedId> {
    constructor() {
        super('year');
    }
}
