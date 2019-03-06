import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class SwDigitSogField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getSwDigitSog();
    }
}