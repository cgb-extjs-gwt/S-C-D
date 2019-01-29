import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class ProActiveField extends DictField<NamedId> {

    protected nameField: string = 'externalName';

    public getItems() {
        return this.srv.getProActive();
    }
}