import { DomainService } from "../../Common/Services/DomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class DurationService extends DomainService<NamedId> {
    constructor() {
        super('duration');
    }
}