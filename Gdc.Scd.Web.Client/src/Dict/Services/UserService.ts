import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class UserService extends CacheDomainService<NamedId> {
    constructor() {
        super('user');
    }
}