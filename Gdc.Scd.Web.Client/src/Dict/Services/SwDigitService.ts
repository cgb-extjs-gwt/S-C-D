import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class SwDigitService extends CacheDomainService<NamedId> {
    constructor() {
        super('swdigit');
    }

    public sog(): Promise<NamedId[]> {
        return this.getFromUrlAll<NamedId>('sog');
    }
}
