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

    protected plaSearch: PlaField;

    protected txtSearch: SearchField & any;

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
                <PlaField ref={x => this.plaSearch = x} placeholder="PLA" onChange={this.filterBy} onClearIconTap={this.filterBy} />
                <SearchField ref={x => this.txtSearch = x} placeholder="Search by wg/sog..." onChange={this.filterBy} onClearIconTap={this.filterBy} />
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
        this.filterBy = this.filterBy.bind(this);
    }

    private filterBy() {
        let pla = this.plaSearch.getValue();
        let query = this.txtSearch.getValue();

        this.lst.getStore().clearFilter(true);
        this.lst.getStore().filterBy(function (record) {

            record = record.data;

            let plaOk = !pla || pla === record.plaId;

            if (!plaOk) {
                return false;
            }

            if (!query) {
                return true;
            }

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