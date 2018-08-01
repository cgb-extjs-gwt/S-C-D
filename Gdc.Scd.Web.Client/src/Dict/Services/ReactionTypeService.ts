import { DomainService } from "../../Common/Services/DomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class ReactionTypeService extends DomainService<NamedId> {
    constructor() {
        super('reactiontype');
    }
}