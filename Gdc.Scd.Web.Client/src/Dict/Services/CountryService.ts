import { DomainService } from "../../Common/Services/DomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryService extends DomainService<NamedId> {
    constructor() {
        super('country');
    }
}