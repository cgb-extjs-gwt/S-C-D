import { CheckBoxField, Container, List, SearchField } from "@extjs/ext-react";
import * as React from "react";
import { PlaField } from "../../Dict/Components/PlaField";
import { MultiSelect } from "./MultiSelect";

export function fillWgSogInfo(wg) {
    if (wg.sog) {
        return <div><strong>{wg.name}</strong> | <strong>{wg.sog.name}</strong><br />({wg.description}/{wg.sog.description})</div>;
    }
    else {
        return <div><strong>{wg.name}</strong>&nbsp;({wg.description})</div>;
    }
};

export class MultiSelectWg extends MultiSelect {

    protected plaField: string;

    public render() {

        let { width, height, maxHeight, title, selectable } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';

        height = height || '100%';

        selectable = selectable || 'multi';

        return (
            <Container width={width}>
                <CheckBoxField
                    ref={x => this.cb = x}
                    boxLabel={title}
                    padding="2px"
                    bodyAlign="left"
                    onChange={this.onTopSelectionChange}
                />
                <PlaField placeholder="PLA" onChange={this.onPlaChange} />
                <SearchField placeholder="Search by wg/sog..." onChange={this.onSearch} />
                <div onClick={this.onListClick}>
                    <Container>
                        <List
                            ref={x => this.lst = x}
                            itemTpl={fillWgSogInfo}
                            store={this.state.items}
                            height={height}
                            maxHeight={maxHeight}
                            selectable={selectable}
                            scrollable="true"
                        />
                    </Container>
                </div>
            </Container>
        );
    }

    protected init() {
        super.init();
        //
        this.onPlaChange = this.onPlaChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
    }

    private onPlaChange(view: any, newValue: string, oldValue: string) {
        this.plaField = newValue;
        this.filter('plaId', newValue);
    }

    private onSearch(view: any, newValue: string, oldValue: string) {
        this.lst.getStore().clearFilter(true);
        this.lst.getStore().filterBy(record => this.filterByWgOrSog(record, newValue));
    }

    private filter(key: string, val: string) {
        val = val || '';
        this.lst.getStore().filter(key, val);
    }

    private filterByWgOrSog(record: any, newValue: string) {
        newValue = newValue || '';
        let pla = this.plaField || '';
        if ((pla === '' || pla === record.data.plaId) &&
            (record.data.name.toLowerCase().startsWith(newValue.toLowerCase())
                || (record.data.sog !== undefined && record.data.sog.name.toLowerCase().startsWith(newValue.toLowerCase()))))
            return true;
        return false;
    }
}