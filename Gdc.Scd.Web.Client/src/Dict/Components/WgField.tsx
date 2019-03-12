import { NamedId } from "../../Common/States/CommonStates";
import { DictField } from "./DictField";

export class WgField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getWG();
    }

    public componentDidUpdate() {
        this.filter('sogId', this.props.sog);
    }
}