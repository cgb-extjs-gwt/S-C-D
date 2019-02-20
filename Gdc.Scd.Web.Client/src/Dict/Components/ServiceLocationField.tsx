import { DictField } from "./DictField";
import { SortableNamedId } from "../../Common/States/CommonStates";

export class ServiceLocationField extends DictField<SortableNamedId> {
    protected orderField: string = 'order';

    public componentDidMount() {
        let store = this.combo.getStore() as any;
        let sorters = store.getSorters();
        sorters.remove(this.orderField);
        sorters.add(this.orderField);

        this.getItems().then(x => store.setData(x));
    }

    public getItems() {
        return this.srv.getServiceLocationTypes();
    }
}