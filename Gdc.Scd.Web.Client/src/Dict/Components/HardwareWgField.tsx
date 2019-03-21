import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class HardwareWgField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getHardwareWg();
    }
}