import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class DurationField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getDurationTypes();
    }
}