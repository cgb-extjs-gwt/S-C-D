import { MultiSelectField } from "./MultiSelectField";
import { fillWgSogInfo } from "./MultiSelectWg";

export class WgMultiSelectField extends MultiSelectField {

    public componentDidMount() {
        super.componentDidMount();
        this.lst.setItemTpl(fillWgSogInfo);
    }

    protected onSearch(view: any, newValue: string, oldValue: string) {
        this.filterBy(newValue);
    }

    private filterBy(query: string) {
        let store = this.lst.getStore();
        store.clearFilter(true);

        if (query) {
            store.filterBy(function (record) {

                record = record.data;

                if (query.length < 4) {
                    let regex = new RegExp('^' + query, 'i');
                    return regex.test(record.name) ? true : record.sog ? regex.test(record.sog.name) : false;
                }

                let regex = new RegExp(query, 'i');

                if (regex.test(record.name) || regex.test(record.description)) {
                    return true;
                }

                if (record.sog) {
                    return regex.test(record.sog.name) || regex.test(record.sog.description);
                }

                return false;
            });
        }
    }

}