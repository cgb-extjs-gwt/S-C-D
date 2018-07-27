import { DomainService } from "../../Common/Services/DomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class WgService extends DomainService<NamedId> {
    constructor() {
        super('wg');
    }
}
