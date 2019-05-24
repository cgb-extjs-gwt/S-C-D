import { MultiSelectField } from "./MultiSelectField";
import { fillWgSogInfo } from "./MultiSelectWg";

export class WgMultiSelectField extends MultiSelectField  {

    public componentDidMount() {
        super.componentDidMount();
        this.lst.setItemTpl(fillWgSogInfo);
    }

    protected onSearch(view: any, newValue: string, oldValue: string) {
        //if (newValue) {
        //    this.filterBy(newValue);
        //}
        //else {
        //    this.filter(this.nameField, newValue);
        //}

        this.filter2(this.nameField, newValue);
    }

    public filter2(key: string, val: string) {

        let cfg: any = {
            property: key
        };

        if (val) {
            cfg.anyMatch = true;
            cfg.value = val;
        }
        else {
            cfg.value = '';
        }

        this.lst.getStore().filter(cfg);
    }

    private filterBy(query: string) {
        this.lst.getStore().filter({
            filterFn: function (record) {

                if (!query) {
                    return true;
                }

                record = record.data;

                //if (query.length < 4) {
                //    let regex = new RegExp('^' + query, 'i');
                //    return regex.test(record.name) ? true : record.sog ? regex.test(record.sog.name) : false;
                //}

                let regex = new RegExp(query, 'i');

                return regex.test(record.name);// || regex.test(record.description);
            }
        });
    }

}