import { NamedId } from "../../Common/States/CommonStates";
import { DictField } from "./DictField";

export class SwDigitField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getSwDigit();
    }

    public componentDidUpdate() {
        this.filter('sogId', this.props.sog);
    }
}