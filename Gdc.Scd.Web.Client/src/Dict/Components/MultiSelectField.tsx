import { Container, List, Panel, SearchField, CheckBoxField } from "@extjs/ext-react";
import * as React from "react";
import { MultiSelect } from "./MultiSelect";

Ext.require('Ext.panel.Collapser');

const W200 = '200px';

export class MultiSelectField extends MultiSelect {

    private panelProps: any;

    public render() {

        let { width, height, maxHeight, title, onselect } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';

        height = height || W200;

        maxHeight = maxHeight || W200;

        return <Panel {...this.panelProps} title={this.props.label}>
            <Container layout="hbox" padding="5px 0">
                <CheckBoxField
                    ref={x => this.cb = x}
                    padding="3px 6px 3px 2px"
                    bodyAlign="left"
                    onChange={this.onTopSelectionChange}
                />
                <SearchField placeholder="Search..." onChange={this.onSearch} />
            </Container>
            <div onClick={this.onListClick}>
                <Container>
                    <List
                        ref={x => this.lst = x}
                        itemTpl={this.props.itemTpl || this.nameFieldTpl}
                        store={this.state.items}
                        height={height}
                        maxHeight={maxHeight}
                        selectable="multi"
                        scrollable="true"
                        onSelect={onselect}
                    />
                </Container>
            </div>
        </Panel>;
    }

    public getValue(): string[] {
        return this.getSelectedKeysOrNull();
    }

    public filter(key: string, val: string, exactMatch: boolean = false) {

        let cfg: any = {
            property: key
        };

        if (val) {
            cfg.exactMatch = exactMatch;
            cfg.value = val;
        }
        else {
            cfg.value = '';
        }

        this.lst.getStore().filter(cfg);
    }

    protected init() {
        super.init();
        this.onSearch = this.onSearch.bind(this);
        this.panelProps = {
            width: this.props.width || W200,
            collapsible: {
                direction: 'top',
                dynamic: true,
                collapsed: true
            },
            userCls: 'multiselect-filter',
            margin: "0 0 2px 0"
        };
    }

    private onSearch(view: any, newValue: string, oldValue: string) {
        this.filter(this.nameField, newValue);
    }
}