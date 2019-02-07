import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class PlaField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getPla();
    }
}