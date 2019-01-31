import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class RoleField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getRoles();
    }
}