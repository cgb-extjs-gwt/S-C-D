import { CheckBoxField, Container, List, SearchField } from "@extjs/ext-react";
import * as React from "react";
import { PlaField } from "../../Dict/Components/PlaField";
import { MultiSelect, MultiSelectProps } from "./MultiSelect";

export class MultiSelectWg extends MultiSelect {

    public constructor(props: MultiSelectProps) {
        super(props);
        this.init();
    }

    public render() {

        let { width, height, maxHeight, title, itemTpl, store, selectable } = this.props;

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
                            itemTpl={itemTpl}
                            store={store}
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
        this.filter('plaId', newValue);
    }

    private onSearch(view: any, newValue: string, oldValue: string) {
        this.filter('name', newValue);
    }


    private filter(key: string, val: string) {
        let lst = this.lst as any;
        lst.getStore().filter(key, val);
    }
}