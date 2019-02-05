import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class YearField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getYears();
    }
}