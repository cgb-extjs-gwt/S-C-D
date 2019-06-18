import { MultiSelectField } from "./MultiSelectField";
import { fillWgSogInfo } from "./MultiSelectWg";

export class WgMultiSelectField extends MultiSelectField {

    public componentDidMount() {
        super.componentDidMount();
        this.lst.setItemTpl(fillWgSogInfo);
    }

    protected onSearch() {
        this.filterBy(this.txtSearch.getValue());
    }

    private filterBy(query: string) {
        let store = this.lst.getStore();
        store.clearFilter(true);

        store.filterBy(function (record) {

            if (!query) {
                return true;
            }

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