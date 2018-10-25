import { Button, ComboBoxField, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { IDictService } from "../../Dict/Services/IDictService";
import { SwCalcFilterModel } from "../Model/SwCalcFilterModel";
import { ReportFactory } from "../Services/ReportFactory";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: SwCalcFilterModel): void;
}

export class SwCalcFilter extends React.Component<FilterPanelProps, any> {

    private sog: ComboBoxField;

    private avail: ComboBoxField;

    private year: ComboBoxField;

    private srv: IDictService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

                <Container margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true'
                    }}
                >
                    <ComboBoxField ref="sog" label="Asset(SOG):" options={this.state.sogs} />
                    <ComboBoxField ref="availability" label="Availability:" options={this.state.availabilityTypes} />
                    <ComboBoxField ref="year" label="Year:" options={this.state.years} />
                </Container>

                <Button text="Search" ui="action" width="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public componentDidMount() {
        Promise.all([
            this.srv.getCountries(),
            this.srv.getSog(),
            this.srv.getAvailabilityTypes(),
            this.srv.getYears()
        ]).then(x => {
            this.setState({
                countries: x[0],
                sogs: x[1],
                availabilityTypes: x[2],
                years: x[3]
            });
        });
        //
        this.sog = this.refs.sog as ComboBoxField;
        this.avail = this.refs.availability as ComboBoxField;
        this.year = this.refs.year as ComboBoxField;
    }

    public getModel(): SwCalcFilterModel {
        return {
            sog: this.getSelected(this.sog),
            availability: this.getSelected(this.avail),
            year: this.getSelected(this.year)
        };
    }

    private init() {
        this.srv = ReportFactory.getDictService();
        //
        this.onSearch = this.onSearch.bind(this);
        //
        this.state = {
            countries: [],
            sogs: [],
            availabilityTypes: [],
            years: []
        };
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private getSelected(cb: ComboBoxField): string {
        let result: string = null;
        let selected = (cb as any).getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }
}