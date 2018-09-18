import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class ReactionTypeService extends CacheDomainService<NamedId> {
    constructor() {
        super('reactiontype');
    }
}