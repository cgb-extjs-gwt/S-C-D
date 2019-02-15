import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class CurrencyService extends CacheDomainService<NamedId> {
    constructor() {
        super('currency');
    }
}