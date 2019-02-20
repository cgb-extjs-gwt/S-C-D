import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class SwDigitField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getSwDigit();
    }
}