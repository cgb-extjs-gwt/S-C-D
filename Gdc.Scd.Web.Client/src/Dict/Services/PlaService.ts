import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class PlaService extends CacheDomainService<NamedId> {
    constructor() {
        super('pla');
    }
}
