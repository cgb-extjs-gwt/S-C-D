import { Button, Container, Panel, PanelProps, TextField } from "@extjs/ext-react";
import * as React from "react";

export interface AutoFilterPanelProps extends PanelProps {
    filter(): React.Component[];
    onSearch(filter: any): void;
}

export class AutoFilter extends React.Component<AutoFilterPanelProps, any> {

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
                    }}>

                    {this.props.filter()}

                </Container>

                <Button text="Search" ui="action" width="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public getModel(): any {
        let result = {};

        let fields = this.props.filter();

        for (let i = 0, f: any; f = fields[i]; i++) {
            result[f.getLabel()] = f.getValue();
        }

        return result;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

}