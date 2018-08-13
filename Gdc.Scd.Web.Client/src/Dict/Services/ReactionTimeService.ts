import { DomainService } from "../../Common/Services/DomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class ReactionTimeService extends DomainService<NamedId> {
    constructor() {
        super('reactiontime');
    }
}