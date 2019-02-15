import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { MultiSelect } from "../../Dict/Components/MultiSelect";
import { MultiSelectWg } from "../../Dict/Components/MultiSelectWg";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { HddCostFilterModel } from "../Model/HddCostFilterModel";

Ext.require('Ext.panel.Collapser');

const SELECT_MAX_HEIGHT: string = '200px';

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: HddCostFilterModel): void;
    onDownload(filter: HddCostFilterModel): void;
}

export class HddCostFilter extends React.Component<FilterPanelProps, any> {

    private wg: MultiSelect;

    private dictSrv: IDictService;

    private multiProps = {
        width: '200px',
        maxHeight: SELECT_MAX_HEIGHT,
        title: "",
        hideCheckbox: true
    };

    private panelProps = {
        width: '300px',
        collapsible: {
            direction: 'top',
            dynamic: true,
            collapsed: true
        },
        userCls: 'multiselect-filter',
        margin: "0 0 2px 0"
    };

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px" layout={{ type: 'vbox', align: 'left' }}>

                <Container margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true'
                    }}
                >

                    <Panel title='Asset(WG)'
                        {...this.panelProps}>
                        <MultiSelectWg ref={x => this.wg = x} {...this.multiProps} store={this.dictSrv.getWG} />
                    </Panel>

                </Container>

                <Button text="Search" ui="action" minWidth="85px" margin="20px auto" handler={this.onSearch} />

                <Button text="Download" ui="action" minWidth="85px" iconCls="x-fa fa-download" handler={this.onDownload} />

            </Panel>
        );
    }

    public getModel(): HddCostFilterModel {
        return {
            wg: this.wg.getSelectedKeysOrNull()
        };
    }

    private init() {
        this.dictSrv = DictFactory.getDictService();
        this.onSearch = this.onSearch.bind(this);
        this.onChange = this.onChange.bind(this);
        this.onDownload = this.onDownload.bind(this);
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private onChange() {
        let handler = this.props.onChange;
        if (handler) {
            handler(this.getModel());
        }
    }

    private onDownload() {
        let handler = this.props.onDownload;
        if (handler) {
            handler(this.getModel());
        }
    }
}