import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class StandardWgField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getStandardWg();
    }
}