import { DomainService } from "../../Common/Services/DomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class ServiceLocationService extends DomainService<NamedId> {
    constructor() {
        super('servicelocation');
    }
}