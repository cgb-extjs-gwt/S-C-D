import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class SogField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getSog();
    }
}