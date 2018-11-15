import { DictField } from "./DictField";

export class RoleField extends DictField {
    public getItems() {
        return this.srv.getRoles();
    }
}