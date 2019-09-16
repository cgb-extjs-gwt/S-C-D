import { NamedId } from "../../Common/States/CommonStates";
import { DictField } from "./DictField";
import { fillWgSogInfo } from "./MultiSelectWg";

export interface WgFieldProps {
    wgStore(): Promise<NamedId[]>;
}

export class WgField extends DictField<WgFieldProps> {

    public getItems() {
        return this.props.wgStore().then((x: any[]) => {

            for (let i = 0; i < x.length; i++) {
                let item = x[i];

                item.__fullname = item.name + ' ' + item.description;

                if (item.sog) {
                    item.__fullname = ' ' + item.sog.name + ' ' + item.sog.description;
                }
            }

            return x;
        });
    }

    public componentDidMount() {
        super.componentDidMount();

        let combo = this.combo;

        combo.setForceSelection(false);
        combo.setItemTpl(fillWgSogInfo);
        combo.setPrimaryFilter({
            filterFn: function (record) {

                let query = combo.getValue();

                if (!query) {
                    return true;
                }

                record = record.data;

                if (query.length < 4) {
                    let regex = new RegExp('^' + query, 'i');
                    return regex.test(record.name) ? true : record.sog ? regex.test(record.sog.name) : false;
                }

                let regex = new RegExp(query, 'i');
                return regex.test(record.__fullname);
            }
        });
    }

    public componentDidUpdate() {
        this.filter('sogId', this.props.sog);
    }
}
