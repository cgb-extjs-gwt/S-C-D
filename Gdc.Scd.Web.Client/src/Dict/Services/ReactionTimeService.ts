import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class ReactionTimeService extends CacheDomainService<NamedId> {
    constructor() {
        super('reactiontime');
    }
}