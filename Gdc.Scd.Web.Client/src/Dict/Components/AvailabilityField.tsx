import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class AvailabilityField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getAvailabilityTypes();
    }
}