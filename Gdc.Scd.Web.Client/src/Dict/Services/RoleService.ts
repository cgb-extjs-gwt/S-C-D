import { CacheDomainService } from "../../Common/Services/CacheDomainService";
import { NamedId } from "../../Common/States/CommonStates";

export class RoleService extends CacheDomainService<NamedId> {
    constructor() {
        super('role');
    }
}