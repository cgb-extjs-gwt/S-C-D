import { Button, Container, Panel, PanelProps, RadioField } from "@extjs/ext-react";
import * as React from "react";
import { NamedId } from "../../Common/States/CommonStates";
import { DictField } from "../../Dict/Components/DictField";
import { SogField } from "../../Dict/Components/SogField";
import { WgField } from "../../Dict/Components/WgField";
import { CurrencyType } from "../Model/CurrencyType";
import { HddCostFilterModel } from "../Model/HddCostFilterModel";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter:   HddCostFilterModel): void;
    onChange(filter:   HddCostFilterModel): void;
    onDownload(filter: HddCostFilterModel): void;
}

export class HddCostFilter extends React.Component<FilterPanelProps, any> {

    private sog: DictField<NamedId>;

    private wg: DictField<NamedId>;

    private localCur: RadioField & any;

    private euroCur: RadioField & any;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        let valid = this.state && this.state.valid;

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

                    <SogField ref={x => this.sog = x} label="SOG:" />
                    <WgField ref={x => this.wg = x} label="Asset(WG):" />

                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} margin="5px 0 0 0" defaults={{ padding: '3px 0' }} >
                    <RadioField ref={x => this.localCur = x} name="currency" boxLabel="Show in local currency" checked onCheck={this.onChange} />
                    <RadioField ref={x => this.euroCur = x} name="currency" boxLabel="Show in EUR" onCheck={this.onChange} />
                </Container>

                <Button text="Search" ui="action" minWidth="85px" margin="20px auto" disabled={!valid} handler={this.onSearch} />

                <Button text="Download" ui="action" minWidth="85px" iconCls="x-fa fa-download" disabled={!valid} handler={this.onDownload} />

            </Panel>
        );
    }

    public getModel(): HddCostFilterModel {
        return {
            wg: this.wg.getSelected(),
            sog: this.sog.getSelected(),
            currency: this.getCurrency()
        };
    }

    private init() {
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

    private getCurrency(): CurrencyType {
        return this.euroCur.getChecked() ? CurrencyType.Euro : CurrencyType.Local
    }
}